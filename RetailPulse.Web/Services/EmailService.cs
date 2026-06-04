using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace RetailPulse.Web.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private bool IsEnabled => _config.GetValue<bool>("Email:Enabled");

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        if (!IsEnabled)
        {
            _logger.LogInformation("Email disabled. Would have sent '{Subject}' to {Email}",
                subject, toEmail);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["Email:FromName"],
                _config["Email:FromEmail"]));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["Email:Host"],
                _config.GetValue<int>("Email:Port"),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                _config["Email:Username"],
                _config["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent: '{Subject}' to {Email}", subject, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }

    // ─── Email Templates ──────────────────────────────────

    public async Task SendLowStockAlertAsync(string toEmail, string productName,
        int currentStock, int reorderLevel, string supplierName)
    {
        var subject = $"Low Stock Alert — {productName}";
        var html = $@"
        <div style='font-family: Nunito Sans, sans-serif; max-width: 600px; margin: 0 auto;
                    background: #0F172A; color: #F8FAFC; padding: 32px; border-radius: 12px;'>
            <div style='display: flex; align-items: center; gap: 12px; margin-bottom: 24px;'>
                <div style='width: 40px; height: 40px; background: #22C55E; border-radius: 10px;
                            display: flex; align-items: center; justify-content: center;
                            font-weight: 700; font-size: 16px; color: white;'>RP</div>
                <div>
                    <div style='font-size: 18px; font-weight: 700;'>RetailPulse</div>
                    <div style='font-size: 12px; color: #64748B;'>ERP Management System</div>
                </div>
            </div>
            <div style='background: rgba(245,158,11,0.1); border: 1px solid rgba(245,158,11,0.3);
                        border-radius: 10px; padding: 20px; margin-bottom: 24px;'>
                <div style='font-size: 14px; font-weight: 700; color: #F59E0B;
                            margin-bottom: 8px;'>Low Stock Alert</div>
                <div style='font-size: 24px; font-weight: 700; margin-bottom: 4px;'>
                    {productName}
                </div>
                <div style='font-size: 13px; color: #94A3B8;'>
                    Supplier: {supplierName}
                </div>
            </div>
            <table style='width: 100%; border-collapse: collapse; margin-bottom: 24px;'>
                <tr style='border-bottom: 1px solid #1E293B;'>
                    <td style='padding: 12px 0; color: #64748B; font-size: 13px;'>
                        Current Stock
                    </td>
                    <td style='padding: 12px 0; font-weight: 700; color: #EF4444;
                                text-align: right; font-family: monospace;'>
                        {currentStock} units
                    </td>
                </tr>
                <tr>
                    <td style='padding: 12px 0; color: #64748B; font-size: 13px;'>
                        Reorder Level
                    </td>
                    <td style='padding: 12px 0; font-weight: 700;
                                text-align: right; font-family: monospace;'>
                        {reorderLevel} units
                    </td>
                </tr>
            </table>
            <a href='#' style='display: inline-block; background: #22C55E; color: white;
                                padding: 12px 24px; border-radius: 8px; text-decoration: none;
                                font-weight: 700; font-size: 14px;'>
                Go to Inventory
            </a>
            <div style='margin-top: 24px; font-size: 11px; color: #475569;'>
                This is an automated alert from RetailPulse ERP.
            </div>
        </div>";

        await SendAsync(toEmail, "Store Manager", subject, html);
    }

    public async Task SendNewOrderNotificationAsync(string toEmail, int orderId,
        string customerName, decimal total, int itemCount)
    {
        var subject = $"New Order #{orderId} — {customerName}";
        var html = $@"
        <div style='font-family: Nunito Sans, sans-serif; max-width: 600px; margin: 0 auto;
                    background: #0F172A; color: #F8FAFC; padding: 32px; border-radius: 12px;'>
            <div style='display: flex; align-items: center; gap: 12px; margin-bottom: 24px;'>
                <div style='width: 40px; height: 40px; background: #22C55E; border-radius: 10px;
                            display: flex; align-items: center; justify-content: center;
                            font-weight: 700; font-size: 16px; color: white;'>RP</div>
                <div>
                    <div style='font-size: 18px; font-weight: 700;'>RetailPulse</div>
                    <div style='font-size: 12px; color: #64748B;'>ERP Management System</div>
                </div>
            </div>
            <div style='background: rgba(34,197,94,0.1); border: 1px solid rgba(34,197,94,0.3);
                        border-radius: 10px; padding: 20px; margin-bottom: 24px;'>
                <div style='font-size: 14px; font-weight: 700; color: #22C55E;
                            margin-bottom: 8px;'>New Order Received</div>
                <div style='font-size: 24px; font-weight: 700; margin-bottom: 4px;'>
                    Order #{orderId}
                </div>
                <div style='font-size: 13px; color: #94A3B8;'>Customer: {customerName}</div>
            </div>
            <table style='width: 100%; border-collapse: collapse; margin-bottom: 24px;'>
                <tr style='border-bottom: 1px solid #1E293B;'>
                    <td style='padding: 12px 0; color: #64748B; font-size: 13px;'>
                        Order Total
                    </td>
                    <td style='padding: 12px 0; font-weight: 700; color: #22C55E;
                                text-align: right; font-family: monospace;'>
                        ${total:N2}
                    </td>
                </tr>
                <tr>
                    <td style='padding: 12px 0; color: #64748B; font-size: 13px;'>
                        Items
                    </td>
                    <td style='padding: 12px 0; font-weight: 700;
                                text-align: right; font-family: monospace;'>
                        {itemCount} items
                    </td>
                </tr>
            </table>
            <a href='#' style='display: inline-block; background: #22C55E; color: white;
                                padding: 12px 24px; border-radius: 8px; text-decoration: none;
                                font-weight: 700; font-size: 14px;'>
                View Order
            </a>
            <div style='margin-top: 24px; font-size: 11px; color: #475569;'>
                This is an automated notification from RetailPulse ERP.
            </div>
        </div>";

        await SendAsync(toEmail, "Store Manager", subject, html);
    }

    public async Task SendOrderStatusChangeAsync(string toEmail, int orderId,
        string customerName, string newStatus, decimal total)
    {
        var statusColor = newStatus == "Completed" ? "#22C55E" : "#EF4444";
        var subject = $"Order #{orderId} {newStatus} — {customerName}";
        var html = $@"
        <div style='font-family: Nunito Sans, sans-serif; max-width: 600px; margin: 0 auto;
                    background: #0F172A; color: #F8FAFC; padding: 32px; border-radius: 12px;'>
            <div style='display: flex; align-items: center; gap: 12px; margin-bottom: 24px;'>
                <div style='width: 40px; height: 40px; background: #22C55E; border-radius: 10px;
                            display: flex; align-items: center; justify-content: center;
                            font-weight: 700; font-size: 16px; color: white;'>RP</div>
                <div>
                    <div style='font-size: 18px; font-weight: 700;'>RetailPulse</div>
                    <div style='font-size: 12px; color: #64748B;'>ERP Management System</div>
                </div>
            </div>
            <div style='background: rgba(59,130,246,0.1); border: 1px solid rgba(59,130,246,0.3);
                        border-radius: 10px; padding: 20px; margin-bottom: 24px;'>
                <div style='font-size: 14px; font-weight: 700; color: #3B82F6;
                            margin-bottom: 8px;'>Order Status Updated</div>
                <div style='font-size: 24px; font-weight: 700; margin-bottom: 4px;'>
                    Order #{orderId}
                </div>
                <div style='font-size: 13px; color: #94A3B8;'>Customer: {customerName}</div>
            </div>
            <table style='width: 100%; border-collapse: collapse; margin-bottom: 24px;'>
                <tr style='border-bottom: 1px solid #1E293B;'>
                    <td style='padding: 12px 0; color: #64748B; font-size: 13px;'>
                        New Status
                    </td>
                    <td style='padding: 12px 0; font-weight: 700;
                                color: {statusColor}; text-align: right;'>
                        {newStatus}
                    </td>
                </tr>
                <tr>
                    <td style='padding: 12px 0; color: #64748B; font-size: 13px;'>
                        Order Total
                    </td>
                    <td style='padding: 12px 0; font-weight: 700;
                                text-align: right; font-family: monospace;'>
                        ${total:N2}
                    </td>
                </tr>
            </table>
            <div style='margin-top: 24px; font-size: 11px; color: #475569;'>
                This is an automated notification from RetailPulse ERP.
            </div>
        </div>";

        await SendAsync(toEmail, "Store Manager", subject, html);
    }
}