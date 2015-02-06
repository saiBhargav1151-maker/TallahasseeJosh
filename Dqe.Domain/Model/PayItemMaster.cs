using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class PayItemMaster : Entity<Transformers.PayItemMaster>
    {
        private readonly ICollection<CountyAveragePrice> _countyAveragePrices;
        private readonly ICollection<MarketAreaAveragePrice> _marketAreaAveragePrices;
        private readonly ICollection<ProposalHistory> _proposalHistories;
        private readonly ICollection<CostGroupPayItem> _costGroups;

        public PayItemMaster()
        {
            _countyAveragePrices = new Collection<CountyAveragePrice>();
            _marketAreaAveragePrices = new Collection<MarketAreaAveragePrice>();
            _proposalHistories = new Collection<ProposalHistory>();
            _costGroups = new Collection<CostGroupPayItem>();
        }

        public virtual IEnumerable<CountyAveragePrice> CountyAveragePrices
        {
            get { return _countyAveragePrices.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<MarketAreaAveragePrice> MarketAreaAveragePrices
        {
            get { return _marketAreaAveragePrices.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<ProposalHistory> ProposalHistories
        {
            get { return _proposalHistories.ToList().AsReadOnly(); }    
        }

        public virtual IEnumerable<CostGroupPayItem> CostGroups
        {
            get { return _costGroups.ToList().AsReadOnly(); }
        } 

        public virtual void ClearHistory()
        {
            _proposalHistories.Clear();
        }

        public virtual ProposalHistory AddProposalHistory(ProposalHistory proposalHistory)
        {
            proposalHistory.MyPayItemMaster = this;
            _proposalHistories.Add(proposalHistory);
            return proposalHistory;
        }

        public virtual void AddCostGroup(CostGroupPayItem costGroupPayItem)
        {
            _costGroups.Add(costGroupPayItem);
            costGroupPayItem.MyPayItem = this;
        }

        public virtual PayItemStructure MyPayItemStructure { get; protected internal set; }

        [Required]
        public virtual MasterFile MyMasterFile { get; protected internal set; }

        public virtual CostBasedTemplate MyCostBasedTemplate { get; protected internal set; }

        [Required]
        public virtual bool IsFixedPrice { get; set; }
        
        [StringLength(20)]
        public virtual string RefItemName { get; protected internal set; }
        //public virtual string PayItemId { get; protected internal set; }

        public virtual string SpecBook 
        {
            get { return MyMasterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'); } 
        }

        [StringLength(256)]
        public virtual string Description { get; protected internal set; }

        /// <summary>
        /// value from code table UNITS
        /// </summary>
        [StringLength(20)]
        public virtual string Unit { get; protected internal set; }

        //required if primary unit is LS - Lump Sum
        public virtual SecondaryUnitType? SecondaryUnit { get; protected internal set; }

        [Required]
        public virtual PrimaryUnitType PrimaryUnit { get; protected internal set; }

        /// <summary>
        /// caculated (GETCALCULATEDUNIT function
        ///SELECT @unit = UNIT, @bidAsLumpSum =  BIDASLUMPSUM FROM trnsport.REFITEM where REFITEM_ID = @refitemId;
        ///
        ///    RETURN CASE
        ///    WHEN (@bidAsLumpSum = 1) THEN 'LS - Lump Sum'
        ///    ELSE (SELECT CODEVALUE_NM + ' - ' + cv.DESCR from trnsport.CODEVALUE cv
        ///         JOIN trnsport.CODETABLE ct on cv.CODETABLE_ID = ct.CODETABLE_ID
        ///         WHERE ct.CODETABLE_NM = 'UNITS' AND cv.CODEVALUE_NM = @unit)
        ///    END;
        /// </summary>
        [StringLength(280)]
        public virtual string CalculatedUnit { get; protected internal set; }

        /// <summary>
        /// value from Codetable UNITTYP, transformed as follows: 'E' is transformed to 'English', 'M' to 'Metric' and 'N' to 'Neutral'
        /// </summary>
        [StringLength(20)]
        public virtual string UnitSystem { get; protected internal set; }

        public virtual int SrsId { get; protected internal set; }

        /// <summary>
        /// Is item Lump Sum? UNIT = 'LS', value is 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool LumpSum { get; protected internal set; }

        /// <summary>
        /// Is item hybrid?  item UNITS =  'LS' or CALCULATEDUNIT = 'LS - Lump Sum', set to 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool BidAsLumpSum { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool SuppDescriptionRequired { get; protected internal set; }
        //public virtual bool IsSupplementalDescriptionRequired { get; protected internal set; }

        public virtual bool CombineWithLikeItems { get; protected internal set; }
        //public virtual bool IsLikeItemsCombined { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool MajorItem { get; protected internal set; }

        /// <summary>
        /// if item is 'DO NOT BID' then set to 1 (true), else set to 0 (false)
        /// A check box indicating that the item will be included in the project but will not appear on any bid documents (for example, a state-supplied item).
        /// </summary>
        public virtual bool NonBid { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool DbeInterest { get; protected internal set; }

        /// <summary>
        /// transformed: 0 is tranformed to NULL, all other values as is.
        /// Value from LRE (LRE BJS job)
        /// </summary>
        public virtual decimal? RefPrice { get; protected internal set; }

        //public virtual DateTime? EffectiveDate { get; protected internal set; }

        public virtual DateTime? ObsoleteDate { get; protected internal set; }

        /// <summary>
        /// value from code table UNITS
        /// </summary>
        [StringLength(20)]
        public virtual string CommonUnit { get; protected internal set; }

        /// <summary>
        /// value from code table ITEMTYP
        /// FDOT uses this for Item Contract Class  (1,7,M)
        /// </summary>
        [StringLength(20)]
        public virtual string ItemType { get; protected internal set; }

        /// <summary>
        /// value from code table ITEMCLS
        /// </summary>
        [StringLength(20)]
        public virtual string ItemClass { get; protected internal set; }

        /// <summary>
        /// value from code table CONTCLS
        /// FDOT uses this for  Vendor Pre-qualification Class
        /// </summary>
        [StringLength(20)]
        public virtual string ContractClass { get; protected internal set; }

        [StringLength(256)]
        public virtual string ShortDescription { get; protected internal set; }

        public virtual decimal? ConversionFactorToCommonUnit { get; protected internal set; }

        public virtual decimal ConcreteFactor { get; protected internal set; }

        public virtual decimal AsphaltFactor { get; protected internal set; }

        public virtual bool IsFederalFunded { get; protected internal set; }

        [StringLength(5000)]
        public virtual string FactorNotes { get; protected internal set; }

        public virtual decimal? DbePercentToApply { get; protected internal set; }

        /// <summary>
        /// Set to NULL
        /// </summary>
        public virtual string RecordSource { get; protected internal set; }

        /// <summary>
        /// A value that may be assigned to an item indicating that pricing and bids for this item will be restricted in one of three ways: price may be fixed, may have an assigned minimum value, or may have an assigned maximum value. If the value is set to Minimum or Maximum, you must also enter a value in the Project Item Unit Price Comparison field.
        /// </summary>
        public virtual string BidRequirementCode { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// A unique identifier assigned to each Administrative Office in the system
        /// </summary>
        public virtual bool Administrative { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// A check box indicating that the reference item does not have a Material Acceptance Action
        /// </summary>
        public virtual bool ExemptFromMaa { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool PayPlan { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool AutoPaidPercentSchedule { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool CoApprovalRequired { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool PercentScheduleItem { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool FuelAdjustment { get; protected internal set; }

        public virtual DateTime? CreatedDate { get; protected internal set; }

        [StringLength(256)]
        public virtual string CreatedBy { get; protected internal set; }

        public virtual DateTime? LastUpdatedDate { get; protected internal set; }

        [StringLength(256)]
        public virtual string LastUpdatedBy { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool SpecialtyItem { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool ExemptFromRetainage { get; protected internal set; }

        /// <summary>
        /// set to 1 (true), else set to 0 (false)
        /// </summary>
        public virtual bool RegressionInclusion { get; protected internal set; }

        [StringLength(20)]
        public virtual string AlternateItemName { get; protected internal set; }

        /// <summary>
        /// value from code table FUELTYP
        /// </summary>
        [StringLength(20)]
        public virtual string FuelAdjustmentType { get; protected internal set; }

        /// <summary>
        /// Valid  effective with “LDT1” letting date mmddyy
        /// </summary>
        public virtual DateTime? EffectiveDate { get; protected internal set; }

        public virtual DateTime? Ildt2 { get; protected internal set; }

        /// <summary>
        /// Item Opened
        /// </summary>
        public virtual DateTime? OpenedDate { get; protected internal set; }

        /// <summary>
        /// Pay item spec type
        /// </summary>
        public virtual string Ilflg1 { get; protected internal set; }

        public virtual decimal? Ilnum1 { get; protected internal set; }

        /// <summary>
        /// Value from LRE (LRE BJS job)
        /// </summary>
        [StringLength(256)]
        public virtual string Ilsst1 { get; protected internal set; }

        /// <summary>
        /// value "NPART" only for non-partisipating specail items, else no value
        /// </summary>
        [StringLength(256)]
        public virtual string Illst1 { get; protected internal set; }

        [StringLength(1)]
        public virtual string Itmqtyprecsn { get; protected internal set; }

        public virtual decimal? StateReferencePrice { get; protected internal set; }

        public override Transformers.PayItemMaster GetTransformer()
        {
            return new Transformers.PayItemMaster
            {
                Id = Id,
                RefItemName = RefItemName,
                Description = Description,
                Unit = Unit,
                CalculatedUnit = CalculatedUnit,
                UnitSystem = UnitSystem,
                LumpSum = LumpSum,
                BidAsLumpSum = BidAsLumpSum,
                SuppDescriptionRequired = SuppDescriptionRequired,
                CombineWithLikeItems = CombineWithLikeItems,
                MajorItem = MajorItem,
                NonBid = NonBid,
                DbeInterest = DbeInterest,
                RefPrice = RefPrice,
                ObsoleteDate = ObsoleteDate,
                CommonUnit = CommonUnit,
                ItemType = ItemType,
                ItemClass = ItemClass,
                ContractClass = ContractClass,
                ShortDescription = ShortDescription,
                ConversionFactorToCommonUnit = ConversionFactorToCommonUnit,
                DbePercentToApply = DbePercentToApply,
                RecordSource = RecordSource,
                BidRequirementCode = BidRequirementCode,
                Administrative = Administrative,
                ExemptFromMaa = ExemptFromMaa,
                PayPlan = PayPlan,
                AutoPaidPercentSchedule = AutoPaidPercentSchedule,
                CoApprovalRequired = CoApprovalRequired,
                PercentScheduleItem = PercentScheduleItem,
                FuelAdjustment = FuelAdjustment,
                CreatedDate = CreatedDate,
                CreatedBy = CreatedBy,
                LastUpdatedDate = LastUpdatedDate,
                LastUpdatedBy = LastUpdatedBy,
                SpecialtyItem = SpecialtyItem,
                ExemptFromRetainage = ExemptFromRetainage,
                RegressionInclusion = RegressionInclusion,
                AlternateItemName = AlternateItemName,
                FuelAdjustmentType = FuelAdjustmentType,
                Ildt2 = Ildt2,
                OpenedDate = OpenedDate,
                Ilflg1 = Ilflg1,
                Ilnum1 = Ilnum1,
                Ilsst1 = Ilsst1,
                Illst1 = Illst1,
                Itmqtyprecsn = Itmqtyprecsn,
                StateReferencePrice = StateReferencePrice,

                PrimaryUnit = PrimaryUnit,
                SecondaryUnit = SecondaryUnit,
                SrsId = SrsId,
                EffectiveDate = EffectiveDate,
                ConcreteFactor = ConcreteFactor,
                AsphaltFactor = AsphaltFactor,
                IsFederalFunded = IsFederalFunded,
                FactorNotes = FactorNotes,
                IsFixedPrice = IsFixedPrice
            };
        }

        public override void Transform(Transformers.PayItemMaster transformer, DqeUser account)
        {
            RefItemName = transformer.RefItemName;
            Description = transformer.Description;
            Unit = transformer.Unit;
            CalculatedUnit = transformer.CalculatedUnit;
            UnitSystem = transformer.UnitSystem;
            LumpSum = transformer.LumpSum;
            BidAsLumpSum = transformer.BidAsLumpSum;
            SuppDescriptionRequired = transformer.SuppDescriptionRequired;
            CombineWithLikeItems = transformer.CombineWithLikeItems;
            MajorItem = transformer.MajorItem;
            NonBid = transformer.NonBid;
            DbeInterest = transformer.DbeInterest;
            RefPrice = transformer.RefPrice;
            ObsoleteDate = transformer.ObsoleteDate;
            CommonUnit = transformer.CommonUnit;
            ItemType = transformer.ItemType;
            ItemClass = transformer.ItemClass;
            ContractClass = transformer.ContractClass;
            ShortDescription = transformer.ShortDescription;
            ConversionFactorToCommonUnit = transformer.ConversionFactorToCommonUnit;
            DbePercentToApply = transformer.DbePercentToApply;
            RecordSource = transformer.RecordSource;
            BidRequirementCode = transformer.BidRequirementCode;
            Administrative = transformer.Administrative;
            ExemptFromMaa = transformer.ExemptFromMaa;
            PayPlan = transformer.PayPlan;
            AutoPaidPercentSchedule = transformer.AutoPaidPercentSchedule;
            CoApprovalRequired = transformer.CoApprovalRequired;
            PercentScheduleItem = transformer.PercentScheduleItem;
            FuelAdjustment = transformer.FuelAdjustment;
            CreatedDate = transformer.CreatedDate;
            CreatedBy = transformer.CreatedBy;
            LastUpdatedDate = transformer.LastUpdatedDate;
            LastUpdatedBy = transformer.LastUpdatedBy;
            SpecialtyItem = transformer.SpecialtyItem;
            ExemptFromRetainage = transformer.ExemptFromRetainage;
            RegressionInclusion = transformer.RegressionInclusion;
            AlternateItemName = transformer.AlternateItemName;
            FuelAdjustmentType = transformer.FuelAdjustmentType;
            Ildt2 = transformer.Ildt2;
            OpenedDate = transformer.OpenedDate;
            Ilflg1 = transformer.Ilflg1;
            Ilnum1 = transformer.Ilnum1;
            Ilsst1 = transformer.Ilsst1;
            Illst1 = transformer.Illst1;
            Itmqtyprecsn = transformer.Itmqtyprecsn;
            StateReferencePrice = transformer.StateReferencePrice;

            PrimaryUnit = transformer.PrimaryUnit;
            SecondaryUnit = transformer.SecondaryUnit;
            SrsId = transformer.SrsId;
            EffectiveDate = transformer.EffectiveDate;
            ConcreteFactor = transformer.ConcreteFactor;
            AsphaltFactor = transformer.AsphaltFactor;
            IsFederalFunded = transformer.IsFederalFunded;
            FactorNotes = transformer.FactorNotes;
            IsFixedPrice = transformer.IsFixedPrice;
        }
    }
}
