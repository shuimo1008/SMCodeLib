using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameApp
{
    public interface IBehaviour
    {
        void Start();
        void Update();
        void Disable();
    }

    public class GameApp
    {
        private static bool IsEnd { get; set; }
        private static IBehaviour[] Behaviours { get; set; }

        private static void Init()
        {
            List<IBehaviour> behaviours = new List<IBehaviour>();
            Assembly assembly = typeof(GameApp).Assembly;
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (type.IsClass && !type.IsAbstract &&
                    type.GetInterface(typeof(IBehaviour).Name) == typeof(IBehaviour))
                {
                    behaviours.Add(Activator.CreateInstance(type) as IBehaviour);
                }
            }
            Behaviours = behaviours.ToArray();
        }

        public static void Run()
        {
            Init();
            OnStart();
            while (!IsEnd)
            {
                OnUpdate();
                Thread.Sleep(10);
            }
            OnDisable();
        }

        static void OnStart()
        {
            for (int i = 0; i < Behaviours.Length; i++)
            {
                Behaviours[i].Start();
            }
        }

        static void OnUpdate()
        {
            for (int i = 0; i < Behaviours.Length; i++)
            {
                Behaviours[i].Update();
            }
        }

        static void OnDisable()
        {
            for (int i = 0; i < Behaviours.Length; i++)
            {
                Behaviours[i].Disable();
            }
        }

        public static void Quit()
        {
            IsEnd = true;
        }
    }
}
