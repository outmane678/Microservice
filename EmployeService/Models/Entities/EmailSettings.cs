namespace EmployeService.Models.Entities;


public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string BackendOrigin { get; set; } = string.Empty;
    public bool EnableAuth { get; set; }
    public bool EnableStartTls { get; set; }
    public string SslTrust { get; set; } = string.Empty;
}