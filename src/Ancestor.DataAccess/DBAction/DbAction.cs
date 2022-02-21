using Ancestor.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DBAction
{
    public abstract class DbAction
    {
        public DBObject DbObject { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Error { get; set; }
        public virtual string DbCommandString
        {
            get { throw new NotImplementedException(); }
        }
        /// <summary>是否處於交易狀態</summary>
        public virtual bool IsTransacting
        {
            get { throw new NotImplementedException(); }
        }
        public bool? AutoClosed { get; set; }
        public bool? Validatable { get; set; }
        protected bool GetAutoClose()
        {
            return AutoClosed ?? AncestorGlobalOptions.AutoClose;
        }
        protected bool GetValidatable()
        {
            return Validatable ?? AncestorGlobalOptions.ValidateConnection;
        }
        protected bool CheckConnection(IDbConnection dbConnection, IDbCommand dataCommand, string testString)
        {
            bool isConn = false;
            //string Error_msg = string.Empty;
            //OracleConnection ClonedCon = (OracleConnection)DB_Connection.Clone();
            if (dbConnection.State != ConnectionState.Open)
            {
                isConn = Connect(dbConnection);
            }
            else if(GetValidatable())
            {
                if (ValidateConnection(dbConnection, dataCommand, testString))
                    isConn = true;
                else
                {
                    dbConnection.Close();
                    isConn = Connect(dbConnection);
                }
            }
            return isConn;
        }

        private bool Connect(IDbConnection dbConnection)
        {
            bool isConn;
            try
            {
                dbConnection.Open();
                isConn = true;
            }
            catch (Exception exception)
            {
                isConn = false;
                ErrorMessage = exception.ToString();
            }

            return isConn;
        }

        private bool ValidateConnection(IDbConnection dbConnection, IDbCommand dataCommand, string testString)
        {
            IDataReader dataReader;
            bool is_success = false;
            ErrorMessage = string.Empty;
            dataCommand = dbConnection.CreateCommand();
            dataCommand.CommandText = testString;
            try
            {
                dataReader = dataCommand.ExecuteReader();
                if (dataReader.Read())
                {
                    dataReader.Close();
                    is_success = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.ToString();
                is_success = false;
            }
            return is_success;
        }
        /// <summary>
        /// SQL 語法參數 Binding 用的符號 (Ex : SQL - > @, Oracle -> :, MySql -> ?)
        /// </summary>
        //public abstract string DBParameterSymbol { get; set; }
        //public abstract bool Query<T>(string SqlString, IModel ModelObject, bool bindingWhere, ref IList<T> dataList);
        //public abstract bool Query(string SqlString, IModel ModelObject, bool bindingWhere, ref System.Data.DataTable dataTable);
        //public abstract bool Insert(IModel ModelObject, ref int SuccessRows);
        //public abstract bool Update(IModel ValueObject, IModel WhereObject, ref int SuccessRows);
        //public abstract bool Delete(IModel WhereObject, ref int SuccessRows);
        //public abstract bool ExecuteNonQuery(string SqlString, IModel ModelObject, bool bindingWhere, ref int SuccessRows);
        //public abstract bool ExecuteStoredProcedure(string ProcedureName, bool BindbyName, List<DBParameter> dBParameter);
    }
}
