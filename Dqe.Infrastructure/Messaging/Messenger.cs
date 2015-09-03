using System;
using System.ComponentModel.DataAnnotations;
using NHibernate.DataAnnotations;
using Dqe.Domain.Messaging;
using Dqe.Infrastructure.Providers;

namespace Dqe.Infrastructure.Messaging
{
    /// <summary>
    /// COMPONENT TEMPLATE
    /// you can tie this to a service bus implementation for async messaging
    /// the event source is implemented in the domain
    /// </summary>
    public class Messenger : IMessenger
    {
        public void Notify(IMessage message)
        {
            if (message.IsCancelable)
            {
                Add(message);
                return;
            }
            Publish(message);
        }

        public void Notify(object o, ValidationContext validationContext)
        {
            //notify subscribers of entity add, remove, and change events
            //always transaction bound
            if (validationContext == null) return;
            if (validationContext.Items == null) return;
            if (!validationContext.Items.ContainsKey("EntityPersistenceContext")) return;
            var persistenceContext = (EntityPersistenceContext)validationContext.Items["EntityPersistenceContext"];
            if (persistenceContext.IsBeingAdded)
                Add(new EntityAddedMessage(validationContext.ObjectInstance, persistenceContext));
            if (persistenceContext.IsBeingModified)
                Add(new EntityModifiedMessage(validationContext.ObjectInstance, persistenceContext));
            if (persistenceContext.IsBeingRemoved)
                Add(new EntityRemovedMessage(validationContext.ObjectInstance, persistenceContext));
        }

        private static void Publish(IMessage message)
        {
            //direct to service bus, log, email, etc...
        }

        private static void Add(IMessage message)
        {
            if (UnitOfWorkProvider.TransactionManager.Id == Guid.Empty) return;
            MessageRepository.Add(UnitOfWorkProvider.TransactionManager.Id, message);
        }
    }
}