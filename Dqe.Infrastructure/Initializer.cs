using System;
using System.Collections.Generic;
using System.Configuration;
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
                    //{"98", "DISTRICT WIDE"},
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
                    //{"99", "STATEWIDE"},
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
                }
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
