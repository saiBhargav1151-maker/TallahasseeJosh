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
        private readonly ICollection<PayItemMaster> _payItemMasters;
        private readonly ICollection<StandardWebLink> _standards;
        private readonly ICollection<PpmChapterWebLink> _ppmChapters;
        private readonly ICollection<SpecificationWebLink> _specifications;
        private readonly ICollection<PrepAndDocChapterWebLink> _prepAndDocChapters;
        private readonly ICollection<OtherReferenceWebLink> _otherReferences;

        private readonly IPayItemStructureRepository _payItemStructureRepository;

        public PayItemStructure(IPayItemStructureRepository payItemStructureRepository)
        {
            _payItemMasters = new Collection<PayItemMaster>();
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
        [StringLength(25)]
        [Display(Name = "Structure Number")]
        public virtual string StructureId { get; protected internal set; }

        //length should match PES
        //[Required]
        [StringLength(500)]
        public virtual string Title { get; protected internal set; }

        //public virtual string PrimaryUnit { get; protected internal set; }

        ////required if primary unit is LS - Lump Sum
        //public virtual string SecondaryUnit { get; protected internal set; }

        //[StringLength(100)]
        //public virtual string Accuracy { get; protected internal set; }

        public virtual bool? IsPlanQuantity { get; protected internal set; }

        public virtual bool IsObsolete { get; protected internal set; }

        [StringLength(500)]
        public virtual string Notes { get; protected internal set; }

        [StringLength(8000)]
        public virtual string Details { get; protected internal set; }

        //TODO: related items?

        [StringLength(5000)]
        public virtual string PlanSummary { get; protected internal set; }

        [StringLength(500)]
        public virtual string DesignFormsText { get; protected internal set; }

        [StringLength(500)]
        public virtual string ConstructionFormsText { get; protected internal set; }

        [StringLength(500)]
        public virtual string PpmChapterText { get; protected internal set; }

        [StringLength(500)]
        public virtual string TrnsportText { get; protected internal set; }

        [StringLength(500)]
        public virtual string OtherText { get; protected internal set; }

        [StringLength(500)]
        public virtual string StandardsText { get; protected internal set; }

        [StringLength(500)]
        public virtual string SpecificationsText { get; protected internal set; }

        //TODO: trnsport category

        [StringLength(5000)]
        public virtual string StructureDescription { get; protected internal set; }

        public virtual IEnumerable<PayItemMaster> PayItemMasters
        {
            get { return _payItemMasters.ToList().AsReadOnly(); }
        }

        //---------- private info

        public virtual int SrsId { get; protected internal set; }

        public virtual bool IsMonitored { get; protected internal set; }

        //required if IsMonitored 
        public virtual int? MonitorSrsId { get; protected internal set; }

        [StringLength(5000)]
        public virtual string PendingInformation { get; protected internal set; }

        [StringLength(8000)]
        public virtual string EssHistory { get; protected internal set; }

        public virtual string RequiredItems { get; protected internal set; }

        public virtual string RecommendedItems { get; protected internal set; }

        public virtual string ReplacementItems { get; protected internal set; }

        //required if a BOE chnage description is added
        public virtual DateTime? BoeRecentChangeDate { get; protected internal set; }

        [StringLength(1000)]
        public virtual string BoeRecentChangeDescription { get; protected internal set; }

        public virtual void AddPayItem(PayItemMaster payItemMaster)
        {
            _payItemMasters.Add(payItemMaster);
            payItemMaster.MyPayItemStructure = this;
        }

        public virtual void RemovePayItem(PayItemMaster payItemMaster)
        {
            payItemMaster.MyPayItemStructure = null;
        }

        //---------------------------------------------------------------------------------------------------

        //must be equal or before obsolete date
        //is it required - we think yes?
        //[Display(Name = "Effective Date")]
        //public virtual DateTime EffectiveDate { get; protected internal set; }

        //must be equal or greater effective date
        //[Display(Name = "Obsolete Date")]
        //public virtual DateTime? ObsoleteDate { get; protected internal set; }

        //public virtual bool IsDoNotBid { get; protected internal set; }

        //public virtual bool IsFixedPrice { get; protected internal set; }

        //required if IsFixedPrice == true, otherwise NULL
        //public virtual decimal? FixedAmount { get; protected internal set; }

        //[Range(1, 10)]
        //[Display(Name = "Decimal Precision Number")]
        //public virtual int Accuracy { get; protected internal set; }

        //[StringLength(255)]
        //public virtual string OtherReferenceNotes { get; protected internal set; }

        //[StringLength(255)]
        //public virtual string RequiredPayItems { get; protected internal set; }

        //[StringLength(255)]
        //public virtual string RecommendedPayItems { get; protected internal set; }

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

        //public virtual void AddCostBasedTemplate(CostBasedTemplate costBasedTemplate, DqeUser account)
        //{
        //    if (costBasedTemplate == null) throw new ArgumentNullException("costBasedTemplate");
        //    if (account == null) throw new ArgumentNullException("account");
        //    if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
        //    {
        //        throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
        //    }
        //    MyCostBasedTemplate = costBasedTemplate;
        //}

        //public virtual void RemoveCostBasedTemplate(DqeUser account)
        //{
        //    if (account == null) throw new ArgumentNullException("account");
        //    if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
        //    {
        //        throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
        //    }
        //    MyCostBasedTemplate = null;
        //}

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
                StructureId = StructureId,
                Title = Title,
                //PrimaryUnit = PrimaryUnit,
                //SecondaryUnit = SecondaryUnit,
                //Accuracy = Accuracy,
                IsPlanQuantity = IsPlanQuantity,
                Notes = Notes,
                Details = Details,
                PlanSummary = PlanSummary,
                DesignFormsText = DesignFormsText,
                ConstructionFormsText = ConstructionFormsText,
                PpmChapterText = PpmChapterText,
                TrnsportText = TrnsportText,
                OtherText = OtherText,
                StandardsText = StandardsText,
                SpecificationsText = SpecificationsText,
                StructureDescription = StructureDescription,
                SrsId = SrsId,
                IsMonitored = IsMonitored,
                MonitorSrsId = MonitorSrsId,
                PendingInformation = PendingInformation,
                EssHistory = EssHistory,
                BoeRecentChangeDate = BoeRecentChangeDate,
                BoeRecentChangeDescription = BoeRecentChangeDescription,
                RecommendedItems = RecommendedItems,
                RequiredItems = RequiredItems,
                ReplacementItems = ReplacementItems,
                IsObsolete = IsObsolete
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
            StructureId = transformer.StructureId;
            Title = transformer.Title;

            //if (PrimaryUnit != "MIXED")
            //{
            //    PrimaryUnit = transformer.PrimaryUnit;
            //}
            //SecondaryUnit = PrimaryUnit == "LS" ? transformer.SecondaryUnit : null;

            //Accuracy = transformer.Accuracy;
            IsPlanQuantity = transformer.IsPlanQuantity;
            Notes = transformer.Notes;
            Details = transformer.Details;
            PlanSummary = transformer.PlanSummary;
            DesignFormsText = transformer.DesignFormsText;
            ConstructionFormsText = transformer.ConstructionFormsText;
            PpmChapterText = transformer.PpmChapterText;
            TrnsportText = transformer.TrnsportText;
            OtherText = transformer.OtherText;
            StandardsText = transformer.StandardsText;
            SpecificationsText = transformer.SpecificationsText;
            StructureDescription = transformer.StructureDescription;
            SrsId = transformer.SrsId;
            IsMonitored = transformer.IsMonitored;
            MonitorSrsId = transformer.MonitorSrsId;
            PendingInformation = transformer.PendingInformation;
            EssHistory = transformer.EssHistory;
            BoeRecentChangeDate = transformer.BoeRecentChangeDate;
            BoeRecentChangeDescription = transformer.BoeRecentChangeDescription;
            RequiredItems = transformer.RequiredItems;
            RecommendedItems = transformer.RecommendedItems;
            ReplacementItems = transformer.ReplacementItems;
            IsObsolete = transformer.IsObsolete;
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
}