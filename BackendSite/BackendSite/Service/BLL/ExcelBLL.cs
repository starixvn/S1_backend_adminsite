using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class ExportDataToExcel
    {
        public string SheetName { get; set; }
        public IEnumerable<Dictionary<string, dynamic>> Content { get; set; }
        public List<string> Titles { get; set; }

    }

    public class ExportReportDataToExcel : ExportDataToExcel
    {
        public List<string> TitlesNameToDisplay { get; set; }

        public List<string> TotalTitles { get; set; }

        public Dictionary<string, int> TitlesToMerge { get; set; }

        public List<string> NotSummaryTitles { get; set; }

    }

    public class ExcelBLL
    {
        public ExcelBLL()
        {

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
                        ExportBody(ws, s.Content, s.Titles, 2,totalTitles: new List<string>());
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
                                int currentRow,
                                List<string> totalTitles,
                                bool isSummary = false,
                                List<string> notSummaryTitles = null)
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
                        if (value != null && colName == "DateTime" && ((DateTime)value).Year > 1)
                        {
                            var time = new DateTimeOffset((DateTime)value);
                            ws.Cells[currentRow, colIdx].Value = time.ToOffset(TimeSpan.FromHours(8)).DateTime;
                            ws.Cells[currentRow, colIdx].Style.Numberformat.Format = "mm/dd/yyyy h:mm:ss\\ AM/PM";
                        }
                        else
                        {
                            if (totalTitles.Contains(colName))
                            {
                                ws.Cells[currentRow, colIdx].Value = value;

                                ws.Cells[currentRow, colIdx].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[currentRow, colIdx].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#CFCFCF"));
                            }
                            else
                            {
                                ws.Cells[currentRow, colIdx].Value = value;
                            }
                        }
                    }
                }

                currentRow++;
            }

            if (isSummary)
            {
                #region Total Data

                Dictionary<string, dynamic> totaldata = new Dictionary<string, dynamic>();

                for (var i = 0; i < data.Count(); i++)
                {
                    var bt = data.ElementAt(i);

                    foreach (var item in bt)
                    {
                        var value = item.Value;
                        var colName = item.Key;

                        var getItem = totaldata.TryGetValue(colName, out dynamic total);

                        if (getItem)
                        {
                            if (!notSummaryTitles.Contains(colName))
                            {
                                totaldata[colName] = (decimal)total + value;
                            }
                        }
                        else
                        {
                            totaldata.Add(colName, value);
                        }

                    }
                }

                #endregion

                foreach (var item in totaldata)
                {
                    var value = item.Value;
                    var colName = item.Key;

                    if (titles.Any(x => string.Equals(x, colName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var colIdx = titles.IndexOf(colName) + 1;
                        if (colIdx == 1)
                        {
                            ws.Cells[currentRow, colIdx].Value = "TOTAL：";
                        }
                        else
                        {
                            ws.Cells[currentRow, colIdx].Value = value;
                        }

                        ws.Cells[currentRow, colIdx].Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                        ws.Cells[currentRow, colIdx].Style.Border.Top.Style = ExcelBorderStyle.Medium;

                        ws.Cells[currentRow, colIdx].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[currentRow, colIdx].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFFF80"));


                    }
                }
            }
        }

        private void ExportHeader(ExcelWorksheet ws,
                                  IEnumerable<string> titles,
                                  int currentRow,
                                  bool isReport = false)
        {
            if (isReport)
            {
                SetHeaderForReport(ws, titles, currentRow);
            }
            else
            {
                SetHeader(ws, titles, currentRow);
            }
        }

        public byte[] ExportToMultiSheetForReport(List<ExportReportDataToExcel> t)
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
                        ExportBody(ws, s.Content, s.Titles, 3, s.TotalTitles,true, s.NotSummaryTitles);
                        if (s.TitlesNameToDisplay.Any())
                        {
                            ExportHeader(ws, s.TitlesNameToDisplay, 2, true);
                        }
                        else
                        {
                            ExportHeader(ws, s.Titles, 2, true);
                        }
                        ExportMergeRowHeader(ws, s.TitlesToMerge);
                    }
                }
            }

            p.Save();
            return ms.ToArray();
        }

        #region Export Data Methods

        public byte[] Export<T>(IEnumerable<T> t) where T : notnull, new()
        {
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);
            var ws = p.Workbook.Worksheets.Add(nameof(t));

            if (t.Any())
            {
                var propertyInfo = t.FirstOrDefault().GetType().GetProperties();
                ExportBody(ws, t, propertyInfo);
                ExportHeader(ws, propertyInfo);
            }
            p.Save();
            return ms.ToArray();
        }

        private void ExportBody<T>(
            ExcelWorksheet ws,
            IEnumerable<T> data,
            IEnumerable<PropertyInfo> propertyInfo)
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

        private void ExportMergeRowHeader(ExcelWorksheet ws, Dictionary<string, int> mergeTitles)
        {
            SetMergeHeaderForReport(ws, mergeTitles, 1);
        }

        #endregion

        #region Set Data Methods

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

        public void SetHeaderForReport(ExcelWorksheet ws, IEnumerable<string> titles, int currentRow)
        {
            for (var i = 0; i < titles.Count(); i++)
            {
                var cell = ws.Cells[currentRow, i + 1];
                cell.Value = titles.ElementAt(i);

                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;

                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));

                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                cell.Style.Border.Top.Style = ExcelBorderStyle.Medium;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Medium;

                ws.Column(i + 1).AutoFit();
            }
        }

        public void SetMergeHeaderForReport(ExcelWorksheet ws, Dictionary<string, int> mergeTitles, int currentRow)
        {
            int cellCount = 1;
            for (var i = 0; i < mergeTitles.Count(); i++)
            {
                var cell = ws.Cells[currentRow, cellCount];
                cell.Value = mergeTitles.ElementAt(i).Key;

                //Cell Merging
                ws.Cells[1, cellCount, 1, cellCount + mergeTitles.ElementAt(i).Value - 1].Merge = true;

                //Style  Setting
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;

                if (i % 2 == 1)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                }
                else
                {
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#E8C1B7"));
                }

                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                cell.Style.Border.Top.Style = ExcelBorderStyle.Medium;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Medium;

                ws.Column(i + 1).AutoFit();
                cellCount = cellCount + mergeTitles.ElementAt(i).Value;
            }
        }

        #endregion
    }
}
