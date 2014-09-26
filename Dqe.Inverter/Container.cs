using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Principal;
using Dqe.ApplicationServices.Fdot;
using Dqe.Domain.Services;
using Dqe.Infrastructure.Fdot;
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

        private static IDeferredTaskHubContext _deferredTaskHubContext;

        public static void Initialize(IIdentityProvider identityProvider, IDeferredTaskHubContext deferredTaskHubContext)
        {
            if (_isInitialized) return;
            lock (Lock)
            {
                if (_isInitialized) return;
                _kernel = new StandardKernel();
                Initializer.Initialize();
                _identityProvider = identityProvider;
                _deferredTaskHubContext = deferredTaskHubContext;
                Bind();
                EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
                Initializer.CreateSystemAccount();
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
            if (args.EntityType.IsAssignableFrom(typeof(DqeUser))) return new object[] { _kernel.Get<IStaffService>(), _kernel.Get<IDqeUserRepository>() };
            if (args.EntityType.IsAssignableFrom(typeof(PayItemStructure))) return new object[] { _kernel.Get<IPayItemStructureRepository>() };
            if (args.EntityType.IsAssignableFrom(typeof(MasterFile))) return new object[] { _kernel.Get<IMasterFileRepository>() };
            if (args.EntityType.IsAssignableFrom(typeof(PayItem))) return new object[] { _kernel.Get<IPayItemRepository>() };
            if (args.EntityType.IsAssignableFrom(typeof(Project))) return new object[] { _kernel.Get<IProjectRepository>(), _kernel.Get<ICommandRepository>() };
            if (args.EntityType.IsAssignableFrom(typeof(MarketArea))) return new object[] { _kernel.Get<IMarketAreaRepository>() };
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
            _kernel.Bind<IStaffService>().To<StaffService>();
            _kernel.Bind<ITaskRunner>().To<TaskRunner>();

            //bind client push messenger
            _kernel.Bind<IDeferredTaskHubContext>().ToMethod(delegate { return _deferredTaskHubContext; });
            
            //bind user identity and principal
            _kernel.Bind<IIdentity>().ToMethod(delegate { return _identityProvider.Current; });
            _kernel.Bind<IPrincipal>().To<CustomPrincipal>();

            //bind repositories
            _kernel.Bind<ICommandRepository>().ToMethod(delegate { return UnitOfWorkProvider.CommandRepository; });
            _kernel.Bind<IDqeUserRepository>().To<DqeUserRepository>();
            _kernel.Bind<IDqeCodeRepository>().To<DqeCodeRepository>();
            _kernel.Bind<IDqeWebLinkRepository>().To<DqeWebLinkRepository>();
            _kernel.Bind<ICostBasedTemplateRepository>().To<CostBasedTemplateRepository>();
            _kernel.Bind<IPayItemStructureRepository>().To<PayItemStructureRepository>();
            _kernel.Bind<IDocumentService>().To<DatabaseDocumentService>();
            _kernel.Bind<IDocumentConverterService>().To<DocumentConverterService>();
            _kernel.Bind<IMasterFileRepository>().To<MasterFileRepository>();
            _kernel.Bind<IPayItemRepository>().To<PayItemRepository>();
            _kernel.Bind<IPricingParameterRepository>().To<PricingParameterRepository>();
            _kernel.Bind<ISystemTaskRepository>().To<SystemTaskRepository>();
            _kernel.Bind<IProjectRepository>().To<ProjectRepository>();
            _kernel.Bind<IMarketAreaRepository>().To<MarketAreaRepository>();

            //bind FDOT services
#if DEBUG
            var vpn = Convert.ToBoolean(ConfigurationManager.AppSettings["vpn"]);
            if (vpn)
            {
                _kernel.Bind<ILreService>().To<LreService>();
                _kernel.Bind<IWebTransportService>().To<WebTransportService>();
            }
            else
            {
                _kernel.Bind<ILreService>().To<MockLreService>();
                _kernel.Bind<IWebTransportService>().To<MockWebTransportService>();
            }
#else
            _kernel.Bind<ILreService>().To<LreService>();
            _kernel.Bind<IWebTransportService>().To<WebTransportService>();
#endif
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
