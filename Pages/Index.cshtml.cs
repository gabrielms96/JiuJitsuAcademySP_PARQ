using JiuJitsuAcademy.Models;
using JiuJitsuAcademy.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JiuJitsuAcademy.Pages;

public class IndexModel : PageModel
{
    private static readonly string[] FormatosImagemPermitidos =
        { "image/jpeg", "image/jpg", "image/png", "image/webp" };
    private const long TamanhoMaximoSelfie = 5 * 1024 * 1024; // 5 MB

    private readonly IEmailService _emailService;
    private readonly IRecaptchaService _recaptchaService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IEmailService emailService, IRecaptchaService recaptchaService, ILogger<IndexModel> logger)
    {
        _emailService = emailService;
        _recaptchaService = recaptchaService;
        _logger = logger;
    }

    [BindProperty]
    public MatriculaFormModel Form { get; set; } = new();

    public bool RecaptchaEnabled => _recaptchaService.Enabled;
    public string RecaptchaSiteKey => _recaptchaService.SiteKey;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        ValidarSelfie();

        var token = Request.Form["g-recaptcha-response"].ToString();
        if (!await _recaptchaService.ValidarAsync(token, cancellationToken))
        {
            ModelState.AddModelError(string.Empty,
                "Falha na verificacao de seguranca (captcha). Tente novamente.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Form.EnderecoCompleto = Form.MontarEnderecoCompleto();

        try
        {
            await _emailService.EnviarMatriculaAsync(Form, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar e-mail de dados para aula introdutoria e PAR-Q.");
            ModelState.AddModelError(string.Empty,
                "Nao foi possivel enviar o formulario no momento. Tente novamente em instantes.");
            return Page();
        }

        return RedirectToPage("Sucesso");
    }

    private void ValidarSelfie()
    {
        var selfie = Form.Selfie;
        if (selfie is null || selfie.Length == 0)
        {
            ModelState.AddModelError("Form.Selfie", "Envie uma selfie para confirmacao facial.");
            return;
        }

        if (selfie.Length > TamanhoMaximoSelfie)
        {
            ModelState.AddModelError("Form.Selfie", "A imagem deve ter no maximo 5 MB.");
        }

        if (!FormatosImagemPermitidos.Contains(selfie.ContentType.ToLowerInvariant()))
        {
            ModelState.AddModelError("Form.Selfie", "Envie uma imagem nos formatos JPG, PNG ou WEBP.");
        }
    }
}
