using Questao5.Commons;

namespace Questao5.Features.ContaCorrente.Domains;

public static class ContaCorrenteValidator
{
    public static void ValidarContaExistente(ContaCorrenteDto conta)
    {
        if (conta == null)
            throw new ValidationException("Conta não encontrada", "INVALID_ACCOUNT");
    }

    public static void ValidarContaAtiva(ContaCorrenteDto conta)
    {
        if (conta.Ativo == 0)
            throw new ValidationException("Conta inativa", "INACTIVE_ACCOUNT");
    }

    public static void ValidarValorMovimentacao(decimal valor)
    {
        if (valor <= 0)
            throw new ValidationException("Valor deve ser positivo", "INVALID_VALUE");
    }

    public static void ValidarTipoMovimento(string tipoMovimento)
    {
        if (tipoMovimento != "C" && tipoMovimento != "D")
            throw new ValidationException("Tipo de movimento inválido", "INVALID_TYPE");
    }
}