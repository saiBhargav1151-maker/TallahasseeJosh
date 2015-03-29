//using System.IO;
//using Dqe.ApplicationServices;
//using Dqe.Domain.Model;
//using Dqe.Domain.Services;
//using Dqe.Infrastructure.Providers;

//namespace Dqe.Infrastructure.Services
//{
//    public class DatabaseDocumentService:IDocumentService
//    {
//        public Document AddDocument(string fileName, Stream fileStream)
//        {
//            var document = new Document();
//            var transformer=document.GetTransformer();

//            using (var ms = new MemoryStream())
//            {
//                fileStream.CopyTo(ms);
//                transformer.FileData = ms.ToArray();
//            }

//            transformer.Name = fileName;

//            document.Transform(transformer,null);
//            UnitOfWorkProvider.CommandRepository.Add(document);

//            return document;
//        }

//        public Document GetDocument(int id)
//        {
//            return UnitOfWorkProvider.Marshaler.CurrentSession.Get<Document>(id);
//        }

//        public void DeleteDocument(int id)
//        {
//            var document= UnitOfWorkProvider.Marshaler.CurrentSession.Get<Document>(id);
//            UnitOfWorkProvider.CommandRepository.Remove(document);
//        }

//        //public void UpdateDocument(int id, Stream fileStream)
//        //{
//        //    var document = UnitOfWorkProvider.Marshaler.CurrentSession.Get<Document>(id);
//        //    var transformer=document.GetTransformer();

//        //                using (var ms = new MemoryStream())
//        //    {
//        //        fileStream.CopyTo(ms);
//        //        transformer.FileData = ms.ToArray();
//        //    }

//        //    document.Transform(transformer,null);
//        //}
//    }
//}
