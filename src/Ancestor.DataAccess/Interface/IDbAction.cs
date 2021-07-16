using Ancestor.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.Interface
{
    /// <summary>
    /// Author  : WizTonE 
    /// Date    : 2015/07/28
    /// Subject : IDBAction 訂定資料庫操作之Interface
    /// 
    /// History : 
    /// 2015/07/28 WizTonE 建立
    /// 2016/04/05 Andycow0 add features DbCommit and DbRollBack.
    /// 2016/05/11 WizTonE added BulkInsert.
    /// 2017/06/15 新增DbCommand
    /// 2017/06/16 新增IsTransacting
    /// </summary>
    public interface IDbAction : IDisposable
    {
        string ErrorMessage { get; set; }
        IDbConnection DBConnection { get; }
        IDbConnection GetConnectionFactory();        
        bool CheckConnectionState();
        bool Query(string sqlString, ICollection parameterCollection, ref DataTable dataTable);
        bool Query<T>(string sqlString, object parameterCollection, ref List<T> dataList) where T: class, new();
        bool Query(string sqlString, object parameterCollection, ref List<object> dataList, Type realType);
        bool ExecuteNonQuery(string sqlString, ICollection parameterCollection, ref int successRow);
        bool ExecuteStoredProcedure(string procedureName, bool bindbyName, ICollection parameterCollection, List<DBParameter> dBParameter);
        bool BulkInsert<T>(List<T> objectList, ref int effectRows) where T : class, IModel, new();
        //
        void DbCommit();
        void DbRollBack();

        IDbTransaction BeginTransaction();
        IDbTransaction BeginTransaction(IsolationLevel isolationLevel);
        string DbCommandString { get; }
        bool IsTransacting { get; }
    }
}
