using Ancestor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.Interface
{
    public interface IDBConnection
    {
        IConnection GetConnectionFactory();
    }
}
