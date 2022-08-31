using Ancestor.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ancestor.Core;
using System.Collections;
using System.Data;

namespace Ancestor.DataAccess.DBAction
{
    /// <summary>
    /// Creator : WizTonE 
    /// Date    : 2017/04/17
    /// Subject : BaseAbstractAction
    /// 
    /// History : 
    /// 2017/04/17 WizTonE 建立 : DbAction底層之Abstract Class, 繼承IDbAction, 並提供virtual機制供override使用
    /// </summary>
    public abstract class BaseAbstractAction : DbAction, IDbAction
    {
        private bool _disposed = false;
        public virtual IDbTransaction DbTransaction { get; set; }


        #region Virtual Function
        protected virtual IDbConnection DBConnection { set; get; }

        protected virtual IDbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        protected virtual IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        protected virtual bool BulkInsert<T>(List<T> objectList, ref int effectRows) where T : class, IModel, new()
        {
            throw new NotImplementedException();
        }

        protected virtual bool CheckConnectionState()
        {
            throw new NotImplementedException();
        }

        protected virtual void DbCommit()
        {
            throw new NotImplementedException();
        }

        protected virtual void DbRollBack()
        {
            throw new NotImplementedException();
        }

        protected virtual bool ExecuteNonQuery(string sqlString, ICollection parameterCollection, ref int successRow)
        {
            throw new NotImplementedException();
        }

        protected virtual bool ExecuteStoredProcedure(string procedureName, bool bindbyName, ICollection parameterCollection, List<DBParameter> dBParameter)
        {
            throw new NotImplementedException();
        }

        protected virtual IDbConnection GetConnectionFactory()
        {
            throw new NotImplementedException();
        }

        protected virtual bool Query(string sqlString, ICollection parameterCollection, ref DataTable dataTable)
        {
            throw new NotImplementedException();
        }

        protected virtual bool Query<T>(string sqlString, object parameterCollection, ref List<T> dataList)
        {
            throw new NotImplementedException();
        }
        protected virtual bool Query(string sqlString, object parameterCollection, ref List<object> dataList, Type realType)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Interface Implement
        string IDbAction.ErrorMessage { get { return ErrorMessage; } set { ErrorMessage = value; } }

        IDbConnection IDbAction.DBConnection { get { return DBConnection; } }

        IDbTransaction IDbAction.BeginTransaction()
        {
            return BeginTransaction();
        }

        IDbTransaction IDbAction.BeginTransaction(IsolationLevel isolationLevel)
        {
            return BeginTransaction(isolationLevel);
        }

        bool IDbAction.BulkInsert<T>(List<T> objectList, ref int effectRows)
        {
            return BulkInsert<T>(objectList, ref effectRows);
        }

        bool IDbAction.CheckConnectionState()
        {
            return CheckConnectionState();
        }

        void IDbAction.DbCommit()
        {
            DbCommit();
        }

        void IDbAction.DbRollBack()
        {
            DbRollBack();
        }

        bool IDbAction.ExecuteNonQuery(string sqlString, ICollection parameterCollection, ref int successRow)
        {
            return ExecuteNonQuery(sqlString, parameterCollection, ref successRow);
        }

        bool IDbAction.ExecuteStoredProcedure(string procedureName, bool bindbyName, ICollection parameterCollection, List<DBParameter> dBParameter)
        {
            return ExecuteStoredProcedure(procedureName, bindbyName, parameterCollection, dBParameter);
        }

        IDbConnection IDbAction.GetConnectionFactory()
        {
            return GetConnectionFactory();
        }

        bool IDbAction.Query(string sqlString, ICollection parameterCollection, ref DataTable dataTable)
        {
            return Query(sqlString, parameterCollection, ref dataTable);
        }

        bool IDbAction.Query<T>(string sqlString, object parameterCollection, ref List<T> dataList)
        {
            return Query<T>(sqlString, parameterCollection, ref dataList);
        }

        bool IDbAction.Query(string sqlString, object parameterCollection, ref List<object> dataList, Type realType)
        {
            return Query(sqlString, parameterCollection, ref dataList, realType);
        }
        #endregion


        ~BaseAbstractAction()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    Disposing();
                _disposed = true;
            }
        }

        protected virtual void Disposing()
        {
            if (DbTransaction != null)
            {
                DbRollBack();
                DbTransaction = null;
            }
            DBConnection.Dispose();
            DBConnection = null;
        }
    }
}
