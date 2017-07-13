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
    public class MSSqlAction : BaseAbstractAction
    {
        SqlTransaction DbTransaction;
        SqlConnection DbConnection
        {
            get { return DBConnection as SqlConnection; }
            set { DBConnection = value; }
        }
        SqlCommand DbCommand { get; set; }

        SqlDataAdapter adapter { get; set; }
        string testString { get; set; }

        public MSSqlAction()
        { }
        public override string DbCommandString
        {
            get { return DbCommand?.CommandText; }
        }

        public override bool IsTransacting
        {
            get { return DbTransaction != null; }
        }
        protected override IDbConnection GetConnectionFactory()
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


        protected override bool Query(string sqlString, ICollection parameterCollection, ref DataTable dataTable)
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

        protected override bool Query<T>(string sqlString, object parameterCollection, ref List<T> dataTable)
        {
            throw new NotImplementedException();
        }


        protected override bool ExecuteNonQuery(string sqlString, ICollection parameterCollection, ref int effectRows)
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

        protected override bool ExecuteStoredProcedure(string procedureName, bool bindbyName, ICollection parameterCollection, List<DBParameter> dBParameter)
        {
            throw new NotImplementedException();
        }


        protected override void DbCommit()
        {
            throw new NotImplementedException();
        }


        protected override void DbRollBack()
        {
            throw new NotImplementedException();
        }

        protected override IDbTransaction BeginTransaction()
        {
            return DbTransaction = DbConnection.BeginTransaction();
        }

        protected override IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
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
