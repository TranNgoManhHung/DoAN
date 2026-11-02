
using System.Net.Sockets;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace QLNS_KPI.Services;

public sealed class SmtpTemporaryPasswordEmailSender(
    IConfiguration configuration,
    ILogger<SmtpTemporaryPasswordEmailSender> logger)
    : ITemporaryPasswordEmailSender
{
    public async Task SendAsync(
        string email,
        string username,
        string temporaryPassword,
        CancellationToken cancellationToken)
    {
        var host = configuration["Smtp:Host"];
        var fromEmail = configuration["Smtp:FromEmail"];
        var smtpUsername = configuration["Smtp:Username"];
        var smtpPassword = configuration["Smtp:Password"];
        var fromName = configuration["Smtp:FromName"] ?? "HUMG HR & KPI";
        var port = configuration.GetValue("Smtp:Port", 587);

        if (string.IsNullOrWhiteSpace(host)
            || string.IsNullOrWhiteSpace(fromEmail)
            || string.IsNullOrWhiteSpace(smtpUsername)
            || string.IsNullOrWhiteSpace(smtpPassword))
        {
            throw new InvalidOperationException(
                "SMTP configuration is incomplete.");
        }

        if (!string.Equals(
            fromEmail,
            smtpUsername,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "SMTP FromEmail must match SMTP Username when using Gmail.");
        }

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(email));

        message.Subject = "Thông tin đăng nhập HUMG HR & KPI";

        message.Body = new TextPart("plain")
        {
            Text = $"""
                Xin chào,

                Tên đăng nhập: {username}
                Mật khẩu tạm thời: {temporaryPassword}

                Mật khẩu tạm thời chỉ sử dụng được một lần.
                Sau khi đăng nhập, vui lòng đổi mật khẩu ngay.

                Nếu bạn không yêu cầu cấp tài khoản hoặc đặt lại mật khẩu,
                vui lòng liên hệ quản trị viên.

                Trân trọng,
                HUMG HR & KPI
                """
        };

        using var client = new SmtpClient
        {
            LocalDomain = "localhost",
            Timeout = 20000
        };

        try
        {
            await client.ConnectAsync(
                host,
                port,
                SecureSocketOptions.StartTls,
                cancellationToken);

            await client.AuthenticateAsync(
                smtpUsername,
                smtpPassword,
                cancellationToken);

            await client.SendAsync(message, cancellationToken);

            logger.LogInformation(
                "Email sent successfully to {Email}",
                email);

            try
            {
                await client.DisconnectAsync(true, cancellationToken);
            }
            catch (Exception ex) when (
                ex is SmtpCommandException or ProtocolException or IOException or SocketException)
            {
                logger.LogWarning(
                    ex,
                    "Email to {Email} was accepted by SMTP, but the connection did not close cleanly.",
                    email);
            }
        }
        catch (SmtpCommandException ex)
        {
            logger.LogError(
                ex,
                "SMTP rejected email to {Email}. StatusCode: {StatusCode}, ErrorCode: {ErrorCode}, Response: {Response}",
                email,
                ex.StatusCode,
                ex.ErrorCode,
                ex.Message);

            throw new InvalidOperationException(
                $"SMTP rejected the email. Status: {ex.StatusCode}. Response: {ex.Message}",
                ex);
        }
        catch (MailKit.Security.AuthenticationException ex)
        {
            logger.LogError(
                ex,
                "SMTP authentication failed for {Email}",
                email);

            throw new InvalidOperationException(
                "SMTP authentication failed. Please check Gmail credentials.",
                ex);
        }
        catch (Exception ex) when (
            ex is ProtocolException
                or IOException
                or OperationCanceledException)
        {
            logger.LogError(
                ex,
                "SMTP connection or sending failed for {Email}",
                email);

            throw;
        }
    }
}
