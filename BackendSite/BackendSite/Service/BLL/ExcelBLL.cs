using System.Reflection;

using BackendSite.Attributes;
using BackendSite.Service.Model;

using Microsoft.AspNetCore.Mvc;

using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BackendSite.Service.BLL
{
    public class ExportDataToExcel
    {
        public string SheetName { get; set; }
        public IEnumerable<Dictionary<string, dynamic>> Content { get; set; }
        public List<string> Titles { get; set; }

    }

    public class ExcelBLL
    {
        private readonly List<string> _excelContentType = new List<string>()
        {
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-excel",
            "application/octet-stream"
        };
        public ExcelBLL()
        {

        }

        public bool IsValidFile(string contentType)
        {
            return _excelContentType.Exists(a => a.Equals(contentType, StringComparison.OrdinalIgnoreCase));
        }

        public List<ExcelData> Read(IFormFile formFile)
        {
            var excelList = new List<ExcelData>();
            var excelPkg = new ExcelPackage(formFile.OpenReadStream());
            var sheet = excelPkg.Workbook.Worksheets.First(); //取得Sheet1
            var isLastRow = false;
            var rowId = 2; // 因為有標題列，所以從第2列開始讀起

            do // 讀取資料，直到讀到空白列為止
            {
                var custUsername = sheet.Cells[rowId, 1].Text.Trim();
                var sabaId = sheet.Cells[rowId, 2].Text.Trim();
                var tmp = sabaId.Split('_');
                int? custId = (tmp.Length == 2 && int.TryParse(tmp[1], out var id)) ? id : null;

                if (string.IsNullOrEmpty(custUsername) && custId == null)
                {
                    isLastRow = true;
                }
                else
                {
                    excelList.Add(new ExcelData
                    {
                        CustId = custId,
                        CustUserName = string.IsNullOrEmpty(custUsername) ? null : custUsername,
                        SabaId = sabaId,
                        RowId = rowId,
                    });
                    rowId += 1;
                }
            } while (!isLastRow);

            return excelList;
        }

        public byte[] ExportToMultiSheet(List<ExportDataToExcel> t)
        {
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);
            if (t.Any())
            {
                foreach (var s in t)
                {
                    var ws = p.Workbook.Worksheets.Add(s.SheetName);
                    if (s.Content.Any())
                    {
                        ExportBody(ws, s.Content, s.Titles, 2);
                        ExportHeader(ws, s.Titles, 1);
                    }
                }
            }

            p.Save();
            return ms.ToArray();
        }

        private void ExportBody(ExcelWorksheet ws,
                                IEnumerable<Dictionary<string, dynamic>> data,
                                List<string> titles,
                                int currentRow)
        {

            for (var i = 0; i < data.Count(); i++)
            {
                var bt = data.ElementAt(i);
                foreach (var item in bt)
                {
                    var value = item.Value;
                    var colName = item.Key;
                    if (titles.Any(x => string.Equals(x, colName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var colIdx = titles.IndexOf(colName) + 1;
                        if (value != null && value.GetType() == typeof(DateTime) && ((DateTime)value).Year > 1)
                        {
                            var time = new DateTimeOffset((DateTime)value);
                            ws.Cells[currentRow, colIdx].Value = time.ToOffset(TimeSpan.FromHours(8)).DateTime;
                            ws.Cells[currentRow, colIdx].Style.Numberformat.Format = "mm/dd/yyyy h:mm:ss\\ AM/PM";
                        }
                        else
                        {
                            ws.Cells[currentRow, colIdx].Value = value;
                        }
                    }
                }

                currentRow++;
            }

        }

        private void ExportHeader(ExcelWorksheet ws,
                                  IEnumerable<string> titles,
                                  int currentRow)
        {
            SetHeader(ws, titles, currentRow);
        }

        public virtual byte[] Export<T>(IEnumerable<T> t, IEnumerable<PropertyInfo> propertyInfo = null, TimeSpan? offset = null) where T : notnull, new()
        {
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);
            var ws = p.Workbook.Worksheets.Add(nameof(t));

            if (t.Any())
            {
                if (propertyInfo == null)
                {
                    propertyInfo = t.FirstOrDefault().GetType().GetProperties()
                    	.Where(x => !Attribute.IsDefined(x, typeof(ExportIgnoreAttribute)));
                }
                propertyInfo = propertyInfo.Where(x => !Attribute.IsDefined(x, typeof(ExportIgnoreAttribute)));

                ExportBody(ws, t, propertyInfo, offset);
                ExportHeader(ws, propertyInfo);
            }
            p.Save();
            return ms.ToArray();
        }

        public virtual byte[] ExportV2<T>(IEnumerable<T> t, IEnumerable<PropertyInfo> properties = null) where T : notnull, new()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);
            var ws = p.Workbook.Worksheets.Add(nameof(t));

            if (t.Any())
            {
                if (properties == null)
                {
                    properties = t.FirstOrDefault().GetType().GetProperties();
                }

                var rows = t.Count() + 1;
                var cols = properties.Count();
                if (rows > 0 && cols > 0)
                {
                    ws.Cells[1, 1].LoadFromCollection(t, true);

                    using (var range = ws.Cells[1, 1, rows, cols])
                    {
                        // 標題列
                        var title = ws.Cells[1, 1, 1, cols];
                        title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        title.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                        title.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                    }
                }
            }
            p.Save();
            return ms.ToArray();
        }

        public virtual byte[] ExportV2<T>(IEnumerable<T> t, int ignoreColumnCount, IEnumerable<PropertyInfo> properties = null) where T : notnull, new()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);
            var ws = p.Workbook.Worksheets.Add(nameof(t));

            if (t.Any())
            {
                if (properties == null)
                {
                    properties = t.FirstOrDefault().GetType().GetProperties();
                }

                var rows = t.Count() + 1;
                var cols = properties.Count() - ignoreColumnCount;
                if (rows > 0 && cols > 0)
                {
                    ws.Cells[1, 1].LoadFromCollection(t, true);

                    using (var range = ws.Cells[1, 1, rows, cols])
                    {
                        // 標題列
                        var title = ws.Cells[1, 1, 1, cols];
                        title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        title.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                        title.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                    }
                }
            }
            p.Save();
            return ms.ToArray();
        }


        public virtual byte[] ExportV2(IEnumerable<Dictionary<string, object>> data)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);
            var ws = p.Workbook.Worksheets.Add("data");

            if (data.Any())
            {
                var rows = data.Count() + 1;
                var cols = data.FirstOrDefault().Count;
                if (rows > 0 && cols > 0)
                {
                    ws.Cells[1, 1].LoadFromDictionaries(data, true);

                    using (var range = ws.Cells[1, 1, rows, cols])
                    {
                        // 標題列
                        var title = ws.Cells[1, 1, 1, cols];
                        title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        title.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                        title.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                    }
                }
            }
            p.Save();
            return ms.ToArray();
        }

        public byte[] ExportV2<T>(Dictionary<string, IEnumerable<T>> t, IEnumerable<PropertyInfo> properties = null) where T : notnull, new()
        {
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);
            foreach (var item in t)
            {
                var ws = p.Workbook.Worksheets.Add(item.Key);

                if (item.Value.Any())
                {
                    if (properties == null)
                    {
                        properties = item.Value.FirstOrDefault().GetType().GetProperties();
                    }

                    var rows = item.Value.Count() + 1;
                    var cols = properties.Count();
                    if (rows > 0 && cols > 0)
                    {
                        ws.Cells[1, 1].LoadFromCollection(item.Value, true);

                        using (var range = ws.Cells[1, 1, rows, cols])
                        {
                            // 標題列
                            var title = ws.Cells[1, 1, 1, cols];
                            title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            title.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                            title.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                        }
                    }
                }
            }
            p.Save();
            return ms.ToArray();
        }

        public byte[] ExportV2<T, S>(Dictionary<string, IEnumerable<T>> t, Dictionary<string, IEnumerable<S>> s, IEnumerable<PropertyInfo> properties = null)
        {
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);
            foreach (var item in t)
            {
                var ws = p.Workbook.Worksheets.Add(item.Key);

                if (item.Value.Any())
                {
                    if (properties == null)
                    {
                        properties = item.Value.FirstOrDefault().GetType().GetProperties();
                    }
                    var rows = item.Value.Count() + 1;
                    var cols = properties.Count();
                    if (rows > 0 && cols > 0)
                    {
                        ws.Cells[1, 1].LoadFromCollection(item.Value, true);

                        using (var range = ws.Cells[1, 1, rows, cols])
                        {
                            // 標題列
                            var title = ws.Cells[1, 1, 1, cols];
                            title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            title.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                            title.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                        }
                    }

                }
            }
            foreach (var item in s)
            {
                var ws = p.Workbook.Worksheets.Add(item.Key);

                if (item.Value.Any())
                {
                    if (properties == null)
                    {
                        properties = item.Value.FirstOrDefault().GetType().GetProperties();
                    }
                    var rows = item.Value.Count() + 1;
                    var cols = properties.Count();
                    if (rows > 0 && cols > 0)
                    {
                        ws.Cells[1, 1].LoadFromCollection(item.Value, true);

                        using (var range = ws.Cells[1, 1, rows, cols])
                        {
                            // 標題列
                            var title = ws.Cells[1, 1, 1, cols];
                            title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            title.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                            title.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                        }
                    }

                }
            }
            p.Save();
            return ms.ToArray();
        }

        public byte[] ExportV2<T, S, U>(Dictionary<string, IEnumerable<T>> t, Dictionary<string, IEnumerable<S>> s, Dictionary<string, IEnumerable<U>> u, IEnumerable<PropertyInfo> properties = null)
        {
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);

            // 第一個 sheet
            foreach (var item in t)
            {
                var ws = p.Workbook.Worksheets.Add(item.Key);

                if (item.Value.Any())
                {
                    if (properties == null)
                    {
                        properties = item.Value.FirstOrDefault().GetType().GetProperties();
                    }
                    var rows = item.Value.Count() + 1;
                    var cols = properties.Count();
                    if (rows > 0 && cols > 0)
                    {
                        ws.Cells[1, 1].LoadFromCollection(item.Value, true);

                        using (var range = ws.Cells[1, 1, rows, cols])
                        {
                            // 標題列
                            var title = ws.Cells[1, 1, 1, cols];
                            title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            title.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                            title.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                        }
                    }
                }
            }

            // 第二個 sheet
            foreach (var item in s)
            {
                var ws = p.Workbook.Worksheets.Add(item.Key);

                if (item.Value.Any())
                {
                    if (properties == null)
                    {
                        properties = item.Value.FirstOrDefault().GetType().GetProperties();
                    }
                    var rows = item.Value.Count() + 1;
                    var cols = properties.Count();
                    if (rows > 0 && cols > 0)
                    {
                        ws.Cells[1, 1].LoadFromCollection(item.Value, true);

                        using (var range = ws.Cells[1, 1, rows, cols])
                        {
                            // 標題列
                            var title = ws.Cells[1, 1, 1, cols];
                            title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            title.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                            title.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                        }
                    }
                }
            }

            // 第三個 sheet
            foreach (var item in u)
            {
                var ws = p.Workbook.Worksheets.Add(item.Key);

                if (item.Value.Any())
                {
                    if (properties == null)
                    {
                        properties = item.Value.FirstOrDefault().GetType().GetProperties();
                    }
                    var rows = item.Value.Count() + 1;
                    var cols = properties.Count();
                    if (rows > 0 && cols > 0)
                    {
                        ws.Cells[1, 1].LoadFromCollection(item.Value, true);

                        using (var range = ws.Cells[1, 1, rows, cols])
                        {
                            // 標題列
                            var title = ws.Cells[1, 1, 1, cols];
                            title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            title.Style.Fill.PatternType = ExcelFillStyle.Solid; // 設定背景填色方法
                            title.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                        }
                    }
                }
            }

            p.Save();
            return ms.ToArray();
        }
        private void ExportBody<T>(
            ExcelWorksheet ws,
            IEnumerable<T> data,
            IEnumerable<PropertyInfo> propertyInfo,
            TimeSpan? offset = null)
        {
            var currentRow = 2;
            for (var i = 0; i < data.Count(); i++)
            {
                var bt = data.ElementAt(i);

                for (var j = 0; j < propertyInfo.Count(); j++)
                {
                    var prop = propertyInfo.ElementAt(j);
                    var propValue = prop.GetValue(bt, null);

                    if (propValue != null && propValue.GetType().Name == "DateTime" && ((DateTime)propValue).Year > 1)
                    {
                        var time = new DateTimeOffset((DateTime)propValue);
                        ws.Cells[currentRow, j + 1].Value = time.ToOffset(TimeSpan.FromHours(8)).DateTime;
                        ws.Cells[currentRow, j + 1].Style.Numberformat.Format = "mm/dd/yyyy h:mm:ss\\ AM/PM";
                    }
                    else if (propValue != null && propValue.GetType().Name == "DateTimeOffset" && ((DateTimeOffset)propValue).Year > 1)
                    {
                        var time = (DateTimeOffset)propValue;
                        var value = offset != null ? time.ToOffset(offset.Value).DateTime : time.DateTime;
                        ws.Cells[currentRow, j + 1].Value = value;
                        ws.Cells[currentRow, j + 1].Style.Numberformat.Format = "mm/dd/yyyy h:mm:ss\\ AM/PM";
                    }
                    else
                    {
                        ws.Cells[currentRow, j + 1].Value = propValue;
                    }
                    //ws.Cells[currentRow, j + 1].
                    //.Style.Numberformat.Format = "mm/dd/yyyy";
                }

                currentRow++;
            }
        }

        private void ExportHeader(ExcelWorksheet ws, IEnumerable<PropertyInfo> propertyInfo)
        {
            //SetColumnWidths(ws, criteria.ColumnWidths, maxColumnIndex: propertyInfo.Count());
            SetHeader(ws, propertyInfo.Select(p => p.Name), 1);
            //ws.View.FreezePanes(2, 1);
        }

        public void SetHeader(ExcelWorksheet ws, IEnumerable<string> titles, int currentRow)
        {
            for (var i = 0; i < titles.Count(); i++)
            {
                var cell = ws.Cells[currentRow, i + 1];
                cell.Value = titles.ElementAt(i);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                ws.Column(i + 1).AutoFit();
            }
        }

        public FileContentResult ExcelFile(byte[] data, string fileName)
        {
            return new FileContentResult(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{fileName}_{DateTime.Now.ToString("yyyyMMdd")}.xlsx"
            };
        }

        public FileContentResult ExcelFileDateRange(byte[] data, DateTime from, DateTime to, string fileName)
        {
            return new FileContentResult(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{fileName}_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx"
            };
        }
    }
}
