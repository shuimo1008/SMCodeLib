using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ZCSharpLib.Exceptions
{
    public class CustomException : Exception
    {
        public CustomException(string iMessage) : base(iMessage) { }
    }
}
