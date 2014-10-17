using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Dqe.Domain.Model;

namespace Dqe.Domain.Services
{
    public interface IDocumentService
    {
        Document AddDocument(string fileName, Stream fileStream);
        Document GetDocument(int id);
        void DeleteDocument(int id);
        //void UpdateDocument(int id, Stream fileStream);
    }
}
