using System;
using System.Collections.Generic;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model.Wt;

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

        public IEnumerable<Project> GetProjects(string number)
        {
            return null;
        }

        public Project GetProject(string number)
        {
            return null;
        }

        public IEnumerable<Proposal> GetProposals(string number)
        {
            return null;
        }

        public Estimate ExportProject(string projectNumber)
        {
            return null;
        }
    }
}