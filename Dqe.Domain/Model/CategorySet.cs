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

        public decimal Total
        {
            get
            {
                Decimal total = 0;
                var nonAlt = _itemSets.FirstOrDefault(i => i.Set == string.Empty && i.Member == string.Empty);
                if (nonAlt != null)
                {
                    total += nonAlt.Total;
                }
                var sets = _itemSets.Where(i => i.Set != string.Empty).Select(i => i.Set).Distinct().ToList();
                foreach (var set in sets)
                {
                    var members = _itemSets.Where(i => i.Set == set).Select(i => i.Member).Distinct().ToList();
                    Decimal subTotal = 0;
                    foreach (var member in members)
                    {
                        subTotal = Math.Min(_itemSets.First(i => i.Set == set && i.Member == member).Total, subTotal);
                    }
                    total += subTotal;
                }
                return total;
            }
        }

        public IList<ItemSet> ItemSets { get { return _itemSets; } }
    }
}