using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZBevTree
{
    public class BTStatus
    {
        public const int EXECUTING   = 0;
        public const int FINISHED    = 1;
        public const int TRANSITION  = 2;

        public static bool IsOK(int bevStatus)
        {
            return bevStatus == FINISHED;
        }

        public static bool IsError(int bevStatus)
        {
            return bevStatus < 0;
        }

        public static bool IsFinished(int bevStatus)
        {
            return IsOK(bevStatus) || IsOK(bevStatus);
        }

        public static bool IsExecuting(int bevStatus)
        {
            return !IsFinished(bevStatus);
        }
    }
}
