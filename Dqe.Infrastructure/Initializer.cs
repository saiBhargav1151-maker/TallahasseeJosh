using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using Dqe.Domain.Model;
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
                    configuration.Configure(asm, "Dqe.Infrastructure.Configuration.dqe.hibernate.config.debug.xml");
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
                    TransportSessionFactory = transportConfiguration.BuildSessionFactory();
                    NHibernate.Glimpse.Plugin.RegisterSessionFactory(TransportSessionFactory);
                    //lre
                    var lreConfiguration = new Configuration();
                    lreConfiguration.Configure(asm, "Dqe.Infrastructure.Configuration.lre.hibernate.config.xml");
                    //LreSessionFactory = lreConfiguration.BuildSessionFactory();    
                }
#else
                configuration.Configure(asm, "Dqe.Infrastructure.Configuration.dqe.hibernate.config.release.xml");
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
                TransportSessionFactory = transportConfiguration.BuildSessionFactory();
                //lre
                var lreConfiguration = new Configuration();
                lreConfiguration.Configure(asm, "Dqe.Infrastructure.Configuration.lre.hibernate.config.xml");
                //LreSessionFactory = lreConfiguration.BuildSessionFactory();
#endif
                //NOTE: CUSTOM - add your mapping assembly
                configuration.AddResource("Dqe.Infrastructure.Mapping.CostBasedTemplate.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.CostBasedTemplateDocumentVersion.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Document.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.DqeCode.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.DqeUser.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.DqeWebLink.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.MasterFile.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.PayItem.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.PayItemStructure.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.PricingParameter.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.SystemTask.hbm.xml", asm);
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
                //report structures
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProposal.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProposalItem.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProposalSummary.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportCategory.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProject.hbm.xml", asm);
                configuration.AddResource("Dqe.Infrastructure.Mapping.Reports.ReportProjectItem.hbm.xml", asm);
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
            SessionFactory = args.SessionFactory;
            //clear any orphaned tasks
            var tasks = new SystemTaskRepository().GetAll();
            foreach (var systemTask in tasks)
            {
                UnitOfWorkProvider.CommandRepository.Remove(systemTask);
            }
            CreateSystemAccount();
            var sys = new DqeUserRepository().GetSystemAccount();
            var marketAreaRepository = new MarketAreaRepository();
            var counties = marketAreaRepository.GetAllCounties();
            var holdCounties = new List<County>();
            if (!counties.Any())
            {
                var cd = new Dictionary<string, string>
                {
                    {"26", "ALACHUA"},
                    {"27", "BAKER"},
                    {"46", "BAY"},
                    {"28", "BRADFORD"},
                    {"70", "BREVARD"},
                    {"86", "BROWARD"},
                    {"47", "CALHOUN"},
                    {"01", "CHARLOTTE"},
                    {"02", "CITRUS"},
                    {"71", "CLAY"},
                    {"03", "COLLIER"},
                    {"29", "COLUMBIA"},
                    {"04", "DESOTO"},
                    {"98", "DISTRICT WIDE"},
                    {"30", "DIXIE"},
                    {"72", "DUVAL"},
                    {"48", "ESCAMBIA"},
                    {"73", "FLAGLER"},
                    {"49", "FRANKLIN"},
                    {"50", "GADSDEN"},
                    {"31", "GILCHRIST"},
                    {"05", "GLADES"},
                    {"51", "GULF"},
                    {"32", "HAMILTON"},
                    {"06", "HARDEE"},
                    {"07", "HENDRY"},
                    {"08", "HERNANDO"},
                    {"09", "HIGHLANDS"},
                    {"10", "HILLSBOROUGH"},
                    {"52", "HOLMES"},
                    {"88", "INDIAN RIVER"},
                    {"53", "JACKSON"},
                    {"54", "JEFFERSON"},
                    {"33", "LAFAYETTE"},
                    {"11", "LAKE"},
                    {"12", "LEE"},
                    {"55", "LEON"},
                    {"34", "LEVY"},
                    {"56", "LIBERTY"},
                    {"35", "MADISON"},
                    {"13", "MANATEE"},
                    {"36", "MARION"},
                    {"89", "MARTIN"},
                    {"87", "MIAMI-DADE"},
                    {"90", "MONROE"},
                    {"74", "NASSAU"},
                    {"57", "OKALOOSA"},
                    {"91", "OKEECHOBEE"},
                    {"75", "ORANGE"},
                    {"92", "OSCEOLA"},
                    {"93", "PALM BEACH"},
                    {"14", "PASCO"},
                    {"15", "PINELLAS"},
                    {"16", "POLK"},
                    {"76", "PUTNAM"},
                    //{"96", "RAIL ENT."},
                    {"58", "SANTA ROSA"},
                    {"17", "SARASOTA"},
                    {"77", "SEMINOLE"},
                    {"78", "ST JOHNS"},
                    {"94", "ST LUCIE"},
                    {"99", "STATEWIDE"},
                    {"18", "SUMTER"},
                    {"37", "SUWANNEE"},
                    {"38", "TAYLOR"},
                    {"97", "TURNPIKE"},
                    {"39", "UNION"},
                    {"79", "VOLUSIA"},
                    {"59", "WAKULLA"},
                    {"60", "WALTON"},
                    {"61", "WASHINGTON"}
                };
                foreach (var k in cd.Keys)
                {
                    var county = new County();
                    var tcounty = county.GetTransformer();
                    tcounty.Code = k;
                    tcounty.Name = cd[k];
                    county.Transform(tcounty, sys);
                    UnitOfWorkProvider.CommandRepository.Add(county);
                    holdCounties.Add(county);
                }
            }
            var marketAreas = marketAreaRepository.GetAllMarketAreas();
            if (!marketAreas.Any())
            {
                for (var i = 0; i < 14; i++)
                {
                    var ma = new MarketArea(marketAreaRepository);
                    var tma = ma.GetTransformer();
                    tma.Name = string.Format("Area {0}", (i + 1).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));
                    ma.Transform(tma, sys);
                    UnitOfWorkProvider.CommandRepository.Add(ma);
                    switch (i)
                    {
                        case 0:
                            var c = holdCounties.First(x => x.Name == "BAY");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "ESCAMBIA");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "OKALOOSA");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "SANTA ROSA");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "WALTON");
                            ma.AddCounty(c, sys);
                            break;
                        case 1:
                            c = holdCounties.First(x => x.Name == "CALHOUN");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "FRANKLIN");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "GULF");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "HOLMES");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "JACKSON");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "LIBERTY");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "WASHINGTON");
                            ma.AddCounty(c, sys);
                            break;
                        case 2:
                            c = holdCounties.First(x => x.Name == "GADSDEN");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "JEFFERSON");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "LEON");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "WAKULLA");
                            ma.AddCounty(c, sys);
                            break;
                        case 3:
                            c = holdCounties.First(x => x.Name == "BAKER");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "BRADFORD");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "COLUMBIA");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "DIXIE");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "GILCHRIST");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "HAMILTON");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "LAFAYETTE");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "LEVY");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "MADISON");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "PUTNAM");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "SUWANNEE");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "TAYLOR");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "UNION");
                            ma.AddCounty(c, sys);
                            break;
                        case 4:
                            c = holdCounties.First(x => x.Name == "CLAY");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "DUVAL");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "NASSAU");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "ST JOHNS");
                            ma.AddCounty(c, sys);
                            break;
                        case 5:
                            c = holdCounties.First(x => x.Name == "ALACHUA");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "MARION");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "VOLUSIA");
                            ma.AddCounty(c, sys);
                            break;
                        case 6:
                            c = holdCounties.First(x => x.Name == "LAKE");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "PASCO");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "HERNANDO");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "CITRUS");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "SUMTER");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "FLAGLER");
                            ma.AddCounty(c, sys);
                            break;
                        case 7:
                            c = holdCounties.First(x => x.Name == "OSCEOLA");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "POLK");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "BREVARD");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "HILLSBOROUGH");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "ORANGE");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "PINELLAS");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "SEMINOLE");
                            ma.AddCounty(c, sys);
                            break;
                        case 8:
                            c = holdCounties.First(x => x.Name == "DESOTO");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "GLADES");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "HARDEE");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "HENDRY");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "HIGHLANDS");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "OKEECHOBEE");
                            ma.AddCounty(c, sys);
                            break;
                        case 9:
                            c = holdCounties.First(x => x.Name == "CHARLOTTE");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "COLLIER");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "LEE");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "MANATEE");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "SARASOTA");
                            ma.AddCounty(c, sys);
                            break;
                        case 10:
                            c = holdCounties.First(x => x.Name == "INDIAN RIVER");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "MARTIN");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "ST LUCIE");
                            ma.AddCounty(c, sys);
                            break;
                        case 11:
                            c = holdCounties.First(x => x.Name == "BROWARD");
                            ma.AddCounty(c, sys);
                            c = holdCounties.First(x => x.Name == "PALM BEACH");
                            ma.AddCounty(c, sys);
                            break;
                        case 12:
                            c = holdCounties.First(x => x.Name == "MIAMI-DADE");
                            ma.AddCounty(c, sys);
                            break;
                        case 13:
                            c = holdCounties.First(x => x.Name == "MONROE");
                            ma.AddCounty(c, sys);
                            break;
                    }
                }
            }
            //cost groups
            var costGroupRepository = new CostGroupRepository();
            var costGroups = costGroupRepository.GetAll();
            if (!costGroups.Any())
            {
                var cg = new CostGroup();
                var cgt = cg.GetTransformer();
                cgt.Name = "CEMENT";
                cgt.Description = "CEMENT ITEMS";
                cgt.Unit = "CY";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "FUEL";
                cgt.Description = "FUEL ITEMS";
                cgt.Unit = "SY";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PRETAPE";
                cgt.Description = "PREFORMED TAPE";
                cgt.Unit = "GM";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PT_BITME";
                cgt.Description = "BITUMINOUS CONCRETE - ENGLISH (PRICE TRENDS)";
                cgt.Unit = "TN";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PT_BITMM";
                cgt.Description = "BITUMINOUS CONCRETE - METRIC (PRICE TRENDS)";
                cgt.Unit = "MT";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PT_ERTHE";
                cgt.Description = "EARTHWORK - ENGLISH (PRICE TRENDS)";
                cgt.Unit = "CY";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PT_ERTHM";
                cgt.Description = "EARTHWORK - METRIC ( PRICE TRENDS)";
                cgt.Unit = "M3";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PT_PORTE";
                cgt.Description = "PORTLAND CEMENT - ENGLISH (PRICE TRENDS)";
                cgt.Unit = "SY";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PT_PORTM";
                cgt.Description = "PORTLAND CEMENT - METRIC (PRICE TRENDS)";
                cgt.Unit = "M2";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PT_RENFE";
                cgt.Description = "REINFORCING STEEL -ENGLISH (PRICE TRENDS)";
                cgt.Unit = "LB";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PT_RENFM";
                cgt.Description = "REINFORCING STEEL - METRIC (PRICE TRENDS)";
                cgt.Unit = "KG";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PT_STCCE";
                cgt.Description = "STRUCTURAL CONCRETE - ENGLISH ( PRICE TRENDS)";
                cgt.Unit = "CY";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PT_STCCM";
                cgt.Description = "STRUCTURAL CONCRETE - METRIC (PRICE TRENDS)";
                cgt.Unit = "M3";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PT_STSTE";
                cgt.Description = "STRUCTURAL STEEL - ENGLISH (PRICE TRENDS)";
                cgt.Unit = "LB";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "PT_STSTM";
                cgt.Description = "STRUCTURAL STEEL - METRIC (PRICE TRENDS)";
                cgt.Unit = "KG";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "SAFETY";
                cgt.Description = "SAFETY PAY ITEMS";
                cgt.Unit = "SF";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "SIDEWALK";
                cgt.Description = "SIDEWALK ITEMS";
                cgt.Unit = "SY";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "STEEL";
                cgt.Description = "STEEL ITEMS";
                cgt.Unit = "LB";
                cg.Transform(cgt, sys);
                UnitOfWorkProvider.CommandRepository.Add(cg);
                cg = new CostGroup();
                cgt = cg.GetTransformer();
                cgt.Name = "STRIPING";
                cgt.Description = "STRIPING ITEMS - PAINT / THERMO";
                cgt.Unit = "NM";
                cg.Transform(cgt, sys);
            }
            var vpn = Convert.ToBoolean(ConfigurationManager.AppSettings["vpn"]);
            if (!vpn) return;
            var masterFileRepository = new MasterFileRepository();
            var masterFiles = masterFileRepository.GetAll().ToList();
            var loadWtPayItems = Convert.ToBoolean(ConfigurationManager.AppSettings["loadWtPayItems"]);
            if (!loadWtPayItems)
            {
                if (!masterFiles.Any())
                {
                    var wtMasterFiles = new WebTransportService().GetMasterFiles();
                    foreach (var masterfile in wtMasterFiles)
                    {
                        var mf = new MasterFile(masterFileRepository);
                        var mft = mf.GetTransformer();
                        mft.FileNumber = int.Parse(masterfile);
                        mf.Transform(mft, sys);
                        UnitOfWorkProvider.CommandRepository.Add(mf);    
                    }
                }
                return;
            }
            //data initialization from WT
            if (!masterFiles.Any())
            {
                
                var mf = new MasterFile(masterFileRepository);
                var mft = mf.GetTransformer();
                mft.FileNumber = 10;
                mf.Transform(mft, sys);
                UnitOfWorkProvider.CommandRepository.Add(mf);
                var payItemStructureRepository = new PayItemStructureRepository();
                var payItemRepository = new PayItemRepository();
                var webTransportService = new WebTransportService();
                var refItems = webTransportService.GetRefItems();
                PayItemStructure structure = null;
                var i = 0;
                foreach (var refItem in refItems)
                {
                    if (i > 700) break;
                    //parse item number
                    if (refItem.Name.Length == 10)
                    {
                        i += 1;
                        var part1 = refItem.Name.Substring(0, 4);
                        var part2 = refItem.Name.Substring(4, 3);
                        var part3 = refItem.Name.Substring(7, 3);
                        if (structure == null || structure.StructureId.Trim() != string.Format("{0}-{1}", part1, part2))
                        {
                            structure = payItemStructureRepository.GetByStructureId(string.Format("{0}-{1}", part1, part2), null);    
                        }
                        if (structure == null)
                        {
                            structure = new PayItemStructure(payItemStructureRepository);
                            var st = structure.GetTransformer();
                            st.EffectiveDate = new DateTime(2000, 1, 1);
                            st.PrimaryUnit = PrimaryUnitType.Mixed;
                            st.Title = string.Format("Test Structure {0}-{1}", part1, part2);
                            st.StructureId = string.Format("{0}-{1}", part1, part2);
                            st.Accuracy = 1;
                            structure.Transform(st, sys);
                            UnitOfWorkProvider.CommandRepository.Add(structure);
                        }
                        var item = new PayItem(payItemRepository);
                        var it = item.GetTransformer();
                        it.PayItemId = string.Format("{0}-{1}-{2}", part1, part2, part3);
                        if (refItem.LumpSum)
                        {
                            it.PrimaryUnit = PrimaryUnitType.LS;
                        }
                        PrimaryUnitType pRes;
                        if (Enum.TryParse(refItem.Unit, true, out pRes))
                        {
                            if (it.PrimaryUnit == PrimaryUnitType.LS)
                            {
                                SecondaryUnitType sRes;
                                it.SecondaryUnit = Enum.TryParse(refItem.Unit, true, out sRes) ? sRes : SecondaryUnitType.LS;
                            }
                            else
                            {
                                it.PrimaryUnit = pRes;
                                if (it.PrimaryUnit == PrimaryUnitType.LS)
                                {
                                    SecondaryUnitType sRes;
                                    it.SecondaryUnit = Enum.TryParse(refItem.Unit, true, out sRes) ? sRes : SecondaryUnitType.LS;
                                }
                            }
                        }
                        else
                        {
                            it.PrimaryUnit = PrimaryUnitType.AC;
                        }
                        it.EffectiveDate = refItem.IlDate1;
                        it.ObsoleteDate = refItem.ObsoleteDate;
                        it.Description = refItem.Description;
                        it.ShortDescription = refItem.ShortDescription;
                        it.DqeReferencePrice = refItem.Price.HasValue ? refItem.Price.Value : 0;
                        item.AssociatePayItemToStructureAndMasterFile(structure, mf);
                        item.Transform(it, sys);
                        UnitOfWorkProvider.CommandRepository.Add(item);
                    }
                }
            }
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
