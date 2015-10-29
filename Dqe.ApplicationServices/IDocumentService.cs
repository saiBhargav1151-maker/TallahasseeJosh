using System.Collections.Generic;
using System.IO;
using Dqe.Domain.Model;

namespace Dqe.ApplicationServices
{
    public interface IDocumentService
    {
        Document SaveDocumentToEdms(string name, Stream stream, string documentType);
        Stream GetDocumentContent(int id);
        void DeleteFromEdms(int id);
        string GetCostBasedTemplateDocumentType();
        string GetProcessedCostBasedTemplateDocumentType();
        string GetAttachmentDocumentType();
    }
}