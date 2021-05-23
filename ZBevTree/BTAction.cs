using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZBevTree
{
    public class BTActionContext
    {

    }

    public class BTAction : BTNode
    {
        public BTPrecondition Precondition { get; set; }

        public BTAction(int maxChildCount) : base(maxChildCount)
        {

        }

        public bool Evaluate(BTWorkingData data)
        {
            return (Precondition == null || Precondition.IsTrue(data)) && OnEvaluate(data);
        }

        public virtual int Update(BTWorkingData data)
        {
            return OnUpdate(data);
        }

        public virtual int OnUpdate(BTWorkingData data)
        {
            return BTStatus.FINISHED;
        }

        public void Transition(BTWorkingData data)
        {
            OnTransition(data);
        }


        protected virtual bool OnEvaluate(BTWorkingData data)
        {
            return true;
        }

        protected virtual void OnTransition(BTWorkingData data)
        {
            
        }

        public BTAction SetPrecondition(BTPrecondition condition)
        {
            Precondition = condition;
            return this;
        }



        protected T GetContext<T>(BTWorkingData data) where T:BTActionContext, new()
        {
            int oGuid = GetHashCode();
            T thisContext = null;
            if (!data.Contexts.ContainsKey(oGuid))
            {
                thisContext = new T();
                data.Contexts.Add(oGuid, thisContext);
            }
            else
            {
                thisContext = data.Contexts[oGuid] as T;
            }
            return thisContext;
        }
    }
}
