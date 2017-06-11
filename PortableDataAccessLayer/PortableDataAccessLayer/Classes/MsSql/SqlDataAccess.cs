using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Collections.Generic;
using DataAccess.Entity;

namespace DataAccess
{

    internal class SqlDataAccess : IDataAccess
    {
        #region Declaration(s)
        private string connectionString = string.Empty;
        private const int commandTimeout = 0;
        private SqlConnection connection;
        private SqlTransaction transaction;
        #endregion

        #region Properties
        private int BatchSize { get; set; } = 10000;
        #endregion

        #region Constructor
        public SqlDataAccess() { }

        public SqlDataAccess(string connectionString)
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
            SqlCommand SqlCommand = null;
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

                            SqlParameter SqlParam = new SqlParameter()
                            {
                                ParameterName = DalParam.ParameterName,
                                SqlDbType = DalParam.ParameterType,
                                Size = DalParam.ParameterSize,
                                Direction = DalParam.ParameterDirection,
                                SqlValue = DalParam.ParameterValue
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
                        foreach (SqlParameter SqlParam in SqlCommand.Parameters)
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
            catch (SqlException SqlEx)
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
            SqlCommand SqlCommand = null;

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


                            SqlParameter SqlParam = new SqlParameter()
                            {
                                ParameterName = DalParam.ParameterName,
                                SqlDbType = DalParam.ParameterType,
                                Direction = DalParam.ParameterDirection,
                                SqlValue = DalParam.ParameterValue
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
                        foreach (SqlParameter SqlParam in SqlCommand.Parameters)
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
            catch (SqlException SqlEx) { throw SqlEx.GetBaseException(); }
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

        #region Execute data set using data adapter
        public int ExecuteDataSetUsingAdapter(List<EntityMetadata> entities, DataSet dataSet)
        {
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                var count = 0;
                try
                {
                    OpenConnection();

                    foreach (var entity in entities)
                    {
                        var adapter = new SqlDataAdapter(entity.Query, connection);
                        entity.Adapter = adapter;
                        if (adapter != null)
                        {
                            var builder = new SqlCommandBuilder(adapter);

                            // add rows to dataset
                            adapter.InsertCommand = builder.GetInsertCommand();
                            // update rows to dataset
                            adapter.UpdateCommand = builder.GetUpdateCommand();
                        }
                    }

                    BeginTransaction();
                    foreach (var entity in entities)
                    {
                        var adapter = entity.Adapter;
                        adapter.InsertCommand.Transaction = transaction;
                        adapter.UpdateCommand.Transaction = transaction;
                        count += adapter.Update(dataSet, entity.Name);
                    }
                    CommitTransaction();
                    return count;
                }
                catch (InvalidOperationException ex)
                {
                    RollbackTransaction();
                    var message = "An exception of type " + ex.GetType() + " was encountered due to invalid data operation. " + "Exception: " + ex.ToString();
                    throw new Exception(message);
                }
                catch (DBConcurrencyException ex)
                {
                    RollbackTransaction();
                    var message = "An exception of type " + ex.GetType() + " was encountered due to concurrency. " + "Exception: " + ex.ToString();
                    throw new Exception(message);
                }
                catch (Exception ex)
                {
                    RollbackTransaction();
                    throw ex;
                }
            }
            else
            {
                var message = "Request rejected due to null argument.";
                throw new ArgumentNullException(message);
            }
        }

        public DataTable GetDataSetUsingAdapter(EntityMetadata entityMetadata)
        {
            try
            {
                var dataSet = new DataSet();
                OpenConnection();

                entityMetadata.Adapter = new SqlDataAdapter(entityMetadata.Query, connection);

                entityMetadata.Adapter.Fill(dataSet, entityMetadata.Name);

                CloseConnection();
                return dataSet.Tables[entityMetadata.Name];
            }
            catch (InvalidOperationException ex)
            {
                RollbackTransaction();
                var message = "An exception of type " + ex.GetType() + " was encountered due to invalid data operation. " + "Exception: " + ex.ToString();
                throw new Exception(message);
            }
            catch (DBConcurrencyException ex)
            {
                RollbackTransaction();
                var message = "An exception of type " + ex.GetType() + " was encountered due to concurrency. " + "Exception: " + ex.ToString();
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                throw ex;
            }
        }

        public DataSet GetDataSetUsingAdapter(List<EntityMetadata> entitiesMetadata)
        {
            try
            {
                var dataSet = new DataSet();

                OpenConnection();

                foreach (var entity in entitiesMetadata)
                {
                    entity.Adapter = new SqlDataAdapter(entity.Query, connection);

                    entity.Adapter.Fill(dataSet, entity.Name);
                    if (!entity.IsMasterEntity)
                    {
                        foreach (var dependency in entity.Dependencies)
                        {
                            var childColumn = dataSet.Tables[entity.Name].Columns[dependency.ChildTableIdentityColumn];
                            var parentColumn = dataSet.Tables[dependency.Name].Columns[dependency.ParentTableIdentityColumn];
                            var pattern = "{0}_{1}";
                            var relation = string.Format(pattern, entity.Name, dependency.Name);
                            dataSet.Relations.Add(relation, parentColumn, childColumn);
                        }
                    }
                }
                CloseConnection();
                return dataSet;
            }
            catch (InvalidOperationException ex)
            {
                var message = "An exception of type " + ex.GetType() + " was encountered due to invalid data operation. " + "Exception: " + ex.ToString();
                throw new Exception(message);
            }
            catch (DBConcurrencyException ex)
            {
                var message = "An exception of type " + ex.GetType() + " was encountered due to concurrency. " + "Exception: " + ex.ToString();
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

            SqlCommand SqlCommand = null;

            int OutputParametersCount = 0; // count Output parameters sent to stored procedure

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

                            SqlParameter SqlParam = new SqlParameter()
                            {
                                ParameterName = DalParam.ParameterName,
                                SqlDbType = DalParam.ParameterType,
                                Direction = DalParam.ParameterDirection,
                                SqlValue = DalParam.ParameterValue
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


                    // executes procedure to insert/update/delete data
                    dbDataReader = SqlCommand.ExecuteReader(CommandBehavior.CloseConnection);

                    // handle output parameters
                    if (OutputParametersCount > 0)
                    {
                        foreach (SqlParameter SqlParam in SqlCommand.Parameters)
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
            catch (SqlException SqlEx) { throw SqlEx.GetBaseException(); }
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
            DataSet TempDataSet = new DataSet();
            SqlDataAdapter SqlDataAdapter = null;
            int OutputParametersCount = 0; // count Output parameters sent to stored procedure
            try
            {

                // get new sql data adapter
                SqlDataAdapter = GetSqlDataAdapter(commandText);

                using (SqlDataAdapter)
                {
                    // prepare procedure
                    SqlDataAdapter.SelectCommand.CommandType = commandType;
                    SqlDataAdapter.SelectCommand.CommandTimeout = commandTimeout;


                    // add sql parameters to procedure
                    if (dalParameterList != null)
                    {
                        foreach (DalParameter DalParam in dalParameterList)
                        {

                            SqlParameter SqlParam = new SqlParameter()
                            {
                                ParameterName = DalParam.ParameterName,
                                SqlDbType = DalParam.ParameterType,
                                Size = DalParam.ParameterSize,
                                Direction = DalParam.ParameterDirection,
                                SqlValue = DalParam.ParameterValue
                            };

                            if (DalParam.ParameterDirection == ParameterDirection.Output)
                            {
                                OutputParametersCount++;
                                if (DalParam.ParameterSize > 0)
                                {
                                    SqlParam.Size = DalParam.ParameterSize;
                                }
                            }

                            SqlDataAdapter.SelectCommand.Parameters.Add(SqlParam);
                        }
                    }

                    // retrieve data into datasets from stored procedure
                    SqlDataAdapter.Fill(TempDataSet);


                    // handle output parameters
                    if (OutputParametersCount > 0)
                    {
                        foreach (SqlParameter SqlParam in SqlDataAdapter.SelectCommand.Parameters)
                        {
                            if (SqlParam.Direction == ParameterDirection.Output)
                            {
                                dalParameterList.Find((x) => x.ParameterName == SqlParam.ParameterName
                                                    && x.ParameterDirection == ParameterDirection.Output)
                                                    .ParameterValue = SqlParam.Value;
                            }
                        }
                    }//if(iOutputParametersCount > 0)

                }

                DataTable ReturnValue = null;

                if (TempDataSet != null && TempDataSet.Tables.Count > 0)
                {
                    ReturnValue = TempDataSet.Tables[0];
                    if (TableName != null && TableName.Trim() != "")
                    {
                        ReturnValue.TableName = TableName;
                    }
                }

                return ReturnValue;
            }
            catch (SqlException SqlEx)
            {
                throw SqlEx.GetBaseException();
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
            finally
            {
                if (SqlDataAdapter != null)
                    SqlDataAdapter.Dispose();

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
            DataSet TempDataSet = new DataSet();
            SqlDataAdapter SqlDataAdapter = null;
            int OutputParametersCount = 0; // count Output parameters sent to stored procedure
            try
            {

                // get new sql data adapter
                SqlDataAdapter = GetSqlDataAdapter(commandText);

                using (SqlDataAdapter)
                {
                    // prepare procedure
                    SqlDataAdapter.SelectCommand.CommandType = commandType;
                    SqlDataAdapter.SelectCommand.CommandTimeout = commandTimeout;

                    // add sql parameters to procedure
                    if (dalParameterList != null)
                    {
                        foreach (DalParameter DalParam in dalParameterList)
                        {

                            SqlParameter SqlParam = new SqlParameter()
                            {
                                ParameterName = DalParam.ParameterName,
                                SqlDbType = DalParam.ParameterType,
                                Size = DalParam.ParameterSize,
                                Direction = DalParam.ParameterDirection,
                                SqlValue = DalParam.ParameterValue
                            };

                            if (DalParam.ParameterDirection == ParameterDirection.Output)
                            {
                                OutputParametersCount++;
                                if (DalParam.ParameterSize > 0)
                                {
                                    SqlParam.Size = DalParam.ParameterSize;
                                }
                            }
                            SqlDataAdapter.SelectCommand.Parameters.Add(SqlParam);
                        }
                    }

                    // retrieve data into datasets from stored procedure
                    SqlDataAdapter.Fill(TempDataSet);


                    // handle output parameters
                    if (OutputParametersCount > 0)
                    {
                        foreach (SqlParameter SqlParam in SqlDataAdapter.SelectCommand.Parameters)
                        {
                            if (SqlParam.Direction == ParameterDirection.Output)
                            {
                                dalParameterList.Find((x) => x.ParameterName == SqlParam.ParameterName
                                                    && x.ParameterDirection == ParameterDirection.Output)
                                                    .ParameterValue = SqlParam.Value;
                            }
                        }
                    }//if(iOutputParametersCount > 0)

                }



                if (TempDataSet != null && TempDataSet.Tables.Count > 0)
                {
                    int counter = 0;
                    int tableNamesCount = TableNames != null ? TableNames.Length : 0;
                    foreach (DataTable table in TempDataSet.Tables)
                    {
                        if (counter < tableNamesCount)
                        {
                            table.TableName = TableNames[counter].Trim() == "" ? table.TableName : TableNames[counter];
                            counter++;
                        }
                    }

                }

                return TempDataSet;
            }
            catch (SqlException SqlEx)
            {
                throw SqlEx.GetBaseException();
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
            finally
            {
                if (SqlDataAdapter != null)
                    SqlDataAdapter.Dispose();

                if (connection != null)
                    connection.Dispose();

            }
        }
        #endregion

        #region BulkCopy
        public bool SqlBulkCopy(DataTable dataTable, string targetTable)
        {
            bool isSuccess = false;
            try
            {
                OpenConnection();
                // Create the SqlBulkCopy object. 
                // Note that the column positions in the source DataTable 
                // match the column positions in the destination table so 
                // there is no need to map columns. 
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = targetTable;
                    try
                    {
                        // Write from the source to the destination.
                        if (BatchSize > 0)
                        {
                            bulkCopy.BatchSize = BatchSize;
                        }

                        bulkCopy.WriteToServer(dataTable);
                        isSuccess = true;
                    }
                    catch (SqlException SqlEx)
                    {
                        throw SqlEx.GetBaseException();
                    }
                    catch (Exception ex)
                    {
                        throw ex.GetBaseException();
                    }
                }
                CloseConnection();
            }
            catch (SqlException SqlEx)
            {
                throw SqlEx.GetBaseException();
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
            return isSuccess;
        }
        #endregion 

        #endregion

        #region Function(s)
        private SqlDataAdapter GetSqlDataAdapter(string commandText = "")
        {
            if (!IsConnectionStringValid())
                throw new Exception("Connection string not set correctly!");

            if (this.connection == null)
                this.connection = new SqlConnection(this.connectionString);
            else
            {
                this.connection.Dispose();
                this.connection = null;
                this.connection = new SqlConnection(this.connectionString);
            }

            return new SqlDataAdapter(commandText, this.connection);
        }

        private SqlCommand GetSqlCommand(string commandText = "")
        {
            if (!IsConnectionStringValid())
                throw new Exception("Connection string not set correctly!");

            if (this.connection == null)
                this.connection = new SqlConnection(this.connectionString);
            else
            {
                this.connection.Dispose();
                this.connection = null;
                this.connection = new SqlConnection(this.connectionString);
            }

            return new SqlCommand(commandText, this.connection);
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
                        connection = new SqlConnection(connectionString);
                        connection.Open();
                    }
                }
                else
                {
                    connection = new SqlConnection(connectionString);
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
