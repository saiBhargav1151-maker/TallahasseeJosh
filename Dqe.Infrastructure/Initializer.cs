using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Dqe.Domain.Model;
using Dqe.Infrastructure.Repositories.Custom;
using Dqe.Infrastructure.Services;
using NHibernate.DataAnnotations;
using NHibernate.Session;
using NHibernate.Tool.hbm2ddl;
using Dqe.Infrastructure.EntityIoC;
using Dqe.Infrastructure.Providers;
using Configuration = NHibernate.Cfg.Configuration;

namespace Dqe.Infrastructure
{
    /// <summary>
    /// COMPONENT TEMPLATE
    /// </summary>
    public static class Initializer
    {
        private static readonly object Lock = new object();

        private static bool _isInitialized;

        private static readonly IList<string> DdlLines = new List<string>();

        /// <summary>
        /// REQUIRES IMPLEMENTATION
        /// </summary>
        public static void Initialize(bool isUnitTesting = false)
        {
            //isUnitTesting is set to true in persistence test setup methods to force the wipe of the database before each test is executed
            if (_isInitialized && !isUnitTesting) return;
            lock(Lock)
            {
                if (_isInitialized && !isUnitTesting) return;
                var configuration = new Configuration();
                //NOTE: CUSTOM - if you want a specific test database, you can do something like the following
                var asm = typeof(Initializer).Assembly;
#if DEBUG
                if (isUnitTesting) { configuration.Configure(); }
                else { configuration.Configure(asm, "Dqe.Infrastructure.Configuration.hibernate.config.debug.xml"); }
#else
                configuration.Configure(asm, "Dqe.Infrastructure.Configuration.hibernate.config.release.xml");
#endif
                //NOTE: CUSTOM - add your mapping assembly
                configuration.AddAssembly(asm);
                //NOTE: STANDARD - this line is what enables DI with NHibernate
                NHibernate.DependencyInjection.Initializer.RegisterBytecodeProvider(new EntityInjector());
                //NOTE: CUSTOM - we wipe the database each time we start the app - obviously, don't do this in production
                var execute = Convert.ToBoolean(ConfigurationManager.AppSettings["executeDdl"]);
                var export = Convert.ToBoolean(ConfigurationManager.AppSettings["exportDdl"]);
                var exportFile = ConfigurationManager.AppSettings["exportDdlFile"];
                if (isUnitTesting || execute) new SchemaExport(configuration).Execute(false, true, false);
                if (export)
                {
                    new SchemaExport(configuration).Create(ExportDdl, false);
                    if (!string.IsNullOrEmpty(exportFile))
                    {
                        if (File.Exists(exportFile)) File.Delete(exportFile);
                        File.WriteAllLines(exportFile, DdlLines);
                    }
                }
                //NOTE: STANDARD - we create the marshaler for automatic session management
                var marshaler = new Marshaler(configuration, typeof (ValidationInterceptor));
                //NOTE: STANDARD - we're doing this to have access to the raw NHibernate session factory in case we need it
                marshaler.OnSessionFactoryCreated += OnSessionFactoryCreated;
                UnitOfWorkProvider.Initialize(marshaler);
                _isInitialized = true;
            }
        }

        private static void ExportDdl(string text)
        {
            DdlLines.Add(text);
        }

        private static void OnSessionFactoryCreated(object sender, SessionFactoryCreatedArgs args)
        {
            NHibernate.Glimpse.Plugin.RegisterSessionFactory(args.SessionFactory);
        }

        public static void CreateSystemAccount()
        {
            var sys = new DqeUserRepository().GetSystemAccount();
            if (sys != null) return;
            sys = new DqeUser(new StaffService(), new DqeUserRepository());
            var sysTrans = sys.GetTransformer();
            sysTrans.IsActive = true;
            sysTrans.Role = DqeRole.System;
            sysTrans.District = "CO";
            sys.Transform(sysTrans, null);
            UnitOfWorkProvider.CommandRepository.Add(sys);
        }
    }
}
