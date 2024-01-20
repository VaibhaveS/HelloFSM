using System.Data.SqlClient;

public class Hydrate
{
    public SqlConnection connection { get; set; }
    public Queue WorkQueue { get; set; }
    public Dictionary<Type, FiniteStateMachineMetaData> StateMachineTypeMetaInformation { get; set; }
    public Hydrate(SqlConnection connection, Queue workQueue)
    {
        this.connection = connection;
        WorkQueue = workQueue;
        StateMachineTypeMetaInformation = new Dictionary<Type, FiniteStateMachineMetaData>();
    }

    public void PushActiveWorkflowsToWorkQueue()
    {
        string query = "SELECT * FROM active_workflows";

        using (SqlCommand command = new SqlCommand(query, connection))
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Push to work queue
                    string? workflowId = reader["workflow_id"].ToString();
                    string? stateMachineType = reader["stateMachineType"].ToString();

                    if (workflowId is null || stateMachineType is null) {
                        throw new InvalidOperationException("workflow_id or stateMachineType is null.");
                    }

                    //populate the metadata for the state machine type
                    //TODO: keep key as FSM id instead of type
                    if (!StateMachineTypeMetaInformation.ContainsKey(Type.GetType(stateMachineType)!))
                    {
                        StateMachineTypeMetaInformation.Add(Type.GetType(stateMachineType)!, new FiniteStateMachineMetaData(Type.GetType(stateMachineType)!));
                    }
                    
                    WorkQueue.Enqueue(Type.GetType(stateMachineType)!, int.Parse(workflowId));
                }
            }
        }
    }
}