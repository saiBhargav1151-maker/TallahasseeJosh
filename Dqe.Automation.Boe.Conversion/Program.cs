using System;
using System.Collections.Generic;
using System.Data;
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

namespace Dqe.Automation.Boe.Conversion
{
    class Program
    {
        private static readonly DataTable PlanSummaryBoxes = new DataTable();
        private static readonly DataTable DesignForms = new DataTable();
        private static readonly DataTable FinalForms = new DataTable();

        static void Main()
        {
            Initializer.Initialize();
            EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
            var connStr = BuildAccessConnectionString(string.Format("{0}\\BOE Database.accdb", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
            //const string structureQuery = @"Select * From [Complete] where [Obsolete] = 'no'";
            const string structureQuery = @"Select * From [Complete]";
            const string linkQuery = @"Select * From [Master Link]";
            const string planSummaryBoxQuery = @"Select * From [Plan Summary Boxes]";
            const string designFormsQuery = @"Select * From [Design Forms]";
            const string finalFormsQuery = @"Select * From [Final Form]";
            IList<PayItemStructure> structures = null;
            using (var conn = new System.Data.OleDb.OleDbConnection(connStr))
            {
                conn.Open();
                using (var cmd = new System.Data.OleDb.OleDbCommand(planSummaryBoxQuery, conn))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr != null)
                        {
                            PlanSummaryBoxes.Load(dr);
                            dr.Close();
                        }
                    }
                }
                using (var cmd = new System.Data.OleDb.OleDbCommand(designFormsQuery, conn))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr != null)
                        {
                            DesignForms.Load(dr);
                            dr.Close();
                        }
                    }
                }
                using (var cmd = new System.Data.OleDb.OleDbCommand(finalFormsQuery, conn))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr != null)
                        {
                            FinalForms.Load(dr);
                            dr.Close();
                        }
                    }
                }
                using (var cmd = new System.Data.OleDb.OleDbCommand(structureQuery, conn))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr != null)
                        {
                            using (var dt = new DataTable())
                            {
                                dt.Load(dr);
                                if (dt.Rows.Count > 0)
                                {
                                    structures = LoadStructuresIntoDqe(dt);
                                }
                            }
                            dr.Close();    
                        }
                    }
                }
                using (var cmd = new System.Data.OleDb.OleDbCommand(linkQuery, conn))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr != null)
                        {
                            using (var dt = new DataTable())
                            {
                                dt.Load(dr);
                                if (dt.Rows.Count > 0)
                                {
                                    PairItemsAndStructuresInDqe(dt, structures);
                                }
                            }
                            dr.Close();
                        }
                    }
                }
                conn.Close();
                Console.WriteLine("Committing...");
                UnitOfWorkProvider.TransactionManager.Commit();
            }
        }

        private static void PairItemsAndStructuresInDqe(DataTable dt, IList<PayItemStructure> structures)
        {
            var allpims = new PayItemMasterRepository().GetAll("*").ToList();
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var structure = dt.Rows[i]["structure"] == DBNull.Value
                    ? string.Empty
                    : dt.Rows[i]["structure"].ToString();
                if (string.IsNullOrWhiteSpace(structure)) continue;
                var item = dt.Rows[i]["Master_PayItem"] == DBNull.Value
                    ? string.Empty
                    : dt.Rows[i]["Master_PayItem"].ToString();
                if (string.IsNullOrWhiteSpace(structure)) continue;
                var pis = structures.FirstOrDefault(ii => ii.StructureId == structure);
                if (pis == null) continue;
                var pims = allpims.Where(ii => ii.RefItemName == item).ToList();
                if (!pims.Any()) continue;
                foreach (var payItemMaster in pims)
                {
                    Console.WriteLine("Associating Pay Item {0}", payItemMaster.RefItemName);
                    pis.AddPayItem(payItemMaster);
                }
            }
        }

        private static IList<PayItemStructure> LoadStructuresIntoDqe(DataTable dt)
        {
            var sys = new DqeUserRepository().GetSystemAccount();
            var pisr = new PayItemStructureRepository();
            var pisl = new List<PayItemStructure>();
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var pis = new PayItemStructure(pisr);
                var pist = pis.GetTransformer();
                //pist.Accuracy = "";
                pist.BoeRecentChangeDate = dt.Rows[i]["EditNoteDate"] == DBNull.Value ? (DateTime?)null : (DateTime)dt.Rows[i]["EditNoteDate"];
                pist.BoeRecentChangeDescription = dt.Rows[i]["updatenotes"] == DBNull.Value ? string.Empty : dt.Rows[i]["updatenotes"].ToString();
                pist.ConstructionFormsText = dt.Rows[i]["Final Form"] == DBNull.Value || dt.Rows[i]["Final Form"].ToString() == "0"
                    ? string.Empty
                    : FinalForms.Select().First(ii => ii["ID"].ToString() == dt.Rows[i]["Final Form"].ToString())["Final, Form"].ToString();
                pist.DesignFormsText = dt.Rows[i]["DesignForm"] == DBNull.Value
                    ? string.Empty
                    : DesignForms.Select().First(ii => ii["ID"].ToString() == dt.Rows[i]["DesignForm"].ToString())["DesignForm"].ToString();
                pist.Details = dt.Rows[i]["Detail"] == DBNull.Value ? string.Empty : dt.Rows[i]["Detail"].ToString().Trim().Substring(0, Math.Min(dt.Rows[i]["Detail"].ToString().Trim().Length, 8000));
                pist.EssHistory = dt.Rows[i]["OldHistory"] == DBNull.Value ? string.Empty : dt.Rows[i]["OldHistory"].ToString();
                pist.IsMonitored = false;
                pist.IsPlanQuantity = dt.Rows[i]["PlanQuantity"] == DBNull.Value 
                    ? false
                    : dt.Rows[i]["PlanQuantity"].ToString().ToLower().StartsWith("yes/no")
                        ? (bool?)null
                        : dt.Rows[i]["PlanQuantity"].ToString().ToLower().StartsWith("yes");
                pist.MonitorSrsId = null;
                pist.Notes = dt.Rows[i]["NotesImportantDates"] == DBNull.Value ? string.Empty : dt.Rows[i]["NotesImportantDates"].ToString();
                pist.OtherText = dt.Rows[i]["OtherRef"] == DBNull.Value ? string.Empty : dt.Rows[i]["OtherRef"].ToString();
                pist.PendingInformation = dt.Rows[i]["PendingInfo"] == DBNull.Value ? string.Empty : dt.Rows[i]["PendingInfo"].ToString();
                pist.PlanSummary = dt.Rows[i]["PlanSummaryForm"] == DBNull.Value
                    ? string.Empty
                    : PlanSummaryBoxes.Select().First(ii => ii["ID"].ToString() == dt.Rows[i]["PlanSummaryForm"].ToString())["DesignForm"].ToString();  
                    //dt.Rows[i]["PlanSummaryForm"].ToString();
                pist.PpmChapterText = dt.Rows[i]["PPM Ref"] == DBNull.Value ? string.Empty : dt.Rows[i]["PPM Ref"].ToString();
                //pist.PrimaryUnit = "";
                //pist.SecondaryUnit = "";
                pist.SpecificationsText = dt.Rows[i]["Specifications"] == DBNull.Value ? string.Empty : dt.Rows[i]["Specifications"].ToString();
                pist.SrsId = 0;
                pist.StandardsText = "";
                pist.StructureDescription = dt.Rows[i]["Z"] == DBNull.Value ? string.Empty : dt.Rows[i]["Z"].ToString();
                pist.StructureId = dt.Rows[i]["EStructure"] == DBNull.Value ? string.Empty : dt.Rows[i]["EStructure"].ToString();
                pist.Title = dt.Rows[i]["ESDescription"] == DBNull.Value ? string.Empty : dt.Rows[i]["ESDescription"].ToString();
                pist.TrnsportText = "";
                pist.RequiredItems = dt.Rows[i]["Related_must"] == DBNull.Value ? string.Empty : dt.Rows[i]["Related_must"].ToString();
                pist.RecommendedItems = dt.Rows[i]["Related_should"] == DBNull.Value ? string.Empty : dt.Rows[i]["Related_should"].ToString();
                pis.Transform(pist, sys);
                pisl.Add(pis);
            }
            foreach (var pis in pisl)
            {
                Console.WriteLine("Adding Pay Item Structure {0}", pis.StructureId);
                UnitOfWorkProvider.CommandRepository.Add(pis);
            }
            return pisl;
        }

        private static string BuildAccessConnectionString(string filename)
        {
            return string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};", filename);
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
