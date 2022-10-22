using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Boxs
{
    public class BoxUtils
    {
        public static IBox NewBox(params object[] vs)
        {


            return new SystemBox();
        }

    }
}
