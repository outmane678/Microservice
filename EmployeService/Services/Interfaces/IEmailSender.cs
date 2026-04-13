namespace EmployeService.Services.Interfaces;

public interface IEmailSender
{
    void SendAccountCreationEmail(string to, string token);
}
