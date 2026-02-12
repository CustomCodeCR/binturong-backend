using System.Net;
using System.Net.Mail;
using Application.Abstractions.Notifications;
using Application.Options;

namespace Infrastructure.Notifications;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _opt;

    public SmtpEmailSender(EmailOptions opt) => _opt = opt;

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(_opt.Host))
            return;

        using var client = new SmtpClient(_opt.Host, _opt.Port) { EnableSsl = _opt.UseSsl };

        if (!string.IsNullOrWhiteSpace(_opt.Username))
            client.Credentials = new NetworkCredential(_opt.Username, _opt.Password);

        using var msg = new MailMessage
        {
            From = new MailAddress(_opt.FromEmail, _opt.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };

        msg.To.Add(toEmail);

        ct.ThrowIfCancellationRequested();
        await client.SendMailAsync(msg);
    }
}
