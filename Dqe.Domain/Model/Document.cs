using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace Dqe.Domain.Model
{
    public class Document : Entity<Transformers.Document>
    {
        public virtual byte[] FileData { get; protected set; }
        public virtual string Name { get; protected set; }

        public override Transformers.Document GetTransformer()
        {
            return new Transformers.Document
            {
                FileData = FileData,
                Name = Name
            };
        }

        public override void Transform(Transformers.Document transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");

            FileData = transformer.FileData;
            Name = transformer.Name;
        }
    }

    public class CostBasedTemplateDocumentVersion : Entity<Transformers.CostBasedTemplateDocumentVersion>
    {
        public virtual DateTime Timestamp { get; protected set; }
        public virtual CostBasedTemplate MyCostBasedTemplate { get; protected internal set; }
        public override Transformers.CostBasedTemplateDocumentVersion GetTransformer()
        {
            return new Transformers.CostBasedTemplateDocumentVersion
            {
                Timestamp = Timestamp
            };
        }

        public override void Transform(Transformers.CostBasedTemplateDocumentVersion transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");

            Timestamp = transformer.Timestamp;
            Id = transformer.DocumentId;
        }
    }
}
