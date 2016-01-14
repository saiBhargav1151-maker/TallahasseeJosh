using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
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

        public void CopyMasterFile(string taskUser, int masterFileId, DateTime effectiveDate, int currentUserSrsId, int fileNumber)
        {
            //try
            //{
            //    _deferredTaskHubContext.SendMessageToUser(taskUser, ApplicationConstants.Tasks.CopyMasterFile, "Copying Master File started.");
            //    var validationResults = new List<ValidationResult>();
            //    //IMPORTANT - Glimpse will cause this operation to fail!
            //    using (var session = Initializer.SessionFactory.OpenSession())
            //    {
            //        var dqeUserRepository = new DqeUserRepository(session);
            //        var masterFileRepository = new MasterFileRepository(session);
            //        var payItemRepository = new PayItemRepository(session);
            //        var systemTaskRepository = new SystemTaskRepository(session);
            //        using (var transaction = session.BeginTransaction())
            //        {
            //            var currentDqeUser = dqeUserRepository.GetBySrsId(currentUserSrsId);
            //            var mf = new MasterFile(masterFileRepository);
            //            var mft = mf.GetTransformer();
            //            mft.FileNumber = fileNumber;
            //            mf.Transform(mft, currentDqeUser);
            //            if (!Validator.TryValidateObject(mf, new ValidationContext(mf, null, null), validationResults)) AbortTask(taskUser, validationResults, ApplicationConstants.Tasks.CopyMasterFile);
            //            if (!validationResults.Any())
            //            {
            //                session.SaveOrUpdate(mf);
            //                var copyMf = masterFileRepository.Get(masterFileId);
            //                foreach (var piCopy in copyMf.PayItems)
            //                {
            //                    piCopy.OverrideRepository(payItemRepository);
            //                    var t = piCopy.GetTransformer();
            //                    var copyToNewMasterFile = false;
            //                    DateTime? newObsoleteDate = null;
            //                    //TODO: how do I copy forward a pay item with a future effective date?
            //                    //BUG - this will skip the 2020 pay item in WT - && piCopy.EffectiveDate.Value.Date <= effectiveDate)
            //                    //if (piCopy.EffectiveDate.HasValue && piCopy.EffectiveDate.Value.Date <= effectiveDate)
            //                    if (piCopy.EffectiveDate.HasValue)
            //                    {
            //                        if (piCopy.ObsoleteDate.HasValue)
            //                        {
            //                            //future obsolete
            //                            if (piCopy.ObsoleteDate.Value.Date >= effectiveDate)
            //                            {
            //                                copyToNewMasterFile = true;
            //                                newObsoleteDate = piCopy.ObsoleteDate;
            //                                t.ObsoleteDate = effectiveDate.AddDays(-1).Date;
            //                                piCopy.Transform(t, currentDqeUser);
            //                                if (!Validator.TryValidateObject(piCopy, new ValidationContext(piCopy, null, null), validationResults))
            //                                {
            //                                    AbortTask(taskUser, validationResults, ApplicationConstants.Tasks.CopyMasterFile);
            //                                    break;
            //                                }
            //                            }
            //                        }
            //                        else
            //                        {
            //                            copyToNewMasterFile = true;
            //                            t.ObsoleteDate = effectiveDate.AddDays(-1).Date;
            //                            piCopy.Transform(t, currentDqeUser);
            //                            if (!Validator.TryValidateObject(piCopy, new ValidationContext(piCopy, null, null), validationResults))
            //                            {
            //                                AbortTask(taskUser, validationResults, ApplicationConstants.Tasks.CopyMasterFile);
            //                                break;
            //                            }
            //                        }
            //                    }
            //                    if (validationResults.Any()) continue;
            //                    if (!copyToNewMasterFile) continue;
            //                    var piNew = new PayItem(payItemRepository);
            //                    t.ObsoleteDate = newObsoleteDate;
            //                    t.EffectiveDate = effectiveDate;
            //                    piNew.AssociatePayItemToStructureAndMasterFile(piCopy.MyPayItemStructure, mf);
            //                    piNew.Transform(t, currentDqeUser);
            //                    session.SaveOrUpdate(piNew);
            //                    if (!Validator.TryValidateObject(piNew, new ValidationContext(piNew, null, null), validationResults)) AbortTask(taskUser, validationResults, ApplicationConstants.Tasks.CopyMasterFile);
            //                }
            //            }
            //            if (!validationResults.Any())
            //            {
            //                var val = systemTaskRepository.GetByTaskId(ApplicationConstants.Tasks.CopyMasterFile);
            //                if (val != null)
            //                {
            //                    session.Delete(val);
            //                }
            //                transaction.Commit();
            //            }
            //        }
            //    }
            //    if (!validationResults.Any()) _deferredTaskHubContext.SendMessageToUser(taskUser, ApplicationConstants.Tasks.CopyMasterFile, "Copying Master File finished.");
            //}
            //catch
            //{
            //    AbortTask(taskUser, new List<ValidationResult> { new ValidationResult("Application exception occurred.") }, ApplicationConstants.Tasks.CopyMasterFile);
            //    throw;
            //}
        }

        private void AbortTask(string taskUser, IEnumerable<ValidationResult> validationResults, string task)
        {
            using (var session = Initializer.SessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var systemTaskRepository = new SystemTaskRepository(session);
                    var val = systemTaskRepository.GetByTaskId(task);
                    if (val != null)
                    {
                        session.Delete(val);
                    }
                    transaction.Commit();
                }
            }
            var sb = new StringBuilder();
            sb.AppendFormat("Task failed...{0}", Environment.NewLine);
            foreach (var validationResult in validationResults)
            {
                sb.AppendFormat("- {0}{1}", validationResult.ErrorMessage, Environment.NewLine);    
            }
            _deferredTaskHubContext.SendMessageToUser(taskUser, task, sb.ToString());  
        }
    }
}