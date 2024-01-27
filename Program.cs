using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Npgsql;

public class Program
{
    public static void Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder().SetBasePath(System.AppContext.BaseDirectory).AddJsonFile("appsettings.json").Build();

        // Establish the connection to the database and initialize the work queue
        SqlUtils sqlUtils = new SqlUtils(configuration);
        Queue workQueue = new Queue();

        // Initialize the example state machine
        initializeExampleStateMachine(sqlUtils);

        NpgsqlConnection connection = sqlUtils.GetNewConnection();
        connection.Open();

        // Hydrate the work queue with active workflows
        Hydrate hydrate = new Hydrate(connection, workQueue);
        hydrate.PushActiveWorkflowsToWorkQueue();
  
        FiniteStateMachineContext finiteStateMachineContext = new FiniteStateMachineContext(workQueue, connection);
        finiteStateMachineContext.ProgressStateMachine();
    }
    
    private static string ConvertCSharpTypeToSqlType(string typeName)
    {
        if (typeName == "String")
        {
            return "VARCHAR";
        }
        else if (typeName == "Int32")
        {
            return "INT";
        }
        else
        {
            throw new NotSupportedException($"Conversion from C# type '{typeName}' to SQL type is not supported.");
        }
    }

    private static void initializeExampleStateMachine(SqlUtils sqlUtils)
    {
        // Initialize the HelloWorldStateMachine
        HelloWorldStateMachine helloWorldStateMachine = new HelloWorldStateMachine("John", 25)
        {
            Id = RandomNumberGenerator.GetInt32(0, 1000000),
            // By default 0
            State = 0
        };

        // Save the context to the database
        FSM.Attributes.TableAttribute tableAttribute = (FSM.Attributes.TableAttribute) typeof(HelloWorldStateMachine).GetCustomAttribute(typeof(FSM.Attributes.TableAttribute));
        NpgsqlConnection connection = sqlUtils.GetNewConnection();
        connection.Open();
        
        string query = $"DROP TABLE IF EXISTS {tableAttribute.Name}";
        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        {
            command.ExecuteNonQuery();
        }

        query = $"DROP TABLE IF EXISTS active_workflows";
        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        {
            command.ExecuteNonQuery();
        }
        
        // Insert the state machine into the database
        query = $"CREATE TABLE {tableAttribute.Name} (";
        foreach(PropertyInfo propertyInfo in typeof(HelloWorldStateMachine).GetProperties())
        {
            query += $"{propertyInfo.Name} {ConvertCSharpTypeToSqlType(propertyInfo.PropertyType.Name)},";
        }
        query = query.TrimEnd(',');
        query += ");";
        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        {
            command.ExecuteNonQuery();
        }
        
        // Populate with initial values
        query = $"INSERT INTO {tableAttribute.Name} VALUES (";
        foreach(PropertyInfo propertyInfo in typeof(HelloWorldStateMachine).GetProperties())
        {
            if (propertyInfo.PropertyType == typeof(string))
            {
                query += $"'{propertyInfo.GetValue(helloWorldStateMachine)}',";
            }
            else
            {
                query += $"{propertyInfo.GetValue(helloWorldStateMachine)},";
            }
        }
        query = query.TrimEnd(',');
        query += ");";
        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        {
            command.ExecuteNonQuery();
        }

        // push to active workflows
        query = $"CREATE TABLE active_workflows (workflow_id int, stateMachineType varchar(255));";
        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        {
            command.ExecuteNonQuery();
        }

        query = $"INSERT INTO active_workflows VALUES ({helloWorldStateMachine.Id}, '{helloWorldStateMachine.GetType().FullName}');";
        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        {
            command.ExecuteNonQuery();
        }

        connection.Close();
    }
}