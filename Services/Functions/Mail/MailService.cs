using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json.Linq;

namespace api.Services.MailService;

public class MailService : IMailService
{
    private readonly EmailConfiguration _emailConfiguration;
    private readonly HttpClient _httpClient;

    public MailService(EmailConfiguration emailConfiguration, HttpClient httpClient)
    {
        _emailConfiguration = emailConfiguration;
        _httpClient = httpClient;
    }

    public async Task<bool> IsValidEmailAsync(string email)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"https://api.zerobounce.net/v2/validate?api_key={_emailConfiguration.ApiKey}&email={email}");
            
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            var status = json["status"]?.ToString();
            return status == "valid";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi xác thực email: {ex.Message}");
            return false;
        }
    }

    public void SendEmail(MailMessages mailMessage)
    {
        try
        {
            var emailMessage = CreateEmailMessage(mailMessage);
            Send(emailMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi tạo hoặc gửi email: {ex.Message}");
            throw;
        }
    }

    private MimeMessage CreateEmailMessage(MailMessages mailMessages)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("email", _emailConfiguration.From));
        emailMessage.To.AddRange(mailMessages.To);
        emailMessage.Subject = mailMessages.Subject ?? "Không có tiêu đề";

        var emailContent = mailMessages.Content ?? "Nội dung email không được để trống";

        emailMessage.Body = new TextPart(mailMessages.IsHtml ? TextFormat.Html : TextFormat.Text)
        {
            Text = emailContent
        };

        return emailMessage;
    }



    private void Send(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        try
        {
            Console.WriteLine("Đang kết nối tới SMTP...");
            client.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.Port, SecureSocketOptions.Auto);

            Console.WriteLine("Đang xác thực...");
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate(_emailConfiguration.UserName, _emailConfiguration.Password);

            Console.WriteLine("Đang gửi email...");
            client.Send(mailMessage);
            Console.WriteLine("Email đã gửi thành công!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi gửi email: {ex.Message}");
            throw;
        }
        finally
        {
            client.Disconnect(true);
            Console.WriteLine("Đã ngắt kết nối SMTP.");
        }
    }
}
