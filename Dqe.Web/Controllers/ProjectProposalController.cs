using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Reports;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;
using Dqe.Web.Services;
using Project = Dqe.Domain.Model.Project;
using Proposal = Dqe.Domain.Model.Proposal;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.StateReviewer,
        DqeRole.DistrictReviewer,
        DqeRole.Coder,
        DqeRole.MaintenanceEstimator,
        DqeRole.MaintenanceDistrictAdmin,
        DqeRole.AdminReadOnly})]
    public class ProjectProposalController : Controller
    {
        private readonly IWebTransportService _webTransportService;
        private readonly IProjectRepository _projectRepository;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly IMasterFileRepository _masterFileRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IMarketAreaRepository _marketAreaRepository;
        private readonly IProposalRepository _proposalRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IPayItemMasterRepository _payItemMasterRepository;
        private readonly IReportRepository _reportRepository;
        private readonly ISystemParametersRepository _systemParametersRepository;
        private readonly ILreService _lreService;
        private readonly IEnvironmentProvider _environmentProvider;

        public ProjectProposalController
            (
            IWebTransportService webTransportService,
            IProjectRepository projectRepository,
            IDqeUserRepository dqeUserRepository,
            IMasterFileRepository masterFileRepository,
            IMarketAreaRepository marketAreaRepository,
            IProposalRepository proposalRepository,
            ICommandRepository commandRepository,
            ITransactionManager transactionManager,
            IPayItemMasterRepository payItemMasterRepository,
            IReportRepository reportRepository,
            ISystemParametersRepository systemParametersRepository,
            ILreService lreService,
            IEnvironmentProvider environmentProvider
            )
        {
            _webTransportService = webTransportService;
            _projectRepository = projectRepository;
            _dqeUserRepository = dqeUserRepository;
            _masterFileRepository = masterFileRepository;
            _marketAreaRepository = marketAreaRepository;
            _proposalRepository = proposalRepository;
            _commandRepository = commandRepository;
            _transactionManager = transactionManager;
            _payItemMasterRepository = payItemMasterRepository;
            _reportRepository = reportRepository;
            _systemParametersRepository = systemParametersRepository;
            _lreService = lreService;            
            _environmentProvider = environmentProvider;
        }

        [HttpGet]
        public ActionResult GetRecentProject()
        {
            //return new DqeResult(null, JsonRequestBehavior.AllowGet);
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProjectEstimate == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            //sync project header
            var r = GetProject(currentDqeUser.MyRecentProjectEstimate.MyProjectVersion.MyProject.ProjectNumber);
            if (!((DqeResult) r).IsValid()) return r;
            var result = ResultStructureFromSnapshot(currentDqeUser.MyRecentProjectEstimate, currentDqeUser);
            return result;
        }

        [HttpGet]
        public ActionResult GetRecentProposal()
        {
            //return new DqeResult(null, JsonRequestBehavior.AllowGet);
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProposal == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            var result = ResultStructureFromProposal(currentDqeUser.MyRecentProposal, currentDqeUser, string.Empty);
            return result;
        }

        [HttpGet]
        public ActionResult GetLsDbProject(string number)
        {
            var currentUser = (DqeIdentity) User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var proj = _projectRepository.GetDetailProjectForLsBd(number, currentDqeUser);
            return new DqeResult(new { hasDetailProject = proj != null }, JsonRequestBehavior.AllowGet); 
        }

        [HttpGet]
        public ActionResult GetLsDbProposal(string number)
        {
            return null;
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator, DqeRole.StateReviewer,
        DqeRole.DistrictReviewer,
        DqeRole.Coder,
        DqeRole.MaintenanceEstimator,
        DqeRole.MaintenanceDistrictAdmin,
        DqeRole.AdminReadOnly})]
        public ActionResult GetProposal(string number)
        {
            return GetProposal(number, string.Empty);
        }
        
        private ActionResult GetProposal(string number, string successMessage)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            //is proposal in DQE?
            var prop = _proposalRepository.GetWtByNumber(number);
            if (prop == null)
            {
                //create proposal and load projects
                var p = _webTransportService.GetProposal(number);
                if (p == null)
                    throw new InvalidOperationException(string.Format("Proposal {0} was not found in WT", number));
                var pro = AddProposal(p, currentDqeUser);
                if (pro as DqeResult != null)
                {
                    _transactionManager.Abort();
                    return (DqeResult)pro;
                }
                _commandRepository.Add(pro);
                currentDqeUser.SetRecentProposal((Proposal)pro);
                currentDqeUser.SetRecentProject(null);
                return ResultStructureFromProposal((Proposal)pro, currentDqeUser, successMessage);
            }
            else
            {
                //update
                var p = _webTransportService.GetProposal(number);
                if (p == null)
                    throw new InvalidOperationException(string.Format("Proposal {0} was not found in WT", number));
                var propt = prop.GetTransformer();
                propt.LettingDate = p.MyLetting == null ? (DateTime?)null : p.MyLetting.LettingDate;
                propt.District = p.District.Name;
                propt.Description = p.Description;
                prop.Transform(propt, currentDqeUser);
                var proposalCounty = p.County;
                var county = _marketAreaRepository.GetCountyByCode(proposalCounty.Name);
                if (county == null)
                {
                    throw new InvalidOperationException(string.Format("County {0} was not found in DQE", proposalCounty.Name));
                }
                prop.SetCounty(county);
            }
            currentDqeUser.SetRecentProposal(prop);
            return ResultStructureFromProposal(prop, currentDqeUser, successMessage);
        }

        [HttpGet]
        public ActionResult IsProjectSyncedForProposal(int projectId)
        {
            return IsProjectSynced(projectId);
        }

        [HttpGet]
        public ActionResult IsProjectSynced(int projectId)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProjectEstimate == null) return new DqeResult(new { isSynced = false }, JsonRequestBehavior.AllowGet);
            var p = _projectRepository.Get(projectId);
            if (p == null) return new DqeResult(new { isSynced = false }, JsonRequestBehavior.AllowGet);
            var vs = p.ProjectVersions.Where(i => i.VersionOwner == currentDqeUser).Distinct().ToList();
            if (vs.Count == 0) return new DqeResult(new { isSynced = false }, JsonRequestBehavior.AllowGet);
            var v = vs.FirstOrDefault(i => i.ProjectEstimates.Any(ii => ii.IsWorkingEstimate));
            if (v == null) return new DqeResult(new { isSynced = false }, JsonRequestBehavior.AllowGet);
            var est = v.ProjectEstimates.FirstOrDefault(i => i.IsWorkingEstimate);
            if (est == null) return new DqeResult(new { isSynced = false }, JsonRequestBehavior.AllowGet);
            var isSynced = est.IsSyncedWithWt();
            return new DqeResult(new { isSynced }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SyncWorkingEstimate(dynamic estimate)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var estimateId = (int)estimate.projectSnapshotId;
            var p = _projectRepository.GetByEstimateId(estimateId);
            var v = p.ProjectVersions.FirstOrDefault(i => i.ProjectEstimates.Any(ii => ii.Id == estimateId));
            if (v == null) throw new InvalidOperationException("Version for estimate not found");
            var e = v.ProjectEstimates.FirstOrDefault(i => i.Id == estimateId);
            if (e == null) throw new InvalidOperationException("Estimate not found");
            //Sync wTproject specbook - get the forign key to DQET019_MSTR_FILE of the specbook from wT
            var wtp = _webTransportService.ExportProject(p.ProjectNumber);
            int mfid;
            if (!int.TryParse(wtp.SpecBook.Trim(), out mfid))
            {
                throw new InvalidOperationException(string.Format("Could not parse spec year from WT project {0}", wtp.ProjectNumber));
            }
            var mf = _masterFileRepository.GetByFileNumber(mfid);
            e.SyncWithWt(false, currentDqeUser,mf, wtp);
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "Your working estimate is now synchronized with Project Preconstruction" });
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator})]
        public ActionResult DeleteProject(dynamic project)
        {
            var p = _projectRepository.Get(project.id);
            _commandRepository.Remove(p);
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "Project was removed from DQE" });
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator })]
        public ActionResult DeleteProjectSnapshot(dynamic project)
        {
            var p = (Project)_projectRepository.Get(project.id);
            if (p != null)
            {
                var cl = p.GetCurrentSnapshotLabel();
                if (cl == SnapshotLabel.Phase4 || cl == SnapshotLabel.Phase3 || cl == SnapshotLabel.Phase2 || cl == SnapshotLabel.Phase1 || cl == SnapshotLabel.Scope || cl == SnapshotLabel.Initial)
                {
                    p.RemoveLabel(cl, project.removeLabelComment);
                    return GetProject(p.ProjectNumber, "Project snapshot was removed from DQE");
                }
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "There are no snapshot labels to remove" });  
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Project not found" });
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator })]
        public ActionResult DeleteProposal(dynamic proposal)
        {
            var p = (Proposal)_proposalRepository.GetById(proposal.id);
            if (p != null)
            {
                var workingEstimate = _reportRepository.GetReportProposalAndItems(p.ProposalNumber, ReportProposalLevel.WorkingEstimate);
                if (workingEstimate != null)
                {
                    _commandRepository.Remove(workingEstimate);
                }
                var authorizationEstimate = _reportRepository.GetReportProposalAndItems(p.ProposalNumber, ReportProposalLevel.Authorization);
                if (authorizationEstimate != null)
                {
                    _commandRepository.Remove(authorizationEstimate);
                }
                var officialEstimate = _reportRepository.GetReportProposalAndItems(p.ProposalNumber, ReportProposalLevel.Official);
                if (officialEstimate != null)
                {
                    _commandRepository.Remove(officialEstimate);
                }
                foreach (var proj in p.Projects)
                {
                    _commandRepository.Remove(proj);
                }
                _commandRepository.Remove(p);    
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "Proposal and Project(s) were removed from DQE" });
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator })]
        public ActionResult DeleteProposalSnapshot(dynamic proposal)
        {
            var p = (Proposal)_proposalRepository.GetById(proposal.id);
            if (p != null)
            {
                var cl = p.GetCurrentSnapshotLabel();
                if (cl == SnapshotLabel.Official)
                {
                    foreach (var project in p.Projects)
                    {
                        project.RemoveLabel(cl, proposal.removeLabelComment);
                    }
                    var officialEstimate = _reportRepository.GetReportProposalAndItems(p.ProposalNumber, ReportProposalLevel.Official);
                    if (officialEstimate != null)
                    {
                        _commandRepository.Remove(officialEstimate);
                    }
                }
                else if (cl == SnapshotLabel.Authorization)
                {
                    foreach (var project in p.Projects)
                    {
                        project.RemoveLabel(cl, proposal.removeLabelComment);
                        //reset public prices
                        foreach (var version in project.ProjectVersions)
                        {
                            foreach (var estimate in version.ProjectEstimates)
                            {
                                foreach (var cat in estimate.EstimateGroups)
                                {
                                    foreach (var item in cat.ProjectItems)
                                    {
                                        if (item.Price > 0 && item.PublicPrice == 0)
                                        {
                                            item.ResetPublicPrice();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    var authorizationEstimate = _reportRepository.GetReportProposalAndItems(p.ProposalNumber, ReportProposalLevel.Authorization);
                    if (authorizationEstimate != null)
                    {
                        _commandRepository.Remove(authorizationEstimate);
                    }
                }
                else if (cl != SnapshotLabel.Estimator)
                {
                    foreach (var project in p.Projects)
                    {
                        project.RemoveLabel(cl, proposal.removeLabelComment);
                    }
                }
                else
                {
                    cl = p.GetGreatesUnleveledCurrentSnapshotLabel();
                    if (cl == SnapshotLabel.Official)
                    {
                        foreach (var project in p.Projects)
                        {
                            if (project.GetCurrentSnapshotLabel() == cl)
                            {
                                project.RemoveLabel(cl, proposal.removeLabelComment);    
                            }
                        }
                        var officialEstimate = _reportRepository.GetReportProposalAndItems(p.ProposalNumber, ReportProposalLevel.Official);
                        if (officialEstimate != null)
                        {
                            _commandRepository.Remove(officialEstimate);
                        }
                    }
                    else if (cl == SnapshotLabel.Authorization)
                    {
                        foreach (var project in p.Projects)
                        {
                            if (project.GetCurrentSnapshotLabel() == cl)
                            {
                                project.RemoveLabel(cl, proposal.removeLabelComment);
                                //reset public prices
                                foreach (var version in project.ProjectVersions)
                                {
                                    foreach (var estimate in version.ProjectEstimates)
                                    {
                                        foreach (var cat in estimate.EstimateGroups)
                                        {
                                            foreach (var item in cat.ProjectItems)
                                            {
                                                if (item.Price > 0 && item.PublicPrice == 0)
                                                {
                                                    item.ResetPublicPrice();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        var authorizationEstimate = _reportRepository.GetReportProposalAndItems(p.ProposalNumber, ReportProposalLevel.Authorization);
                        if (authorizationEstimate != null)
                        {
                            _commandRepository.Remove(authorizationEstimate);
                        }
                    }
                    else if (cl != SnapshotLabel.Estimator)
                    {
                        foreach (var project in p.Projects)
                        {
                            if (project.GetCurrentSnapshotLabel() == cl)
                            {
                                project.RemoveLabel(cl, proposal.removeLabelComment);
                            }
                        }
                    }
                    else
                    {
                        return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "There are no snapshot labels to remove" });    
                    }
                }
                return GetProposal(p.ProposalNumber, "Proposal snapshot label was removed from DQE or labels cannot be removed because the project estimates are not at the same level.");
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Proposal not found" });
        }


        [HttpPost]
        public ActionResult AuthorizeUser(dynamic user)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var assignmentUser = _dqeUserRepository.GetBySrsId((int)user.id);
            var project = _projectRepository.Get((int)user.project.id);
            if (assignmentUser == null)
            {
                throw new InvalidOperationException(string.Format("User {0} not found", user.name));
            }
            if (project == null)
            {
                throw new InvalidOperationException(string.Format("Project {0} not found", user.project.number));
            }
            currentDqeUser.AssignProjectToUser(project, assignmentUser);
            _commandRepository.Flush();
            var authorizedUsers = project.AssignedUsers.Select(i => new
            {
                id = i.Id,
                name = i.Name,
                district = i.District
            });
            return new DqeResult(authorizedUsers, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "User is now authorized to work on the project" });
        }

        [HttpPost]
        public ActionResult DeauthorizeUser(dynamic user)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var assignmentUser = _dqeUserRepository.Get((int)user.id);
            var project = _projectRepository.Get((int)user.project.id);
            if (assignmentUser == null)
            {
                throw new InvalidOperationException(string.Format("User {0} not found", user.name));
            }
            if (project == null)
            {
                throw new InvalidOperationException(string.Format("Project {0} not found", user.project.number));
            }
            currentDqeUser.UnassignProjectToUser(project, assignmentUser);
            _commandRepository.Flush();
            var authorizedUsers = project.AssignedUsers.Select(i => new
            {
                id = i.Id,
                name = i.Name,
                district = i.District
            });
            return new DqeResult(authorizedUsers, new ClientMessage { Severity = ClientMessageSeverity.Success, text = "User is no longer authorized to work on the project" });
        }

        private ActionResult ResultStructureFromProposal(Proposal prop, DqeUser currentDqeUser, string successMessage)
        {
            var nextSnapshot = prop.GetNextSnapshotLabel();
            var currentSnapshot = prop.GetCurrentSnapshotLabel();
            var authorizationTotal = (Decimal)0;
            var officialTotal = (Decimal) 0;
            if (currentSnapshot == SnapshotLabel.Official || currentSnapshot == SnapshotLabel.Authorization)
            {
                var est = _reportRepository.GetReportProposal(prop.ProposalNumber, ReportProposalLevel.Authorization);
                authorizationTotal = est == null ? 0 : est.Total;
                if (currentSnapshot == SnapshotLabel.Official)
                {
                    est = _reportRepository.GetReportProposal(prop.ProposalNumber, ReportProposalLevel.Official);
                    officialTotal = est == null ? 0 : est.Total;
                }
            }

            return new DqeResult(new
            {
                security = new
                {
                    isSystemAdmin = currentDqeUser.Role == DqeRole.Administrator,
                },
                proposal = new
                {
                    id = prop.Id,
                    wtId = prop.Id,
                    created = prop.Created,
                    lastUpdated = prop.LastUpdated,
                    number = prop.ProposalNumber,
                    description = prop.Description,
                    district = prop.District,
                    county = prop.County.Name,
                    filenumber = prop.Projects.FirstOrDefault().MyMasterFile.FileNumber,
                    lettingDate = prop.LettingDate.HasValue ? prop.LettingDate.Value.ToShortDateString() : string.Empty,
                    comment = prop.Comment,
                    hasCustody = prop.Projects.All(i => i.CustodyOwner == currentDqeUser),
                    canSnapshot = prop.Projects.All(i => i.ProjectHasWorkingEstimateForUser(currentDqeUser)),
                    nextEstimate = DynamicHelper.GetSnapshotLabelString(nextSnapshot),
                    isOfficial = prop.GetCurrentSnapshotLabel() == SnapshotLabel.Official,
                    authorizationTotal = authorizationTotal,
                    officialTotal = officialTotal,
                    removeLabelComment = string.Empty
                },
                projects = prop.Projects.OrderBy(i => i.ProjectNumber).Select(i => new
                {
                    id = i.Id,
                    wtId = i.WtId,
                    number = i.ProjectNumber,
                    district = i.District,
                    description = i.Description,
                    designer = i.DesignerName,
                    county = i.MyCounty.Name,
                    owner = i.CustodyOwner == null ? string.Empty : i.CustodyOwner.Name,
                    hasCustody = i.CustodyOwner == currentDqeUser,
                    label = DynamicHelper.GetSnapshotLabelString(i.GetCurrentSnapshotLabel()),
                    hasWorkingEstimate = i.ProjectHasWorkingEstimateForUser(currentDqeUser)
                })
            },
                new ClientMessage
                {
                    Severity = ClientMessageSeverity.Success,
                    text = successMessage
                },
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetProject(string number)
        {
            return GetProject(number, string.Empty);
        }

        private ActionResult GetProject(string number, string successMessage)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            //is project in DQE?
            var project = _projectRepository.GetByProjectNumber(number);
            if (project == null)
            {
                var r = AddProjectToDqe(number, currentDqeUser);
                if (r as DqeResult != null)
                {
                    _transactionManager.Abort();
                    return (DqeResult)r;
                }
                var p = r as Project;
                if (p == null) throw new InvalidOperationException("Expected project cast");
                currentDqeUser.SetRecentProject(p);
                //does WT have a proposal
                var wtp = _webTransportService.GetProject(p.ProjectNumber);
                if (wtp != null && wtp.MyProposal != null)
                {
                    var result = AddProposal(wtp.MyProposal, currentDqeUser);
                    if (result as Proposal != null)
                    {
                        currentDqeUser.SetRecentProposal((Proposal)result);
                    }
                    if (result as DqeResult != null)
                    {
                        _transactionManager.Abort();
                        return (DqeResult)result;
                    }
                }
                return ResultStructureFromProjectSelection(p, currentDqeUser);
            }
            else
            {
                //TODO: should I check the synchronization status of the proposal here?
                //should the proposal in DQE be matched against the proposal in wT at this point to remove the proposal from DQE its not a match?

                currentDqeUser.SetRecentProject(project);
                LoadLreSnapshotData(project);
                var proposal = project.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
                currentDqeUser.SetRecentProposal(proposal);
                var wtp = _webTransportService.GetProject(project.ProjectNumber);
                if (wtp != null)
                {
                    //sync project
                    int mfid;
                    if (!int.TryParse(wtp.SpecBook.Trim(), out mfid))
                    {
                        throw new InvalidOperationException(string.Format("Could not parse spec year from WT project {0}", wtp.ProjectNumber));
                    }
                    var mf = _masterFileRepository.GetByFileNumber(mfid);
                    if (mf == null)
                    {
                        return new DqeResult(null,
                            new ClientMessage
                            {
                                Severity = ClientMessageSeverity.Error,
                                text = string.Format("DQE does not contain Master File {0} for Project {1}", mfid, number)
                            },
                            JsonRequestBehavior.AllowGet);
                    }
                    //Sync wTproject specbook - get the forign key to DQET019_MSTR_FILE of the specbook from wT
                    mf.AddProject(project, currentDqeUser);
                    //var p = new Project(_projectRepository, _commandRepository, _webTransportService);
                    var t = project.GetTransformer();
                    t.WtId = wtp.Id;
                    t.Description = wtp.Description;
                    t.ProjectNumber = wtp.ProjectNumber;                    
                    var district = wtp.Districts.FirstOrDefault(i => i.PrimaryDistrict);
                    if (district == null)
                    {
                        _transactionManager.Abort();
                        return new DqeResult(null,
                            new ClientMessage
                            {
                                Severity = ClientMessageSeverity.Error,
                                text = string.Format("Primary District was not found for project {0}", wtp.ProjectNumber)
                            },
                            JsonRequestBehavior.AllowGet);
                        //throw new InvalidOperationException(string.Format("Primary District was not found for project {0}", wtp.ProjectNumber));
                    }
                    t.District = district.MyRefDistrict.Name;
                    //t.LettingDate = wtProjectEstimate.MyProposal == null || wtProjectEstimate.MyProposal.MyLetting == null
                    //    ? (DateTime?)null
                    //    : wtProjectEstimate.MyProposal.MyLetting.LettingDate;
                    project.Transform(t, currentDqeUser);
                    var projectCounty = wtp.Counties.FirstOrDefault(i => i.PrimaryCounty);
                    if (projectCounty == null)
                    {
                        _transactionManager.Abort();
                        return new DqeResult(null,
                            new ClientMessage
                            {
                                Severity = ClientMessageSeverity.Error,
                                text = string.Format("Primary County was not found for project {0}", wtp.ProjectNumber)
                            },
                            JsonRequestBehavior.AllowGet);
                        //throw new InvalidOperationException(string.Format("Primary County was not found for project {0}", wtp.ProjectNumber));
                    }
                    var county = _marketAreaRepository.GetCountyByCode(projectCounty.MyRefCounty.Name);
                    if (county == null)
                    {
                        throw new InvalidOperationException(string.Format("County {0} was not found in DQE", projectCounty.MyRefCounty.Name));
                    }
                    project.SetCounty(county);
                    //check that proposal still exists

                }
                var currentProp = project.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
                if (currentProp == null)
                {

                    if (wtp != null && wtp.MyProposal != null)
                    {
                        var dqeProps = _proposalRepository.GetByNumber(wtp.MyProposal.ProposalNumber);
                        if (dqeProps != null)
                        {
                            var dqeProp = dqeProps.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
                            if (dqeProp != null)
                            {
                                var currentSnapshot = dqeProp.GetCurrentSnapshotLabel();
                                if (currentSnapshot == SnapshotLabel.Authorization || currentSnapshot == SnapshotLabel.Official)
                                {
                                    return new DqeResult(null,
                                    new ClientMessage
                                    {
                                        Severity = ClientMessageSeverity.Error,
                                        text = string.Format("Project {0} was added to proposal {1} in Project Preconstruction and there is currently an Authorization Estimate for proposal {1} in DQE.  Please remove the Authorization Estimate for proposal {1} so DQE can sync the proposal update.", project.ProjectNumber, wtp.MyProposal.ProposalNumber)
                                    },
                                    JsonRequestBehavior.AllowGet);
                                }    
                            }
                        }
                        //create WT proposal in DQE
                        var result = AddProposal(wtp.MyProposal, currentDqeUser);
                        if (result as DqeResult != null)
                        {
                            _transactionManager.Abort();
                            return (DqeResult)result;
                        }
                    }
                }
                else
                {
                    if (wtp != null && wtp.MyProposal != null)
                    {
                        //synchronization error when project disassociated from proposal in wT

                        //updated many-to-many relationship between proposal and project to be non-inverse on both sides

                        //proosal in DQE and WT
                        if (currentProp.ProposalNumber != wtp.MyProposal.ProposalNumber)
                        {
                            var currentSnapshot = currentProp.GetCurrentSnapshotLabel();
                            if (currentSnapshot == SnapshotLabel.Authorization || currentSnapshot == SnapshotLabel.Official)
                            {
                                return new DqeResult(null,
                                new ClientMessage
                                {
                                    Severity = ClientMessageSeverity.Error,
                                    text = string.Format("Proposal has changed from {0} to {1} in Project Preconstruction and there is currently an Authorization Estimate for proposal {0}.  Please remove the Authorization Estimate for proposal {0} so DQE can sync the proposal update.", currentProp.ProposalNumber, wtp.MyProposal.ProposalNumber)
                                },
                                JsonRequestBehavior.AllowGet);
                            }
                            //not same proposal - convert WT proposal to Gaming
                            //currentProp.ConvertToGaming();
                            currentProp.RemoveProjects();
                            //currentProp.RemoveProject(project);
                            _commandRepository.Remove(currentProp);

                            //create WT proposal in DQE
                            var result = AddProposal(wtp.MyProposal, currentDqeUser);
                            if (result as DqeResult != null)
                            {
                                _transactionManager.Abort();
                                return (DqeResult)result;
                            }
                        }
                    }
                    if (wtp != null && wtp.MyProposal == null)
                    {
                        var currentSnapshot = currentProp.GetCurrentSnapshotLabel();
                        if (currentSnapshot == SnapshotLabel.Authorization || currentSnapshot == SnapshotLabel.Official)
                        {
                            return new DqeResult(null,
                            new ClientMessage
                            {
                                Severity = ClientMessageSeverity.Error,
                                text = string.Format("Proposal {0} has been removed from Project Preconstruction and there is currently an Authorization Estimate for proposal {0}.  Please remove the Authorization Estimate for proposal {0} so DQE can sync the proposal update.", currentProp.ProposalNumber)
                            },
                            JsonRequestBehavior.AllowGet);
                        }

                        //proposal in WT removed - convert WT proposal to Gaming
                        //currentProp.ConvertToGaming();
                        currentProp.RemoveProjects();
                        //currentProp.RemoveProject(project);
                        _commandRepository.Remove(currentProp);
                    }
                }
            }
            //in DQE
            return ResultStructureFromProjectSelection(project, currentDqeUser, successMessage);
        }

        private object AddProjectToDqe(string number, DqeUser currentDqeUser)
        {
            //not in DQE
            //var wtProjectEstimate = _webTransportService.ExportProject(number);
            var wtProjectEstimate = _webTransportService.ExportProjectForInitialLoad(number);
            if (wtProjectEstimate == null)
            {
                return new DqeResult(null,
                    new ClientMessage
                    {
                        Severity = ClientMessageSeverity.Error,
                        text = string.Format("Project {0} was not found in Project Preconstruction", number)
                    },
                    JsonRequestBehavior.AllowGet);
            }
            int mfid;
            if (!int.TryParse(wtProjectEstimate.SpecBook.Trim(), out mfid))
            {
                throw new InvalidOperationException(string.Format("Could not parse spec year from WT project {0}",
                    wtProjectEstimate.ProjectNumber));
            }
            var mf = _masterFileRepository.GetByFileNumber(mfid);
            if (mf == null)
            {
                return new DqeResult(null,
                    new ClientMessage
                    {
                        Severity = ClientMessageSeverity.Error,
                        text = string.Format("DQE does not contain Master File {0} for Project {1}", mfid, number)
                    },
                    JsonRequestBehavior.AllowGet);
            }
            var p = new Project(_projectRepository, _commandRepository, _webTransportService);
            var t = p.GetTransformer();
            t.WtId = wtProjectEstimate.Id;
            t.WtLsDbId = wtProjectEstimate.LsDbId;
            t.LsDbCode = string.IsNullOrWhiteSpace(wtProjectEstimate.LsDbCode) ? string.Empty : wtProjectEstimate.LsDbCode;
            t.Description = wtProjectEstimate.Description;
            t.ProjectNumber = wtProjectEstimate.ProjectNumber;
            var district = wtProjectEstimate.Districts.FirstOrDefault(i => i.PrimaryDistrict);
            if (district == null)
            {
                _transactionManager.Abort();
                return new DqeResult(null,
                    new ClientMessage
                    {
                        Severity = ClientMessageSeverity.Error,
                        text = string.Format("Primary District was not found for project {0}", wtProjectEstimate.ProjectNumber)
                    },
                    JsonRequestBehavior.AllowGet);
                //throw new InvalidOperationException(string.Format("Primary District was not found for project {0}", wtProjectEstimate.ProjectNumber));
            }
            t.District =  district.MyRefDistrict.Name;
            //t.LettingDate = wtProjectEstimate.MyProposal == null || wtProjectEstimate.MyProposal.MyLetting == null
            //    ? (DateTime?)null
            //    : wtProjectEstimate.MyProposal.MyLetting.LettingDate;
            p.Transform(t, currentDqeUser);
            //initial load of LRE snapshot data
            var result = LoadLreSnapshotData(p);
            if (result != null) return result;
            
            var projectCounty = wtProjectEstimate.Counties.FirstOrDefault(i => i.PrimaryCounty);
            if (projectCounty == null)
            {
                _transactionManager.Abort();
                return new DqeResult(null,
                        new ClientMessage
                        {
                            Severity = ClientMessageSeverity.Error,
                            text = string.Format("Primary County was not found for project {0}", wtProjectEstimate.ProjectNumber)
                        },
                        JsonRequestBehavior.AllowGet);
                //throw new InvalidOperationException(string.Format("Primary County was not found for project {0}", wtProjectEstimate.ProjectNumber));
            }
            var county = _marketAreaRepository.GetCountyByCode(projectCounty.MyRefCounty.Name);
            if (county == null)
            {
                throw new InvalidOperationException(string.Format("County {0} was not found in DQE", projectCounty.MyRefCounty.Name));
            }
            p.SetCounty(county);
            mf.AddProject(p, currentDqeUser);
            _commandRepository.Add(p);
            return p;
        }

        private DqeResult LoadLreSnapshotData(Project p)
        {
            //set earlier estimate data
            /*
                00 NO LABEL 
                04 STATE REVIEWER 
                05 CONVERSION 
                10 INITIAL LRE ESTIMATE 
                20 SCOPE LRE 
                30 PHASE 1 (30% PLANS) 
                40 PHASE 2 (60% PLANS) 
                50 PHASE 3 (90% PLANS) 
                60 PHASE 4 (100% PLANS) 
                70 AUTHORIZATION ESTIMATE 
             */
            Domain.Model.Lre.Project lreProject = null;

            // 3/4/2021 Service Request 613498 skip update DQE DQET020_PROJ from LRE snapshot phase estimates 1,2,3,4 if DQE project is a LS/DB project number
            // LS/DB project number  example: 01234515201LS
            if (p.ProjectNumber.Length > 11)
                return null;  
 
            var lreNumber = p.ProjectNumber;
            //var lreNumber = p.ProjectNumber.Length > 11 ? p.ProjectNumber.Substring(0, 11) : p.ProjectNumber;
            var lreProjects = _lreService.GetProjects(lreNumber).ToList();
            if (lreProjects.Count == 1)
            {
                lreProject = lreProjects[0];
            }
            if (lreProjects.Count > 1)
            {
                foreach (var project in lreProjects)
                {
                    if (p.District == project.District)
                    {
                        lreProject = project;
                        break;
                    }
                }
                if (lreProject == null)
                {
                    return new DqeResult(null,
                        new ClientMessage
                        {
                            Severity = ClientMessageSeverity.Error,
                            text = string.Format("LRE project {0} has been created in a different district than the Preconstruction assigned District.  The District assignments must match to continue.", lreNumber)
                        },
                        JsonRequestBehavior.AllowGet);
                }
            }
            if (lreProject != null)
            {
                var lreSnapshot = _lreService.GetProjectSnapshot(lreProject.Id);
                if (lreSnapshot != null)
                {
                    var initialEstimate = (decimal?)null;
                    var scopeEstimate = (decimal?)null;
                    var phase1Estimate = (decimal?)null;
                    var phase2Estimate = (decimal?)null;
                    var phase3Estimate = (decimal?)null;
                    var phase4Estimate = (decimal?)null;
                    foreach (var version in lreSnapshot.Versions)
                    {
                        //if (version.ProjectVersionNumber == 0)
                        //{
                        //    //this was written out by DQE
                        //    continue;
                        //}
                        if (version.LabelCode == "10")
                        {
                            initialEstimate = version.Amount;
                            continue;
                        }
                        if (version.LabelCode == "20")
                        {
                            scopeEstimate = version.Amount;
                            continue;
                        }
                        if (version.LabelCode == "30")
                        {
                            phase1Estimate = version.Amount;
                            continue;
                        }
                        if (version.LabelCode == "40")
                        {
                            phase2Estimate = version.Amount;
                            continue;
                        }
                        if (version.LabelCode == "50")
                        {
                            phase3Estimate = version.Amount;
                            continue;
                        }
                        if (version.LabelCode == "60")
                        {
                            phase4Estimate = version.Amount;
                        }    
                    }
                    if (phase4Estimate.HasValue && !phase3Estimate.HasValue)
                    {
                        phase3Estimate = 0;
                    }
                    if (phase3Estimate.HasValue && !phase2Estimate.HasValue)
                    {
                        phase2Estimate = 0;
                    }
                    if (phase2Estimate.HasValue && !phase1Estimate.HasValue)
                    {
                        phase1Estimate = 0;
                    }
                    if (phase1Estimate.HasValue && !scopeEstimate.HasValue)
                    {
                        scopeEstimate = 0;
                    }
                    if (scopeEstimate.HasValue && !initialEstimate.HasValue)
                    {
                        initialEstimate = 0;
                    }
                    p.SetSeedEstimateValues(initialEstimate, scopeEstimate, phase1Estimate, phase2Estimate, phase3Estimate, phase4Estimate);
                }
            }
            return null;
        }

        private object AddProposal(Domain.Model.Wt.Proposal wtp, DqeUser currentDqeUser)
        {
            Proposal prop;
            var existingProps = _proposalRepository.GetByNumber(wtp.ProposalNumber).ToList();
            var existingProp = existingProps.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
            if (existingProp != null)
            {
                prop = existingProp;
            }
            else
            {
                prop = new Proposal(_proposalRepository);
                var propt = prop.GetTransformer();
                propt.ProposalNumber = wtp.ProposalNumber;
                propt.ProposalSource = ProposalSourceType.Wt;
                propt.Comment = "Loaded from Project Preconstruction";
                propt.Description = wtp.Description;
                propt.WtId = wtp.Id;
                propt.LettingDate = wtp.MyLetting == null ? (DateTime?)null : wtp.MyLetting.LettingDate;
                propt.District = wtp.District.Name;
                prop.Transform(propt, currentDqeUser);
                var proposalCounty = wtp.County;
                var county = _marketAreaRepository.GetCountyByCode(proposalCounty.Name);
                if (county == null)
                {
                    throw new InvalidOperationException(string.Format("County {0} was not found in DQE", proposalCounty.Name));
                }
                prop.SetCounty(county);
                  
            }
            var projects = _webTransportService.GetProjectsByProposalId(wtp.Id);
            foreach (var p in projects)
            {
                var ep = _projectRepository.GetByProjectNumber(p.ProjectNumber);
                if (ep != null)
                {
                    ep.AddProposal(prop, currentDqeUser);
                    continue;
                }
                var r = AddProjectToDqe(p.ProjectNumber, currentDqeUser);
                if (r as DqeResult != null) return r;
                if (r as Project != null) ((Project)r).AddProposal(prop, currentDqeUser);
            }
            return prop;  
        }

        [HttpPost]
        public ActionResult CreateProjectVersionFromWt(string number)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var project = _projectRepository.GetByProjectNumber(number);
            if (project == null) throw new InvalidOperationException(string.Format("Project {0} was not found in DQE", number));
            var wtProjectEstimate = _webTransportService.ExportProject(number);
            if (wtProjectEstimate == null)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} was not found in Project Preconstruction", number) }, JsonRequestBehavior.AllowGet);
            }
            //TODO: add call to determine whether to initialize prices from wT
            //var loadPrices = Convert.ToBoolean(ConfigurationManager.AppSettings["loadPrices"]);
            var loadPrices = false;
            if(currentDqeUser.Role != DqeRole.Coder)
            {
                loadPrices = _systemParametersRepository.Get().LoadPrices;
            }


            var ss = project.CreateNewVersionFromWt(string.Empty, wtProjectEstimate, loadPrices
                , currentDqeUser);
            return ResultStructureFromProjectSelection(project, currentDqeUser);
        }

        [HttpPost]
        public ActionResult ReleaseCustody(dynamic project)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.Get((int)project.id);
            if (p.CustodyOwner == currentDqeUser 
                || currentDqeUser.Role == DqeRole.Administrator 
                || (currentDqeUser.Role == DqeRole.DistrictAdministrator && currentDqeUser.IsInDqeDistrict((((int)project.district).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')))))
            {
                p.ReleaseCustody(currentDqeUser);
                return ResultStructureFromProjectSelection(p, currentDqeUser);       
            }
            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} is not owned by you", project.number) }); 
        }

        [HttpPost]
        public ActionResult AquireCustody(dynamic project)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.Get((int)project.id);
            if (p.CustodyOwner != null)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} is currently owned by {1}", project.number, p.CustodyOwner.Name) });
            }
            p.AquireCustody(currentDqeUser);
            return ResultStructureFromProjectSelection(p, currentDqeUser);
        }

        [HttpPost]
        public ActionResult SnapshotWorkingEstimate(dynamic project)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.Get((int)project.id);
            if (p.CustodyOwner != currentDqeUser && currentDqeUser.Role != DqeRole.Coder)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} is not currently owned by {1}", project.number, currentDqeUser.Name) });
            }

            //prevent snapshot of prior let projects
#if !DEBUG
            var en = _environmentProvider.GetEnvironment().ToUpper();
            if (!en.StartsWith("U"))
            {
                if (p.Proposals.Any())
                {
                    var prop = p.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
                    if (prop != null)
                    {
                        if (prop.LettingDate.HasValue)
                        {
                            if (prop.LettingDate.Value.Date < new DateTime(2015, 6, 1).Date)
                            {
                                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} was let prior to 6/1/2015, so a snapshot cannot be taken.", project.number) });
                            }
                        }
                    }
                }
            }
#endif
            //end snapshot prevent

            p.SnapshotWorkingEstimate(currentDqeUser, DynamicHelper.HasNotNullProperty(project, "takeLabeledSnapshot") && project.takeLabeledSnapshot, _lreService);
            return ResultStructureFromProjectSelection(p, currentDqeUser, "Working estimate snapshot created");
        }

        [HttpPost]
        public ActionResult SnapshotProposalWorkingEstimate(dynamic project)
        {
            //var currentUser = (DqeIdentity)User.Identity;
            //var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            //var p = currentDqeUser.MyRecentProposal;
            //if (!currentDqeUser.CanEstimateRecentProposal())
            //{
            //    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Proposal {0} is not currently owned by {1}", proposal.number, currentDqeUser.Name) });
            //}
            //p.SnapshotWorkingEstimate(currentDqeUser, DynamicHelper.HasNotNullProperty(proposal, "takeLabeledSnapshot") && proposal.takeLabeledSnapshot);
            //return ResultStructureFromProposal(p, currentDqeUser);
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.Get((int)project.id);
            if (p.CustodyOwner != currentDqeUser)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} is not currently owned by {1}", project.number, currentDqeUser.Name) });
            }

            //prevent snapshot of prior let projects
#if !DEBUG
            var en = _environmentProvider.GetEnvironment().ToUpper();
            if (!en.StartsWith("U"))
            {
                if (p.Proposals.Any())
                {
                    var prop = p.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
                    if (prop != null)
                    {
                        if (prop.LettingDate.HasValue)
                        {
                            if (prop.LettingDate.Value.Date < new DateTime(2015, 6, 1).Date)
                            {
                                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} was let prior to 6/1/2015, so a snapshot cannot be taken.", project.number) });
                            }
                        }
                    }
                }
            }
#endif
            //end snapshot prevent

            p.SnapshotWorkingEstimate(currentDqeUser, DynamicHelper.HasNotNullProperty(project, "takeLabeledSnapshot") && project.takeLabeledSnapshot, project.comment, true, _lreService);
            var currentLabel = p.GetCurrentSnapshotLabel();
            return new DqeResult(new
            {
                label = currentLabel == SnapshotLabel.Phase2
                    ? "Phase II"
                    : currentLabel == SnapshotLabel.Phase3
                        ? "Phase III"
                        : currentLabel == SnapshotLabel.Phase4
                            ? "Phase IV"
                            : currentLabel == SnapshotLabel.Authorization
                                ? "Authorization"
                                : currentLabel == SnapshotLabel.Official
                                    ? "Official"
                                    : string.Empty
            });
        }

        [HttpPost]
        public ActionResult GetProposalNextSnapshot(dynamic proposal)
        {
            //var currentUser = (DqeIdentity)User.Identity;
            //var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = (Proposal)_proposalRepository.GetById(proposal.id);
            var nextLabel = p == null ? SnapshotLabel.Estimator : p.GetNextSnapshotLabel();
            return new DqeResult(new
            {
                label = nextLabel == SnapshotLabel.Initial ? "Intial"
                : nextLabel == SnapshotLabel.Scope ? "Scope"
                : nextLabel == SnapshotLabel.Phase1 ? "Phase I"
                : nextLabel == SnapshotLabel.Phase2 ? "Phase II"
                : nextLabel == SnapshotLabel.Phase3 ? "Phase III"
                : nextLabel == SnapshotLabel.Phase4 ? "Phase IV"
                : nextLabel == SnapshotLabel.Authorization ? "Authorization"
                : nextLabel == SnapshotLabel.Official ? "Official"
                : string.Empty,
                isOfficial = p != null && p.GetCurrentSnapshotLabel() == SnapshotLabel.Official
            });
        }

        [HttpPost]
        public ActionResult AssignWorkingEstimate(dynamic version)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.GetByVersionId((int)version.projectVersionId);
            if (p.CustodyOwner != currentDqeUser)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} is not currently owned by {1}", p.ProjectNumber, p.CustodyOwner.Name) });
            }
            p.AssignWorkingEstimate(p.ProjectVersions.FirstOrDefault(i => i.Id == version.projectVersionId), currentDqeUser);
            return ResultStructureFromProjectSelection(p, currentDqeUser);
        }

        [HttpPost]
        public ActionResult CreateProjectVersionFromEstimate(dynamic snapshot)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.GetByEstimateId((int)snapshot.projectSnapshotId);
            var sourceVersion = p.ProjectVersions.First(i => i.ProjectEstimates.Any(ii => ii.Id == (int)snapshot.projectSnapshotId));
            var source = sourceVersion.ProjectEstimates.First(i => i.Id == (int)snapshot.projectSnapshotId);
            p.CreateNewVersionFromSnapshot(string.Empty, source, currentDqeUser);
            return ResultStructureFromProjectSelection(p, currentDqeUser);
        }

        /// <summary>
        /// This creates a new Version with a single estimate of type Review 'R'. 
        /// This does NOT update info to LRE, this is intended to be read only (except the notes). MB. 
        /// </summary>
        /// <param name="snapshot">The snapshot in which the review will be based upon</param>
        /// <returns><returns><see cref="ActionResult"/></returns></returns>
        [HttpPost]
        public ActionResult CreateReviewProjectVersionFromEstimate(dynamic snapshot)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.GetByEstimateId((int)snapshot.projectSnapshotId);
            var sourceVersion = p.ProjectVersions.First(i => i.ProjectEstimates.Any(ii => ii.Id == (int)snapshot.projectSnapshotId));
            var source = sourceVersion.ProjectEstimates.First(i => i.Id == (int)snapshot.projectSnapshotId);
            p.CreateNewReviewVersionFromSnapshot(string.Empty, source, currentDqeUser);
            return ResultStructureFromProjectSelection(p, currentDqeUser);
        }

        /// <summary>
        /// This creates a new Version with a single estimate of type Review 'R'. 
        /// This does NOT update info to LRE, this is intended to be read only (except the notes). MB. 
        /// </summary>
        /// <param name="snapshot">The snapshot in which the review will be based upon</param>
        [HttpPost]
        public ActionResult CreateCoderProjectVersionFromWorkingEstimate(dynamic project)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var p = _projectRepository.Get((int)project.id);
            if (p.CustodyOwner != currentDqeUser && currentDqeUser.Role != DqeRole.Coder)
            {
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} is not currently owned by {1}", project.number, currentDqeUser.Name) });
            }

            //prevent snapshot of prior let projects
#if !DEBUG
            var en = _environmentProvider.GetEnvironment().ToUpper();
            if (!en.StartsWith("U"))
            {
                if (p.Proposals.Any())
                {
                    var prop = p.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
                    if (prop != null)
                    {
                        if (prop.LettingDate.HasValue)
                        {
                            if (prop.LettingDate.Value.Date < new DateTime(2015, 6, 1).Date)
                            {
                                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} was let prior to 6/1/2015, so a snapshot cannot be taken.", project.number) });
                            }
                        }
                    }
                }
            }
#endif
            //end snapshot prevent

            p.CoderSnapshotWorkingEstimate(currentDqeUser, _lreService);
            //var currentLabel = p.GetCurrentSnapshotLabel();
            return new DqeResult(new
            {
            });
        }

        [HttpPost]
        public ActionResult SaveComment(dynamic snapshot)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var est = (ProjectEstimate)_projectRepository.GetEstimate(snapshot.projectSnapshotId);
            est.SetComment(snapshot.comment, currentDqeUser);
            //var p = _projectRepository.Get((int)project.id);
            //currentDqeUser.MyRecentProjectEstimate.SetComment(project.comment, currentDqeUser);
            //return ResultStructureFromProjectSelection(p, currentDqeUser);
            return new DqeResult(null);
        }

        [HttpGet]
        public ActionResult GetProjects(string number)
        {
            var projects = _webTransportService.GetProjects(number);
            return Json(projects
                .Select(i => new
                {
                    id = i.Id,
                    number = i.ProjectNumber,
                }),
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetProposals(string number)
        {
            var proposals = _webTransportService.GetProposals(number);
            return Json(proposals
                .Select(i => new
                {
                    id = i.Id,
                    number = i.ProposalNumber,
                }),
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDqeProposals(string proposalNumber, string estimateType)
        {
            var proposals = _proposalRepository.GetProposalByEstimateTypeAndCategory(proposalNumber, estimateType == "A" ? SnapshotLabel.Authorization : SnapshotLabel.Official).ToList();

            if (proposals.Any())
            {
                var proposalsInReporting = _reportRepository.GetProposalsInList(proposals.Select(p => p.ProposalNumber).ToList(), estimateType == "A" ? ReportProposalLevel.Authorization : ReportProposalLevel.Official);
                return Json(proposalsInReporting
                    .Select(i => new
                    {
                        number = i.ProposalNumber
                    }),
                    JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        private ActionResult ResultStructureFromProjectSelection(Project project, DqeUser currentUser, string message = "")
        {
            var wtProposal = project.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
            var version = project.ProjectVersions.Where(i => i.VersionOwner == currentUser).FirstOrDefault(i => i.ProjectEstimates.FirstOrDefault(ii => ii.IsWorkingEstimate) != null);
            ProjectEstimate snapshot = null;
            if (version != null)
            {
                _transactionManager.Flush();
                snapshot = version.ProjectEstimates.FirstOrDefault(ii => ii.IsWorkingEstimate);
            }

            var projectLetting = _webTransportService.GetProjectLetting(project.WtId);

            var result = new DqeResult(new
            {
                security = new
                {
                    role = currentUser.Role,
                    district = project.District,
                    userInDistrict = currentUser.IsInDqeDistrict(project.District),
                    isAuthorized = currentUser.Role == DqeRole.Administrator || currentUser.IsInDqeDistrict(project.District) || currentUser.IsAuthorizedOnProject(project),
                    isSystemAdmin = currentUser.Role == DqeRole.Administrator,
                    isDistrictAdmin = currentUser.Role == DqeRole.DistrictAdministrator && currentUser.IsInDqeDistrict(project.District),
                    isReviewRole = (currentUser.Role == DqeRole.DistrictReviewer && currentUser.IsInDqeDistrict(project.District) ) || (currentUser.Role == DqeRole.StateReviewer),
                    isCoderRole = (currentUser.Role == DqeRole.Coder)
                },
                authorizedUsers = project.AssignedUsers.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    district = i.District
                }),
                project = new
                {
                    id = project.Id,
                    wtId = project.WtId,
                    number = project.ProjectNumber,
                    description = project.Description,
                    district = project.District,
                    designer = project.DesignerName,
                    county = project.MyCounty.Name,
                    filenumber = project.MyMasterFile.FileNumber,
                    comment = snapshot == null ? string.Empty : snapshot.EstimateComment,
                    lettingDate = wtProposal == null 
                        ? projectLetting.HasValue
                            ? projectLetting.Value.ToShortDateString()
                            : string.Empty
                        : wtProposal.LettingDate.HasValue 
                            ? wtProposal.LettingDate.Value.ToShortDateString()
                            : projectLetting.HasValue
                                ? projectLetting.Value.ToShortDateString()
                                : string.Empty,
                    isAvailable = project.CustodyOwner == null,
                    userHasCustody = project.CustodyOwner == currentUser,
                    custodyOwner = project.CustodyOwner == null ? string.Empty : project.CustodyOwner.Name,
                    nextLabel = project.GetNextSnapshotLabel() == SnapshotLabel.Initial ? "Initial"
                        : project.GetNextSnapshotLabel() == SnapshotLabel.Scope ? "Scope"
                        : project.GetNextSnapshotLabel() == SnapshotLabel.Phase1 ? "Phase1"
                        : project.GetNextSnapshotLabel() == SnapshotLabel.Phase2 ? "Phase II"
                        : project.GetNextSnapshotLabel() == SnapshotLabel.Phase3 ? "Phase III"
                        : project.GetNextSnapshotLabel() == SnapshotLabel.Phase4 ? "Phase IV"
                        : string.Empty,
                    isOfficial = project.GetCurrentSnapshotLabel() == SnapshotLabel.Official,
                    removeLabelComment = string.Empty
                },
                proposals = project.Proposals.Select(i => new
                {
                    id = i.Id,
                    wtId = i.WtId,
                    number = i.ProposalNumber,
                    source = i.ProposalSource == ProposalSourceType.Wt ? "Project Preconstruction" : "Gaming",
                    comment = i.Comment,
                    created = i.Created,
                    lastUpdated = i.LastUpdated,
                    filenumber = i.Projects.FirstOrDefault().MyMasterFile.FileNumber
                }),
                workingEstimate = snapshot == null
                ? new
                {
                    projectVersionId = (long)0,
                    projectSnapshotId = (long)0,
                    projectVersion = 0,
                    projectSnapshot = 0,
                    label = string.Empty,
                    created = string.Empty,
                    lastUpdated = string.Empty,
                    estimate = new EstimateTotal(),
                    comment = string.Empty,
                    owner = string.Empty,
                    source = string.Empty
                }
                : new
                {
                    projectVersionId = snapshot.MyProjectVersion.Id,
                    projectSnapshotId = snapshot.Id,
                    projectVersion = snapshot.MyProjectVersion.Version,
                    projectSnapshot = snapshot.Estimate,
                    label = string.Empty,
                    created = string.Format("{0} @ {1}", snapshot.Created.ToShortDateString(), snapshot.Created.ToShortTimeString()),
                    lastUpdated = string.Format("{0} @ {1}", snapshot.LastUpdated.ToShortDateString(), snapshot.LastUpdated.ToShortTimeString()),
                    estimate = snapshot.GetEstimateTotal(),
                    comment = snapshot.EstimateComment,
                    owner = snapshot.MyProjectVersion.VersionOwner.Name,
                    source = snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Lre
                        ? "LRE"
                        : snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Wt
                            ? "Project Preconstruction"
                            : string.Format("Version {0} Estimate {1}", snapshot.MyProjectVersion.EstimateSource.MyProjectVersion.Version, snapshot.MyProjectVersion.EstimateSource.Estimate)
                },
            versions = project
                    .ProjectVersions
                    .Where(v => !(currentUser.Role == DqeRole.Coder) || (v.ProjectEstimates.Any() && v.ProjectEstimates.ToList()[0].Label == SnapshotLabel.Coder)) //if coder user then only pull in coder estimates
                    .OrderByDescending(i => i.Version)
                    .Select(i => new
                    {
                        projectVersionId = i.Id,
                        owner = i.VersionOwner.Name,
                        isCurrentOwner = i.VersionOwner == currentUser,
                        projectVersion = i.Version,
                        versionLabel = i.ProjectSource.ToString(),
                        source = i.ProjectSource == ProjectSourceType.Lre
                            ? "LRE"
                            : i.ProjectSource == ProjectSourceType.Wt
                                ? "Project Preconstruction"
                                : string.Format("Version {0} Estimate {1}", i.EstimateSource.MyProjectVersion.Version, i.EstimateSource.Estimate),
                        hasWorkingEstimate = i.ProjectEstimates.Any(ii => ii.IsWorkingEstimate),
                        snapshots = i
                            .ProjectEstimates
                            .OrderByDescending(ii => ii.Estimate)
                            .Select(ii => new
                            {
                                projectSnapshotId = ii.Id,
                                projectSnapshot = ii.Estimate,
                                created = string.Format("{0} @ {1}", ii.Created.ToShortDateString(), ii.Created.ToShortTimeString()),
                                lastUpdated = string.Format("{0} @ {1}", ii.LastUpdated.ToShortDateString(), ii.LastUpdated.ToShortTimeString()),
                                lastUpdatedRaw = ii.LastUpdated,
                                comment = ii.EstimateComment,
                                estimate = ii.GetEstimateTotal(),
                                snapshotRemoved = ii.LabelRemovedOn.HasValue,
                                snapshotRemovedOn = ii.LabelRemovedOn.HasValue 
                                    ? ii.LabelRemovedOn.Value.ToShortDateString()
                                    : string.Empty,
                                snapshotRemovedComment = string.IsNullOrWhiteSpace(ii.LabelRemovedComment)
                                    ? string.Empty
                                    : ii.LabelRemovedComment,
                                label = DynamicHelper.GetSnapshotLabelString(ii.Label),
                                isWorkingEstimate = ii.IsWorkingEstimate ? "Yes" : string.Empty
                            })
                    })
            },
                new ClientMessage
                {
                    Severity = ClientMessageSeverity.Success,
                    text = string.IsNullOrWhiteSpace(message) ? string.Empty : message
                },
                JsonRequestBehavior.AllowGet);

            return result;
        }

        private ActionResult ResultStructureFromSnapshot(ProjectEstimate snapshot, DqeUser currentUser)
        {
            var wtProposal = snapshot.MyProjectVersion.MyProject.Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
            var projectLetting = _webTransportService.GetProjectLetting(snapshot.MyProjectVersion.MyProject.WtId);
            return new DqeResult(new
            {
                security = new
                {
                    isAuthorized = currentUser.Role == DqeRole.Administrator || currentUser.IsInDqeDistrict(snapshot.MyProjectVersion.MyProject.District) || currentUser.IsAuthorizedOnProject(snapshot.MyProjectVersion.MyProject),
                    isSystemAdmin = currentUser.Role == DqeRole.Administrator,
                    isDistrictAdmin = currentUser.Role == DqeRole.DistrictAdministrator && currentUser.IsInDqeDistrict(snapshot.MyProjectVersion.MyProject.District)    
                },
                authorizedUsers = snapshot.MyProjectVersion.MyProject.AssignedUsers.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    district = i.District
                }),
                project = new
                {
                    id = snapshot.MyProjectVersion.MyProject.Id,
                    wtId = snapshot.MyProjectVersion.MyProject.WtId,
                    number = snapshot.MyProjectVersion.MyProject.ProjectNumber,
                    description = snapshot.MyProjectVersion.MyProject.Description,
                    district = snapshot.MyProjectVersion.MyProject.District,
                    designer = snapshot.MyProjectVersion.MyProject.DesignerName,
                    county = snapshot.MyProjectVersion.MyProject.MyCounty.Name,
                    comment = snapshot.EstimateComment,
                    filenumber = snapshot.MyProjectVersion.MyProject.MyMasterFile.FileNumber,
                    //lettingDate = wtProposal == null ? string.Empty : wtProposal.LettingDate.HasValue ? wtProposal.LettingDate.Value.ToShortDateString() : string.Empty,
                    lettingDate = wtProposal == null
                        ? projectLetting.HasValue
                            ? projectLetting.Value.ToShortDateString()
                            : string.Empty
                        : wtProposal.LettingDate.HasValue
                            ? wtProposal.LettingDate.Value.ToShortDateString()
                            : projectLetting.HasValue
                                ? projectLetting.Value.ToShortDateString()
                                : string.Empty,
                    isAvailable = snapshot.MyProjectVersion.MyProject.CustodyOwner == null,
                    userHasCustody = snapshot.MyProjectVersion.MyProject.CustodyOwner == currentUser,
                    custodyOwner = snapshot.MyProjectVersion.MyProject.CustodyOwner == null ? string.Empty : snapshot.MyProjectVersion.MyProject.CustodyOwner.Name,
                    nextLabel =  DynamicHelper.GetSnapshotLabelString(snapshot.MyProjectVersion.MyProject.GetNextSnapshotLabel())
                },
                proposals = snapshot.MyProjectVersion.MyProject.Proposals.Select(i => new
                {
                    id = i.Id,
                    wtId = i.WtId,
                    number = i.ProposalNumber,
                    source = i.ProposalSource == ProposalSourceType.Wt ? "Project Preconstruction" : "Gaming",
                    comment = i.Comment,
                    created = i.Created,
                    lastUpdated = i.LastUpdated,
                    filenumber = i.Projects.FirstOrDefault().MyMasterFile.FileNumber
                }),
                workingEstimate = new
                {
                    projectVersionId = snapshot.MyProjectVersion.Id,
                    projectSnapshotId = snapshot.Id,
                    projectVersion = snapshot.MyProjectVersion.Version,
                    projectSnapshot = snapshot.Estimate,
                    label = string.Empty,
                    created = string.Format("{0} @ {1}", snapshot.Created.ToShortDateString(), snapshot.Created.ToShortTimeString()),
                    lastUpdated = string.Format("{0} @ {1}", snapshot.LastUpdated.ToShortDateString(), snapshot.LastUpdated.ToShortTimeString()),
                    estimate = snapshot.GetEstimateTotal(),
                    comment = snapshot.EstimateComment,
                    owner = snapshot.MyProjectVersion.VersionOwner.Name,
                    source = snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Lre
                        ? "LRE"
                        : snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Wt
                            ? "Project Preconstruction"
                            : string.Format("Version {0} Estimate {1}", snapshot.MyProjectVersion.EstimateSource.MyProjectVersion.Version, snapshot.MyProjectVersion.EstimateSource.Estimate)
                },
                versions = snapshot
                    .MyProjectVersion
                    .MyProject
                    .ProjectVersions
                    .OrderByDescending(i => i.Version)
                    .Select(i => new
                    {
                        projectVersionId = i.Id,
                        owner = i.VersionOwner.Name,
                        isCurrentOwner = i.VersionOwner == currentUser,
                        projectVersion = i.Version,
                        source = i.ProjectSource == ProjectSourceType.Lre
                            ? "LRE"
                            : i.ProjectSource == ProjectSourceType.Wt
                                ? "Project Preconstruction"
                                : string.Format("Version {0} Estimate {1}", i.EstimateSource.MyProjectVersion.Version, i.EstimateSource.Estimate),
                        hasWorkingEstimate = i.ProjectEstimates.Any(ii => ii.IsWorkingEstimate),
                        snapshots = i
                            .ProjectEstimates
                            .OrderByDescending(ii => ii.Estimate)
                            .Select(ii => new
                            {
                                projectSnapshotId = ii.Id,
                                projectSnapshot = ii.Estimate,
                                created = string.Format("{0} @ {1}", ii.Created.ToShortDateString(), ii.Created.ToShortTimeString()),
                                lastUpdated = string.Format("{0} @ {1}", ii.LastUpdated.ToShortDateString(), ii.LastUpdated.ToShortTimeString()),
                                comment = ii.EstimateComment,
                                estimate = ii.GetEstimateTotal(),
                                snapshotRemoved = ii.LabelRemovedOn.HasValue,
                                snapshotRemovedOn = ii.LabelRemovedOn.HasValue
                                    ? ii.LabelRemovedOn.Value.ToShortDateString()
                                    : string.Empty,
                                snapshotRemovedComment = string.IsNullOrWhiteSpace(ii.LabelRemovedComment)
                                    ? string.Empty
                                    : ii.LabelRemovedComment,
                                label = DynamicHelper.GetSnapshotLabelString(ii.Label),
                                isWorkingEstimate = ii.IsWorkingEstimate ? "Yes" : string.Empty
                            })
                    })
            },
                new ClientMessage
                {
                    Severity = ClientMessageSeverity.Success,
                    text = string.Empty
                },
                JsonRequestBehavior.AllowGet);
        }
    }
}