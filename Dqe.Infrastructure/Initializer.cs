using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Dqe.Domain.Model;
using Dqe.Infrastructure.Driver;
using Dqe.Infrastructure.Fdot;
using Dqe.Infrastructure.Repositories.Custom;
using Dqe.Infrastructure.Services;
using NHibernate;
using NHibernate.DataAnnotations;
using NHibernate.Session;
using NHibernate.Tool.hbm2ddl;
using Dqe.Infrastructure.EntityIoC;
using Dqe.Infrastructure.Providers;
using Configuration = NHibernate.Cfg.Configuration;

//[assembly: InternalsVisibleTo("Dqe.Automation.PayItemProcessing", AllInternalsVisible = true)]

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

        internal static ISessionFactory TransportSessionFactory;

        internal static ISessionFactory LreSessionFactory;

        internal static ISessionFactory SessionFactory;

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
                if (isUnitTesting)
                {
                    configuration.Configure();
                }
                else
                {
                    configuration.Configure(asm, "Dqe.Infrastructure.Configuration.dqe.hibernate.config.xml");
                }
                var vpn = Convert.ToBoolean(ConfigurationManager.AppSettings["vpn"]);
                if (vpn)
                {
                    //wt
                    var transportConfiguration = new Configuration();
                    transportConfiguration.Configure(asm, "Dqe.Infrastructure.Configuration.wt.hibernate.config.xml");
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.CodeTable.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.CodeValue.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.RefItem.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Project.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Proposal.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.ProposalItem.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.ProjectItem.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Section.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Category.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.RefCounty.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.RefDistrict.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.District.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.County.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.CategoryAlternate.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Alternate.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Letting.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.FundPackage.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.ProposalVendor.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Bid.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.BidTime.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Milestone.hbm.xml", asm);
                    transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.RefVendor.hbm.xml", asm);
                    TransportSessionFactory = transportConfiguration.BuildSessionFactory();
                    NHibernate.Glimpse.Plugin.RegisterSessionFactory(TransportSessionFactory);
                    //lre
                    var lreConfiguration = new Configuration();
                    lreConfiguration.Configure(asm, "Dqe.Infrastructure.Configuration.lre.hibernate.config.xml");
                    lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.Project.hbm.xml", asm);
                    lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.ProjectSnapshot.hbm.xml", asm);
                    lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.VersionSnapshot.hbm.xml", asm);
                    lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.PayItem.hbm.xml", asm);
                    lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.PayItemGroup.hbm.xml", asm);
                    lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.PayItemPayItemGroup.hbm.xml", asm);
                    lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.PayItemCounty.hbm.xml", asm);
                    lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.PayItemDistrict.hbm.xml", asm);
                    lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.PayItemMarketArea.hbm.xml", asm);
                    lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.MarketArea.hbm.xml", asm);
                    LreSessionFactory = lreConfiguration.BuildSessionFactory();
                }
#else
                configuration.Configure(asm, "Dqe.Infrastructure.Configuration.dqe.hibernate.config.xml");
                //wt
                var transportConfiguration = new Configuration();
                transportConfiguration.Configure(asm, "Dqe.Infrastructure.Configuration.wt.hibernate.config.xml");
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.CodeTable.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.CodeValue.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.RefItem.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Project.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Proposal.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.ProposalItem.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.ProjectItem.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Section.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Category.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.RefCounty.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.RefDistrict.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.District.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.County.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.CategoryAlternate.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Alternate.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Letting.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.FundPackage.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.ProposalVendor.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Bid.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.BidTime.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.Milestone.hbm.xml", asm);
                transportConfiguration.AddResource("Dqe.Infrastructure.Mapping.Wt.RefVendor.hbm.xml", asm);
                TransportSessionFactory = transportConfiguration.BuildSessionFactory();
                //lre
                var lreConfiguration = new Configuration();
                lreConfiguration.Configure(asm, "Dqe.Infrastructure.Configuration.lre.hibernate.config.xml");
                lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.Project.hbm.xml", asm);
                lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.ProjectSnapshot.hbm.xml", asm);
                lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.VersionSnapshot.hbm.xml", asm);
                lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.PayItem.hbm.xml", asm);
                lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.PayItemGroup.hbm.xml", asm);
                lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.PayItemPayItemGroup.hbm.xml", asm);
                lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.PayItemCounty.hbm.xml", asm);
                lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.PayItemDistrict.hbm.xml", asm);
                lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.PayItemMarketArea.hbm.xml", asm);
                lreConfiguration.AddResource("Dqe.Infrastructure.Mapping.Lre.MarketArea.hbm.xml", asm);
                LreSessionFactory = lreConfiguration.BuildSessionFactory();
#endif
                //NOTE: CUSTOM - add your mapping assembly
                configuration.AddResource("Dqe.Infrastructure.Mapping.CostBasedTemplate.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.CostBasedTemplateDocumentVersion.hbm.xml", asm);
                //configuration.AddResource("Dqe.Infrastructure.Mapping.Document.hbm.xml", asm);
                //configuration.AddResource("Dqe.Infrastructure.Mapping.DqeCode.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.DqeUser.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.DqeWebLink.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.MasterFile.hbm.xml", asm);
                //configuration.AddResource("Dqe.Infrastructure.Mapping.PayItem.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.PayItemStructure.hbm.xml", asm);
                //configuration.AddResource("Dqe.Infrastructure.Mapping.PricingParameter.hbm.xml", asm);
                //configuration.AddResource("Dqe.Infrastructure.Mapping.SystemTask.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Project.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.ProjectVersion.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.ProjectEstimate.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.EstimateGroup.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.ProjectItem.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.MarketArea.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.County.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Proposal.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.SectionGroup.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.ProposalItem.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.ProposalHistory.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.BidHistory.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.PayItemMaster.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.AveragePrice.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.CostGroupPayItem.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.CostGroup.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.SystemParameters.hbm.xml", asm);
                //report structures
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportLetting.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportLettingSummary.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportMilestoneBid.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProposal.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProposalItem.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProposalSummary.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProposalMilestone.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProposalVendor.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportVendorBid.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportCategory.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProject.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProjectItem.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProjectSummary.hbm.xml", asm);
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
                SqlReviewHelper.Current = new SqlReviewHelper();
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
            SessionFactory = args.SessionFactory;
        }

        public static void CreateSystemAccount()
        {
            var sys = new DqeUserRepository().GetSystemAccount();
            if (sys != null) return;
            sys = new DqeUser(new StaffService(), new DqeUserRepository(), new ProposalRepository(), new ProjectRepository());
            var sysTrans = sys.GetTransformer();
            sysTrans.IsActive = true;
            sysTrans.Role = DqeRole.System;
            sysTrans.District = "CO";
            sys.Transform(sysTrans, null);
            UnitOfWorkProvider.CommandRepository.Add(sys);
        }
    }
}
