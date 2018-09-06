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

        private Encoding encoding;
        public int CodePage
        {
            get { return codePage; }
            set
            {
                codePage = value;
                Encoding = Encoding.GetEncoding(value);                
            }
        }
        public string CodeName
        {
            get { return codeName; }
            set
            {
                codeName = value;
                Encoding = Encoding.GetEncoding(value);
            }
        }
        public Encoding Encoding
        {
            get { return encoding; }
            private set { encoding = value; }
        }
        public HardWordAttribute()
        {
            CodePage = 950;
        }

        public HardWordAttribute(int codepage)
        {
            CodePage = codepage;
        }

        public HardWordAttribute(string codename)
        {
            CodeName = codename;
        }
    }
}
