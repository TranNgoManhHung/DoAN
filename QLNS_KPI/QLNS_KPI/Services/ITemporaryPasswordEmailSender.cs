namespace QLNS_KPI.Services;

public interface ITemporaryPasswordEmailSender
{
    Task SendAsync(string email, string username, string temporaryPassword, CancellationToken cancellationToken);
}
