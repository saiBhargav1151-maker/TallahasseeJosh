using System;
using Dqe.Domain.Messaging;

namespace Dqe.ApplicationServices
{
    public abstract class EmailMessage : IMessage
    {
        private readonly string _id = Guid.NewGuid().ToString();

        public string Id
        {
            get { return _id; }
        }

        public bool IsCancelable
        {
            get { return true; }
        }

        public abstract string To { get; }

        public abstract string Subject { get; }

        public abstract string Body { get; }
    }
}