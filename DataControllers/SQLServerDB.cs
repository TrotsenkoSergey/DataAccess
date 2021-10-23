using System.Data;
using System.Data.SqlClient;

namespace DataControllers
{
    public class SQLServerDB
    {
        private SqlConnection connection;
        private SqlCommand command;
        private SqlConnectionStringBuilder conStr;
        //private SqlDataAdapter dataAdapter;
        //private DataTable dateTable;

        public SQLServerDB(string sqlConnectionString)
        {
            connection = new SqlConnection(sqlConnectionString);
        }

        public SQLServerDB(string conStrLogin, string conStrPassword)
        {
            conStr = new SqlConnectionStringBuilder()
            {
                DataSource = "(local)",
                InitialCatalog = "SQLServDB",
                IntegratedSecurity = false,
                UserID = conStrLogin, //"user1",
                Password = conStrPassword, //"user1"
                //Pooling = false
            };

            connection = new SqlConnection(conStr.ConnectionString);
        }

        public string ConnectionString { get => conStr.ConnectionString; }

        public SqlConnection Connection { get => (connection != null) ? connection : null;}

        public SqlCommand CommandLast { get => command; }

        public SqlCommand Command(string sqlScript)
        {
            return command = new SqlCommand(sqlScript, connection);
        }

        public SqlDataAdapter DataAdapter { get; set; } = new SqlDataAdapter();

        public DataTable DataTable { get; set; } = new DataTable();
    }
}
