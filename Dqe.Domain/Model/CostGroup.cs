using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;

namespace Dqe.Domain.Model
{
    public class CostGroup : Entity<Transformers.CostGroup>
    {
        private readonly ICollection<CostGroupPayItem> _payItems;

        public CostGroup()
        {
            _payItems = new Collection<CostGroupPayItem>();
        }

        [StringLength(20)]
        public virtual string Name { get; protected internal set; }

        [StringLength(256)]
        public virtual string Description { get; protected internal set; }

        [StringLength(20)]
        public virtual string Unit { get; protected internal set; }

        public virtual IEnumerable<CostGroupPayItem> PayItems
        {
            get { return _payItems.ToList().AsReadOnly(); }
        }

        public virtual void AddPayItem(CostGroupPayItem costGroupPayItem)
        {
            _payItems.Add(costGroupPayItem);
            costGroupPayItem.MyCostGroup = this;
        }

        public override Transformers.CostGroup GetTransformer()
        {
            return new Transformers.CostGroup
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Unit = Unit
            };
        }

        public override void Transform(Transformers.CostGroup transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.Administrator && account.Role != DqeRole.System)
            {
                throw new SecurityException("Not authorized");
            }
            Name = transformer.Name;
            Description = transformer.Description;
            Unit = transformer.Unit;
        }
    }
}