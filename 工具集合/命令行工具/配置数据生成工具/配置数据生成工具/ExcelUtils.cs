using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;


namespace Tools
{
    public class ExcelUtils
    {
        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="data">要导入的数据</param>
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>
        /// <param name="sheetName">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public static int DataTableToExcel(string fileName, DataTable data, string sheetName, bool isColumnWritten)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (fileName.IndexOf(".xlsx", StringComparison.Ordinal) > 0)    // 2007版本
                    workbook = new XSSFWorkbook();
                else if (fileName.IndexOf(".xls", StringComparison.Ordinal) > 0) // 2003版本
                    workbook = new HSSFWorkbook();

                try
                {
                    ISheet sheet;
                    if (workbook != null)
                    {
                        sheet = workbook.CreateSheet(sheetName);
                    }
                    else return -1;

                    int i = 0, j = 0, count = 0;

                    //写入DataTable的列名
                    if (!isColumnWritten)
                        count = 0;
                    else
                    {
                        IRow row = sheet.CreateRow(0);
                        for (j = 0; j < data.Columns.Count; ++j)
                        {
                            row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                        }
                        count = 1;
                    }

                    for (i = 0; i < data.Rows.Count; ++i)
                    {
                        IRow row = sheet.CreateRow(count);
                        for (j = 0; j < data.Columns.Count; ++j)
                        {
                            row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                        }
                        ++count;
                    }
                    workbook.Write(fs); //写入到excel
                    return count;
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception: " + ex.Message);
                    return -1;
                }
            }
        }

        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public static DataTable ExcelToDataTable(string fileName, string sheetName, bool isFirstRowColumn)
        {
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = null;
                    if (fileName.IndexOf(".xlsx", StringComparison.Ordinal) > 0) // 2007版本
                        workbook = new XSSFWorkbook(fs);
                    else if (fileName.IndexOf(".xls", StringComparison.Ordinal) > 0) // 2003版本
                        workbook = new HSSFWorkbook(fs);

                    if (workbook == null)
                        return null;

                    ISheet sheet = null;
                    if (sheetName != null)
                    {
                        //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                        sheet = workbook.GetSheet(sheetName) ?? workbook.GetSheetAt(0);
                    }
                    else sheet = workbook.GetSheetAt(0);

                    DataTable dataTable = new DataTable();
                    dataTable.Prefix = sheet.SheetName;

                    IRow firstRow = sheet.GetRow(1); // 实际表格中第二行为字段,所以这里GetRow=1
                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    dataTable.Columns.Add(column);
                                }
                            }
                            //if (cell != null)
                            //{
                            //    DataColumn column = new DataColumn(i.ToString());
                            //    dataTable.Columns.Add(column);
                            //}
                        }
                    }

                    int startRow = 0;
                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = dataTable.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                            {
                                ICell cell = row.GetCell(j);

                                switch (cell.CellType)
                                {
                                    case CellType.String:
                                        dataRow[j] = cell.StringCellValue;
                                        break;
                                    case CellType.Numeric:
                                        dataRow[j] = cell.NumericCellValue;
                                        break;
                                    case CellType.Formula:
                                        dataRow[j] = cell.NumericCellValue;
                                        break;
                                    default:
                                        dataRow[j] = cell.ToString();
                                        break;
                                }
                            }
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            return null;
        }
    }
}
