using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class PrepAndDocChapterWebLink : DqeWebLink
    {
        private readonly ICollection<PayItemStructure> _payItemStructures;

        public PrepAndDocChapterWebLink()
        {
            _payItemStructures = new Collection<PayItemStructure>();
        }

        public virtual IEnumerable<PayItemStructure> PayItemStructures
        {
            get { return _payItemStructures.ToList().AsReadOnly(); }
        } 

        public override Transformers.DqeWebLink GetTransformer()
        {
            return new Transformers.PrepAndDocChapterWebLink
            {
                Id = Id,
                Name = Name,
                WebLink = WebLink
            };
        }
    }
}