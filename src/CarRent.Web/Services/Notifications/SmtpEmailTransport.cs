using CarRent.Web.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CarRent.Web.Services.Notifications;

public sealed class SmtpEmailTransport(
    IOptions<FleetNotificationOptions> options,
    ILogger<SmtpEmailTransport> logger) : IEmailTransport
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var smtp = options.Value.Smtp;
        if (!smtp.IsConfigured)
            throw new InvalidOperationException("SMTP nije konfiguriran (FleetNotifications:Smtp:Host).");

        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(smtp.FromName, smtp.FromAddress));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new TextPart("plain") { Text = message.Body };

        using var client = new SmtpClient();
        var secure = ResolveSecureSocketOptions(smtp);

        await client.ConnectAsync(smtp.Host, smtp.Port, secure, cancellationToken);

        if (!string.IsNullOrWhiteSpace(smtp.Username))
            await client.AuthenticateAsync(smtp.Username, smtp.Password, cancellationToken);

        await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("Email poslan na {Recipient}: {Subject}", message.To, message.Subject);
    }

    private static SecureSocketOptions ResolveSecureSocketOptions(SmtpOptions smtp)
    {
        if (!smtp.UseSsl)
            return SecureSocketOptions.None;

        // Gmail: 587 = STARTTLS, 465 = SSL
        return smtp.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;
    }
}
