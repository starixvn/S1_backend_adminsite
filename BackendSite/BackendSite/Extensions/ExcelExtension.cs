using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using BackendSite.Attributes;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace BackendSite.Extensions
{
    public static class ExcelExtension
    {
        /// <summary>
        /// 依 Class 建立 Header
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sheet"></param>
        /// <param name="freeze">是否凍結窗格</param>
        public static void CreateHeaders<T>(this ISheet sheet, bool freeze = false)
            where T: class
        {
            var headerRow = sheet.CreateRow(0);
            var props = typeof(T).GetProperties();

            var index = 0;
            foreach (var prop in props)
            {
                if (prop.ExcelIgnore())
                {
                    continue;
                }

                var name = prop.GetDisplayName();
                headerRow.CreateCell(index).SetCellValue(name);
                index++;
            }

            // 凍結窗格
            if (freeze)
                sheet.CreateFreezePane(0, 1);
        }

         public static void SetDropDownList(this ISheet sheet, string[] datas,
            int firstRow, int lastRow, int firstCol, int lastCol)
        {
            var dvHelper = sheet.GetDataValidationHelper();
            var cellRange = new CellRangeAddressList(firstRow, lastRow, firstCol, lastCol);
            var constraints = dvHelper.CreateExplicitListConstraint(datas);
            var dataValidation = dvHelper.CreateValidation(constraints, cellRange);
            dataValidation.SuppressDropDownArrow = true;

            sheet.AddValidationData(dataValidation);
        }

        public static string CellItemToString(this ICell cell)
        {
            return cell?.ToString().Trim() ?? string.Empty;
        }

        public static decimal CellItemToDecimal(this ICell cell)
        {
            return cell?.NumericCellValue.ToDecimal() ?? 0m;
        }
        
        public static int CellItemToInt(this ICell cell)
        {
            return cell?.NumericCellValue.ToInt() ?? 0;
        }

        public static byte[] ToExcelFile<T>(this IEnumerable<T> list, string sheetName, bool freeze = false)
            where T : class
        {
            var workbook = new XSSFWorkbook();
            var sheet1 = workbook.CreateSheet(sheetName);

            WriteToSheet(sheet1, list, freeze);

            using var memoryStream = new MemoryStream();
            workbook.Write(memoryStream);
            return memoryStream.GetBuffer();
        }

        public static void WriteToSheet<T>(this ISheet sheet, IEnumerable<T> list, bool freeze = false)
            where T : class
        {
            sheet.CreateHeaders<T>(freeze);
            var rowTitle = sheet.CreateRow(1);

            if (!list.Any())
            {
                rowTitle.CreateCell(0).SetCellValue("No Data");
            }
            else
            {
                var props = list.First().GetType().GetProperties();

                var index = 1;
                foreach (var item in list)
                {
                    var row = sheet.CreateRow(index);

                    var i = 0;
                    foreach (var prop in props)
                    {
                        if (prop.ExcelIgnore())
                        {
                            continue;
                        }
                        
                        var val = item.GetType().GetProperty(prop.Name)?.GetValue(item);
                        if (val is DateTimeOffset offset && offset.Year > 1)
                        {
                            offset = new DateTimeOffset(offset.DateTime, TimeSpan.FromHours(8));
                            row.CreateCell(i).SetCellValue($"{offset:MM/dd/yyyy h:mm:ss tt}");
                        }
                        else
                        {
                            row.CreateCell(i).SetCellValue(val?.ToString() ?? string.Empty);
                        }
                        
                        i++;
                    }
                    index++;
                }
            }
        }
        
        private static decimal ToDecimal(this object value)
        {
            if (value == null)
                return default;
            
            decimal.TryParse(value.ToString()
                .Replace(",", string.Empty), out decimal result);
            
            return result;
        }
        
        private static int ToInt(this object value)
        {
            if (value == null)
                return default;
            
            int.TryParse(value.ToString()
                .Replace(",", string.Empty), out int result);
            
            return result;
        }
    
        private static bool ExcelIgnore(this ICustomAttributeProvider propertyInfo)
        {
            var attributes = (ExportIgnoreAttribute[])propertyInfo
                .GetCustomAttributes(typeof(ExportIgnoreAttribute), false);
     
            return attributes.Length > 0;
        }
        
        private static string GetDisplayName(this PropertyInfo propertyInfo)
        {
            var attributes = (DisplayNameAttribute[])propertyInfo
                .GetCustomAttributes(typeof(DisplayNameAttribute), false);
            
            return attributes.Length > 0 
                ? attributes.First().DisplayName
                : propertyInfo.Name;
        }
    }
}