using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
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
    [CustomAuthorize(Roles = new[] { DqeRole.Administrator, DqeRole.DistrictAdministrator, DqeRole.Estimator })]
    public class PricingEngineController : Controller
    {
        private readonly IWebTransportService _webTransportService;
        private readonly IProjectRepository _projectRepository;
        private readonly IDqeUserRepository _dqeUserRepository;
        private readonly IMasterFileRepository _masterFileRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IMarketAreaRepository _marketAreaRepository;
        private readonly IProposalRepository _proposalRepository;
        private readonly ITransactionManager _transactionManager;

        public PricingEngineController
            (
            IWebTransportService webTransportService,
            IProjectRepository projectRepository,
            IDqeUserRepository dqeUserRepository,
            IMasterFileRepository masterFileRepository,
            IMarketAreaRepository marketAreaRepository,
            IProposalRepository proposalRepository,
            ICommandRepository commandRepository,
            ITransactionManager transactionManager
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
        }

        [HttpPost]
        public ActionResult CalculateBidHistory(dynamic item)
        {
            var history = (ApplicationServices.BidHistory)_webTransportService.GetBidHistory(item.itemNumber.ToString(), 0);
            return new DqeResult(ConvertBidHistory(history));
        }

        [HttpPost]
        public ActionResult AsyncCalculateBidHistory(dynamic item)
        {
            var history = _webTransportService.GetBidHistory(item.itemNumber.ToString(), 0);
            return new DqeResult(ConvertBidHistory(history));
        }

        private object ConvertBidHistory(ApplicationServices.BidHistory history)
        {
            return new
            {
                maxBiddersProposal = history.MaxBiddersProposal,
                proposals = history.Proposals.Select(i => new
                {
                    proposal = i.Proposal,
                    county = i.County,
                    include = true,
                    letting = i.Letting.ToShortDateString(),
                    bids = i.Bids.Select(ii => new
                    {
                        blank = ii.IsBlank,
                        price = ii.Price,
                        include = ii.Included,
                        lowCost = ii.IsLowCost,
                        awarded = ii.IsAwarded
                    })
                })
            };
        }
    }
}