using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using Ancestor.Core;
using Ancestor.DataAccess.Factory;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using Ancestor.DataAccess.SqlMapper;
using Ancestor.DataAccess.Interface;
using System.IO;

namespace Ancestor.DataAccess.DBAction
{
    /// <summary>
    /// Creator : Andycow0 
    /// Date    : 2015/07/28
    /// Subject : Oracle Action
    /// 
    /// History : 
    /// 2015/07/28 WizTonE 建立
    /// 2015/09/01 Andycow0, 因發現 delete 時, 未回傳影響欄位數量, 而增加 ExecuteNonQuery 回傳 effectRows 的值
    /// 2016/04/05 Andycow0  1. 新增 Transaction 功能，包括 DbCommit 與 DbRollBack 方法. 
    ///                      2. 並取消 ExecuteNonQuery 中的 DbConnection.Close() 功能.
    /// 2016/09-14 Andycow0  Added IDbConnection for returning Connection of DB.
    /// </summary>
    public class OracleAction : BaseAbstractAction
    {
        #region Settings
        private delegate bool TryParseDelegate<T>(string text, out T value);
        private static int? _cmdFetchSize;
        private static int? _cmdInitialLongFetchSize;
        private static int? _cmdInitialLobFetchSize;
        static OracleAction()
        {
            _cmdFetchSize = GetConfigValue<int>("OracleCommandFetchSize", int.TryParse);
            _cmdInitialLongFetchSize = GetConfigValue<int>("OracleCommandInitialLongFetchSize", int.TryParse);
            _cmdInitialLobFetchSize = GetConfigValue<int>("OracleCommandInitialLobFetchSize", int.TryParse);
        }

        private static Nullable<T> GetConfigValue<T>(string key, TryParseDelegate<T> tryParseDelegate) where T : struct
        {
            var settingText = System.Configuration.ConfigurationManager.AppSettings.Get(key);
            T value;
            if (tryParseDelegate(settingText, out value))
                return new Nullable<T>(value);
            return null;
        }
        #endregion


        OracleConnection DbConnection
        {
            get { return DBConnection as OracleConnection; }
            set { DBConnection = value; }
        }
        OracleCommand DbCommand { get; set; }
        OracleDataAdapter adapter { get; set; }
        int InitialLongFetchSize
        {
            get { return _cmdInitialLongFetchSize ?? -1; }
        }
        int InitialLobFetchSize
        {
            get { return _cmdInitialLobFetchSize ?? -1; }
        }

        //
        //IDbTransaction DbTransaction { get; set; }
        object locker = new object();
        //TODO: 與OracleDaoAction的DbTypeDic整合
        Dictionary<string, OracleDbType> _OracleDbTypeDic = new Dictionary<string, OracleDbType>
                {
                    { "VARCHAR2", OracleDbType.Varchar2 },
                    { "SYSTEM.STRING", OracleDbType.Varchar2 },
                    { "STRING", OracleDbType.Varchar2 },
                    { "SYSTEM.DATETIME", OracleDbType.Date },
                    { "DATETIME", OracleDbType.Date },
                    { "DATE", OracleDbType.Date },
                    { "INT64", OracleDbType.Int64 },
                    { "INT32", OracleDbType.Int32 },
                    { "INT16", OracleDbType.Int16 },
                    { "BYTE", OracleDbType.Byte },
                    { "DECIMAL", OracleDbType.Decimal },
                    { "FLOAT", OracleDbType.Single },
                    { "DOUBLE", OracleDbType.Double },
                    { "BYTE[]", OracleDbType.Blob },
                    { "CHAR", OracleDbType.Char },
                    { "CHAR[]", OracleDbType.Char },
                    { "TIMESTAMP", OracleDbType.TimeStamp },
                    { "REFCURSOR", OracleDbType.RefCursor },
                    { "CLOB", OracleDbType.Clob },
                    { "LONG", OracleDbType.Long }
                };
        //
        string testString { get; set; }

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
            DbConnection = (OracleConnection)conn.GetConnectionFactory().GetConnectionObject();
            return DbConnection;
        }

        public OracleAction()
        {

        }

        public OracleAction(DBObject _dBObject)
        {
            DbObject = _dBObject;
            //DbConnection = (OracleConnection)GetConnectionFactory();
            testString = "select 1 from dual";
        }

        private class TimeoutObject : IDisposable
        {
            private bool _disposed = false;
            private System.Timers.Timer _timer;
            public TimeoutObject()
            {
                if (AncestorGlobalOptions.EnableTimeout)
                {
                    _timer = new System.Timers.Timer(AncestorGlobalOptions.TimeoutInterval);
                    _timer.Elapsed += (s, e) => throw new TimeoutException("execute timeout on " + e.SignalTime.ToString("yyyy/MM/dd HH:mm:ss"));
                    _timer.Start();
                }
            }



            ~TimeoutObject()
            {
                Dispose(false);
            }
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            protected void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    if (disposing)
                    {
                        if (_timer != null)
                        {
                            _timer.Stop();
                        }
                    }
                    _disposed = true;
                }
            }
        }
        protected override bool Query(string sqlString, ICollection parameterCollection, ref DataTable dataTable)
        {
            lock (locker)
            {
                bool is_success = false;
                ErrorMessage = string.Empty;
                DbCommand = DbConnection.CreateCommand();
                DbCommand.CommandText = sqlString;
                DbCommand.InitialLONGFetchSize = InitialLongFetchSize;
                DbCommand.InitialLOBFetchSize = InitialLobFetchSize;
                adapter = new OracleDataAdapter();
                DbCommand.BindByName = true;
                //DbCommand.AddRowid = true;
                if (_cmdFetchSize != null)
                    DbCommand.FetchSize = _cmdFetchSize.Value;
                using (var timeout = new TimeoutObject())
                {
                    var flgConnEof = 0;
                    do
                    {
                        if (CheckConnection(DbConnection, DbCommand, testString))
                        {
                            try
                            {
                                try
                                {
                                    var parameters = (List<OracleParameter>)parameterCollection;
                                    DbCommand.Parameters.AddRange(parameters.ToArray());
                                    adapter.SelectCommand = DbCommand;
                                    adapter.Fill(dataTable);
                                    is_success = true;
                                }
                                catch (OracleException ex)
                                {
                                    if (ex.Number == 3113)
                                        ++flgConnEof;
                                    throw;
                                }
                            }
                            catch (Exception exception)
                            {
                                is_success = false;
                                ErrorMessage = exception.ToString();
                                Error = exception;
                            }
                        }
                        CloseConnection();
                    }
                    while (!is_success && (flgConnEof > 0 && flgConnEof <= 3));
                }
                return is_success;
            }
        }
        protected override bool Query(string sqlString, object parameterCollection, ref List<object> dataList, Type realType)
        {
            lock (locker)
            {
                bool is_success = false;
                ErrorMessage = string.Empty;
                DbCommand = DbConnection.CreateCommand();
                DbCommand.CommandText = sqlString;
                DbCommand.InitialLONGFetchSize = InitialLongFetchSize;
                DbCommand.InitialLOBFetchSize = InitialLobFetchSize;
                adapter = new OracleDataAdapter();
                DbCommand.BindByName = true;
                //DbCommand.AddRowid = true;
                if (_cmdFetchSize != null)
                    DbCommand.FetchSize = _cmdFetchSize.Value;
                using (var timeout = new TimeoutObject())
                {
                    var flgConnEof = 0;
                    do
                    {
                        if (CheckConnection(DbConnection, DbCommand, testString))
                        {
                            try
                            {
                                try
                                {
                                    //var parameters = (IEnumerable)parameterCollection;
                                    dataList = DbConnection.QueryMultiple(sqlString, parameterCollection).Read(realType).ToList();
                                    is_success = true;
                                }
                                catch (OracleException ex)
                                {
                                    if (ex.Number == 3113)
                                        ++flgConnEof;
                                    throw;
                                }
                            }
                            catch (Exception exception)
                            {
                                is_success = false;
                                ErrorMessage = exception.ToString();
                            }
                        }
                        CloseConnection();
                    }
                    while (!is_success && (flgConnEof > 0 && flgConnEof <= 3));
                }
                return is_success;
            }
        }
        protected override bool Query<T>(string sqlString, object parameterCollection, ref List<T> dataList)
        {
            lock (locker)
            {
                bool is_success = false;
                ErrorMessage = string.Empty;
                DbCommand = DbConnection.CreateCommand();
                DbCommand.CommandText = sqlString;
                adapter = new OracleDataAdapter();
                DbCommand.InitialLONGFetchSize = InitialLongFetchSize;
                DbCommand.InitialLOBFetchSize = InitialLobFetchSize;
                DbCommand.BindByName = true;
                if (_cmdFetchSize != null)
                    DbCommand.FetchSize = _cmdFetchSize.Value;
                using (var timeout = new TimeoutObject())
                {
                    var flgConnEof = 0;
                    do
                    {
                        if (CheckConnection(DbConnection, DbCommand, testString))
                        {
                            try
                            {
                                try
                                {
                                    //var parameters = (IEnumerable)parameterCollection;
                                    dataList = DbConnection.QueryMultiple(sqlString, parameterCollection).Read<T>().ToList();
                                    is_success = true;
                                }
                                catch (OracleException ex)
                                {
                                    if (ex.Number == 3113)
                                        ++flgConnEof;
                                    throw;
                                }
                            }
                            catch (Exception exception)
                            {
                                is_success = false;
                                ErrorMessage = exception.ToString();
                            }
                        }
                        CloseConnection();
                    }
                    while (!is_success && (flgConnEof > 0 && flgConnEof <= 3));
                }
                return is_success;
            }
        }

        protected override bool ExecuteNonQuery(string sqlString, ICollection parameterCollection, ref int effectRows)
        {
            lock (locker)
            {
                bool isSuccessful = false;
                ErrorMessage = string.Empty;
                DbCommand = DbConnection.CreateCommand();
                DbCommand.CommandText = sqlString;
                DbCommand.BindByName = true;
                DbCommand.AddRowid = true;
                DbCommand.InitialLONGFetchSize = InitialLongFetchSize;
                DbCommand.InitialLOBFetchSize = InitialLobFetchSize;
                if (_cmdFetchSize != null)
                    DbCommand.FetchSize = _cmdFetchSize.Value;
                using (var timeout = new TimeoutObject())
                {
                    var flgConnEof = 0;
                    do
                    {
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
                                try
                                {
                                    var parameters = (List<OracleParameter>)parameterCollection;
                                    DbCommand.Parameters.AddRange(parameters.ToArray());
                                    DbCommand.CommandText = sqlString;

                                    // 2015-09-01
                                    //DbCommand.ExecuteNonQuery();
                                    effectRows = DbCommand.ExecuteNonQuery();
                                    isSuccessful = true;
                                }
                                catch (OracleException ex)
                                {
                                    if (ex.Number == 3113)
                                        ++flgConnEof;
                                    throw;
                                }
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
                        CloseConnection();
                    }
                    while (!isSuccessful && (flgConnEof > 0 && flgConnEof <= 3));
                }
                return isSuccessful;
            }
        }

        protected override bool ExecuteStoredProcedure(string procedureName, bool bindbyName, ICollection parameterCollection, List<DBParameter> dBParameter)
        {
            lock (locker)
            {
                bool is_success = false;
                ErrorMessage = string.Empty;
                DbCommand = DbConnection.CreateCommand();
                DbCommand.CommandText = procedureName;
                DbCommand.CommandType = CommandType.StoredProcedure;
                DbCommand.BindByName = bindbyName;
                //DbCommand.AddRowid = true;
                DbCommand.InitialLONGFetchSize = InitialLongFetchSize;
                DbCommand.InitialLOBFetchSize = InitialLobFetchSize;
                if (_cmdFetchSize != null)
                    DbCommand.FetchSize = _cmdFetchSize.Value;
                using (var timeout = new TimeoutObject())
                {
                    var flgConnEof = 0;
                    do
                    {
                        if (CheckConnection(DbConnection, DbCommand, testString))
                        {
                            try
                            {
                                try
                                {
                                    var parameters = (List<OracleParameter>)parameterCollection;
                                    DbCommand.Parameters.AddRange(parameters.ToArray());
                                    DbCommand.ExecuteNonQuery();
                                    is_success = true;

                                    var directionFilter = new ParameterDirection[] { ParameterDirection.Output, ParameterDirection.InputOutput, ParameterDirection.ReturnValue };
                                    foreach (OracleParameter OPara in DbCommand.Parameters)
                                    {
                                        if (directionFilter.Contains(OPara.Direction))
                                        {
                                            var parameter = dBParameter.FirstOrDefault(r => r.Name == OPara.ParameterName && r.ParameterDirection == OPara.Direction);
                                            if (parameter != null)
                                            {
                                                if (OPara.OracleDbType == OracleDbType.RefCursor)
                                                {
                                                    adapter = new OracleDataAdapter(DbCommand);
                                                    DataTable dt = new DataTable("Result");
                                                    adapter.Fill(dt, (OracleRefCursor)OPara.Value);
                                                    parameter.Value = dt;
                                                }
                                                else if (OPara.OracleDbType == OracleDbType.Clob)
                                                {
                                                    var clob = (OracleClob)OPara.Value;
                                                    var reader = new StreamReader(clob, Encoding.Unicode);
                                                    char[] buffer = new char[parameter.Size];
                                                    int actual = 0;

                                                    while ((actual = reader.Read(buffer, 0, buffer.Length)) > 0)
                                                        parameter.Value = new string(buffer, 0, actual);
                                                }
                                                else
                                                    parameter.Value = OPara.Value;
                                            }
                                        }
                                    }
                                }
                                catch (OracleException ex)
                                {
                                    if (ex.Number == 3113)
                                        ++flgConnEof;
                                    throw;
                                }
                                //foreach (DBParameter Parameter in dBParameter)
                                //{
                                //    if (Parameter.ParameterDirection == ParameterDirection.Output)
                                //    {
                                //        foreach (OracleParameter OPara in DbCommand.Parameters)
                                //        {
                                //            if (OPara.Direction == ParameterDirection.Output && OPara.ParameterName == Parameter.Name)
                                //            {
                                //                if (OPara.OracleDbType == OracleDbType.RefCursor)
                                //                {
                                //                    adapter = new OracleDataAdapter(DbCommand);
                                //                    DataTable dt = new DataTable("Result");
                                //                    adapter.Fill(dt, (OracleRefCursor)OPara.Value);
                                //                    Parameter.Value = dt;
                                //                }
                                //                else if (OPara.OracleDbType == OracleDbType.Clob)
                                //                {
                                //                    var clob = (OracleClob)OPara.Value;
                                //                    var reader = new StreamReader(clob, Encoding.Unicode);
                                //                    char[] buffer = new char[OPara.Size];
                                //                    int actual = 0;
                                //                    while ((actual = reader.Read(buffer, 0, buffer.Length)) > 0)
                                //                        Parameter.Value = new string(buffer, 0, actual);
                                //                }
                                //                else
                                //                    Parameter.Value = OPara.Value;
                                //                break;


                                //            }
                                //        }
                                //    }
                                //    if (Parameter.ParameterDirection == ParameterDirection.InputOutput)
                                //    {
                                //        foreach (OracleParameter OPara in DbCommand.Parameters)
                                //        {
                                //            if (OPara.Direction == ParameterDirection.InputOutput)
                                //            {
                                //                if (OPara.ParameterName == Parameter.Name)
                                //                {
                                //                    if (OPara.OracleDbType == OracleDbType.RefCursor)
                                //                    {
                                //                        adapter = new OracleDataAdapter(DbCommand);
                                //                        DataTable dt = new DataTable("Result");
                                //                        adapter.Fill(dt, (OracleRefCursor)OPara.Value);
                                //                        Parameter.Value = dt;
                                //                    }
                                //                    else if (OPara.OracleDbType == OracleDbType.Clob)
                                //                    {
                                //                        var clob = (OracleClob)OPara.Value;
                                //                        var reader = new StreamReader(clob, Encoding.Unicode);
                                //                        char[] buffer = new char[OPara.Size];
                                //                        int actual = 0;
                                //                        while ((actual = reader.Read(buffer, 0, buffer.Length)) > 0)
                                //                            Parameter.Value = new string(buffer, 0, actual);
                                //                    }
                                //                    else
                                //                        Parameter.Value = OPara.Value;
                                //                }
                                //            }
                                //        }
                                //    }
                                //    if (Parameter.ParameterDirection == ParameterDirection.ReturnValue)
                                //    {
                                //        foreach (OracleParameter OPara in DbCommand.Parameters)
                                //        {
                                //            if (OPara.Direction == ParameterDirection.ReturnValue)
                                //            {
                                //                if (OPara.ParameterName == Parameter.Name)
                                //                {
                                //                    if (OPara.OracleDbType == OracleDbType.RefCursor)
                                //                    {
                                //                        adapter = new OracleDataAdapter(DbCommand);
                                //                        DataTable dt = new DataTable("Result");
                                //                        adapter.Fill(dt, (OracleRefCursor)OPara.Value);
                                //                        Parameter.Value = dt;
                                //                    }
                                //                    else if (OPara.OracleDbType == OracleDbType.Clob)
                                //                    {
                                //                        var clob = (OracleClob)OPara.Value;
                                //                        var reader = new StreamReader(clob, Encoding.Unicode);
                                //                        char[] buffer = new char[OPara.Size];
                                //                        int actual = 0;
                                //                        while ((actual = reader.Read(buffer, 0, buffer.Length)) > 0)
                                //                            Parameter.Value = new string(buffer, 0, actual);
                                //                    }
                                //                    else
                                //                        Parameter.Value = OPara.Value;
                                //                }
                                //            }
                                //        }
                                //    }
                                //}
                            }
                            catch (Exception exception)
                            {
                                is_success = false;
                                ErrorMessage = exception.ToString();
                            }
                        }
                        CloseConnection();
                    }
                    while (!is_success && (flgConnEof > 0 && flgConnEof <= 3));
                }
                return is_success;
            }
        }
        // 2016-04-05 Add feature for transaction.
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

        protected override bool BulkInsert<T>(List<T> objList, ref int effectRows)
        {
            lock (locker)
            {
                string table_name = string.Empty;
                int loop_for = 0;
                bool isSuccessful = false;
                ErrorMessage = string.Empty;
                StringBuilder sb = new StringBuilder();
                StringBuilder sb2 = new StringBuilder();

                if (objList.Count > 0)
                {
                    loop_for = (int)Math.Ceiling(Math.Round((double)objList.Count / 30000, 10));
                    for (int i = 0; i < (loop_for); i++)
                    {
                        if (CheckConnection(DbConnection, DbCommand, testString))
                        {
                            DbCommand = DbConnection.CreateCommand();
                            DbCommand.BindByName = true;
                            List<T> TempList = objList.GetRange(i * 30000, Math.Min(30000, objList.Count - i * 30000));
                            //ArrayBindCount一定要填入, 否則會跳ora-01404 insert value too large for column
                            DbCommand.ArrayBindCount = TempList.Count;
                            //if (sb.Length == 0 && sb2.Length == 0)
                            //{
                            foreach (PropertyInfo prop in TempList[0].GetType().GetProperties())
                            {
                                table_name = TempList[0].GetType().Name;
                                if (prop.Name != "ROWID")
                                {
                                    sb.Append(prop.Name.ToUpper() + ",");
                                    sb2.Append(":" + prop.Name.ToUpper() + ",");

                                    var propertyType = prop.PropertyType;
                                    if (prop.PropertyType.IsGenericType &&
                                            prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                        propertyType = prop.PropertyType.GetGenericArguments()[0];
                                    //var ttt = prop.GetType();
                                    var expression = DynamicSelect<T, dynamic>(prop);
                                    var valueList = TempList.Select(expression).ToArray();

                                    OracleDbType dbType = OracleDbType.Varchar2;
                                    _OracleDbTypeDic.TryGetValue(propertyType.Name.ToUpper(), out dbType);

                                    DbCommand.Parameters.Add(":" + prop.Name.ToUpper(), dbType, valueList, ParameterDirection.Input);
                                }
                            }
                            sb.Remove(sb.Length - 1, 1);
                            sb2.Remove(sb2.Length - 1, 1);
                            try
                            {
                                DbCommand.CommandText = "INSERT INTO " + table_name + " (" + sb + ")" + " values (" + sb2 + ")";
                                //}
                                effectRows += DbCommand.ExecuteNonQuery();
                                isSuccessful = true;
                            }
                            catch (Exception exception)
                            {
                                isSuccessful = false;
                                ErrorMessage = exception.ToString();
                            }

                            sb.Clear();
                            sb2.Clear();
                        }
                    }
                    CloseConnection();
                }
                return isSuccessful;
            }
        }

        //private Func<TItem, object> SelectExpression<TItem, TField>(string fieldName)
        //{

        //    var param = Expression.Parameter(typeof(TItem), "item");
        //    var field = Expression.Property(param, fieldName);
        //    return Expression.Lambda<Func<TItem, object>>(field, new ParameterExpression[] { param }).Compile();

        //}

        private Func<TEntity, object> DynamicSelect<TEntity, TField>(PropertyInfo prop) where TEntity : class, new()
        {
            var parameterExpression = Expression.Parameter(typeof(TEntity), "x");
            var memberExpression = Expression.PropertyOrField(parameterExpression, prop.Name);
            var memberExpressionConversion = Expression.Convert(memberExpression, typeof(object));
            var lambda = Expression.Lambda<Func<TEntity, object>>(memberExpressionConversion, parameterExpression).Compile();
            return lambda;
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
            if (CheckConnection(DbConnection, DbCommand, testString))
            {
                DbTransaction = DbConnection.BeginTransaction(isolationLevel);
            }
            return DbTransaction;
        }
        private void CloseConnection()
        {
            if (DbTransaction == null)
                DbConnection.Close();
        }
        protected override void Disposing()
        {
            if (DbTransaction != null)
            {
                DbRollBack();
            }
        }

    }
}
