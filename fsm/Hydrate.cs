using System.Data.SqlClient;

namespace HelloFSM
{
    public class Hydrate
    {
        public string ConnectionString { get; set; }
        public Queue WorkQueue { get; set; }

        public Hydrate(string connectionString, Queue workQueue)
        {
            ConnectionString = connectionString;
            WorkQueue = workQueue;
        }

        public void PushActiveWorkflowsToWorkQueue()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = "SELECT * FROM active_workflows";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Push to work queue
                            string workflowId = reader["workflow_id"].ToString();
                            string stateMachineType = reader["stateMachineType"].ToString();

                            if (workflowId is null || stateMachineType is null) {
                                throw new InvalidOperationException("workflow_id or stateMachineType is null.");
                            }

                            WorkQueue.Enqueue(Type.GetType(stateMachineType)!, int.Parse(workflowId));
                        }
                    }
                }
            }
        }
    }
}