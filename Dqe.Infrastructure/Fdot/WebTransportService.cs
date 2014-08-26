using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Dqe.ApplicationServices.Fdot;
using Dqe.Domain.Model.Wt;
using FDOT.Enterprise;
using FDOT.Enterprise.ConnectionStrings.Client;

namespace Dqe.Infrastructure.Fdot
{
    public class WebTransportService : IWebTransportService
    {
        public IEnumerable<CodeTable> GetCodeTables()
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session.QueryOver<CodeTable>()
                        .OrderBy(i => i.CodeTableName).Asc
                        .Fetch(i => i.CodeValues).Eager
                        .List()
                        .Distinct();
            }
        }

        public CodeTable GetCodeTable(string codeType)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session.QueryOver<CodeTable>()
                        .Where(i => i.CodeTableName == codeType)
                        .Fetch(i => i.CodeValues).Eager
                        .SingleOrDefault();
            }
        }

        public IEnumerable<RefItem> GetRefItems()
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<RefItem>()
                    .Where(i => i.SpecBook == "10")
                    .List()
                    .OrderBy(i => i.Name)
                    .ToList();
            }
        }

        public void ExportProject()
        {
            var token = ChannelProvider<IConnectionStringService>.Default.GetConnectionToken("DQEWT_SRV");
            var svc = new WtEstimatorService.EstimatorServiceClient();
            if (svc.ClientCredentials == null) throw new ServiceActivationException("ClientCredentials cannot be null"); 
            svc.ClientCredentials.UserName.UserName = token.UserId;
            svc.ClientCredentials.UserName.Password = token.Password;
            var project = svc.ExportProject("02001-BID");
        }
    }
}