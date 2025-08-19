using System;
using System.Collections.Generic;
using System.Configuration;
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
using CommandLine;


namespace Dqe.Automation.EstimateProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            string argProposal = string.Empty;
            string argUserid = string.Empty;


            try
            {
                //check for aguments
                if (args?.Length != 0)
                {
                    var commandLineOptions = Parser.Default.ParseArguments<CommandLineOptions>(args);

                    commandLineOptions.WithParsed(options =>
                    {
                        argUserid = options.Puserid?.Trim();
                        argProposal = options.Pproposal?.Trim();

                        if (argUserid.Contains('\\'))
                            argUserid = argUserid.Split(new[] { '\\' })[1];

                        Console.WriteLine($"On-Demand DQE OE load submit by Userid: {argUserid}");
                        Console.WriteLine($"Parm Proposal: {argProposal}");
                    });
                }

                Console.WriteLine("Starting DQE Estimate Processing {0}", DateTime.Now);
                Initializer.Initialize();
                EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
                var dqeUserRepository = new DqeUserRepository();
                var environmentProvider = new EnvironmentProvider();
                var environment = environmentProvider.GetEnvironment();

                var webTransportService = new WebTransportService();
                var wtProposals = webTransportService.GetProposalsReadyForOfficialEstimate(argProposal).ToList();

                Console.WriteLine("Processing {0} proposals for Official Estimate updates in AASHTOWare Project", wtProposals.Count());
                Console.WriteLine("Criteria:");
                Console.WriteLine("     Letting date has occured");
                Console.WriteLine("     Letting status is 'ARCH' or 'SCHD'");
                Console.WriteLine("     Proposal status is not 05-withdrawn, 09-moved, 17-postponed");
                Console.WriteLine("     Proposal officialEstimate flag PRFLG4 is null");
                Console.WriteLine("     Proposal is not marked rejected" + Environment.NewLine);

                if (wtProposals.Count > 0)
                {        
                    var proposalRepository = new ProposalRepository();
                    var skipMaintProposals = new List<string>();
                    var unSynchronizedProposals = new List<Proposal>();
                    var noOfficialProposals = new List<Domain.Model.Wt.Proposal>();
                    var processedProposals = new List<Proposal>();
                    foreach (var wtProposal in wtProposals)
                    {
                        var dqeProposal = proposalRepository.GetOfficialProposal(wtProposal.ProposalNumber);

                        if (dqeProposal != null)
                        {
                            var proposal = proposalRepository.GetById(dqeProposal.Id);
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
                        {
                            // Exclude all maintenance contracts except for D3
                            // Include all other contract types
                            if (wtProposal.ContractType.StartsWith("M"))
                            {
                                if (wtProposal.District.Name == "03")
                                    noOfficialProposals.Add(wtProposal);
                                else skipMaintProposals.Add(wtProposal.ProposalNumber);
                            }
                            else
                            {
                                noOfficialProposals.Add(wtProposal);
                            }
                        }
                    }

                    var emailAddresses = AcquireEmailAddresses(dqeUserRepository);
                    if (skipMaintProposals.Any())
                        Console.WriteLine("Skipped {0} proposals of maintenance types, excluding district 03" + Environment.NewLine, skipMaintProposals.Count());
                    if (processedProposals.Any())
                        HandleProcessedEmail(dqeUserRepository, processedProposals, environment, emailAddresses, argUserid);
                    if (unSynchronizedProposals.Any())
                        HandleSynchronizationEmail(dqeUserRepository, unSynchronizedProposals, environment, emailAddresses, argUserid);
                    if (noOfficialProposals.Any())
                        HandleNoOfficialEmail(dqeUserRepository, noOfficialProposals, environment, emailAddresses, argUserid);
                    UnitOfWorkProvider.TransactionManager.Commit();
                }
                else
                {
                    Console.WriteLine("No Proposals found that meet criteria");
                }

                Console.WriteLine("End DQE Estimate Processing {0}", DateTime.Now);

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

        private static void HandleNoOfficialEmail(IDqeUserRepository dqeUserRepository, IEnumerable<Domain.Model.Wt.Proposal> wtProposal, string environment, List<string> emailAddresses, string argUserid)
        {
            var webTransportService = new WebTransportService();

            if (!emailAddresses.Any())
                emailAddresses = AcquireEmailAddresses(dqeUserRepository);

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(argUserid))
                sb.AppendLine("On-Demand submit by: " + argUserid + " <br /><br />");

            sb.AppendLine("The following {" + wtProposal.Count() + "} Project Preconstruction Proposals were not found in DQE. <br />");

            foreach (var proposal in wtProposal.OrderBy(i => i.ProposalNumber))
            {
                var code = webTransportService.GetCodeTable("PRPSTAT");
                var status = code.CodeValues.First(i => i.CodeValueName == proposal.ProposalStatus);
                sb.AppendLine("Proposal " + proposal.ProposalNumber + " was found in Project Preconstruction with a status of " + status.CodeValueName + "-" + status.Description + " without a corresponding record in DQE.<br />");
            }
            Console.WriteLine(Environment.NewLine + sb.ToString());

            foreach (var emailAddress in emailAddresses)
            {
                var noOfficialEstimate = new NoOfficialEstimateInDqeEmail(emailAddress,  sb.ToString(), environment);

                var messenger = new Messenger();
                messenger.Notify(noOfficialEstimate);
            }
        }

        private static void HandleSynchronizationEmail(IDqeUserRepository dqeUserRepository, IEnumerable<Proposal> proposals, string environment, List<string> emailAddresses, string argUserid )
        {
            if (!emailAddresses.Any())
                emailAddresses = AcquireEmailAddresses(dqeUserRepository);

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(argUserid))
                sb.AppendLine("On-Demand submit by: " + argUserid + " <br /><br />");

            sb.AppendLine("The following {" + proposals.Count() + "} Official Estimate Proposals were not updated in Project Preconstruction. <br />");
            sb.AppendLine("Please check the synchronization of the project(s) on the proposal provided. <br />");
            sb.AppendLine("Once the estimate is fixed you can immediately take the official estimate and the system will automatically push prices. <br />");

            foreach (var proposal in proposals)
            {
                sb.AppendLine("Proposal: " + proposal.ProposalNumber + " <br />");
            }
            Console.WriteLine(Environment.NewLine + sb.ToString());

            foreach (var emailAddress in emailAddresses)
            {
                var officialPriceError = new OfficialPricePushErrorEmail(emailAddress, sb.ToString(), environment);

                var messenger = new Messenger();
                messenger.Notify(officialPriceError);
            }
        }

        private static void HandleProcessedEmail(IDqeUserRepository dqeUserRepository, IEnumerable<Proposal> proposals, string environment, List<string> emailAddresses, string argUserid)
        {
            if (!emailAddresses.Any())
                emailAddresses = AcquireEmailAddresses(dqeUserRepository);

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(argUserid))
                sb.AppendLine("On-Demand submit by: " + argUserid + " <br /><br />");

            sb.AppendLine("The following {" + proposals.Count() + "} Official Estimate Proposals were updated in Project Preconstruction. <br />");
            
            foreach (var proposal in proposals.OrderBy(i => i.ProposalNumber))
            {
                sb.AppendLine("Proposal: " + proposal.ProposalNumber + " <br />");
            }
            Console.WriteLine(sb.ToString());

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

            var toField = ConfigurationManager.AppSettings["AdditionalDOTEmails"];
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
            get { return _environment + " - Proposals not found in DQE"; }
        }

        public override string Body
        {
            get { return _body; }
        }
    }

    class CommandLineOptions
    {
        [Option('u', "ARG_Userid", Required = false, HelpText = "Userid that submitted job")]
        public string Puserid { get; set; }

        [Option('p', "ARG_Proposal", Required = false, HelpText = "Proposal to process")]
        public string Pproposal { get; set; }
 
    }
}
