using System;
using System.Drawing;
using System.Reflection;

using BackendSite.Attributes;
using BackendSite.Service.Model;
using BackendSite.Service.Model.Mpa.WinLoss;
using Microsoft.AspNetCore.Mvc;

using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BackendSite.Service.BLL
{
    public class MpaExcelBLL
    {
        public virtual byte[] ExportV2<T>(IEnumerable<T> t, MpaWinLossRequest request, string IsoCode, IEnumerable<PropertyInfo> properties = null) where T : notnull, new()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);
            var ws = p.Workbook.Worksheets.Add(nameof(t));

            // 將 request 轉換為 JSON 字符串
            var requestJson = $"WIN LOST REPORT {request.StarTime.DateTime.ToString("yyyy/MM/dd")} - {request.EndTime.DateTime.ToString("yyyy/MM/dd")}, View In {IsoCode}, GMT{FormatTimeZoneOffset(request.StarTime.Offset)}";

            // 在第一行寫入 request JSON
            ws.Cells[1, 1].Value = requestJson;

            if (t.Any())
            {
                if (properties == null)
                {
                    properties = t.FirstOrDefault().GetType().GetProperties();
                }

                #region 

                var propCount = properties.Count();

                // 合併第一行的儲存格
                ws.Cells[1, 1, 1, propCount].Merge = true;

                // 在合併的儲存格中寫入 request JSON
                ws.Cells[1, 1].Value = requestJson;

                // 設定背景顏色和文字樣式
                var requestCell = ws.Cells[1, 1, 1, propCount];
                requestCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                requestCell.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#F4A460"));
                requestCell.Style.Font.Bold = true;
                requestCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                #endregion

                int startRow = 2;

                if (t.Any())
                {
                    if (properties == null)
                    {
                        properties = t.FirstOrDefault().GetType().GetProperties();
                    }

                    var rows = t.Count() + startRow;
                    var cols = properties.Count();

                    if (rows > startRow && cols > 0)
                    {
                        ws.Cells[startRow, 1].LoadFromCollection(t, true);

                        // 標題列
                        using (var range = ws.Cells[startRow, 1, startRow, cols])
                        {
                            var title = ws.Cells[startRow, 1, startRow, cols];
                            title.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            title.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            title.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#B7DEE8"));
                        }

                        int lastRow = ws.Dimension.End.Row;

                        for (int row = startRow + 1; row <= rows; row++)
                        {
                            var currencyCell = ws.Cells[row, properties.ToList().FindIndex(p => p.Name == "Currency") + 1];
                            if (currencyCell.Value?.ToString().Substring(0, 3).Equals("ALL", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                using (var range = ws.Cells[row, 1, row, cols])
                                {
                                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFACD"));
                                }
                            }

                            for (int col = 1; col <= cols; col++)
                            {
                                var cell = ws.Cells[row, col];
                                if (cell.Value?.ToString().Substring(0, 1).Equals("-", StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    cell.Style.Font.Color.SetColor(Color.Red);
                                }
                            }
                        }

                        using (var range = ws.Cells[rows, 1, rows, cols])
                        {
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFB6C1"));
                        }
                    }
                }
            }

            // 自動調整列寬
            ws.Cells.AutoFitColumns();

            p.Save();
            return ms.ToArray();
        }

        public FileContentResult ExcelFile(byte[] data, DateTime from, DateTime to, string fileName)
        {
            return new FileContentResult(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{fileName}_{from:yyyyMMdd}_{to:yyyyMMdd}_{DateTime.Now:yyMMddHHmmss}.xlsx"
            };
        }

        private static string FormatTimeZoneOffset(TimeSpan offset)
        {
            int hours = offset.Hours;
            return hours >= 0 ? $"+{hours:D2}" : $"{hours:D3}";
        }
    }
}
