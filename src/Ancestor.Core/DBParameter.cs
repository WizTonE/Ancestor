using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    public class DBParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Int32 Size { get; set; }
        public string Direction { get; set; }
        public ParameterDirection ParameterDirection { get; set; }
        public object Value { get; set; }
    }
}
