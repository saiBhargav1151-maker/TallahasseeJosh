using System;
using System.Collections.Generic;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class CategorySet
    {
        private readonly List<ItemSet> _itemSets = new List<ItemSet>(); 
        
        public string Set { get; set; }

        public string Member { get; set; }

        public bool Included { get; internal set; }

        public decimal Total
        {
            get
            {
                Decimal total = 0;
                var nonAlt = _itemSets.FirstOrDefault(i => i.Set == string.Empty && i.Member == string.Empty);
                if (nonAlt != null)
                {
                    total += nonAlt.Total;
                    nonAlt.Included = true;
                }
                var sets = _itemSets.Where(i => i.Set != string.Empty).Select(i => i.Set).Distinct().OrderBy(i => i).ToList();
                foreach (var set in sets)
                {
                    var members = _itemSets.Where(i => i.Set == set).Select(i => i.Member).Distinct().OrderBy(i => i).ToList();
                    var dict = new Dictionary<ItemSet, decimal>();
                    foreach (var member in members)
                    {
                        var itemSet = _itemSets.First(i => i.Set == set && i.Member == member);
                        itemSet.Included = false;
                        dict.Add(itemSet, itemSet.Total);
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

        public IList<ItemSet> ItemSets { get { return _itemSets; } }
    }
}