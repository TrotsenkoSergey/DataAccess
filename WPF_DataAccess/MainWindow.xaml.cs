using DataControllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;

namespace WPF_DataAccess
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SQLServerDB sqlServerDB;
        private DataTable usersDataTable;
        private DataRowView usersRow;

        private AccessDB accessDB;
        private DataTable productsDataTable;
        private DataRowView productsRow;

        public MainWindow()
        {
            InitializeComponent();
            Preparing();
        }

        private void Preparing()
        {
            var logInWindow = new Window1();
            logInWindow.ShowDialog();

            string userID = logInWindow.textBoxUserId.Text;
            string password = logInWindow.textBoxPassword.Password;

            sqlServerDB = new SQLServerDB(userID, password);

            textBlockSQLConnString.Text = sqlServerDB.ConnectionString;

            sqlServerDB.Connection.StateChange +=
                new StateChangeEventHandler(OnStateChange);

            sqlServerDB.Command(@"SELECT * FROM Customers Order By Customers.ID");
            sqlServerDB.DataAdapter.SelectCommand = sqlServerDB.CommandLast;

            sqlServerDB.Command(@"INSERT INTO Customers ( FirstName,  LastName,  PhoneNumber,  Email) 
                                  VALUES                (@FirstName, @LastName, @PhoneNumber, @Email);");
            sqlServerDB.DataAdapter.InsertCommand = sqlServerDB.CommandLast;
            sqlServerDB.DataAdapter.InsertCommand.Parameters.Add(
                "@ID", SqlDbType.Int, 4, "ID").Direction = ParameterDirection.Output;
            sqlServerDB.DataAdapter.InsertCommand.Parameters.Add(
                "@FirstName", SqlDbType.NVarChar, 30, "FirstName");
            sqlServerDB.DataAdapter.InsertCommand.Parameters.Add(
                "@LastName", SqlDbType.NVarChar, 30, "LastName");
            sqlServerDB.DataAdapter.InsertCommand.Parameters.Add(
                "@PhoneNumber", SqlDbType.Int, 11, "PhoneNumber");
            sqlServerDB.DataAdapter.InsertCommand.Parameters.Add(
                "@Email", SqlDbType.NVarChar, 30, "Email");

            sqlServerDB.Command(@"UPDATE Customers 
                                  SET 
                                  FirstName = @FirstName,
                                  LastName = @LastName, 
                                  PhoneNumber = @PhoneNumber,
                                  Email = @Email
                                  WHERE ID = @ID");
            sqlServerDB.DataAdapter.UpdateCommand = sqlServerDB.CommandLast;
            sqlServerDB.DataAdapter.UpdateCommand.Parameters.Add(
                "@ID", SqlDbType.Int, 4, "ID").SourceVersion = DataRowVersion.Original;
            sqlServerDB.DataAdapter.UpdateCommand.Parameters.Add(
                "@FirstName", SqlDbType.NVarChar, 30, "FirstName");
            sqlServerDB.DataAdapter.UpdateCommand.Parameters.Add(
                "@LastName", SqlDbType.NVarChar, 30, "LastName");
            sqlServerDB.DataAdapter.UpdateCommand.Parameters.Add(
                "@PhoneNumber", SqlDbType.Int, 11, "PhoneNumber");
            sqlServerDB.DataAdapter.UpdateCommand.Parameters.Add(
                "@Email", SqlDbType.NVarChar, 30, "Email");

            sqlServerDB.Command(@"DELETE FROM Customers WHERE ID = @ID");
            sqlServerDB.DataAdapter.DeleteCommand = sqlServerDB.CommandLast;
            sqlServerDB.DataAdapter.DeleteCommand.Parameters.Add("@ID", SqlDbType.Int, 4, "ID");
        }

        private void ConnectToDB(object sender, RoutedEventArgs e)
        {
            ConnectToSQLServerDB(e);
            ConnectToAccessDB(e);
        }

        private void ConnectToSQLServerDB(RoutedEventArgs e)
        {
            string userID = textBoxUserId.Text;
            string password = textBoxPassword.Password;
            SqlConnectionStringBuilder strConSql = new SqlConnectionStringBuilder()
            {
                DataSource = "(local)",
                //DataSource = @"(localdb)\MSSQLLocalDB",
                //AttachDBFilename = @"|DataDirectory|\Data\Database1.mdf",
                InitialCatalog = "SQLServDB",
                //AttachDBFilename = @"C:\Users\Пк\source\repos\DataAccess\WPF_DataAccess\Data\SQLSeDB.mdf",
                IntegratedSecurity = false,
                UserID = userID, //"user1",
                Password = password, //"user1"
                //ConnectTimeout = 30,
                //Pooling = false
            };

            textBlockSQLConnString.Text = strConSql.ConnectionString;

            var sqlServerDB = new SQLServerDB(strConSql.ConnectionString);
            sqlServerDB.Connection.StateChange +=
                new StateChangeEventHandler(OnStateChange);
            //(s, ea) =>
            //    Dispatcher.BeginInvoke((Action)(() =>
            //        textBlockSQLConnState.Text = $" {(s as SqlConnection).State}"));

            Task.Run(async () => await OpenConnection(sqlServerDB));
        }

        private void ConnectToAccessDB(RoutedEventArgs e)
        {
            string connStrAccessDB =
            ConfigurationManager.ConnectionStrings["connStrAccessDB"].ConnectionString;
            textBlockAccessConnString.Text = connStrAccessDB;

            accessDB = new AccessDB(connStrAccessDB);
            accessDB.Connection.StateChange +=
                new StateChangeEventHandler(OnStateChange);

            Task.Run(async () => await OpenConnection(accessDB));
        }

        private void OnStateChange(object sender, StateChangeEventArgs e)
        {
            if (sender is SqlConnection)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                      textBlockSQLConnState.Text = $"{ e.CurrentState}"));
            }
            else if (sender is OleDbConnection)
            {
                Dispatcher.BeginInvoke((Action)(() =>
               textBlockAccessConnState.Text = $"{e.CurrentState}"));
            }
        }

        private Task OpenConnection(object sender)
        {
            return Task.Run(() =>
            {
                if (sender is SQLServerDB sender1) sender = sender1;
                else if (sender is AccessDB) sender = sender as AccessDB;

                try
                {
                    if (sender is SQLServerDB sender3)
                        sender3.Connection.Open();
                    else if (sender is AccessDB sender4)
                        sender4.Connection.Open();

                    MessageBox.Show($"DataBase {sender} successfully opened", "DataAccess",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message} {ex.StackTrace}", "ConnectionExceptions", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if ((sender is SQLServerDB sender3) && sender3.Connection.State == ConnectionState.Open)
                    {
                        sender3.Connection.Close();
                    }
                    else if ((sender is AccessDB sender4) && sender4.Connection.State == ConnectionState.Open)
                    {
                        sender4.Connection.Close();
                    }
                }
            });
        }

        private void buttonUsersFormFill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sqlServerDB.DataTable != null) sqlServerDB.DataTable.Clear();
                sqlServerDB.DataAdapter.Fill(sqlServerDB.DataTable);
                dataGridUsers.DataContext = sqlServerDB.DataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.StackTrace}", "DataFillError", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonUsersFormInsert_Click(object sender, RoutedEventArgs e)
        {
            DataRow row = sqlServerDB.DataTable.NewRow();

            row["FirstName"] = textBoxUserFirstName.Text;
            row["LastName"] = textBoxUserLastName.Text;
            int phoneNumber;
            Int32.TryParse(textBoxUserPhone.Text, out phoneNumber);
            row["PhoneNumber"] = phoneNumber;
            row["Email"] = textBoxUserEmail.Text;

            sqlServerDB.DataTable.Rows.Add(row);
            sqlServerDB.DataAdapter.Update(sqlServerDB.DataTable);
        }

        private void dataGridUsers_BeginningEdit(object sender, EventArgs e)
        {
            usersRow = dataGridUsers.SelectedItem as DataRowView;
            usersRow.BeginEdit();
            //da.Update(dt);
        }

        private void dataGridUsers_CurrentCellChanged(object sender, EventArgs e)
        {
            if (usersRow == null) return;
            usersRow.EndEdit();
            sqlServerDB.DataAdapter.Update(sqlServerDB.DataTable);
        }

        private void buttonUsersFormDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                usersRow = dataGridUsers.SelectedItem as DataRowView;
                usersRow?.Row?.Delete();
                sqlServerDB.DataAdapter.Update(sqlServerDB.DataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.StackTrace}", "DataDeleteError", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
