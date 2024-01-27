using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Npgsql;

public class SqlUtils
{
    private readonly string _connectionString;
    private NpgsqlConnection _connection;

    public SqlUtils(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    public NpgsqlConnection GetNewConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
