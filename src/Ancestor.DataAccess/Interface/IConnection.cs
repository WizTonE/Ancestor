using Ancestor.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.Interface
{
    /// <summary>
    /// Author  : WizTonE 
    /// Date    : 2015/07/27
    /// Subject : IConnection 訂定連線物件之Interface
    /// 
    /// History : 
    /// 2015/07/27 WizTonE建立
    /// 
    /// </summary>
    public interface IConnection : IDisposable
    {
        DBObject dBObject { get; set; }
        void SetConnectionObject(DBObject dbObject);
        DbConnection GetConnectionObject();
    }
}
