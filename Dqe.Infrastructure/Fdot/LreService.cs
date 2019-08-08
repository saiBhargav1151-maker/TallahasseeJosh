using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Lre;
using NHibernate.Hql.Ast.ANTLR;

namespace Dqe.Infrastructure.Fdot
{
    public class LreService : ILreService
    {

        public IEnumerable<Domain.Model.Lre.Project> GetProjects(string projectName)
        {
            using (var session = Initializer.LreSessionFactory.OpenSession())
            {
                var ps = session.QueryOver<Domain.Model.Lre.Project>()
                    .Where(i => i.ProjectName == projectName)
                    .List();
                return ps;
            }
        }

        public ProjectSnapshot GetProjectSnapshot(long id)
        {
            using (var session = Initializer.LreSessionFactory.OpenSession())
            {
                var ps = session.QueryOver<ProjectSnapshot>()
                    .Where(i => i.Id == id)
                    .Left.JoinQueryOver(i => i.Versions)
                    .SingleOrDefault();
                return ps;
            }
        }

        public void UpdateLrePrices(IEnumerable<PayItemMaster> items)
        {
            /*
            Regarding setting pay item unit prices in LRE, here are the high level steps:
 
            Delete Unlocked Turnpike-Counties prices
            DELETE LREDBA.LRET026_PI_DIST_CN WHERE PAY_ITM_ID = @pay_item AND UNIT_PRCE_LOCK_CD <> 'L' 
 
            Delete Unlocked Prices of all counties
            DELETE LREDBA.LRET025_PI_CNTY WHERE PAY_ITM_ID = @pay_item AND UNIT_PRCE_LOCK_CD <> 'L' 
 
            Delete Previous Market Area Prices
            DELETE LREDBA.LRET023_PI_MKT_AR WHERE PAY_ITM_ID = @pay_item
 
            Update statewide unit price
            UPDATE LREDBA.LRET019_PAY_ITEM WHERE PAY_ITM_ID = @pay_item
 
            INSERT new county level unit prices into LREDBA.LRET025_PI_CNTY
 
            Copy the same copy level unit prices into the Turnpike-Counties unit prices, hard code Turnpike district (MNG_DIST_CD = ‘08’)
            INSERT same county unit prices into LREDBA.LRET026_PI_DIST_CN
 
            INSERT new market area unit prices into LREDBA.LRET023_PI_MKT_AR
             */

            using (var session = Initializer.LreSessionFactory.OpenSession())
            {
                using (var t = session.BeginTransaction())
                {
                    items = items.OrderBy(i => i.RefItemName);
                    var mal = session.QueryOver<Domain.Model.Lre.MarketArea>().List().Distinct().ToList();
                    Console.WriteLine("Query all LRE items... {0}", DateTime.Now);
                    var pil = session
                        .CreateQuery("from PayItem pi")
                        .List<PayItem>()
                        .Distinct()
                        .ToList();
                    session.Clear();
                    Console.WriteLine("Deleting unlocked Turnpike prices... {0}", DateTime.Now);
                    session
                        .CreateQuery("delete from PayItemDistrict pid where pid.Id.District = '08' and pid.LockCode <> 'L'")
                        .ExecuteUpdate();
                    //delete unlocked county prices that have no child districts for the item
                    Console.WriteLine("Deleting unlocked county prices that have no district price association... {0}", DateTime.Now);
                    //session.CreateQuery("delete from PayItemCounty where exists (select 1 from PayItemCounty pic where pic.LockCode <> 'L' and size(pic.PayItemDistricts) = 0)")
                    //    .ExecuteUpdate();
                    session.CreateQuery("delete from PayItemCounty c where c.Id in (select distinct pic.Id from PayItemCounty pic left join pic.PayItemDistricts pid where pic.LockCode <> 'L' and pid.LockCode = null)")
                        .ExecuteUpdate();
                    //delete market area prices for the item
                    Console.WriteLine("Deleting market area prices... {0}", DateTime.Now);
                    session
                        .CreateQuery("delete from PayItemMarketArea pima")
                        .ExecuteUpdate();
                    foreach (var item in items)
                    {
                        //flush and clear to eliminate nhibernate change tracking
                        session.Flush();
                        session.Clear();
                        //does LRE have the pay item
                        var pi = pil.FirstOrDefault(i => i.Id.ToUpper().Trim() == item.RefItemName.ToUpper().Trim());
                        if (pi == null) continue;
                        //delete unlocked TP district prices for the item
                        Console.WriteLine("Updating LRE prices for item {0}... {1}", pi.Id, DateTime.Now);
                        //update the LRE reference price for the item
                        //Console.WriteLine("Updating reference price for item {0}... {1}", item.RefItemName, DateTime.Now);
                        session
                            .CreateQuery("update PayItem pi set pi.ReferencePrice = :referencePrice where pi.Id = :payItemId")
                            .SetParameter("referencePrice", item.StateReferencePrice.HasValue && item.StateReferencePrice.Value > 0 ? item.StateReferencePrice.Value : item.RefPrice.HasValue && item.RefPrice.Value > 0 ? item.RefPrice.Value : 0)
                            .SetParameter("payItemId", item.RefItemName.ToUpper().Trim().PadRight(10, ' '))
                            .ExecuteUpdate();
                        //get the county price for the item and county
                        //Console.WriteLine("Get county prices for item {0}... {1}", item.RefItemName, DateTime.Now);
                        var picl = session
                            .CreateQuery("from PayItemCounty pic where pic.Id.PayItemId = :payItemId")
                            .SetParameter("payItemId", item.RefItemName.ToUpper().Trim().PadRight(10, ' '))
                            .List<PayItemCounty>()
                            .Distinct()
                            .ToList();
                        //loop county average prices
                        foreach (var cp in item.CountyAveragePrices)
                        {
                            var pic = picl.FirstOrDefault(i => i.Id.County == cp.MyCounty.Code);
                            if (pic == null)
                            {
                                //insert the county price
                                pic = new PayItemCounty
                                {
                                    Id = new PayItemCountyId
                                    {
                                        County = cp.MyCounty.Code,
                                        PayItemId = item.RefItemName
                                    },
                                    LockCode = "N",
                                    UnitPrice = cp.Price
                                };
                                //Console.WriteLine("Insert county {0}:{1} price for item {2}... {3}", cp.MyCounty.Name, cp.MyCounty.Code, item.RefItemName, DateTime.Now);
                                session.SaveOrUpdate(pic);
                                //insert the TP district price for the county
                                var pid = new PayItemDistrict
                                {
                                    Id = new PayItemDistrictId
                                    {
                                        County = cp.MyCounty.Code,
                                        PayItemId = item.RefItemName,
                                        District = "08"
                                    },
                                    LockCode = "N",
                                    UnitPrice = cp.Price
                                };
                                //Console.WriteLine("Insert Turnpike county {0}:{1} price for item {2}... {3}", cp.MyCounty.Name, cp.MyCounty.Code, item.RefItemName, DateTime.Now);
                                session.SaveOrUpdate(pid);
                            }
                            else
                            {
                                //update the county price
                                if (pic.LockCode != "L")
                                {
                                    //Console.WriteLine("Update county {0}:{1} price for item {2}... {3}", cp.MyCounty.Name, cp.MyCounty.Code, item.RefItemName, DateTime.Now);
                                    pic.UnitPrice = cp.Price;    
                                }
                                //get the TP district price
                                var pid = session
                                    .CreateQuery("from PayItemDistrict pid where pid.Id.PayItemId = :payItemId and pid.Id.District = '08' and pid.Id.County = :county")
                                    .SetParameter("payItemId", item.RefItemName.ToUpper().Trim().PadRight(10, ' '))
                                    .SetParameter("county", cp.MyCounty.Code)
                                    .UniqueResult<PayItemDistrict>();
                                if (pid == null)
                                {
                                    //insert the TP district price for the county
                                    pid = new PayItemDistrict
                                    {
                                        Id = new PayItemDistrictId
                                        {
                                            County = cp.MyCounty.Code,
                                            PayItemId = item.RefItemName,
                                            District = "08"
                                        },
                                        LockCode = "N",
                                        UnitPrice = cp.Price
                                    };
                                    //Console.WriteLine("Insert Turnpike county {0}:{1} price for item {2}... {3}", cp.MyCounty.Name, cp.MyCounty.Code, item.RefItemName, DateTime.Now);
                                    session.SaveOrUpdate(pid);
                                }
                                else
                                {
                                    if (pid.LockCode != "L")
                                    {
                                        //Console.WriteLine("Update Turnpike county {0}:{1} price for item {2}... {3}", cp.MyCounty.Name, cp.MyCounty.Code, item.RefItemName, DateTime.Now);
                                        pid.UnitPrice = cp.Price;
                                    }
                                }
                            }
                        }
                        //create market area prices for the item
                        foreach (var ma in item.MarketAreaAveragePrices)
                        {
                            //verify that LRE market areas contain a match for the DQE item's market area
                            var name = ma.MyMarketArea.Name.Trim();
                            if (name.Length < 2) continue;
                            var code = name.Substring(name.Length - 2, 2);
                            if (mal.All(i => i.Id != code)) continue;
                            var pima = new PayItemMarketArea
                            {
                                Id = new PayItemMarketAreaId
                                    {
                                        MarketArea = code,
                                        PayItemId = item.RefItemName
                                    },
                                UnitPrice = ma.Price
                            };
                            //Console.WriteLine("Insert market area {0} price for item {1}... {2}", code, item.RefItemName, DateTime.Now);
                            session.SaveOrUpdate(pima);
                        }
                    }
                    t.Commit();
                }
            }
        }

        public void SetDqeSnapshotInLre(Domain.Model.Project p, DqeUser account, SnapshotLabel label, decimal amount)
        {

            var updateLreSnapshots = Convert.ToBoolean(ConfigurationManager.AppSettings["updateLreSnapshots"]);
            if (!updateLreSnapshots) return;

            //TODO: do LRE snapshots from DQE now need to be deleted in LRE on the removal of DQE proposal/project or labeled snapshot?
            /*
             Regarding snapshots:
                I confirmed it is enough to create a snapshot in LRE by inserting rows in the following two tables for a snapshot:
                LRET085_PROJ_SNAP
                LRET086_VER_SNAP: You don’t have to provide values for all the amounts but if you them, they will show up in LRE snapshot reports 
                    PROJ_VER_NUM: 0, I tested and didn’t see any problem with LRE functionality
                    VER_STAT_CD: ACT (Active)
                    PROJ_VER_AMT: This is the only required value since it is the overall estimate amount in the snapshot
                    MAINT_TRAF_CST_AMT: MOT amount
                    MOBIL_CST_AMT: MOB amount
                    SCOP_CREEP_AMT: Scope Creep amount
                    DSGN_BLD_AMT: Design Build amount
                    LAST_UPDT_USER_ID: DQE User ID (RACF ID)
                    LAST_UPDT_TMS: Create timestamp
             */
            using (var session = Initializer.LreSessionFactory.OpenSession())
            {
                using (var t = session.BeginTransaction())
                {
                    //elminate LS/DB from project number
                    var projectNumber = p.ProjectNumber.Trim();
                    if (projectNumber.Length > 11) return;
                    var pl = session.QueryOver<Domain.Model.Lre.Project>()
                        .Where(i => i.ProjectName == projectNumber)
                        .List();
                    long id = 0;
                    if (pl.Count == 1)
                    {
                        id = pl[0].Id;
                    }
                    if (pl.Count > 1)
                    {
                        foreach (var project in pl)
                        {
                            if (p.District != project.District) continue;
                            id = project.Id;
                            break;
                        }
                        if (id == 0)
                        {
                            return;
                        }
                    }
                    var ps = session.QueryOver<ProjectSnapshot>()
                        .Where(i => i.Id == id)
                        .Left.JoinQueryOver(i => i.Versions)
                        .SingleOrDefault();
                    if (ps == null)
                    {
                        ps = new ProjectSnapshot {Id = id, ProjectName = projectNumber};
                        session.SaveOrUpdate(ps);
                    }
                    VersionSnapshot version = null;
                    switch (label)
                    {
                        case SnapshotLabel.Phase2:
                            version = ps.Versions.FirstOrDefault(i => i.ProjectVersionNumber == 0 && i.LabelCode == "40");
                            break;
                        case SnapshotLabel.Phase3:
                            version = ps.Versions.FirstOrDefault(i => i.ProjectVersionNumber == 0 && i.LabelCode == "50");
                            break;
                        case SnapshotLabel.Phase4:
                            version = ps.Versions.FirstOrDefault(i => i.ProjectVersionNumber == 0 && i.LabelCode == "60");
                            break;
                        case SnapshotLabel.Authorization:
                            version = ps.Versions.FirstOrDefault(i => i.ProjectVersionNumber == 0 && i.LabelCode == "70");
                            break;
                    }
                    if (version == null)
                    {
                        version = new VersionSnapshot
                        {
                            Amount = amount,
                            CreatedCode = "M",
                            Description = " ",
                            DesignBuildAmount = 0,
                            Id = new VersionSnapshotId {ProjectSnapshotId = ps.Id, SnapshotDateTime = DateTime.Now},
                            LastUpdatedBy = account.RacfId,
                            LastUpdatedOn = DateTime.Now,
                            MaintenanceTrafficCost = 0,
                            MobileCost = 0,
                            MyProjectSnapshot = ps,
                            PrimaryVersionCode = "Y",
                            ProjectVersionNumber = 0,
                            ScopeCreepAmount = 0,
                            VersionStatusCode = "ACT"
                        };
                        switch (label)
                        {
                            case SnapshotLabel.Phase2:
                                version.LabelCode = "40";
                                break;
                            case SnapshotLabel.Phase3:
                                version.LabelCode = "50";
                                break;
                            case SnapshotLabel.Phase4:
                                version.LabelCode = "60";
                                break;
                            case SnapshotLabel.Authorization:
                                version.LabelCode = "70";
                                break;
                        }
                        session.SaveOrUpdate(version);
                    }
                    else
                    {
                        version.Amount = amount;
                        version.LastUpdatedOn = DateTime.Now;
                        version.LastUpdatedBy = account.RacfId;
                    }
                    t.Commit();
                }
            }
        }

        public IEnumerable<PayItemGroup> GetLrePickLists()
        {
            using (var session = Initializer.LreSessionFactory.OpenSession())
            {
                var ps = session.QueryOver<PayItemGroup>()
                    .Where(i => i.GroupTypeCode == "3")
                    .List();
                return ps;
            }
        }

        public PayItem GetLrePayItem(string payItemName)
        {
            using (var session = Initializer.LreSessionFactory.OpenSession())
            {
                var ps = session.QueryOver<PayItem>()
                    .Where(i => i.Id == payItemName.PadRight(10))
                    .Left.JoinQueryOver(i => i.PayItemPayItemGroups)
                    .Left.JoinQueryOver(i => i.MyPayItemGroup)
                    .SingleOrDefault();
                return ps;
            }
        }

        public void UpdateRefItem(PayItemMaster payItemMaster, DqeUser user)
        {
            UpdateRefItem(payItemMaster, null, user);
        }

        public void UpdateRefItem(PayItemMaster payItemMaster, dynamic lrePickLists, DqeUser user)
        {
            using (var session = Initializer.LreSessionFactory.OpenSession())
            {
                using (var t = session.BeginTransaction())
                {
                    var isInLre = false;
                    var ps = session.QueryOver<PayItem>()
                    .Where(i => i.Id == payItemMaster.RefItemName.PadRight(10))
                    .SingleOrDefault();
                    if (ps == null)
                    {
                        //LRE only allows 10 chars
                        ps = new PayItem
                        {
                            Id = payItemMaster.RefItemName
                        };
                    }
                    else
                    {
                        isInLre = true;
                    }
                    ps.Description = payItemMaster.Description;
                    //BUG-Fix: Only set the english unit type for new pay items
                    if (!isInLre)
                    {
                        ps.EnglishMetricCode = "E";
                        ps.MetricPayItemId = " ";
                    }
                    ps.OtherShortDescription = payItemMaster.ShortDescription;
                    //TODO: how do we handle obsolete items - LRE doesn't have an obsolete date, so we have a problem
                    if (!isInLre)
                    {
                        //Changeing PayItemStatusCode from "N" to "A" for Cherwell ticket 418654 
                        ps.PayItemStatusCode = "A";
                    }
                    else
                    {
                        if (!payItemMaster.ObsoleteDate.HasValue)
                        {
                            ps.PayItemStatusCode = "A";
                        }
                        //BUG-Fix: removed setting LRE pay item obsolete under any condition
                        //else
                        //{
                        //    if (payItemMaster.ObsoleteDate.Value.Date <= DateTime.Now.Date)
                        //    {
                        //        ps.PayItemStatusCode = "O";
                        //    }
                        //}
                    }
                    ps.PayItemStatusDate = DateTime.Now;
                    //DQE will zero out LRE reference price if DQE can calculate bid history, so we use the DQE calculated statewide average price
                    var isManual = !payItemMaster.StateReferencePrice.HasValue || payItemMaster.StateReferencePrice.Value == 0;
                    ps.ReferencePrice = payItemMaster.RefPrice.HasValue
                        ? payItemMaster.RefPrice.Value == 0
                            ? payItemMaster.StateReferencePrice.HasValue
                                ? payItemMaster.StateReferencePrice.Value
                                : 0
                            : payItemMaster.RefPrice.Value
                        : payItemMaster.StateReferencePrice.HasValue
                            ? payItemMaster.StateReferencePrice.Value
                            : 0;
                    ps.ShortDescription = payItemMaster.ShortDescription;
                    ps.Source = isManual ? "MANUAL" : "CALCULATED";
                    ps.UnitOfMeasureCode = payItemMaster.Unit;
                    session.SaveOrUpdate(ps);
                    
                    var gs = session.QueryOver<PayItemGroup>()
                        .Where(i => i.GroupTypeCode == "3")
                        .List();


                    //foreach (var pl in ps.PayItemPayItemGroups)
                    //{
                    //    if (pl.MyPayItemGroup.GroupTypeCode == "3")
                    //    {
                    //        pl.Status = "I";    
                    //    }
                    //}

                    if (ps.PayItemStatusCode == "A" || ps.PayItemStatusCode == "N")
                    {
                        foreach (var lrePickList in lrePickLists)
                        {
                            if (!bool.Parse(lrePickList.selected.ToString())) continue;
                            var list = lrePickList;
                            //get a 3 digit (char) ID - the dynmic type will consider the ID as numeric and drop leading zeros
                            var listId = list.id.ToString().Trim().PadLeft(3, '0');
                            var existingList = ps.PayItemPayItemGroups.FirstOrDefault(i => i.MyPayItemGroup.Id.Trim() == listId);
                            if (existingList == null)
                            {
                                var g = gs.First(i => i.Id.Trim() == listId);
                                var pig = new PayItemPayItemGroup { Status = "A", Quantity = 0 };
                                ps.AddPayItemPayItemGroup(pig);
                                g.AddPayItemPayItemGroup(pig);
                            }
                            else
                            {
                                existingList.Status = "A";
                            }
                        }    
                    }
                    
                    t.Commit();
                }
            }
        }
    }
}