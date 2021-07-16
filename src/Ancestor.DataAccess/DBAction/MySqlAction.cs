using Ancestor.Core;
using Ancestor.DataAccess.Interface;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DBAction
{
    /// <summary>
    /// Author  : Andycow0 
    /// Date    : 2015/07/31 14:00
    /// Subject : MySqlAction 
    /// 
    /// History : 
    /// 2015/07/31 Andycow0 建立
    /// 2017/04/17 WizTonE 改繼承BaseAbstractAction, 提供override機制
    /// </summary>
    public class MySqlAction : BaseAbstractAction
    {
        //MySqlTransaction DbTransaction;
        MySqlConnection DbConnection { get; set; }
        MySqlCommand DbCommand { get; set; }

        MySqlDataAdapter adapter { get; set; }
        string testString { get; set; }
        object locker = new object();

        public MySqlAction()
        { }
        protected override IDbConnection DBConnection
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
        protected override IDbConnection GetConnectionFactory()
        {
            IDBConnection conn = new ConnectionFactory(DbObject);
            return DbConnection = (MySqlConnection)conn.GetConnectionFactory().GetConnectionObject();
        }

        public MySqlAction(DBObject _dBObject)
        {
            DbObject = _dBObject;
            //DbConnection = (SqlConnection)GetConnectionFactory();
            testString = "select 1 ";
        }

        protected override bool CheckConnectionState()
        {
            throw new NotImplementedException();
        }

        protected override bool Query(string sqlString, ICollection parameterCollection, ref DataTable dataTable)
        {
            lock (locker)
            {
                bool is_success = false;
                ErrorMessage = string.Empty;
                DbCommand = DbConnection.CreateCommand();
                DbCommand.CommandText = sqlString;
                adapter = new MySqlDataAdapter();
                //DbCommand.BindByName = true;
                //DbCommand.AddRowid = true;

                if (CheckConnection(DbConnection, DbCommand, testString))
                {
                    try
                    {
                        var parameters = (List<MySqlParameter>)parameterCollection;
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
        }

        protected override bool Query<T>(string sqlString, object parameterCollection, ref List<T> dataTable)
        {
            throw new NotImplementedException();
        }


        protected override bool ExecuteNonQuery(string sqlString, ICollection parameterCollection, ref int effectRows)
        {
            lock (locker)
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
                        var parameters = (List<MySqlParameter>)parameterCollection;
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
        }

        protected override bool ExecuteStoredProcedure(string procedureName, bool bindbyName, ICollection parameterCollection, List<DBParameter> dBParameter)
        {
            throw new NotImplementedException();
        }


        protected override void DbCommit()
        {
            if (DbTransaction != null)
            {
                DbTransaction.Commit();
                DbTransaction = null;
                CloseConnection();
            }
        }


        protected override void DbRollBack()
        {
            if (DbTransaction != null)
            {
                DbTransaction.Rollback();
                DbTransaction = null;
                CloseConnection();
            }
        }

        protected override bool BulkInsert<T>(List<T> objectList, ref int effectRows)
        {
            throw new NotImplementedException();
        }

        protected override IDbTransaction BeginTransaction()
        {
            if (CheckConnection(DbConnection, DbCommand, testString))
            {
                DbTransaction = DbConnection.BeginTransaction();
            }
            return DbTransaction;
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