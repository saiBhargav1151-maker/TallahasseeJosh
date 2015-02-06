using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using System.Runtime.Remoting.Messaging;
using Dqe.ApplicationServices;
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
            //NOTE: this will trigger an exception unless you have configured system.net in web or machine.config
            /*
             * This is what I did to temporarily use Gmail...
             * 
             * <system.net>
             *   <mailSettings>
             *     <smtp from="randy.w.lee@gmail.com">
             *       <network host="smtp.gmail.com" port="587" enableSsl="true" userName="john.doe@gmail.com" password="MyPassword" defaultCredentials="false"/>
             *     </smtp>
             *   </mailSettings>
             * </system.net>
             */
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
                    email.From = new MailAddress(setting);
                    email.To.Add(new MailAddress(message.To));
                    email.Subject = message.Subject;
                    email.Body = message.Body;
                    smtp.Send(email);
                }
            }
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