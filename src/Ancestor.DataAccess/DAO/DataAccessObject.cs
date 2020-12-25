using Ancestor.Core;
using System.Text;
using Ancestor.DataAccess.DBAction;
using System;
using Ancestor.DataAccess.Interface;

namespace Ancestor.DataAccess.DAO
{
    // 2016-02-08 1. Add feature for Dispose. 
    // 2017-06-15 Add SQL Command for debug.
    public abstract class DataAccessObject : IDisposable
    {
        public enum UpdateMode { Original, All };
        public DBObject DbObject { get; set; }
        public IDbAction DB { get; set; }
        //public StringBuilder SqlString { get; set; }
        internal string DbSymbolize { get; set; }
        internal string DbLikeSymbolize { get; set; }
        private bool _disposed = false;
        /// <summary>DBCommand語法(Debug Only)</summary>
        internal string DBCommand
        {
            get
            {
                return DB?.DbCommandString;
            }
        }
        public bool IsTransacting
        {
            get
            {
                return DB?.IsTransacting ?? false;
            }
        }


        internal virtual object GetDbType(string typeString)
        {
            return null;
        }
        internal virtual object GetDbType(Type type)
        {
            if (type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                type = type.GetGenericArguments()[0];
            var typeName = type == null ? "" : type.Name;
            return GetDbType(typeName);
        }
        ~DataAccessObject()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    Disposing();
                _disposed = true;
            }
        }

        protected virtual void Disposing()
        {
            if (DB != null)
            {
                DB.Dispose();
                DB = null;
            }
        }
    }
}
