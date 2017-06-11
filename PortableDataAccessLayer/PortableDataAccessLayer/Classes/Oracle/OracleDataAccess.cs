using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DataAccess
{

    internal class OracleDataAccess : IDataAccess
    {
        public DbDataReader GetDataReader(string commandText)
        {
            throw new NotImplementedException();
        }

        public DbDataReader GetDataReader(string commandText, CommandType commandType)
        {
            throw new NotImplementedException();
        }

        public DbDataReader GetDataReader(string commandText, CommandType commandType, DalParameterList dalParameterList)
        {
            throw new NotImplementedException();
        }

        public DataSet GetDataSet(string commandText, string[] TableNames = null)
        {
            throw new NotImplementedException();
        }

        public DataSet GetDataSet(string commandText, CommandType commandType, string[] TableNames = null)
        {
            throw new NotImplementedException();
        }

        public DataSet GetDataSet(string commandText, CommandType commandType, DalParameterList dalParameterList, string[] TableNames = null)
        {
            throw new NotImplementedException();
        }

        public DataTable GetDataTable(string commandText, string TableName = null)
        {
            throw new NotImplementedException();
        }

        public DataTable GetDataTable(string commandText, CommandType commandType, string TableName = null)
        {
            throw new NotImplementedException();
        }

        public DataTable GetDataTable(string commandText, CommandType commandType, DalParameterList dalParameterList, string TableName = null)
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery(string commandText)
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery(string commandText, CommandType commandType)
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery(string commandText, CommandType commandType, DalParameterList dalParameterList)
        {
            throw new NotImplementedException();
        }

        public object GetScalar(string commandText)
        {
            throw new NotImplementedException();
        }

        public object GetScalar(string commandText, CommandType commandType)
        {
            throw new NotImplementedException();
        }

        public object GetScalar(string commandText, CommandType commandType, DalParameterList dalParameterList)
        {
            throw new NotImplementedException();
        }

        
        public bool SqlBulkCopy(DataTable dataTable, string targetTable)
        {
            throw new NotImplementedException();
        }

        public void SetBatchSizeForBulkInsert(int size)
        {
            throw new NotImplementedException();
        }
    }
}
