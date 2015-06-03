using System;

namespace Dqe.Domain.Model
{
    public class CostBasedTemplateDocumentVersion : Entity<Transformers.CostBasedTemplateDocumentVersion>
    {
        public virtual DateTime Timestamp { get; protected set; }
        public virtual CostBasedTemplate MyCostBasedTemplate { get; protected internal set; }

        public virtual int EdmsDocumentId { get; protected internal set; }

        public override Transformers.CostBasedTemplateDocumentVersion GetTransformer()
        {
            return new Transformers.CostBasedTemplateDocumentVersion
            {
                Timestamp = Timestamp,
                EdmsDocumentId = EdmsDocumentId
            };
        }

        public override void Transform(Transformers.CostBasedTemplateDocumentVersion transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");

            Timestamp = transformer.Timestamp;
            Id = transformer.DocumentId;
            EdmsDocumentId = transformer.EdmsDocumentId;
        }
    }
}