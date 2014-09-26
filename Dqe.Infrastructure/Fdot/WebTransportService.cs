using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml.Serialization;
using Dqe.ApplicationServices.Fdot;
using Dqe.Domain.Model.Wt;
using FDOT.Enterprise;
using FDOT.Enterprise.ConnectionStrings.Client;
using NHibernate.Criterion;

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

        public IEnumerable<Project> GetProjects(string number)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<Project>()
                    .WhereRestrictionOn(i => i.ProjectNumber).IsLike(number, MatchMode.Start)
                    //.Where(i => i.SpecBook == "10")
                    .List()
                    .OrderBy(i => i.ProjectNumber)
                    .ToList();
            }
        }

        public Estimate ExportProject(string projectNumber)
        {
            var token = ChannelProvider<IConnectionStringService>.Default.GetConnectionToken("DQEWT_SRV");
            var svc = new WtEstimatorService.EstimatorServiceClient();
            if (svc.ClientCredentials == null) throw new ServiceActivationException("ClientCredentials cannot be null"); 
            svc.ClientCredentials.UserName.UserName = token.UserId;
            svc.ClientCredentials.UserName.Password = token.Password;
            try
            {
                var project = svc.ExportProject(projectNumber);
                var serializer = new XmlSerializer(typeof (Estimate));
                using (var stream = new StringReader(project))
                {
                    return (Estimate)serializer.Deserialize(stream);
                }
            }
            catch
            {
                return null;   
            }
        }
    }
}