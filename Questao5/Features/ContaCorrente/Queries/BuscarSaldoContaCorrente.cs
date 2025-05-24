using Dapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Questao5.Commons;
using Questao5.Features.ContaCorrente.Domains;
using Questao5.Features.ContaCorrente.Services;
using Questao5.Infrastructure.DbConnectionFactory;

namespace Questao5.Features.ContaCorrente.Queries;

public sealed record BuscarSaldoContaCorrenteRequest(int NumeroConta) : IRequest<BuscarSaldoContaCorrenteResponse>;

public sealed class BuscarSaldoContaCorrenteResponse
{
    public int Numero { get; init; }
    public string Nome { get; init; } = default!;
    public DateTime DataResposta { get; init; }
    public decimal Saldo { get; init; }
}

public sealed class BuscarSaldoContaCorrente : IEndpoint
{
    public static void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/conta-corrente/saldo",
            async ([FromQuery] int numeroConta, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new BuscarSaldoContaCorrenteRequest(numeroConta), cancellationToken);
                return Results.Ok(result);
            })
        .WithName("BuscarSaldoContaCorrente")
        .Produces<BuscarSaldoContaCorrenteResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithTags("ContaCorrente");
    }
}

internal class Handler(IDbConnectionFactory dbConnectionFactory, IContaCorrenteService contaCorrenteService): IRequestHandler<BuscarSaldoContaCorrenteRequest, BuscarSaldoContaCorrenteResponse>
{
    public async Task<BuscarSaldoContaCorrenteResponse> Handle(BuscarSaldoContaCorrenteRequest request, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var conta = await contaCorrenteService.BuscarContaCorrenteAsync(connection, request.NumeroConta, transaction);

        ValidarConta(conta);
        
        var movimentos = await BuscarMovimentos(connection, conta);

        var saldo = CalcularSaldo(movimentos);

        return new BuscarSaldoContaCorrenteResponse
        {
            Numero = conta.Numero,
            Nome = conta.Nome,
            DataResposta = DateTime.UtcNow,
            Saldo = saldo
        };
    }

    private decimal CalcularSaldo(IEnumerable<MovimentoDto> movimentos)
    {
        decimal creditos = movimentos.Where(x => x.TipoMovimento == "C").Sum(x => x.Valor);
        decimal debitos = movimentos.Where(x => x.TipoMovimento == "D").Sum(x => x.Valor);
        return creditos - debitos;
    }

    private async Task<IEnumerable<MovimentoDto>> BuscarMovimentos(System.Data.IDbConnection connection, dynamic? conta)
    {
        return await connection.QueryAsync<MovimentoDto>(@"SELECT tipomovimento, valor 
                                                             FROM movimento 
                                                            WHERE idcontacorrente = @IdConta",
                                                           new { IdConta = conta.IdContaCorrente });
    }

    private void ValidarConta(dynamic? conta)
    {
        ContaCorrenteValidator.ValidarContaExistente(conta);
        ContaCorrenteValidator.ValidarContaAtiva(conta);
    }
}