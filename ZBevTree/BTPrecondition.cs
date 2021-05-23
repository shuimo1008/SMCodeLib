using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZBevTree
{
    public abstract class BTPrecondition : BTNode
    {
        public BTPrecondition(int maxChildCount) : 
            base(maxChildCount)
        {
        }

        public abstract bool IsTrue(BTWorkingData data);
    }

    public abstract class BTPreconditionLeaf : BTPrecondition
    {
        public BTPreconditionLeaf() : 
            base(0) { }
    }

    public abstract class BTPreconditonUnary : BTPrecondition
    {
        public BTPreconditonUnary(BTPrecondition lhs)
            : base(1)
        {
            AddChild(lhs);
        }
    }

    public abstract class BTPreconditionBinary : BTPrecondition
    {
        public BTPreconditionBinary(BTPrecondition lhs, BTPrecondition rhs)
            :base(2)
        {
            AddChild(lhs).AddChild(rhs);
        }
    }

    public class BTPreconditionTRUE : BTPreconditionLeaf
    {
        public override bool IsTrue(BTWorkingData data)
        {
            return true;
        }
    }

    public class BTPreconditionFALSE : BTPreconditionLeaf
    {
        public override bool IsTrue(BTWorkingData data)
        {
            return false;
        }
    }

    public class BTPreconditionNOT : BTPreconditonUnary
    {
        public BTPreconditionNOT(BTPrecondition lhs)
            : base(lhs)
        {
        }

        public override bool IsTrue(BTWorkingData data)
        {
            return !GetChild<BTPrecondition>(0).IsTrue(data);
        }
    }

    public class BTPreconditionAND : BTPreconditionBinary
    {
        public BTPreconditionAND(BTPrecondition lhs, BTPrecondition rhs)
            : base(lhs, rhs)
        { }
        public override bool IsTrue(BTWorkingData data)
        {
            return GetChild<BTPrecondition>(0).IsTrue(data) &&
                   GetChild<BTPrecondition>(1).IsTrue(data);
        }
    }
    public class BTPreconditionOR : BTPreconditionBinary
    {
        public BTPreconditionOR(BTPrecondition lhs, BTPrecondition rhs)
            : base(lhs, rhs)
        { }
        public override bool IsTrue(BTWorkingData data)
        {
            return GetChild<BTPrecondition>(0).IsTrue(data) ||
                   GetChild<BTPrecondition>(1).IsTrue(data);
        }
    }
    public class BTPreconditionXOR : BTPreconditionBinary
    {
        public BTPreconditionXOR(BTPrecondition lhs, BTPrecondition rhs)
            : base(lhs, rhs)
        { }
        public override bool IsTrue(BTWorkingData wData)
        {
            return GetChild<BTPrecondition>(0).IsTrue(wData) ^
                   GetChild<BTPrecondition>(1).IsTrue(wData);
        }
    }
}
