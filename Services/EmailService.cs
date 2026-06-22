using JiuJitsuAcademy.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text;
using static System.Collections.Specialized.BitVector32;

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
        message.Headers.Replace(HeaderId.Subject, Encoding.UTF8, "Your International Subject: áéíóú Üñ");
        message.Subject = $"Dados para aula Introdutória e PAR-Q - {form.NomeCompleto}";

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

        _logger.LogInformation("Dados para aula introdutória e PAR-Q enviados por e-mail (aluno: {Nome}).",
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
        Linha(sb, "CEP", f.Cep);
        Linha(sb, "Endereço", f.Logradouro);
        Linha(sb, "Número", f.Numero);
        Linha(sb, "Complemento", f.Complemento);
        Linha(sb, "Bairro", f.Bairro);
        Linha(sb, "Cidade", f.Cidade);
        Linha(sb, "Estado", f.Estado);
        Linha(sb, "Endereço Completo", f.EnderecoCompleto);
        Linha(sb, "CPF", f.Cpf);
        Linha(sb, "Estado Civil", f.EstadoCivil);
        Linha(sb, "Profissão", f.Profissao);
        Linha(sb, "Filhos", f.Filhos);
        Linha(sb, "WhatsApp", f.Whatsapp);
        Linha(sb, "Contato para Urgência", f.ContatoUrgencia);
        Linha(sb, "Convênio Médico", f.ConvenioMedico);
        Linha(sb, "Esportes já praticados", f.EsportesPraticados);

        Secao(sb, "Jiu-Jitsu");
        Linha(sb, "Já treinou antes?", f.JaTreinou);
        Linha(sb, "Graduação", f.Graduacao);
        Linha(sb, "Equipes anteriores", f.EquipesAnteriores);

        Secao(sb, "PAR-Q - Questionário de Prontidão para Atividade Física");
        Linha(sb, "1. Problema cardíaco / prescrição medica", f.ParQ1);
        Linha(sb, "2. Dor no tórax ao praticar atividade", f.ParQ2);
        Linha(sb, "3. Dor torácica em repouso (último mês)", f.ParQ3);
        Linha(sb, "4. Tonturas / perda de consciência", f.ParQ4);
        Linha(sb, "5. Problema ósseo ou articular", f.ParQ5);
        Linha(sb, "6. Medicamento para pressão/cardiovascular", f.ParQ6);
        Linha(sb, "7. Outra razão física impeditiva", f.ParQ7);

        Secao(sb, "Declarações");
        Linha(sb, "Declaração de Responsabilidade", f.DeclaracaoResponsabilidade);

        if (!string.IsNullOrWhiteSpace(f.ResponsavelNome) ||
            !string.IsNullOrWhiteSpace(f.ResponsavelCpf) ||
            !string.IsNullOrWhiteSpace(f.ResponsavelParentesco))
        {
            Secao(sb, "Responsável (menor de 18 anos)");
            Linha(sb, "Nome do responsável", f.ResponsavelNome);
            Linha(sb, "CPF do responsável", f.ResponsavelCpf);
            Linha(sb, "Grau de parentesco", f.ResponsavelParentesco);
        }

        if (!string.IsNullOrWhiteSpace(f.TermoNomeParticipante) ||
            !string.IsNullOrWhiteSpace(f.TermoNomeResponsavel))
        {
            Secao(sb, "Termo de Responsabilidade (PAR-Q)");
            Linha(sb, "Nome do participante", f.TermoNomeParticipante);
            Linha(sb, "Nome do responsável", f.TermoNomeResponsavel);
        }

        sb.Append($"""
                <p style="margin-top:24px;font-size:12px;color:#888;">
                  Enviado em {DateTime.Now:dd/MM/yyyy 'as' HH:mm}. A confirmação facial (selfie) segue em anexo, quando enviada.
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
