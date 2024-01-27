using System.Reflection;
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
        string query = $"SELECT * FROM {tableName} where id = {id}";

        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        {
            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    FiniteStateMachine? stateMachineObject = (FiniteStateMachine?)Activator.CreateInstance(stateMachineType);

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
                    reader.Close();

                    // Use the stateMachineObject as needed
                    FiniteStateMachineMetaData metaData = StateMachineTypeMetaInformation[stateMachineType];
                    MethodInfo methodInfo = metaData.transitions[(int) stateMachineType.GetProperty("State").GetValue(stateMachineObject)];

                    // Invoke the transition method
                    Outcome actionOutcome = new Outcome();
                    methodInfo.Invoke(stateMachineObject, [actionOutcome]);

                    // If the actionOutcome.TargetState is not stable, populate the work queue
                    if (metaData.transitions.ContainsKey(actionOutcome.TargetState))
                    {
                        WorkQueue.Enqueue(stateMachineType, id);
                    } 
                    else 
                    {
                        // If the state machine is in a final state, remove it from active workflows
                        query = $"DELETE FROM active_workflows WHERE workflow_id = {id} AND stateMachineType = '{stateMachineType.Name}';";            
                        using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Update the state machine in the database
                    stateMachineObject.State = actionOutcome.TargetState;
                    foreach(PropertyInfo propertyInfo in typeof(HelloWorldStateMachine).GetProperties())
                    {            
                        if (propertyInfo.PropertyType == typeof(string))
                        {
                            query = $"UPDATE {tableName} SET {propertyInfo.Name} = '{propertyInfo.GetValue(stateMachineObject)}' WHERE id = {id};";
                        }
                        else
                        {
                            query = $"UPDATE {tableName} SET {propertyInfo.Name} = {propertyInfo.GetValue(stateMachineObject)} WHERE id = {id};";
                        }

                        using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}