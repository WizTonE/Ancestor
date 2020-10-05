using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    //10g ref: https://docs.oracle.com/cd/B19306_01/win.102/b14307/featConnecting.htm
    public class OracleConnectionString : IConnectionString
    {
        public int? Connection_Lifetime { get; set; }
        public int? Connection_Timeout { get; set; }
        public bool? Context_Connection { get; set; }
        public string DBA_Privilege { get; set; }
        public string Decr_Pool_Size { get; set; }
        public int? Enlist { get; set; }
        public bool? HA_Events { get; set; }
        public bool? Load_Balancing { get; set; }
        public int? Incr_Pool_Size { get; set; }
        [Default(true)]
        public bool? Pooling { set; get; }
        [Default(10)]
        public int? Max_Pool_Size { get; set; }
        public int? Min_Pool_Size { get; set; }
        public bool? Persist_Security_Info { get; set; }        
        public string Proxy_User_Id { get; set; }
        public string Proxy_Password { get; set; }
        public bool? Statement_Cache_Purge { get; set; }
        public int? Statement_Cache_Size { get; set; }
        public bool? Validate_Connection { get; set; }

    }
}
