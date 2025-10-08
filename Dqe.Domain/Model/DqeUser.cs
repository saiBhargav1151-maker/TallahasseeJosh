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
        private readonly ICollection<Project> _assignedProjects;
        private readonly IProposalRepository _proposalRepository;
        private readonly IProjectRepository _projectRepository;
        private string _name;
        private string _racfId;

        public DqeUser(IStaffService staffService, IDqeUserRepository dqeUserRepository, IProposalRepository proposalRepository, IProjectRepository projectRepository)
        {
            _staffService = staffService;
            _dqeUserRepository = dqeUserRepository;
            _proposalRepository = proposalRepository;
            _projectRepository = projectRepository;
            _assignedProjects = new Collection<Project>();
        }

        public virtual int SrsId { get; protected internal set; }

        public virtual DqeRole Role { get; protected internal set; }

        [Required]
        [StringLength(2)]
        public virtual string District { get; protected internal set; }

        [Required]
        [StringLength(1)]
        public virtual string CostGroupAuthorization { get; protected internal set; }

        public virtual ProjectEstimate MyRecentProjectEstimate { get; protected internal set; }

        public virtual Proposal MyRecentProposal { get; protected internal set; }

        public virtual IEnumerable<Project> AssignedProjects
        {
            get { return _assignedProjects.ToList().AsReadOnly(); }
        } 

        public virtual void SetRecentProposal(Proposal proposal)
        {
            MyRecentProposal = proposal;
        }

        public virtual bool IsInDqeDistrict(string wtDistrict)
        {
            switch (wtDistrict)
            {
                case "01":
                    return District == "D1";
                case "02":
                    return District == "D2";
                case "03":
                    return District == "D3";
                case "04":
                    return District == "D4";
                case "05":
                    return District == "D5";
                case "06":
                    return District == "D6";
                case "07":
                    return District == "D7";
                case "08":
                    return District == "TP";
                default:
                    return District == "CO";
            }
        }

        public virtual bool IsAuthorizedOnProject(Project project)
        {
            return _assignedProjects.Contains(project);
        }

        public virtual void SetRecentProject(Project project)
        {
            if (project == null)
            {
                MyRecentProjectEstimate = null;
                return;
            }

            List<ProjectVersion> versions = new List<ProjectVersion>();
            if (Role == DqeRole.Administrator || Role == DqeRole.AdminReadOnly)
            {
                versions = project.ProjectVersions.ToList();
            }
            else
            {
                versions = project.ProjectVersions.Where(i => i.VersionOwner == this).ToList();
            }
            
            if (versions.Count <= 0) return;
            ProjectEstimate estimate = null;

            //check each version for a working estimate flag, get the latest occurance
            foreach (var v in versions.OrderBy(v=> v.Version))
            {
                if(v.ProjectEstimates.Any(i => i.IsWorkingEstimate))
                {
                    //mark
                    estimate = v.ProjectEstimates.FirstOrDefault(e => e.IsWorkingEstimate);
                    break;
                }
            }

            MyRecentProjectEstimate = estimate;
        }

        public virtual bool CanEstimateRecentProposal()
        {
            if (MyRecentProposal == null) return false;

            //var proposal = _proposalRepository.GetById(MyRecentProposal.Id);

            var projects = _projectRepository.GetByProposalId(MyRecentProposal.Id).ToList();

            if (Role != DqeRole.Administrator && !IsInDqeDistrict(MyRecentProposal.District)) return false;
            var hasCustodyAndEstimate = projects.All(i => i.CustodyOwner == this) && projects.All(i => i.ProjectHasWorkingEstimateForUser(this));
            if (!hasCustodyAndEstimate) return false;
            //var projects = proposal.Projects.Where(i => i.ProjectVersions.Any(ii => ii.VersionOwner == this));

            

            foreach (var project in projects)
            {
                if (Role != DqeRole.Administrator && !IsInDqeDistrict(project.District) && !IsAuthorizedOnProject(project)) return false;
                var versions = project.ProjectVersions.Where(i => i.ProjectEstimates.Any(ii => ii.IsWorkingEstimate));
                foreach (var version in versions)
                {
                    var estimate = version.ProjectEstimates.FirstOrDefault(ii => ii.IsWorkingEstimate);
                    if (estimate == null) return false;
                    if (!estimate.IsSyncedWithWt()) return false;
                }
            }
            return true;
        }

        public virtual bool CanTotalProposal(Proposal proposal)
        {
            if (proposal == null) return false;
            if (Role != DqeRole.Administrator && !IsInDqeDistrict(proposal.District)) return false;
            var hasCustodyAndEstimate = proposal.Projects.All(i => i.ProjectHasWorkingEstimateForUser(this));
            if (!hasCustodyAndEstimate) return false;
            var projects = proposal.Projects.Where(i => i.ProjectVersions.Any(ii => ii.VersionOwner == this));
            foreach (var project in projects)
            {
                if (Role != DqeRole.Administrator && !IsInDqeDistrict(project.District) && !IsAuthorizedOnProject(project)) return false;
                var versions = project.ProjectVersions.Where(i => i.ProjectEstimates.Any(ii => ii.IsWorkingEstimate));
                foreach (var version in versions)
                {
                    var estimate = version.ProjectEstimates.FirstOrDefault(ii => ii.IsWorkingEstimate);
                    if (estimate == null) return false;
                    if (!estimate.IsSyncedWithWt()) return false;
                }
            }
            return true;
        }

        public virtual void AssignProjectToUser(Project project , DqeUser assignmentUser)
        {
            if (Role == DqeRole.Administrator || ((Role == DqeRole.DistrictAdministrator || Role == DqeRole.MaintenanceDistrictAdmin) && IsInDqeDistrict(project.District)))
            {
                if (assignmentUser._assignedProjects.Contains(project)) return;
                assignmentUser._assignedProjects.Add(project);
            }
            else
            {
                throw new SecurityException("Not Authorized");
            }
        }

        public virtual void UnassignProjectToUser(Project project, DqeUser assignmentUser)
        {
            if (Role == DqeRole.Administrator || ((Role == DqeRole.DistrictAdministrator || Role == DqeRole.MaintenanceDistrictAdmin) && IsInDqeDistrict(project.District)))
            {
                if (!assignmentUser._assignedProjects.Contains(project)) return;
                assignmentUser._assignedProjects.Remove(project);
            }
            else
            {
                throw new SecurityException("Not Authorized");
            }
        }

        public virtual bool CanEstimateRecentProject()
        {
            if (MyRecentProjectEstimate == null) return false;
            if (Role != DqeRole.Administrator && !IsInDqeDistrict(MyRecentProjectEstimate.MyProjectVersion.MyProject.District) && !IsAuthorizedOnProject(MyRecentProjectEstimate.MyProjectVersion.MyProject)) return false;
            return (MyRecentProjectEstimate.MyProjectVersion.MyProject.CustodyOwner == this);
        }

        public virtual string Name
        {
            get
            {
                if (_name == null)
                {
                    var staff = _staffService.GetStaffById(SrsId);
                    _name = staff == null ? null : staff.FullName;    
                }
                return _name;
            }
        }

        public virtual string RacfId
        {
            get
            {
                if (_racfId == null)
                {
                    var staff = _staffService.GetStaffById(SrsId);
                    _racfId = staff.UserId;
                }
                return _racfId;
            }
        }

        public virtual bool IsActive { get; protected internal set; }

        /// <summary>
        /// Given an abbreviation, return the Role name as a string
        /// </summary>
        /// <param name="abbreviation">Char</param>
        /// <returns></returns>
        public static string GetRoleNameString(string abbreviation)
        {
            if (Enum.TryParse<DqeRole>(abbreviation, out DqeRole result))
            {
                return Enum.Parse(typeof(DqeRole), abbreviation, true).ToString();
            }
            return string.Empty;
        }

      
        /// <summary>
        /// THIS FUNCTION TAKES A LOT OF TIME TO RUN on dev. MB.
        /// </summary>
        /// <returns></returns>
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
                CostGroupAuthorization = CostGroupAuthorization,
                RoleAsString = Helper.GetRoleDisplayLabel(Role)
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
                CostGroupAuthorization = "N";
                SrsId = 0;
                return;
            }
            if (account.Role != DqeRole.System 
                && account.Role != DqeRole.Administrator 
                && account.Role != DqeRole.DistrictAdministrator
                && account.Role != DqeRole.MaintenanceDistrictAdmin
                && account.Role != DqeRole.AdminReadOnly
                && account.Role != DqeRole.DistrictReviewer
                )
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
                    && transformer.Role != DqeRole.PayItemAdministrator
                    && transformer.Role != DqeRole.Estimator
                    && transformer.Role != DqeRole.StateReviewer
                    && transformer.Role != DqeRole.Coder
                    && transformer.Role != DqeRole.AdminReadOnly
                     && transformer.Role != DqeRole.DistrictReviewer
                     && transformer.Role != DqeRole.MaintenanceDistrictAdmin
                     && transformer.Role != DqeRole.MaintenanceEstimator)
                {
                    throw new InvalidOperationException(string.Format("Role {0} is invalid for CO.", transformer.Role));
                }
            }
            else
            {
                if (transformer.Role != DqeRole.DistrictAdministrator 
                    && transformer.Role != DqeRole.Estimator 
                    && transformer.Role != DqeRole.Estimator 
                    && transformer.Role != DqeRole.DistrictReviewer 
                    && transformer.Role != DqeRole.StateReviewer 
                    && transformer.Role != DqeRole.MaintenanceDistrictAdmin 
                    && transformer.Role != DqeRole.MaintenanceEstimator)
                {
                    throw new InvalidOperationException(string.Format("Role {0} is invalid for district {1}.", transformer.Role, transformer.District));
                }
            }
            if (account.Role == DqeRole.System
                || account.Role == DqeRole.Administrator)
            {
                CostGroupAuthorization = transformer.CostGroupAuthorization;
            }
            else
            {
                CostGroupAuthorization = string.IsNullOrWhiteSpace(transformer.CostGroupAuthorization) ? "N" : transformer.CostGroupAuthorization;
            }
            IsActive = transformer.IsActive;
            Role = transformer.Role;
            District = transformer.District;
            SrsId = transformer.SrsId;
        }
    }
}