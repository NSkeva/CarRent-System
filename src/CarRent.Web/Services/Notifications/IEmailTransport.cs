namespace CarRent.Web.Services.Notifications;

public sealed record EmailMessage(string To, string Subject, string Body);

public interface IEmailTransport
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
