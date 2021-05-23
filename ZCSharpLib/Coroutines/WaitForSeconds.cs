using ZCSharpLib.Utils;

namespace ZCSharpLib.Coroutines.Build
{
    public class WaitForSeconds: IInterruptInstruction
    {
        private float UseTime { get; set; }
        private float OwnTime { get; set; }

        public WaitForSeconds(float time)
        {
            OwnTime = time;
        }

        public bool Await(float deltaTime)
        {
            UseTime = MathUtils.Clamp(UseTime + deltaTime, 0, OwnTime);
            if (UseTime == OwnTime) 
                return false;
            else return true;
        }
    }
}
