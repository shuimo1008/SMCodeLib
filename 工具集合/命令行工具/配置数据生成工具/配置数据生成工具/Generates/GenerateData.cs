using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class GenerateData
    {
        public byte[] Gen(List<DataTable> dataTables)
        {
            ByteBuffer buffer = new ByteBuffer();
            for (int tableIndex = 0; tableIndex < dataTables.Count; tableIndex++)
            {
                DataTable oDataTable = dataTables[tableIndex];

                Logger.Info($"打包数据:<{oDataTable.TableName}>");

                int rowCount = oDataTable.Rows.Count;
                int realyRowCount = Math.Max((rowCount - 3), 0);

                // 写入行数
                buffer.WriteInt32(realyRowCount); 

                // 如果真实数据是零行则略过
                if (realyRowCount == 0) continue;

                int columnsCount = oDataTable.Columns.Count;
                // 记录类型数据
                string[] strTypes = new string[columnsCount];
                {
                    DataRow oDataRow = oDataTable.Rows[0]; // 类型数据在第1行
                    for (int columnsIndex = 0; columnsIndex < strTypes.Length; columnsIndex++)
                    {
                        string strType = oDataRow[columnsIndex].ToString();
                        strTypes[columnsIndex] = strType;
                    }
                }
                // 写入实际数据
                for (int rowIndex = 3; rowIndex < rowCount; rowIndex++)
                {
                    DataRow oDataRow = oDataTable.Rows[rowIndex];
                    for (int columnIndex = 0; columnIndex < columnsCount; columnIndex++)
                    {
                        try
                        {
                            object obj = oDataRow[columnIndex];

                            string strType = strTypes[columnIndex];
                            if (strType.Equals("int"))
                            {
                                buffer.WriteInt32(GuardInt(obj));
                            }
                            else if (strType.Equals("float"))
                            {
                                buffer.WriteFloat(GuardFloat(obj));
                            }
                            else if (strType.Equals("string"))
                            {
                                buffer.WriteUTF8(GuardString(obj));
                            }
                            else if (strType.Contains("[]"))
                            {
                                string value = string.Empty;
                                if (obj != null) value = obj.ToString();
                                string[] values = value.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
                                buffer.WriteInt32(values.Length); // 写入数组长度
                                if (strType.Equals("int[]"))
                                {
                                    foreach (var t in values) buffer.WriteInt32(GuardInt(t));
                                }
                                else if (strType.Equals("float[]"))
                                {
                                    foreach (var t in values) buffer.WriteFloat(GuardFloat(t));
                                }
                                else if (strType.Equals("string[]"))
                                {
                                    foreach (var t in values) buffer.WriteUTF8(GuardString(t));
                                }
                            }
                        }
                        catch (FormatException e)
                        {
                            Logger.Error("数据生成时<{0}>表 第{1}行,第{2}列出现数据格式解析错误!\n{3}", Path.GetFileNameWithoutExtension(oDataTable.Prefix), rowIndex + 1, columnIndex + 1, e.ToString());
                        }
                    }
                }
            }
            return buffer.Bytes;
        }

        public int GuardInt(object obj)
        {
            if (obj == null) return 0;
            string value = obj.ToString().Trim();
            if (string.IsNullOrEmpty(value)) return 0;
            return int.Parse(value);
        }

        public float GuardFloat(object obj)
        {
            if (obj == null) return 0.0f;
            string value = obj.ToString().Trim();
            if (string.IsNullOrEmpty(value)) return 0.0f;
            return float.Parse(value);
        }

        public string GuardString(object obj)
        {
            if (obj == null)
                return string.Empty;
            return obj.ToString();
        }
    }
}
