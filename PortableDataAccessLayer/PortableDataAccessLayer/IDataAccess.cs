

using DataAccess.Entity;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DataAccess
{

    /// <summary>
    /// Interface IDataAccess
    /// </summary>
    public interface IDataAccess
    {

        /// <summary>
        /// Executes the Sql Command or Stored Procedure and returns a single value. 
        /// (First row's first cell value, if more than one row and column is returned.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure name</param>
        /// <returns>System.Object.</returns>
        object GetScalar(string commandText);


        /// <summary>
        /// Executes the Sql Command or Stored Procedure and returns a single value. 
        /// (First row's first cell value, if more than one row and column is returned.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure name</param>
        /// <param name="commandType">Type of command (i.e. Sql Command/ Stored Procedure name/ Table Direct)</param>
        /// <returns>System.Object.</returns>
        object GetScalar(string commandText, CommandType commandType);


        /// <summary>
        /// Executes the Sql Command or Stored Procedure and returns a single value. 
        /// (First row's first cell value, if more than one row and column is returned.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure name</param>
        /// <param name="commandType">Type of command (i.e. Sql Command/ Stored Procedure name/ Table Direct)</param>
        /// <param name="dalParameterList">Parameter List to be associated with the Command or Stored Procedure.</param>
        /// <returns>System.Object.</returns>
        object GetScalar(string commandText, CommandType commandType, DalParameterList dalParameterList);


        /// <summary>
        /// Executes Sql Command or Stored procedure and returns number of rows affected.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure name</param>
        /// <param name="commandType">Type of command (i.e. Sql Command/ Stored Procedure name/ Table Direct)</param>
        /// <returns>Number of rows affected.</returns>
        int ExecuteNonQuery(string commandText);


        /// <summary>
        /// Executes Sql Command or Stored procedure and returns number of rows affected.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure name</param>
        /// <param name="commandType">Type of command (i.e. Sql Command/ Stored Procedure name/ Table Direct)</param>
        /// <returns>Number of rows affected.</returns>
        int ExecuteNonQuery(string commandText, CommandType commandType);


        /// <summary>
        /// Executes Sql Command or Stored procedure and returns number of rows affected.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure name</param>
        /// <param name="commandType">Type of command (i.e. Sql Command/ Stored Procedure name/ Table Direct)</param>
        /// /// <param name="dalParameterList">Parameter List to be associated with the Command or Stored Procedure.</param>
        /// <returns>Number of rows affected.</returns>
        int ExecuteNonQuery(string commandText, CommandType commandType, DalParameterList dalParameterList);

        /// <summary>
        /// Executes the Sql Command or Stored Procedure and returns the DataReader.
        /// Call this method in using block to close the open connection automatically.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure Name</param>        
        /// <returns>DbDataReader.</returns>
        DbDataReader GetDataReader(string commandText);


        /// <summary>
        /// Executes the Sql Command or Stored Procedure and returns the DataReader.
        /// Call this method in using block to close the open connection automatically.
        /// eg. 
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure Name</param>        
        /// <param name="commandType">Type of command (i.e. Sql Command/ Stored Procedure name/ Table Direct)</param>
        /// <returns>DbDataReader</returns>
        DbDataReader GetDataReader(string commandText, CommandType commandType);


        /// <summary>
        /// Executes the Sql Command or Stored Procedure and returns the DataReader.
        /// Call this method in using block to close the open connection automatically.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure Name</param>        
        /// <param name="con">Database Connection object (DBHelper.GetConnObject() may be used)</param>
        /// <param name="commandType">Type of command (i.e. Sql Command/ Stored Procedure name/ Table Direct)</param>
        /// <param name="dalParameterList">Parameter List to be associated with the Command or Stored Procedure.</param>
        /// <returns>DbDataReader.</returns>
        DbDataReader GetDataReader(string commandText, CommandType commandType, DalParameterList dalParameterList);


        /// <summary>
        /// Executes the Sql Command or Stored Procedure and return result set in the form of DataTable.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure name</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Result in the form of DataTable</returns>
        DataTable GetDataTable(string commandText, string tableName = null);


        /// <summary>
        /// Executes the Sql Command or Stored Procedure and return result set in the form of DataTable.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure name</param>
        /// <param name="tableName">Table name</param>
        /// <param name="commandType">Type of command (i.e. Sql Command/ Stored Procedure name/ Table Direct)</param>
        /// <returns>Result in the form of DataTable</returns>
        DataTable GetDataTable(string commandText, CommandType commandType, string tableName = null);


        /// <summary>
        /// Executes the Sql Command or Stored Procedure and return result set in the form of DataTable.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure name</param>
        /// <param name="tableName">Table name</param>
        /// <param name="dalParameterList">Parameter List to be associated with the Command or Stored Procedure.</param>
        /// <param name="commandType">Type of command (i.e. Sql Command/ Stored Procedure name/ Table Direct)</param>
        /// <returns>Result in the form of DataTable</returns>
        DataTable GetDataTable(string commandText, CommandType commandType, DalParameterList dalParameterList, string tableName = null);


        /// <summary>
        /// Executes the Sql Command or Stored Procedure and return result set in the form of DataSet.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure name</param>
        /// <param name="tableNames">Table Names</param>
        /// <returns>Result in the form of DataSet</returns>
        DataSet GetDataSet(string commandText, string[] tableNames = null);


        /// <summary>
        /// Executes the Sql Command or Stored Procedure and return result set in the form of DataSet.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure name</param>
        /// <param name="tableNames">Table Names</param>
        /// <param name="commandType">Type of command (i.e. Sql Command/ Stored Procedure name/ Table Direct)</param>
        /// <returns>Result in the form of DataSet</returns>
        DataSet GetDataSet(string commandText, CommandType commandType, string[] tableNames = null);


        /// <summary>
        /// Executes the Sql Command or Stored Procedure and return result set in the form of DataSet.
        /// </summary>
        /// <param name="commandText">Sql Command or Stored Procedure name</param>
        /// <param name="tableNames">Table Names</param>
        /// <param name="commandType">Type of command (i.e. Sql Command/ Stored Procedure name/ Table Direct)</param>
        /// <param name="dalParameterList">Parameter List to be associated with the Command or Stored Procedure.</param>
        /// <returns>Result in the form of DataSet</returns>
        DataSet GetDataSet(string commandText, CommandType commandType, DalParameterList dalParameterList, string[] tableNames = null);

        /// <summary>
        /// Get relational table data.
        /// </summary>
        /// <param name="entityMetadata">entity to get master table details.</param>
        /// <returns>Returns data table.</returns>
        DataTable GetDataSetUsingAdapter(EntityMetadata entityMetadata);

        /// <summary>
        /// Get multiple relational tables data.
        /// </summary>
        /// <param name="entitiesMetadata">List of entities to get master table details.</param>
        /// <returns>Returns set of data tables.</returns>
        DataSet GetDataSetUsingAdapter(List<EntityMetadata> entitiesMetadata);

        /// <summary>
        /// Update new, updated and deleted records from data set into the database.
        /// </summary>
        /// <param name="entities">List of entities to get master table details.</param>
        /// <param name="dataSet">Data set to hold update data.</param>
        /// <returns>Returns number of rows successfully updated from the data set.</returns>
        int ExecuteDataSetUsingAdapter(List<EntityMetadata> entities, DataSet dataSet);

        /// <summary>
        /// Set the batch size to process data batch-wise.
        /// </summary>
        /// <param name="size">Data size for bulk insert.</param>
        void SetBatchSizeForBulkInsert(int size);

        /// <summary>
        /// Insert data in target table.
        /// </summary>
        /// <param name="dataTable">Contains rows which need to insert</param>
        /// <param name="targetTable">Target table name</param>
        /// <returns></returns>
        bool SqlBulkCopy(DataTable dataTable, string targetTable);
    }
}
