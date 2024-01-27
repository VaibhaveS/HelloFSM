using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using FSM;
using Npgsql;

public class FiniteStateMachineContext : Hydrate
{
    public FiniteStateMachineContext(Queue WorkQueue, NpgsqlConnection Connection) : base(Connection, WorkQueue)
    {
    }

    public void ProgressStateMachine()
    {
        (Type stateMachineType, int id) = WorkQueue.Dequeue();

        FSM.Attributes.TableAttribute? tableAttribute = Attribute.GetCustomAttribute(stateMachineType, typeof(FSM.Attributes.TableAttribute)) as FSM.Attributes.TableAttribute;

        if (tableAttribute is null)
        {
            throw new InvalidOperationException("FSM.Attributes.TableAttribute is null.");
        }

        string tableName = tableAttribute.Name;
        Console.WriteLine(tableName);

        string query = $"SELECT * FROM {tableName} where id = {id}";

        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        {
            using (NpgsqlDataReader  reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    object? stateMachineObject = Activator.CreateInstance(stateMachineType);

                    if (stateMachineObject is null)
                    {
                        throw new InvalidOperationException("stateMachineObject is null.");
                    }

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string propertyName = reader.GetName(i);
                        object propertyValue = reader.GetValue(i);

                        var x = stateMachineType.GetProperties();
                        PropertyInfo? property = stateMachineType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        if (property != null && property.CanWrite)
                        {
                            property.SetValue(stateMachineObject, propertyValue);
                        }
                    }

                    // Use the stateMachineObject as needed
                    FiniteStateMachineMetaData metaData = StateMachineTypeMetaInformation[stateMachineType];
                    MethodInfo methodInfo = metaData.transitions[(Enum) stateMachineType.GetProperty("State").GetValue(stateMachineObject)];

                    // Invoke the transition method
                    Outcome actionOutcome = new Outcome();
                    methodInfo.Invoke(stateMachineObject, [actionOutcome]);
                }
            }
        }
    }

    private static string ConvertSqlTypeToCSharpType(string typeName)
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
}