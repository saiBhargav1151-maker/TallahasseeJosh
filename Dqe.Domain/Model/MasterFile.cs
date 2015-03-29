using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.Domain.Model
{
    public class MasterFile : Entity<Transformers.MasterFile>
    {
        private readonly ICollection<PayItemMaster> _payItemMasters;
        private readonly ICollection<Project> _projects; 
        private readonly IMasterFileRepository _masterFileRepository;

        public MasterFile(IMasterFileRepository masterFileRepository)
        {
            _payItemMasters = new Collection<PayItemMaster>();
            _projects = new Collection<Project>();
            _masterFileRepository = masterFileRepository;
        }

        //[Range(1, int.MaxValue)]
        public virtual int FileNumber { get; protected internal set; }

        public virtual IEnumerable<PayItemMaster> PayItemMasters
        {
            get { return _payItemMasters.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<Project> Projects
        {
            get { return _projects.ToList().AsReadOnly(); }
        }

        public virtual void AddPayItemMaster(PayItemMaster payItemMaster)
        {
            _payItemMasters.Add(payItemMaster);
            payItemMaster.MyMasterFile = this;
        }

        public virtual void AddProject(Project project, DqeUser account)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            project.MyMasterFile = this;
        }

        public override Transformers.MasterFile GetTransformer()
        {
            return new Transformers.MasterFile
            {
                Id = Id,
                FileNumber = FileNumber
            };
        }

        public override void Transform(Transformers.MasterFile transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            FileNumber = transformer.FileNumber;
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var existingMasterFile = _masterFileRepository.GetByFileNumber(FileNumber);
            if (existingMasterFile != null && existingMasterFile.Id != Id)
            {
                yield return new ValidationResult("A Master File with this number already exists");
            }
        }
    }
}