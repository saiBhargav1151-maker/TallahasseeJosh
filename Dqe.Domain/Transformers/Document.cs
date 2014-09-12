using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dqe.Domain.Transformers
{
    public class Document : Transformer
    {
        public byte[] FileData { get; set; }
        public string Name { get; set; }
    }

    public class CostBasedTemplateDocumentVersion : Transformer
    {
        public DateTime Timestamp { get; set; }
        public int DocumentId { get; set; }
    }
}
