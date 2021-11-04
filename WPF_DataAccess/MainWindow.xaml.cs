using DataControllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;

namespace WPF_DataAccess
{

    public partial class MainWindow : Window
    {
        private SQLServerDB sqlServerDB;
        private DataRowView usersRow;

        private AccessDB accessDB;
        private DataRowView productsRow;
        private int index;

        private DataSet dataSet;

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

            PreparingSQLServerDB(userID, password);
            PreparingAccessDB();
        }

        private void PreparingSQLServerDB(string userID, string password)
        {
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

            sqlServerDB.DataAdapter.TableMappings.Add("Table", "Customers");
        }

        private void PreparingAccessDB()
        {
            accessDB = new AccessDB();

            textBlockAccessConnString.Text = accessDB.ConnectionString;

            accessDB.Connection.StateChange +=
                new StateChangeEventHandler(OnStateChange);

            accessDB.Connection.Open();
            using (var reader = accessDB.Command(@"SELECT Max(ID) FROM Products").ExecuteReader())
            {
                reader.Read();
                index = (int)reader[0];
            }
            accessDB.Connection.Close();

            accessDB.Command(@"SELECT * FROM Products ORDER BY Products.ID");
            accessDB.DataAdapter.SelectCommand = accessDB.CommandLast;

            //accessDB.DataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

            accessDB.Command(@"INSERT INTO Products (EmailCustomer, ProductName, ProductCode)
                                      VALUES        (?, ?, ?)");
            accessDB.DataAdapter.InsertCommand = accessDB.CommandLast;
            //accessDB.DataAdapter.InsertCommand.Parameters.Add(
            //    "@ID", OleDbType.Integer, 4, "ID");
            accessDB.DataAdapter.InsertCommand.Parameters.Add(
                "@EmailCustomer", OleDbType.VarChar, 30, "EmailCustomer");
            accessDB.DataAdapter.InsertCommand.Parameters.Add(
                "@ProductName", OleDbType.VarChar, 30, "ProductName");
            accessDB.DataAdapter.InsertCommand.Parameters.Add(
                "@ProductCode", OleDbType.VarChar, 30, "ProductCode");

            var updateCommand = accessDB.Command(@"UPDATE Products 
                                  SET 
                                  EmailCustomer = ?,
                                  ProductName = ?, 
                                  ProductCode = ?
                                  WHERE ID = ? ");
            updateCommand.Parameters.Add("@EmailCustomer", OleDbType.VarChar,
                30, "EmailCustomer");
            updateCommand.Parameters.Add("@ProductName", OleDbType.VarChar,
                30, "ProductName");
            updateCommand.Parameters.Add("@ProductCode", OleDbType.VarChar,
                30, "ProductCode");
            updateCommand.Parameters.Add("@ID", OleDbType.Integer,
                4, "ID").SourceVersion = DataRowVersion.Original;
            accessDB.DataAdapter.UpdateCommand = updateCommand;

            accessDB.Command(@"DELETE FROM Products WHERE ID = ?");
            accessDB.DataAdapter.DeleteCommand = accessDB.CommandLast;
            accessDB.DataAdapter.DeleteCommand.Parameters.Add("@ID", OleDbType.Integer, 4, "ID").
                SourceVersion = DataRowVersion.Original;

            accessDB.DataAdapter.TableMappings.Add("Table", "Products");
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
                DataSource = "SERGDEV",
                //DataSource = @"(localdb)\MSSQLLocalDB",
                //AttachDBFilename = @"|DataDirectory|\Data\Database1.mdf",
                InitialCatalog = "SQLServDB",
                AttachDBFilename = @"D:\REPOS\DataAccess\WPF_DataAccess\Data\SQLServDB.mdf",
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

            var accessDB = new AccessDB(connStrAccessDB);
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
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.StackTrace}", "DataInsertError",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dataGridUsers_BeginningEdit(object sender, EventArgs e)
        {
            usersRow = dataGridUsers.SelectedItem as DataRowView;
            usersRow.BeginEdit();
        }

        private void dataGridUsers_CurrentCellChanged(object sender, EventArgs e)
        {
            if (usersRow == null) return;
            usersRow.EndEdit();
            try
            {
                sqlServerDB.DataAdapter.Update(sqlServerDB.DataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.StackTrace}", "DataUpdateError",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void buttonProductsFormFill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (accessDB.DataTable != null) accessDB.DataTable.Clear();
                accessDB.DataAdapter.Fill(accessDB.DataTable);
                dataGridProducts.DataContext = accessDB.DataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.StackTrace}", "DataFillError",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonProductsFormInsert_Click(object sender, RoutedEventArgs e)
        {
            #region MainVariant

            //DataRow row = accessDB.DataTable.NewRow();

            //row["EmailCustomer"] = textBoxCustomerEmail.Text;
            //row["ProductName"] = textBoxProductName.Text;
            //row["ProductCode"] = textBoxProductCode.Text;

            //try
            //{
            //    accessDB.DataTable.Rows.Add(row);
            //    accessDB.DataAdapter.Update(accessDB.DataTable);
            //    accessDB.DataTable.Clear();
            //    accessDB.DataAdapter.Fill(accessDB.DataTable);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"{ex.Message} {ex.StackTrace}", "DataInsertError",
            //        MessageBoxButton.OK, MessageBoxImage.Error);
            //}

            #endregion

            #region SecondVariant

            accessDB.Connection.Open();
            var insert = accessDB.Command(@"INSERT INTO Products     ( ID, EmailCustomer,  ProductName,  ProductCode) 
                                      VALUES            (?, ?, ?, ?)");
            insert.Parameters.AddWithValue("@ID", ++index);
            insert.Parameters.AddWithValue("@EmailCustomer", textBoxCustomerEmail.Text);
            insert.Parameters.AddWithValue("@ProductName", textBoxProductName.Text);
            insert.Parameters.AddWithValue("@ProductCode", textBoxProductCode.Text);
            var num = insert.ExecuteNonQuery();
            accessDB.Connection.Close();

            #endregion

            #region ThirdVariant

            //accessDB.Connection.Open();
            //var num = accessDB.Command(@"INSERT INTO Products ( EmailCustomer,  ProductName,  ProductCode) 
            //                             VALUES               ('Fedor', 'bbb', 'ccc')").ExecuteNonQuery();
            //accessDB.Connection.Close();

            #endregion
        }

        private void buttonProductsRowDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                productsRow = dataGridProducts.SelectedItem as DataRowView;
                productsRow?.Row?.Delete();
                accessDB.DataAdapter.Update(accessDB.DataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.StackTrace}", "DataDeleteError",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dataGridProducts_BeginningEdit(object sender, EventArgs e)
        {
            productsRow = dataGridProducts.SelectedItem as DataRowView;
            productsRow.BeginEdit();
        }

        private void dataGridProducts_CurrentCellChanged(object sender, EventArgs e)
        {
            if (productsRow == null) return;
            try
            {
                productsRow.EndEdit();
                accessDB.DataAdapter.Update(accessDB.DataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {ex.StackTrace}", "DataUpdateError",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonRelation_Click(object sender, EventArgs e)
        {
            dataSet = new DataSet();
            sqlServerDB.DataAdapter.Fill(dataSet);
            accessDB.DataAdapter.Fill(dataSet);

            //dataSet.Tables["Customers"].PrimaryKey = 
            //    new DataColumn[] { dataSet.Tables["Customers"].Columns["Email"] };
            //dataSet.Tables["Products"].PrimaryKey =
            //    new DataColumn[] { dataSet.Tables["Products"].Columns["EmailCustomer"] };

            DataColumn parentColumn = dataSet.Tables["Customers"].Columns["Email"];
            DataColumn childColumn = dataSet.Tables["Products"].Columns["EmailCustomer"];
            var relation = new DataRelation("ProductsCustomers", parentColumn, childColumn);
            dataSet.Relations.Add(relation);

            dataGridUsers.ItemsSource = dataSet.Tables["Customers"].DefaultView;
            dataGridProducts.ItemsSource = dataSet.Tables["Products"].DefaultView;

            var relationWindow = new RelationWindow();
            relationWindow.dataGrid.ItemsSource = dataSet.Tables["Customers"].DefaultView;
            relationWindow.Owner = this;
            relationWindow.Show();
        }
    }
}
