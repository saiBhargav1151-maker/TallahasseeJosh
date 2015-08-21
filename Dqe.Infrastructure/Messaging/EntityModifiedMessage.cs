using System;
using System.Text;
using NHibernate.DataAnnotations;
using Dqe.Domain.Messaging;

namespace Dqe.Infrastructure.Messaging
{
    /// <summary>
    /// COMPONENT TEMPLATE
    /// </summary>
    public class EntityModifiedMessage : IMessage
    {
        private readonly EntityPersistenceContext _entityPersistenceContext;
        private readonly object _entity;

        public EntityModifiedMessage(object entity, EntityPersistenceContext entityPersistenceContext)
        {
            _entity = entity;
            _entityPersistenceContext = entityPersistenceContext;
        }

        public string Id
        {
            get { return "ENTITY_MODIFIED"; }
        }

        public bool IsCancelable
        {
            get { return true; }
        }

        public override string ToString()
        {
            var nameArray = _entity.GetType().Name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var name = nameArray[nameArray.GetUpperBound(0)];
            var sb = new StringBuilder();
            sb.AppendFormat("Modified {0}:: ", name);
            foreach (var item in _entityPersistenceContext.PreviousState)
            {
                sb.AppendFormat(" [ {0}: {1} ] ", item.Key, item.Value);
            }
            sb.Append("To:: ");
            foreach (var item in _entityPersistenceContext.CurrentState)
            {
                sb.AppendFormat(" [ {0}: {1} ] ", item.Key, item.Value);
            }
            return sb.ToString();
        }
    }
}