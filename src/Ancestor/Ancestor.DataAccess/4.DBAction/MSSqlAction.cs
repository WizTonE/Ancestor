using Ancestor.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Ancestor.DataAccess.Factory;
using System.Data.SqlClient;
using Ancestor.DataAccess.Interface;

namespace Ancestor.DataAccess.DBAction
{
    /// <summary>
    /// Author  : Andycow0 
    /// Date    : 2015/07/31 10:00
    /// Subject : MSSqlAction
    /// 
    /// History : 
    /// 2015/07/31 Andycow0 建立
    /// </summary>
    public class MSSqlAction : DbAction, IDbAction
    {
        SqlTransaction DbTransaction;
        SqlConnection DbConnection { get; set; }
        SqlCommand DbCommand { get; set; }

        SqlDataAdapter adapter { get; set; }
        string testString { get; set; }

        public MSSqlAction()
        { }
        public IDbConnection DBConnection
        {
            get { return DbConnection; }
        }
        public override string DbCommandString
        {
            get { return DbCommand?.CommandText; }
        }

        public override bool IsTransacting
        {
            get { return DbTransaction != null; }
        }
        public IDbConnection GetConnectionFactory()
        {
            IDBConnection conn = new ConnectionFactory(DbObject);
            return DbConnection = (SqlConnection)conn.GetConnectionFactory().GetConnectionObject();
        }

        public MSSqlAction(DBObject _dBObject)
        {
            DbObject = _dBObject;
            //DbConnection = (SqlConnection)GetConnectionFactory();
            testString = "select 1 ";
        }

        public bool CheckConnectionState()
        {
            throw new NotImplementedException();
        }

        public bool Query(string sqlString, ICollection parameterCollection, ref DataTable dataTable)
        {
            bool is_success = false;
            ErrorMessage = string.Empty;
            DbCommand = DbConnection.CreateCommand();
            DbCommand.CommandText = sqlString;
            adapter = new SqlDataAdapter();
            //DbCommand.BindByName = true;
            //DbCommand.AddRowid = true;

            if (CheckConnection(DbConnection, DbCommand, testString))
            {
                try
                {
                    var parameters = (List<SqlParameter>)parameterCollection;
                    DbCommand.Parameters.AddRange(parameters.ToArray());
                    adapter.SelectCommand = DbCommand;
                    adapter.Fill(dataTable);
                    is_success = true;
                }
                catch (Exception exception)
                {
                    is_success = false;
                    ErrorMessage = exception.ToString();
                }
            }
            CloseConnection();
            return is_success;
        }

        public bool Query<T>(string sqlString, object parameterCollection, ref List<T> dataTable) where T : class, new()
        {
            throw new NotImplementedException();
        }


        public bool ExecuteNonQuery(string sqlString, ICollection parameterCollection, ref int effectRows)
        {
            bool isSuccessful = false;
            ErrorMessage = string.Empty;
            DbCommand = DbConnection.CreateCommand();
            DbCommand.CommandText = sqlString;
            //DbCommand.BindByName = true;

            if (CheckConnection(DbConnection, DbCommand, testString))
            {
                // 2016-05-23 Commend.
                //if (DbTransaction == null)
                //{
                //    DbTransaction = DbConnection.BeginTransaction();
                //}
                //
                try
                {
                    var parameters = (List<SqlParameter>)parameterCollection;
                    DbCommand.Parameters.AddRange(parameters.ToArray());
                    DbCommand.CommandText = sqlString;

                    // 2015-09-01
                    //DbCommand.ExecuteNonQuery();
                    effectRows = DbCommand.ExecuteNonQuery();
                    isSuccessful = true;
                }
                catch (Exception exception)
                {
                    // 2016-05-23 Commend.
                    //DbTransaction.Rollback();
                    isSuccessful = false;
                    ErrorMessage = exception.ToString();
                }
            }
            // 2016-04-05 commend this line for transaction feature.
            //DbConnection.Close();
            CloseConnection();
            return isSuccessful;
        }

        public bool ExecuteStoredProcedure(string procedureName, bool bindbyName, ICollection parameterCollection, List<DBParameter> dBParameter)
        {
            throw new NotImplementedException();
        }


        public void DbCommit()
        {
            throw new NotImplementedException();
        }


        public void DbRollBack()
        {
            throw new NotImplementedException();
        }

        bool IDbAction.BulkInsert<T>(List<T> objectList, ref int effectRows)
        {
            throw new NotImplementedException();
        }

        public IDbTransaction BeginTransaction()
        {
            return DbTransaction = DbConnection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return DbTransaction = DbConnection.BeginTransaction(isolationLevel);
        }

        private void CloseConnection()
        {
            if (DbTransaction == null)
                DbConnection.Close();
        }
    }
}
