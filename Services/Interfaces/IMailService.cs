namespace api.Services.MailService;

public interface IMailService
{
    void SendEmail(MailMessages mailMessage);
    Task<bool> IsValidEmailAsync(string email);
}