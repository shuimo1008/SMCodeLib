using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SMCore.Utils
{
    public static class AccessorUtils
    {
        public static Func<T, U> BuildGetter<T, U>(Expression<Func<T, U>> propertySelector)
        {
            return propertySelector.GetPropertyInfo().GetGetMethod().CreateDelegate<Func<T, U>>();
        }

        public static Action<T, U> BuildSetter<T, U>(Expression<Func<T, U>> propertySelector)
        {
            return propertySelector.GetPropertyInfo().GetSetMethod(true).CreateDelegate<Action<T, U>>();
        }

        public static T CreateDelegate<T>(this MethodInfo method) where T : class
        {
            return Delegate.CreateDelegate(typeof(T), method) as T;
        }

        public static PropertyInfo GetPropertyInfo<T, U>(this Expression<Func<T, U>> propertySelector)
        {
            MemberExpression oBody = propertySelector.Body as MemberExpression;
            if (oBody == null) throw new MissingMemberException("this pargram is not MemberExpression type!");
            PropertyInfo oPropertyInfo = oBody.Member as PropertyInfo;
            if (oPropertyInfo == null) throw new NotSupportedException("not support field assignment, use property to instead!");
            return oBody.Member as PropertyInfo;
        }
    }
}
