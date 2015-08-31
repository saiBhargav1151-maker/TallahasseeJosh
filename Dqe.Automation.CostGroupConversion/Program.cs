using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Dqe.Domain.Model;
using Dqe.Infrastructure;
using Dqe.Infrastructure.EntityIoC;
using Dqe.Infrastructure.Fdot;
using Dqe.Infrastructure.Providers;
using Dqe.Infrastructure.Repositories.Custom;
using Dqe.Infrastructure.Services;

namespace Dqe.Automation.CostGroupConversion
{
    class Program
    {
        static void Main()
        {
            Initializer.Initialize();
            EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
            var sys = new DqeUserRepository().GetSystemAccount();
            var pimr = new PayItemMasterRepository();
            var cgr = new CostGroupRepository();
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "Dqe.Automation.CostGroupConversion.CostGroups.dat";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    var costGroups = cgr.GetAll().ToList();
                    var mfl = new MasterFileRepository().GetAll();
                    var syl = mfl.Select(masterFile => masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).StartsWith("9")
                        ? string.Format("0{0}", masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'))
                        : string.Format("1{0}", masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'))).ToList();
                    var sy = syl.OrderByDescending(i => i).First();
                    //var 0items = pimr.GetAll(sy.Substring(1)).OrderBy(i => i.RefItemName).ToList();

                    var items = pimr.GetAll("*").OrderBy(i => i.RefItemName).ToList();

                    Console.WriteLine("Processing conversion file.");
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (line == null) continue;
                            var array = line.Replace('�', ' ').Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            var cgName = array[0].Trim();
                            var specYear = array[1].Trim();
                            var piName = array[2].Trim();
                            var conversionFactor = Convert.ToDecimal(array[3]);
                            var cg = costGroups.FirstOrDefault(i => i.Name == cgName);
                            var item = items.FirstOrDefault(i => i.RefItemName == piName && i.SpecBook == specYear);
                            if (cg == null || item == null) continue;
                            var cgpi = new CostGroupPayItem();
                            var cgpit = cgpi.GetTransformer();
                            cgpit.ConversionFactor = conversionFactor;
                            cgpi.Transform(cgpit, sys);
                            item.AddCostGroup(cgpi);
                            cg.AddPayItem(cgpi);
                            Console.WriteLine("Item added.");
                        }
                    }
                    UnitOfWorkProvider.TransactionManager.Commit();
                }
            }
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
