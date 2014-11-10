using System;
using System.Collections.Generic;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Wt;
using Project = Dqe.Domain.Model.Wt.Project;
using Proposal = Dqe.Domain.Model.Wt.Proposal;

namespace Dqe.Infrastructure.Fdot
{
    public class MockWebTransportService : IWebTransportService
    {
        public IEnumerable<CodeTable> GetCodeTables()
        {
            return null;
        }

        public CodeTable GetCodeTable(string codeType)
        {
            var ct = new CodeTable
            {
                CodeTableName = "",
                CreatedBy = "",
                CreatedDate = DateTime.Now,
                Description = "",
                Id = 0,
                LastUpdatedBy = "",
                LastUpdatedDate = DateTime.Now,
                RecordSource = ""
            };
            ct.AddCodeValue(new CodeValue
            {
                CodeValueName = "",
                CreatedBy = "",
                CreatedDate = DateTime.Now,
                Description = "",
                Id = 0,
                LastUpdatedBy = "",
                LastUpdatedDate = DateTime.Now,
                MyCodeTable = ct,
                ObsoleteDate = null,
                RecordSource = ""
            });
            return ct;
        }

        public IEnumerable<RefItem> GetRefItems()
        {
            return null;
        }

        public IEnumerable<RefItem> GetRefItemsBySpecYear(int specYear)
        {
            return null;
        }

        public IEnumerable<Project> GetProjects(string number)
        {
            return null;
        }

        public Project GetProject(string number)
        {
            return null;
        }

        public IEnumerable<Project> GetProjectsByProposalId(long id)
        {
            return null;
        }

        public IEnumerable<Proposal> GetProposals(string number)
        {
            return null;
        }

        public Proposal GetProposal(string number)
        {
            return null;
        }

        public Project ExportProject(string projectNumber)
        {
            return null;
        }

        public Proposal ExportProposal(string proposalNumber)
        {
            return null;
        }

        public bool IsProjectSynced(ProjectEstimate projectEstimate)
        {
            return true;
        }

        public IEnumerable<string> GetMasterFiles()
        {
            throw new NotImplementedException();
        }
    }
}