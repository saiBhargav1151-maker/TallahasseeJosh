using System;

namespace Dqe.Domain.Transformers
{
    public class PayItem : Transformer
    {
        public string PayItemId { get; set; }
        public DateTime OpenedDate { get; set; }
        public DateTime ObsoleteDate { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public decimal ReferencePrice { get; set; }
        public decimal ConcreteFactor { get; set; }
        public decimal AsphaltFactor { get; set; }
        public decimal FuelFactor { get; set; }
        public string FactorNotes { get; set; }
        public bool IsLikeItemsCombined { get; set; }
        public bool IsSupplementalDescriptionRequired { get; set; }
        public bool IsFederalFunded { get; set; }
        public int ContractTypeCode { get; set; }
        public int ItemClassCode { get; set; }
    }
}