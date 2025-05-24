using Dapper;
using Questao5.Features.ContaCorrente.Domains;
using System.Data;

namespace Questao5.Features.ContaCorrente.Services;

public class ContaCorrenteService() : IContaCorrenteService
{
    public async Task<ContaCorrenteDto?> BuscarContaCorrenteAsync(IDbConnection connection, int numeroConta, IDbTransaction transaction)
    {
        return await connection.QueryFirstOrDefaultAsync<ContaCorrenteDto>(@"SELECT idcontacorrente AS IdContaCorrente,
                                                                                    numero AS Numero,
                                                                                    nome AS Nome, 
                                                                                    ativo AS Ativo
                                                                               FROM contacorrente 
                                                                              WHERE numero = @numeroConta",
                                                                             new { numeroConta }, transaction
        );
    }
}
