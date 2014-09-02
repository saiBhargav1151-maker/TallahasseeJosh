using System;
using NHibernate;
using NHibernate.Session;
using Dqe.ApplicationServices;
using Dqe.Infrastructure.Messaging;

namespace Dqe.Infrastructure.Services
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    public class TransactionManager : ITransactionManager
    {
        private readonly Marshaler _marshaler;

        public TransactionManager(Marshaler marshaler)
        {
            _marshaler = marshaler;
        }

        public Guid Id
        {
            get
            {
                return _marshaler.HasSession
                           ? _marshaler.CurrentSession.GetSessionImplementation().SessionId
                           : Guid.Empty;
            }
        }

        public void Abort()
        {
            if (!_marshaler.HasSession) return;
            if (_marshaler.CurrentSession.Transaction != null && _marshaler.CurrentSession.Transaction.IsActive)
            {
                PurgeQueue(Id);
                _marshaler.CurrentSession.Transaction.Rollback();
            }
            _marshaler.End();
        }

        public void Commit()
        {
            if (!_marshaler.HasSession) return;
            var success = true;
            try
            {
                if (_marshaler.CurrentSession.Transaction != null && _marshaler.CurrentSession.Transaction.IsActive)
                {
                    _marshaler.CurrentSession.GetValidator().Eval(_marshaler.CurrentSession.Transaction);
                }
            }
            catch
            {
                success = false;
                PurgeQueue(Id);
                throw;
            }
            finally
            {
                if (success) PublishQueue(Id);
                _marshaler.End();
            }
        }

        public void PurgeQueue(Guid id)
        {
            //TODO: implement
            try
            {
                MessageRepository.Purge(id);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                //log errors
            }
        }

        public void PublishQueue(Guid id)
        {
            //TODO: implement
            try
            {
                MessageRepository.Publish(id);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                //log errors
            }
        }
    }
}
