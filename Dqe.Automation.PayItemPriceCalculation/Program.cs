using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Infrastructure;
using Dqe.Infrastructure.EntityIoC;
using Dqe.Infrastructure.Fdot;
using Dqe.Infrastructure.Providers;
using Dqe.Infrastructure.Repositories.Custom;
using Dqe.Infrastructure.Services;
using BidHistory = Dqe.ApplicationServices.BidHistory;

namespace Dqe.Automation.PayItemPriceCalculation
{
    class Program
    {
        static void Main()
        {
            Initializer.Initialize();
            EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
            var sys = new DqeUserRepository().GetSystemAccount();
            var pimr = new PayItemMasterRepository();
            Console.WriteLine("Resetting Average Prices and Bid History");
            pimr.ResetAveragePricesAndClearBidHistory();
            Console.WriteLine("Querying Pay Items");
            var mfl = new MasterFileRepository().GetAll().ToList();
            //temp
            //var allItems = pimr.GetAll("*");
            //foreach (var item in allItems)
            //{
            //    var specBook = item.SpecBook;
            //    foreach (var masterFile in mfl)
            //    {
            //        if (specBook != masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'))
            //        {
            //            var pim = new PayItemMaster();
            //            var pimt = pim.GetTransformer();
            //            pimt.Administrative = item.Administrative;
            //            pimt.AlternateItemName = item.AlternateItemName;
            //            pimt.AutoPaidPercentSchedule = item.AutoPaidPercentSchedule;
            //            pimt.BidAsLumpSum = item.BidAsLumpSum;
            //            pimt.BidRequirementCode = item.BidRequirementCode;
            //            pimt.CalculatedUnit = item.CalculatedUnit;
            //            pimt.CoApprovalRequired = item.CoApprovalRequired;
            //            pimt.CombineWithLikeItems = item.CombineWithLikeItems;
            //            pimt.CommonUnit = item.CommonUnit;
            //            pimt.ContractClass = item.ContractClass;
            //            pimt.ConversionFactorToCommonUnit = item.ConversionFactorToCommonUnit;
            //            pimt.CreatedBy = item.CreatedBy;
            //            pimt.CreatedDate = item.CreatedDate;
            //            pimt.DbeInterest = item.DbeInterest;
            //            pimt.DbePercentToApply = item.DbePercentToApply;
            //            pimt.Description = item.Description;
            //            pimt.ExemptFromMaa = item.ExemptFromMaa;
            //            pimt.ExemptFromRetainage = item.ExemptFromRetainage;
            //            pimt.FuelAdjustment = item.FuelAdjustment;
            //            pimt.FuelAdjustmentType = item.FuelAdjustmentType;
            //            pimt.Ildt2 = item.Ildt2;
            //            pimt.OpenedDate = item.OpenedDate;
            //            pimt.Ilflg1 = item.Ilflg1;
            //            pimt.Illst1 = item.Illst1;
            //            pimt.Ilnum1 = item.Ilnum1;
            //            pimt.Ilsst1 = item.Ilsst1;
            //            pimt.ItemClass = item.ItemClass;
            //            pimt.ItemType = item.ItemType;
            //            pimt.LastUpdatedBy = item.LastUpdatedBy;
            //            pimt.LastUpdatedDate = item.LastUpdatedDate;
            //            pimt.LumpSum = item.LumpSum;
            //            pimt.MajorItem = item.MajorItem;
            //            pimt.NonBid = item.NonBid;
            //            pimt.ObsoleteDate = item.ObsoleteDate;
            //            pimt.PayPlan = item.PayPlan;
            //            pimt.PercentScheduleItem = item.PercentScheduleItem;
            //            pimt.RecordSource = item.RecordSource;
            //            pimt.RefItemName = item.RefItemName;
            //            pimt.RefPrice = item.RefPrice;
            //            pimt.RegressionInclusion = item.RegressionInclusion;
            //            pimt.ShortDescription = item.ShortDescription;
            //            pimt.SpecBook = masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
            //            pimt.SpecialtyItem = item.SpecialtyItem;
            //            pimt.SuppDescriptionRequired = item.SuppDescriptionRequired;
            //            pimt.Unit = item.Unit;
            //            pimt.UnitSystem = item.UnitSystem;
            //            pim.Transform(pimt, sys);
            //            UnitOfWorkProvider.CommandRepository.Add(pim);
            //        }
            //    }
            //}
            //end temp
            var syl = mfl.Select(masterFile => masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).StartsWith("9") 
                ? string.Format("0{0}", masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')) 
                : string.Format("1{0}", masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'))).ToList();
            var sy = syl.OrderByDescending(i => i).First();
            var items = pimr.GetAll(sy.Substring(1));
            var wTservice = new WebTransportService();
            Console.WriteLine("Querying Bid History");
            var allHistory = ((IEnumerable<BidHistory>) wTservice.GetAllBidHistory(48)).ToList();
            var mar = new MarketAreaRepository();
            var mal = mar.GetAllMarketAreas().ToList();
            var counties = mar.GetAllCounties().ToList();
            var engine = new PricingEngine(mar);
            Console.WriteLine("Processing item's average prices and bid history");
            foreach (var item in items)
            {
                try
                {
                    var bidHistory = allHistory.FirstOrDefault(i => i.ItemName == item.RefItemName);
                    if (bidHistory == null) continue;
                    var deriveLsPrice = item.CalculatedUnit.ToUpper().StartsWith("LS") && !item.Unit.ToUpper().StartsWith("LS");
                    if (deriveLsPrice)
                    {
                        foreach (var proposal in bidHistory.Proposals)
                        {
                            foreach (var bid in proposal.Bids)
                            {
                                bid.Price = bid.Price/bid.Quantity;
                            }
                        }
                    }
                    var bidSet = engine.CalculateStateAveragePrice(bidHistory, item, sys);
                    var tItem = item.GetTransformer();
                    tItem.StateReferencePrice = Math.Round((bidSet.QuantityWeightedAveragePrice + bidSet.TimeWeightedAveragePrice) /2, 2);

                    //todo: check
                    if (tItem.StateReferencePrice != 0)
                    {
                        tItem.RefPrice = 0;
                    }

                    item.Transform(tItem, sys);
                    foreach (var ma in mal)
                    {
                        bidSet = engine.CalculateMarketAreaAveragePrice(bidHistory, ma);
                        var maap = new MarketAreaAveragePrice();
                        var tMaap = maap.GetTransformer();
                        tMaap.Price = Math.Round((bidSet.QuantityWeightedAveragePrice + bidSet.TimeWeightedAveragePrice) / 2, 2);
                        maap.Transform(tMaap, sys);
                        ma.AddItemAveragePrice(item, maap);
                    }
                    foreach (var county in counties)
                    {
                        bidSet = engine.CalculateCountyAveragePrice(bidHistory, county);
                        var cap = new CountyAveragePrice();
                        var tCap = cap.GetTransformer();
                        tCap.Price = Math.Round((bidSet.QuantityWeightedAveragePrice + bidSet.TimeWeightedAveragePrice) / 2, 2);
                        cap.Transform(tCap, sys);
                        county.AddItemAveragePrice(item, cap);
                    }
                }
                catch(Exception exception)
                {
                    Console.WriteLine("Failed to query bid history for {0} : {1}", item.RefItemName, exception.Message);
                }
            }
            Console.WriteLine("Committing");
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
