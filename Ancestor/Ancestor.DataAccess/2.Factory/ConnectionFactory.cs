using Ancestor.Core;
using Ancestor.DataAccess.Connections;
using Ancestor.DataAccess.Factory;
using Ancestor.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess
{
    public class ConnectionFactory : DBConnection, IDBConnection
    {

        public ConnectionFactory(DBObject _dBObject)
        {
            DbObject = _dBObject;
        }

        public IConnection GetConnectionFactory()
        {
            var resource = new ConnectionResource(DbObject).Connections.Where(
                (x) => x.Database == DbObject.DataBaseType
                ).FirstOrDefault();
            if (resource != null && resource.Iconnection != null)
                return resource.Iconnection;
            throw new NullReferenceException("Suitable Connection isn't existed");
        }
    }

    internal class ConnectionResource
    {
        private List<ConnectionResource> _connections;
        internal IConnection Iconnection { get; private set; }
        internal DBObject DbObject { get; set; }
        internal DBObject.DataBase Database { get; private set; }
        internal List<ConnectionResource> Connections
        {
            get
            {
                if(_connections == null)
                {
                    GetConnections(DbObject);
                }
                return _connections;
            }
        }
        internal ConnectionResource(DBObject _DbObject)
        {
            DbObject = _DbObject;
        }
        private ConnectionResource(DBObject.DataBase _DataBase, IConnection _Connection)
        {
            Database = _DataBase;
            Iconnection = _Connection;
        }
        private void GetConnections(DBObject dbOBject)
        {
            _connections = new List<ConnectionResource>();
            _connections.Add(new ConnectionResource(DBObject.DataBase.Oracle, new OracleDBConnection(dbOBject)));
            _connections.Add(new ConnectionResource(DBObject.DataBase.MSSQL, new MsSqlDBConnection(dbOBject)));
            _connections.Add(new ConnectionResource(DBObject.DataBase.MySQL, new MySqlDbConnection(dbOBject)));
        }
    }
}
