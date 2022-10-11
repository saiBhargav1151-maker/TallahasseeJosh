using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
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
            try
            {
                Initializer.Initialize();
                EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
                var dqeUserRepository = new DqeUserRepository();
                var environmentProvider = new EnvironmentProvider();
                var environment = environmentProvider.GetEnvironment();

                var webTransportService = new WebTransportService();
                var wtProposals = webTransportService.GetProposalsReadyForOfficialEstimate().ToList();

                var proposalRepository = new ProposalRepository();
                var unSynchronizedProposals = new List<Proposal>();
                var noOfficialProposals = new List<Domain.Model.Wt.Proposal>();
                var processedProposals = new List<Proposal>();
                foreach (var wtProposal in wtProposals)
                {
                    var dqeProposal = proposalRepository.GetOfficialProposal(wtProposal.ProposalNumber);

                    if (dqeProposal != null)
                    {
                        var proposal = proposalRepository.GetById(dqeProposal.Id);
                        
                        //var currentDqeUser = dqeUserRepository.GetBySrsId(proposal.Users.First().SrsId);
                        var currentDqeUser = proposal
                            .Projects
                            .First()
                            .ProjectVersions
                            .First(i => i.ProjectEstimates.FirstOrDefault(ii => ii.Label == SnapshotLabel.Official) != null)
                            .VersionOwner;

                        proposal.SetCurrentEstimator(currentDqeUser);
                        var wtp = webTransportService.GetProposal(proposal.ProposalNumber);
                        if (proposal.SynchronizeStructure(wtp, currentDqeUser, true))
                        {
                            webTransportService.UpdatePrices(proposal, true, currentDqeUser);
                            processedProposals.Add(proposal);
                        }
                        else
                        {
                            unSynchronizedProposals.Add(proposal);
                    }
                    }
                    else
                        noOfficialProposals.Add(wtProposal);
                }
                var emailAddresses = AcquireEmailAddresses(dqeUserRepository);
                if (processedProposals.Any())
                    HandleProcessedEmail(dqeUserRepository, processedProposals, environment, emailAddresses);
                    if (unSynchronizedProposals.Any())
                        HandleSynchronizationEmail(dqeUserRepository, unSynchronizedProposals, environment, emailAddresses);
                    if (noOfficialProposals.Any())
                        HandleNoOfficialEmail(dqeUserRepository, noOfficialProposals, environment, emailAddresses);
                UnitOfWorkProvider.TransactionManager.Commit();

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
#if !DEBUG
                FDOT.Enterprise.Architecture.Core.Logging.LogManager.LogError(ex);
#endif
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    Console.WriteLine("ERROR: " + ex);
                }
                Environment.Exit(1);
            }
        }

        private static void HandleNoOfficialEmail(IDqeUserRepository dqeUserRepository, IEnumerable<Domain.Model.Wt.Proposal> wtProposal, string environment, List<string> emailAddresses)
        {
            if (!emailAddresses.Any())
                emailAddresses = AcquireEmailAddresses(dqeUserRepository);

            var sb = new StringBuilder();
            foreach (var proposal in wtProposal.OrderBy(i => i.ProposalNumber))
            {
                var status = proposal.ProposalStatus == "02" ? "Awarded" : "Executed";
                sb.AppendLine("Proposal " + proposal.ProposalNumber + " was found in Project Preconstruction with a status of " + status + " without a corresponding record in DQE.<br />");
            }

            foreach (var emailAddress in emailAddresses)
            {
                var noOfficialEstimate = new NoOfficialEstimateInDqeEmail(emailAddress,  sb.ToString(), environment);

                var messenger = new Messenger();
                messenger.Notify(noOfficialEstimate);
            }
        }

        private static void HandleSynchronizationEmail(IDqeUserRepository dqeUserRepository, IEnumerable<Proposal> proposals, string environment, List<string> emailAddresses)
        {
            if (!emailAddresses.Any())
                emailAddresses = AcquireEmailAddresses(dqeUserRepository);

            var sb = new StringBuilder();
            sb.AppendLine("The following Official Estimate Proposals were not updated in Project Preconstruction. <br />");
            sb.AppendLine("Please check the synchronization of the project(s) on the proposal provided. <br />");
            sb.AppendLine("Once the estimate is fixed you can immediately take the official estimate and the system will automatically push prices. <br />");

            foreach (var proposal in proposals)
            {
                sb.AppendLine("Proposal: " + proposal.ProposalNumber + " <br />");
            }

            foreach (var emailAddress in emailAddresses)
            {
                var officialPriceError = new OfficialPricePushErrorEmail(emailAddress, sb.ToString(), environment);

                var messenger = new Messenger();
                messenger.Notify(officialPriceError);
            }
        }

        private static void HandleProcessedEmail(IDqeUserRepository dqeUserRepository, IEnumerable<Proposal> proposals, string environment, List<string> emailAddresses)
        {
            if (!emailAddresses.Any())
                emailAddresses = AcquireEmailAddresses(dqeUserRepository);

            var sb = new StringBuilder();
            sb.AppendLine("The following Official Estimate Proposals were updated in Project Preconstruction. <br />");
            
            foreach (var proposal in proposals.OrderBy(i => i.ProposalNumber))
            {
                sb.AppendLine("Proposal: " + proposal.ProposalNumber + " <br />");
            }

            foreach (var emailAddress in emailAddresses)
            {
                var officialPriceError = new OfficialPricePushSuccessEmail(emailAddress, sb.ToString(), environment);

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

            var toField = ConfigurationManager.AppSettings["AdditionalDOTEmail"];
            string[] toArray = toField.Split(';');
            for (int idx = 0; idx < toArray.Length; idx++)
            {
                if (!string.IsNullOrEmpty(toArray[idx]))
                {
                    emailAddresses.Add(toArray[idx]);
                }
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
        private readonly string _body;
        private readonly string _environment;

        public OfficialPricePushErrorEmail(string to, string body, string environment)
        {
            _to = to;
            _body = body;
            _environment = environment;
        }

        public override string To { get { return _to; } }

        public override string Subject { get { return _environment + " - Official Prices were not updated "; } }

        public override string Body
        {
            get { return _body; }
        }
    }

    public class OfficialPricePushSuccessEmail : EmailMessage
    {
        private readonly string _to;
        private readonly string _body;
        private readonly string _environment;

        public OfficialPricePushSuccessEmail(string to, string body, string environment)
        {
            _to = to;
            _body = body;
            _environment = environment;
        }

        public override string To { get { return _to; } }

        public override string Subject { get { return _environment + " - Official Prices were updated "; } }

        public override string Body
        {
            get { return _body; }
        }
    }

    public class NoOfficialEstimateInDqeEmail : EmailMessage
    {
        private readonly string _to;
        private readonly string _body;
        private readonly string _environment;

        public NoOfficialEstimateInDqeEmail(string to, string body, string environment)
        {
            _to = to;
            _body = body;
            _environment = environment;
        }

        public override string To
        {
            get { return _to; }
        }

        public override string Subject
        {
            get { return _environment + " - Proposals Not found in DQE"; }
        }

        public override string Body
        {
            get { return _body; }
        }
    }
}
