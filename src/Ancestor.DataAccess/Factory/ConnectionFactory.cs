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
        private Lazy<IConnection> _connection;
        internal IConnection Iconnection { get { return _connection.Value; } }
        internal DBObject DbObject { get; set; }
        internal DBObject.DataBase Database { get; private set; }
        internal List<ConnectionResource> Connections
        {
            get
            {
                if (_connections == null)
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
        private ConnectionResource(DBObject.DataBase _DataBase, Lazy<IConnection> _Connection)
        {
            Database = _DataBase;
            _connection = _Connection;
        }
        private void GetConnections(DBObject dbOBject)
        {
            _connections = new List<ConnectionResource>();
            _connections.Add(new ConnectionResource(DBObject.DataBase.Oracle, new Lazy<IConnection>(() => new OracleDBConnection(dbOBject))));
            _connections.Add(new ConnectionResource(DBObject.DataBase.MSSQL, new Lazy<IConnection>(() => new MsSqlDBConnection(dbOBject))));
            _connections.Add(new ConnectionResource(DBObject.DataBase.MySQL, new Lazy<IConnection>(() => new MySqlDbConnection(dbOBject))));
        }
    }
}
