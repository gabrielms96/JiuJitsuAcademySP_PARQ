using System.Text;
using JiuJitsuAcademy.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace JiuJitsuAcademy.Services;

public interface IEmailService
{
    Task EnviarMatriculaAsync(MatriculaFormModel form, CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _settings = configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
        _logger = logger;
    }

    public async Task EnviarMatriculaAsync(MatriculaFormModel form, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.AppPassword))
        {
            throw new InvalidOperationException(
                "App Password do Gmail nao configurado. Defina Email:AppPassword em User Secrets ou App Settings.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(form.Email));
        message.ReplyTo.Add(MailboxAddress.Parse(_settings.FromAddress));
        message.Cc.Add(MailboxAddress.Parse(_settings.FromAddress));
        message.Subject = $"Dados para aula Introdutoria e PAR-Q - {form.NomeCompleto}";

        var builder = new BodyBuilder
        {
            HtmlBody = MontarCorpoHtml(form)
        };

        // Anexa a selfie de confirmacao facial, se enviada.
        if (form.Selfie is { Length: > 0 })
        {
            using var ms = new MemoryStream();
            await form.Selfie.CopyToAsync(ms, cancellationToken);
            builder.Attachments.Add(form.Selfie.FileName, ms.ToArray(),
                ContentType.Parse(form.Selfie.ContentType));
        }

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort,
                SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_settings.FromAddress, _settings.AppPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true, cancellationToken);
            }
        }

        _logger.LogInformation("Dados para aula introdutoria e PAR-Q enviados por e-mail (aluno: {Nome}).",
            form.NomeCompleto);
    }

    private static string MontarCorpoHtml(MatriculaFormModel f)
    {
        var sb = new StringBuilder();
        sb.Append("""
            <div style="font-family:Arial,Helvetica,sans-serif;max-width:680px;margin:0 auto;color:#1a1a1a;">
              <div style="background:#0a0a0a;padding:24px;text-align:center;">
                <h1 style="color:#e11d2a;margin:0;font-size:22px;">JIU-JITSU ACADEMY SP</h1>
                <p style="color:#ffffff;margin:6px 0 0;font-size:14px;">Dados para aula Introdutoria e PAR-Q</p>
              </div>
              <div style="padding:24px;">
            """);

        Secao(sb, "Dados Pessoais");
        Linha(sb, "Nome Completo", f.NomeCompleto);
        Linha(sb, "E-mail", f.Email);
        Linha(sb, "Data de Nascimento", f.DataNascimento?.ToString("dd/MM/yyyy"));
        Linha(sb, "Endereco Completo", f.EnderecoCompleto);
        Linha(sb, "CPF", f.Cpf);
        Linha(sb, "Estado Civil", f.EstadoCivil);
        Linha(sb, "Profissao", f.Profissao);
        Linha(sb, "Filhos", f.Filhos);
        Linha(sb, "WhatsApp", f.Whatsapp);
        Linha(sb, "Contato para Urgencia", f.ContatoUrgencia);
        Linha(sb, "Convenio Medico", f.ConvenioMedico);
        Linha(sb, "Esportes ja praticados", f.EsportesPraticados);

        Secao(sb, "Jiu-Jitsu");
        Linha(sb, "Ja treinou antes?", f.JaTreinou);
        Linha(sb, "Graduacao", f.Graduacao);
        Linha(sb, "Equipes anteriores", f.EquipesAnteriores);

        Secao(sb, "PAR-Q - Questionario de Prontidao para Atividade Fisica");
        Linha(sb, "1. Problema cardiaco / prescricao medica", f.ParQ1);
        Linha(sb, "2. Dor no torax ao praticar atividade", f.ParQ2);
        Linha(sb, "3. Dor toracica em repouso (ultimo mes)", f.ParQ3);
        Linha(sb, "4. Tonturas / perda de consciencia", f.ParQ4);
        Linha(sb, "5. Problema osseo ou articular", f.ParQ5);
        Linha(sb, "6. Medicamento para pressao/cardiovascular", f.ParQ6);
        Linha(sb, "7. Outra razao fisica impeditiva", f.ParQ7);

        Secao(sb, "Declaracoes");
        Linha(sb, "Declaracao de Responsabilidade", f.DeclaracaoResponsabilidade);

        if (!string.IsNullOrWhiteSpace(f.ResponsavelNome) ||
            !string.IsNullOrWhiteSpace(f.ResponsavelCpf) ||
            !string.IsNullOrWhiteSpace(f.ResponsavelParentesco))
        {
            Secao(sb, "Responsavel (menor de 18 anos)");
            Linha(sb, "Nome do responsavel", f.ResponsavelNome);
            Linha(sb, "CPF do responsavel", f.ResponsavelCpf);
            Linha(sb, "Grau de parentesco", f.ResponsavelParentesco);
        }

        if (!string.IsNullOrWhiteSpace(f.TermoNomeParticipante) ||
            !string.IsNullOrWhiteSpace(f.TermoNomeResponsavel))
        {
            Secao(sb, "Termo de Responsabilidade (PAR-Q)");
            Linha(sb, "Nome do participante", f.TermoNomeParticipante);
            Linha(sb, "Nome do responsavel", f.TermoNomeResponsavel);
        }

        sb.Append($"""
                <p style="margin-top:24px;font-size:12px;color:#888;">
                  Enviado em {DateTime.Now:dd/MM/yyyy 'as' HH:mm}. A confirmacao facial (selfie) segue em anexo, quando enviada.
                </p>
              </div>
            </div>
            """);

        return sb.ToString();
    }

    private static void Secao(StringBuilder sb, string titulo) =>
        sb.Append($"""
            <h2 style="color:#e11d2a;font-size:16px;border-bottom:2px solid #e11d2a;padding-bottom:6px;margin:24px 0 12px;">
              {System.Net.WebUtility.HtmlEncode(titulo)}
            </h2>
            """);

    private static void Linha(StringBuilder sb, string rotulo, string? valor) =>
        sb.Append($"""
            <p style="margin:6px 0;font-size:14px;line-height:1.4;">
              <strong style="color:#0a0a0a;">{System.Net.WebUtility.HtmlEncode(rotulo)}:</strong>
              {System.Net.WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(valor) ? "-" : valor)}
            </p>
            """);
}
