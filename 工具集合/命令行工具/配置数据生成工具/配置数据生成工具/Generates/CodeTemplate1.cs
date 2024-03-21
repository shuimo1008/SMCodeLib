using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
    public class CodeTemplate1 : CodeTemplate
    {
        public override string REFERENCE =>
            "using System;\r\n" +
            "using System.Collections.Generic;\r\n" +
            "using System.Linq;\r\n" +
            "using SMCore;\r\n" +
            "using SMCore.Buffer;\r\n" +
            "using SMCore.Logger;\r\n";

        public override string TMP_BASETPL =>
            "public abstract class BaseTpl\r\n" +
            "__LK__ \r\n" +
            "   public int Tid __LK__ get; protected set; __RK__\r\n" +
            "   public abstract void SetupData(ByteBuffer buffer);\r\n" +
            "__RK__ \r\n";

        public override string TMP_TEMPLATEMGR =>
            "public class TemplateMgr<T> where T : BaseTpl, new()\r\n" +
            "__LK__ \r\n" +
            "   private readonly Dictionary<int, T> DataTable = new Dictionary<int, T>();\r\n" +
            "\r\n" +
            "   public void SetupData(ByteBuffer buffer) \r\n" +
            "   __LK__ \r\n" +
            "       int dataCount = buffer.ReadInt32(); \r\n" +
            "       for (int i = 0; i < dataCount; i++) \r\n" +
            "       __LK__ \r\n" +
            "           T t = new T();\r\n" +
            "           t.SetupData(buffer);\r\n" +
            "           if (!DataTable.ContainsKey(t.Tid)) DataTable.Add(t.Tid, t);\r\n" +
            "           else UnityEngine.Debug.LogError($\"模板__LK__typeof(T).Name__RK__Mgr已经包含Tid=__LK__t.Tid__RK__的对象!\");\r\n" +
            "       __RK__ \r\n" +
            "   __RK__ \r\n" +
            "\r\n" +
            "   public T Find(int id)\r\n" +
            "   __LK__ \r\n" +
            "       if (!DataTable.TryGetValue(id, out var tpl))\r\n" +
            "       __LK__\r\n" +
            "           UnityEngine.Debug.LogError($\"模板__LK__typeof(T).Name__RK__Mgr没有包含Tid=__LK__id__RK__的对象!\");\r\n" +
            "       __RK__\r\n" +
            "       return tpl;\r\n" +
            "   __RK__ \r\n" +
            "   public T Find(Predicate<T> match)\r\n" +
            "   __LK__\r\n" +
            "       return DataTable.Values.FirstOrDefault(t => match(t));\r\n" +
            "   __RK__\r\n" +
            "   public IList<T> FindAll(Predicate<T> match)\r\n" +
            "   __LK__ \r\n" +
            "       return DataTable.Values.Where(t => match(t)).ToList();\r\n" +
            "   __RK__ \r\n" +
            "\r\n" +
            "   private IList<T> datas;\r\n" +
            "   public IList<T> Datas => datas ?? (datas = DataTable.Values.ToList());\r\n" +
            "\r\n" +
            "__RK__ \r\n";

        public override string CLASSCONTENT1 =>
            "public class Tpl\r\n" +
            "__LK__ \r\n" +
                "{0}" +
            "   public static void Setup(byte[] bytes)\r\n" +
            "   __LK__ \r\n" +
            "       ByteBuffer buffer = new ByteBuffer(bytes);\r\n" +
                    "{1}" +
            "    __RK__ \r\n" +
            "__RK__ \r\n";

        public override string CLASSCONTENT2 =>
            "/// <summary> \r\n" +
            "/// {0} \r\n" +
            "/// </summary> \r\n" +
            "public class __CLASSNAME__Mgr\r\n" +
            "   : TemplateMgr<__CLASSNAME__Tpl>\r\n" +
            "__LK__ \r\n" +
            "__RK__ \r\n" +
            "public class __CLASSNAME__Tpl : BaseTpl\r\n" +
            "__LK__ \r\n" +
                "{1}\r\n" +
            "   public override void SetupData(ByteBuffer buffer)\r\n" +
            "   __LK__ \r\n" +
            "       Tid = buffer.ReadInt32();\r\n" +
                    "{2} \r\n" +
            "   __RK__ \r\n" +
            "__RK__ \r\n";

        public override string COMMENT =>
            "   /// <summary> \r\n" +
            "   /// {0} \r\n" +
            "   /// </summary> \r\n";

        public override string FIELDTYPE =>
                "{0}" +
            "   public {1} {2} __LK__get; private set; __RK__ \r\n";
    }
}
