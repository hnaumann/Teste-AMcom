using System.Data;

namespace Questao5.Infrastructure.DbConnectionFactory;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
