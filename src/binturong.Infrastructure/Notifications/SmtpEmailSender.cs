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
        if (!_opt.Enabled)
            return;

        using var msg = new MailMessage();
        msg.From = new MailAddress(_opt.FromEmail, _opt.FromName);
        msg.To.Add(toEmail);
        msg.Subject = subject;
        msg.Body = htmlBody;
        msg.IsBodyHtml = true;

        using var client = new SmtpClient(_opt.Host, _opt.Port)
        {
            EnableSsl = _opt.UseSsl,
            Credentials = new NetworkCredential(_opt.User, _opt.Password),
        };

        // SmtpClient has no true cancellation; best effort
        await client.SendMailAsync(msg);
    }
}
