using Ancestor.Core;
using Ancestor.DataAccess.Interface;
using Ancestor.DataAccess.DBAction;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    /// <summary>
    /// Creator : WizTonE 
    /// Date    : 2017/04/17
    /// Subject : MySqlDao
    /// 
    /// History : 
    /// 2017/04/17 WizTonE 建立 : MySqlDao
    /// </summary>
    public class MySqlDao : BaseAbstractDao
    {
        Dictionary<string, MySqlDbType> _MySqlDbTypeDic;
        public MySqlDao()
        {
            //base.SqlString = new StringBuilder();
        }
        public MySqlDao(DBObject _dBObject)
            : this()
        {
            base.DbObject = _dBObject;
            //base.DB = GetActionFactory();
            base.DbSymbolize = "?";
            base.DbLikeSymbolize = "&";
        }
        protected override AncestorResult Delete(IModel whereObject)
        {
            var SqlString = new StringBuilder();
            //StringBuilder sb2 = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<MySqlParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;

            try
            {

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
            var parameters = new List<MySqlParameter>();
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

                    var paras = Parameters.Select(x =>
                    {
                        var parameter = new MySqlParameter(x.Name, (MySqlDbType)GetDbType(x.Type));
                        parameter.Value = x.Value;
                        parameter.Direction = ParameterDirection.Input;
                        return parameter;
                    });
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
            var parameters = new List<MySqlParameter>();
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
                        var parameter = new MySqlParameter(DbSymbolize + prop.Name.ToUpper(), (MySqlDbType)GetDbTypeFromType(propertyType));
                        parameter.Value = prop.GetValue(modelObject, null);
                        parameter.Direction = ParameterDirection.Input;
                        parameters.Add(parameter);
                    }
                }
            }

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
            var parameters = new List<MySqlParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;

            try
            {
                foreach (DBParameter Parameter in dBParameter)
                {
                    if (Parameter.ParameterDirection == ParameterDirection.Input)
                    {
                        parameters.Add(new MySqlParameter
                        {
                            ParameterName = Parameter.Name,
                            MySqlDbType = (MySqlDbType)GetDbType(Parameter.Type),
                            Value = Parameter.Value.ToString().Length > 0 ? Parameter.Value : DBNull.Value,
                            Direction = ParameterDirection.Input,
                            Size = Parameter.Size
                        });
                    }
                    if (Parameter.ParameterDirection == ParameterDirection.Output)
                    {
                        parameters.Add(new MySqlParameter
                        {
                            ParameterName = Parameter.Name,
                            MySqlDbType = (MySqlDbType)GetDbType(Parameter.Type),
                            Direction = ParameterDirection.Output,
                            Size = Parameter.Size
                        });
                    }
                    if (Parameter.ParameterDirection == ParameterDirection.InputOutput)
                    {
                        parameters.Add(new MySqlParameter()
                        {
                            ParameterName = Parameter.Name,
                            MySqlDbType = (MySqlDbType)GetDbType(Parameter.Type),
                            Value = Parameter.Value.ToString().Length > 0 ? Parameter.Value : DBNull.Value,
                            Direction = ParameterDirection.InputOutput,
                            Size = Parameter.Size
                        });
                    }
                    if (Parameter.ParameterDirection == ParameterDirection.ReturnValue)
                    {
                        parameters.Add(new MySqlParameter()
                        {
                            ParameterName = Parameter.Name,
                            MySqlDbType = (MySqlDbType)GetDbType(Parameter.Type),
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

        public IDbAction GetActionFactory()
        {
            return base.DB = ActionFactory.GetDBAction(DbObject);
        }

        protected override AncestorResult Insert(IModel objectModel)
        {
            var SqlString = new StringBuilder();
            var sqlValueString = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<MySqlParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;

            SqlString.Append("INSERT INTO " + objectModel.GetType().Name + " (");
            foreach (PropertyInfo prop in objectModel.GetType().GetProperties())
            {
                if (prop.GetValue(objectModel, null) != null)
                {
                    if (CheckBrowsable(objectModel, prop.Name))
                    {
                        SqlString.Append(prop.Name.ToUpper() + ",");
                        sqlValueString.Append(DbSymbolize + prop.Name.ToUpper() + ",");
                        var propertyType = prop.PropertyType;
                        if (prop.PropertyType.IsGenericType &&
                                prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            propertyType = prop.PropertyType.GetGenericArguments()[0];
                        var parameter = new MySqlParameter(DbSymbolize + prop.Name.ToUpper(), (MySqlDbType)GetDbTypeFromType(propertyType));
                        parameter.Value = prop.GetValue(objectModel, null);
                        parameter.Direction = ParameterDirection.Input;
                        parameters.Add(parameter);
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

        protected override AncestorResult Query(IModel objectModel)
        {
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<MySqlParameter>();
            var dataTable = new DataTable();
            var SqlString = new StringBuilder();

            try
            {
                // 2015-08-31
                //sqlString = QueryStringGenerator(objectModel, parameters);
                SqlString.Clear();
                var tableName = objectModel.GetType().Name;
                SqlString.Append("SELECT * FROM " + tableName);
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

        protected override AncestorResult Query<T>(IModel objectModel)
        {
            var SqlString = new StringBuilder();
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<MySqlParameter>();
            var dataTable = new DataTable();

            try
            {
                SqlString.Clear();
                // 2015-08-31
                //sqlString = QueryStringGenerator(objectModel, parameters);
                var tableName = objectModel.GetType().Name;
                SqlString.Append("SELECT * FROM " + tableName);
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

        protected override AncestorResult Query(string sqlString, object paramsObjects)
        {
            var isSuccess = false;
            var returnResult = new AncestorResult();
            var parameters = new List<MySqlParameter>();
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
                    IEnumerable<MySqlParameter> paras = null;



                    //var paras = from prop in paramsObjects.GetType().GetProperties()
                    //            select
                    //                new MySqlParameter(DbSymbolize + prop.Name, (MySqlDbType)GetDbType(prop.PropertyType.Name));
                    //2017-09-22 追加IDictionary<string, ?>的支援
                    var type = paramsObjects.GetType();
                    if (paramsObjects is System.Collections.IDictionary && type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
                        paras = from dynamic kv in (paramsObjects as System.Collections.IDictionary)
                                select new MySqlParameter(DbSymbolize + kv.Key, (MySqlDbType)GetDbTypeFromType(kv.Value == null ? null : kv.Value.GetType()))
                                {
                                   Value = kv.Value,
                                   Direction = ParameterDirection.Input
                                }; 
                    else
                        paras = paramsObjects.GetType().GetProperties().Select(x =>
                        {
                            var parameter = new MySqlParameter(DbSymbolize + x.Name, (MySqlDbType)GetDbTypeFromType(x.PropertyType));
                            parameter.Value = x.GetValue(paramsObjects, null);
                            parameter.Direction = ParameterDirection.Input;
                            return parameter;
                        });
                    //Todo
                    if (((MySqlParameter)paras.FirstOrDefault()).Value != null)
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

        protected override AncestorResult Query<T>(Expression<Func<T, bool>> predicate)
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<MySqlParameter>();
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

                    SqlString.Append("SELECT * FROM " + tableName);
                    SqlString.Append(whereString);

                    var paras = Parameters.Select(x =>
                    {
                        var parameter = new MySqlParameter(x.Name, (MySqlDbType)GetDbTypeFromType(x.Value.GetType()));
                        parameter.Value = x.Value;
                        parameter.Direction = ParameterDirection.Input;
                        return parameter;
                    });
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

        protected override AncestorResult QueryNoRowid(IModel objectModel)
        {
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<MySqlParameter>();
            var dataTable = new DataTable();
            var SqlString = new StringBuilder();

            try
            {
                // 2015-08-31
                //sqlString = QueryStringGenerator(objectModel, parameters);
                SqlString.Clear();
                var tableName = objectModel.GetType().Name;
                SqlString.Append("SELECT * FROM " + tableName);
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
        protected override AncestorResult QueryNoRowid<T>(IModel objectModel)
        {
            var SqlString = new StringBuilder();
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<MySqlParameter>();
            var dataTable = new DataTable();

            try
            {
                SqlString.Clear();
                // 2015-08-31
                //sqlString = QueryStringGenerator(objectModel, parameters);
                var tableName = objectModel.GetType().Name;
                SqlString.Append("SELECT * FROM " + tableName);
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

        protected override AncestorResult QueryNoRowid<T>(Expression<Func<T, bool>> predicate)
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<MySqlParameter>();
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

                    SqlString.Append("SELECT * FROM " + tableName);
                    SqlString.Append(whereString);

                    var paras = Parameters.Select(x =>
                    {
                        var parameter = new MySqlParameter(x.Name, (MySqlDbType)GetDbTypeFromType(x.Value.GetType()));
                        parameter.Value = x.Value;
                        parameter.Direction = ParameterDirection.Input;
                        return parameter;
                    });

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
        protected override AncestorResult Update(IModel valueObject, IModel whereObject)
        {
            var SqlString = new StringBuilder();
            var sb2 = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<MySqlParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;
            var tableName = valueObject.GetType().Name;

            try
            {
                SqlString.Append("UPDATE " + tableName + " set ");
                SqlString.Append(UpdateTranslate(valueObject, parameters, UpdateMode.Original));
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
        protected override AncestorResult Update(IModel valueObject, object paramsObjects)
        {
            var SqlString = new StringBuilder();
            var sb2 = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<MySqlParameter>();
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


        protected override AncestorResult Update<T>(IModel valueObject, Expression<Func<T, bool>> predicate)
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<MySqlParameter>();
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

                    //var paras = from p in expParameters
                    //            select new MySqlParameter(p.Name, (MySqlDbType)GetDbType(p.Type),
                    //          p.Value, ParameterDirection.Input);
                    var paras = expParameters.Select(x =>
                    {
                        var parameter = new MySqlParameter(x.Name, (MySqlDbType)GetDbType(x.Type));
                        parameter.Value = x.Value;
                        parameter.Direction = ParameterDirection.Input;
                        return parameter;
                    });
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

        protected override AncestorResult BulkInsert<T>(List<T> ObjList)
        {
            throw new NotImplementedException();
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

        protected string ParseWhereCondition(object objectModel, ICollection<MySqlParameter> parameters)
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


                    sqlConditionWhere.Append(parameterName + " = " + DbSymbolize + parameterName);

                    var parameter = new MySqlParameter(DbSymbolize + prop.Name.ToUpper(), (MySqlDbType)GetDbTypeFromType(propertyType));
                    parameter.Value = prop.GetValue(objectModel, null);
                    parameter.Direction = ParameterDirection.Input;
                    parameters.Add(parameter);
                    //parameters.Add(
                    //        new MySqlParameter(DbSymbolize + parameterName, (OracleDbType)GetDbType(propertyType.Name), prop.GetValue(objectModel, null), ParameterDirection.Input)
                    //        );
                    sqlConditionWhere.Append(" AND ");
                }
                if (parameters.Count > 0)
                    sqlConditionWhere.Remove(sqlConditionWhere.Length - 5, 5);
                else
                    sqlConditionWhere.Remove(sqlConditionWhere.Length - 7, 7);
            }

            return sqlConditionWhere.ToString();
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
            MySqlDbType returnType = MySqlDbType.VarChar;
            _MySqlDbTypeDic = SetMySqlDbTypeList();
            if (!_MySqlDbTypeDic.TryGetValue(typeString.ToUpper(), out returnType))
            {
                returnType = MySqlDbType.VarChar;
            }
            return returnType;
        }

        private Dictionary<string, MySqlDbType> SetMySqlDbTypeList()
        {
            if (_MySqlDbTypeDic == null)
            {
                _MySqlDbTypeDic = new Dictionary<string, MySqlDbType>();
                _MySqlDbTypeDic.Add("VARCHAR2", MySqlDbType.VarChar);
                _MySqlDbTypeDic.Add("SYSTEM.STRING", MySqlDbType.VarChar);
                _MySqlDbTypeDic.Add("STRING", MySqlDbType.VarChar);
                _MySqlDbTypeDic.Add("SYSTEM.DATETIME", MySqlDbType.Datetime);
                _MySqlDbTypeDic.Add("DATETIME", MySqlDbType.Datetime);
                _MySqlDbTypeDic.Add("DATE", MySqlDbType.Date);
                _MySqlDbTypeDic.Add("INT64", MySqlDbType.Int64);
                _MySqlDbTypeDic.Add("INT32", MySqlDbType.Int32);
                _MySqlDbTypeDic.Add("INT16", MySqlDbType.Int16);
                _MySqlDbTypeDic.Add("BYTE", MySqlDbType.Binary);
                _MySqlDbTypeDic.Add("DECIMAL", MySqlDbType.Decimal);
                _MySqlDbTypeDic.Add("FLOAT", MySqlDbType.Float);
                _MySqlDbTypeDic.Add("DOUBLE", MySqlDbType.Double);
                _MySqlDbTypeDic.Add("BYTE[]", MySqlDbType.Binary);
                _MySqlDbTypeDic.Add("CHAR", MySqlDbType.String);
                _MySqlDbTypeDic.Add("CHAR[]", MySqlDbType.String);
                _MySqlDbTypeDic.Add("TIMESTAMP", MySqlDbType.Timestamp);
            }
            return _MySqlDbTypeDic;
        }

        protected string UpdateTranslate(IModel valueObject, List<MySqlParameter> parameters, UpdateMode mode)
        {
            var SqlString = new StringBuilder();
            foreach (PropertyInfo prop in valueObject.GetType().GetProperties())
            {
                if (prop.GetValue(valueObject, null) != null)
                {
                    UpdateAllTranslate(valueObject, parameters, SqlString, prop);

                }
            }
            if (SqlString.Length > 0)
                SqlString.Remove(SqlString.Length - 1, 1);
            return SqlString.ToString();
        }

        private void UpdateAllTranslate(IModel valueObject, List<MySqlParameter> parameters, StringBuilder SqlString, PropertyInfo prop)
        {
            if (CheckBrowsable(valueObject, prop.Name))
            {
                //SqlStringBuilder.Append(prop.Name.ToUpper() + " = :" + prop.Name.ToUpper() + ",");
                SqlString.Append(prop.Name.ToUpper() + " = " + DbSymbolize + prop.Name.ToUpper() + ",");

                var propertyType = prop.PropertyType;

                if (prop.PropertyType.IsGenericType &&
                        prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    propertyType = prop.PropertyType.GetGenericArguments()[0];

                //如果obj value非null但長度為0, 代表需為NULL, 以DBnull.Value傳值
                //parameters.Add(new MySqlParameter(DbSymbolize + prop.Name.ToUpper(), (MySqlDbType)GetDbType(propertyType.Name), prop.GetValue(valueObject, null).ToString().Length > 0 ? prop.GetValue(valueObject, null) : DBNull.Value, ParameterDirection.Input));
                var parameter = new MySqlParameter(DbSymbolize + prop.Name.ToUpper(), (MySqlDbType)GetDbTypeFromType(prop.PropertyType))
                {
                    Value = prop.GetValue(valueObject, null)?.ToString() != null ? prop.GetValue(valueObject, null) : DBNull.Value,
                    Direction = ParameterDirection.Input
                };
                parameters.Add(parameter);
            }
        }


        protected override AncestorResult UpdateAll(IModel valueObject, IModel whereObject)
        {
            var SqlString = new StringBuilder();
            var sb2 = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<MySqlParameter>();
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
            var parameters = new List<MySqlParameter>();
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

                    //var paras = from p in expParameters
                    //            select new MySqlParameter(p.Name, (MySqlDbType)GetDbType(p.Type),
                    //          p.Value, ParameterDirection.Input);
                    var paras = expParameters.Select(x =>
                    {
                        var parameter = new MySqlParameter(x.Name, (MySqlDbType)GetDbType(x.Type));
                        parameter.Value = x.Value;
                        parameter.Direction = ParameterDirection.Input;
                        return parameter;
                    });
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
