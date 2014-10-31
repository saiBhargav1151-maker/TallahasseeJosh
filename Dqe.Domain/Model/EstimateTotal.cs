using System;
using System.Collections.Generic;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class EstimateTotal
    {
        private readonly List<CategorySet> _categorySets = new List<CategorySet>();

        public decimal Total
        {
            get
            {
                Decimal total = 0;
                var nonAlt = _categorySets.FirstOrDefault(i => i.Set == string.Empty && i.Member == string.Empty);
                if (nonAlt != null)
                {
                    total += nonAlt.Total;
                }
                var sets = _categorySets.Where(i => i.Set != string.Empty).Select(i => i.Set).Distinct().ToList();
                foreach (var set in sets)
                {
                    var members = _categorySets.Where(i => i.Set == set).Select(i => i.Member).Distinct().ToList();
                    Decimal subTotal = 0;
                    foreach (var member in members)
                    {
                        subTotal = Math.Min(_categorySets.First(i => i.Set == set && i.Member == member).Total, subTotal);
                    }
                    total += subTotal;
                }
                return total;
            }
        }
        
        public IList<CategorySet> CategorySets { get { return _categorySets; } }
    }
}