namespace Dqe.Domain.Model
{
    public class Document
    {
        public int EdmsId { get; set; }
        public byte[] FileData { get; set; }
        public string Name { get; set; }
        public long FileLength { get; set; }
    }
}
