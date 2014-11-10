using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;
using Dqe.Web.Services;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new[] { DqeRole.Administrator })]
    public class MarketAreaController : Controller
    {
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IMarketAreaRepository _marketAreaRepository;
        private readonly ITransactionManager _transactionManager;

        public MarketAreaController
            (
            IDqeUserRepository dqeUserRepository,
            IMarketAreaRepository marketAreaRepository,
            ICommandRepository commandRepository,
            ITransactionManager transactionManager
            )
        {
            _dqeUserRepository = dqeUserRepository;
            _marketAreaRepository = marketAreaRepository;
            _commandRepository = commandRepository;
            _transactionManager = transactionManager;
        }

        [HttpGet]
        public ActionResult GetMarketAreas()
        {
            return GetMarketAreasResult();
        }

        public ActionResult GetUnassignedCounties()
        {
            var counties = _marketAreaRepository.GetUnassignedCounties();
            return new DqeResult(new
            {
                counties = counties.Select(i => new
                {
                    id = i.Id,
                    name = i.Name
                })
            }, new ClientMessage
            {
                Severity = ClientMessageSeverity.Success,
                text = string.Empty
            }, JsonRequestBehavior.AllowGet);
        }

        private ActionResult GetMarketAreasResult()
        {
            var marketAreas = _marketAreaRepository.GetAllMarketAreas();
            return new DqeResult(new
            {
                marketAreas = marketAreas.Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    counties = i.Counties.Select(ii => new
                    {
                        id = ii.Id,
                        name = ii.Name,
                        code = ii.Code
                    })
                })
            }, new ClientMessage
            {
                Severity = ClientMessageSeverity.Success,
                text = string.Empty
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddMarketArea(dynamic marketArea)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var ma = new MarketArea(_marketAreaRepository);
            var t = ma.GetTransformer();
            t.Name = marketArea.name.ToString();
            ma.Transform(t, currentDqeUser);
            var r = EntityValidator.Validate(_transactionManager, ma);
            if (r != null) return r;
            _commandRepository.Add(ma);
            return GetMarketAreasResult();
        }

        [HttpPost]
        public ActionResult AddCounty(dynamic marketArea)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var ma = _marketAreaRepository.GetMarketAreaById((int)marketArea.id);
            var county = _marketAreaRepository.GetCountyById((int)marketArea.newCounty.id);
            ma.AddCounty(county, currentDqeUser);
            return GetMarketAreasResult();
        }

        [HttpPost]
        public ActionResult RemoveCounty(dynamic county)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var c = _marketAreaRepository.GetCountyById((int)county.id);
            c.RemoveFromMarketArea(currentDqeUser);
            return GetMarketAreasResult();
        }

        [HttpPost]
        public ActionResult RemoveMarketArea(dynamic marketArea)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            var m = _marketAreaRepository.GetMarketAreaById((int)marketArea.id);
            m.RemoveCounties(currentDqeUser);
            _commandRepository.Remove(m);
            return GetMarketAreasResult();
        }
    }
}