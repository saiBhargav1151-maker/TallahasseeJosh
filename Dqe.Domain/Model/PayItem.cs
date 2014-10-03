using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.Domain.Model
{
    public class PayItem : Entity<Transformers.PayItem>
    {
        private readonly ICollection<PayItemLrePickList> _payItemLrePickLists;
        private IPayItemRepository _payItemRepository;
         
        public PayItem(IPayItemRepository payItemRepository)
        {
            _payItemLrePickLists = new Collection<PayItemLrePickList>();
            _payItemRepository = payItemRepository;
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
        public virtual DateTime? EffectiveDate { get; protected internal set; }

        //must be equal or greater effective date
        //must be equal or less than structure obsolete date
        public virtual DateTime? ObsoleteDate { get; protected internal set; }

        //length should match PES
        [StringLength(255)]
        public virtual string ShortDescription { get; protected internal set; }

        //length should match PES
        [StringLength(255)]
        public virtual string Description { get; protected internal set; }

        [Required]
        public virtual PayItemStructure MyPayItemStructure { get; protected internal set; }

        [Required]
        public virtual MasterFile MyMasterFile { get; protected internal set; }

        public virtual CostBasedTemplate MyCostBasedTemplate { get; protected internal set; }

        public virtual int SrsId { get; protected internal set; }

        //required if primary unit is LS - Lump Sum
        public virtual SecondaryUnitType? SecondaryUnit { get; protected internal set; }

        [Required]
        public virtual PrimaryUnitType PrimaryUnit { get; protected internal set; }

        //might be a BJS job that updates nightly
        //do we need to keep the LRE reference price as well
        public virtual decimal DqeReferencePrice { get; protected internal set; }

        public virtual decimal LreReferencePrice { get; protected internal set; }

        public virtual decimal ConcreteFactor { get; protected internal set; }

        public virtual decimal AsphaltFactor { get; protected internal set; }

        [StringLength(5000)]
        public virtual string FactorNotes { get; protected internal set; }

        public virtual bool IsLikeItemsCombined { get; protected internal set; }

        //what is this???
        public virtual bool IsSupplementalDescriptionRequired { get; protected internal set; }

        public virtual bool IsFederalFunded { get; protected internal set; }

        //populate from Trns*Port
        //public virtual int ContractTypeCode { get; protected internal set; }

        //populate from Trns*Port
        //public virtual int ItemClassCode { get; protected internal set; }

        //collection must contain at least one pick list
        public virtual IEnumerable<PayItemLrePickList> PayItemLrePickLists
        {
            get { return _payItemLrePickLists.ToList().AsReadOnly(); }
        }

        public virtual void AssociatePayItemToStructureAndMasterFile(PayItemStructure payItemStructure, MasterFile masterFile)
        {
            MyMasterFile = masterFile;
            MyPayItemStructure = payItemStructure;
        }

        public virtual void AddCostBasedTemplate(CostBasedTemplate costBasedTemplate, DqeUser account)
        {
            if (costBasedTemplate == null) throw new ArgumentNullException("costBasedTemplate");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            MyCostBasedTemplate = costBasedTemplate;
        }

        public virtual void RemoveCostBasedTemplate(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            MyCostBasedTemplate = null;
        }

        public override Transformers.PayItem GetTransformer()
        {
            return new Transformers.PayItem
            {
                AsphaltFactor = AsphaltFactor,
                ConcreteFactor = ConcreteFactor,
                //ContractTypeCode = ContractTypeCode,
                Description = Description,
                FactorNotes = FactorNotes,
                Id = Id,
                IsFederalFunded = IsFederalFunded,
                IsLikeItemsCombined = IsLikeItemsCombined,
                IsSupplementalDescriptionRequired = IsSupplementalDescriptionRequired,
                //ItemClassCode = ItemClassCode,
                ObsoleteDate = ObsoleteDate,
                EffectiveDate = EffectiveDate,
                PayItemId = PayItemId,
                DqeReferencePrice = DqeReferencePrice,
                LreReferencePrice = LreReferencePrice,
                ShortDescription = ShortDescription,
                PrimaryUnit = PrimaryUnit,
                SecondaryUnit = SecondaryUnit,
                SrsId = SrsId
            };
        }

        public override void Transform(Transformers.PayItem transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            AsphaltFactor = transformer.AsphaltFactor;
            ConcreteFactor = transformer.ConcreteFactor;
            //ContractTypeCode = transformer.ContractTypeCode;
            Description = transformer.Description;
            FactorNotes = transformer.FactorNotes;
            IsFederalFunded = transformer.IsFederalFunded;
            IsLikeItemsCombined = transformer.IsLikeItemsCombined;
            IsSupplementalDescriptionRequired = transformer.IsSupplementalDescriptionRequired;
            //ItemClassCode = transformer.ItemClassCode;
            ObsoleteDate = transformer.ObsoleteDate.HasValue ? (DateTime?)transformer.ObsoleteDate.Value.Date : null;
            EffectiveDate = transformer.EffectiveDate.HasValue ? (DateTime?)transformer.EffectiveDate.Value.Date : null;
            PayItemId = transformer.PayItemId;
            DqeReferencePrice = transformer.DqeReferencePrice;
            LreReferencePrice = transformer.LreReferencePrice;
            ShortDescription = transformer.ShortDescription;
            SrsId = transformer.SrsId;
            if (MyPayItemStructure.PrimaryUnit == PrimaryUnitType.Mixed)
            {
                PrimaryUnit = transformer.PrimaryUnit;
                SecondaryUnit = transformer.SecondaryUnit;
            }
            else
            {
                PrimaryUnit = MyPayItemStructure.PrimaryUnit;
                SecondaryUnit = MyPayItemStructure.SecondaryUnit;
            }
        }

        public virtual void OverrideRepository(IPayItemRepository payItemRepository)
        {
            _payItemRepository = payItemRepository;
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //TODO: effective date <= obsolete date is not yet validated - WT has future effective dates (2020) - how to handle?
            if (PrimaryUnit == PrimaryUnitType.None)
            {
                yield return new ValidationResult(string.Format("Pay Item {0}: Primary Unit is required.", PayItemId));
            }
            if (PrimaryUnit == PrimaryUnitType.LS && (!SecondaryUnit.HasValue || SecondaryUnit.Value == SecondaryUnitType.None))
            {
                yield return new ValidationResult(string.Format("Pay Item {0}: Secondary Unit is required if Primary Unit is Lump Sum.", PayItemId));
            }
            var existingPayItem = _payItemRepository.GetByNumberAndMasterFile(PayItemId, MyMasterFile.FileNumber);
            if (existingPayItem != null && existingPayItem.Id != Id)
            {
                yield return new ValidationResult(string.Format("Pay Item {0}: A Pay Item with this Number and Master File already exists", PayItemId));
            }
            if (ObsoleteDate.HasValue && !EffectiveDate.HasValue)
            {
                yield return new ValidationResult(string.Format("Pay Item {0}: A Pay Item must have an Effective Date to have an Obsolete Date", PayItemId));
            }
            if (MyPayItemStructure.ObsoleteDate.HasValue && !ObsoleteDate.HasValue)
            {
                yield return new ValidationResult(string.Format("Pay Item {0}: A Pay Item must have an Obsolete Date if the parent Structure has an Obsolete Date", PayItemId));
            }
            var likePayItems = _payItemRepository.GetByNumber(PayItemId);
            var isOverlap = false;
            foreach (var likePayItem in likePayItems)
            {
                if (likePayItem.Id == Id) continue;
                if (likePayItem.EffectiveDate.HasValue && EffectiveDate.HasValue && likePayItem.EffectiveDate.Value.Date == EffectiveDate.Value.Date)
                {
                    isOverlap = true;
                    break;
                }
                if (likePayItem.ObsoleteDate.HasValue && ObsoleteDate.HasValue && likePayItem.ObsoleteDate.Value.Date == ObsoleteDate.Value.Date)
                {
                    isOverlap = true;
                    break;
                }
                if (likePayItem.EffectiveDate.HasValue && likePayItem.ObsoleteDate.HasValue)
                {
                    if (EffectiveDate.HasValue && ObsoleteDate.HasValue)
                    {
                        // lpi.ed -----> pi.ed---------------------pi.od <----------lpi.od 
                        if (EffectiveDate.Value.Date > likePayItem.EffectiveDate.Value.Date && ObsoleteDate.Value.Date < likePayItem.ObsoleteDate.Value.Date)
                        {
                            isOverlap = true;
                            break;
                        }
                        // pi.ed -----> lpi.ed---------------------lpi.od <----------pi.od 
                        if (EffectiveDate.Value.Date < likePayItem.EffectiveDate.Value.Date && ObsoleteDate.Value.Date > likePayItem.ObsoleteDate.Value.Date)
                        {
                            isOverlap = true;
                            break;
                        }
                        // pi.ed -----> lpi.ed <-------------------pi.od 
                        if (EffectiveDate.Value.Date < likePayItem.EffectiveDate.Value.Date && ObsoleteDate.Value.Date > likePayItem.EffectiveDate.Value.Date)
                        {
                            isOverlap = true;
                            break;
                        }
                        // lpi.ed -----> pi.ed <-------------------lpi.od 
                        if (likePayItem.EffectiveDate.Value.Date < EffectiveDate.Value.Date && EffectiveDate.Value.Date < likePayItem.ObsoleteDate.Value.Date)
                        {
                            isOverlap = true;
                            break;
                        }
                    }
                    else if (EffectiveDate.HasValue)
                    {
                        // lpi.ed----------------> pi.ed <---------lpi.od
                        if (EffectiveDate.Value.Date < likePayItem.ObsoleteDate.Value.Date)
                        {
                            isOverlap = true;
                            break;
                        }
                    }
                }
                else if (likePayItem.EffectiveDate.HasValue)
                {
                    if (EffectiveDate.HasValue && ObsoleteDate.HasValue)
                    {
                        // pi.ed---------> lpi.ed <---------pi.od
                        if (EffectiveDate.Value.Date < likePayItem.EffectiveDate.Value.Date &&
                            ObsoleteDate.Value.Date > likePayItem.EffectiveDate.Value.Date)
                        {
                            isOverlap = true;
                            break;
                        }
                    }
                    else
                    {
                        // pi.ed------------> lpi.ed--------------->
                        isOverlap = true;
                        break;    
                    }
                }
            }
            if (isOverlap)
            {
                yield return new ValidationResult(string.Format("Pay Item {0}: The effective range for this Pay Item overlaps with another Pay Item with the same Number", PayItemId));
            }
            if (EffectiveDate.HasValue && EffectiveDate.Value.Date < MyPayItemStructure.EffectiveDate.Date)
            {
                yield return new ValidationResult(string.Format("Pay Item {0}: Effective Date must be equal to or greater than the Pay Item Structure's Effective Date", PayItemId));
            }
            if (ObsoleteDate.HasValue && MyPayItemStructure.ObsoleteDate.HasValue && ObsoleteDate.Value > MyPayItemStructure.ObsoleteDate.Value)
            {
                yield return new ValidationResult(string.Format("Pay Item {0}: Obsolete Date must be equal to or less than the Pay Item Structure's Obsolete Date", PayItemId));
            }
        }
    }
}