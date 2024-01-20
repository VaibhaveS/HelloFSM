using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using FSM;

public class FiniteStateMachineContext : Hydrate
{
    public FiniteStateMachineContext(Queue WorkQueue, SqlConnection Connection) : base(Connection, WorkQueue)
    {
    }

    public void ProgressStateMachine()
    {
        (Type stateMachineType, int id) = WorkQueue.Dequeue();

        TableAttribute? tableAttribute = Attribute.GetCustomAttribute(stateMachineType, typeof(TableAttribute)) as TableAttribute;

        if (tableAttribute is null)
        {
            throw new InvalidOperationException("TableAttribute is null.");
        }

        string tableName = tableAttribute.Name;
        Console.WriteLine(tableName);

        string query = $"SELECT * FROM {tableName} where id = {id}";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.FieldCount != 1)
                {
                    throw new InvalidOperationException("FieldCount is not 1.");
                }

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

                        PropertyInfo? property = stateMachineType.GetProperty(propertyName);
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
}