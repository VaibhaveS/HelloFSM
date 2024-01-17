using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class SqlUtils
{
    private readonly string _connectionString;
    private SqlConnection _connection;

    public SqlUtils(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _connection = new SqlConnection(_connectionString);
        _connection.Open();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    public SqlConnection GetNewConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
