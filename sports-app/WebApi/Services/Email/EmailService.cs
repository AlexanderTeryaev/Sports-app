using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace WebApi.Services
{
    public class EmailService
    {
        public Task SendAsync(IdentityMessage message)
        {
            using (var msg = new MailMessage())
            {
                msg.To.Add(message.Destination);
                msg.From = new MailAddress(ConfigurationManager.AppSettings["MailServerSenderAdress"]);
                msg.Subject = message.Subject;
                msg.Body = message.Body;
                msg.SubjectEncoding = System.Text.Encoding.UTF8;
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.IsBodyHtml = true;
                msg.Priority = MailPriority.Normal;

                using (var client = new SmtpClient())
                {

#if DEBUG
                    client.Host = ConfigurationManager.AppSettings["MailServerDebug"];
#else
                    client.Host = ConfigurationManager.AppSettings["MailServer"];
#endif
                    client.Port = 587;
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential("logligwebapi", "YK6dZ(mv8h");
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    try
                    {
                        client.Send(msg);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
            }
            return Task.FromResult(0);
        }
    }
}