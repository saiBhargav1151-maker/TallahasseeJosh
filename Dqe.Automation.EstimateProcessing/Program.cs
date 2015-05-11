using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dqe.ApplicationServices;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure;
using Dqe.Infrastructure.EntityIoC;
using Dqe.Infrastructure.Fdot;
using Dqe.Infrastructure.Messaging;
using Dqe.Infrastructure.Providers;
using Dqe.Infrastructure.Repositories.Custom;
using Dqe.Infrastructure.Services;

namespace Dqe.Automation.EstimateProcessing
{
    class Program
    {
        static void Main()
        {
            Initializer.Initialize();
            EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
            var dqeUserRepository = new DqeUserRepository();
            var environmentProvider = new EnvironmentProvider();
            var environment = environmentProvider.GetEnvironment();

            var webTransportService = new WebTransportService();
            var wtProposals = webTransportService.GetProposalsReadyForOfficialEstimate().ToList();

            var proposalRepository = new ProposalRepository();
            var emailAddresses = new List<string>();
            foreach (var wtProposal in wtProposals)
            {
                var dqeProposal = proposalRepository.GetOfficialProposal(wtProposal.ProposalNumber);

                if (dqeProposal != null)
                {
                    var proposal = proposalRepository.GetById(dqeProposal.Id);
                    var currentDqeUser = dqeUserRepository.GetBySrsId(proposal.Users.First().SrsId);
                    proposal.SetCurrentEstimator(currentDqeUser);
                    var wtp = webTransportService.GetProposal(proposal.ProposalNumber);
                    if (proposal.SynchronizeStructure(wtp, currentDqeUser))
                        webTransportService.UpdatePrices(proposal, true, currentDqeUser);
                    else
                        HandleSynchronizationEmail(dqeUserRepository, proposal, environment, ref emailAddresses);
                }
                else
                    HandleNoOfficialEmail(dqeUserRepository, wtProposal, environment, ref emailAddresses);
            }

            UnitOfWorkProvider.TransactionManager.Commit();
        }

        private static void HandleNoOfficialEmail(IDqeUserRepository dqeUserRepository, Domain.Model.Wt.Proposal wtProposal, string environment, ref List<string> emailAddresses)
        {
            if (!emailAddresses.Any())
                emailAddresses = AcquireEmailAddresses(dqeUserRepository);

            foreach (var emailAddress in emailAddresses)
            {
                var noOfficialEstimate = new NoOfficialEstimateInDqeEmail(emailAddress,
                    wtProposal.ProposalNumber, wtProposal.ProposalStatus == "02" ? "Awarded" : "Executed", environment);

                var messenger = new Messenger();
                messenger.Notify(noOfficialEstimate);
            }
        }

        private static void HandleSynchronizationEmail(IDqeUserRepository dqeUserRepository, Proposal proposal, string environment, ref List<string> emailAddresses)
        {
            if (!emailAddresses.Any())
                emailAddresses = AcquireEmailAddresses(dqeUserRepository);

            foreach (var emailAddress in emailAddresses)
            {
                var officialPriceError = new OfficialPricePushErrorEmail(emailAddress, proposal.ProposalNumber, environment);

                var messenger = new Messenger();
                messenger.Notify(officialPriceError);
            }
        }

        private static List<string> AcquireEmailAddresses(IDqeUserRepository dqeUserRepository)
        {
            var administrators = dqeUserRepository.GetAllSystemAdministrators().ToList();
            var staffService = new StaffService();
            var emailAddresses = new List<string>();
            foreach (var administrator in administrators)
            {
                var userAccount = staffService.GetStaffById(administrator.SrsId);
                if (userAccount != null)
                    emailAddresses.Add(userAccount.Email);
            }
            return emailAddresses;
        }

        private static object[] EntityDependencyResolverOnResolveConstructorArguments(object sender, ResolveConstructorArgumentsArgs args)
        {
            if (args.EntityType.IsAssignableFrom(typeof(DqeUser))) return new object[] { new StaffService(), new DqeUserRepository(), new ProposalRepository(), new ProjectRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(PayItemStructure))) return new object[] { new PayItemStructureRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(MasterFile))) return new object[] { new MasterFileRepository() };
            //if (args.EntityType.IsAssignableFrom(typeof(PayItem))) return new object[] { new PayItemRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(Project))) return new object[] { new ProjectRepository(), UnitOfWorkProvider.CommandRepository, new WebTransportService() };
            if (args.EntityType.IsAssignableFrom(typeof(MarketArea))) return new object[] { new MarketAreaRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(Proposal))) return new object[] { new ProposalRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(ProjectEstimate))) return new object[] { new WebTransportService() };
            return null;
        }
    }

    public class OfficialPricePushErrorEmail : EmailMessage
    {
        private readonly string _to;
        private readonly string _proposal;
        private readonly string _environment;

        public OfficialPricePushErrorEmail(string to, string proposal, string environment)
        {
            _to = to;
            _proposal = proposal;
            _environment = environment;
        }

        public override string To { get { return _to; } }

        public override string Subject { get { return _environment + " - Official Prices were not updated - " + _proposal; } }

        public override string Body
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine("The Official Estimate for proposal " + _proposal + " was not updated in WT. <br />");
                sb.AppendLine("Please check the synchronization of the project(s) on the proposal provided. <br />");
                sb.AppendLine(
                    "Once the estimate is fixed you can immediately take the official estimate and the system will automatically push prices.");

                return sb.ToString();
            }
        }
    }

    public class NoOfficialEstimateInDqeEmail : EmailMessage
    {
        private readonly string _to;
        private readonly string _proposal;
        private readonly string _status;
        private readonly string _environment;

        public NoOfficialEstimateInDqeEmail(string to, string proposal, string status, string environment)
        {
            _to = to;
            _proposal = proposal;
            _status = status;
            _environment = environment;
        }

        public override string To
        {
            get { return _to; }
        }

        public override string Subject
        {
            get { return _environment + " - Proposal Not found in DQE - " + _proposal; }
        }

        public override string Body
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine("Proposal " + _proposal + " was found in WT with a status of " + _status + " without a corresponding record in DQE.<br />");
                return sb.ToString();
            }
        }
    }
}
