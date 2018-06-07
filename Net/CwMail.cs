using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;

namespace CwLib.Net
{
    public class CwMail
    {
        public MailAddress From { get; set; }
        public string ServerIp { get; set; }
        public int ServerPort { get; set; }
        public bool EnableSSL { get; set; }
        public string MailAccount { get; set; }
        public string MailPassword { private get; set; }
        public bool UseHtml { get; set; }

        public CwMail(MailAddress from, string mailAccount, string mailPassword, string mailServer, int mailPort = 25, bool enableSSL = true, bool useHtml = true)
        {
            From = from;
            UseHtml = useHtml;
            ServerIp = mailServer;
            ServerPort = mailPort;
            EnableSSL = enableSSL;
            MailAccount = mailAccount;
            MailPassword = mailPassword;
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void Sand(string subject, string content, string to)
        {
            Sand(subject, content, new MailAddress(to));
        }

        public void Sand(string subject, string content, MailAddress to)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(ValidateServerCertificate);

            var message = new MailMessage(From, to)
            {
                IsBodyHtml = UseHtml,
                BodyEncoding = System.Text.Encoding.UTF8,
                Subject = subject,
                Body = content
            };

            var client = new SmtpClient(ServerIp, ServerPort)
            {
                Credentials = new NetworkCredential(MailAccount, MailPassword),
                EnableSsl = EnableSSL
            };

            client.Send(message);
        }

    }
}