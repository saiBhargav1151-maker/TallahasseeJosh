//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;

//namespace Dqe.Domain.Model
//{
//    public class PrimaryUnit : DqeCode
//    {
//        private readonly ICollection<PayItemStructure> _payItemStructures;

//        public PrimaryUnit()
//        {
//            _payItemStructures = new Collection<PayItemStructure>();
//        }

//        public virtual IEnumerable<PayItemStructure> PayItemStructures
//        {
//            get { return _payItemStructures.ToList().AsReadOnly(); }
//        } 

//        public override Transformers.DqeCode GetTransformer()
//        {
//            return new Transformers.PrimaryUnit
//            {
//                Id = Id,
//                Name = Name,
//                IsActive = IsActive
//            };
//        }
//    }
//}