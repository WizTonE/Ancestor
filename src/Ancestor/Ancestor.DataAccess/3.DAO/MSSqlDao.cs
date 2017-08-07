using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Ancestor.Core;
using Ancestor.DataAccess.DBAction;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.ComponentModel;
using Ancestor.DataAccess.Interface;

namespace Ancestor.DataAccess.DAO
{
    public class MSSqlDao : DataAccessObject, IDataAccessObject
    {
        Dictionary<string, SqlDbType> _SqlDbTypeDic;
        public MSSqlDao()
        {
            //base.SqlString = new StringBuilder();
        }
        public MSSqlDao(DBObject _dBObject)
            : this()
        {
            base.DbObject = _dBObject;
            //base.DB = GetActionFactory();
            base.DbSymbolize = "@";
            base.DbLikeSymbolize = "&";
        }
        public AncestorResult Delete(IModel whereObject)
        {
            var SqlString = new StringBuilder();
            //StringBuilder sb2 = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<SqlParameter>();
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

        public AncestorResult Delete<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<SqlParameter>();
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

                    var paras = Parameters.GetType().GetProperties().Select(x =>
                    {
                        var parameter = new SqlParameter(DbSymbolize + x.Name, (SqlDbType)GetDbType(x.PropertyType.Name));
                        parameter.Value = x.GetValue(Parameters, null);
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

        public AncestorResult ExecuteNonQuery(string sqlString, object modelObject)
        {
            var SqlString = new StringBuilder();
            SqlString.Append(sqlString);
            var effectRows = 0;
            var parameters = new List<SqlParameter>();
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
                        var parameter = new SqlParameter(DbSymbolize + prop.Name.ToUpper(), (SqlDbType)GetDbType(propertyType.Name));
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

        public AncestorResult ExecuteStoredProcedure(string procedureName, bool bindbyName, List<DBParameter> dBParameter)
        {
            var parameters = new List<SqlParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;

            try
            {
                foreach (DBParameter Parameter in dBParameter)
                {
                    if (Parameter.ParameterDirection == ParameterDirection.Input)
                    {
                        parameters.Add(new SqlParameter
                        {
                            ParameterName = Parameter.Name,
                            SqlDbType = (SqlDbType)GetDbType(Parameter.Type),
                            Value = Parameter.Value.ToString().Length > 0 ? Parameter.Value : DBNull.Value,
                            Direction = ParameterDirection.Input,
                            Size = Parameter.Size
                        });
                    }
                    if (Parameter.ParameterDirection == ParameterDirection.Output)
                    {
                        parameters.Add(new SqlParameter
                        {
                            ParameterName = Parameter.Name,
                            SqlDbType = (SqlDbType)GetDbType(Parameter.Type),
                            Direction = ParameterDirection.Output,
                            Size = Parameter.Size
                        });
                    }
                    if (Parameter.ParameterDirection == ParameterDirection.InputOutput)
                    {
                        parameters.Add(new SqlParameter()
                        {
                            ParameterName = Parameter.Name,
                            SqlDbType = (SqlDbType)GetDbType(Parameter.Type),
                            Value = Parameter.Value.ToString().Length > 0 ? Parameter.Value : DBNull.Value,
                            Direction = ParameterDirection.InputOutput,
                            Size = Parameter.Size
                        });
                    }
                    if (Parameter.ParameterDirection == ParameterDirection.ReturnValue)
                    {
                        parameters.Add(new SqlParameter()
                        {
                            ParameterName = Parameter.Name,
                            SqlDbType = (SqlDbType)GetDbType(Parameter.Type),
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

        public AncestorResult Insert(IModel objectModel)
        {
            var SqlString = new StringBuilder();
            var sqlValueString = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<SqlParameter>();
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
                        var parameter = new SqlParameter(DbSymbolize + prop.Name.ToUpper(), (SqlDbType)GetDbType(propertyType.Name));
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

        public AncestorResult Query(IModel objectModel)
        {
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<SqlParameter>();
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

        public AncestorResult Query(string sqlString, object paramsObjects)
        {
            var isSuccess = false;
            var returnResult = new AncestorResult();
            var parameters = new List<SqlParameter>();
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
                    //var paras = from prop in paramsObjects.GetType().GetProperties()
                    //            select
                    //                new SqlParameter(DbSymbolize + prop.Name, (SqlDbType)GetDbType(prop.PropertyType.Name));

                    var paras = paramsObjects.GetType().GetProperties().Select(x =>
                    {
                        var parameter = new SqlParameter(DbSymbolize + x.Name, (SqlDbType)GetDbType(x.PropertyType.Name));
                        parameter.Value = x.GetValue(paramsObjects, null);
                        parameter.Direction = ParameterDirection.Input;
                        return parameter;
                    });
                    //Todo
                    if (((SqlParameter)paras.FirstOrDefault()).Value != null)
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

        public AncestorResult Query<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<SqlParameter>();
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
                        var parameter = new SqlParameter(x.Name, (SqlDbType)GetDbType(x.Value.GetType().Name));
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

        public AncestorResult QueryNoRowid(IModel objectModel)
        {
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<SqlParameter>();
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

        public AncestorResult QueryNoRowid<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<SqlParameter>();
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
                        var parameter = new SqlParameter(x.Name, (SqlDbType)GetDbType(x.Value.GetType().Name));
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
        public AncestorResult Update(IModel valueObject, object paramsObjects)
        {
            var SqlString = new StringBuilder();
            var sb2 = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<SqlParameter>();
            var returnResult = new AncestorResult();
            var isSuccess = false;
            var tableName = valueObject.GetType().Name;

            try
            {
                SqlString.Append("UPDATE " + tableName + " set ");
                SqlString.Append(UpdateTranslate(valueObject, parameters, UpdateMode.Original));


                if (paramsObjects != null)
                {
                    var paras = paramsObjects.GetType().GetProperties().Select(x => {
                        var parameter = new SqlParameter(DbSymbolize + x.Name, (SqlDbType)GetDbType(x.PropertyType.Name));
                        parameter.Value = x.GetValue(valueObject, null);
                        parameter.Direction = ParameterDirection.Input;
                        return parameter;
                    });
                    
                    //Todo
                    if (((SqlParameter)paras.FirstOrDefault()).Value != null)
                        parameters.AddRange(paras);
                }
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
        public AncestorResult Update(IModel valueObject, IModel whereObject)
        {
            var SqlString = new StringBuilder();
            var sb2 = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<SqlParameter>();
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

        public AncestorResult Update<T>(IModel valueObject, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<SqlParameter>();
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
                    //            select new SqlParameter(p.Name, (SqlDbType)GetDbType(p.Type),
                    //          p.Value, ParameterDirection.Input);
                    var paras = expParameters.Select(x =>
                    {
                        var parameter = new SqlParameter(x.Name, (SqlDbType)GetDbType(x.Type));
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

        AncestorResult IDataAccessObject.BulkInsert<T>(List<T> ObjList)
        {
            var SqlString = new StringBuilder();
            //var sqlValueString = new StringBuilder();
            var effectRows = 0;
            //var parameters = new List<M>();
            var returnResult = new AncestorResult();
            var isSuccess = false;
            var tableName = new T().GetType().Name;

            try
            {
                isSuccess = DB.BulkInsert(ObjList, ref effectRows);
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

        AncestorResult IDataAccessObject.Query<T>(IModel objectModel)
        {
            var SqlString = new StringBuilder();
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<SqlParameter>();
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

        AncestorResult IDataAccessObject.QueryNoRowid<T>(IModel objectModel)
        {
            var SqlString = new StringBuilder();
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<SqlParameter>();
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

        private string ParseWhereCondition(object objectModel, ICollection<SqlParameter> parameters)
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

                    var parameter = new SqlParameter(DbSymbolize + prop.Name.ToUpper(), (SqlDbType)GetDbType(propertyType.Name));
                    parameter.Value = prop.GetValue(objectModel, null);
                    parameter.Direction = ParameterDirection.Input;
                    parameters.Add(parameter);
                    //parameters.Add(
                    //        new SqlParameter(DbSymbolize + parameterName, (OracleDbType)GetDbType(propertyType.Name), prop.GetValue(objectModel, null), ParameterDirection.Input)
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
            SqlDbType returnType = SqlDbType.VarChar;
            _SqlDbTypeDic = SetSqlDbTypeList();
            if (!_SqlDbTypeDic.TryGetValue(typeString.ToUpper(), out returnType))
            {
                returnType = SqlDbType.VarChar;
            }
            return returnType;
        }

        private Dictionary<string, SqlDbType> SetSqlDbTypeList()
        {
            if (_SqlDbTypeDic == null)
            {
                _SqlDbTypeDic = new Dictionary<string, SqlDbType>();
                _SqlDbTypeDic.Add("VARCHAR2", SqlDbType.VarChar);
                _SqlDbTypeDic.Add("SYSTEM.STRING", SqlDbType.VarChar);
                _SqlDbTypeDic.Add("STRING", SqlDbType.VarChar);
                _SqlDbTypeDic.Add("SYSTEM.DATETIME", SqlDbType.Date);
                _SqlDbTypeDic.Add("DATETIME", SqlDbType.Date);
                _SqlDbTypeDic.Add("DATE", SqlDbType.Date);
                _SqlDbTypeDic.Add("INT64", SqlDbType.BigInt);
                _SqlDbTypeDic.Add("INT32", SqlDbType.Int);
                _SqlDbTypeDic.Add("INT16", SqlDbType.SmallInt);
                _SqlDbTypeDic.Add("BYTE", SqlDbType.Binary);
                _SqlDbTypeDic.Add("DECIMAL", SqlDbType.Decimal);
                _SqlDbTypeDic.Add("FLOAT", SqlDbType.Float);
                _SqlDbTypeDic.Add("DOUBLE", SqlDbType.Decimal);
                _SqlDbTypeDic.Add("BYTE[]", SqlDbType.Binary);
                _SqlDbTypeDic.Add("CHAR", SqlDbType.Char);
                _SqlDbTypeDic.Add("CHAR[]", SqlDbType.Char);
                _SqlDbTypeDic.Add("TIMESTAMP", SqlDbType.Timestamp);
                _SqlDbTypeDic.Add("REFCURSOR", SqlDbType.Variant);
            }
            return _SqlDbTypeDic;
        }

        private string UpdateTranslate(IModel valueObject, List<SqlParameter> parameters, UpdateMode mode)
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

        private void UpdateAllTranslate(IModel valueObject, List<SqlParameter> parameters, StringBuilder SqlString, PropertyInfo prop)
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
                //parameters.Add(new SqlParameter(DbSymbolize + prop.Name.ToUpper(), (SqlDbType)GetDbType(propertyType.Name), prop.GetValue(valueObject, null).ToString().Length > 0 ? prop.GetValue(valueObject, null) : DBNull.Value, ParameterDirection.Input));
                var parameter = new SqlParameter(DbSymbolize + prop.Name.ToUpper(), (SqlDbType)GetDbType(prop.PropertyType.Name))
                {
                    Value = prop.GetValue(valueObject, null)?.ToString() != null ? prop.GetValue(valueObject, null) : DBNull.Value,
                    Direction = ParameterDirection.Input
                };
                parameters.Add(parameter);
            }
        }

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release or Dispose managed resources.
                // Free other state (managed objects).
                DbSymbolize = string.Empty;
                DbLikeSymbolize = string.Empty;
            }
            // Set large fields to null.
            // Call Dispose on your base class.
            // Free your own state (unmanaged objects).
            DB = null;
        }

        public IDbTransaction BeginTransaction()
        {
            return DB.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel isoLationLevel)
        {
            return DB.BeginTransaction(isoLationLevel);
        }
        public AncestorResult Query<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> selectCondition)
            where T : class, new()
        {
            throw new NotImplementedException();
        }
        public AncestorResult Query<T1, T2>(Expression<Func<T1, T2, bool>> predicate, Expression<Func<T1, T2, object>> selectCondition)
            where T1 : class, new()
            where T2 : class, new()
        {
            throw new NotImplementedException();
        }

        public AncestorResult Query<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> predicate, Expression<Func<T1, T2, T3, object>> selectCondition)
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
        {
            throw new NotImplementedException();
        }

        public AncestorResult Query<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> predicate, Expression<Func<T1, T2, T3, T4, object>> selectCondition)
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
        {
            throw new NotImplementedException();
        }

        public AncestorResult Query<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, object>> selectCondition)
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
            where T5 : class, new()
        {
            throw new NotImplementedException();
        }

        public AncestorResult Query<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate, Expression<Func<T1, T2, T3, T4, T5, T6, object>> selectCondition)
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
            where T5 : class, new()
            where T6 : class, new()
        {
            throw new NotImplementedException();
        }



        ~MSSqlDao()
        {
            Dispose(false);
        }


        IDbConnection IDataAccessObject.DBConnection
        {
            get { return DB.GetConnectionFactory(); }
        }

        public void Commit()
        {
            DB.DbCommit();
        }


        public void Rollback()
        {
            DB.DbRollBack();
        }

        public AncestorResult UpdateAll(IModel valueObject, IModel whereObject)
        {
            var SqlString = new StringBuilder();
            var sb2 = new StringBuilder();
            var effectRows = 0;
            var parameters = new List<SqlParameter>();
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

        public AncestorResult UpdateAll<T>(IModel valueObject, Expression<Func<T, bool>> predicate) where T : class, new()
        {
            string whereString = string.Empty;
            var isSuccess = false;
            var sqlString = string.Empty;
            var returnResult = new AncestorResult();
            var parameters = new List<SqlParameter>();
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
                    //            select new SqlParameter(p.Name, (SqlDbType)GetDbType(p.Type),
                    //          p.Value, ParameterDirection.Input);
                    var paras = expParameters.Select(x =>
                    {
                        var parameter = new SqlParameter(x.Name, (SqlDbType)GetDbType(x.Type));
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
