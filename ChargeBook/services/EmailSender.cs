using System;
using System.Threading.Tasks;
using chargebook.models.email;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace ChargeBook.services {
    public class EmailSender : IEmailSender, IDisposable {

        private readonly SmtpClient client;
        private readonly string host;
        private readonly int port;
        private readonly string password;
        private readonly string fromAddress;

        public EmailSender(IConfiguration configuration) {
            IConfiguration config = configuration;
            host = config.GetValue<string>("Smtp:Server");
            port = config.GetValue<int>("Smtp:Port");
            fromAddress = config.GetValue<string>("Smtp:FromAddress");
            password = config.GetValue<string>("Smtp:Password");
            client = new SmtpClient();
            connectToSmtpServer();
        }

        private void connectToSmtpServer() {
            client.Connect(host, port, true);
            client.Authenticate(fromAddress, password);
        }


        private void disconnectToSmtpServer() {
            client.Disconnect(true);
        }

        public Task sendEmailAsync(string email, string subject, string htmlMessage, string textMessage, string username) {
            MimeMessage message = new MimeMessage();

            MailboxAddress from = new MailboxAddress("ChargeBook", fromAddress);
            message.From.Add(from);

            MailboxAddress to = new MailboxAddress(username, email);
            message.To.Add(to);

            message.Subject = subject;

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = htmlMessage;
            bodyBuilder.TextBody = textMessage;
            message.Body = bodyBuilder.ToMessageBody();

            return client.SendAsync(message);
        }

        public void Dispose() {
            disconnectToSmtpServer();
            client?.Dispose();
        }
    }
}