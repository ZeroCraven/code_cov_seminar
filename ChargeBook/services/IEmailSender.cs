using System.Threading.Tasks;

namespace ChargeBook.services {
    public interface IEmailSender {
        public Task sendEmailAsync(string email, string subject, string htmlMessage, string textMessage, string username);
    }
}