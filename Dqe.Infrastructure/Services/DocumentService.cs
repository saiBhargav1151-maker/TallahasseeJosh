using System;
using System.IO;
using System.Linq;
using Dqe.ApplicationServices;
using FDOT.Enterprise;
using FDOT.Enterprise.DocumentManagement.Client;
using Document = Dqe.Domain.Model.Document;

namespace Dqe.Infrastructure.Services
{
    public class DocumentService : IDocumentService
    {
        private const string ConnectionLabel = "DQE_EDMS";

        public string GetCostBasedTemplateDocumentType()
        {
            return "ENGDSN198";
        }

        public string GetProcessedCostBasedTemplateDocumentType()
        {
            return "ENGDSN199";
        }

        public string GetAttachmentDocumentType()
        {
            return "ENGDSN200";
        }

        public Document SaveDocumentToEdms(string name, Stream stream, string documentType)
        {
            var nameSegments = name.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            name = nameSegments[nameSegments.GetUpperBound(0)];
            var array = name.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var extension = array.Length > 1
                ? array[array.GetUpperBound(0)]
                : "txt";
            using (var ms = new MemoryStream())
            {
                long fileLength;
                using (var fs = stream)
                {
                    fileLength = fs.Length;
                    var bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, (int)fs.Length);
                    ms.Write(bytes, 0, (int)fs.Length);
                }
                ms.Seek(0, SeekOrigin.Begin);
                var proxy = ChannelProvider<IDocumentManagementService>.Default;
                var authToken = proxy.GenerateAuthenticationToken(ConnectionLabel);
                var createRequest = new DocumentCreateRequest { AuthenticationToken = authToken, Content = ms };
                var properties = proxy.GetDefaultPropertyData(authToken);
                properties.First(i => i.Name == "FINPROJ").Value = "00000000000";
                var businessAreaId = proxy.ListBusinessAreas(authToken).Where(i => i.Id == "ENGDSN").Select(i => i.SystemId).First();
                var documentGroupId = proxy.ListDocumentGroupsForBusinessArea(authToken, businessAreaId).Where(i => i.Id == "ENGDSN05").Select(i => i.SystemId).First();
                var documentTypes = proxy.ListDocumentTypesForGroup(authToken, documentGroupId)
                    .Where(i => i.ParentDocumentGroupId == documentGroupId)
                    .OrderBy(i => i.Description)
                    .Distinct();
                var docType = documentTypes.First(i => i.Id == documentType);
                var prop = properties.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.Name) && i.Name.ToUpper().Trim() == "SENSITIVE_DOC");
                if (prop != null)
                {
                    prop.IsRequired = true;
                    prop.Value = "Y";
                }
                createRequest.Metadata = new DocumentData
                                         {
                                             Properties = properties,
                                             Name = name,
                                             Extension = extension,
                                             DocumentType = docType,
                                             RetentionYears = 99,
                                             RetentionStartDate = DateTime.Now,
                                             FormName = ""
                                         };
                var doc = proxy.AddNewDocument(createRequest);
                return new Document
                {

                    EdmsId = doc.Id,
                    Name = name,
                    FileLength = fileLength,
                    FileData = ms.ToArray()
                };
            }
        }

        public Stream GetDocumentContent(int id)
        {
            var proxy = ChannelProvider<IDocumentManagementService>.Default;
            var authToken = proxy.GenerateAuthenticationToken(ConnectionLabel);
            return proxy.GetDocumentContent(authToken, id);
        }

        public void DeleteFromEdms(int id)
        {
            var proxy = ChannelProvider<IDocumentManagementService>.Default;
            var authToken = proxy.GenerateAuthenticationToken(ConnectionLabel);
            proxy.DeleteDocument(authToken, id);
        }
    }
}