using Ancestor.Core;
using Ancestor.DataAccess.Interface;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.Connections
{
    /// <summary>
    /// Author  : WizTonE 
    /// Date    : 2015/07/27
    /// Subject : OracleDBConnection 設定與傳回Oracle連線物件
    /// 
    /// History : 
    /// 2015/07/27 WizTonE建立
    /// 
    /// </summary>
    public class OracleDBConnection : IConnection
    {
        OracleConnection DBConnection { get; set; }
        string ConnectionString { get; set; }

        public OracleDBConnection(DBObject dbObject)
        {
            DBConnection = new OracleConnection();
            this.SetConnectionObject(dbObject);
        }

        /// <summary>
        /// 設定連線
        /// 1. DBObject.Mode.Direct為直接連線, 直接連線至ping的到的資料庫位置
        /// 2. 其餘一律參照tnsname.ora中之連線位置
        /// </summary>
        /// <param name="dbObject"></param>
        public void SetConnectionObject(DBObject dbObject)
        {
            dBObject = dbObject;
            // 2015-09-02 Renew data source.
            var dataSource = dbObject.IP == null ? dbObject.Node : dbObject.IP;

            if (dbObject.ConnectedMode == DBObject.Mode.Direct)
                ConnectionString = @"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)"
                        + "(HOST=" + dataSource + ")(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=" + dbObject.Node + ")));";
            else
                ConnectionString = @"Data Source = " + dataSource + "; ";

            //ConnectionString += "User Id=" + dbObject.ID + ";Password=" + dbObject.Password + ";Pooling=true";
            var connectionStrings = new List<string>
            {
                "User Id=" + dbObject.ID,
                "Password=" + dbObject.Password,
            };

            dbObject.ConnectionString = dbObject.ConnectionString ?? new OracleConnectionString();

            var properties = dbObject.ConnectionString.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(dbObject.ConnectionString, null);
                var defaultValue = property.GetCustomAttributes(typeof(DefaultAttribute), true).FirstOrDefault();
                if (value != null)
                {
                    connectionStrings.Add(string.Format("{0}={1}", property.Name.Replace("_", " "), value));
                }
                else if(defaultValue != null)
                {
                    connectionStrings.Add(string.Format("{0}={1}", property.Name.Replace("_", " "), ((DefaultAttribute)defaultValue).Value));
                }
            }

            ConnectionString += string.Join(";", connectionStrings);

            DBConnection.ConnectionString = ConnectionString;
        }

        /// <summary>
        /// 傳回連線物件
        /// </summary>
        /// <returns></returns>
        public DbConnection GetConnectionObject()
        {
            return DBConnection;
        }

        public void Dispose()
        {
            try
            {
                Dispose(true);
            }
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            finally
            {
                GC.SuppressFinalize(this);
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources.
            }
        }

        public DBObject dBObject
        {
            get;
            set;
        }
    }
}
