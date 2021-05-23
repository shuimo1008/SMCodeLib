using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ZCSharpLib.Comms
{
    public static class AccessorExts
    {
        public static Func<T, U> BuildGetter<T, U>(Expression<Func<T, U>> iPropertySelector)
        {
            return iPropertySelector.GetPropertyInfo().GetGetMethod().CreateDelegate<Func<T, U>>();
        }

        public static Action<T, U> BuildSetter<T, U>(Expression<Func<T, U>> iPropertySelector)
        {
            return iPropertySelector.GetPropertyInfo().GetSetMethod(true).CreateDelegate<Action<T, U>>();
        }

        public static T CreateDelegate<T>(this MethodInfo method) where T : class
        {
            return Delegate.CreateDelegate(typeof(T), method) as T;
        }

        public static PropertyInfo GetPropertyInfo<T, U>(this Expression<Func<T, U>> iPropertySelector)
        {
            MemberExpression oBody = iPropertySelector.Body as MemberExpression;
            if (oBody == null) throw new MissingMemberException("this pargram is not MemberExpression type!");
            PropertyInfo oPropertyInfo = oBody.Member as PropertyInfo;
            if (oPropertyInfo == null) throw new NotSupportedException("not support field assignment, use property to instead!");
            return oBody.Member as PropertyInfo;
        }
    }
}
