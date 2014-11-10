using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Dqe.Web.ActionResults
{
    public class DqeResult : JsonResult
    {
        private readonly object _data;
        private readonly JsonRequestBehavior _jsonRequestBehavior = JsonRequestBehavior.DenyGet;
        private readonly string _contentType = "application/json";
        private readonly Encoding _encoding = Encoding.Default;
        private readonly IEnumerable<ClientMessage> _clientMessages = new List<ClientMessage>();

        public bool IsValid()
        {
            if (!_clientMessages.Any()) return true;
            if (_clientMessages.All(i => i.Severity == ClientMessageSeverity.Success)) return true;
            return false;
        }

        public DqeResult(object data)
        {
            _data = data;
        }

        public DqeResult(object data, JsonRequestBehavior jsonRequestBehavior)
        {
            _data = data;
            _jsonRequestBehavior = jsonRequestBehavior;
        }

        public DqeResult(object data, string contentType)
        {
            _data = data;
            _contentType = contentType;
        }

        public DqeResult(object data, Encoding contentEncoding)
        {
            _data = data;
            _encoding = contentEncoding;
        }

        public DqeResult(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior jsonRequestBehavior)
        {
            _data = data;
            _jsonRequestBehavior = jsonRequestBehavior;
            _contentType = contentType;
            _encoding = contentEncoding;
        }

        public DqeResult(object data, string contentType, JsonRequestBehavior jsonRequestBehavior)
        {
            _data = data;
            _jsonRequestBehavior = jsonRequestBehavior;
            _contentType = contentType;
        }


        public DqeResult(object data, ClientMessage message)
        {
            _data = data;
            _clientMessages = new[] {message};
        }

        public DqeResult(object data, ClientMessage message, JsonRequestBehavior jsonRequestBehavior)
        {
            _data = data;
            _jsonRequestBehavior = jsonRequestBehavior;
            _clientMessages = new[] { message };
        }

        public DqeResult(object data, ClientMessage message, string contentType)
        {
            _data = data;
            _contentType = contentType;
            _clientMessages = new[] { message };
        }

        public DqeResult(object data, ClientMessage message, Encoding contentEncoding)
        {
            _data = data;
            _encoding = contentEncoding;
            _clientMessages = new[] { message };
        }

        public DqeResult(object data, ClientMessage message, string contentType, Encoding contentEncoding, JsonRequestBehavior jsonRequestBehavior)
        {
            _data = data;
            _jsonRequestBehavior = jsonRequestBehavior;
            _contentType = contentType;
            _encoding = contentEncoding;
            _clientMessages = new[] { message };
        }

        public DqeResult(object data, ClientMessage message, string contentType, JsonRequestBehavior jsonRequestBehavior)
        {
            _data = data;
            _jsonRequestBehavior = jsonRequestBehavior;
            _contentType = contentType;
            _clientMessages = new[] { message };
        }

        public DqeResult(object data, IEnumerable<ClientMessage> messages)
        {
            _data = data;
            _clientMessages = messages;
        }

        public DqeResult(object data, IEnumerable<ClientMessage> messages, JsonRequestBehavior jsonRequestBehavior)
        {
            _data = data;
            _jsonRequestBehavior = jsonRequestBehavior;
            _clientMessages = messages;
        }

        public DqeResult(object data, IEnumerable<ClientMessage> messages, string contentType)
        {
            _data = data;
            _contentType = contentType;
            _clientMessages = messages;
        }

        public DqeResult(object data, IEnumerable<ClientMessage> messages, Encoding contentEncoding)
        {
            _data = data;
            _encoding = contentEncoding;
            _clientMessages = messages;
        }

        public DqeResult(object data, IEnumerable<ClientMessage> messages, string contentType, Encoding contentEncoding, JsonRequestBehavior jsonRequestBehavior)
        {
            _data = data;
            _jsonRequestBehavior = jsonRequestBehavior;
            _contentType = contentType;
            _encoding = contentEncoding;
            _clientMessages = messages;
        }

        public DqeResult(object data, IEnumerable<ClientMessage> messages, string contentType, JsonRequestBehavior jsonRequestBehavior)
        {
            _data = data;
            _jsonRequestBehavior = jsonRequestBehavior;
            _contentType = contentType;
            _clientMessages = messages;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var wrapper = new DqeResultObject {messages = _clientMessages, data = _data};
            var result = new JsonResult
            {
                ContentEncoding = _encoding,
                ContentType = _contentType,
                Data = wrapper,
                JsonRequestBehavior = _jsonRequestBehavior
            };
            result.ExecuteResult(context);
        }
    }
}