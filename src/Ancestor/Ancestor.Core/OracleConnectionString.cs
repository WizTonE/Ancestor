using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    public class OracleConnectionString : IConnectionString
    {
        public int Connection_Lifetime { get; set; }
    }
}
