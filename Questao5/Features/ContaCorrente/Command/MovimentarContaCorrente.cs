using Dapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Questao5.Commons;
using Questao5.Features.ContaCorrente.Domains;
using Questao5.Features.ContaCorrente.Services;
using Questao5.Infrastructure.DbConnectionFactory;
using System.Data;

namespace Questao5.Features.ContaCorrente.Command;

public sealed record MovimentarContaCorrenteRequest(Guid IdempotenciaId,
                                                    int NumeroConta,
                                                    decimal Valor,
                                                    string TipoMovimento) : IRequest<MovimentarContaCorrenteResponse>;

public sealed class MovimentarContaCorrenteResponse
{
    public string IdMovimento { get; init; } = default!;
}

public sealed class MovimentarContaCorrenteEndpoint : IEndpoint
{
    public static void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/conta-corrente/movimentar",
            async ([FromBody] MovimentarContaCorrenteRequest movimentarContaCorrenteRequest, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(movimentarContaCorrenteRequest, cancellationToken);
                return Results.Ok(result);
            })
        .WithName("MovimentarContaCorrente")
        .Produces<MovimentarContaCorrenteResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithTags("ContaCorrente");
    }
}

internal sealed class MovimentarContaCorrenteHandler(IDbConnectionFactory dbConnectionFactory, IContaCorrenteService contaCorrenteService) : IRequestHandler<MovimentarContaCorrenteRequest, MovimentarContaCorrenteResponse>
{
    public async Task<MovimentarContaCorrenteResponse> Handle(MovimentarContaCorrenteRequest movimentarContaCorrenteRequest, CancellationToken cancellationToken)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var idempotenciaExistente = await VerificarIdempotenciaExiste(connection, movimentarContaCorrenteRequest, transaction);
        if (idempotenciaExistente is not null)
        {
            var resultado = System.Text.Json.JsonSerializer.Deserialize<MovimentarContaCorrenteResponse>(idempotenciaExistente.Resultado);
            return resultado!;
        }

        var conta = await contaCorrenteService.BuscarContaCorrenteAsync(connection, movimentarContaCorrenteRequest.NumeroConta, transaction);
        ValidarMovimentacao(conta, movimentarContaCorrenteRequest);

        var idMovimento = Guid.NewGuid();

        await InserirMovimento(connection, idMovimento, conta, movimentarContaCorrenteRequest, transaction);

        var movimentarContaCorrenteResponse = new MovimentarContaCorrenteResponse { IdMovimento = idMovimento.ToString() };

        await InserirIdempotencia(connection, idempotenciaExistente, movimentarContaCorrenteRequest, movimentarContaCorrenteResponse, transaction);

        transaction.Commit();

        return movimentarContaCorrenteResponse;
    }

    private async Task InserirIdempotencia(IDbConnection connection, VerificarIdempotenciaDto idempotencia, MovimentarContaCorrenteRequest movimentarContaCorrenteRequest, MovimentarContaCorrenteResponse movimentarContaCorrenteResponse, IDbTransaction transaction)
    {
        await connection.ExecuteAsync(@"INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
                                        VALUES (@Chave, @Requisicao, @Resultado)",
                                        new
                                        {
                                            Chave = movimentarContaCorrenteRequest.IdempotenciaId.ToString(),
                                            Requisicao = System.Text.Json.JsonSerializer.Serialize(movimentarContaCorrenteRequest),
                                            Resultado = System.Text.Json.JsonSerializer.Serialize(movimentarContaCorrenteResponse)
                                        }, transaction);
    }

    private async Task<VerificarIdempotenciaDto?> VerificarIdempotenciaExiste(IDbConnection connection, MovimentarContaCorrenteRequest movimentarContaCorrenteRequest, IDbTransaction transaction)
    {
        return await connection.QueryFirstOrDefaultAsync<VerificarIdempotenciaDto>(@"SELECT chave_idempotencia AS Chave, requisicao AS Requisicao, resultado AS Resultado
                                                                                       FROM idempotencia
                                                                                      WHERE chave_idempotencia = @IdempotenciaId",
                                                                                    new { IdempotenciaId = movimentarContaCorrenteRequest.IdempotenciaId.ToString() }, transaction);
    }

    private async Task InserirMovimento(IDbConnection connection, Guid idMovimento, ContaCorrenteDto conta, MovimentarContaCorrenteRequest movimentarContaCorrenteRequest, IDbTransaction transaction)
    {
        await connection.ExecuteAsync(@"INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor)
                                        VALUES (@IdMovimento, @IdConta, @DataMovimento, @TipoMovimento, @Valor)",
                                        new
                                        {
                                            IdMovimento = idMovimento.ToString(),
                                            IdConta = conta.IdContaCorrente,
                                            DataMovimento = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                                            TipoMovimento = movimentarContaCorrenteRequest.TipoMovimento,
                                            Valor = movimentarContaCorrenteRequest.Valor
                                        }, transaction);
    }

    private void ValidarMovimentacao(ContaCorrenteDto conta, MovimentarContaCorrenteRequest movimentarContaCorrenteRequest)
    {
        ContaCorrenteValidator.ValidarContaExistente(conta);
        ContaCorrenteValidator.ValidarContaAtiva(conta);
        ContaCorrenteValidator.ValidarValorMovimentacao(movimentarContaCorrenteRequest.Valor);
        ContaCorrenteValidator.ValidarTipoMovimento(movimentarContaCorrenteRequest.TipoMovimento);
    }
}