using System.Collections.Generic;
using Dqe.Domain.Model.Wt;

namespace Dqe.ApplicationServices.Fdot
{
    public interface IWebTransportService
    {
        IEnumerable<CodeTable> GetCodeTables();
        CodeTable GetCodeTable(string codeType);
        IEnumerable<RefItem> GetRefItems();
        IEnumerable<Project> GetProjects(string number); 
        Estimate ExportProject(string projectNumber);
    }
}