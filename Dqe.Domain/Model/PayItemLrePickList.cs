using System;

namespace Dqe.Domain.Model
{
    public class PayItemLrePickList : Entity<Transformers.PayItemLrePickList>
    {
        public virtual LrePickListType PickList { get; protected internal set; }

        public virtual PayItemMaster MyPayItemMaster { get; protected internal set; }

        public override Transformers.PayItemLrePickList GetTransformer()
        {
            return new Transformers.PayItemLrePickList
            {
                Id = Id,
                PickList = PickList
            };
        }

        public override void Transform(Transformers.PayItemLrePickList transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            PickList = transformer.PickList;
        }
    }
}