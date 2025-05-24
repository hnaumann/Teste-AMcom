namespace Questao5.Features.ContaCorrente.Domains;

public sealed class ContaCorrenteDto
{
    public string IdContaCorrente { get; private set; }
    public int Numero { get; private set; }
    public string Nome { get; private set; }
    public int Ativo { get; private set; }
}
