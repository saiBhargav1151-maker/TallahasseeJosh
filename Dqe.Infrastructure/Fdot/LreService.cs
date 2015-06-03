using System;
using System.Collections.Generic;
using System.Linq;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Lre;

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
                    .Where(i => i.Id == payItemName)
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
                    .Where(i => i.Id == payItemMaster.RefItemName)
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
                    ps.EnglishMetricCode = "E";
                    ps.MetricPayItemId = " ";
                    ps.OtherShortDescription = payItemMaster.ShortDescription;
                    //TODO: how do we handle obsolete items - LRE doesn't have an obsolete date, so we have a problem
                    if (!isInLre)
                    {
                        ps.PayItemStatusCode = "N";
                    }
                    else
                    {
                        if (!payItemMaster.ObsoleteDate.HasValue)
                        {
                            ps.PayItemStatusCode = "A";
                        }
                        else
                        {
                            if (payItemMaster.ObsoleteDate.Value.Date <= DateTime.Now.Date)
                            {
                                ps.PayItemStatusCode = "O";
                            }
                        }
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
                    //if (!isInLre)
                    //{
                    var gs = session.QueryOver<PayItemGroup>()
                        .Where(i => i.GroupTypeCode == "3")
                        .List();


                    foreach (var pl in ps.PayItemPayItemGroups)
                    {
                        pl.Status = "I";
                    }

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

                    
                    //}
                    t.Commit();
                }
            }
        }
    }
}