using MimeKit;

namespace api.Services.MailService;

public class MailMessages
{
    public MailMessages(IEnumerable<string> to, string subject, string content, bool isHtml = false)
    {
        // To = new List<MailboxAddress>();
        //To.AddRange(to.Select(x => new MailboxAddress("email", x)));
        To = to.Select(email => new MailboxAddress("email", email)).ToList();
        Subject = subject;
        Content = content;
        IsHtml = isHtml;

    }

    public List<MailboxAddress> To { get; set; }
    public string Subject { get; set; }

    public string Content { get; set; }
    public bool IsHtml { get; set; }
}