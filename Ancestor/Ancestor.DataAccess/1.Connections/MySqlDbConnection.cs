using Ancestor.Core;
using Ancestor.DataAccess.Connections;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.Connections
{
    /// <summary>
    /// Creator : WizTonE 
    /// Date    : 2017/04/17
    /// Subject : MySqlDbConnection
    /// 
    /// History : 
    /// 2017/04/17 WizTonE 建立 : MySqlDbConnection
    /// </summary>
    internal class MySqlDbConnection : BaseDbAbstractConnection
    {
        public MySqlDbConnection(DBObject dbObject)
        {
            DBConnection = new MySqlConnection();
            this.SetConnectionObject(dbObject);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public DbConnection GetConnectionObject()
        {
            return DBConnection;
        }

        protected override void SetConnectionObject(DBObject dbObject)
        {
            dBObject = dbObject;
            // 2015-09-02 Renew data source.
            var dataSource = dbObject.IP == null ? dbObject.Node : dbObject.IP;
            dataSource += dbObject.Port == null ? "" : ";Port=" + dbObject.Port;

            ConnectionString = @"Server = " + dataSource + "; Database = " + dbObject.Schema;
            ConnectionString += ";Uid=" + dbObject.ID + ";Pwd=" + dbObject.Password;
            DBConnection.ConnectionString = ConnectionString;
        }
    }
}
