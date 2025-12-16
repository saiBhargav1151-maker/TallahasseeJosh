using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;
using Dqe.Web.ActionResults;

namespace Dqe.Web.Controllers
{
    public class DefaultPricingParameterController : Controller
    {
        private readonly IPricingParameterRepository _pricingParameterRepository;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly ICommandRepository _commandRepository;

        public DefaultPricingParameterController(IPricingParameterRepository pricingParameterRepository, IDqeUserRepository dqeUserRepository,
            ICommandRepository commandRepository)
        {
            _pricingParameterRepository = pricingParameterRepository;
            _dqeUserRepository = dqeUserRepository;
            _commandRepository = commandRepository;
        }

        [HttpPost]
        public ActionResult Save(dynamic defaultPricingParameter)
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            PricingParameter pricingParameter = null;
            string successMsg;

            if (defaultPricingParameter.id != null && defaultPricingParameter.id > 0)
            {
                successMsg = "The default pricing parameter has been updated.";
                switch (currentDqeUser.Role)
                {
                    case DqeRole.Administrator:
                        pricingParameter = _pricingParameterRepository.GetStateWideDefault();
                        break;
                    case DqeRole.DistrictAdministrator:
                        pricingParameter = _pricingParameterRepository.GetDistrictDefault(currentDqeUser.District);
                        break;
                    case DqeRole.Estimator:
                        pricingParameter = _pricingParameterRepository.GetEstimatorDefaultByUserId(currentDqeUser);
                        break;
                }
            }
            else
            {
                successMsg = "The default pricing parameter has been added.";
                switch (currentDqeUser.Role)
                {
                    case DqeRole.Administrator:
                        pricingParameter = new StatewidePricingParameter();
                        break;
                    case DqeRole.DistrictAdministrator:
                        pricingParameter = new DistrictPricingParameter();
                        break;
                    case DqeRole.Estimator:
                        pricingParameter = new EstimatorPricingParameter();
                        break;
                }
            }

            Domain.Transformers.PricingParameter transformer = pricingParameter.GetTransformer();

            switch (currentDqeUser.Role)
            {
                case DqeRole.DistrictAdministrator:
                    ((Domain.Transformers.DistrictPricingParameter) transformer).District = currentDqeUser.District;
                    break;
                case DqeRole.Estimator:
                    ((Domain.Transformers.EstimatorPricingParameter) transformer).User = currentDqeUser;
                    break;
                
            }

            transformer.Bidders = (BiddersType)defaultPricingParameter.bidders;
            transformer.ContractType = (ContractType)defaultPricingParameter.contractType;
            transformer.Quantities = (QuantitiesType)defaultPricingParameter.quantities;
            transformer.PricingModel = (PricingModelType)defaultPricingParameter.pricingModel;

            pricingParameter.Transform(transformer,currentDqeUser);

            pricingParameter.ClearWorkTypes(currentDqeUser);

            if (defaultPricingParameter.workTypes != null && defaultPricingParameter.workTypes.Count>0)
            {
                foreach (var workType in defaultPricingParameter.workTypes)
                {
                    pricingParameter.AddWorkType((WorkType)workType,currentDqeUser);
                }
            }

            _commandRepository.Add(pricingParameter);

            return new DqeResult(new { id=pricingParameter.Id,bidders=pricingParameter.Bidders,
                contractType=pricingParameter.ContractType,quantities=pricingParameter.Quantities,
            workTypes=pricingParameter.WorkTypes,pricingModel=pricingParameter.PricingModel},
    new ClientMessage { Severity = ClientMessageSeverity.Success, text = successMsg });
        }

        [HttpGet]
        public ActionResult GetUsersDefaultPricingParameter()
        {
            var currentUser = (DqeIdentity)User.Identity;
            var currentDqeUser = _dqeUserRepository.GetBySrsId(currentUser.SrsId);
            PricingParameter pricingParameter = null;

            switch (currentDqeUser.Role)
            {
                case DqeRole.Administrator:
                    pricingParameter = _pricingParameterRepository.GetStateWideDefault();
                    break;
                case DqeRole.DistrictAdministrator:
                    pricingParameter = _pricingParameterRepository.GetDistrictDefault(currentDqeUser.District);
                    break;
                case DqeRole.Estimator:
                    pricingParameter = _pricingParameterRepository.GetEstimatorDefaultByUserId(currentDqeUser);
                    break;
            }

            if (pricingParameter != null)
            {
                    return Json(GetDefaultObjectForReturn(pricingParameter), JsonRequestBehavior.AllowGet);  
            }
            
            return null;
        }

        public object GetDefaultObjectForReturn(PricingParameter pricingParameter)
        {
            return new
            {
                months = pricingParameter.Months,
                contractType=pricingParameter.ContractType,
                quantities=pricingParameter.Quantities,
                workTypes=pricingParameter.WorkTypes,
                pricingModel=pricingParameter.PricingModel,
                bidders=pricingParameter.Bidders,
                id=pricingParameter.Id
            };
        }
    }
}