using Ancestor.Core;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Ancestor.DataAccess.DBAction;
using System.Dynamic;
using Ancestor.DataAccess.Interface;

namespace Ancestor.DataAccess.DAO
{
    // 2015-10-22 1. GetDbType() 加入 SYSTEM.DATETIME 型態的轉換
    // 2016-02-08 1. Add feature for Dispose. 2. 修改取得DbType的方法, 改為 Dictionary 方式取得
    // 2016-04-05 Add feature SaveChange and CancellChange for transaction.
    // 2017-07-13 修正為繼承BasweAbstractDao
    public class ManagedOracleDao : BaseAbstractDao
    {
        Dictionary<string, OracleDbType> _OracleDbTypeDic;
        public ManagedOracleDao()
        {
            //base.SqlString = new StringBuilder();
        }

        public ManagedOracleDao(DBObject _dBObject)
            : this()
        {
            base.DbObject = _dBObject;
            //base.DB = GetActionFactory();
            base.DbSymbolize = ":";
            base.DbLikeSymbolize = "||";
        }
        internal override object GetDbType(string typeString)
        {
            //switch (typeString.ToUpper())
            //{
            //    case "VARCHAR2": return OracleDbType.Varchar2;
            //    case "STRING": return OracleDbType.Varchar2;
            //    case "SYSTEM.DATETIME":     // 2015-10-22 加入 SYSTEM.DATETIME 型態的轉換
            //    case "DATETIME": return OracleDbType.Date;
            //    case "DATE": return OracleDbType.Date;
            //    case "INT64": return OracleDbType.Int64;
            //    case "INT32": return OracleDbType.Int32;
            //    case "INT16": return OracleDbType.Int16;
            //    case "BYTE": return OracleDbType.Byte;
            //    case "DECIMAL": return OracleDbType.Decimal;
            //    case "FLOAT": return OracleDbType.Single;
            //    case "DOUBLE": return OracleDbType.Double;
            //    case "BYTE[]": return OracleDbType.Blob;
            //    case "CHAR": return OracleDbType.Char;
            //    case "CHAR[]": return OracleDbType.Char;
            //    case "TIMESTAMP": return OracleDbType.TimeStamp;
            //    case "REFCURSOR": return OracleDbType.RefCursor;
            //    default: return OracleDbType.Varchar2;
            //}
            OracleDbType returnType = OracleDbType.Varchar2;
            _OracleDbTypeDic = SetOracleDbTypeList();
            if (!_OracleDbTypeDic.TryGetValue(typeString.ToUpper(), out returnType))
            {
                returnType = OracleDbType.Varchar2;
            }
            return returnType;
        }
        private const string UpdateParameterPrefix = "NEW_";
        private Dictionary<string, OracleDbType> SetOracleDbTypeList()
        {
            if (_OracleDbTypeDic == null)
            {
                _OracleDbTypeDic = new Dictionary<string, OracleDbType>
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
            }
            return _OracleDbTypeDic;
        }



        private string GenerateSelectString(object select_obj, bool withRowId = true)
        {
            var SqlStr = new StringBuilder();
            if (withRowId)
                SqlStr.Append(" ROWID,");
            foreach (PropertyInfo prop in select_obj.GetType().GetProperties())
            {
                if (CheckBrowsable(select_obj, prop.Name))
                {
                    var hardWord = prop.GetCustomAttributes(typeof(HardWordAttribute), false).FirstOrDefault();
                    var FindHardWord = hardWord != null;
                    //遇到HardWord要用rawtohex轉成byte傳出
                    if (FindHardWord)
                        SqlStr.Append(" RAWTOHEX(" + prop.Name + ") " + prop.Name + " ,");
                    else
                        SqlStr.Append(" " + prop.Name + ",");
                }
            }
            SqlStr.Remove(SqlStr.Length - 1, 1);
            return SqlStr.ToString();
        }

        protected override AncestorResult Query<T>(IModel objectModel)
        {
            var SqlString = new StringBuilder();
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<OracleParameter>();
            var dataTable = new DataTable();

            try
            {
                SqlString.Clear();
                // 2015-08-31
                //sqlString = QueryStringGenerator(objectModel, parameters);
                var tableName = new T().GetType().Name;
                SqlString.Append("SELECT " + GenerateSelectString(objectModel) + " FROM " + tableName);
                var sqlWhereCondition = ParseWhereCondition(objectModel, parameters);
                SqlString.Append(sqlWhereCondition);

                isSuccess = DB.Query(SqlString.ToString(), parameters, ref dataTable);
                returnResult.Message = DB.ErrorMessage;
                returnResult.DataList = dataTable.ToList<T>();
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }

            returnResult.IsSuccess = isSuccess;

            return returnResult;
        }
        protected override AncestorResult QueryNoRowid<T>(IModel objectModel)
        {
            var SqlString = new StringBuilder();
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<OracleParameter>();
            var dataTable = new DataTable();

            try
            {
                SqlString.Clear();
                // 2015-08-31
                //sqlString = QueryStringGenerator(objectModel, parameters);
                var tableName = new T().GetType().Name;
                SqlString.Append("SELECT " + GenerateSelectString(objectModel, false) + " FROM " + tableName);
                var sqlWhereCondition = ParseWhereCondition(objectModel, parameters);
                SqlString.Append(sqlWhereCondition);

                isSuccess = DB.Query(SqlString.ToString(), parameters, ref dataTable);
                returnResult.Message = DB.ErrorMessage;
                returnResult.DataList = dataTable.ToList<T>();
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }
            returnResult.IsSuccess = isSuccess;

            return returnResult;
        }
        protected override AncestorResult Query<T>(Expression<Func<T, bool>> predicate)
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<OracleParameter>();
            var dataTable = new DataTable();
            var dataList = new List<T>();
            var SqlString = new StringBuilder();
            using (LambdaExpressionHelper helper = new LambdaExpressionHelper(DbSymbolize, DbLikeSymbolize))
            {

                try
                {
                    var rootExp = predicate.Body as Expression;
                    whereString = helper.Translate(rootExp);
                    var Parameters = helper.Parameters;
                    var tableName = new T().GetType().Name;

                    SqlString.Append("SELECT " + GenerateSelectString(new T()) + " FROM " + tableName);
                    SqlString.Append(whereString);

                    var paras = from parameter in Parameters
                                select new OracleParameter(parameter.Name, (OracleDbType)GetDbType(parameter.Type),
                              parameter.Value, ParameterDirection.Input);
                    parameters.AddRange(paras);

                    var eo = new ExpandoObject();
                    var eoColl = (ICollection<KeyValuePair<string, object>>)eo;
                    foreach (var item in Parameters.ToDictionary(x => x.Name, x => x.Value))
                    {
                        eoColl.Add(item);
                    }
                    dynamic eoDynamic = eo;

                    isSuccess = DB.Query<T>(SqlString.ToString(), eoDynamic, ref dataList);
                    returnResult.Message = DB.ErrorMessage;
                    returnResult.DataList = dataList;
                }
                catch (Exception exception)
                {
                    returnResult.Message = exception.ToString();
                    isSuccess = false;
                }
            }
            returnResult.IsSuccess = isSuccess;

            return returnResult;
        }

        protected override AncestorResult QueryNoRowid<T>(Expression<Func<T, bool>> predicate)
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<OracleParameter>();
            var dataTable = new DataTable();
            var SqlString = new StringBuilder();

            using (LambdaExpressionHelper helper = new LambdaExpressionHelper(DbSymbolize, DbLikeSymbolize))
            {

                try
                {
                    var rootExp = predicate.Body as Expression;
                    whereString = helper.Translate(rootExp);
                    var Parameters = helper.Parameters;
                    var tableName = new T().GetType().Name;

                    SqlString.Append("SELECT " + GenerateSelectString(new T(), false) + " FROM " + tableName);
                    SqlString.Append(whereString);

                    var paras = from parameter in Parameters
                                select new OracleParameter(parameter.Name, (OracleDbType)GetDbType(parameter.Type),
                              parameter.Value, ParameterDirection.Input);
                    parameters.AddRange(paras);

                    isSuccess = DB.Query(SqlString.ToString(), parameters, ref dataTable);
                    returnResult.Message = DB.ErrorMessage;
                    returnResult.DataList = dataTable.ToList<T>();
                }
                catch (Exception exception)
                {
                    returnResult.Message = exception.ToString();
                    isSuccess = false;
                }
            }
            returnResult.IsSuccess = isSuccess;

            return returnResult;
        }

        protected override AncestorResult Query(IModel objectModel)
        {
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<OracleParameter>();
            var dataTable = new DataTable();
            var SqlString = new StringBuilder();

            try
            {
                // 2015-08-31
                //sqlString = QueryStringGenerator(objectModel, parameters);
                SqlString.Clear();
                var tableName = objectModel.GetType().Name;
                SqlString.Append("SELECT " + GenerateSelectString(objectModel) + " FROM " + tableName);
                var sqlWhereCondition = ParseWhereCondition(objectModel, parameters);
                SqlString.Append(sqlWhereCondition);
                sqlString = SqlString.ToString();

                isSuccess = DB.Query(SqlString.ToString(), parameters, ref dataTable);
                returnResult.Message = DB.ErrorMessage;
                returnResult.ReturnDataTable = dataTable;
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }
            returnResult.IsSuccess = isSuccess;

            return returnResult;
        }

        protected override AncestorResult QueryNoRowid(IModel objectModel)
        {
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<OracleParameter>();
            var dataTable = new DataTable();
            var SqlString = new StringBuilder();

            try
            {
                // 2015-08-31
                //sqlString = QueryStringGenerator(objectModel, parameters);
                SqlString.Clear();
                var tableName = objectModel.GetType().Name;
                SqlString.Append("SELECT " + GenerateSelectString(objectModel, false) + " FROM " + tableName);
                var sqlWhereCondition = ParseWhereCondition(objectModel, parameters);
                SqlString.Append(sqlWhereCondition);
                sqlString = SqlString.ToString();

                isSuccess = DB.Query(SqlString.ToString(), parameters, ref dataTable);
                returnResult.Message = DB.ErrorMessage;
                returnResult.ReturnDataTable = dataTable;
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }
            returnResult.IsSuccess = isSuccess;

            return returnResult;
        }

        /// <summary>
        /// 20160629 Line:339 WizTonE : 新增塞值判斷, 值為NULL不塞入parameters
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="paramsObjects"></param>
        /// <returns></returns>
        protected override AncestorResult Query(string sqlString, object paramsObjects)
        {
            var isSuccess = false;
            var returnResult = new AncestorResult();
            var parameters = new List<OracleParameter>();
            var dataTable = new DataTable();

            try
            {
                //foreach (var prop in paramsObjects.GetType().GetProperties())
                //{
                //    var propertyType = prop.PropertyType;
                //    parameters.Add(
                //            new OracleParameter(":" + prop.Name, (OracleDbType)GetDbType(propertyType.Name), prop.GetValue(paramsObjects, null), ParameterDirection.Input)
                //            );
                //}
                //2015-10-12 null 的參數
                if (paramsObjects != null)
                {
                    IEnumerable<OracleParameter> paras = null;

                    //2017-09-22 追加IDictionary<string, ?>的支援
                    var type = paramsObjects.GetType();
                    if (paramsObjects is System.Collections.IDictionary && type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
                        paras = from dynamic kv in (paramsObjects as System.Collections.IDictionary)
                                select new OracleParameter(DbSymbolize + kv.Key,
                                                           (OracleDbType)GetDbType(kv.Value == null ? null : kv.Value.GetType()),
                                                           kv.Value, ParameterDirection.Input);
                    else
                        paras = from prop in paramsObjects.GetType().GetProperties()
                                select
                                    new OracleParameter(DbSymbolize + prop.Name, (OracleDbType)GetDbType(prop.PropertyType),
                                        prop.GetValue(paramsObjects, null), ParameterDirection.Input);


                    //Todo
                    if (paras.Count() > 0)
                        parameters.AddRange(paras);
                }

                isSuccess = DB.Query(sqlString, parameters, ref dataTable);
                returnResult.Message = DB.ErrorMessage;
                returnResult.ReturnDataTable = dataTable;
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }

            returnResult.IsSuccess = isSuccess;
            return returnResult;
        }

        protected override AncestorResult Insert(IModel objectModel)
        {
            var SqlString = new StringBuilder();
            var sqlValueString = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<OracleParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;

            SqlString.Append("INSERT INTO " + objectModel.GetType().Name + " (");
            foreach (PropertyInfo prop in objectModel.GetType().GetProperties())
            {
                var value = prop.GetValue(objectModel, null);
                if (value != null)
                {
                    if (CheckBrowsable(objectModel, prop.Name))
                    {
                        SqlString.Append(prop.Name.ToUpper() + ",");

                        var propertyType = prop.PropertyType;
                        if (prop.PropertyType.IsGenericType &&
                                prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            propertyType = prop.PropertyType.GetGenericArguments()[0];

                        //修正: Insert HardWord
                        //sqlValueString.Append(DbSymbolize + prop.Name.ToUpper() + ",");
                        //更新使用難字
                        var hardWord = prop.GetCustomAttributes(typeof(HardWordAttribute), false).FirstOrDefault() as HardWordAttribute;
                        var findHardWord = hardWord != null;
                        var oracleType = (OracleDbType)GetDbType(propertyType);
                        OracleParameter oracleParameter = null;

                        //如果有找到難字註記
                        if (findHardWord && oracleType == OracleDbType.Varchar2)
                        {
                            oracleType = OracleDbType.Raw;
                            var str = value.ToString();
                            sqlValueString.Append(string.Format("UTL_RAW.CAST_TO_VARCHAR2({0}),",
                                   DbSymbolize + prop.Name.ToUpper()));
                            oracleParameter = new OracleParameter(DbSymbolize + prop.Name.ToUpper(), oracleType, hardWord.Encoding.GetBytes(str), ParameterDirection.Input);
                        }
                        //若沒有找到
                        else
                        {
                            sqlValueString.Append(DbSymbolize + prop.Name.ToUpper() + ",");
                            oracleParameter = new OracleParameter(DbSymbolize + prop.Name.ToUpper(), oracleType, value, ParameterDirection.Input);
                        }
                        parameters.Add(oracleParameter);



                    }
                }
            }
            SqlString.Remove(SqlString.Length - 1, 1);
            sqlValueString.Remove(sqlValueString.Length - 1, 1);
            SqlString.Append(") ");
            SqlString.Append("values ");
            SqlString.Append("(");
            SqlString.Append(sqlValueString);
            SqlString.Append(")");
            try
            {
                isSuccess = DB.ExecuteNonQuery(SqlString.ToString(), parameters, ref effectRows);
                returnResult.EffectRows = effectRows;
                returnResult.Message = DB.ErrorMessage;
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }

            returnResult.IsSuccess = isSuccess;
            return returnResult;
        }

        protected override AncestorResult Update(IModel valueObject, object paramsObjects)
        {
            var SqlString = new StringBuilder();
            var sb2 = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<OracleParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;
            var tableName = valueObject.GetType().Name;

            try
            {
                SqlString.Append("UPDATE " + tableName + " set ");
                SqlString.Append(UpdateTranslate(valueObject, parameters, UpdateMode.Original));

                var sqlWhereCondition = ParseWhereCondition(paramsObjects, parameters);
                SqlString.Append(sqlWhereCondition);

                if (string.IsNullOrEmpty(sqlWhereCondition))
                    throw new ArgumentNullException("All properties of model aren't allowed to be null for updating columns.");

                isSuccess = DB.ExecuteNonQuery(SqlString.ToString(), parameters, ref effectRows);
                returnResult.EffectRows = effectRows;
                returnResult.Message = DB.ErrorMessage;
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }

            returnResult.IsSuccess = isSuccess;
            return returnResult;
        }

        protected override AncestorResult Update(IModel valueObject, IModel whereObject)
        {
            var SqlString = new StringBuilder();
            var sb2 = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<OracleParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;
            var tableName = valueObject.GetType().Name;

            try
            {
                SqlString.Append("UPDATE " + tableName + " set ");
                // 2015-09-03 update set 欄位語法, 重構為 UpdateTranslate method.
                SqlString.Append(UpdateTranslate(valueObject, parameters, UpdateMode.Original));

                //foreach (PropertyInfo prop in valueObject.GetType().GetProperties())
                //{
                //    if (prop.GetValue(valueObject, null) != null)
                //    {
                //        if (CheckBrowsable(valueObject, prop.Name))
                //        {
                //            //SqlStringBuilder.Append(prop.Name.ToUpper() + " = :" + prop.Name.ToUpper() + ",");
                //            SqlString.Append(prop.Name.ToUpper() + " = " + DbSymbolize + prop.Name.ToUpper() + ",");

                //            var propertyType = prop.PropertyType;

                //            if (prop.PropertyType.IsGenericType &&
                //                    prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                //                propertyType = prop.PropertyType.GetGenericArguments()[0];

                //            //如果obj value非null但長度為0, 代表需為NULL, 以DBnull.Value傳值
                //            parameters.Add(new OracleParameter(DbSymbolize + prop.Name.ToUpper(), (OracleDbType)GetDbType(propertyType.Name), prop.GetValue(valueObject, null).ToString().Length > 0 ? prop.GetValue(valueObject, null) : DBNull.Value, ParameterDirection.Input));
                //        }

                //    }
                //}

                // 2015-08-31
                //if (whereObject != null)
                //{
                //    sb2.Append(" WHERE ");
                //    foreach (PropertyInfo prop in whereObject.GetType().GetProperties())
                //    {
                //        var propertyType = prop.PropertyType;
                //        if (prop.PropertyType.IsGenericType &&
                //                prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                //            propertyType = prop.PropertyType.GetGenericArguments()[0];

                //        if (prop.GetValue(whereObject, null) != null)
                //        {
                //            if (prop.Name.ToUpper() == "ROWID")
                //            {
                //                sb2.Append(prop.Name.ToUpper() + " = :" + prop.Name.ToUpper() + "1" + " and ");
                //                parameters.Add(new OracleParameter(":" + prop.Name.ToUpper() + "1", (OracleDbType)GetDbType(propertyType.Name), prop.GetValue(whereObject, null).ToString().Length > 0 ? prop.GetValue(whereObject, null) : DBNull.Value, ParameterDirection.Input));
                //            }
                //            else
                //            {
                //                sb2.Append(prop.Name.ToUpper() + " = :" + prop.Name.ToUpper() + " and ");
                //                parameters.Add(new OracleParameter(":" + prop.Name.ToUpper(), (OracleDbType)GetDbType(propertyType.Name), prop.GetValue(whereObject, null).ToString().Length > 0 ? prop.GetValue(whereObject, null) : DBNull.Value, ParameterDirection.Input));
                //            }
                //        }
                //    }
                //}

                //SqlString.Remove(SqlString.Length - 1, 1);
                //sb2.Remove(sb2.Length - 4, 4);
                //SqlStringBuilder.Append(sb2);
                var sqlWhereCondition = ParseWhereCondition(whereObject, parameters);
                SqlString.Append(sqlWhereCondition);

                if (string.IsNullOrEmpty(sqlWhereCondition))
                    throw new ArgumentNullException("All properties of model aren't allowed to be null for updating columns.");

                isSuccess = DB.ExecuteNonQuery(SqlString.ToString(), parameters, ref effectRows);
                returnResult.EffectRows = effectRows;
                returnResult.Message = DB.ErrorMessage;
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }

            returnResult.IsSuccess = isSuccess;
            return returnResult;

        }

        protected override AncestorResult Update<T>(IModel valueObject, Expression<Func<T, bool>> predicate)
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<OracleParameter>();
            var effectRows = 0;
            var tableName = valueObject.GetType().Name;
            var SqlString = new StringBuilder();

            SqlString.Append("UPDATE " + tableName + " set ");
            // 2015-09-03 update set 欄位語法, 重構為 UpdateTranslate method.
            SqlString.Append(UpdateTranslate(valueObject, parameters, UpdateMode.Original));

            using (LambdaExpressionHelper helper = new LambdaExpressionHelper(DbSymbolize, DbLikeSymbolize))
            {
                try
                {
                    var rootExp = predicate.Body as Expression;
                    whereString = helper.Translate(rootExp);
                    var expParameters = helper.Parameters;

                    sqlString += SqlString.ToString();
                    sqlString += whereString;

                    var paras = from p in expParameters
                                select new OracleParameter(p.Name, (OracleDbType)GetDbType(p.Type),
                              p.Value, ParameterDirection.Input);
                    parameters.AddRange(paras);

                    isSuccess = DB.ExecuteNonQuery(sqlString, parameters, ref effectRows);
                    returnResult.Message = DB.ErrorMessage;
                    returnResult.EffectRows = effectRows;
                }
                catch (Exception exception)
                {
                    returnResult.Message = exception.ToString();
                    isSuccess = false;
                }
            }
            returnResult.IsSuccess = isSuccess;

            return returnResult;
        }
        protected override AncestorResult Delete(IModel whereObject)
        {
            var SqlString = new StringBuilder();
            //StringBuilder sb2 = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<OracleParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;

            try
            {
                // 2015-08-31
                // SqlStringBuilder.Append("DELETE FROM " + whereObject.GetType().Name);

                //if (whereObject != null)
                //{
                //    sb2.Append(" WHERE ");
                //    foreach (PropertyInfo prop in whereObject.GetType().GetProperties())
                //    {
                //        if (prop.GetValue(whereObject, null) != null)
                //        {
                //            if (prop.Name.ToUpper() == "ROWID")
                //                sb2.Append(prop.Name.ToUpper() + " = :" + prop.Name.ToUpper() + "1" + " and ");
                //            else
                //                sb2.Append(prop.Name.ToUpper() + " = :" + prop.Name.ToUpper() + " and ");
                //            //檢查Nullable
                //            //若為Nullable,則型態設為prop.PropertyType.GetGenericArguments()[0]
                //            //否則仍為prop.PropertyType
                //            var propertyType = prop.PropertyType;
                //            if (prop.PropertyType.IsGenericType &&
                //                    prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                //                propertyType = prop.PropertyType.GetGenericArguments()[0];

                //            if (prop.Name.ToUpper() == "ROWID")
                //                parameters.Add(new OracleParameter(":" + prop.Name.ToUpper() + "1", (OracleDbType)GetDbType(propertyType.Name), prop.GetValue(whereObject, null).ToString().Length > 0 ? prop.GetValue(whereObject, null) : DBNull.Value, ParameterDirection.Input));
                //            else
                //                parameters.Add(new OracleParameter(":" + prop.Name.ToUpper(), (OracleDbType)GetDbType(propertyType.Name), prop.GetValue(whereObject, null).ToString().Length > 0 ? prop.GetValue(whereObject, null) : DBNull.Value, ParameterDirection.Input));
                //        }
                //    }
                //}

                //sb2.Remove(sb2.Length - 4, 4);
                //SqlStringBuilder.Append(sb2);

                var tableName = whereObject.GetType().Name;
                SqlString.Append("DELETE FROM " + tableName);
                var sqlWhereCondition = ParseWhereCondition(whereObject, parameters);

                if (string.IsNullOrEmpty(sqlWhereCondition))
                    throw new ArgumentNullException("All properties of model aren't allowed to be null for deleting columns.");

                SqlString.Append(sqlWhereCondition);

                isSuccess = DB.ExecuteNonQuery(SqlString.ToString(), parameters, ref effectRows);
                returnResult.EffectRows = effectRows;
                returnResult.Message = DB.ErrorMessage;
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }

            returnResult.IsSuccess = isSuccess;
            return returnResult;
        }
        protected override AncestorResult Delete<T>(Expression<Func<T, bool>> predicate)
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<OracleParameter>();
            var dataTable = new DataTable();
            var effectRows = 0;

            using (LambdaExpressionHelper helper = new LambdaExpressionHelper(DbSymbolize, DbLikeSymbolize))
            {
                try
                {
                    var rootExp = predicate.Body as Expression;
                    whereString = helper.Translate(rootExp);
                    var Parameters = helper.Parameters;

                    var tableName = new T().GetType().Name;
                    sqlString = "DELETE FROM " + tableName;
                    // 2015-09-03
                    sqlString += whereString;

                    var paras = from parameter in Parameters
                                select new OracleParameter(parameter.Name, (OracleDbType)GetDbType(parameter.Type),
                              parameter.Value, ParameterDirection.Input);
                    parameters.AddRange(paras);

                    isSuccess = DB.ExecuteNonQuery(sqlString, parameters, ref effectRows);
                    returnResult.Message = DB.ErrorMessage;
                    returnResult.EffectRows = effectRows;
                }
                catch (Exception exception)
                {
                    returnResult.Message = exception.ToString();
                    isSuccess = false;
                }
            }
            returnResult.IsSuccess = isSuccess;
            return returnResult;
        }
        protected override AncestorResult ExecuteNonQuery(string sqlString, object modelObject)
        {
            var SqlString = new StringBuilder();
            SqlString.Append(sqlString);
            var effectRows = 0;
            var parameters = new List<OracleParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;

            if (modelObject != null)
            {
                foreach (PropertyInfo prop in modelObject.GetType().GetProperties())
                {
                    if (prop.GetValue(modelObject, null) != null)
                    {
                        //檢查Nullable
                        //若為Nullable,則型態設為prop.PropertyType.GetGenericArguments()[0]
                        //否則仍為prop.PropertyType
                        var propertyType = prop.PropertyType;
                        if (prop.PropertyType.IsGenericType &&
                                prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            propertyType = prop.PropertyType.GetGenericArguments()[0];
                        if (prop.Name.ToUpper() == "ROWID")
                            parameters.Add(new OracleParameter(DbSymbolize + prop.Name.ToUpper() + "1", (OracleDbType)GetDbType(propertyType), prop.GetValue(modelObject, null).ToString().Length > 0 ? prop.GetValue(modelObject, null) : DBNull.Value, ParameterDirection.Input));
                        else
                            parameters.Add(new OracleParameter(DbSymbolize + prop.Name.ToUpper(), (OracleDbType)GetDbType(propertyType), prop.GetValue(modelObject, null).ToString().Length > 0 ? prop.GetValue(modelObject, null) : DBNull.Value, ParameterDirection.Input));
                    }
                }
            }
            if (SqlString.ToString().IndexOf(":ROWID") > 0)
                SqlString = SqlString.Replace(":ROWID", ":ROWID1");

            try
            {
                isSuccess = DB.ExecuteNonQuery(SqlString.ToString(), parameters, ref effectRows);
                returnResult.EffectRows = effectRows;
                returnResult.Message = DB.ErrorMessage;
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }

            returnResult.IsSuccess = isSuccess;
            return returnResult;
        }
        protected override AncestorResult ExecuteStoredProcedure(string procedureName, bool bindbyName, List<DBParameter> dBParameter)
        {
            var parameters = new List<OracleParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;

            try
            {
                foreach (DBParameter Parameter in dBParameter)
                {
                    if (Parameter.ParameterDirection == ParameterDirection.Input)
                    {
                        parameters.Add(new OracleParameter()
                        {
                            ParameterName = Parameter.Name,
                            OracleDbType = (OracleDbType)GetDbType(Parameter.Type),
                            Value = Parameter.Value?.ToString() != null ? Parameter.Value : DBNull.Value,
                            Direction = ParameterDirection.Input,
                            Size = Parameter.Size
                        });
                    }
                    if (Parameter.ParameterDirection == ParameterDirection.Output)
                    {
                        parameters.Add(new OracleParameter()
                        {
                            ParameterName = Parameter.Name,
                            OracleDbType = (OracleDbType)GetDbType(Parameter.Type),
                            Direction = ParameterDirection.Output,
                            Size = Parameter.Size
                        });
                    }
                    if (Parameter.ParameterDirection == ParameterDirection.InputOutput)
                    {
                        parameters.Add(new OracleParameter()
                        {
                            ParameterName = Parameter.Name,
                            OracleDbType = (OracleDbType)GetDbType(Parameter.Type),
                            Value = Parameter.Value?.ToString() != null ? Parameter.Value : DBNull.Value,
                            Direction = ParameterDirection.InputOutput,
                            Size = Parameter.Size
                        });
                    }
                    if (Parameter.ParameterDirection == ParameterDirection.ReturnValue)
                    {
                        parameters.Add(new OracleParameter()
                        {
                            ParameterName = Parameter.Name,
                            OracleDbType = (OracleDbType)GetDbType(Parameter.Type),
                            Direction = ParameterDirection.ReturnValue,
                            Size = Parameter.Size
                        });
                    }
                }
                isSuccess = DB.ExecuteStoredProcedure(procedureName, bindbyName, parameters, dBParameter);
                returnResult.Message = DB.ErrorMessage;
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }
            returnResult.IsSuccess = isSuccess;
            return returnResult;
        }
        private string UpdateTranslate(IModel valueObject, List<OracleParameter> parameters, UpdateMode mode)
        {
            var SqlString = new StringBuilder();
            foreach (PropertyInfo prop in valueObject.GetType().GetProperties())
            {
                if (mode == UpdateMode.All)
                {
                    UpdateAllTranslate(valueObject, parameters, SqlString, prop);
                }
                else
                {
                    if (prop.GetValue(valueObject, null) != null)
                    {
                        UpdateAllTranslate(valueObject, parameters, SqlString, prop);
                    }
                }
            }
            if (SqlString.Length > 0)
                SqlString.Remove(SqlString.Length - 1, 1);
            return SqlString.ToString();
        }

        private void UpdateAllTranslate(IModel valueObject, List<OracleParameter> parameters, StringBuilder SqlString, PropertyInfo prop)
        {
            if (CheckBrowsable(valueObject, prop.Name) && prop.Name != "ROWID")
            {
                //SqlStringBuilder.Append(prop.Name.ToUpper() + " = :" + prop.Name.ToUpper() + ",");
                //SqlString.Append(prop.Name.ToUpper() + " = " + DbSymbolize + prop.Name.ToUpper() + ",");

                var propertyType = prop.PropertyType;

                if (prop.PropertyType.IsGenericType &&
                        prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    propertyType = prop.PropertyType.GetGenericArguments()[0];

                //如果obj value非null但長度為0, 代表需為NULL, 以DBnull.Value傳值
                //parameters.Add(new OracleParameter(DbSymbolize + prop.Name.ToUpper(), (OracleDbType)GetDbType(propertyType.Name), prop.GetValue(valueObject, null)?.ToString() != null ? prop.GetValue(valueObject, null) : DBNull.Value, ParameterDirection.Input));

                //更新使用難字
                var hardWord = prop.GetCustomAttributes(typeof(HardWordAttribute), false).FirstOrDefault() as HardWordAttribute;
                var findHardWord = hardWord != null;
                var oracleType = (OracleDbType)GetDbType(propertyType);
                OracleParameter oracleParameter = null;
                var value = prop.GetValue(valueObject, null);
                //如果有找到難字註記
                if (findHardWord && oracleType == OracleDbType.Varchar2)
                {
                    if (value != null)    //有值，要使用UTL_RAW.CAST_TO_VARCHAR2轉換
                    {
                        oracleType = OracleDbType.Raw;
                        var str = value.ToString();
                        SqlString.Append(string.Format("{1} = UTL_RAW.CAST_TO_VARCHAR2({0}),",
                               DbSymbolize + UpdateParameterPrefix + prop.Name.ToUpper(),
                               prop.Name.ToUpper()));
                        oracleParameter = new OracleParameter(DbSymbolize + UpdateParameterPrefix + prop.Name.ToUpper(), oracleType, hardWord.Encoding.GetBytes(str), ParameterDirection.Input);
                    }
                    else
                    {
                        SqlString.Append(prop.Name.ToUpper() + " = " + DbSymbolize + UpdateParameterPrefix + prop.Name.ToUpper() + ",");
                        oracleParameter = new OracleParameter(DbSymbolize + UpdateParameterPrefix + prop.Name.ToUpper(), oracleType, DBNull.Value, ParameterDirection.Input);
                    }

                }
                //若沒有找到
                else
                {
                    SqlString.Append(prop.Name.ToUpper() + " = " + DbSymbolize + UpdateParameterPrefix + prop.Name.ToUpper() + ",");
                    oracleParameter = new OracleParameter(DbSymbolize + UpdateParameterPrefix + prop.Name.ToUpper(), oracleType, value, ParameterDirection.Input);
                }
                parameters.Add(oracleParameter);
            }
        }


        private string QueryStringGenerator(IModel objectModel, ICollection<OracleParameter> parameters)
        {
            var SqlString = new StringBuilder();
            SqlString.Append("Select " + objectModel.GetType().Name + ".*, ROWID from " + objectModel.GetType().Name);

            if (objectModel != null)
            {
                SqlString.Append(" Where ");
                foreach (var prop in objectModel.GetType().GetProperties())
                {
                    var propertyType = prop.PropertyType;
                    var parameterName = prop.Name.ToUpper();

                    if (prop.PropertyType.IsGenericType &&
                                    prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        propertyType = prop.PropertyType.GetGenericArguments()[0];

                    if (parameterName == "ROWID")
                    {
                        parameterName = parameterName + "1";
                        SqlString.Append(" ROWID = :" + parameterName);
                    }
                    else
                    {
                        SqlString.Append(parameterName + " = :" + parameterName);
                    }
                    parameters.Add(
                            new OracleParameter(DbSymbolize + parameterName, (OracleDbType)GetDbType(propertyType), prop.GetValue(objectModel, null), ParameterDirection.Input)
                            );
                    SqlString.Append(" and ");
                }
                SqlString.Remove(SqlString.Length - 5, 5);
            }

            return SqlString.ToString();
        }
        private string ParseWhereCondition(object objectModel, ICollection<OracleParameter> parameters)
        {
            StringBuilder sqlConditionWhere = new StringBuilder();

            if (objectModel != null)
            {
                sqlConditionWhere.Append(" WHERE ");
                foreach (var prop in objectModel.GetType().GetProperties())
                {
                    if (Object.Equals(prop.GetValue(objectModel, null), null))
                        continue;

                    var propertyType = prop.PropertyType;
                    var parameterName = prop.Name.ToUpper();

                    if (prop.PropertyType.IsGenericType &&
                                    prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        propertyType = prop.PropertyType.GetGenericArguments()[0];

                    if (parameterName == "ROWID")
                    {
                        parameterName = parameterName + "1";
                        sqlConditionWhere.Append(" ROWID = " + DbSymbolize + parameterName);
                    }
                    else
                    {
                        sqlConditionWhere.Append(parameterName + " = " + DbSymbolize + parameterName);
                    }
                    parameters.Add(
                            new OracleParameter(DbSymbolize + parameterName, (OracleDbType)GetDbType(propertyType), prop.GetValue(objectModel, null), ParameterDirection.Input)
                            );
                    sqlConditionWhere.Append(" AND ");
                }
                if (parameters.Count > 0)
                    sqlConditionWhere.Remove(sqlConditionWhere.Length - 5, 5);
                else
                    sqlConditionWhere.Remove(sqlConditionWhere.Length - 7, 7);
            }

            return sqlConditionWhere.ToString();
        }
        /// <summary>
        /// 檢查物件內的[Browsable]屬性是true 或 false
        /// true代表可以存在於欄位, 可讓程式自動帶入SQL中
        /// false代表搜尋用欄位, 可自動略過不帶入SQL中
        /// </summary>
        /// <param name="model"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private bool CheckBrowsable(object model, string columnName)
        {
            AttributeCollection attributes =
                TypeDescriptor.GetProperties(model)[columnName].Attributes;
            BrowsableAttribute myAttribute =
                (BrowsableAttribute)attributes[typeof(BrowsableAttribute)];

            return myAttribute.Browsable;
        }

        protected override AncestorResult BulkInsert<T>(List<T> objList)
        {
            var SqlString = new StringBuilder();
            //var sqlValueString = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<OracleParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;
            var tableName = new T().GetType().Name;

            try
            {
                isSuccess = DB.BulkInsert(objList, ref effectRows);
                returnResult.EffectRows = effectRows;
                returnResult.Message = DB.ErrorMessage;
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }

            returnResult.IsSuccess = isSuccess;
            return returnResult;
        }

        protected override AncestorResult QueryWithJoinCondition(Expression predicate, Expression selectCondition, Type[] types, Type[] fakeTypes = null)
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<OracleParameter>();
            var dataTable = new DataTable();
            var SqlString = new StringBuilder();
            Dictionary<Type, object> mapping = null;
            if (fakeTypes != null && fakeTypes.Length == types.Length)
            {
                mapping = new Dictionary<Type, object>();
                for (int i = 0; i < types.Length; i++)
                    if (!string.Equals(fakeTypes[i].Name, types[i].Name, StringComparison.OrdinalIgnoreCase))
                        mapping.Add(fakeTypes[i], types[i]);
            }
            using (LambdaExpressionHelper helper = new LambdaExpressionHelper(DbSymbolize, DbLikeSymbolize, mapping))
            {
                try
                {
                    var rootExp = predicate;
                    whereString = helper.Translate(rootExp);
                    var Parameters = helper.Parameters;
                    var tableName = string.Join(", ", from type in types select type.Name);
                    SqlString.Append("SELECT " + helper.SelectString(selectCondition) + " FROM " + tableName);
                    SqlString.Append(whereString);

                    var paras = from parameter in Parameters
                                select new OracleParameter(parameter.Name, (OracleDbType)GetDbType(parameter.Type),
                              parameter.Value, ParameterDirection.Input);
                    parameters.AddRange(paras);

                    isSuccess = DB.Query(SqlString.ToString(), parameters, ref dataTable);
                    returnResult.Message = DB.ErrorMessage;
                    returnResult.ReturnDataTable = dataTable;
                }
                catch (Exception exception)
                {
                    returnResult.Message = exception.ToString();
                    isSuccess = false;
                }
            }
            returnResult.IsSuccess = isSuccess;

            return returnResult;
        }
        protected override AncestorResult QueryWithJoinCondition(Expression predicate, Expression selectCondition, string[] names, Type[] fakeTypes = null)
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<OracleParameter>();
            var dataTable = new DataTable();
            var SqlString = new StringBuilder();
            Dictionary<Type, object> mapping = null;
            if (fakeTypes != null && fakeTypes.Length == names.Length)
            {
                mapping = new Dictionary<Type, object>();
                for (int i = 0; i < names.Length; i++)
                    if (!string.Equals(fakeTypes[i].Name, names[i], StringComparison.OrdinalIgnoreCase))
                        mapping.Add(fakeTypes[i], names[i]);
            }
            using (LambdaExpressionHelper helper = new LambdaExpressionHelper(DbSymbolize, DbLikeSymbolize, mapping))
            {
                try
                {
                    var rootExp = predicate;
                    whereString = helper.Translate(rootExp);
                    var Parameters = helper.Parameters;
                    var tableName = string.Join(", ", from name in names select name);
                    SqlString.Append("SELECT " + helper.SelectString(selectCondition) + " FROM " + tableName);
                    SqlString.Append(whereString);

                    var paras = from parameter in Parameters
                                select new OracleParameter(parameter.Name, (OracleDbType)GetDbType(parameter.Type),
                              parameter.Value, ParameterDirection.Input);
                    parameters.AddRange(paras);

                    isSuccess = DB.Query(SqlString.ToString(), parameters, ref dataTable);
                    returnResult.Message = DB.ErrorMessage;
                    returnResult.ReturnDataTable = dataTable;
                }
                catch (Exception exception)
                {
                    returnResult.Message = exception.ToString();
                    isSuccess = false;
                }
            }
            returnResult.IsSuccess = isSuccess;

            return returnResult;
        }


        protected override AncestorResult UpdateAll(IModel valueObject, IModel whereObject)
        {
            var SqlString = new StringBuilder();
            var sb2 = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<OracleParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;
            var tableName = valueObject.GetType().Name;

            try
            {
                SqlString.Append("UPDATE " + tableName + " set ");
                SqlString.Append(UpdateTranslate(valueObject, parameters, UpdateMode.All));
                var sqlWhereCondition = ParseWhereCondition(whereObject, parameters);
                SqlString.Append(sqlWhereCondition);

                if (string.IsNullOrEmpty(sqlWhereCondition))
                    throw new ArgumentNullException("All properties of model aren't allowed to be null for updating columns.");

                isSuccess = DB.ExecuteNonQuery(SqlString.ToString(), parameters, ref effectRows);
                returnResult.EffectRows = effectRows;
                returnResult.Message = DB.ErrorMessage;
            }
            catch (Exception exception)
            {
                returnResult.Message = exception.ToString();
                isSuccess = false;
            }

            returnResult.IsSuccess = isSuccess;
            return returnResult;

        }

        protected override AncestorResult UpdateAll<T>(IModel valueObject, Expression<Func<T, bool>> predicate)
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<OracleParameter>();
            var effectRows = 0;
            var tableName = valueObject.GetType().Name;
            var SqlString = new StringBuilder();

            SqlString.Append("UPDATE " + tableName + " set ");
            // 2015-09-03 update set 欄位語法, 重構為 UpdateTranslate method.
            SqlString.Append(UpdateTranslate(valueObject, parameters, UpdateMode.All));

            using (LambdaExpressionHelper helper = new LambdaExpressionHelper(DbSymbolize, DbLikeSymbolize))
            {
                try
                {
                    var rootExp = predicate.Body as Expression;
                    whereString = helper.Translate(rootExp);
                    var expParameters = helper.Parameters;

                    sqlString += SqlString.ToString();
                    sqlString += whereString;

                    var paras = from p in expParameters
                                select new OracleParameter(p.Name, (OracleDbType)GetDbType(p.Type),
                              p.Value, ParameterDirection.Input);
                    parameters.AddRange(paras);

                    isSuccess = DB.ExecuteNonQuery(sqlString, parameters, ref effectRows);
                    returnResult.Message = DB.ErrorMessage;
                    returnResult.EffectRows = effectRows;
                }
                catch (Exception exception)
                {
                    returnResult.Message = exception.ToString();
                    isSuccess = false;
                }
            }
            returnResult.IsSuccess = isSuccess;

            return returnResult;
        }
    }
}
