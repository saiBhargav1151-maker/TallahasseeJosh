using System;

namespace Dqe.Domain.Model.Wt
{
    public class Letting
    {
        public virtual long Id { get; set; }

        public virtual DateTime LettingDate { get; set; }

        public virtual string LettingName { get; set; }
    }
}