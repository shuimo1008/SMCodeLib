using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class GenerateCode
    {
        public string REFERENCE =
            "using System;\r\n" +
            "using System.Collections.Generic;\r\n" +
            "using System.Linq;\r\n" +
            "using SMCore;\r\n" +
            "using SMCore.Cores;\r\n" +
            "using SMCore.Logger;\r\n";

        public string TMP_BASETPL =
            "public abstract class BaseTpl\r\n" +
            "__LK__ \r\n" +
            "   public int Tid __LK__ get; protected set; __RK__\r\n" +
            "   public abstract void SetupData(ByteBuffer buffer);\r\n" +
            "__RK__ \r\n";

        public string TMP_TEMPLATEMGR =
            "public class TemplateMgr<T> where T : BaseTpl, new()\r\n"+
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
            "           else IoC.Resolve<ILoggerS>().Error($\"模板__LK__typeof(T).Name__RK__Mgr已经包含Tid=__LK__t.Tid__RK__的对象!\");\r\n" +
            "       __RK__ \r\n" +
            "   __RK__ \r\n" +
            "\r\n" +
            "   public T Find(int id)\r\n" +
            "   __LK__ \r\n" +
            "       if (!DataTable.TryGetValue(id, out var tpl))\r\n" +
            "       __LK__\r\n" +
            "           IoC.Resolve<ILoggerS>().Error($\"模板__LK__typeof(T).Name__RK__Mgr没有包含Tid=__LK__id__RK__的对象!\");\r\n" +
            "       __RK__\r\n" +
            "       return tpl;\r\n" +
            "   __RK__ \r\n" +
            "   public T Find(Predicate<T> match)\r\n"+
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

        public string CLASSCONTENT1 =
            "public class Tpl\r\n" +
            "__LK__ \r\n" +
                "{0}"+
            "   public static void Setup(byte[] bytes)\r\n" +
            "   __LK__ \r\n" +
            "       ByteBuffer buffer = new ByteBuffer(bytes);\r\n" +
                    "{1}" +
            "    __RK__ \r\n" +
            "__RK__ \r\n";

        public string CLASSCONTENT2 =
            "/// <summary> \r\n" +
            "/// {0} \r\n" +
            "/// </summary> \r\n"+
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

        public string COMMENT =
            "   /// <summary> \r\n" +
            "   /// {0} \r\n" +
            "   /// </summary> \r\n";

        public string FIELDTYPE =
                "{0}" +
            "   public {1} {2} __LK__get; private set; __RK__ \r\n";

        public string Gen(List<FileData> fileDatas)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(REFERENCE);
            sb.Append("\r\n");

            // 数据设置
            StringBuilder sbTemplateFieldData = new StringBuilder();
            for (int fileIndex = 0; fileIndex < fileDatas.Count; fileIndex++)
            {
                FileData oFileData = fileDatas[fileIndex];
                string oTableName = oFileData.dataTable.TableName;
                sbTemplateFieldData.Append($"   public readonly static {oTableName}Mgr {oTableName}Mgr = new {oTableName}Mgr();\r\n");
            }

            StringBuilder sbTemplateSetupData = new StringBuilder();
            for (int fileIndex = 0; fileIndex < fileDatas.Count; fileIndex++)
            {
                FileData oFileData = fileDatas[fileIndex];
                string oTableName = oFileData.dataTable.TableName;
                sbTemplateSetupData.Append($"       {oTableName}Mgr.SetupData(buffer);\r\n");
            }
            sb.Append(string.Format(CLASSCONTENT1, sbTemplateFieldData, sbTemplateSetupData));
            sb.Append("\r\n");

            sb.Append(TMP_BASETPL);
            sb.Append("\r\n");

            sb.Append(TMP_TEMPLATEMGR);
            sb.Append("\r\n");

            for (int fileIndex = 0; fileIndex < fileDatas.Count; fileIndex++)
            {
                FileData oFileData = fileDatas[fileIndex];
                string oTableName = oFileData.dataTable.TableName;
                int rowCount = oFileData.dataTable.Rows.Count;
                int columnsCount = oFileData.dataTable.Columns.Count;

                // 获取数据类型
                string[] sTypes = new string[columnsCount];
                {
                    DataRow oDataRow = oFileData.dataTable.Rows[0]; // 数据类型在配置表第1行
                    for (int columnsIndex = 0; columnsIndex < sTypes.Length; columnsIndex++)
                    {
                        string strType = oDataRow[columnsIndex].ToString();
                        sTypes[columnsIndex] = strType;
                    }
                }
                // 获取字段名称
                string[] sFiledNames = new string[columnsCount];
                {
                    DataRow oDataRow = oFileData.dataTable.Rows[1];
                    for (int columnsIndex = 0; columnsIndex < sFiledNames.Length; columnsIndex++)
                    {
                        string fileName = oDataRow[columnsIndex].ToString();
                        sFiledNames[columnsIndex] = fileName;
                    }
                }
                // 获取字段注释
                string[] sComments = new string[columnsCount];
                {
                    DataRow oDataRow = oFileData.dataTable.Rows[2];
                    for (int columnsIndex = 0; columnsIndex < sComments.Length; columnsIndex++)
                    {
                        string comment = oDataRow[columnsIndex].ToString();
                        sComments[columnsIndex] = comment;
                    }
                }

                //sb.Append("\r\n");
                StringBuilder filedSB = new StringBuilder();
                StringBuilder methodSB = new StringBuilder();
                for (int i = 1; i < sTypes.Length; i++)
                {
                    // i=1 跳过第一行tplID代码的生成
                    string sType = sTypes[i];
                    string sFiledName = sFiledNames[i];
                    string sComment = sComments[i];

                    if (i > 0)
                    {
                        // 跳过ID字段
                        sComment = sComment.Replace("\r\n", "");
                        sComment = sComment.Replace("\n", "");
                        string combineComment = string.Format(COMMENT, sComment);
                        filedSB.Append(string.Format(FIELDTYPE, combineComment, sType, sFiledName));
                        //filedSB.Append("\r\n");
                    }
                    methodSB.Append(GetMethod(sType, sFiledName));
                    if (i < sTypes.Length - 1) methodSB.Append("\r\n");     // 添加换行
                }
                string sClass2 = CLASSCONTENT2.Replace("__CLASSNAME__", oTableName);
                sb.Append(string.Format(sClass2, oFileData.dataTable.Prefix, filedSB, methodSB));
                if (fileIndex < fileDatas.Count) sb.Append("\r\n");       // 添加换行
            }
            string final = sb.ToString();
            final = final.Replace("__LK__", "{");
            final = final.Replace("__RK__", "}");
            return final;
        }

        private string GetMethod(string sType, string fieldName)
        {
            string sMethod = string.Empty;
            if (sType.Equals("int")) sMethod = $"       {fieldName} = buffer.ReadInt32();";
            else if (sType.Equals("float")) sMethod = $"        {fieldName} = buffer.ReadFloat();";
            else if (sType.Equals("double")) sMethod = $"        {fieldName} = buffer.ReadDouble();";
            else if (sType.Equals("string")) sMethod = $"       {fieldName} = buffer.ReadUTF8();";
            else if (sType.Contains("[]"))
            {
                sMethod = sMethod +
                              $"       int o{fieldName}Count = buffer.ReadInt32();\r\n";
                if (sType.Equals("int[]"))
                {
                    sMethod = sMethod + 
                              $"       {fieldName} = new int[o{fieldName}Count];\r\n";
                    sMethod = sMethod + 
                              $"       for(int i = 0; i < o{fieldName}Count; i++)\r\n";
                    sMethod = sMethod +
                              $"       __LK__\r\n" +
                              $"          {fieldName}[i] = buffer.ReadInt32();\r\n" +
                              $"       __RK__";
                }
                else if (sType.Equals("float[]"))
                {
                    sMethod = sMethod + 
                              $"       {fieldName} = new float[o{fieldName}Count];\r\n";
                    sMethod = sMethod + 
                              $"       for(int i = 0; i < o{fieldName}Count; i++)\r\n";
                    sMethod = sMethod +
                              $"       __LK__\r\n" +
                              $"          {fieldName}[i] = buffer.ReadFloat();\r\n" +
                              $"       __RK__";
                }
                else if (sType.Equals("double[]"))
                {
                    sMethod = sMethod +
                              $"       {fieldName} = new double[o{fieldName}Count];\r\n";
                    sMethod = sMethod +
                              $"       for(int i = 0; i < o{fieldName}Count; i++)\r\n";
                    sMethod = sMethod +
                              $"       __LK__\r\n" +
                              $"          {fieldName}[i] = buffer.ReadDouble();\r\n" +
                              $"       __RK__";
                }
                else if (sType.Equals("string[]"))
                {
                    sMethod = sMethod + 
                              $"       {fieldName} = new string[o{fieldName}Count];\r\n";
                    sMethod = sMethod + 
                              $"       for(int i = 0; i < o{fieldName}Count; i++)\r\n";
                    sMethod = sMethod +
                              $"       __LK__\r\n" +
                              $"          {fieldName}[i] = buffer.ReadUTF8();\r\n" +
                              $"       __RK__";
                }
            }
            return sMethod;
        }
    }
}
