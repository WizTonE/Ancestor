using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ancestor.Core;
using Ancestor.DataAccess.DAO;
using Ancestor.DataAccess.Interface;

namespace Ancestor.DataAccess.Factory
{
    public class DAOFactory : DAOFactoryBase, IDAOFactory
    {
        public DAOFactory (DBObject _dbObject)
        {
            DbObject = _dbObject;
        }

        public IDataAccessObject GetDataAccessObjectFactory()
        {
            var resource = new DataObjectAccessResource(DbObject).DataObjectAccessResources.FirstOrDefault(x => x.Database == DbObject.DataBaseType);
            if (resource != null && resource.IDAOobject != null)
            {
                resource.IDAOobject.GetActionFactory();
                return resource.IDAOobject;
            }
            throw new NullReferenceException("Suitable Connection isn't existed");
        }


    }

    internal class DataObjectAccessResource
    {
        private List<DataObjectAccessResource> _DataObjectAccessResources;
        internal IDataAccessObject IDAOobject { get; private set; }
        internal DBObject DbObject { get; set; }
        internal DBObject.DataBase Database { get; private set; }
        internal List<DataObjectAccessResource> DataObjectAccessResources
        {
            get
            {
                if (_DataObjectAccessResources == null)
                {
                    GetConnections(DbObject);
                }
                return _DataObjectAccessResources;
            }
        }
        internal DataObjectAccessResource(DBObject _DbObject)
        {
            DbObject = _DbObject;
        }
        private DataObjectAccessResource(DBObject.DataBase _DataBase, IDataAccessObject _Connection)
        {
            Database = _DataBase;
            IDAOobject = _Connection;
        }
        private void GetConnections(DBObject dbOBject)
        {
            _DataObjectAccessResources = new List<DataObjectAccessResource>();
            _DataObjectAccessResources.Add(new DataObjectAccessResource(DBObject.DataBase.Oracle, new OracleDao(dbOBject)));
            _DataObjectAccessResources.Add(new DataObjectAccessResource(DBObject.DataBase.MSSQL, new MSSqlDao(dbOBject)));
            _DataObjectAccessResources.Add(new DataObjectAccessResource(DBObject.DataBase.MySQL, new MySqlDao(dbOBject)));
        }
    }
}
