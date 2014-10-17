using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Dqe.Domain.Repositories.Custom;
using Dqe.Domain.Services;

namespace Dqe.Domain.Model
{
    public class DqeUser : Entity<Transformers.DqeUser>
    {
        private readonly IStaffService _staffService;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICollection<Project> _projects;

        public DqeUser(IStaffService staffService, IDqeUserRepository dqeUserRepository)
        {
            _staffService = staffService;
            _dqeUserRepository = dqeUserRepository;
            _projects = new Collection<Project>();
        }

        public virtual int SrsId { get; protected internal set; }

        [Range(1, 6)]
        public virtual DqeRole Role { get; protected internal set; }

        [Required]
        [StringLength(2)]
        public virtual string District { get; protected internal set; }

        public virtual ProjectEstimate MyRecentProjectEstimate { get; protected internal set; }

        public virtual Proposal MyRecentProposal { get; protected internal set; }

        public virtual void SetRecentProposal(Proposal proposal)
        {
            MyRecentProposal = proposal;
        }

        public virtual void SetRecentProject(Project project)
        {
            var versions = project.ProjectVersions.Where(i => i.VersionOwner == this).ToList();
            if (versions.Count <= 0) return;
            var estimate = versions.First(i => i.ProjectEstimates.Any(ii => ii.IsWorkingEstimate)).ProjectEstimates.First(i => i.IsWorkingEstimate);
            MyRecentProjectEstimate = estimate;
        }

        public virtual bool CanEstimateRecentProposal()
        {
            if (MyRecentProposal == null) return false;
            return MyRecentProposal.Projects.All(i => i.CustodyOwner == this) && MyRecentProposal.Projects.All(i => i.ProjectHasWorkingEstimateForUser(this));
        }

        public virtual bool CanEstimateRecentProject()
        {
            if (MyRecentProjectEstimate == null) return false;
            return (MyRecentProjectEstimate.MyProjectVersion.MyProject.CustodyOwner == this);
        }

        public virtual string Name
        {
            get
            {
                var staff = _staffService.GetStaffById(SrsId);
                return staff == null ? string.Empty : staff.FullName;
            }
        }

        public virtual bool IsActive { get; protected internal set; }

        public override Transformers.DqeUser GetTransformer()
        {
            return new Transformers.DqeUser
            {
                Id = Id,
                IsActive = IsActive,
                Role = Role,
                District = District,
                SrsId = SrsId,
                FullName = Name,
                RoleAsString =
                    Role == DqeRole.System
                        ? "System"
                        : Role == DqeRole.Administrator
                            ? "System Administrator"
                            : Role == DqeRole.DistrictAdministrator
                                ? "District Administrator"
                                : Role == DqeRole.PayItemAdministrator
                                    ? "Pay Item Administrator"
                                    : Role == DqeRole.CostBasedTemplateAdministrator
                                        ? "Cost-Based Template Administrator"
                                        : Role == DqeRole.Estimator ? "Estimator" : "No Role"
            };
        }

        public override void Transform(Transformers.DqeUser transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null)
            {
                if (transformer.Role != DqeRole.System) throw new ArgumentNullException("account");
                if (_dqeUserRepository.GetSystemAccount() != null)
                {
                    throw new InvalidOperationException("Only one system account can exist.");
                }
                IsActive = true;
                Role = transformer.Role;
                District = "CO";
                SrsId = 0;
                return;
            }
            if (account.Role != DqeRole.System 
                && account.Role != DqeRole.Administrator 
                && account.Role != DqeRole.DistrictAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (account.Role == DqeRole.DistrictAdministrator)
            {
                if (transformer.Role != DqeRole.DistrictAdministrator && transformer.Role != DqeRole.Estimator)
                {
                    throw new SecurityException(string.Format("District administrator is not authorized to set a role of {0}.", transformer.Role));
                }
                if (transformer.District != account.District)
                {
                    throw new SecurityException(string.Format("District administrator for district {0} is not authorized to set a district of {1}.", account.District, transformer.District));
                }
            }
            if (transformer.District == "CO")
            {
                if (transformer.Role != DqeRole.Administrator 
                    && transformer.Role != DqeRole.CostBasedTemplateAdministrator 
                    && transformer.Role != DqeRole.PayItemAdministrator)
                {
                    throw new InvalidOperationException(string.Format("Role {0} is invalid for CO.", transformer.Role));
                }
            }
            else
            {
                if (transformer.Role != DqeRole.DistrictAdministrator && transformer.Role != DqeRole.Estimator)
                {
                    throw new InvalidOperationException(string.Format("Role {0} is invalid for district {1}.", transformer.Role, transformer.District));
                }
            }
            IsActive = transformer.IsActive;
            Role = transformer.Role;
            District = transformer.District;
            SrsId = transformer.SrsId;
        }
    }
}