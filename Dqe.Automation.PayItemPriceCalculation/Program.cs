using System;
using System.Linq;
using System.Threading.Tasks;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Infrastructure;
using Dqe.Infrastructure.EntityIoC;
using Dqe.Infrastructure.Fdot;
using Dqe.Infrastructure.Providers;
using Dqe.Infrastructure.Repositories.Custom;
using Dqe.Infrastructure.Services;

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
            var items = pimr.GetAll();
            //var x = 0;
            var wTservice = new WebTransportService();
            var mar = new MarketAreaRepository();
            var mal = mar.GetAllMarketAreas().ToList();
            var counties = mar.GetAllCounties().ToList();
            var engine = new PricingEngine();
            //filter items for testing
            items = items.Where(i => !i.LumpSum && !i.CalculatedUnit.StartsWith("ZZ") && !i.CalculatedUnit.StartsWith("LS"));
            //items = items.Take(1000);
            //end filter
            var lock1 = new object();
            var lock2 = new object();
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 25};
            Parallel.ForEach(items, parallelOptions, item =>
            {
                //don't calculate averages for lump sum
                //if (item.LumpSum || item.CalculatedUnit.StartsWith("ZZ") || item.CalculatedUnit.StartsWith("LS")) continue;   
                //if (x > 25) break;
                //Console.WriteLine(x);
                try
                {
                    var bidHistory = (ApplicationServices.BidHistory)wTservice.GetBidHistory(item.RefItemName, 48);
                    //calculate state ref price
                    var bids = (from proposal
                                    in bidHistory.Proposals
                                from bid in proposal.Bids
                                where !bid.IsBlank
                                select bid).ToList();
                    var bidSet = new BidSet
                    {
                        Bids = bids
                    };
                    engine.CalculateAveragePrice(bidSet);
                    var tItem = item.GetTransformer();
                    tItem.StateReferencePrice = bidSet.AveragePrice;
                    item.Transform(tItem, sys);
                    foreach (var ma in mal)
                    {
                        //calculate market area price
                        bids.Clear();
                        bids.AddRange(from proposal
                                          in bidHistory.Proposals
                                      where ma.Counties.Select(i => i.Name).Contains(proposal.County)
                                      from bid in proposal.Bids
                                      where !bid.IsBlank
                                      select bid);
                        bidSet = new BidSet
                        {
                            Bids = bids
                        };
                        engine.CalculateAveragePrice(bidSet);
                        var maap = new MarketAreaAveragePrice();
                        var tMaap = maap.GetTransformer();
                        tMaap.Price = bidSet.AveragePrice;
                        maap.Transform(tMaap, sys);
                        lock (lock1)
                        {
                            ma.AddItemAveragePrice(item, maap);
                        }

                    }
                    foreach (var county in counties)
                    {
                        //calculate county price
                        bids.Clear();
                        bids.AddRange(from proposal
                                          in bidHistory.Proposals
                                      where county.Name == proposal.County
                                      from bid in proposal.Bids
                                      where !bid.IsBlank
                                      select bid);
                        bidSet = new BidSet
                        {
                            Bids = bids
                        };
                        engine.CalculateAveragePrice(bidSet);
                        var cap = new CountyAveragePrice();
                        var tCap = cap.GetTransformer();
                        tCap.Price = bidSet.AveragePrice;
                        cap.Transform(tCap, sys);
                        lock (lock2)
                        {
                            county.AddItemAveragePrice(item, cap);
                        }
                    }
                }
                catch
                {
                    Console.WriteLine(item.RefItemName);
                }
                //x += 1;
            });
            UnitOfWorkProvider.TransactionManager.Commit();
            //foreach (var item in items)
            //{
            //    //don't calculate averages for lump sum
            //    if (item.LumpSum || item.CalculatedUnit.StartsWith("ZZ") || item.CalculatedUnit.StartsWith("LS")) continue;
            //    if (x > 25) break;
            //    Console.WriteLine(x);
            //    var bidHistory = (ApplicationServices.BidHistory)wTservice.GetBidHistory(item.RefItemName, 12);
            //    //calculate state ref price
            //    var bids = (from proposal 
            //                    in bidHistory.Proposals 
            //                from bid in proposal.Bids 
            //                where !bid.IsBlank 
            //                select bid).ToList();
            //    var bidSet = new BidSet
            //    {
            //        Bids = bids
            //    };
            //    engine.CalculateAveragePrice(bidSet);
            //    var o = new object();
            //    var tItem = item.GetTransformer();
            //    tItem.StateReferencePrice = bidSet.AveragePrice;
            //    item.Transform(tItem, sys);
            //    foreach (var ma in mal)
            //    {
            //        //calculate market area price
            //        bids.Clear();
            //        bids.AddRange(from proposal 
            //                          in bidHistory.Proposals 
            //                      where ma.Counties.Select(i => i.Name).Contains(proposal.County) 
            //                      from bid in proposal.Bids 
            //                      where !bid.IsBlank 
            //                      select bid);
            //        bidSet = new BidSet
            //        {
            //            Bids = bids
            //        };
            //        engine.CalculateAveragePrice(bidSet);
            //        if (bidSet.Bids.Any())
            //        {
            //            o = new object();
            //        }
            //        var maap = new MarketAreaAveragePrice();
            //        var tMaap = maap.GetTransformer();
            //        tMaap.Price = bidSet.AveragePrice;
            //        maap.Transform(tMaap, sys);
            //        ma.AddItemAveragePrice(item, maap);
            //    }
            //    foreach (var county in counties)
            //    {
            //        //calculate county price
            //        bids.Clear();
            //        bids.AddRange(from proposal 
            //                          in bidHistory.Proposals 
            //                      where county.Name == proposal.County 
            //                      from bid in proposal.Bids 
            //                      where !bid.IsBlank 
            //                      select bid);
            //        bidSet = new BidSet
            //        {
            //            Bids = bids
            //        };
            //        engine.CalculateAveragePrice(bidSet);
            //        if (bidSet.Bids.Any())
            //        {
            //            o = new object();
            //        }
            //        var cap = new CountyAveragePrice();
            //        var tCap = cap.GetTransformer();
            //        tCap.Price = bidSet.AveragePrice;
            //        cap.Transform(tCap, sys);
            //        county.AddItemAveragePrice(item, cap);
            //    }
            //    x += 1;
            //}
            //UnitOfWorkProvider.TransactionManager.Commit();
        }

        private static object[] EntityDependencyResolverOnResolveConstructorArguments(object sender, ResolveConstructorArgumentsArgs args)
        {
            if (args.EntityType.IsAssignableFrom(typeof(DqeUser))) return new object[] { new StaffService(), new DqeUserRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(PayItemStructure))) return new object[] { new PayItemStructureRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(MasterFile))) return new object[] { new MasterFileRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(PayItem))) return new object[] { new PayItemRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(Project))) return new object[] { new ProjectRepository(), UnitOfWorkProvider.CommandRepository, new WebTransportService() };
            if (args.EntityType.IsAssignableFrom(typeof(MarketArea))) return new object[] { new MarketAreaRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(Proposal))) return new object[] { new ProposalRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(ProjectEstimate))) return new object[] { new WebTransportService() };
            return null;
        }
    }
}
