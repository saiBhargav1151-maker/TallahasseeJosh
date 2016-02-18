using System.Linq;
using Dqe.Domain.Model;
using Dqe.Infrastructure;
using Dqe.Infrastructure.EntityIoC;
using Dqe.Infrastructure.Fdot;
using Dqe.Infrastructure.Providers;
using Dqe.Infrastructure.Repositories.Custom;
using Dqe.Infrastructure.Services;

namespace Dqe.Automation.PayItemConversion
{
    class Program
    {
        static void Main()
        {
            Initializer.Initialize();
            EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
            var wTservice = new WebTransportService();
            wTservice.ConvertPayItems();
            //var sys = new DqeUserRepository().GetSystemAccount();
            //var payItemNumbers = wTservice.GetDistinctRefItemNumbers();
            //var x = 0;
            //foreach (var payItemNumber in payItemNumbers)
            //{
            //    if (x > 1000) break;
            //    //logic
            //    var items = wTservice.GetRefItems(payItemNumber).ToList();
            //    var specBooks = items.Select(i => i.SpecBook).ToList();
            //    for (var i = 0; i < specBooks.Count(); i++)
            //    {
            //        if (specBooks[i].StartsWith("9"))
            //        {
            //            specBooks[i] = string.Format("0{0}", specBooks[i]);
            //        }
            //        else
            //        {
            //            specBooks[i] = string.Format("1{0}", specBooks[i]);
            //        }
            //    }
            //    specBooks = specBooks.OrderByDescending(i => i).ToList();
            //    var maxSpecBook = specBooks.First().Substring(1);
            //    var item = items.First(i => i.SpecBook == maxSpecBook);
            //    var pim = new PayItemMaster();
            //    var pimt = pim.GetTransformer();
            //    pimt.Administrative = item.Administrative;
            //    pimt.AlternateItemName = item.AlternateItemName;
            //    pimt.AutoPaidPercentSchedule = item.AutoPaidPercentSchedule;
            //    pimt.BidAsLumpSum = item.BidAsLumpSum;
            //    pimt.BidRequirementCode = item.BidRequirementCode;
            //    pimt.CalculatedUnit = item.CalculatedUnit;
            //    pimt.CoApprovalRequired = item.CoApprovalRequired;
            //    pimt.CombineWithLikeItems = item.CombineWithLikeItems;
            //    pimt.CommonUnit = item.CommonUnit;
            //    pimt.ContractClass = item.ContractClass;
            //    pimt.ConversionFactorToCommonUnit = item.ConversionFactorToCommonUnits;
            //    pimt.CreatedBy = item.CreatedBy;
            //    pimt.CreatedDate = item.CreatedDate;
            //    pimt.DbeInterest = item.DbeInterest;
            //    pimt.DbePercentToApply = item.DbePercentToApply;
            //    pimt.Description = item.Description;
            //    pimt.ExemptFromMaa = item.ExemptFromMaa;
            //    pimt.ExemptFromRetainage = item.ExemptFromRetainage;
            //    pimt.FuelAdjustment = item.FuelAdjustment;
            //    pimt.FuelAdjustmentType = item.FuelAdjustmentType;
            //    pimt.Ildt1 = item.IlDate1;
            //    pimt.Ildt2 = item.IlDate2;
            //    pimt.Ildt3 = item.IlDate3;
            //    pimt.Ilflg1 = item.IlFlag1;
            //    pimt.Illst1 = item.Illst1;
            //    pimt.Ilnum1 = item.IlNumber1;
            //    pimt.Ilsst1 = item.Ilsst1;
            //    pimt.ItemClass = item.ItemClass;
            //    pimt.ItemType = item.ItemType;
            //    pimt.LastUpdatedBy = item.LastUpdatedBy;
            //    pimt.LastUpdatedDate = item.LastUpdatedDate;
            //    pimt.LumpSum = item.LumpSum;
            //    pimt.MajorItem = item.MajorItem;
            //    pimt.NonBid = item.NonBid;
            //    pimt.ObsoleteDate = item.ObsoleteDate;
            //    pimt.PayPlan = item.PayPlan;
            //    pimt.PercentScheduleItem = item.PercentScheduleItem;
            //    pimt.RecordSource = item.RecordSource;
            //    pimt.RefItemName = item.Name;
            //    pimt.RefPrice = item.Price;
            //    pimt.RegressionInclusion = item.RegressionInclusion;
            //    pimt.ShortDescription = item.ShortDescription;
            //    pimt.SpecBook = item.SpecBook;
            //    pimt.SpecialtyItem = item.SpecialtyItem;
            //    pimt.SuppDescriptionRequired = item.SuppDescriptionRequired;
            //    pimt.Unit = item.Unit;
            //    pimt.UnitSystem = item.UnitSystem;
            //    pim.Transform(pimt, sys);
            //    UnitOfWorkProvider.CommandRepository.Add(pim);
            //    x += 1;
            //}
            //UnitOfWorkProvider.TransactionManager.Commit();
        }

        private static object[] EntityDependencyResolverOnResolveConstructorArguments(object sender, ResolveConstructorArgumentsArgs args)
        {
            if (args.EntityType.IsAssignableFrom(typeof(DqeUser))) return new object[] {  new StaffService(), new DqeUserRepository(), new ProposalRepository(), new ProjectRepository() };
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
