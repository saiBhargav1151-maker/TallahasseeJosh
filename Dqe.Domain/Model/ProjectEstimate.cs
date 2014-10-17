using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Dqe.Domain.Fdot;

namespace Dqe.Domain.Model
{
    public class ProjectEstimate : Entity<Transformers.ProjectEstimate>
    {
        private readonly ICollection<EstimateGroup> _estimateGroups;
        private readonly IWebTransportService _webTransportService;

        public ProjectEstimate(IWebTransportService webTransportService)
        {
            _estimateGroups = new Collection<EstimateGroup>();
            _webTransportService = webTransportService;
        }

        [Range(1, int.MaxValue)]
        public virtual int Estimate { get; protected internal set; }

        [StringLength(500)]
        public virtual string EstimateComment { get; protected internal set; }

        public virtual DateTime Created { get; protected internal set; }

        public virtual DateTime LastUpdated { get; set; }

        public virtual bool IsWorkingEstimate { get; set; }

        public virtual SnapshotLabel Label { get; protected internal set; }

        [Required]
        public virtual ProjectVersion MyProjectVersion { get; protected internal set; }

        public virtual IEnumerable<EstimateGroup> EstimateGroups
        {
            get { return _estimateGroups.ToList().AsReadOnly(); }
        }

        public virtual decimal GetEstimateTotal()
        {
            Decimal total = 0;
            var nonAlternates = _estimateGroups.Where(i => i.AlternateSet == string.Empty).ToList();
            total += nonAlternates.Sum(i => i.ProjectItems.Sum(ii => ii.Quantity * ii.Price));
            var alternateSetsAndMembers = _estimateGroups.Where(i => i.AlternateSet != string.Empty)
                    .Select(i => new
                    {
                        Set = i.AlternateSet,
                        Member = i.AlternateMember
                    }).Distinct().ToList();
            var alternateSets = alternateSetsAndMembers.Select(i => i.Set).Distinct().ToList();
            total += alternateSets
                .Select(set => alternateSetsAndMembers
                    .Where(i => i.Set == set)
                    .Select(alternateSetsAndMember => _estimateGroups
                        .Where(i => i.AlternateSet == alternateSetsAndMember.Set && i.AlternateMember == alternateSetsAndMember.Member)
                        .Sum(i => i.ProjectItems.Sum(ii => ii.Quantity*ii.Price))).ToList())
                        .Select(l => l.Min(i => i))
                        .Sum();
            return total;
        }

        public virtual bool IsSyncedWithWt()
        {
            return _webTransportService.IsProjectSynced(this);
        }

        public virtual void SyncWithWt(DqeUser account)
        {
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (MyProjectVersion.VersionOwner != account)
            {
                throw new InvalidOperationException(string.Format("{0} is not the owner of Project {1} Version {2} Estimate {3}", account.Name, MyProjectVersion.MyProject.ProjectNumber, MyProjectVersion.Version, Estimate));
            }
            var project = _webTransportService.ExportProject(MyProjectVersion.MyProject.ProjectNumber);
            if (project == null) return;
            var newEstimate = MyProjectVersion.MyProject.CreateNewVersionFromWt("Synchronized with Web Trns*Port design changes", project, account);
            foreach (var estimateGroup in EstimateGroups)
            {
                var eg = estimateGroup;
                var egMatch = newEstimate
                    .EstimateGroups
                    .Where(i => i.CombineWithLikeItems == eg.CombineWithLikeItems)
                    .Where(i => i.AlternateMember == eg.AlternateMember)
                    .Where(i => i.AlternateSet == eg.AlternateSet)
                    .Where(i => i.Name == eg.Name)
                    .Where(i => i.Description == eg.Description)
                    .FirstOrDefault(i => i.WtId == eg.WtId);
                if (egMatch != null)
                {
                    foreach (var pItem in estimateGroup.ProjectItems)
                    {
                        var pi = pItem;
                        var piMatch = egMatch
                            .ProjectItems
                            .Where(i => i.CombineWithLikeItems == pi.CombineWithLikeItems)
                            .Where(i => i.AlternateMember == pi.AlternateMember)
                            .Where(i => i.AlternateSet == pi.AlternateSet)
                            .Where(i => i.PayItemNumber == pi.PayItemNumber)
                            .FirstOrDefault(i => i.WtId == pi.WtId);
                        if (piMatch != null)
                        {
                            piMatch.Price = pi.Price;
                        }
                    }    
                }
            }
        }

        public virtual void SetComment(string comment, DqeUser account)
        {
            if (comment == null) throw new ArgumentNullException("comment");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (MyProjectVersion.VersionOwner != account)
            {
                throw new InvalidOperationException(string.Format("{0} is not the owner of Project {1} Version {2} Estimate {3}", account.Name, MyProjectVersion.MyProject.ProjectNumber, MyProjectVersion.Version, Estimate));
            }
            EstimateComment = comment;
        }

        protected internal virtual void AddEstimateGroup(EstimateGroup estimateGroup)
        {
            _estimateGroups.Add(estimateGroup);
            estimateGroup.MyProjectEstimate = this;
        }

        public override Transformers.ProjectEstimate GetTransformer()
        {
            throw new NotImplementedException();
        }

        public override void Transform(Transformers.ProjectEstimate transformer, DqeUser account)
        {
            throw new NotImplementedException();
        }
    }
}