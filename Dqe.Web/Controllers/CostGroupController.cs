using System;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.ActionResults;
using Dqe.Web.Attributes;

namespace Dqe.Web.Controllers
{
    [RemoteRequireHttps]
    [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
    public class CostGroupController : Controller
    {
        private readonly ICostGroupRepository _costGroupRepository;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IPayItemMasterRepository _payItemMasterRepository;

        public CostGroupController(ICostGroupRepository costGroupRepository, IDqeUserRepository dqeUserRepository, ICommandRepository commandRepository, IPayItemMasterRepository payItemMasterRepository)
        {
            _costGroupRepository = costGroupRepository;
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
            _payItemMasterRepository = payItemMasterRepository;
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetCostGroups()
        {
            var costGroups = _costGroupRepository.GetAllCostGroupsWithPayItems();

            var cGroups = costGroups.Select(i => new
            {
                id = i.Id,
                name = i.Name,
                description = i.Description,
                unit = i.Unit,
                payItems = i.PayItems.Select(c => new
                {
                    id = c.Id,
                    conversionFactor = c.ConversionFactor,
                    payItemId = c.MyPayItem.Id,
                    payItem = c.MyPayItem.RefItemName,
                    payItemDescription = c.MyPayItem.Description,
                    payItemUnit = c.MyPayItem.Unit,
                    specBook = c.MyPayItem.SpecBook
                })
            });

            return new DqeResult(cGroups, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult GetPayItems(string payItemName, int costGroupId)
        {
            var payItems = _payItemMasterRepository.PayItemSearchByName(payItemName).ToList();

            for (var i = payItems.Count() - 1; i >= 0; i--)
            {
                if (payItems[i].CostGroups.Any(costGroup => costGroup.MyCostGroup.Id == costGroupId))
                    payItems.Remove(payItems[i]);
            }

            var result = payItems.Select(i => new
            {
                id = i.Id,
                specBook = i.SpecBook,
                payItemName = i.RefItemName
            });

            return new DqeResult(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult SaveCostGroup(dynamic costGroup)
        {
            if (costGroup.name == null || string.IsNullOrWhiteSpace(costGroup.name))
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Cost Group Name is Required" });

            if (costGroup.description == null || string.IsNullOrWhiteSpace(costGroup.description))
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Cost Group Description is Required" });

            if (costGroup.unit == null || string.IsNullOrWhiteSpace(costGroup.unit))
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Cost Group Unit is Required" });

            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);

            var cg = new CostGroup();
            var cgt = cg.GetTransformer();
            cgt.Name = ((string)costGroup.name).ToUpper();
            cgt.Description = ((string)costGroup.description).ToUpper();
            cgt.Unit = ((string)costGroup.unit).ToUpper();

            cg.Transform(cgt, currentDqeUser);
            _commandRepository.Add(cg);

            var result = new
            {
                id = cg.Id,
                name = cg.Name,
                description = cg.Description,
                unit = cg.Unit
            };

            return new DqeResult(result, new ClientMessage { Severity = ClientMessageSeverity.Success, text = string.Format("Cost Group {0} Added", cg.Name) });
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult EditCostGroup(dynamic costGroup)
        {
            if (costGroup.name == null || string.IsNullOrWhiteSpace(costGroup.name))
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Cost Group Name is Required" });

            if (costGroup.description == null || string.IsNullOrWhiteSpace(costGroup.description))
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Cost Group Description is Required" });

            if (costGroup.unit == null || string.IsNullOrWhiteSpace(costGroup.unit))
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Cost Group Unit is Required" });

            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);

            var cg = _costGroupRepository.Get(costGroup.id);
            var cgt = cg.GetTransformer();
            cgt.Name = ((string)costGroup.name).ToUpper();
            cgt.Description = ((string)costGroup.description).ToUpper();
            cgt.Unit = ((string)costGroup.unit).ToUpper();

            cg.Transform(cgt, currentDqeUser);

            var result = new
            {
                id = cg.Id,
                name = cg.Name,
                description = cg.Description,
                unit = cg.Unit
            };

            return new DqeResult(result, new ClientMessage { Severity = ClientMessageSeverity.Success, text = string.Format("Cost Group {0} Added", cg.Name) });
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult DeleteCostGroup(dynamic costGroup)
        {
            var cg = (CostGroup)_costGroupRepository.Get(costGroup.id);

            _commandRepository.Remove(cg);

            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = string.Format("Cost Group {0} Removed", cg.Name) });
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult RemovePayItem(dynamic payItem)
        {
            var cgpi = (CostGroupPayItem)_costGroupRepository.GetCostGroupPayItem(payItem.id);

            _commandRepository.Remove(cgpi);

            return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Success, text = string.Format("Cost Group Pay Item Removed") });
        }

        [HttpPost]
        [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.PayItemAdministrator })]
        public ActionResult SaveCostGroupPayItem(dynamic costGroupPayItem)
        {
            //if (costGroupPayItem.conversionFactor == null || string.IsNullOrWhiteSpace(costGroupPayItem.conversionFactor.ToString()))  
            if (costGroupPayItem.conversionFactor == null) 
                return new DqeResult(null, new ClientMessage { Severity = ClientMessageSeverity.Error, text = "Conversion Factor is Required" });

            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);

            var cg = (CostGroup)_costGroupRepository.Get(costGroupPayItem.costGroupId);
            var pi = (PayItemMaster)_payItemMasterRepository.Get(costGroupPayItem.payItemId);

            var cgpi = new CostGroupPayItem();
            var cgpit = cgpi.GetTransformer();
            cgpit.ConversionFactor = Convert.ToDecimal(costGroupPayItem.conversionFactor);
            cgpi.Transform(cgpit, currentDqeUser);
            cg.AddPayItem(cgpi);
            pi.AddCostGroup(cgpi);

            _commandRepository.Add(cgpi);

            var result = new
            {
                id = cgpi.Id,
                conversionFactor = cgpi.ConversionFactor,
                payItemId = cgpi.MyPayItem.Id,
                payItem = cgpi.MyPayItem.RefItemName,
                payItemDescription = cgpi.MyPayItem.Description,
                payItemUnit = cgpi.MyPayItem.Unit,
                specBook = cgpi.MyPayItem.SpecBook
            };

            return new DqeResult(result, new ClientMessage { Severity = ClientMessageSeverity.Success, text = string.Format("Cost Group Pay Item Added") });
        }
    }
}