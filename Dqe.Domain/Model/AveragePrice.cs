using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dqe.Domain.Model
{
    public class AveragePrice : Entity<Transformers.AveragePrice>
    {
        public virtual decimal Price { get; protected internal set; }

        public virtual PayItemMaster MyPayItemMaster { get; set; }

        public override Transformers.AveragePrice GetTransformer()
        {
            return new Transformers.AveragePrice
            {
                Price = Price
            };
        }

        public override void Transform(Transformers.AveragePrice transformer, DqeUser account)
        {
            Price = transformer.Price;
        }
    }
}
