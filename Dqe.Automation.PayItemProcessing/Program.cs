using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Infrastructure;
using Dqe.Infrastructure.Driver;
using Dqe.Infrastructure.EntityIoC;
using Dqe.Infrastructure.Fdot;
using Dqe.Infrastructure.Providers;
using Dqe.Infrastructure.Repositories.Custom;
using Dqe.Infrastructure.Services;
using BidHistory = Dqe.ApplicationServices.BidHistory;

namespace Dqe.Automation.PayItemProcessing
{
    class Program
    {
        static void Main()
        {
            try
            {
                Initializer.Initialize();
                EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
                var sys = new DqeUserRepository().GetSystemAccount();
                var pimr = new PayItemMasterRepository();
                //TODO: copy master file
                //price calculation
                Console.WriteLine("Starting Step 1 - {0}", DateTime.Now);
                Console.WriteLine("Resetting Average Prices and Bid History");
                pimr.ResetAveragePricesAndClearBidHistory();
                Console.WriteLine("Starting Step 2 - {0}", DateTime.Now);
                Console.WriteLine("Querying Pay Items");
                var mfl = new MasterFileRepository().GetAll().ToList();
                var syl = mfl.Select(masterFile => masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).StartsWith("9") 
                    ? string.Format("0{0}", masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')) 
                    : string.Format("1{0}", masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'))).ToList();
                var sy = syl.OrderByDescending(i => i).First();
                var wTservice = new WebTransportService();
                Console.WriteLine("Starting Step 3 - {0}", DateTime.Now);
                Console.WriteLine("Querying Bid History");
                var allHistory = ((IEnumerable<BidHistory>)wTservice.GetAllBidHistory(36)).ToList();
                var mar = new MarketAreaRepository();
                var mal = mar.GetAllMarketAreas().ToList();
                var counties = mar.GetAllCounties().ToList();
                var engine = new PricingEngine(mar);
                Console.WriteLine("Starting Step 4 - {0}", DateTime.Now);
                Console.WriteLine("Processing item's average prices and bid history");
                var itemCount = pimr.GetAllCount(sy.Substring(1));
                var processed = 0;
                while(processed < itemCount)
                {
                    var items = pimr.GetAllRanged(sy.Substring(1), processed, Math.Min(20, itemCount - processed));
                    foreach (var item in items)
                    {
                        processed += 1;
                        var bidHistory = allHistory.FirstOrDefault(i => i.ItemName == item.RefItemName);
                        if (bidHistory == null) continue;
                        var deriveLsPrice = item.CalculatedUnit.ToUpper().StartsWith("LS") && !item.Unit.ToUpper().StartsWith("LS");
                        if (deriveLsPrice)
                        {
                            foreach (var proposal in bidHistory.Proposals)
                            {
                                foreach (var bid in proposal.Bids)
                                {
                                    bid.Price = bid.Price / bid.Quantity;
                                }
                            }
                        }
                        var bidSet = engine.CalculateStateAveragePrice(bidHistory, item, sys);
                        var tItem = item.GetTransformer();
                        tItem.StateReferencePrice = Math.Round((bidSet.QuantityWeightedAveragePrice + bidSet.TimeWeightedAveragePrice) / 2, 2);

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
                            maap.MyMarketArea = ma;
                            maap.MyPayItemMaster = item;
                            UnitOfWorkProvider.CommandRepository.Add(maap);
                            //ma.AddItemAveragePrice(item, maap);
                        }
                        foreach (var county in counties)
                        {
                            bidSet = engine.CalculateCountyAveragePrice(bidHistory, county);
                            var cap = new CountyAveragePrice();
                            var tCap = cap.GetTransformer();
                            tCap.Price = Math.Round((bidSet.QuantityWeightedAveragePrice + bidSet.TimeWeightedAveragePrice) / 2, 2);
                            cap.Transform(tCap, sys);
                            cap.MyCounty = county;
                            cap.MyPayItemMaster = item;
                            UnitOfWorkProvider.CommandRepository.Add(cap);
                            //county.AddItemAveragePrice(item, cap);
                        }
                    }
                    Console.WriteLine("Flushing... {0} of {1} @ {2}", processed, itemCount, DateTime.Now);
                    UnitOfWorkProvider.CommandRepository.Flush();
                    UnitOfWorkProvider.CommandRepository.Clear();
                }
                Console.WriteLine("Starting Step 5 - {0}", DateTime.Now);
                Console.WriteLine("Committing");
                UnitOfWorkProvider.TransactionManager.Commit();
                //if (SqlReviewHelper.Current != null)
                //{
                //    SqlReviewHelper.Current.Dump();    
                //}
                Environment.Exit(0);
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
#if !DEBUG
                FDOT.Enterprise.Architecture.Core.Logging.LogManager.LogError(ex);
#endif
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    Console.WriteLine("ERROR: " + ex);
                }
                Environment.Exit(1);
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
