using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using System.Runtime.Remoting.Messaging;
using Dqe.ApplicationServices;
using Dqe.Infrastructure.Providers;
using FDOT.Enterprise;
using FDOT.Enterprise.Configuration.Client;
using IMessage = Dqe.Domain.Messaging.IMessage;

namespace Dqe.Infrastructure.Messaging
{
    /// <summary>
    /// COMPONENT TEMPLATE
    /// </summary>
    public static class MessageRepository
    {
        private static readonly ConcurrentDictionary<Guid, IList<IMessage>> Messages = new ConcurrentDictionary<Guid, IList<IMessage>>();

        public static void Add(Guid transactionId, IMessage message)
        {
            IList<IMessage> messageList;
            if (Messages.TryGetValue(transactionId, out messageList))
            {
                //modification of the list should always be synchronous since the transaction is tied to a thread
                messageList.Add(message);
                return;
            }
            if (!Messages.TryAdd(transactionId, new List<IMessage> { message })) Add(transactionId, message);
        }

        public static void Publish(Guid transactionId)
        {
            IList<IMessage> messageList;
            if (Messages.TryRemove(transactionId, out messageList))
            {
                //do this async
                //publish to service bus, msmq, email, log, etc...
                foreach (var message in messageList)
                {
                    if (message as EmailMessage != null)
                    {
                        //async send
                        //var mailAction = new Action<EmailMessage>(SendMail);
                        //var mailActionEnd = new AsyncCallback(SendMailEnd);
                        //mailAction.BeginInvoke((EmailMessage)message, mailActionEnd, null);
                        //in-thread send
                        SendMail((EmailMessage)message);
                    }
                }
            }
        }

        public static void SendMail(EmailMessage message)
        {
#if DEBUG
            using (var smtp = new SmtpClient())
            {
                using (var email = new MailMessage())
                {
                    var setting = ConfigurationManager.AppSettings.Get("sendNotifications");
                    if (string.IsNullOrWhiteSpace(setting)) return;
                    bool sendNotifications;
                    if (!bool.TryParse(setting, out sendNotifications) || !sendNotifications) return;
                    setting = ConfigurationManager.AppSettings.Get("distributionGroup");
                    if (string.IsNullOrWhiteSpace(setting)) return;
                    email.IsBodyHtml = true;
                    email.From = new MailAddress(setting);
                    email.To.Add(new MailAddress(message.To));
                    email.Subject = message.Subject;
                    email.Body = message.Body;
                    smtp.Send(email);
                }
            }
#else
            var proxy = ChannelProvider<IConfigurationService>.Default;
            var valueDictionary = proxy.GetValueDictionary("EmailRelayServer");
            var serverName = valueDictionary["server"];
            var port = valueDictionary["port"];
            using (var smtp = new SmtpClient(serverName, Convert.ToInt32(port)))
            {
                smtp.EnableSsl = false;
                using (var email = new MailMessage())
                {
                    var setting = ConfigurationManager.AppSettings.Get("sendNotifications");
                    if (string.IsNullOrWhiteSpace(setting)) return;
                    bool sendNotifications;
                    if (!bool.TryParse(setting, out sendNotifications) || !sendNotifications) return;
                    setting = ConfigurationManager.AppSettings.Get("distributionGroup");
                    if (string.IsNullOrWhiteSpace(setting)) return;
                    email.IsBodyHtml = true;
                    email.From = new MailAddress("Do.Not.Reply.APL@dot.state.fl.us");
                    email.To.Add(new MailAddress(message.To));
                    email.Subject = message.Subject;
                    email.Body = message.Body;
                    smtp.Send(email);
                }
            }
#endif
        }

        public static void SendMailEnd(IAsyncResult result)
        {
            ((Action<EmailMessage>)((AsyncResult)result).AsyncDelegate).EndInvoke(result);
        }

        public static void Purge(Guid transactionId)
        {
            IList<IMessage> messageList;
            Messages.TryRemove(transactionId, out messageList);
        }
    }
}