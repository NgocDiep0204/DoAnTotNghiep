namespace api.Services.MailService;

public class EmailConfiguration
{
    public required string From { get; set; }
    public required string SmtpServer { get; set; } = null!;
    public required int Port { get; set; }
    public string? UserName { get; set; } = null!;
    public string? Password { get; set; } = null!;
    public string? ApiKey { get; set; }
}