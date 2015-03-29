using NHibernate.SqlTypes;
using NHibernate.Type;

namespace Dqe.Infrastructure.Types
{
    public class BoolType : CharBooleanType
    {
        public BoolType() : base(new AnsiStringFixedLengthSqlType(1)) { }

        protected override string TrueString
        {
            get { return "Y"; }
        }

        protected override string FalseString
        {
            get { return "N"; }
        }

        public override string Name
        {
            get { return "bool"; }
        }
    }
}
