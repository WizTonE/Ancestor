using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    public class HardWordAttribute : Attribute
    {
        private int codePage;
        private string codeName;
        public int CodePage { get { return codePage; } set { codePage = value; } }
        public string CodeName { get { return codeName; } set { codeName = value; } }

        public HardWordAttribute()
        {

        }

        public HardWordAttribute(int codepage)
        {
            this.codePage = codepage;
        }

        public HardWordAttribute(string codename)
        {
            this.codeName = codename;
        }
    }
}
