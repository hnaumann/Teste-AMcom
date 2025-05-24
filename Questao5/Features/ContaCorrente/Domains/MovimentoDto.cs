namespace Questao5.Features.ContaCorrente.Domains;

internal sealed class MovimentoDto
{
    public string TipoMovimento { get; init; } = default!;
    public decimal Valor { get; init; }
}