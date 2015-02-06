using System;
using NHibernate.Session;
using Dqe.ApplicationServices;
using Dqe.Domain.Repositories;
using Dqe.Infrastructure.Repositories;
using Dqe.Infrastructure.Services;

namespace Dqe.Infrastructure.Providers
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    public static class UnitOfWorkProvider
    {
        private const string NotInitializedError = "UnitOfWorkProvider has not been initialized";
        
        private static readonly object Lock = new object();

        private static bool _isInitialized;

        private static Marshaler _marshaler;

        //internal so that the infrastructure project can have direct access to ISession
        internal static Marshaler Marshaler 
        { 
            get
            {
                if (!_isInitialized) throw new InvalidOperationException(NotInitializedError);
                return _marshaler;
            } 
        }

        public static void Initialize(Marshaler marshaler)
        {
            if (_isInitialized) return;
            lock(Lock)
            {
                if (_isInitialized) return;
                _marshaler = marshaler;
                _isInitialized = true;
            }
        }

        //the factory for IoC container to expose an IUnitOfWork 
        public static ICommandRepository CommandRepository
        {
            get
            {
                if (!_isInitialized) throw new InvalidOperationException(NotInitializedError);
                return new CommandRepository(_marshaler);    
            }
        }

        public static ITransactionManager TransactionManager
        {
            get
            {
                if (!_isInitialized) throw new InvalidOperationException(NotInitializedError);
                return new TransactionManager(_marshaler);
            }
        }
    }
}