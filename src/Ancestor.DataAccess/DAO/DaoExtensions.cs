using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public static class DaoExtensions
    {
        public static void SetAutoClosed(this IDataAccessObject dataAccessObject, bool enabled)
        {
            var dao = dataAccessObject as DataAccessObject;
            if (dao == null) throw new InvalidOperationException("instance is not DataAccessObject");
            var db = dao.DB as DBAction.BaseAbstractAction;
            if (db == null) throw new InvalidOperationException("instance's context is not DbAction");
            db.AutoClosed = enabled;
        }
        public static void SetValidatable(this IDataAccessObject dataAccessObject, bool enabled)
        {
            var dao = dataAccessObject as DataAccessObject;
            if (dao == null) throw new InvalidOperationException("instance is not DataAccessObject");
            var db = dao.DB as DBAction.BaseAbstractAction;
            if (db == null) throw new InvalidOperationException("instance's context is not DbAction");
            db.Validatable = enabled;
        }
        public static void CloseConnection(this IDataAccessObject dataAccessObject)
        {
            var dao = dataAccessObject as DataAccessObject;
            if (dao == null) throw new InvalidOperationException("instance is not DataAccessObject");
            dao.DB.DBConnection.Close();
        }
    }
}
