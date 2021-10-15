using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Tools
{
    public class FileData
    {
        public string file;
        public string fileName;
        public DataTable dataTable;
    }

    public class ProcessExcel
    {
        public List<FileData> FileDatas { get; private set; }

        public ProcessExcel()
        {
            FileDatas = new List<FileData>();
        }

        public bool Parse(string[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                file = file.Replace("\\", "/");
                string fileName = Path.GetFileNameWithoutExtension(file);
                Logger.Info($"解析文件:{file}");
                DataTable oDataTable = ExcelUtils.ExcelToDataTable(file, null, true);
                oDataTable.TableName = fileName;
                if (oDataTable != null) FileDatas.Add(new FileData() { file = file, fileName = fileName, dataTable = oDataTable });
                else return false;
            }

            FileDatas.Sort((a, b) => 
                string.Compare(a.fileName, b.fileName, StringComparison.Ordinal));

            return true;
        }

        public bool Check()
        {
            bool isPass = true;
            for (int fileIndex = 0; fileIndex < FileDatas.Count; fileIndex++)
            {
                FileData oFileData = FileDatas[fileIndex];
                Logger.Info($"检查文件:{oFileData.file}");
                int rowCount = oFileData.dataTable.Rows.Count;
                if (rowCount < 3) 
                {
                    Logger.Error("<{0}>表配置定义不全!", oFileData.dataTable.TableName);
                    isPass = false;
                    continue; // 如果没有元数据则不再对当前表进行检查
                }
                int columnsCount = oFileData.dataTable.Columns.Count;

                {
                    // 第1行数据检查
                    DataRow oDataRow = oFileData.dataTable.Rows[0];
                    for (int columnsIndex = 0; columnsIndex < columnsCount; columnsIndex++)
                    {
                        if (oDataRow.IsNull(columnsIndex))
                        {
                            Logger.Error("<{0}>表第{1}行, 第{2}列数据为空", oFileData.dataTable.Prefix, 1, columnsIndex + 1);
                            isPass = false;
                        }
                        else
                        {
                            string strType = oDataRow[columnsIndex].ToString();

                            if (columnsIndex == 0)
                            {
                                if (!strType.Equals("int"))
                                {
                                    Logger.Error("<{0}>表第一列必须是int类型的唯一ID", oFileData.dataTable.Prefix);
                                    isPass = false;
                                }
                            }
                            else
                            {
                                bool isType = false;
                                if (strType.Equals("int")) isType = true;
                                else if (strType.Equals("float")) isType = true;
                                else if (strType.Equals("double")) isType = true;
                                else if (strType.Equals("string")) isType = true;
                                else if (strType.Equals("int[]")) isType = true;
                                else if (strType.Equals("float[]")) isType = true;
                                else if (strType.Equals("double[]")) isType = true;
                                else if (strType.Equals("string[]")) isType = true;
                                if (!isType)
                                {
                                    Logger.Error("<{0}>表第{1}行，第{2}列数据数据异常，数据类型只能为int;float;double;string;int[];float[];double[];string[]这几种类型!", oFileData.dataTable.Prefix, 1, columnsIndex + 1);
                                    isPass = false;
                                }
                            }
                        }
                    }
                    if (!isPass) continue; // 如果首行有数据异常则不再对这张表进行检查 
                }

                {
                    // 第2行数据检查
                    List<string> oFiledNames = new List<string>();
                    DataRow oDataRow = oFileData.dataTable.Rows[1];
                    for (int columnsIndex = 0; columnsIndex < columnsCount; columnsIndex++)
                    {
                        if (oDataRow.IsNull(columnsIndex))
                        {
                            Logger.Error("<{0}>表第{1}行,第{2}列数据为空,", oFileData.dataTable.Prefix, 2, columnsIndex + 1);
                            isPass = false;
                        }
                        else
                        {
                            string strType = oDataRow[columnsIndex].ToString();
                            if (oFiledNames.IndexOf(strType) != -1)
                            {
                                Logger.Error("<{0}>表第{1}行,第{2}列与已有的字段名重复.", oFileData.dataTable.Prefix, 2, columnsIndex + 1);
                                isPass = false;
                            }
                            oFiledNames.Add(strType);
                        }
                    }
                    if (!isPass) continue; // 如果第1行有数据异常则不再对这张表进行检查 
                }

                {
                    // 第三行数据略过
                }

                {
                    // 从第四行数据开始检查

                    // 记录列数据类型 --数据类型在数据生成时进行检查(2020.07.22 author:yunchen1008)
                    string[] strTypes = new string[columnsCount];
                    {
                        DataRow oDataRow = oFileData.dataTable.Rows[0]; // 配置表第1行为数据类型
                        for (int columnsIndex = 0; columnsIndex < columnsCount; columnsIndex++)
                        {
                            string strType = oDataRow[columnsIndex].ToString();
                            strTypes[columnsIndex] = strType;
                        }
                    }
                    for (int rowIndex = 3; rowIndex < rowCount; rowIndex++)
                    {
                        DataRow oDataRow = oFileData.dataTable.Rows[rowIndex];
                        object obj = oDataRow[0];
                        if (obj == null)
                        {
                            Logger.Error("<{0}>表第{1}行,首列ID数据不能为null.", oFileData.dataTable.TableName, rowIndex + 1);
                            isPass = false;
                            continue;
                        }
                        if (string.IsNullOrEmpty(obj.ToString()))
                        {
                            Logger.Error("<{0}>表第{1}行,首列ID数据不能为空字符.", oFileData.dataTable.TableName, rowIndex + 1);
                            isPass = false;
                        }

                        //for (int columnsIndex = 0; columnsIndex < columnsCount; columnsIndex++)
                        //{
                        //    if (oDataRow.IsNull(columnsIndex) &&
                        //        (strTypes[columnsIndex].Equals("int") ||
                        //         strTypes[columnsIndex].Equals("float")))
                        //    {
                        //        Logger.Error("<{0}>表第{1}行,第{2}列数据不能为空.", oDataTable.TableName, rowIndex + 1, columnsIndex + 1);
                        //        isPass = false;
                        //    }
                        //    if (!oDataRow.IsNull(columnsIndex))
                        //    {
                        //        object oValue = oDataRow[columnsIndex];
                        //        if (strTypes[columnsIndex].Equals("int"))
                        //        {
                        //            int value = 0;
                        //            if (!int.TryParse(oValue.ToString(), out value))
                        //            {
                        //                Logger.Error("<{0}>表第{1}行,第{2}列数据与定义的int类型不符.", oDataTable.TableName, rowIndex + 1, columnsIndex + 1);
                        //                isPass = false;
                        //            }
                        //        }
                        //        else if (strTypes[columnsIndex].Equals("float"))
                        //        {
                        //            float value = 0;
                        //            if (!float.TryParse(oValue.ToString(), out value))
                        //            {
                        //                Logger.Error("<{0}>表第{1}行,第{2}列数据与定义的float类型不符.", oDataTable.TableName, rowIndex + 1, columnsIndex + 1);
                        //                isPass = false;
                        //            }
                        //        }
                        //    }
                        //}
                    }
                }
            }
            return isPass;
        }
    }
}
