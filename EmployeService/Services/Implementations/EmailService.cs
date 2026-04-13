using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using EmployeService.Services.Interfaces;

namespace EmployeService.Services.Implementations;

public class EmailService : IEmailSender
{
    // PLUS DE CHAMP _smtpClient - On crée une nouvelle connexion à chaque envoi
    private readonly string ORIGIN;
    private readonly string from;
    private readonly string _password;
    private readonly string _host;
    private readonly int _port;

    public EmailService(IConfiguration configuration)
    {
        ORIGIN = configuration["EmailSettings:BackendOrigin"] 
            ?? throw new InvalidOperationException("EmailSettings:BackendOrigin not configured");
        
        from = configuration["EmailSettings:Username"]
            ?? throw new InvalidOperationException("EmailSettings:Username not configured");
        
        _password = configuration["EmailSettings:Password"]
            ?? throw new InvalidOperationException("EmailSettings:Password not configured");
        
        _host = configuration["EmailSettings:Host"] ?? "smtp.gmail.com";
        _port = int.Parse(configuration["EmailSettings:Port"] ?? "587");
        
        // Ne PAS connecter ici!
    }

    public void SendAccountCreationEmail(string to, string token)
    {
        using var client = new SmtpClient();
        try
        {
            client.Connect(_host, _port, MailKit.Security.SecureSocketOptions.StartTls);
            client.Authenticate(from, _password);

            string link = $"{ORIGIN}/auth/signup?token={token}";
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Employee Service", from));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = "Activate your account";
            message.Body = new TextPart("plain")
            {
                Text = $"Please click the following link to activate your account: {link}"
            };

            client.Send(message);
            client.Disconnect(true);
        }
        catch
        {
            // Échec SMTP : ne pas remonter (la création employé est déjà persistée côté appelant).
        }
    }
}