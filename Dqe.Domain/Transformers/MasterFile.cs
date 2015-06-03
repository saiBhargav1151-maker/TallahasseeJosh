using System;

namespace Dqe.Domain.Transformers
{
    public class MasterFile : Transformer
    {
        public int FileNumber { get; set; }
        public bool DoMasterFileCopy { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }
}