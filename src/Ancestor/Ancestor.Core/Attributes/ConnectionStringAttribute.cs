using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    public  class DefaultAttribute : Attribute
    {
        
        public object Value { get; private set; }
        public DefaultAttribute(object value)
        {
            Value = value;
        }
    }
}
