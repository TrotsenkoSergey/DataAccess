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
        private AccessDB accessDB;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectToDB(object sender, RoutedEventArgs e)
        {
            string userID = textBoxUserId.Text;
            string password = textBoxPassword.Password;
            SqlConnectionStringBuilder strConSql = new SqlConnectionStringBuilder()
            {
                DataSource = "(local)",
                //DataSource = @"(localdb)\MSSQLLocalDB",
                //AttachDBFilename = @"|DataDirectory|\Data\Database1.mdf",
                InitialCatalog = "SQLServDB",
                //AttachDBFilename = @"D:\SQL2019\TestDB.mdf",
                IntegratedSecurity = false,
                UserID = userID, //"user1",
                Password = password, //"user1"
                //ConnectTimeout = 30,
                //Pooling = false
            };

            string connStrAccessDB =
            ConfigurationManager.ConnectionStrings["connStrAccessDB"].ConnectionString;

            textBlockSQLConnString.Text = strConSql.ConnectionString;
            textBlockAccessConnString.Text = connStrAccessDB;

            sqlServerDB = new SQLServerDB(strConSql.ConnectionString);
            sqlServerDB.Connection.StateChange += 
                new StateChangeEventHandler(OnStateChange);
            //(s, ea) =>
            //    Dispatcher.BeginInvoke((Action)(() =>
            //        textBlockSQLConnState.Text = $" {(s as SqlConnection).State}"));

            accessDB = new AccessDB(connStrAccessDB);
            accessDB.Connection.StateChange +=
                new StateChangeEventHandler(OnStateChange);

            Task.Run(async () => await OpenConnection(sqlServerDB));
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
    }
}
