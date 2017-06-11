using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DataAccess
{
    internal class MySqlDataAccess : IDataAccess
    {
        #region Declaration(s)
        private string connectionString = string.Empty;
        private const int commandTimeout = 0;
        private MySqlConnection connection;
        private MySqlTransaction transaction;
        #endregion

        #region Properties
        private int BatchSize { get; set; } = 10000;
        #endregion

        #region Constructor
        public MySqlDataAccess() { }

        public MySqlDataAccess(string connectionString)
        {
            this.connectionString = connectionString;
        }
        #endregion

        #region Method(s)
        #region GetScalar
        public object GetScalar(string commandText)
        {
            return GetScalar(commandText, CommandType.Text, (DalParameterList)null);
        }

        public object GetScalar(string commandText, CommandType commandType)
        {
            return GetScalar(commandText, commandType, (DalParameterList)null);
        }

        public object GetScalar(string commandText, CommandType commandType, DalParameterList dalParameterList)
        {
            IsValidCommandText(commandText);
            MySqlCommand SqlCommand = null;
            int OutputParametersCount = 0; // count Output parameters sent to stored procedure

            try
            {

                // get new sql data adapter
                object ReturnValue = null;
                SqlCommand = GetSqlCommand(commandText);

                using (SqlCommand)
                {
                    // prepare procedure
                    SqlCommand.CommandType = commandType;
                    SqlCommand.CommandTimeout = commandTimeout;

                    // add sql parameters to procedure
                    if (dalParameterList != null)
                    {
                        foreach (DalParameter DalParam in dalParameterList)
                        {

                            MySqlParameter SqlParam = new MySqlParameter()
                            {
                                ParameterName = DalParam.ParameterName,
                                DbType = DalParam.ParameterType,
                                Size = DalParam.ParameterSize,
                                Direction = DalParam.ParameterDirection,
                                Value = DalParam.ParameterValue
                            };
                            if (DalParam.ParameterDirection == ParameterDirection.Output)
                            {
                                OutputParametersCount++;
                                if (DalParam.ParameterSize > 0)
                                {
                                    SqlParam.Size = DalParam.ParameterSize;
                                }
                            }
                            SqlCommand.Parameters.Add(SqlParam);
                        }
                    }

                    // retrieve data into datasets from stored procedure
                    SqlCommand.Connection.Open();
                    ReturnValue = SqlCommand.ExecuteScalar();


                    // handle output parameters
                    if (OutputParametersCount > 0)
                    {
                        foreach (MySqlParameter SqlParam in SqlCommand.Parameters)
                        {
                            if (SqlParam.Direction == ParameterDirection.Output)
                            {
                                dalParameterList.Find((x) => x.ParameterName == SqlParam.ParameterName
                                                    && x.ParameterDirection == ParameterDirection.Output)
                                                    .ParameterValue = SqlParam.Value;
                            }
                        }
                    }//if(iOutputParametersCount > 0)

                    return ReturnValue;
                }
            }
            catch (MySqlException SqlEx)
            {
                throw SqlEx.GetBaseException();
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
            finally
            {
                if (SqlCommand.Connection.State != ConnectionState.Closed)
                    SqlCommand.Connection.Close();

                if (SqlCommand != null)
                    SqlCommand.Dispose();

                if (connection != null)
                    connection.Dispose();

            }
        }
        #endregion

        #region ExecuteNonQuery
        public int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(commandText, CommandType.Text, (DalParameterList)null);
        }

        public int ExecuteNonQuery(string commandText, CommandType commandType)
        {
            return ExecuteNonQuery(commandText, commandType, (DalParameterList)null);
        }

        public int ExecuteNonQuery(string commandText, CommandType commandType, DalParameterList dalParameterList)
        {
            IsValidCommandText(commandText);
            int RecordsAffected = -1;
            MySqlCommand SqlCommand = null;

            int OutputParametersCount = 0; // count Output parameters sent to stored procedure
            bool HasTransactionBegan = false;

            try
            {
                // get new sql command
                SqlCommand = GetSqlCommand(commandText);

                using (SqlCommand)
                {
                    // prepare procedure
                    SqlCommand.CommandType = commandType;
                    SqlCommand.CommandTimeout = commandTimeout;

                    // add sql parameters to procedure
                    if (dalParameterList != null)
                    {
                        foreach (DalParameter DalParam in dalParameterList)
                        {


                            MySqlParameter SqlParam = new MySqlParameter()
                            {
                                ParameterName = DalParam.ParameterName,
                                DbType = DalParam.ParameterType,
                                Direction = DalParam.ParameterDirection,
                                Value = DalParam.ParameterValue
                            };


                            if (DalParam.ParameterDirection == ParameterDirection.Output)
                            {
                                OutputParametersCount++;
                                if (DalParam.ParameterSize > 0)
                                {
                                    SqlParam.Size = DalParam.ParameterSize;
                                }
                            }

                            SqlCommand.Parameters.Add(SqlParam);
                        }
                    }

                    // take care of transaction business

                    SqlCommand.Connection.Open();
                    if (commandType != CommandType.StoredProcedure)
                    {
                        SqlCommand.Transaction = SqlCommand.Connection.BeginTransaction();
                        HasTransactionBegan = true;
                    }

                    // executes procedure to insert/update/delete data
                    RecordsAffected = SqlCommand.ExecuteNonQuery();

                    if (commandType != CommandType.StoredProcedure)
                        SqlCommand.Transaction.Commit();

                    // handle output parameters
                    if (OutputParametersCount > 0)
                    {
                        foreach (MySqlParameter SqlParam in SqlCommand.Parameters)
                        {
                            if (SqlParam.Direction == ParameterDirection.Output)
                            {
                                dalParameterList.Find((x) => x.ParameterName == SqlParam.ParameterName
                                                    && x.ParameterDirection == ParameterDirection.Output)
                                                    .ParameterValue = SqlParam.Value;
                            }
                        }
                    }
                }//using SureScoreCommand
            }
            catch (MySqlException SqlEx) { throw SqlEx.GetBaseException(); }
            catch (Exception ex) { throw ex.GetBaseException(); }
            finally
            {
                if (commandType != CommandType.StoredProcedure)
                {
                    if (SqlCommand != null && SqlCommand.Transaction != null && HasTransactionBegan)
                        SqlCommand.Transaction.Rollback();
                }

                if (SqlCommand.Connection.State != ConnectionState.Closed)
                    SqlCommand.Connection.Close();

                if (SqlCommand != null)
                    SqlCommand.Dispose();

                if (connection != null)
                    connection.Dispose();
            }

            return RecordsAffected;
        }
        #endregion

        #region GetDataReader
        public DbDataReader GetDataReader(string commandText)
        {
            return GetDataReader(commandText, CommandType.Text, (DalParameterList)null);
        }

        public DbDataReader GetDataReader(string commandText, CommandType commandType)
        {
            return GetDataReader(commandText, commandType, (DalParameterList)null);
        }


        public DbDataReader GetDataReader(string commandText, CommandType commandType, DalParameterList dalParameterList)
        {
            IsValidCommandText(commandText);
            DbDataReader dbDataReader = null;

            MySqlCommand sqlCommand = null;

            int OutputParametersCount = 0; // count Output parameters sent to stored procedure

            try
            {
                // get new sql command
                sqlCommand = GetSqlCommand(commandText);

                using (sqlCommand)
                {
                    // prepare procedure
                    sqlCommand.CommandType = commandType;
                    sqlCommand.CommandTimeout = commandTimeout;

                    // add sql parameters to procedure
                    if (dalParameterList != null)
                    {
                        foreach (DalParameter DalParam in dalParameterList)
                        {

                            MySqlParameter SqlParam = new MySqlParameter()
                            {
                                ParameterName = DalParam.ParameterName,
                                DbType = DalParam.ParameterType,
                                Direction = DalParam.ParameterDirection,
                                Value = DalParam.ParameterValue
                            };

                            if (DalParam.ParameterDirection == ParameterDirection.Output)
                            {
                                OutputParametersCount++;
                                if (DalParam.ParameterSize > 0)
                                {
                                    SqlParam.Size = DalParam.ParameterSize;
                                }
                            }

                            sqlCommand.Parameters.Add(SqlParam);
                        }
                    }

                    // take care of transaction business

                    sqlCommand.Connection.Open();


                    // executes procedure to insert/update/delete data
                    dbDataReader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);

                    // handle output parameters
                    if (OutputParametersCount > 0)
                    {
                        foreach (MySqlParameter SqlParam in sqlCommand.Parameters)
                        {
                            if (SqlParam.Direction == ParameterDirection.Output)
                            {
                                dalParameterList.Find((x) => x.ParameterName == SqlParam.ParameterName
                                                    && x.ParameterDirection == ParameterDirection.Output)
                                                    .ParameterValue = SqlParam.Value;
                            }
                        }
                    }
                }//using SureScoreCommand
            }
            catch (MySqlException SqlEx) { throw SqlEx.GetBaseException(); }
            catch (Exception ex) { throw ex.GetBaseException(); }
            finally
            {

                //if (_sqlCommand.Connection.State != ConnectionState.Closed)
                //    _sqlCommand.Connection.Close();

                //if (SureScoreCommand != null)
                // SureScoreCommand.Dispose();

                //if (SureScoreConnection != null)
                //  SureScoreConnection.Dispose();
            }

            return dbDataReader;
        }
        #endregion

        #region GetDataTable
        public DataTable GetDataTable(string commandText, string TableName = null)
        {
            return GetDataTable(commandText, CommandType.Text, (DalParameterList)null, TableName);
        }

        public DataTable GetDataTable(string commandText, CommandType commandType, string TableName = null)
        {
            return GetDataTable(commandText, commandType, (DalParameterList)null, TableName);
        }

        public DataTable GetDataTable(string commandText, CommandType commandType, DalParameterList dalParameterList, string TableName = null)
        {
            IsValidCommandText(commandText);
            DataSet tempDataSet = new DataSet();
            MySqlDataAdapter sqlDataAdapter = null;
            int OutputParametersCount = 0; // count Output parameters sent to stored procedure
            try
            {

                // get new sql data adapter
                sqlDataAdapter = GetSqlDataAdapter(commandText);

                using (sqlDataAdapter)
                {
                    // prepare procedure
                    sqlDataAdapter.SelectCommand.CommandType = commandType;
                    sqlDataAdapter.SelectCommand.CommandTimeout = commandTimeout;


                    // add sql parameters to procedure
                    if (dalParameterList != null)
                    {
                        foreach (DalParameter DalParam in dalParameterList)
                        {

                            MySqlParameter SqlParam = new MySqlParameter()
                            {
                                ParameterName = DalParam.ParameterName,
                                DbType = DalParam.ParameterType,
                                Size = DalParam.ParameterSize,
                                Direction = DalParam.ParameterDirection,
                                Value = DalParam.ParameterValue
                            };

                            if (DalParam.ParameterDirection == ParameterDirection.Output)
                            {
                                OutputParametersCount++;
                                if (DalParam.ParameterSize > 0)
                                {
                                    SqlParam.Size = DalParam.ParameterSize;
                                }
                            }

                            sqlDataAdapter.SelectCommand.Parameters.Add(SqlParam);
                        }
                    }

                    // retrieve data into datasets from stored procedure
                    sqlDataAdapter.Fill(tempDataSet);


                    // handle output parameters
                    if (OutputParametersCount > 0)
                    {
                        foreach (MySqlParameter sqlParam in sqlDataAdapter.SelectCommand.Parameters)
                        {
                            if (sqlParam.Direction == ParameterDirection.Output)
                            {
                                dalParameterList.Find((x) => x.ParameterName == sqlParam.ParameterName
                                                    && x.ParameterDirection == ParameterDirection.Output)
                                                    .ParameterValue = sqlParam.Value;
                            }
                        }
                    }//if(iOutputParametersCount > 0)

                }

                DataTable ReturnValue = null;

                if (tempDataSet != null && tempDataSet.Tables.Count > 0)
                {
                    ReturnValue = tempDataSet.Tables[0];
                    if (TableName != null && TableName.Trim() != "")
                    {
                        ReturnValue.TableName = TableName;
                    }
                }

                return ReturnValue;
            }
            catch (MySqlException SqlEx)
            {
                throw SqlEx.GetBaseException();
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
            finally
            {
                if (sqlDataAdapter != null)
                    sqlDataAdapter.Dispose();

                if (connection != null)
                    connection.Dispose();

            }
        }
        #endregion

        #region GetDataSet
        public DataSet GetDataSet(string commandText, string[] TableNames = null)
        {
            return GetDataSet(commandText, CommandType.Text, (DalParameterList)null, TableNames);
        }

        public DataSet GetDataSet(string commandText, CommandType commandType, string[] TableNames = null)
        {
            return GetDataSet(commandText, commandType, (DalParameterList)null, TableNames);
        }

        public DataSet GetDataSet(string commandText, CommandType commandType, DalParameterList dalParameterList, string[] TableNames = null)
        {
            IsValidCommandText(commandText);
            DataSet tempDataSet = new DataSet();
            MySqlDataAdapter sqlDataAdapter = null;
            int OutputParametersCount = 0; // count Output parameters sent to stored procedure
            try
            {

                // get new sql data adapter
                sqlDataAdapter = GetSqlDataAdapter(commandText);

                using (sqlDataAdapter)
                {
                    // prepare procedure
                    sqlDataAdapter.SelectCommand.CommandType = commandType;
                    sqlDataAdapter.SelectCommand.CommandTimeout = commandTimeout;

                    // add sql parameters to procedure
                    if (dalParameterList != null)
                    {
                        foreach (DalParameter DalParam in dalParameterList)
                        {

                            MySqlParameter sqlParam = new MySqlParameter()
                            {
                                ParameterName = DalParam.ParameterName,
                                DbType = DalParam.ParameterType,
                                Size = DalParam.ParameterSize,
                                Direction = DalParam.ParameterDirection,
                                Value = DalParam.ParameterValue
                            };

                            if (DalParam.ParameterDirection == ParameterDirection.Output)
                            {
                                OutputParametersCount++;
                                if (DalParam.ParameterSize > 0)
                                {
                                    sqlParam.Size = DalParam.ParameterSize;
                                }
                            }
                            sqlDataAdapter.SelectCommand.Parameters.Add(sqlParam);
                        }
                    }

                    // retrieve data into datasets from stored procedure
                    sqlDataAdapter.Fill(tempDataSet);


                    // handle output parameters
                    if (OutputParametersCount > 0)
                    {
                        foreach (MySqlParameter sqlParam in sqlDataAdapter.SelectCommand.Parameters)
                        {
                            if (sqlParam.Direction == ParameterDirection.Output)
                            {
                                dalParameterList.Find((x) => x.ParameterName == sqlParam.ParameterName
                                                    && x.ParameterDirection == ParameterDirection.Output)
                                                    .ParameterValue = sqlParam.Value;
                            }
                        }
                    }//if(iOutputParametersCount > 0)

                }



                if (tempDataSet != null && tempDataSet.Tables.Count > 0)
                {
                    int counter = 0;
                    int tableNamesCount = TableNames != null ? TableNames.Length : 0;
                    foreach (DataTable table in tempDataSet.Tables)
                    {
                        if (counter < tableNamesCount)
                        {
                            table.TableName = TableNames[counter].Trim() == "" ? table.TableName : TableNames[counter];
                            counter++;
                        }
                    }

                }

                return tempDataSet;
            }
            catch (MySqlException SqlEx)
            {
                throw SqlEx.GetBaseException();
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
            finally
            {
                if (sqlDataAdapter != null)
                    sqlDataAdapter.Dispose();

                if (connection != null)
                    connection.Dispose();

            }
        }
        #endregion

        #region BulkCopy
        public bool SqlBulkCopy(DataTable dataTable, string targetTable)
        {
            return false;
        }
        #endregion 

        #endregion

        #region Function(s)
        private MySqlDataAdapter GetSqlDataAdapter(string commandText = "")
        {
            if (!IsConnectionStringValid())
                throw new Exception("Connection string not set correctly!");

            if (this.connection == null)
                this.connection = new MySqlConnection(this.connectionString);
            else
            {
                this.connection.Dispose();
                this.connection = null;
                this.connection = new MySqlConnection(this.connectionString);
            }

            return new MySqlDataAdapter(commandText, this.connection);
        }

        private MySqlCommand GetSqlCommand(string commandText = "")
        {
            if (!IsConnectionStringValid())
                throw new Exception("Connection string not set correctly!");

            if (this.connection == null)
                this.connection = new MySqlConnection(this.connectionString);
            else
            {
                this.connection.Dispose();
                this.connection = null;
                this.connection = new MySqlConnection(this.connectionString);
            }

            return new MySqlCommand(commandText, this.connection);
        }

        private void IsValidCommandText(string commandText)
        {
            if (commandText == null || commandText.Trim() == "")
            {
                throw new Exception("Invalid Command Text");
            }
        }

        private bool IsConnectionStringValid()
        {
            return !String.IsNullOrEmpty(this.connectionString);
        }

        #region Connection and Transaction
        private void OpenConnection()
        {
            try
            {
                if (connection != null)
                {
                    if (connection.State != ConnectionState.Connecting &&
                        connection.State != ConnectionState.Executing &&
                        connection.State != ConnectionState.Fetching &&
                        connection.State != ConnectionState.Open)
                    {
                        connection = new MySqlConnection(connectionString);
                        connection.Open();
                    }
                }
                else
                {
                    connection = new MySqlConnection(connectionString);
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                var message = "An exception of type " + ex.GetType() + " was encountered while opening the connection. " + "Exception: " + ex.ToString();
                throw new Exception(message);
            }
        }

        private void CloseConnection()
        {
            try
            {
                if (connection == null)
                    return;
                if (connection.State == ConnectionState.Closed)
                    return;
                connection.Close();
            }
            catch (Exception ex)
            {
                var message = "An exception of type " + ex.GetType() + " was encountered while opening the connection. " + "Exception: " + ex.ToString();
                throw new Exception(message);
            }
        }

        private void BeginTransaction()
        {
            try
            {
                transaction = connection.BeginTransaction();
            }
            catch (Exception ex)
            {
                var message = "An exception of type " + ex.GetType() + " was encountered  while attempting to begin the transaction. " + "Exception: " + ex.ToString();
                throw new Exception(message);
            }
        }

        private void CommitTransaction()
        {
            try { transaction.Commit(); }
            catch (Exception ex)
            {
                var message = "An exception of type " + ex.GetType() + " was encountered  while attempting to commit the transaction. " + "Exception: " + ex.ToString();
                throw new Exception(message);
            }
        }

        private void RollbackTransaction()
        {
            try { transaction.Rollback(); }
            catch (Exception ex)
            {
                var message = "An exception of type " + ex.GetType() + " was encountered  while attempting to roll back the transaction. " + "Exception: " + ex.ToString();
                throw new Exception(message);
            }
        }

        public void SetBatchSizeForBulkInsert(int size)
        {
            BatchSize = size;
        }
        #endregion

        #endregion
    }
}
