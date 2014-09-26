using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dqe.Domain.Model;

namespace Dqe.Domain.Transformers
{
    public class PricingParameter : Transformer
    {
        public int Months { get; set; }
        public ContractType ContractType { get; set; }
        public QuantitiesType Quantities { get; set; }
        public IEnumerable<WorkType> WorkTypes { get; set; }
        public PricingModelType PricingModel { get; set; }
        public BiddersType Bidders { get; set; }
    }

    public class DistrictPricingParameter : PricingParameter
    {
        public string District { get; set; }
    }

    public class EstimatorPricingParameter : PricingParameter
    {
        public Model.DqeUser User { get; set; }
    }
}
