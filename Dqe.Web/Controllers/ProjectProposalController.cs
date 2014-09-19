using System;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.ApplicationServices.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;
using Project = Dqe.Domain.Model.Project;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
    public class ProjectProposalController : Controller
    {
        private readonly IWebTransportService _webTransportService;
        private readonly IProjectRepository _projectRepository;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly IMasterFileRepository _masterFileRepository;
        private readonly ICommandRepository _commandRepository;

        public ProjectProposalController
            (
            IWebTransportService webTransportService,
            IProjectRepository projectRepository,
            IDqeUserRepository dqeUserRepository,
            IMasterFileRepository masterFileRepository,
            ICommandRepository commandRepository
            )
        {
            _webTransportService = webTransportService;
            _projectRepository = projectRepository;
            _dqeUserRepository = dqeUserRepository;
            _masterFileRepository = masterFileRepository;
            _commandRepository = commandRepository;
        }

        [HttpGet]
        public ActionResult GetRecentProject()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProjectSnapshot == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            return ResultStructureFromSnapshot(currentDqeUser.MyRecentProjectSnapshot, currentDqeUser);
        }

        [HttpGet]
        public ActionResult GetProject(string number)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            //is project in DQE?
            var project  = _projectRepository.GetByProjectNumber(number);
            if (project == null)
            {
                //not in DQE
                var wtProjectEstimate = _webTransportService.ExportProject(number);
                if (wtProjectEstimate == null)
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} was not found in Web Trnsport", number) }, JsonRequestBehavior.AllowGet);
                }
                int mfid;
                if (!int.TryParse(wtProjectEstimate.SpecYear.Trim(), out mfid))
                {
                    throw new InvalidOperationException(string.Format("Could not parse spec year from WT project {0}", wtProjectEstimate.EstimateId));
                }
                var mf = _masterFileRepository.GetByFileNumber(mfid);
                if (mf == null)
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("DQE does not contain Master File {0} for Project {1}", mfid, number) }, JsonRequestBehavior.AllowGet);
                }
                var p = new Project(_projectRepository, _commandRepository);
                var t = p.GetTransformer();
                t.County = wtProjectEstimate.County;
                t.Description = wtProjectEstimate.Description;
                t.ProjectNumber = wtProjectEstimate.EstimateId;
                p.Transform(t, currentDqeUser);
                mf.AddProject(p, currentDqeUser);
                _commandRepository.Add(p);
                return ResultStructureFromProjectSelection(p, currentDqeUser);
            }
            //in DQE
            return ResultStructureFromProjectSelection(project, currentDqeUser);
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
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} was not found in Web Trnsport", number) }, JsonRequestBehavior.AllowGet);
            }
            var ss = project.CreateNewVersionFromWt("Initial Load of Project from WT", wtProjectEstimate, currentDqeUser);
            return ResultStructureFromSnapshot(ss, currentDqeUser);
        }

        private ActionResult ResultStructureFromProjectSelection(Project project, DqeUser currentUser)
        {
            var version = project.ProjectVersions.Where(i => i.VersionOwner == currentUser).FirstOrDefault(i => i.ProjectSnapshots.FirstOrDefault(ii => ii.IsWorkingEstimate) != null);
            ProjectSnapshot snapshot = null;
            if (version != null)
            {
                snapshot = version.ProjectSnapshots.FirstOrDefault(ii => ii.IsWorkingEstimate);
            }
            return new DqeResult(new
            {
                project = new
                {
                    id = project.Id,
                    number = project.ProjectNumber,
                    description = project.Description,
                    district = project.District,
                    county = project.County,
                    lettingDate = project.LettingDate,
                    isAvailable = project.CustodyOwner == null,
                    userHasCustody = project.CustodyOwner == currentUser
                },
                workingEstimate = snapshot == null 
                ? new {
                    projectVersionId = 0,
                    projectSnapshotId = 0,
                    projectVersion = 0,
                    projectSnapshot = 0,
                    label = string.Empty,
                    created = string.Empty,
                    lastUpdated = string.Empty,
                    estimate = (decimal)0,
                    comment = string.Empty,
                    owner = string.Empty,
                    source = string.Empty 
                } 
                : new {
                    projectVersionId = snapshot.MyProjectVersion.Id,
                    projectSnapshotId = snapshot.Id,
                    projectVersion = snapshot.MyProjectVersion.Version,
                    projectSnapshot = snapshot.Snapshot,
                    label = string.Empty,
                    created = snapshot.Created.ToLongDateString(),
                    lastUpdated = snapshot.LastUpdated.ToLongDateString(),
                    estimate = snapshot.EstimateGroups.Sum(i => i.ProjectItems.Sum(ii => ii.Price)),
                    comment = snapshot.SnapshotComment,
                    owner = snapshot.MyProjectVersion.VersionOwner.Name,
                    source = snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Lre
                        ? "LRE"
                        : snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Wt
                            ? "Web TransPort"
                            : string.Format("Snapshot Version {0} Snapshot {1}", snapshot.MyProjectVersion.SnapshotSource.MyProjectVersion.Version, snapshot.MyProjectVersion.SnapshotSource.Snapshot)
                },
                versions = project
                    .ProjectVersions
                    .OrderBy(i => i.Version)
                    .Select(i => new
                    {
                        projectVersionId = i.Id,
                        owner = i.VersionOwner.Name,
                        projectVersion = i.Version,
                        source = i.ProjectSource == ProjectSourceType.Lre
                            ? "LRE"
                            : i.ProjectSource == ProjectSourceType.Wt
                                ? "Web Trns*port"
                                : string.Format("Snapshot Version {0} Snapshot {1}", i.SnapshotSource.MyProjectVersion.Version, i.SnapshotSource.Snapshot),
                        snapshots = i
                            .ProjectSnapshots
                            .OrderBy(ii => ii.Snapshot)
                            .Select(ii => new
                            {
                                projectSnapshotId = ii.Id,
                                projectSnapshot = ii.Snapshot,
                                created = ii.Created.ToLongDateString(),
                                lastUpdated = ii.LastUpdated.ToLongDateString(),
                                comment = ii.SnapshotComment,
                                estimate = ii.EstimateGroups.Sum(iii => iii.ProjectItems.Sum(iiii => iiii.Price)),
                                label = ii.Label == SnapshotLabel.Phase2
                                        ? "Phase II"
                                        : ii.Label == SnapshotLabel.Phase3 ? "Phase III" : string.Empty
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

        private ActionResult ResultStructureFromSnapshot(ProjectSnapshot snapshot, DqeUser currentUser)
        {
            return new DqeResult(new
            {
                project = new
                {
                    id = snapshot.MyProjectVersion.MyProject.Id,
                    number = snapshot.MyProjectVersion.MyProject.ProjectNumber,
                    description = snapshot.MyProjectVersion.MyProject.Description,
                    district = snapshot.MyProjectVersion.MyProject.District,
                    county = snapshot.MyProjectVersion.MyProject.County,
                    lettingDate = snapshot.MyProjectVersion.MyProject.LettingDate,
                    isAvailable = snapshot.MyProjectVersion.MyProject.CustodyOwner == null,
                    userHasCustody = snapshot.MyProjectVersion.MyProject.CustodyOwner == currentUser
                },
                workingEstimate = new
                {
                    projectVersionId = snapshot.MyProjectVersion.Id,
                    projectSnapshotId = snapshot.Id,
                    projectVersion = snapshot.MyProjectVersion.Version,
                    projectSnapshot = snapshot.Snapshot,
                    label = string.Empty,
                    created = snapshot.Created.ToLongDateString(),
                    lastUpdated = snapshot.LastUpdated.ToLongDateString(),
                    estimate = snapshot.EstimateGroups.Sum(i => i.ProjectItems.Sum(ii => ii.Price)),
                    comment = snapshot.SnapshotComment,
                    owner = snapshot.MyProjectVersion.VersionOwner.Name,
                    source = snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Lre
                        ? "LRE"
                        : snapshot.MyProjectVersion.ProjectSource == ProjectSourceType.Wt
                            ? "Web Trns*port"
                            : string.Format("Snapshot Version {0} Snapshot {1}", snapshot.MyProjectVersion.SnapshotSource.MyProjectVersion.Version, snapshot.MyProjectVersion.SnapshotSource.Snapshot)
                },
                versions = snapshot
                    .MyProjectVersion
                    .MyProject
                    .ProjectVersions
                    .OrderBy(i => i.Version)
                    .Select(i => new
                    {
                        projectVersionId = i.Id,
                        owner = i.VersionOwner.Name,
                        projectVersion = i.Version,
                        source = i.ProjectSource == ProjectSourceType.Lre
                            ? "LRE"
                            : i.ProjectSource == ProjectSourceType.Wt
                                ? "Web Trns*port"
                                : string.Format("Snapshot Version {0} Snapshot {1}", i.SnapshotSource.MyProjectVersion.Version, i.SnapshotSource.Snapshot),
                        snapshots = i
                            .ProjectSnapshots
                            .OrderBy(ii => ii.Snapshot)
                            .Select(ii => new
                            {
                                projectSnapshotId = ii.Id,
                                projectSnapshot = ii.Snapshot,
                                created = ii.Created.ToLongDateString(),
                                lastUpdated = ii.LastUpdated.ToLongDateString(),
                                comment = ii.SnapshotComment,
                                estimate = ii.EstimateGroups.Sum(iii => iii.ProjectItems.Sum(iiii => iiii.Price)),
                                label = ii.Label == SnapshotLabel.Phase2
                                        ? "Phase II"
                                        : ii.Label == SnapshotLabel.Phase3 ? "Phase III" : string.Empty
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

        [HttpGet]
        public ActionResult GetProjects(string number)
        {            
            return Json(_webTransportService.GetProjects(number)
                .Select(i => new
                {
                    id = i.Id,
                    number = i.ProjectNumber,
                }),
                JsonRequestBehavior.AllowGet);
        }
    }
}