using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using Dqe.ApplicationServices;
using Dqe.ApplicationServices.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;

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
        private readonly ITransactionManager _transactionManager;
        private readonly IPayItemRepository _payItemRepository;

        public ProjectProposalController
            (
            IWebTransportService webTransportService,
            IProjectRepository projectRepository,
            ICommandRepository commandRepository,
            IDqeUserRepository dqeUserRepository,
            IMasterFileRepository masterFileRepository,
            ITransactionManager transactionManager,
            IPayItemRepository payItemRepository
            )
        {
            _webTransportService = webTransportService;
            _projectRepository = projectRepository;
            _commandRepository = commandRepository;
            _dqeUserRepository = dqeUserRepository;
            _masterFileRepository = masterFileRepository;
            _transactionManager = transactionManager;
            _payItemRepository = payItemRepository;
        }

        public ActionResult GetRecentProject()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            if (currentDqeUser.MyRecentProject == null)
            {
                return new DqeResult(null, JsonRequestBehavior.AllowGet);
            }
            return new DqeResult(new
            {
                id = currentDqeUser.MyRecentProject.Id,
                number = currentDqeUser.MyRecentProject.ProjectNumber,
                description = currentDqeUser.MyRecentProject.Description,
                county = currentDqeUser.MyRecentProject.County
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetProject(string number)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var dqeProject = _projectRepository.GetByNumber(number);
            if (dqeProject == null)
            {
                //new load
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
                var p = new Project();
                var t = p.GetTransformer();
                t.County = wtProjectEstimate.County;
                t.Description = wtProjectEstimate.Description;
                t.ProjectNumber = wtProjectEstimate.EstimateId;
                p.Transform(t, currentDqeUser);
                mf.AddProject(p, currentDqeUser);
                _commandRepository.Add(p);
                //add project items
                foreach (var group in wtProjectEstimate.EstimateGroup)
                {
                    var eg = new EstimateGroup();
                    var egt = eg.GetTransformer();
                    egt.Description = group.Description;
                    eg.Transform(egt, currentDqeUser);
                    p.AddEstimateGroup(eg, currentDqeUser);
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
                        pit.UnknownPayItemNumber = item.ItemCode;
                        pit.UnknownPayItemDescription = item.Description;
                        pi.Transform(pit, currentDqeUser);
                        eg.AddProjectItem(pi, currentDqeUser);
                        if (item.ItemCode.Length == 10)
                        {
                            var pin = string.Format("{0}-{1}-{2}", item.ItemCode.Substring(0, 4), item.ItemCode.Substring(4, 3), item.ItemCode.Substring(7, 3));
                            var payItem = _payItemRepository.GetByNumberAndMasterFile(pin, mf.FileNumber);
                            if (payItem != null)
                            {
                                payItem.AddProjectItem(pi, currentDqeUser);
                            }
                        }
                        //if (pi.MyPayItem == null)
                        //{
                        //    pit = pi.GetTransformer();
                        //    pit.UnknownPayItemNumber = item.ItemCode;
                        //    pit.UnknownPayItemDescription = item.Description;
                        //    pi.Transform(pit, currentDqeUser);
                        //}
                    }
                }
                dqeProject = p;
            }
            currentDqeUser.SetRecentProject(dqeProject);
            //_transactionManager.Abort();
            return new DqeResult(new
            {
                id = dqeProject.Id,
                number = dqeProject.ProjectNumber,
                description = dqeProject.Description,
                county = dqeProject.County
            },
                new ClientMessage
                {
                    Severity = ClientMessageSeverity.Success,
                    text = string.Format("Project {0} was loaded from Web Trnsport", number)
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