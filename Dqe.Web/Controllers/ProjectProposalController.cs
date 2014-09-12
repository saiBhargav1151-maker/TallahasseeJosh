using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using Dqe.ApplicationServices;
using Dqe.ApplicationServices.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Wt;
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
        private readonly ICommandRepository _commandRepository;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly IMasterFileRepository _masterFileRepository;

        public ProjectProposalController
            (
            IWebTransportService webTransportService,
            IProjectRepository projectRepository,
            ICommandRepository commandRepository,
            IDqeUserRepository dqeUserRepository,
            IMasterFileRepository masterFileRepository
            )
        {
            _webTransportService = webTransportService;
            _projectRepository = projectRepository;
            _commandRepository = commandRepository;
            _dqeUserRepository = dqeUserRepository;
            _masterFileRepository = masterFileRepository;
        }

        [HttpGet]
        public ActionResult GetRecentProject()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProject == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            return GetProjectList(currentDqeUser.MyRecentProject.ProjectNumber, false, 0, false);
        }

        [HttpGet]
        public ActionResult GetProjectList(string number)
        {
            return GetProjectList(number, false, 0, false);
        }

        [HttpPost]
        public ActionResult CreateProjectFromWt(dynamic wtProject)
        {
            return GetProjectList(wtProject.number.ToString(), true, 0, false);
        }

        [HttpPost]
        public ActionResult CreateProjectFromVersion(dynamic sourceProject)
        {
            return GetProjectList(sourceProject.number.ToString(), true, sourceProject.id, false);
        }

        [HttpPost]
        public ActionResult SnapshotProjectVersion(dynamic sourceProject)
        {
            return GetProjectList(sourceProject.number.ToString(), true, sourceProject.id, true);
        }

        private ActionResult GetProjectList(string number, bool createNewVersion, int sourceId, bool takeSnapshot)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            //load all dqe projects with the project number
            var dqeProjects = _projectRepository.GetAllByNumber(number).ToList();
            //load from WT if necessary - only???
            object wtProject;
            Estimate wtProjectEstimate = null;
            MasterFile mf;
            if (!dqeProjects.Any() || (createNewVersion && sourceId == 0))
            {
                //load from WT
                wtProjectEstimate = _webTransportService.ExportProject(number);
                if (wtProjectEstimate == null)
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("Project {0} was not found in Web Trnsport", number) }, JsonRequestBehavior.AllowGet);
                }
                int mfid;
                if (!int.TryParse(wtProjectEstimate.SpecYear.Trim(), out mfid))
                {
                    throw new InvalidOperationException(string.Format("Could not parse spec year from WT project {0}", wtProjectEstimate.EstimateId));
                }
                mf = _masterFileRepository.GetByFileNumber(mfid);
                if (mf == null)
                {
                    return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = string.Format("DQE does not contain Master File {0} for Project {1}", mfid, number) }, JsonRequestBehavior.AllowGet);
                }
                //create anonymous wtProject for binding project information
                wtProject = new
                {
                    id = 0,
                    number = wtProjectEstimate.EstimateId,
                    description = wtProjectEstimate.Description,
                    county = wtProjectEstimate.County
                };
            }
            else
            {
                wtProject = new
                {
                    id = 0,
                    number = dqeProjects.First().ProjectNumber,
                    description = dqeProjects.First().Description,
                    county = dqeProjects.First().County
                };
                mf = dqeProjects.First().MyMasterFile;
            }
            //sync dqe project's project-level data with WT - in case project data has changed in WT
            //foreach (var dqeProject in dqeProjects)
            //{
            //    var t = dqeProject.GetTransformer();
            //    t.Description = wtProjectEstimate.Description;
            //    t.County = wtProjectEstimate.County;
            //    dqeProject.TransformForSync(t, currentDqeUser);
            //}
            //load working estimate for current user from DQE
            var usersProjects = dqeProjects.Where(i => i.Owner == currentDqeUser);
            var workingVersionNumber = usersProjects.Any() ? dqeProjects.Where(i => i.Owner == currentDqeUser).Max(i => i.Version) : 0;
            var workingVersion = dqeProjects.SingleOrDefault(i => i.Owner == currentDqeUser && i.Version == workingVersionNumber);
            IEnumerable<Project> userVersions = new List<Project>();
            //load all versions (including snapshots) for current user - sort version descending
            if (createNewVersion && sourceId == 0)
            {
                workingVersion = CreateProjectVersionFromWt(wtProjectEstimate, mf, currentDqeUser);
            }
            if (createNewVersion && sourceId != 0)
            {
                var source = _projectRepository.Get(sourceId);
                if (takeSnapshot)
                {
                    source.TakeSnapshot(currentDqeUser);    
                }
                workingVersion = source.VersionProject(currentDqeUser);
            }
            if (workingVersion != null)
            {
                userVersions = dqeProjects.Where(i => i.Owner == currentDqeUser && i.Version != workingVersion.Version).OrderByDescending(i => i.Version).ToList();
            }
            //load all versions (including snapshots) for other users - sort last update date
            var otherVersions = dqeProjects.Where(i => i.Owner != currentDqeUser).OrderByDescending(i => i.LastUpdated).ToList();
            var primary = new object();
            var message = string.Empty;
            if (!userVersions.Any() && !otherVersions.Any())
            {
                primary = new
                {
                    id = 0,
                    number = string.Empty,
                    description = string.Empty,
                    county = string.Empty,
                    version = 0,
                    source = "Web Transport",
                    lastUpdated = "",
                    estimate = "",
                    snapshot = "",
                    created = ""
                };
                message = string.Format("WT Project {0} has not been copied into DQE.", number);
            }
            if (workingVersion != null)
            {
                primary = new
                {
                    id = workingVersion.Id,
                    number = workingVersion.ProjectNumber,
                    description = workingVersion.Description,
                    county = workingVersion.County,
                    version = workingVersion.Version,
                    source = workingVersion.LoadedFromWtOn.HasValue 
                    ? string.Format("Loaded from WT on {0} at {1}", workingVersion.LoadedFromWtOn.Value.ToShortDateString(), workingVersion.LoadedFromWtOn.Value.ToShortTimeString())
                    : string.Format("Copied from District {0} - Owner {1} - Version {2}", workingVersion.MySourceProject.Owner.District, workingVersion.MySourceProject.Owner.Name, workingVersion.MySourceProject.Version),
                    lastUpdated = string.Format("{0} at {1}", workingVersion.LastUpdated.ToShortDateString(), workingVersion.LastUpdated.ToShortTimeString()),
                    estimate = workingVersion.EstimateGroups.Sum(x => x.ProjectItems.Sum(y => y.Price * y.Quantity)),
                    snapshot = workingVersion.Snapshot == SnapshotType.Phase2 ? "Phase II" : workingVersion.Snapshot == SnapshotType.Phase3 ? "Phase III" : string.Empty,
                    created = string.Format("{0} at {1}", workingVersion.Created.ToShortDateString(), workingVersion.Created.ToShortTimeString())
                };
                message = string.Format("DQE Project {0} - Version {1} is now your working version", number, workingVersion.Version);
                currentDqeUser.SetRecentProject(workingVersion);
            }
            var snapshotOption = workingVersion == null
                ? 0
                : dqeProjects.Any(i => i.Snapshot == SnapshotType.Phase2)
                    ? dqeProjects.Any(i => i.Snapshot == SnapshotType.Phase3)
                        ? 0
                        : 2
                    : 1;
            return new DqeResult(new
            {
                snapshot = snapshotOption,
                wtProject,
                workingVersion = primary,
                userVersions = userVersions.Select(i => new
                {
                    id = i.Id,
                    number = i.ProjectNumber,
                    description = i.Description,
                    county = i.County,
                    version = i.Version,
                    lastUpdated = string.Format("{0} at {1}", i.LastUpdated.ToShortDateString(), i.LastUpdated.ToShortTimeString()),
                    source = i.LoadedFromWtOn.HasValue 
                    ? string.Format("Loaded from WT on {0} at {1}", i.LoadedFromWtOn.Value.ToShortDateString(), i.LoadedFromWtOn.Value.ToShortTimeString())
                    : string.Format("Copied from District {0} - Owner {1} - Version {2}", i.MySourceProject.Owner.District, i.MySourceProject.Owner.Name, i.MySourceProject.Version),
                    estimate = i.EstimateGroups.Sum(x => x.ProjectItems.Sum(y => y.Price * y.Quantity)),
                    snapshot = i.Snapshot == SnapshotType.Phase2 ? "Phase II" : i.Snapshot == SnapshotType.Phase3 ? "Phase III" : string.Empty,
                    created = string.Format("{0} at {1}", i.Created.ToShortDateString(), i.Created.ToShortTimeString())
                }),
                otherVersions = otherVersions.Select(i => new
                {
                    id = i.Id,
                    number = i.ProjectNumber,
                    description = i.Description,
                    county = i.County,
                    version = i.Version,
                    owner = string.Format("District {0} User {1}", i.Owner.District, i.Owner.Name),
                    source = i.LoadedFromWtOn.HasValue
                    ? string.Format("Loaded from WT on {0} at {1}", i.LoadedFromWtOn.Value.ToShortDateString(), i.LoadedFromWtOn.Value.ToShortTimeString())
                    : string.Format("Copied from District {0} - Owner {1} - Version {2}", i.MySourceProject.Owner.District, i.MySourceProject.Owner.Name, i.MySourceProject.Version),
                    lastUpdated = string.Format("{0} at {1}", i.LastUpdated.ToShortDateString(), i.LastUpdated.ToShortTimeString()),
                    estimate = i.EstimateGroups.Sum(x => x.ProjectItems.Sum(y => y.Price * y.Quantity)),
                    snapshot = i.Snapshot == SnapshotType.Phase2 ? "Phase II" : i.Snapshot == SnapshotType.Phase3 ? "Phase III" : string.Empty,
                    created = string.Format("{0} at {1}", i.Created.ToShortDateString(), i.Created.ToShortTimeString())
                })
            },
                new ClientMessage
                {
                    Severity = ClientMessageSeverity.Success,
                    text = message
                },
                JsonRequestBehavior.AllowGet);
        }

        private Project CreateProjectVersionFromWt(Estimate estimate, MasterFile mf, DqeUser dqeUser)
        {
            //new load
            var p = new Project(_commandRepository, _projectRepository);
            var t = p.GetTransformer();
            t.County = estimate.County;
            t.Description = estimate.Description;
            t.ProjectNumber = estimate.EstimateId;
            p.Transform(t, dqeUser);
            mf.AddProject(p, dqeUser);
            _commandRepository.Add(p);
            //add project items
            foreach (var group in estimate.EstimateGroup)
            {
                var eg = new EstimateGroup();
                var egt = eg.GetTransformer();
                egt.Description = group.Description;
                eg.Transform(egt, dqeUser);
                p.AddEstimateGroup(eg, dqeUser);
                foreach (var item in group.EstimateItem)
                {
                    var pi = new ProjectItem();
                    var pit = pi.GetTransformer();
                    var q = item.Quantity as XmlNode[];
                    if (q != null && q.Length == 1)
                    {
                        var qq = q[0] as XmlText;
                        if (qq != null)
                        {
                            decimal val;
                            if (decimal.TryParse(qq.Value, out val))
                            {
                                pit.Quantity = val;
                            }
                        }
                    }
                    pit.PayItemNumber = item.ItemCode;
                    pit.PayItemDescription = item.Description;
                    pit.PayItemNumber = item.ItemCode;
                    pit.PayItemDescription = item.Description;
                    pi.Transform(pit, dqeUser);
                    eg.AddProjectItem(pi, dqeUser);
                }
            }
            return p;
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