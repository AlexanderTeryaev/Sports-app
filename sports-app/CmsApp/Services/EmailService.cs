using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CmsApp.Services
{
    public class EmailService
    {
        public Task SendAsync(string recipientName, string body, string subject = null, string sender = null)
        {
            var emails = recipientName.Split(',');
            foreach (var emailBcc in emails)
            {
                using (var msg = new MailMessage())
                {
                    msg.To.Add(new MailAddress(emailBcc));
                    msg.From = new MailAddress(sender ?? ConfigurationManager.AppSettings["MailServerSenderAdress"]);
                    if (!string.IsNullOrEmpty(sender))
                        msg.ReplyToList.Add(new MailAddress(sender));

                    msg.Subject = subject ?? "Loglig";

                    msg.Body = body;
                    msg.Body = body.Replace("\r\n", "<br />");

                    msg.SubjectEncoding = Encoding.UTF8;
                    msg.BodyEncoding = Encoding.UTF8;
                    msg.IsBodyHtml = true;
                    msg.Priority = MailPriority.Normal;
                    using (var client = new SmtpClient())
                    {
#if DEBUG
                        client.Host = ConfigurationManager.AppSettings["MailServerDebug"];
#else
                    client.Host = ConfigurationManager.AppSettings["MailServer"];
#endif
                        client.EnableSsl = true;
                        client.Port = 587;
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
            }
            return Task.FromResult(0);
        }

        public Task SendWithFileAsync(string recipientName, string body, string subject = null,
            HttpPostedFileBase emailFile = null, string sender = null)
        {
            var emails = recipientName.Split(',');
            foreach (var emailBcc in emails)
            {
                using (var msg = new MailMessage())
                {
                    msg.Bcc.Add(new MailAddress(emailBcc));
                    msg.From = new MailAddress(sender ?? ConfigurationManager.AppSettings["MailServerSenderAdress"]);
                    if (!string.IsNullOrEmpty(sender))
                        msg.ReplyToList.Add(new MailAddress(sender));
                    msg.Subject = subject ?? "Loglig";
                    msg.Body = body;
                    msg.SubjectEncoding = Encoding.UTF8;
                    msg.BodyEncoding = Encoding.UTF8;
                    msg.IsBodyHtml = true;
                    msg.Priority = MailPriority.Normal;
                    using (Attachment data = new Attachment(emailFile.InputStream, Path.GetFileName(emailFile.FileName), emailFile.ContentType))
                    {
                        msg.Attachments.Add(data);

                        using (var client = new SmtpClient())
                        {
#if DEBUG
                            client.Host = ConfigurationManager.AppSettings["MailServerDebug"];
#else
                    client.Host = ConfigurationManager.AppSettings["MailServer"];
#endif
                            client.EnableSsl = true;
                            client.Port = 587;
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
                }
            }
            return Task.FromResult(0);
        }
    }
}