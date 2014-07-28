// ReSharper disable InconsistentNaming
using System.Collections.Generic;

namespace Dqe.Web.ActionResults
{
    public class DqeResultObject
    {

        public IEnumerable<ClientMessage> messages { get; set; }

        public object data { get; set; }
    }
}
// ReSharper restore InconsistentNaming