
using System;

namespace SMCore.Coroutines.Build
{
    public class WaitForSecondsSM : IYielInstruction
    {
        private float UseTime { get; set; }
        private float OwnTime { get; set; }

        public WaitForSecondsSM(float time)
        {
            OwnTime = time;
        }

        public bool Await(float deltaTime)
        {
            UseTime = Clamp(UseTime + deltaTime, 0, OwnTime);
            if (UseTime == OwnTime) return false;
            else return true;
        }

        private float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
