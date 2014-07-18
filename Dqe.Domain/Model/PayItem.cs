using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class PayItem : Entity<Transformers.PayItem>
    {

        private readonly ICollection<PayItemLrePickList> _payItemLrePickLists; 

        public PayItem()
        {
            _payItemLrePickLists = new Collection<PayItemLrePickList>();
        }

        //e.g. 102-71-1
        //format = 9999-999-999
        //unique within spec year - structure
        //should numeric be enforced?
        [Required]
        [StringLength(12)]
        public virtual string PayItemId { get; protected internal set; }

        //must be equal or before obsolete date
        //must be equal or greater than structure effective date
        //is it required - we think yes?
        public virtual DateTime OpenedDate { get; protected internal set; }

        //must be equal or greater effective date
        //must be equal or less than structure obsolete date
        public virtual DateTime ObsoleteDate { get; protected internal set; }

        //length should match PES
        [Required]
        [StringLength(255)]
        public virtual string ShortDescription { get; protected internal set; }

        //length should match PES
        [Required]
        [StringLength(255)]
        public virtual string Description { get; protected internal set; }

        [Required]
        public virtual PayItemStructure MyPayItemStructure { get; protected internal set; }

        [Required]
        public virtual SpecYear MySpecYear { get; protected internal set; }

        //required if primary unit is LS - Lump Sum
        //public virtual SecondaryUnit MySecondaryUnit { get; protected internal set; }

        //public virtual PrimaryUnit MyPrimaryUnit { get; protected internal set; }


        //might be a BJS job that updates nightly
        //do we need to keep the LRE reference price as well
        public virtual decimal ReferencePrice { get; protected internal set; }

        public virtual decimal ConcreteFactor { get; protected internal set; }

        public virtual decimal AsphaltFactor { get; protected internal set; }

        public virtual decimal FuelFactor { get; protected internal set; }

        [StringLength(5000)]
        public virtual string FactorNotes { get; protected internal set; }

        public virtual bool IsLikeItemsCombined { get; protected internal set; }

        //what is this???
        public virtual bool IsSupplementalDescriptionRequired { get; protected internal set; }

        public virtual bool IsFederalFunded { get; protected internal set; }

        //populate from Trns*Port
        public virtual int ContractTypeCode { get; protected internal set; }

        //populate from Trns*Port
        public virtual int ItemClassCode { get; protected internal set; }

        public virtual SpecTypeWebLink MySpecType { get; protected internal set; }

        //collection must contain at least one pick list
        public IEnumerable<PayItemLrePickList> PayItemLrePickLists
        {
            get { return _payItemLrePickLists.ToList().AsReadOnly(); }
        } 

        public override Transformers.PayItem GetTransformer()
        {
            return new Transformers.PayItem
            {
                AsphaltFactor = AsphaltFactor,
                ConcreteFactor = ConcreteFactor,
                ContractTypeCode = ContractTypeCode,
                Description = Description,
                FactorNotes = FactorNotes,
                FuelFactor = FuelFactor,
                Id = Id,
                IsFederalFunded = IsFederalFunded,
                IsLikeItemsCombined = IsLikeItemsCombined,
                IsSupplementalDescriptionRequired = IsSupplementalDescriptionRequired,
                ItemClassCode = ItemClassCode,
                ObsoleteDate = ObsoleteDate,
                OpenedDate = OpenedDate,
                PayItemId = PayItemId,
                ReferencePrice = ReferencePrice,
                ShortDescription = ShortDescription
            };
        }

        public override void Transform(Transformers.PayItem transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            AsphaltFactor = transformer.AsphaltFactor;
            ConcreteFactor = transformer.ConcreteFactor;
            ContractTypeCode = transformer.ContractTypeCode;
            Description = transformer.Description;
            FactorNotes = transformer.FactorNotes;
            FuelFactor = transformer.FuelFactor;
            IsFederalFunded = transformer.IsFederalFunded;
            IsLikeItemsCombined = transformer.IsLikeItemsCombined;
            IsSupplementalDescriptionRequired = transformer.IsSupplementalDescriptionRequired;
            ItemClassCode = transformer.ItemClassCode;
            ObsoleteDate = transformer.ObsoleteDate;
            OpenedDate = transformer.OpenedDate;
            PayItemId = transformer.PayItemId;
            ReferencePrice = transformer.ReferencePrice;
            ShortDescription = transformer.ShortDescription;
        }
    }
}