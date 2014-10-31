using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.Domain.Model
{
    public class PayItemStructure : Entity<Transformers.PayItemStructure>
    {
        private readonly ICollection<PayItem> _payItems;
        private readonly ICollection<StandardWebLink> _standards;
        private readonly ICollection<PpmChapterWebLink> _ppmChapters;
        private readonly ICollection<SpecificationWebLink> _specifications;
        private readonly ICollection<PrepAndDocChapterWebLink> _prepAndDocChapters;
        private readonly ICollection<OtherReferenceWebLink> _otherReferences;

        private readonly IPayItemStructureRepository _payItemStructureRepository;

        public PayItemStructure(IPayItemStructureRepository payItemStructureRepository)
        {
            _payItems = new Collection<PayItem>();
            _standards = new Collection<StandardWebLink>();
            _ppmChapters = new Collection<PpmChapterWebLink>();
            _specifications = new Collection<SpecificationWebLink>();
            _prepAndDocChapters = new Collection<PrepAndDocChapterWebLink>();
            _otherReferences = new Collection<OtherReferenceWebLink>();
            _payItemStructureRepository = payItemStructureRepository;
        }

        public virtual CostBasedTemplate MyCostBasedTemplate { get; protected internal set; }

        //e.g. 102-71-AB
        //unique within spec year
        //format = 9999-999-999
        [Required]
        [StringLength(12)]
        [Display(Name = "Structure Number")]
        public virtual string StructureId { get; protected internal set; }

        //length should match PES
        [Required]
        [StringLength(255)]
        public virtual string Title { get; protected internal set; }

        //must be equal or before obsolete date
        //is it required - we think yes?
        [Display(Name = "Effective Date")]
        public virtual DateTime EffectiveDate { get; protected internal set; }

        //must be equal or greater effective date
        [Display(Name = "Obsolete Date")]
        public virtual DateTime? ObsoleteDate { get; protected internal set; }

        public virtual bool IsPlanQuantity { get; protected internal set; }

        public virtual bool IsDoNotBid { get; protected internal set; }

        public virtual bool IsFixedPrice { get; protected internal set; }

        public virtual int SrsId { get; protected internal set; }

        //required if IsFixedPrice == true, otherwise NULL
        public virtual decimal? FixedAmount { get; protected internal set; }

        [Range(1, 10)]
        [Display(Name = "Decimal Precision Number")]
        public virtual int Accuracy { get; protected internal set; }

        [StringLength(255)]
        public virtual string OtherReferenceNotes { get; protected internal set; }

        //[StringLength(255)]
        //public virtual string RequiredPayItems { get; protected internal set; }

        //[StringLength(255)]
        //public virtual string RecommendedPayItems { get; protected internal set; }

        [StringLength(200)]
        public virtual string Notes { get; protected internal set; }

        [StringLength(5000)]
        public virtual string Details { get; protected internal set; }

        [StringLength(5000)]
        public virtual string PendingInformation { get; protected internal set; }

        [StringLength(5000)]
        public virtual string EssHistory { get; protected internal set; }

        //required if a BOE chnage description is added
        public virtual DateTime? BoeRecentChangeDate { get; protected internal set; }

        [StringLength(200)]
        public virtual string BoeRecentChangeDescription { get; protected internal set; }

        [StringLength(5000)]
        public virtual string StructureDescription { get; protected internal set; }

        [StringLength(5000)]
        public virtual string PlanSummary { get; protected internal set; }

        public virtual bool IsMonitored { get; protected internal set; }

        //required if IsMonitored 
        public virtual int? MonitorSrsId { get; protected internal set; }

        public virtual PrimaryUnitType PrimaryUnit { get; protected internal set; }

        //required if primary unit is LS - Lump Sum
        public virtual SecondaryUnitType? SecondaryUnit { get; protected internal set; }

        public virtual IEnumerable<PayItem> PayItems
        {
            get { return _payItems.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<StandardWebLink> Standards
        {
            get { return _standards.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<PpmChapterWebLink> PpmChapters
        {
            get { return _ppmChapters.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<SpecificationWebLink> Specifications
        {
            get { return _specifications.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<PrepAndDocChapterWebLink> PrepAndDocChapters
        {
            get { return _prepAndDocChapters.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<OtherReferenceWebLink> OtherReferences
        {
            get { return _otherReferences.ToList().AsReadOnly(); }
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

        public virtual void AddOtherReference(OtherReferenceWebLink otherReference, DqeUser account)
        {
            if (otherReference == null) throw new ArgumentNullException("otherReference");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (!_otherReferences.Contains(otherReference)) _otherReferences.Add(otherReference);
        }

        public virtual void RemoveOtherReference(OtherReferenceWebLink otherReference, DqeUser account)
        {
            if (otherReference == null) throw new ArgumentNullException("otherReference");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (_otherReferences.Contains(otherReference)) _otherReferences.Remove(otherReference);
        }

        public virtual void ClearOtherReferences(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            _otherReferences.Clear();
        }

        public virtual void AddStandard(StandardWebLink standard, DqeUser account)
        {
            if (standard == null) throw new ArgumentNullException("standard");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (!_standards.Contains(standard)) _standards.Add(standard);
        }

        public virtual void RemoveStandard(StandardWebLink standard, DqeUser account)
        {
            if (standard == null) throw new ArgumentNullException("standard");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (_standards.Contains(standard)) _standards.Remove(standard);
        }

        public virtual void ClearStandards(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            _standards.Clear();
        }

        public virtual void AddPpmChapter(PpmChapterWebLink ppmChapter, DqeUser account)
        {
            if (ppmChapter == null) throw new ArgumentNullException("ppmChapter");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (!_ppmChapters.Contains(ppmChapter)) _ppmChapters.Add(ppmChapter);
        }

        public virtual void RemovePpmChapter(PpmChapterWebLink ppmChapter, DqeUser account)
        {
            if (ppmChapter == null) throw new ArgumentNullException("ppmChapter");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (_ppmChapters.Contains(ppmChapter)) _ppmChapters.Remove(ppmChapter);
        }

        public virtual void ClearPpmChapters(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            _ppmChapters.Clear();
        }

        public virtual void AddSpecification(SpecificationWebLink specification, DqeUser account)
        {
            if (specification == null) throw new ArgumentNullException("specification");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (!_specifications.Contains(specification)) _specifications.Add(specification);
        }

        public virtual void RemoveSpecification(SpecificationWebLink specification, DqeUser account)
        {
            if (specification == null) throw new ArgumentNullException("specification");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (_specifications.Contains(specification)) _specifications.Remove(specification);
        }

        public virtual void ClearSpecifications(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            _specifications.Clear();
        }

        public virtual void AddPrepAndDocChapter(PrepAndDocChapterWebLink prepAndDocChapter, DqeUser account)
        {
            if (prepAndDocChapter == null) throw new ArgumentNullException("prepAndDocChapter");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (!_prepAndDocChapters.Contains(prepAndDocChapter)) _prepAndDocChapters.Add(prepAndDocChapter);
        }

        public virtual void RemovePrepAndDocChapter(PrepAndDocChapterWebLink prepAndDocChapter, DqeUser account)
        {
            if (prepAndDocChapter == null) throw new ArgumentNullException("prepAndDocChapter");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (_prepAndDocChapters.Contains(prepAndDocChapter)) _prepAndDocChapters.Remove(prepAndDocChapter);
        }

        public virtual void ClearPrepAndDocChapters(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            _prepAndDocChapters.Clear();
        }

        public override Transformers.PayItemStructure GetTransformer()
        {
            return new Transformers.PayItemStructure
            {
                Accuracy = Accuracy,
                BoeRecentChangeDate = BoeRecentChangeDate,
                BoeRecentChangeDescription = BoeRecentChangeDescription,
                Details = Details,
                EffectiveDate = EffectiveDate,
                EssHistory = EssHistory,
                FixedAmount = FixedAmount,
                Id = Id,
                IsDoNotBid = IsDoNotBid,
                IsFixedPrice = IsFixedPrice,
                IsMonitored = IsMonitored,
                IsPlanQuantity = IsPlanQuantity,
                MonitorSrsId = MonitorSrsId,
                Notes = Notes,
                ObsoleteDate = ObsoleteDate,
                OtherReferenceNotes = OtherReferenceNotes,
                PlanSummary = PlanSummary,
                //RecommendedPayItems = RecommendedPayItems,
                //RequiredPayItems = RequiredPayItems,
                StructureDescription = StructureDescription,
                StructureId = StructureId,
                Title = Title,
                SecondaryUnit = SecondaryUnit,
                PrimaryUnit = PrimaryUnit,
                SrsId = SrsId,
                PendingInformation = PendingInformation
            };
        }

        public override void Transform(Transformers.PayItemStructure transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            Accuracy = transformer.Accuracy;
            BoeRecentChangeDate = transformer.BoeRecentChangeDate;
            BoeRecentChangeDescription = transformer.BoeRecentChangeDescription;
            Details = transformer.Details;
            PendingInformation = transformer.PendingInformation;
            EffectiveDate = transformer.EffectiveDate == DateTime.MinValue ? DateTime.MaxValue : transformer.EffectiveDate.Date;
            EssHistory = transformer.EssHistory;
            IsFixedPrice = transformer.IsFixedPrice;
            FixedAmount = IsFixedPrice ? transformer.FixedAmount : null;
            IsDoNotBid = transformer.IsDoNotBid;
            IsMonitored = transformer.IsMonitored;
            IsPlanQuantity = transformer.IsPlanQuantity;
            MonitorSrsId = transformer.MonitorSrsId;
            Notes = transformer.Notes;
            ObsoleteDate = transformer.ObsoleteDate.HasValue ? transformer.ObsoleteDate.Value == DateTime.MinValue ? null : (DateTime?)transformer.ObsoleteDate.Value.Date : null;

            if (ObsoleteDate.HasValue)
            {
                foreach (var payItem in _payItems)
                {
                    if (!payItem.EffectiveDate.HasValue) continue;
                    if (!payItem.ObsoleteDate.HasValue)
                    {
                        payItem.ObsoleteDate = ObsoleteDate;
                    }
                }
            }

            OtherReferenceNotes = transformer.OtherReferenceNotes;
            PlanSummary = transformer.PlanSummary;
            //RecommendedPayItems = transformer.RecommendedPayItems;
            //RequiredPayItems = transformer.RequiredPayItems;
            StructureDescription = transformer.StructureDescription;
            StructureId = transformer.StructureId;
            SrsId = transformer.SrsId;
            Title = transformer.Title;
            if (PrimaryUnit != PrimaryUnitType.Mixed)
            {
                PrimaryUnit = transformer.PrimaryUnit;    
            }
            SecondaryUnit = PrimaryUnit == PrimaryUnitType.LS ? transformer.SecondaryUnit : null;
            //if (PrimaryUnit != PrimaryUnitType.Mixed)
            //{
            //    foreach (var payItem in _payItems)
            //    {
            //        payItem.PrimaryUnit = PrimaryUnit;
            //        payItem.SecondaryUnit = SecondaryUnit;
            //    }
            //}
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var pis = _payItemStructureRepository.GetByStructureId(StructureId, Id);
            if (pis != null)
            {
                yield return new ValidationResult("A Pay Item Structure with this Structure ID already exists.");
            }
            if (EffectiveDate == DateTime.MaxValue)
            {
                yield return new ValidationResult("Effective Date is required.");
            }
            else
            {
                foreach (var payItem in _payItems)
                {
                    if (payItem.EffectiveDate.HasValue && payItem.EffectiveDate.Value < EffectiveDate)
                    {
                        yield return new ValidationResult("Effective Date must be equal to or less than each Pay Item's Effective Date.");
                        break;
                    }
                }
                if (ObsoleteDate.HasValue)
                {
                    if (EffectiveDate > ObsoleteDate.Value)
                    {
                        yield return new ValidationResult("Effective Date must be equal to or less than Obsolete Date.");
                    }
                }
            }
            if (ObsoleteDate.HasValue)
            {
                foreach (var payItem in _payItems)
                {
                    if (payItem.ObsoleteDate.HasValue && payItem.ObsoleteDate.Value > ObsoleteDate)
                    {
                        yield return new ValidationResult("Obsolete Date must be equal to or greater than each Pay Item's Obsolete Date.");
                        break;
                    }
                }
            }
            if (PrimaryUnit == PrimaryUnitType.None)
            {
                yield return new ValidationResult("Primary Unit is required.");
            }
            if (PrimaryUnit == PrimaryUnitType.LS && (!SecondaryUnit.HasValue ||  SecondaryUnit.Value == SecondaryUnitType.None))
            {
                yield return new ValidationResult("Secondary Unit is required if Primary Unit is Lump Sum.");
            }
            if (IsFixedPrice && (!FixedAmount.HasValue || FixedAmount.Value == 0))
            {
                yield return new ValidationResult("Fixed Amount is required for Fixed Price Pay Item Structures.");
            }
            if (FixedAmount.HasValue && FixedAmount.Value < 0)
            {
                yield return new ValidationResult("Fixed Amount must be a positive amount.");
            }
            if (BoeRecentChangeDate.HasValue && string.IsNullOrWhiteSpace(BoeRecentChangeDescription) 
                || !BoeRecentChangeDate.HasValue && !string.IsNullOrWhiteSpace(BoeRecentChangeDescription))
            {
                yield return new ValidationResult("Date and description are required for Recent BOE Change.");
            }
        }
    }
}