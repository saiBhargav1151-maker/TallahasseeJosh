using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;

namespace Dqe.Domain.Model
{
    public class County : Entity<Transformers.County>
    {
        private readonly ICollection<Project> _projects;

        public County()
        {
            _projects = new Collection<Project>();
        }
        
        [StringLength(100)]
        public virtual string Name { get; protected internal set; }

        [StringLength(2, MinimumLength = 2)]
        public virtual string Code { get; protected internal set; }

        public virtual MarketArea MyMarketArea { get; protected internal set; }

        public virtual IEnumerable<Project> Projects
        {
            get { return _projects.ToList().AsReadOnly(); }
        }

        public virtual void RemoveFromMarketArea(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            MyMarketArea = null;
        }

        public override Transformers.County GetTransformer()
        {
            return new Transformers.County
            {
                Code = Code,
                Name = Name,
                Id = Id
            };
        }

        public override void Transform(Transformers.County transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            Code = transformer.Code;
            Name = transformer.Name;
        }
    }
}