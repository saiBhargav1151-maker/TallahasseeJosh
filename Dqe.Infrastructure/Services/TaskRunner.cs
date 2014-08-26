using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Infrastructure.Repositories.Custom;

namespace Dqe.Infrastructure.Services
{
    public class TaskRunner : ITaskRunner
    {
        private readonly IDeferredTaskHubContext _deferredTaskHubContext;

        public TaskRunner(IDeferredTaskHubContext deferredTaskHubContext)
        {
            _deferredTaskHubContext = deferredTaskHubContext;
        }

        public void CopyMasterFile(int masterFileId, DateTime effectiveDate, int currentUserSrsId, int fileNumber)
        {
            try
            {
                _deferredTaskHubContext.SendMessage("Copying Master File started.");
                var validationResults = new List<ValidationResult>();
                //IMPORTANT - Glimpse will cause this operation to fail!
                using (var session = Initializer.SessionFactory.OpenSession())
                {
                    var dqeUserRepository = new DqeUserRepository(session);
                    var masterFileRepository = new MasterFileRepository(session);
                    var payItemRepository = new PayItemRepository(session);
                    using (var transaction = session.BeginTransaction())
                    {
                        var currentDqeUser = dqeUserRepository.GetBySrsId(currentUserSrsId);
                        var mf = new MasterFile(masterFileRepository);
                        var mft = mf.GetTransformer();
                        mft.FileNumber = fileNumber;
                        mf.Transform(mft, currentDqeUser);
                        if (!Validator.TryValidateObject(mf, new ValidationContext(mf, null, null), validationResults)) AbortTask(validationResults);
                        if (!validationResults.Any())
                        {
                            session.SaveOrUpdate(mf);
                            var copyMf = masterFileRepository.Get(masterFileId);
                            foreach (var piCopy in copyMf.PayItems)
                            {
                                piCopy.OverrideRepository(payItemRepository);
                                var t = piCopy.GetTransformer();
                                var copyToNewMasterFile = false;
                                DateTime? newObsoleteDate = null;
                                if (piCopy.EffectiveDate.HasValue)
                                {
                                    if (piCopy.ObsoleteDate.HasValue)
                                    {
                                        //future obsolete
                                        if (piCopy.ObsoleteDate.Value.Date >= effectiveDate)
                                        {
                                            copyToNewMasterFile = true;
                                            newObsoleteDate = piCopy.ObsoleteDate;
                                            t.ObsoleteDate = effectiveDate.AddDays(-1).Date;
                                            piCopy.Transform(t, currentDqeUser);
                                            if (!Validator.TryValidateObject(piCopy, new ValidationContext(piCopy, null, null), validationResults))
                                            {
                                                AbortTask(validationResults);
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        copyToNewMasterFile = true;
                                        t.ObsoleteDate = effectiveDate.AddDays(-1).Date;
                                        piCopy.Transform(t, currentDqeUser);
                                        if (!Validator.TryValidateObject(piCopy, new ValidationContext(piCopy, null, null), validationResults))
                                        {
                                            AbortTask(validationResults);
                                            break;
                                        }
                                    }
                                }
                                if (validationResults.Any()) continue;
                                if (!copyToNewMasterFile) continue;
                                var piNew = new PayItem(payItemRepository);
                                t.ObsoleteDate = newObsoleteDate;
                                t.EffectiveDate = effectiveDate;
                                piNew.AssociatePayItemToStructureAndMasterFile(piCopy.MyPayItemStructure, mf);
                                piNew.Transform(t, currentDqeUser);
                                session.SaveOrUpdate(piNew);
                                if (!Validator.TryValidateObject(piNew, new ValidationContext(piNew, null, null), validationResults)) AbortTask(validationResults);
                            }
                        }
                        if (!validationResults.Any()) transaction.Commit();
                    }
                }
                if (!validationResults.Any()) _deferredTaskHubContext.SendMessage("Copying Master File finished.");
            }
            catch
            {
                AbortTask(new List<ValidationResult> { new ValidationResult("Copying Master File failed.") });
                throw;
            }
        }

        private void AbortTask(IEnumerable<ValidationResult> validationResults)
        {
            foreach (var validationResult in validationResults)
            {
                _deferredTaskHubContext.SendMessage(validationResult.ErrorMessage);    
            }
            _deferredTaskHubContext.SendMessage("Task failed.");  
        }
    }
}