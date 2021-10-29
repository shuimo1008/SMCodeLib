using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace ZCSharpLib.Utils
{
    public class ReflUtils
    {
        /// <summary>
        /// 获取所有程序集的符合条件的类型
        /// </summary>
        /// <param name="match">查找条件</param>
        /// <returns>找到的类型</returns>
        public static Type[] GetAllTypes(Predicate<Type> match = null)
        {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly oAssembly = assemblies[i];
                Type[] nTypes = GetTypes(oAssembly, match);
                types.AddRange(nTypes);
            }
            return types.ToArray();
        }

        /// <summary>
        /// 获取指定程序集中符合条件的类型
        /// </summary>
        /// <param name="iAssembly">指定程序集</param>
        /// <param name="match">查找条件</param>
        /// <returns>找到的类型</returns>
        public static Type[] GetTypes(Assembly iAssembly, Predicate<Type> match = null)
        {
            List<Type> types = new List<Type>();
            Type[] oTypes = iAssembly.GetTypes();
            foreach (var item in oTypes)
            {
                bool isSucess = true;
                if (match != null) 
                    isSucess = match(item);
                if (isSucess) types.Add(item);
            }
            return types.ToArray();
        }

        /// <summary>
        /// IL创建一个对象
        /// </summary>
        /// <typeparam name="T">需要创建对象的类型</typeparam>
        /// <returns>创建的对象</returns>
        public static T ILCreateInstance<T>() where T : class, new()
        {
            Type type = typeof(T);
            return ILCreateInstance(type) as T;
        }

        /// <summary>
        /// IL创建一个对象
        /// </summary>
        /// <param name="type">需要创建对象的类型</param>
        /// <returns>对象</returns>
        public static object ILCreateInstance(Type type)
        {
            //ConstructorInfo defaultCtor = type.GetConstructor(new Type[] { });

            //System.Reflection.Emit.DynamicMethod dynMethod = new System.Reflection.Emit.DynamicMethod(
            //    name: string.Format("_{0:N}", Guid.NewGuid()),
            //    returnType: type,
            //    parameterTypes: null);

            //var gen = dynMethod.GetILGenerator();
            //gen.Emit(System.Reflection.Emit.OpCodes.Newobj, defaultCtor);
            //gen.Emit(System.Reflection.Emit.OpCodes.Ret);

            //return dynMethod.CreateDelegate(typeof(Func<T>)) as Func<T>;

            System.Reflection.Emit.DynamicMethod dm = new System.Reflection.Emit.DynamicMethod(string.Empty, typeof(object), Type.EmptyTypes);
            var gen = dm.GetILGenerator();
            gen.Emit(System.Reflection.Emit.OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
            gen.Emit(System.Reflection.Emit.OpCodes.Ret);
            return (Func<object>)dm.CreateDelegate(typeof(Func<object>));
        }


        public static object Construct(Type type, params object[] args)
        {
            return GetConstructor(type, args).Invoke(args);
        }

        /// <summary>
        /// 获取对应参数的构造函数
        /// </summary>
        /// <param name="type">类</param>
        /// <param name="args">构造参数</param>
        /// <returns></returns>
        public static ConstructorInfo GetConstructor(Type type, params object[] args)
        {
            ConstructorInfo oConstructorInfo = null;
            ConstructorInfo[] oConstructors = type.GetConstructors();
            for (int i = 0; i < oConstructors.Length; i++)
            {
                ConstructorInfo nConstructorInfo = oConstructors[i];
                ParameterInfo[] parameters = nConstructorInfo.GetParameters();
                if (parameters.Length == args.Length)
                {
                    bool flag = true;
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        ParameterInfo parameterInfo = parameters[i];
                        flag = parameterInfo.ParameterType == args[i].GetType();
                        if (!flag) break;
                    }
                    if (flag) oConstructorInfo = nConstructorInfo;
                    if (flag) break;
                }
            }
            if (oConstructorInfo == null)
            {
                throw new ArgumentException(string.Format("没有找到类型{0}的对应构造方法!\n{1}", type.Name, new StackTrace()));
            }
            return oConstructorInfo;
        }

        /// <summary>
        /// 获取所有字段名称
        /// </summary>
        public static string[] GetFields(Type iType, BindingFlags iBindFlags)
        {
            FieldInfo[] oFieldInfos = iType.GetFields(iBindFlags);
            string[] oFields = new string[oFieldInfos.Length];
            for (int i = 0; i < oFields.Length; i++)
                oFields[i] = oFieldInfos[i].Name;
            return oFields;
        }
    }
}
