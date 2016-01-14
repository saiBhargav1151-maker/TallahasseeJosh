using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Infrastructure;
using Dqe.Infrastructure.EntityIoC;
using Dqe.Infrastructure.Fdot;
using Dqe.Infrastructure.Providers;
using Dqe.Infrastructure.Repositories.Custom;
using Dqe.Infrastructure.Services;

namespace Dqe.Automation.InitializationConversion
{
    class Program
    {
        static void Main()
        {
            Initializer.Initialize();
            EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
            var parms = new SystemParameters { Id = 1, LoadPrices = true };
            UnitOfWorkProvider.CommandRepository.Add(parms);
            Initializer.CreateSystemAccount();
            var sys = new DqeUserRepository().GetSystemAccount();
            //add admin users
            var u = new DqeUser(new StaffService(), new DqeUserRepository(), new ProposalRepository(), new ProjectRepository());
            var t = u.GetTransformer();
            t.IsActive = true;
            t.Role = DqeRole.Administrator;
            t.SrsId = 776;
            t.CostGroupAuthorization = "U";
            t.District = "CO";
            u.Transform(t, sys);
            UnitOfWorkProvider.CommandRepository.Add(u);
            u = new DqeUser(new StaffService(), new DqeUserRepository(), new ProposalRepository(), new ProjectRepository());
            t = u.GetTransformer();
            t.IsActive = true;
            t.Role = DqeRole.Administrator;
            t.SrsId = 3837;
            t.CostGroupAuthorization = "U";
            t.District = "CO";
            u.Transform(t, sys);
            UnitOfWorkProvider.CommandRepository.Add(u);
            u = new DqeUser(new StaffService(), new DqeUserRepository(), new ProposalRepository(), new ProjectRepository());
            t = u.GetTransformer();
            t.IsActive = true;
            t.Role = DqeRole.Administrator;
            t.SrsId = 39864;
            t.CostGroupAuthorization = "U";
            t.District = "CO";
            u.Transform(t, sys);
            UnitOfWorkProvider.CommandRepository.Add(u);
            u = new DqeUser(new StaffService(), new DqeUserRepository(), new ProposalRepository(), new ProjectRepository());
            t = u.GetTransformer();
            t.IsActive = true;
            t.Role = DqeRole.Administrator;
            t.SrsId = 4176;
            t.CostGroupAuthorization = "U";
            t.District = "CO";
            u.Transform(t, sys);
            UnitOfWorkProvider.CommandRepository.Add(u);
            u = new DqeUser(new StaffService(), new DqeUserRepository(), new ProposalRepository(), new ProjectRepository());
            t = u.GetTransformer();
            t.IsActive = true;
            t.Role = DqeRole.Administrator;
            t.SrsId = 3580;
            t.CostGroupAuthorization = "U";
            t.District = "CO";
            u.Transform(t, sys);
            UnitOfWorkProvider.CommandRepository.Add(u);
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
                    {"99", "DIST/ST-WIDE"},
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
            var masterFileRepository = new MasterFileRepository();
            var wtMasterFiles = new WebTransportService().GetMasterFiles();
            foreach (var masterfile in wtMasterFiles)
            {
                var mf = new MasterFile(masterFileRepository);
                var mft = mf.GetTransformer();
                mft.FileNumber = int.Parse(masterfile);
                mf.Transform(mft, sys);
                UnitOfWorkProvider.CommandRepository.Add(mf);
            }
            UnitOfWorkProvider.TransactionManager.Commit();
        }

        

        private static object[] EntityDependencyResolverOnResolveConstructorArguments(object sender, ResolveConstructorArgumentsArgs args)
        {
            if (args.EntityType.IsAssignableFrom(typeof(DqeUser))) return new object[] { new StaffService(), new DqeUserRepository(), new ProposalRepository(), new ProjectRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(PayItemStructure))) return new object[] { new PayItemStructureRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(MasterFile))) return new object[] { new MasterFileRepository() };
            //if (args.EntityType.IsAssignableFrom(typeof(PayItem))) return new object[] { new PayItemRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(Project))) return new object[] { new ProjectRepository(), UnitOfWorkProvider.CommandRepository, new WebTransportService() };
            if (args.EntityType.IsAssignableFrom(typeof(MarketArea))) return new object[] { new MarketAreaRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(Proposal))) return new object[] { new ProposalRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(ProjectEstimate))) return new object[] { new WebTransportService() };
            return null;
        }
    }
}
