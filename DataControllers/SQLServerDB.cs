using System.Data;
using System.Data.SqlClient;

namespace DataControllers
{
    public class SQLServerDB
    {
        private SqlConnection connection;
        private SqlCommand command;
        private SqlDataAdapter dataAdapter;
        private DataTable dateTable;

        public SQLServerDB(string sqlConnectionString)
        {
            connection = new SqlConnection(sqlConnectionString);
        }

        public SqlConnection Connection { get => (connection != null) ? connection : null;}

        public SqlCommand CommandLast { get => command; }

        public SqlCommand Command(string sqlScript)
        {
            return command = new SqlCommand(sqlScript, connection);
        }
    }
}
