using GloboTicket.Services.Ordering.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GloboTicket.Services.Ordering.Saga;

// SMTP wrapper for the order-confirmation email. The Aspire Mailpit
// integration injects a connection string of the form
//   "Endpoint=smtp://host:port"
// into the consuming project under the connection name "mailpit".
//
// Connection is opened per send. Mailpit is local and idle reconnect is
// cheap; cached SmtpClient state would buy us nothing in this demo.
public class EmailSender
{
    private readonly Uri smtpEndpoint;
    private readonly ILogger<EmailSender> logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        this.logger = logger;

        // The connection string has the form "Endpoint=smtp://host:port".
        // Aspire's hosting integration produces this shape; we strip the
        // prefix and parse what's left.
        var raw = configuration.GetConnectionString("mailpit")
            ?? throw new InvalidOperationException("Missing 'mailpit' connection string.");
        var endpoint = raw.StartsWith("Endpoint=", StringComparison.OrdinalIgnoreCase)
            ? raw["Endpoint=".Length..]
            : raw;
        smtpEndpoint = new Uri(endpoint);
    }

    public async Task SendOrderConfirmation(Order order, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("GloboTicket", "noreply@globoticket.shop"));
        message.To.Add(new MailboxAddress(order.CustomerName, order.CustomerEmail));
        message.Subject = "Thank you for your order";

        var body = new BodyBuilder { HtmlBody = BuildBody(order) };
        message.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        // Mailpit doesn't require TLS; SecureSocketOptions.None matches
        // the dapr demo's binding setup.
        await client.ConnectAsync(smtpEndpoint.Host, smtpEndpoint.Port, SecureSocketOptions.None, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        logger.LogInformation("Sent order confirmation email for {OrderId} to {Email}",
            order.OrderId, order.CustomerEmail);
    }

    private static string BuildBody(Order order)
    {
        var lines = string.Join(Environment.NewLine, order.Lines.Select(l =>
            $"<li>{l.EventName} ({l.ArtistName}) ${l.Price} × {l.TicketCount}</li>"));

        return $"""
            <h2>Your order has been received</h2>
            <ul>{lines}</ul>
            <p>Total: ${order.Total}</p>
            <p>Your tickets are on the way! They will be delivered to:</p>
            {order.CustomerName}<br/>
            {order.ShippingAddress}<br/>
            {order.ShippingTown}<br/>
            {order.ShippingPostalCode}<br/>
            """;
    }
}
