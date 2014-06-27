using System;
using System.Collections.Generic;
using System.Security.Principal;
using Dqe.Domain.Services;
using Dqe.Infrastructure.Services;
using Ninject;
using Dqe.ApplicationServices;
using Dqe.Domain.Messaging;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure;
using Dqe.Infrastructure.EntityIoC;
using Dqe.Infrastructure.Messaging;
using Dqe.Infrastructure.Providers;
using Dqe.Infrastructure.Repositories.Custom;

namespace Dqe.Inverter
{
    /// <summary>
    /// COMPONENT TEMPLATE
    /// </summary>
    public static class Container
    {
        private const string NotInitializedError = "Dqe.Coupler.Container has not been initialized";

        private static readonly object Lock = new object();

        private static bool _isInitialized;

        private static IKernel _kernel;

        private static IIdentityProvider _identityProvider;
        private static IContextService _contextService;

        public static void Initialize(IIdentityProvider identityProvider, IContextService contextService)
        {
            if (_isInitialized) return;
            lock (Lock)
            {
                if (_isInitialized) return;
                _kernel = new StandardKernel();
                Initializer.Initialize();
                _identityProvider = identityProvider;
                _contextService = contextService;
                Bind();
                EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
                _isInitialized = true;
            }
        }

        /// <summary>
        /// REQUIRES CUSTOM IMPLEMENTATION
        /// NOTE: Try to avoid injecting dependencies into entities.
        /// NOTE: Doing so will obscure the intention.
        /// NOTE: Passing the service and utilizing a single or double-dispatch pattern will be more intention-revealing
        /// NOTE: Sometimes though, you just have to do it...
        /// NOTE: In these circumstances, try to keep the dependency domain-related.
        /// </summary>
        /// <param name="sender">IEntityInjector</param>
        /// <param name="args">ResolveConstructorArgumentsArgs</param>
        /// <returns>object array of the constructor arguments for the type, or null for default construction</returns>
        private static object[] EntityDependencyResolverOnResolveConstructorArguments(object sender, ResolveConstructorArgumentsArgs args)
        {
            if (args.EntityType.IsAssignableFrom(typeof(UserAccount))) return new object[] { _kernel.Get<IMessenger>() };
            if (args.EntityType.IsAssignableFrom(typeof(DqeUser))) return new object[] { _kernel.Get<IStaffService>() };
            return null;
        }

        /// <summary>
        /// REQUIRES CUSTOM IMPLEMENTATION
        /// </summary>
        private static void Bind()
        {
            //bind services            
            _kernel.Bind<ITransactionManager>().ToMethod(delegate { return UnitOfWorkProvider.TransactionManager; });
            _kernel.Bind<IMessenger>().To<Messenger>();
            _kernel.Bind<IContextService>().ToMethod(delegate { return _contextService; });
            _kernel.Bind<IStaffService>().To<StaffService>();
            
            //bind user identity and principal
            _kernel.Bind<IIdentity>().ToMethod(delegate { return _identityProvider.Current; });
            _kernel.Bind<IPrincipal>().To<CustomPrincipal>();

            //bind repositories
            _kernel.Bind<ICommandRepository>().ToMethod(delegate { return UnitOfWorkProvider.CommandRepository; });
            _kernel.Bind<IUserAccountRepository>().To<UserAccountRepository>();
            _kernel.Bind<IDqeUserRepository>().To<DqeUserRepository>();

        }

        public static ITransactionManager ResolveTransactionManager()
        {
            if (!_isInitialized) throw new InvalidOperationException(NotInitializedError);
            return _kernel.Get<ITransactionManager>();
        }

        public static IPrincipal ResolveCurrentPrincipal()
        {
            if (!_isInitialized) throw new InvalidOperationException(NotInitializedError);
            return _kernel.Get<IPrincipal>();
        }

        public static object ResolveService(Type serviceType)
        {
            if (!_isInitialized) throw new InvalidOperationException(NotInitializedError);
            return _kernel.TryGet(serviceType);
        }

        public static IEnumerable<object> ResolveServiceCollection(Type serviceType)
        {
            if (!_isInitialized) throw new InvalidOperationException(NotInitializedError);
            return _kernel.GetAll(serviceType);
        }

        public static void BindController(Type controller)
        {
            if (!_isInitialized) throw new InvalidOperationException(NotInitializedError);
            _kernel.Bind(controller).ToSelf().InTransientScope();
        }

        public static object ResolveController(Type controller)
        {
            if (!_isInitialized) throw new InvalidOperationException(NotInitializedError);
            return _kernel.Get(controller);
        }
    }
}
