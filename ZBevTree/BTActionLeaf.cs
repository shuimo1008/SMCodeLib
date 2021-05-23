using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZBevTree
{
    public class BTActionLeaf : BTAction
    {
        private const int ACTION_READY = 0;
        private const int ACTION_RUNNING = 1;
        private const int ACTION_FINISHED = 2;

        public BTActionLeaf() : base(0)
        {
        }

        class BTActionLeafContext : BTActionContext
        {
            public int Status { get; set; }
            public bool NeedExit { get; set; }
            private object UserData { get; set; }

            public BTActionLeafContext()
            {
                Status = ACTION_READY;
                NeedExit = false;
                UserData = null;
            }

            public T GetUserData<T>() where T : class, new()
            {
                if (UserData == null) UserData = new T();
                return UserData as T;
            }
        }


        public override int OnUpdate(BTWorkingData data)
        {
            int oRunningState = BTStatus.FINISHED;
            BTActionLeafContext oThisContext = GetContext<BTActionLeafContext>(data);
            if (oThisContext.Status == ACTION_READY)
            {
                OnEnter(data);
                oThisContext.NeedExit = true;
                oThisContext.Status = ACTION_RUNNING;
            }
            if (oThisContext.Status == ACTION_RUNNING)
            {
                oRunningState = OnExecute(data);
                if (BTStatus.IsFinished(oRunningState))
                {
                    oThisContext.Status = ACTION_FINISHED;
                }
            }
            if (oThisContext.Status == ACTION_FINISHED)
            {
                if (oThisContext.NeedExit) OnExit(data);
                oThisContext.Status = ACTION_READY;
                oThisContext.NeedExit = false;
            }

            return oRunningState;
        }

        protected sealed override void OnTransition(BTWorkingData data)
        {
            BTActionLeafContext oThisContext = GetContext<BTActionLeafContext>(data);
            if (oThisContext.NeedExit)
            {
                OnExit(data);
            }
            oThisContext.Status = ACTION_READY;
            oThisContext.NeedExit = false;
        }

        protected T GetUserContextData<T>(BTWorkingData data) where T : class, new()
        {
            return GetContext<BTActionLeafContext>(data).GetUserData<T>();
        }

        protected virtual void OnEnter(BTWorkingData data)
        {

        }

        protected virtual int OnExecute(BTWorkingData data)
        {
            return BTStatus.FINISHED;
        } 

        protected virtual void OnExit(BTWorkingData data)
        {

        }
    }
}
