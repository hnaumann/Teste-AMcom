using Questao5.Features.ContaCorrente.Domains;
using System.Data;

namespace Questao5.Features.ContaCorrente.Services;

public interface IContaCorrenteService
{
    Task<ContaCorrenteDto?> BuscarContaCorrenteAsync(IDbConnection connection, int numeroConta, IDbTransaction transaction);
}