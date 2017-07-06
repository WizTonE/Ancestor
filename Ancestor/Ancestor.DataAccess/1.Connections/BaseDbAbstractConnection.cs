using Ancestor.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ancestor.Core;
using System.Data.Common;

namespace Ancestor.DataAccess.Connections
{
    /// <summary>
    /// Creator : WizTonE 
    /// Date    : 2017/04/17
    /// Subject : BaseDbAbstractConnection
    /// 
    /// History : 
    /// 2017/04/17 WizTonE 建立 : DbConnection底層之Abstract Class, 繼承IConnection, 並提供virtual機制供override使用
    /// </summary>
    public abstract class BaseDbAbstractConnection : IConnection
    {
        protected DBObject dBObject { get; set; }
        protected string ConnectionString { get; set; }
        protected DbConnection DBConnection { get; set; }


        #region Virtual Function
        protected virtual void SetConnectionObject(DBObject dbObject)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Interface Implement
        DBObject IConnection.dBObject { get { return dBObject; } set { dBObject = value; } }
        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        DbConnection IConnection.GetConnectionObject()
        {
            return DBConnection;
        }

        void IConnection.SetConnectionObject(DBObject dbObject)
        {
            SetConnectionObject(dbObject);
        }
        #endregion
        
    }
}
