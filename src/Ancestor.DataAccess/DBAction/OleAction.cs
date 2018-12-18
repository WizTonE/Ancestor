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
using System.Reflection;
using MySql.Data.MySqlClient;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Data.OleDb;

namespace Ancestor.DataAccess.DBAction
{
    /// <summary>
    /// Author  : poohlin 
    /// Date    : 2018/11/05 10:00
    /// Subject : OleAction
    /// 
    /// History : 
    /// 2018/11/05 poohlin 建立
    /// 注意 ActionFactory 要加入 Access
    /// _actions.Add((new DbActionResource(DBObject.DataBase.Access, new OleAction(dbObject))));
    /// </summary>
    public class OleAction : BaseAbstractAction
    {
        private Dictionary<string, OleDbType> _sqlDbTypeDic;
        private Dictionary<string, OleDbType> _SqlDbTypeDic
        {
            get
            {
                if (_sqlDbTypeDic == null)
                {
                    _sqlDbTypeDic = new Dictionary<string, OleDbType>();
                    _sqlDbTypeDic.Add("VARCHAR2", OleDbType.VarChar);
                    _sqlDbTypeDic.Add("SYSTEM.STRING", OleDbType.VarChar);
                    _sqlDbTypeDic.Add("STRING", OleDbType.VarChar);
                    _sqlDbTypeDic.Add("SYSTEM.DATETIME", OleDbType.Date);
                    _sqlDbTypeDic.Add("DATETIME", OleDbType.Date);
                    _sqlDbTypeDic.Add("DATE", OleDbType.Date);
                    _sqlDbTypeDic.Add("INT64", OleDbType.BigInt);
                    _sqlDbTypeDic.Add("INT32", OleDbType.Integer);
                    _sqlDbTypeDic.Add("INT16", OleDbType.SmallInt);
                    _sqlDbTypeDic.Add("BYTE", OleDbType.Binary);
                    _sqlDbTypeDic.Add("DECIMAL", OleDbType.Decimal);
                    _sqlDbTypeDic.Add("FLOAT", OleDbType.Double);
                    _sqlDbTypeDic.Add("DOUBLE", OleDbType.Decimal);
                    _sqlDbTypeDic.Add("BYTE[]", OleDbType.Binary);
                    _sqlDbTypeDic.Add("CHAR", OleDbType.Char);
                    _sqlDbTypeDic.Add("CHAR[]", OleDbType.Char);
                    _sqlDbTypeDic.Add("TIMESTAMP", OleDbType.DBTimeStamp);//??????
                    _sqlDbTypeDic.Add("REFCURSOR", OleDbType.Variant);
                }
                return _sqlDbTypeDic;
            }
        }
        OleDbTransaction DbTransaction;
        OleDbConnection DbConnection
        {
            get { return DBConnection as OleDbConnection; }
            set { DBConnection = value; }
        }
        OleDbCommand DbCommand { get; set; }

        OleDbDataAdapter adapter { get; set; }
        string testString { get; set; }

        public OleAction()
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
            DbConnection = (OleDbConnection)conn.GetConnectionFactory().GetConnectionObject();
            return DbConnection;
        }

        public OleAction(DBObject _dBObject)
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
            adapter = new OleDbDataAdapter();
            //DbCommand.BindByName = true;
            //DbCommand.AddRowid = true;

            if (CheckConnection(DbConnection, DbCommand, testString))
            {
                try
                {
                    var parameters = (List<OleDbParameter>)parameterCollection;
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
                try
                {
                    var parameters = (List<OleDbParameter>)parameterCollection;
                    DbCommand.Parameters.AddRange(parameters.ToArray());
                    DbCommand.CommandText = sqlString;

                    effectRows = DbCommand.ExecuteNonQuery();
                    isSuccessful = true;
                }
                catch (Exception exception)
                {
                    isSuccessful = false;
                    ErrorMessage = exception.ToString();
                }
            }
            CloseConnection();
            return isSuccessful;
        }

        protected override bool BulkInsert<T>(List<T> objectList, ref int effectRows)
        {
            string table_name = string.Empty;
            int loop_for = 0;
            bool isSuccessful = false;
            ErrorMessage = string.Empty;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            string fields = null, parameters = null;

            if (objectList.Count > 0)
            {
                Dictionary<PropertyInfo, object[]> dic = null;
                var connectionFlag = CheckConnection(DbConnection, DbCommand, testString);
                loop_for = (int)Math.Ceiling(Math.Round((double)objectList.Count / 30000, 10));
                for (int i = 0; i < (loop_for); i++)
                {
                    if (connectionFlag)
                    {
                        if (DbCommand == null)
                            DbCommand = DbConnection.CreateCommand();
                        List<T> TempList = objectList.GetRange(i * 30000, Math.Min(30000, objectList.Count - i * 30000));
                        dic = new Dictionary<PropertyInfo, object[]>();
                        foreach (PropertyInfo prop in TempList[0].GetType().GetProperties())
                        {
                            table_name = TempList[0].GetType().Name;
                            if (prop.Name != "ROWID")
                            {
                                var p = prop.GetCustomAttributes(false).FirstOrDefault(a => a is BrowsableAttribute);
                                if (p != null && !((BrowsableAttribute)p).Browsable)
                                    continue;


                                if (fields == null)
                                {
                                    sb.Append(prop.Name.ToUpper() + ",");
                                    sb2.Append("@" + prop.Name.ToUpper() + ",");
                                }
                                var propertyType = prop.PropertyType;
                                if (prop.PropertyType.IsGenericType &&
                                        prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    propertyType = prop.PropertyType.GetGenericArguments()[0];
                                var expression = DynamicSelect<T, dynamic>(prop);
                                var valueList = TempList.Select(expression).ToArray();
                                dic.Add(prop, valueList);
                            }
                        }
                        if (fields == null)
                        {
                            fields = sb.Remove(sb.Length - 1, 1).ToString();
                            parameters = sb2.Remove(sb2.Length - 1, 1).ToString();
                            sb.Clear();
                            sb2.Clear();
                        }
                        try
                        {
                            if (DbCommand.CommandText == "")
                                DbCommand.CommandText = "INSERT INTO " + table_name + " (" + fields + ")" + " values (" + parameters + ")";
                            for (int j = 0; j < TempList.Count; j++)
                            {
                                DbCommand.Parameters.Clear();
                                foreach (var kv in dic)
                                {
                                    DbCommand.Parameters.AddWithValue("@" + kv.Key.Name.ToUpper(), kv.Value[j] ?? DBNull.Value);
                                }
                                effectRows += DbCommand.ExecuteNonQuery();
                            }
                            isSuccessful = true;
                        }
                        catch (Exception exception)
                        {
                            isSuccessful = false;
                            ErrorMessage = exception.ToString();
                        }


                    }
                }
                CloseConnection();
            }
            return isSuccessful;
        }
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
