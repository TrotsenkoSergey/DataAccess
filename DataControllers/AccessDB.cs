using System.Data.OleDb;
using System.Data;
using System.Data.Common;
using System.Configuration;

namespace DataControllers
{
    public class AccessDB
    {
        private OleDbConnection connection;
        private OleDbCommand command;
        private string conStr;

        public AccessDB(string accessConnectionString)
        {
            connection = new OleDbConnection(accessConnectionString);
        }

        public AccessDB()
        {
            conStr = ConfigurationManager.ConnectionStrings["connStrAccessDB"].ConnectionString;

            connection = new OleDbConnection(conStr);
        }

        public string ConnectionString { get => conStr; }

        public OleDbConnection Connection { get => (connection != null) ? connection : null; }

        public OleDbCommand CommandLast { get => command; }

        public OleDbCommand Command(string sqlScript)
        {
            return command = new OleDbCommand(sqlScript, connection);
        }

        public OleDbDataAdapter DataAdapter { get; set; } = new OleDbDataAdapter();

        public DataTable DataTable { get; set; } = new DataTable();
    }
}
