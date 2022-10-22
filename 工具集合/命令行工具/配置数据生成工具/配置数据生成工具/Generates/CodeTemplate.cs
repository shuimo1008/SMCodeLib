using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
    public abstract class CodeTemplate
    {
        public abstract string REFERENCE { get; }

        public abstract string TMP_BASETPL { get; }

        public abstract string TMP_TEMPLATEMGR { get; }

        public abstract string CLASSCONTENT1 { get; }

        public abstract string CLASSCONTENT2 { get; }

        public abstract string COMMENT { get;  }

        public abstract string FIELDTYPE { get; }
    }
}
