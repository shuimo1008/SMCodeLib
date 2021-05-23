using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ZCSharpLib.Guids
{
    public class LongGuid
    {
        private long mGuid = 0;

        public LongGuid(long iStartID)
        {
            mGuid = iStartID;
        }

        public long GenerateGuid()
        {
            long oGuid = Interlocked.Increment(ref mGuid);
            byte[] bytes = Guid.NewGuid().ToByteArray();

            return oGuid;
        }

        //public 
    }
}
