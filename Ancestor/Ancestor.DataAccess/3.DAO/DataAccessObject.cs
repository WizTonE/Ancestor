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
        // 2016-02-08 Add Dispose function for OracleDao.
        public void Dispose()
        {
            try
            {
                this.Dispose(true);
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
        public abstract void Dispose(bool disposing);
        ~DataAccessObject()
        {
            Dispose(false);
        }
    }
}
