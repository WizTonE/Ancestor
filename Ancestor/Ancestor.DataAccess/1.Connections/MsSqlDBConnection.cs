using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Ancestor.Core;
using System.Data.SqlClient;
using Ancestor.DataAccess.Interface;

namespace Ancestor.DataAccess.Connections
{
    public class MsSqlDBConnection : IConnection
    {
        SqlConnection DBConnection { get; set; }
        string ConnectionString { get; set; }
        public DBObject dBObject { get; set; }

        public MsSqlDBConnection(DBObject dbObject)
        {
            DBConnection = new SqlConnection();
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

        public void SetConnectionObject(DBObject dbObject)
        {
            dBObject = dbObject;
            // 2015-09-02 Renew data source.
            var dataSource = dbObject.IP == null ? dbObject.Node : dbObject.IP;

            ConnectionString = @"Data Source = " + dataSource + "; Initial catalog = " + dbObject.Schema;
            ConnectionString += ";User Id=" + dbObject.ID + ";Password=" + dbObject.Password;
            DBConnection.ConnectionString = ConnectionString;
        }
    }
}
