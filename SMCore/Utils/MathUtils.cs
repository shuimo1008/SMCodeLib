using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Utils
{
    public class MathUtils
    {
        public static bool IsZero(float v)
        {
            return Math.Abs(v) < 0.0001f;
        }

        public static bool Equal(float v1, float v2)
        {
            return Math.Abs(v1 - v2) < 0.001f;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            else if (value > max) return max;
            return value;
        }

        public static float BezierCurve(float t, float p0, float p1, float p2)
        {
            float f0 = (float)Math.Pow((1 - t), 2) * p0;
            float f1 = 2 * t * (1 - t) * p1;
            float f2 = t * t * p2;
            return f0 + f1 + f2;
        }


        public static float BezierCurve(float t, float p0, float p1, float p2, float p3)
        {
            float f0 = p0 * (float)Math.Pow(1 - t, 3);
            float f1 = 3 * p1 * t * (float)Math.Pow(1 - t, 2);
            float f2 = 3 * p2 * t * t * (1 - t);
            float f3 = p3 * t * t * t;
            return f0 + f1 + f2 + f3;
        }
    }
}
