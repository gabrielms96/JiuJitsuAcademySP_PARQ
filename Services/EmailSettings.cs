namespace JiuJitsuAcademy.Services;

public class EmailSettings
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string FromName { get; set; } = "Jiu-Jitsu Academy SP";
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public string AppPassword { get; set; } = string.Empty;
}
