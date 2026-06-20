using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace JiuJitsuAcademy.Models;

/// <summary>
/// Representa a ficha de Dados para aula Introdutoria e PAR-Q do aluno,
/// espelhando o formulario original do Google Forms da Jiu-Jitsu Academy SP.
/// </summary>
public class MatriculaFormModel
{
    // ----- Dados Pessoais -----

    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    [Display(Name = "Nome Completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de Nascimento")]
    public DateTime? DataNascimento { get; set; }

    [Required(ErrorMessage = "Informe o endereço completo.")]
    [StringLength(250)]
    [Display(Name = "Endereço Completo")]
    public string EnderecoCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CPF.")]
    [StringLength(14, MinimumLength = 11, ErrorMessage = "CPF inválido.")]
    [Display(Name = "CPF")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o estado civil.")]
    [Display(Name = "Estado Civil")]
    public string EstadoCivil { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a profissão.")]
    [StringLength(120)]
    [Display(Name = "Profissão")]
    public string Profissao { get; set; } = string.Empty;

    [Display(Name = "Filhos? Se sim, quantos e quais idades?")]
    [StringLength(250)]
    public string? Filhos { get; set; }

    [Required(ErrorMessage = "Informe o WhatsApp.")]
    [Phone(ErrorMessage = "Informe um número valido.")]
    [Display(Name = "WhatsApp")]
    public string Whatsapp { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe um contato para urgência.")]
    [Display(Name = "Contato para Urgencia")]
    [StringLength(150)]
    public string ContatoUrgencia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe se possui convênio médico.")]
    [Display(Name = "Tem Convênio Médico? Se sim, qual?")]
    [StringLength(150)]
    public string ConvenioMedico { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe os esportes já praticados.")]
    [Display(Name = "Esportes já praticados")]
    [StringLength(250)]
    public string EsportesPraticados { get; set; } = string.Empty;

    // ----- Confirmacao Facial -----

    [Display(Name = "Confirmacao Facial (selfie)")]
    public IFormFile? Selfie { get; set; }

    // ----- Jiu-Jitsu -----

    [Required(ErrorMessage = "Informe se já treinou Jiu-Jitsu antes.")]
    [Display(Name = "Já treinou Jiu-Jitsu antes?")]
    public string JaTreinou { get; set; } = string.Empty;

    [Display(Name = "Se já treinou, qual a graduação?")]
    public string? Graduacao { get; set; }

    [Display(Name = "Quais equipes já treinou?")]
    [StringLength(250)]
    public string? EquipesAnteriores { get; set; }

    // ----- PAR-Q (Questionario de Prontidao para Atividade Fisica) -----

    [Required(ErrorMessage = "Responda a pergunta 1.")]
    [Display(Name = "1. Alguma vez seu médico disse que você possui algum problema cardíaco e recomendou que você só praticasse atividade física sob prescrição médica?")]
    public string ParQ1 { get; set; } = string.Empty;

    [Required(ErrorMessage = "Responda a pergunta 2.")]
    [Display(Name = "2. Você sente dor no tórax quando pratica uma atividade física?")]
    public string ParQ2 { get; set; } = string.Empty;

    [Required(ErrorMessage = "Responda a pergunta 3.")]
    [Display(Name = "3. No último mês você sentiu dor torácica quando não estava praticando atividade física?")]
    public string ParQ3 { get; set; } = string.Empty;

    [Required(ErrorMessage = "Responda a pergunta 4.")]
    [Display(Name = "4. Você perdeu o equilíbrio em virtude de tonturas ou perdeu a consciência quando estava praticando atividade física?")]
    public string ParQ4 { get; set; } = string.Empty;

    [Required(ErrorMessage = "Responda a pergunta 5.")]
    [Display(Name = "5. Você tem algum problema ósseo ou articular que poderia ser agravado com a prática de atividades físicas?")]
    public string ParQ5 { get; set; } = string.Empty;

    [Required(ErrorMessage = "Responda a pergunta 6.")]
    [Display(Name = "6. Seu médico já recomendou o uso de medicamentos para controle da sua pressão arterial ou condição cardiovascular?")]
    public string ParQ6 { get; set; } = string.Empty;

    [Required(ErrorMessage = "Responda a pergunta 7.")]
    [Display(Name = "7. Você tem conhecimento de alguma outra razão física que o impeça de participar de atividades físicas?")]
    public string ParQ7 { get; set; } = string.Empty;

    // ----- Declaracao de Responsabilidade -----

    [Required(ErrorMessage = "Você precisa responder a declaração de responsabilidade.")]
    [Display(Name = "Declaracao de Responsabilidade")]
    public string DeclaracaoResponsabilidade { get; set; } = string.Empty;

    // ----- Dados do responsavel (menor de 18 anos) -----

    [Display(Name = "Nome do responsável (se menor de 18 anos)")]
    [StringLength(150)]
    public string? ResponsavelNome { get; set; }

    [Display(Name = "CPF do responsável (se menor de 18 anos)")]
    [StringLength(14)]
    public string? ResponsavelCpf { get; set; }

    [Display(Name = "Grau de parentesco do responsável (se menor de 18 anos)")]
    [StringLength(60)]
    public string? ResponsavelParentesco { get; set; }

    // ----- Termo PAR-Q -----

    [Display(Name = "Nome do(a) participante (Termo PAR-Q)")]
    [StringLength(150)]
    public string? TermoNomeParticipante { get; set; }

    [Display(Name = "Nome do responsável (Termo PAR-Q, se menor de 18 anos)")]
    [StringLength(150)]
    public string? TermoNomeResponsavel { get; set; }

    /// <summary>
    /// Opcoes reutilizadas pela view e validacao.
    /// </summary>
    public static class Opcoes
    {
        public static readonly string[] EstadoCivil =
            { "Solteiro(a)", "Casado(a)", "Divorciado(a)", "Viúvo(a)", "União Estável" };

        public static readonly string[] JaTreinou =
            { "Sim", "Nao", "Sim mas faz muito tempo" };

        public static readonly string[] Graduacao =
            { "Branca", "Azul", "Roxa", "Marrom", "Preta", "Graduação Kids (de Cinza a Verde)" };

        public static readonly string[] SimNao = { "Sim", "Não" };

        public static readonly string[] Declaracao = { "Concordo", "Não Concordo" };
    }
}
