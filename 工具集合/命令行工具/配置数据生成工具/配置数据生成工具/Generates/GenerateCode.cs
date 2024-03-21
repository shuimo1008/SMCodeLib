using Newtonsoft.Json;
using NPOI.SS.Formula.PTG;
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
        public string Gen(List<FileData> fileDatas)
        {
            // 判断是否String类型的ID
            bool isSid = false;
            if (fileDatas.Count > 0)
            {
                // 获取数据类型
                DataRow oDataRow = fileDatas[0].dataTable.Rows[0]; // 数据类型在配置表第1行
                string strType = oDataRow[0].ToString();
                isSid = strType.ToLower().Equals("string");
            }

            CodeTemplate codeTemplate = new CodeTemplate1();
            if (isSid) codeTemplate = new CodeTemplate2();

            StringBuilder sb = new StringBuilder();
            sb.Append(codeTemplate.REFERENCE);
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
            
            sb.Append(string.Format(codeTemplate.CLASSCONTENT1, sbTemplateFieldData, sbTemplateSetupData));
            sb.Append("\r\n");

            sb.Append(codeTemplate.TMP_BASETPL);
            sb.Append("\r\n");

            sb.Append(codeTemplate.TMP_TEMPLATEMGR);
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
                        string strType = oDataRow[columnsIndex].ToValue().value.ToString();
                        sTypes[columnsIndex] = strType;
                    }
                }
                // 获取字段名称
                string[] sFiledNames = new string[columnsCount];
                {
                    DataRow oDataRow = oFileData.dataTable.Rows[1];
                    for (int columnsIndex = 0; columnsIndex < sFiledNames.Length; columnsIndex++)
                    {
                        string fileName = oDataRow[columnsIndex].ToValue().value.ToString();
                        sFiledNames[columnsIndex] = fileName;
                    }
                }
                // 获取字段注释
                string[] sComments = new string[columnsCount];
                {
                    DataRow oDataRow = oFileData.dataTable.Rows[2];
                    for (int columnsIndex = 0; columnsIndex < sComments.Length; columnsIndex++)
                    {
                        CellValue cellValue = oDataRow[columnsIndex].ToValue();
                        string comment = cellValue.value.ToString();
                        if (!string.IsNullOrEmpty(cellValue.comment))
                        {
                            string cellComment = cellValue.comment;
                            cellComment = cellComment.Replace("\t", "");
                            cellComment = cellComment.Replace("\n", "__Enter__");
                            comment += "__Enter__" + cellComment;
                        }
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
                        sComment = sComment.Replace("__Enter__", $"\n   /// ");
                        string combineComment = string.Format(codeTemplate.COMMENT, sComment);
                        filedSB.Append(string.Format(codeTemplate.FIELDTYPE, combineComment, sType, sFiledName));
                        //filedSB.Append("\r\n");
                    }
                    methodSB.Append(GetMethod(sType, sFiledName));
                    if (i < sTypes.Length - 1) methodSB.Append("\r\n");     // 添加换行
                }
                string sClass2 = codeTemplate.CLASSCONTENT2.Replace("__CLASSNAME__", oTableName);
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
