using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using Dqe.ApplicationServices;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Infrastructure;
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
                //test
                //var mfl1 = new MasterFileRepository().GetAll().ToList();
                //var syl1 = mfl1.Select(masterFile => masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).StartsWith("9")
                //   ? string.Format("0{0}", masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'))
                //   : string.Format("1{0}", masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'))).ToList();
                //var sy1 = syl1.OrderByDescending(i => i).First();
                //LrePriceSet(sy1.Substring(1));
                //return;
                //end test

                var sys = new DqeUserRepository().GetSystemAccount();
                var pimr = new PayItemMasterRepository();
                var wTservice = new WebTransportService();
                       
                MasterFileCopyProcess(wTservice);
                
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
                Console.WriteLine("Starting Step 3 - {0}", DateTime.Now);
                Console.WriteLine("Querying Bid History");
                var allHistory = ((IEnumerable<BidHistory>)wTservice.GetAllBidHistory(36)).ToList();

                //***** BEGIN add ls/db history to all history
                var lsDbHistory = ((IEnumerable<BidHistory>)wTservice.GetLsDbEstimateHistory()).ToList();
                //get distinct proposals that have bid data
                var distinctProposals = new Dictionary<string, DateTime>();
                foreach (var bidHistory in allHistory)
                {
                    foreach (var proposal in bidHistory.Proposals)
                    {
                        if (distinctProposals.ContainsKey(proposal.Proposal)) continue;
                        distinctProposals.Add(proposal.Proposal, proposal.Letting);
                    }
                }
                //identify matching estimate only data (ls and db non-bid proposals)
                foreach (var bidHistory in allHistory)
                {
                    var h = lsDbHistory.FirstOrDefault(i => i.ItemName == bidHistory.ItemName);
                    if (h == null) continue;
                    //ls/db history has the item
                    foreach (var hp in h.Proposals)
                    {
                        var proposal = hp.Proposal;
                        var index = proposal.LastIndexOf("LS", StringComparison.Ordinal);
                        var test = hp.Quantity;
                        if (index >= 0)
                        {
                            proposal = proposal.Remove(index, 2);
                        }
                        else
                        {
                            index = proposal.LastIndexOf("DB", StringComparison.Ordinal);
                            if (index >= 0) proposal = proposal.Remove(index, 2);
                        }
                        if (!distinctProposals.ContainsKey(proposal)) continue;
                        //found a db or ls proposal for an 'as bid' proposal
                        var letting = distinctProposals[proposal];
                        hp.Letting = letting;
                        hp.Bids[0].LettingDate = letting;
                        bidHistory.Proposals.Add(hp);
                    }
                }
                //return;
                //***** END add ls/db history to all history

                var mar = new MarketAreaRepository();
                var mal = mar.GetAllMarketAreas().ToList();
                var counties = mar.GetAllCounties().ToList();
                var engine = new PricingEngine(mar);
                Console.WriteLine("Starting Step 4 - {0}", DateTime.Now);
                Console.WriteLine("Processing item's average prices and bid history");
                var itemCount = pimr.GetAllCount(sy.Substring(1));
                var processed = 0;
                while (processed < itemCount)
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
                                //check for zero quantity
                                if (proposal.Quantity == 0)
                                {
                                    throw new Exception($"Proposal Id:{proposal.Proposal} and Item {item.RefItemName} has a zero quantity.");
                                }
                                proposal.EstimateAmount = proposal.ExtendedAmount / proposal.Quantity;

                                if (proposal.Proposal.LastIndexOf("LS", StringComparison.Ordinal) >= 0 ||
                                    proposal.Proposal.LastIndexOf("DB", StringComparison.Ordinal) >= 0)
                                    // For the LS and DB duplicate jobs, it is already a unit price
                                    continue;

                                foreach (var bid in proposal.Bids)
                                {
                                    bid.Price = bid.Price / bid.Quantity;
                                }
                            }
                        }
                        var bidSet = engine.CalculateStateAveragePrice(bidHistory, item, sys);
                        var tItem = item.GetTransformer();
                        tItem.StateReferencePrice = Math.Round((bidSet.QuantityWeightedAveragePrice + bidSet.TimeWeightedAveragePrice) / 2, 2, MidpointRounding.AwayFromZero);

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
                            tMaap.Price = Math.Round((bidSet.QuantityWeightedAveragePrice + bidSet.TimeWeightedAveragePrice) / 2, 2, MidpointRounding.AwayFromZero);
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
                            tCap.Price = Math.Round((bidSet.QuantityWeightedAveragePrice + bidSet.TimeWeightedAveragePrice) / 2, 2, MidpointRounding.AwayFromZero);
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
                Console.WriteLine("Starting Step 6 - {0}", DateTime.Now);
                Console.WriteLine("Push Prices to LRE");
                LrePriceSet(sy.Substring(1));

                //if (SqlReviewHelper.Current != null)
                //{
                //    SqlReviewHelper.Current.Dump();
                //}
                Environment.Exit(0);
            }
            catch (Exception ex)
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

        private static void LrePriceSet(string specYear)
        {
            var pimr = new PayItemMasterRepository();
            Console.WriteLine("Query all DQE current spec year items and eager load county and market area prices... {0}", DateTime.Now);
            var items = pimr.GetAllWithPrices(specYear);
            UnitOfWorkProvider.CommandRepository.Clear();

            new LreService().UpdateLrePrices(items);

        }

        private static void MasterFileCopyProcess(IWebTransportService webTransportService)
        {
            var masterFileRepository = new MasterFileRepository();
            var masterFiles = masterFileRepository.GetAll().ToList();
            var currentMasterFile = masterFiles.FirstOrDefault(i => i.DoMasterFileCopy);
            if (currentMasterFile == null)
                return;

            var sys = new DqeUserRepository().GetSystemAccount();
            if (currentMasterFile.EffectiveDate == null)
            {
                var cmft = currentMasterFile.GetTransformer();
                cmft.EffectiveDate = null;
                cmft.DoMasterFileCopy = false;
                currentMasterFile.Transform(cmft, sys);
                UnitOfWorkProvider.TransactionManager.Commit();
                return;
            }

            var effectiveDate = currentMasterFile.EffectiveDate;
            var newFileNum = Convert.ToInt32(currentMasterFile.EffectiveDate.Value.Year.ToString(CultureInfo.InvariantCulture).Substring(2));
            var greatestMasterFile = masterFiles.Where(i => i.FileNumber < 90).Max(i => i.FileNumber);
            if (newFileNum <= greatestMasterFile)
            {
                var cmft = currentMasterFile.GetTransformer();
                cmft.EffectiveDate = null;
                cmft.DoMasterFileCopy = false;
                currentMasterFile.Transform(cmft, sys);
                UnitOfWorkProvider.TransactionManager.Commit();
                return;
            }
            Console.WriteLine("Copying Master File - {0}", DateTime.Now);
            var newMasterFile = new MasterFile(masterFileRepository);
            var masterFileTransformer = newMasterFile.GetTransformer();
            masterFileTransformer.FileNumber = newFileNum;
            newMasterFile.Transform(masterFileTransformer, sys);
            UnitOfWorkProvider.CommandRepository.Add(newMasterFile);

            //var payItemsUpdated = new List<PayItemMaster>();
            var payItemsUpdated = new Dictionary<long, DateTime?>();
            var payItemsCopied = new List<PayItemMaster>();
            var MasterFileIncludeObsoletePayItems = Convert.ToBoolean(ConfigurationManager.AppSettings["MasterFileIncludeObsoletePayItems"]);
            var OldSpecBookVaildAsOfDate = Convert.ToString(ConfigurationManager.AppSettings["OldSpecBookVaildAsOfDate"]);
            foreach (var payItemMaster in currentMasterFile.PayItemMasters)
            {
                if (payItemMaster.ObsoleteDate.HasValue)
                {
                    if (payItemMaster.ObsoleteDate.Value > effectiveDate && MasterFileIncludeObsoletePayItems == false)
                    {
                        payItemsUpdated.Add(payItemMaster.Id, payItemMaster.ObsoleteDate);
                        CreatePayItemMaster(newMasterFile, payItemMaster, effectiveDate, payItemMaster.ObsoleteDate, sys);
                    }
                    else
                    {
                        CreatePayItemMaster(newMasterFile, payItemMaster, Convert.ToDateTime(OldSpecBookVaildAsOfDate), payItemMaster.ObsoleteDate, sys);
                        payItemsCopied.Add(payItemMaster);
                    }
                        continue;
                }
                else
                {
                    CreatePayItemMaster(newMasterFile, payItemMaster, effectiveDate, null, sys);
                    payItemsCopied.Add(payItemMaster);
                }

                var pim = payItemMaster.GetTransformer();
                pim.ObsoleteDate = effectiveDate.Value.AddDays(-1);
                payItemMaster.Transform(pim, sys);
            }

            var currentMasterFileTransformer = currentMasterFile.GetTransformer();
            currentMasterFileTransformer.EffectiveDate = null;
            currentMasterFileTransformer.DoMasterFileCopy = false;
            currentMasterFile.Transform(currentMasterFileTransformer, sys);
            UnitOfWorkProvider.TransactionManager.Commit();

            var mergedPayItems = new List<PayItemMaster>();
            mergedPayItems.AddRange(currentMasterFile.PayItemMasters);
            mergedPayItems.AddRange(newMasterFile.PayItemMasters);

            var insertItemsToWt = webTransportService.InsertRefItems(mergedPayItems, sys);
            if (insertItemsToWt != null)
            {
                try
                {
                    var mfr = new MasterFileRepository();
                    var newMf = mfr.GetByFileNumber(newMasterFile.FileNumber);
                    var oldMf = mfr.GetByFileNumber(currentMasterFile.FileNumber);

                    foreach (var payItemCopied in payItemsCopied)
                    {
                        var payItemMaster = oldMf.PayItemMasters.First(i => i.Id == payItemCopied.Id);
                        var pim = payItemMaster.GetTransformer();
                        pim.ObsoleteDate = null;
                        payItemMaster.Transform(pim, sys);
                    }
                    foreach (var payItemUpdate in payItemsUpdated)
                    {
                        var payItemMaster = oldMf.PayItemMasters.First(i => i.Id == payItemUpdate.Key);
                        var pim = payItemMaster.GetTransformer();
                        pim.ObsoleteDate = payItemUpdate.Value;
                        payItemMaster.Transform(pim, sys);
                    }
                    UnitOfWorkProvider.CommandRepository.Remove(newMf);
                    UnitOfWorkProvider.TransactionManager.Commit();
                }
                catch (Exception exception)
                {
                    //If this error is thrown then the insert of the new pay items into WT failed and the subsequent attempt to rollback the DQE master file failed.
                    //This was the only way to handle the scenario since the inserts/updates are in two separate transactions.
                    var ex = new Exception("WT pay item synchronization & the rollback of DQE pay item list failed.  This exception is for the rollback only.", exception);
                    throw ex;
                }

                //If the above try/catch passes this means that WT failed, however we were able to rollback DQE to its previous state.
                //We still must throw the original error though.
                throw insertItemsToWt;
            }
        }

        private static void CreatePayItemMaster(MasterFile masterFile, PayItemMaster payItemMaster, DateTime? effectiveDate, DateTime? obsoleteDate, DqeUser sys)
        {
            var newMasterFileItem = new PayItemMaster();
            var newMasterFileItemTransformer = newMasterFileItem.GetTransformer();
            //A
            newMasterFileItemTransformer.Administrative = payItemMaster.Administrative;
            newMasterFileItemTransformer.AlternateItemName = payItemMaster.AlternateItemName;
            newMasterFileItemTransformer.AutoPaidPercentSchedule = payItemMaster.AutoPaidPercentSchedule;
            newMasterFileItemTransformer.AsphaltFactor = payItemMaster.AsphaltFactor;
            //B
            newMasterFileItemTransformer.BidAsLumpSum = payItemMaster.BidAsLumpSum;
            newMasterFileItemTransformer.BidRequirementCode = payItemMaster.BidRequirementCode;
            //C
            newMasterFileItemTransformer.CalculatedUnit = payItemMaster.CalculatedUnit;
            newMasterFileItemTransformer.CoApprovalRequired = payItemMaster.CoApprovalRequired;
            newMasterFileItemTransformer.CombineWithLikeItems = payItemMaster.CombineWithLikeItems;
            newMasterFileItemTransformer.CommonUnit = payItemMaster.CommonUnit;
            newMasterFileItemTransformer.ContractClass = payItemMaster.ContractClass;
            newMasterFileItemTransformer.ConversionFactorToCommonUnit = payItemMaster.ConversionFactorToCommonUnit;
            //Check to see if the payItemMaster.CreatedDate is null if so set to DQE 
            newMasterFileItemTransformer.CreatedBy = payItemMaster.CreatedBy == null ? "DQE" :  payItemMaster.CreatedBy;
            //Check to see if the payItemMaster.CreatedBy is null if so set to todays date         
            newMasterFileItemTransformer.CreatedDate = payItemMaster.CreatedDate == null ? DateTime.Now  : payItemMaster.CreatedDate; 
            newMasterFileItemTransformer.ConcreteFactor = payItemMaster.ConcreteFactor;
            //D
            newMasterFileItemTransformer.DbeInterest = payItemMaster.DbeInterest;
            newMasterFileItemTransformer.DbePercentToApply = payItemMaster.DbePercentToApply;
            newMasterFileItemTransformer.Description = payItemMaster.Description;
            //E
            newMasterFileItemTransformer.ExemptFromMaa = payItemMaster.ExemptFromMaa;
            newMasterFileItemTransformer.ExemptFromRetainage = payItemMaster.ExemptFromRetainage;
            newMasterFileItemTransformer.EffectiveDate = effectiveDate;
            //F
            newMasterFileItemTransformer.FuelAdjustment = payItemMaster.FuelAdjustment;
            newMasterFileItemTransformer.FuelAdjustmentType = payItemMaster.FuelAdjustmentType;
            newMasterFileItemTransformer.FactorNotes = payItemMaster.FactorNotes;
            //I
            newMasterFileItemTransformer.Ildt2 = payItemMaster.Ildt2;
            newMasterFileItemTransformer.Ilflg1 = payItemMaster.Ilflg1;
            newMasterFileItemTransformer.Illst1 = payItemMaster.Illst1;
            newMasterFileItemTransformer.Ilnum1 = payItemMaster.Ilnum1;
            newMasterFileItemTransformer.Ilsst1 = payItemMaster.Ilsst1;
            newMasterFileItemTransformer.IsFederalFunded = payItemMaster.IsFederalFunded;
            newMasterFileItemTransformer.IsFixedPrice = payItemMaster.IsFixedPrice;
            newMasterFileItemTransformer.IsFrontLoadedItem = payItemMaster.IsFrontLoadedItem;
            newMasterFileItemTransformer.ItemClass = payItemMaster.ItemClass;
            newMasterFileItemTransformer.ItemType = payItemMaster.ItemType;
            newMasterFileItemTransformer.Itmqtyprecsn = payItemMaster.Itmqtyprecsn;
            //L
            newMasterFileItemTransformer.LastUpdatedBy = payItemMaster.LastUpdatedBy;
            newMasterFileItemTransformer.LastUpdatedDate = payItemMaster.LastUpdatedDate;
            newMasterFileItemTransformer.LumpSum = payItemMaster.LumpSum;
            //M
            newMasterFileItemTransformer.MajorItem = payItemMaster.MajorItem;
            //N
            newMasterFileItemTransformer.NonBid = payItemMaster.NonBid;
            //O
            newMasterFileItemTransformer.ObsoleteDate = obsoleteDate;
            newMasterFileItemTransformer.OpenedDate = DateTime.Now;
            //P
            newMasterFileItemTransformer.PayPlan = payItemMaster.PayPlan;
            newMasterFileItemTransformer.PercentScheduleItem = payItemMaster.PercentScheduleItem;
            //R
            newMasterFileItemTransformer.RecordSource = payItemMaster.RecordSource;
            newMasterFileItemTransformer.RefItemName = payItemMaster.RefItemName;
            newMasterFileItemTransformer.RefPrice = payItemMaster.RefPrice;
            newMasterFileItemTransformer.RegressionInclusion = payItemMaster.RegressionInclusion;
            //S
            newMasterFileItemTransformer.ShortDescription = payItemMaster.ShortDescription;
            newMasterFileItemTransformer.SpecialtyItem = payItemMaster.SpecialtyItem;
            newMasterFileItemTransformer.SuppDescriptionRequired = payItemMaster.SuppDescriptionRequired;
            newMasterFileItemTransformer.SrsId = payItemMaster.SrsId;
            newMasterFileItemTransformer.StateReferencePrice = payItemMaster.StateReferencePrice;
            //U
            newMasterFileItemTransformer.Unit = payItemMaster.Unit;
            newMasterFileItemTransformer.UnitSystem = payItemMaster.UnitSystem;
            //
            newMasterFileItem.Transform(newMasterFileItemTransformer, sys);
            newMasterFileItem.SetCostBasedTemplate(payItemMaster.MyCostBasedTemplate);
            newMasterFileItem.SetPayItemStructure(payItemMaster.MyPayItemStructure);

            foreach (var costGroup in payItemMaster.CostGroups)
                newMasterFileItem.AddCostGroup(costGroup);

            masterFile.AddPayItemMaster(newMasterFileItem);
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
