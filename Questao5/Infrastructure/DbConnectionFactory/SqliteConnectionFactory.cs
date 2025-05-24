using Microsoft.Data.Sqlite;
using Questao5.Infrastructure.Sqlite;
using System.Data;

namespace Questao5.Infrastructure.DbConnectionFactory;

public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(DatabaseConfig databaseConfig)
    {
        _connectionString = databaseConfig.Name;
    }

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
