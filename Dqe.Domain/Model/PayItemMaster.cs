using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class PayItemMaster : Entity<Transformers.PayItemMaster>
    {
        private readonly ICollection<CountyAveragePrice> _countyAveragePrices;
        private readonly ICollection<MarketAreaAveragePrice> _marketAreaAveragePrices;

        public PayItemMaster()
        {
            _countyAveragePrices = new Collection<CountyAveragePrice>();
            _marketAreaAveragePrices = new Collection<MarketAreaAveragePrice>();
        }

        public virtual IEnumerable<CountyAveragePrice> CountyAveragePrices
        {
            get { return _countyAveragePrices.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<MarketAreaAveragePrice> MarketAreaAveragePrices
        {
            get { return _marketAreaAveragePrices.ToList().AsReadOnly(); }
        }
            
        [StringLength(20)]
        public virtual string RefItemName { get; protected internal set; }

        [StringLength(20)]
        public virtual string SpecBook { get; protected internal set; }

        [StringLength(256)]
        public virtual string Description { get; protected internal set; }

        [StringLength(20)]
        public virtual string Unit { get; protected internal set; }

        [StringLength(280)]
        public virtual string CalculatedUnit { get; protected internal set; }

        [StringLength(20)]
        public virtual string UnitSystem { get; protected internal set; }

        public virtual bool LumpSum { get; protected internal set; }

        public virtual bool BidAsLumpSum { get; protected internal set; }

        public virtual bool SuppDescriptionRequired { get; protected internal set; }

        public virtual bool CombineWithLikeItems { get; protected internal set; }

        public virtual bool MajorItem { get; protected internal set; }

        public virtual bool NonBid { get; protected internal set; }

        public virtual bool DbeInterest { get; protected internal set; }

        public virtual decimal? RefPrice { get; protected internal set; }

        public virtual DateTime? ObsoleteDate { get; protected internal set; }

        [StringLength(20)]
        public virtual string CommonUnit { get; protected internal set; }

        [StringLength(20)]
        public virtual string ItemType { get; protected internal set; }

        [StringLength(20)]
        public virtual string ItemClass { get; protected internal set; }

        [StringLength(20)]
        public virtual string ContractClass { get; protected internal set; }

        [StringLength(256)]
        public virtual string ShortDescription { get; protected internal set; }

        public virtual decimal? ConversionFactorToCommonUnit { get; protected internal set; }

        public virtual decimal? DbePercentToApply { get; protected internal set; }

        public virtual string RecordSource { get; protected internal set; }

        public virtual string BidRequirementCode { get; protected internal set; }

        public virtual bool Administrative { get; protected internal set; }

        public virtual bool ExemptFromMaa { get; protected internal set; }

        public virtual bool PayPlan { get; protected internal set; }

        public virtual bool AutoPaidPercentSchedule { get; protected internal set; }

        public virtual bool CoApprovalRequired { get; protected internal set; }

        public virtual bool PercentScheduleItem { get; protected internal set; }

        public virtual bool FuelAdjustment { get; protected internal set; }

        public virtual DateTime? CreatedDate { get; protected internal set; }

        [StringLength(256)]
        public virtual string CreatedBy { get; protected internal set; }

        public virtual DateTime? LastUpdatedDate { get; protected internal set; }

        [StringLength(256)]
        public virtual string LastUpdatedBy { get; protected internal set; }

        public virtual bool SpecialtyItem { get; protected internal set; }

        public virtual bool ExemptFromRetainage { get; protected internal set; }

        public virtual bool RegressionInclusion { get; protected internal set; }

        [StringLength(20)]
        public virtual string AlternateItemName { get; protected internal set; }

        [StringLength(20)]
        public virtual string FuelAdjustmentType { get; protected internal set; }

        public virtual DateTime? Ildt1 { get; protected internal set; }

        public virtual DateTime? Ildt2 { get; protected internal set; }

        public virtual DateTime? Ildt3 { get; protected internal set; }

        public virtual string Ilflg1 { get; protected internal set; }

        public virtual decimal? Ilnum1 { get; protected internal set; }

        [StringLength(256)]
        public virtual string Ilsst1 { get; protected internal set; }

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
                SpecBook = SpecBook,
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
                Ildt1 = Ildt1,
                Ildt2 = Ildt2,
                Ildt3 = Ildt3,
                Ilflg1 = Ilflg1,
                Ilnum1 = Ilnum1,
                Ilsst1 = Ilsst1,
                Illst1 = Illst1,
                Itmqtyprecsn = Itmqtyprecsn,
                StateReferencePrice = StateReferencePrice
            };
        }

        public override void Transform(Transformers.PayItemMaster transformer, DqeUser account)
        {
            RefItemName = transformer.RefItemName;
            SpecBook = transformer.SpecBook;
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
            Ildt1 = transformer.Ildt1;
            Ildt2 = transformer.Ildt2;
            Ildt3 = transformer.Ildt3;
            Ilflg1 = transformer.Ilflg1;
            Ilnum1 = transformer.Ilnum1;
            Ilsst1 = transformer.Ilsst1;
            Illst1 = transformer.Illst1;
            Itmqtyprecsn = transformer.Itmqtyprecsn;
            StateReferencePrice = transformer.StateReferencePrice;
        }
    }
}
