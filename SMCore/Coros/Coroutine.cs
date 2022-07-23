using SMCore;
using SMCore.Driver;
using SMCore.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMCore.Coros
{
    public class Coroutine
    {
        public Coroutiner Coroutiner { get; private set; }
        public IEnumerator Enumerator { get; private set; }
        private Stack<IEnumerator> Storages { get; set; }

        public Coroutine(IEnumerator iEnumerator)
        {
            Storages = new Stack<IEnumerator>();
            Enumerator = iEnumerator;
        }

        public void Setup(Coroutiner coroutineer)
        {
            Coroutiner = coroutineer;
        }

        public void Start()
        {
            IoC.Resolve<IDriverService>().Subscribe(Update);
        }

        private bool isWaiting = false;
        void Update(float deltaTime)
        {
            bool isBreak = false;

            // 如果不处于等待状态则前往下一个中断点
            if (!isWaiting)
            {
                while (!Enumerator.MoveNext())
                {
                    if (Storages.Count > 0)
                        Enumerator = Storages.Pop();
                    else { isBreak = true; break; }
                }
            }

            if (isBreak)
            {
                Coroutiner.Stop(this);
                return;
            }

            object current = Enumerator.Current;

            // 当中断点为空时跳过一帧
            if (current == null) return;

            // 如果当前对象为指定中断指令
            IYielInstruction yieldInstruction = current as IYielInstruction;
            if (yieldInstruction != null)
            {
                isWaiting = yieldInstruction.Await(deltaTime);
                return;
            }

            // 如果当前对象为新迭代器
            IEnumerator enumerator = current as IEnumerator;
            if (enumerator != null)
            {
                if (Storages.Count > 100)
                {
                    IoC.Resolve<ILoggerService>().Warning($"警告: 迭代器堆栈存放数据过大[{Storages.Count}]");
                }
                Storages.Push(Enumerator); // 保存现有迭代器
                Enumerator = enumerator;
                return;
            }
        }

        public void Stop()
        {
            IoC.Resolve<IDriverService>().Unsubscribe(Update);
        }
    }
}
