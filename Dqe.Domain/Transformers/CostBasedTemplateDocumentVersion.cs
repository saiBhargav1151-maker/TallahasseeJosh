using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dqe.Domain.Transformers
{
    public class CostBasedTemplateDocumentVersion : Transformer
    {
        public DateTime Timestamp { get; set; }
        public long DocumentId { get; set; }
        public int EdmsDocumentId { get; set; }
    }
}
