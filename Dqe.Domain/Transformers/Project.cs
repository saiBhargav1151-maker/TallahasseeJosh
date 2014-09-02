using System;

namespace Dqe.Domain.Transformers
{
    public class Project : Transformer
    {
        public string ProjectNumber { get; set; }
        public string District { get; set; }
        public string County { get; set; }
        public DateTime? LettingDate { get; set; }
        public string Description { get; set; }
        public int? DesignerSrsId { get; set; }
        public int? PseeContactSrsId { get; set; }
    }
}