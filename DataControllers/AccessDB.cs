using System.Data.OleDb;
using System.Data;

namespace DataControllers
{
    public class AccessDB
    {
        OleDbConnection connection;
        OleDbCommand command;
        OleDbDataAdapter dataAdapter;
        DataTable dateTable;
        public AccessDB(string accessConnectionString)
        {
            connection = new OleDbConnection(accessConnectionString);
        }

        public OleDbConnection Connection { get => connection; }

        public OleDbCommand CommandLast { get => command; }

        public OleDbCommand Command(string sqlScript)
        {
            return command = new OleDbCommand(sqlScript, connection);
        }
    }
}
