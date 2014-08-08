using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.Domain.Model
{
    public class MasterFile : Entity<Transformers.MasterFile>
    {
        private readonly ICollection<PayItem> _payItems;
        private readonly IMasterFileRepository _masterFileRepository;

        public MasterFile(IMasterFileRepository masterFileRepository)
        {
            _payItems = new Collection<PayItem>();
            _masterFileRepository = masterFileRepository;
        }

        [Range(1, int.MaxValue)]
        public virtual int FileNumber { get; protected internal set; }

        public virtual IEnumerable<PayItem> PayItems
        {
            get { return _payItems.ToList().AsReadOnly(); }
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