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
                    nonAlt.Included = true;
                }
                var sets = _categorySets.Where(i => i.Set != string.Empty).Select(i => i.Set).Distinct().OrderBy(i => i).ToList();
                foreach (var set in sets)
                {
                    var members = _categorySets.Where(i => i.Set == set).Select(i => i.Member).Distinct().OrderBy(i => i).ToList();
                    var dict = new Dictionary<CategorySet, decimal>();
                    foreach (var member in members)
                    {
                        var categorySet = _categorySets.First(i => i.Set == set && i.Member == member);
                        categorySet.Included = false;
                        dict.Add(categorySet, categorySet.Total);
                    }
                    var lowests =
                        dict.Where(i => i.Value == dict.Min(ii => ii.Value))
                            .Select(i => i.Key)
                            .OrderBy(i => i.Set)
                            .ThenBy(i => i.Member)
                            .ToList();
                    if (lowests.Count > 0)
                    {
                        var lowest = lowests.Last();
                        lowest.Included = true;
                        total += dict[lowest];
                    }
                }
                return total;
            }
        }

        public void Initialize()
        {
            var t = Total;
        }
        
        public IList<CategorySet> CategorySets { get { return _categorySets; } }
    }
}