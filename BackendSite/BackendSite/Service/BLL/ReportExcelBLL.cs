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
    #region Public Class : Parameter Models

    /// <summary>
    /// 一般報表轉excel單頁參數
    /// </summary>
    public class GeneralReportExportDataToExcel
    {
        public string SheetName { get; set; }
        public IEnumerable<Dictionary<string, dynamic>> Content { get; set; }
        public Dictionary<string, string> Titles { get; set; }

    }

    /// <summary>
    /// 複雜報表轉excel的參數
    /// </summary>
    public class ComplexReportExportDataToExcel
    {
        /// <summary>
        /// Sheet 名稱
        /// </summary>
        public string SheetName
        {
            get;
            set;
        }

        /// <summary>
        /// 內容
        /// </summary>
        public IEnumerable<Dictionary<string, dynamic>> Content { get; set; }

        public IEnumerable<Dictionary<string, dynamic>> TotalContent { get; set; }

        public Dictionary<string, string> Titles { get; set; }

        public List<string> TotalTitles { get; set; }

        public List<Dictionary<string, MergeTitleModel>> TitlesToMerge { get; set; }

        public List<string> NotSummaryTitles { get; set; } = new List<string>();

        public List<int> MergeContentRules { get; set; } = new List<int>();

        public List<string> GroupTitles { get; set; } = new List<string>();

        public bool IsSummary { get; set; } = true;

        public bool IsTotalTitleMax { get; set; } = false;

    }

    #region

    /// <summary>
    /// 合併標題模型
    /// </summary>
    public class MergeTitleModel
    {
        public string Name { get; set; }

        public int MergeRange { get; set; }
    }

    #endregion

    #endregion

    public class ReportExcelBLL
    {
        #region Constructors

        public ReportExcelBLL()
        {

        }

        #endregion

        #region Purblic Methods

        /// <summary>
        /// 一般報表匯出多頁Excel
        /// </summary>
        /// <param name="t">匯出各頁資料清單</param>
        /// <returns>Excel</returns>
        public byte[] GeneralReportExportToMultiSheet(List<GeneralReportExportDataToExcel> t)
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

                        GeneralReportExportHeader(ws,
                                                  s.Titles.Select(x => x.Value),
                                                  1);

                        GeneralReportExportBody(ws,
                                                s.Content,
                                                s.Titles.Select(x => x.Value).ToList(),
                                                2);
                    }
                }
            }

            p.Save();

            return ms.ToArray();
        }

        /// <summary>
        /// 複雜報表匯出Excel
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public byte[] ComplexReportExportToMultiSheet(List<ComplexReportExportDataToExcel> t)
        {
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);

            if (t.Any())
            {
                foreach (var s in t)
                {
                    var currentRow = 1;

                    var ws = p.Workbook.Worksheets.Add(s.SheetName);

                    if (s.Content.Any())
                    {
                        currentRow = ComplexReportExportMergeRowHeader(ws,
                                                                       s.TitlesToMerge,
                                                                       currentRow);

                        currentRow = ComplexReportExportHeader(ws,
                                                  s.Titles.Select(x => x.Value),
                                                   currentRow);
                        ComplexReportExportBody(ws,
                                                s.Content,
                                                s.TotalContent,
                                                s.Titles.Select(x => x.Key).ToList(),
                                               currentRow,
                                                s.TotalTitles,
                                                s.IsSummary,
                                                s.IsTotalTitleMax,
                                                s.NotSummaryTitles,
                                                s.MergeContentRules,
                                                s.GroupTitles);

                    }
                }
            }

            p.Save();
            return ms.ToArray();
        }

        #endregion

        #region Private Methods

        #region GeneralReportExportToMultiSheet

        #region GeneralReportExportHeader

        private void GeneralReportExportHeader(ExcelWorksheet ws,
                                               IEnumerable<string> titles,
                                               int currentRow)
        {
            GeneralReportSetHeader(ws,
                                   titles,
                                   currentRow);
        }

        private void GeneralReportSetHeader(ExcelWorksheet ws,
                                            IEnumerable<string> titles,
                                            int currentRow)
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

        #endregion

        #region GeneralReportExportBody
        private void GeneralReportExportBody(ExcelWorksheet ws,
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
                        if (value != null && colName == "DateTime" && ((DateTime)value).Year > 1)
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

        #endregion

        #endregion

        #region ComplexReportExportToMultiSheet

        #region ComplexReportExportMergeRowHeader

        private int ComplexReportExportMergeRowHeader(ExcelWorksheet ws,
                                                      List<Dictionary<string, MergeTitleModel>> mergeTitles,
                                                      int currentRow)
        {
            return ComplexReportSetMergeHeader(ws,
                                               mergeTitles,
                                               currentRow);
        }

        private int ComplexReportSetMergeHeader(ExcelWorksheet ws,
                                                List<Dictionary<string, MergeTitleModel>> mergeTitlesList,
                                                int currentRow)
        {
            foreach (var mergeTitles in mergeTitlesList)
            {
                var cellCount = 1;
                for (var i = 0; i < mergeTitles.Count(); i++)
                {
                    var cell = ws.Cells[currentRow, cellCount];
                    cell.Value = mergeTitles.ElementAt(i).Value.Name;

                    //Cell Merging
                    ws.Cells[currentRow, cellCount, currentRow, cellCount + mergeTitles.ElementAt(i).Value.MergeRange - 1].Merge = true;

                    //Style  Setting
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;

                    if (currentRow % 2 == 1)
                    {
                        if (i % 2 == 1)
                        {
                            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFCF78"));
                        }
                        else
                        {
                            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FF7878"));
                        }
                    }
                    else
                    {
                        if (i % 2 == 1)
                        {
                            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                        }
                        else
                        {
                            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#E8C1B7"));
                        }
                    }

                    for (var j = cellCount; j < (cellCount + mergeTitles.ElementAt(i).Value.MergeRange); j++)
                    {
                        ws.Cells[currentRow, j].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                        ws.Cells[currentRow, j].Style.Border.Top.Style = ExcelBorderStyle.Medium;
                        ws.Cells[currentRow, j].Style.Border.Left.Style = ExcelBorderStyle.Medium;
                        ws.Cells[currentRow, j].Style.Border.Right.Style = ExcelBorderStyle.Medium;
                    }

                    ws.Column(i + 1).AutoFit();
                    cellCount = cellCount + mergeTitles.ElementAt(i).Value.MergeRange;
                }
                currentRow++;
            }

            return currentRow;
        }

        #endregion

        #region ComplexReportExportHeader

        private int ComplexReportExportHeader(ExcelWorksheet ws,
                                               IEnumerable<string> titles,
                                               int currentRow)
        {
            return ComplexReportSetHeader(ws,
                                   titles,
                                   currentRow);
        }

        private int ComplexReportSetHeader(ExcelWorksheet ws,
                                            IEnumerable<string> titles,
                                            int currentRow)
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

            return (currentRow + 1);
        }

        #endregion

        #region ComplexReportExportBody

        private void ComplexReportExportBody(ExcelWorksheet ws,
                                             IEnumerable<Dictionary<string, dynamic>> data,
                                             IEnumerable<Dictionary<string, dynamic>> totalData,
                                             List<string> titles,
                                             int currentRow,
                                             List<string> totalTitles,
                                             bool isSummary,
                                             bool IsTotalTitleMax,
                                             List<string> notSummaryTitles,
                                             List<int> mergeContentRules,
                                             List<string> groupTitles)
        {
            var bodyStartRow = currentRow;
            if (IsTotalTitleMax)
            {
                for (var i = 0; i < data.Count(); i++)
                {
                    var bt = data.ElementAt(i);
                    var maxFlag = bt.ContainsKey("MaxCount");

                    if (maxFlag)
                    {
                        ComplexReportSetBodyMax(ws,
                                                bt,
                                                titles,
                                                currentRow,
                                                totalTitles);
                    }
                    else
                    {
                        ComplexReportSetBodyGeneral(ws,
                                                    bt,
                                                    titles,
                                                    currentRow,
                                                    totalTitles);
                    }
                    currentRow++;
                }
            }
            else
            {
                //All Data General Export
                currentRow = ComplexReportExportBodyGeneral(ws,
                                                            data,
                                                            titles,
                                                            currentRow,
                                                            totalTitles);
            }

            if (mergeContentRules.Count() > 0)
            {
                ComplexReportSetCustomizeBodyCellMerge(ws,
                                                       mergeContentRules,
                                                       bodyStartRow);
            }

            if (groupTitles.Count() > 0)
            {
                ComplexReportSetBodyGroupContentStyle(ws,
                                                      titles,
                                                      groupTitles,
                                                      bodyStartRow,
                                                      currentRow);
            }

            if (isSummary && notSummaryTitles.Count() > 0)
            {
                ComplexReportSetBodySummary(ws,
                                            data,
                                            titles,
                                            currentRow,
                                            notSummaryTitles);
            }
            if (isSummary && totalData != null)
            {
                ComplexReportSetCustomizeBodySummary(ws,
                                                     totalData,
                                                     titles,
                                                     currentRow);
            }
        }

        private int ComplexReportExportBodyGeneral(ExcelWorksheet ws,
                                                   IEnumerable<Dictionary<string, dynamic>> data,
                                                   List<string> titles,
                                                   int currentRow,
                                                   List<string> totalTitles)
        {

            for (var i = 0; i < data.Count(); i++)
            {
                var bt = data.ElementAt(i);

                ComplexReportSetBodyGeneral(ws,
                                            bt,
                                            titles,
                                            currentRow,
                                            totalTitles);

                currentRow++;
            }

            return currentRow;
        }

        private void ComplexReportSetBodyGeneral(ExcelWorksheet ws,
                                                 Dictionary<string, dynamic> bt,
                                                 List<string> titles,
                                                 int currentRow,
                                                 List<string> totalTitles)
        {
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

        }

        private void ComplexReportSetBodyMax(ExcelWorksheet ws,
                                             Dictionary<string, dynamic> bt,
                                             List<string> titles,
                                             int currentRow,
                                             List<string> totalTitles)
        {
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
                            ws.Cells[currentRow, colIdx].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FBFF76"));
                        }
                        else
                        {
                            ws.Cells[currentRow, colIdx].Value = value;

                            ws.Cells[currentRow, colIdx].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[currentRow, colIdx].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FBFF76"));
                        }
                    }
                }
            }

        }

        private void ComplexReportSetBodyGroupContentStyle(ExcelWorksheet ws,
                                                           List<string> titles,
                                                           List<string> groupTitles,
                                                           int bodyStartRow,
                                                           int bodyEndRow)
        {
            foreach (var title in titles)
            {
                if (groupTitles.Contains(title))
                {
                    var colIdx = titles.IndexOf(title) + 1;

                    for (var i = bodyStartRow; i < bodyEndRow; i++)
                    {
                        var value = (string)ws.Cells[i, colIdx].Value;

                        if (value.Contains("TOTAL"))
                        {
                            for (var j = colIdx; j <= titles.Count(); j++)
                            {
                                ws.Cells[i, j].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[i, j].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#BFFFFF"));
                            }
                        }
                    }
                }
            }
        }

        private void ComplexReportSetCustomizeBodyCellMerge(ExcelWorksheet ws,
                                                    List<int> mergeContentRules,
                                                    int currentRow)
        {
            foreach (var row in mergeContentRules)
            {
                var cell = ws.Cells[currentRow, 1];

                //Cell Merging
                ws.Cells[currentRow, 1, currentRow + row - 1, 1].Merge = true;

                //Style  Setting
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                for (var i = currentRow; i <= currentRow + row - 1; i++)
                {
                    ws.Cells[i, 1, i + 1, 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[i, 1, i + 1, 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[i, 1, i + 1, 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[i, 1, i + 1, 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;


                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE8BF"));

                currentRow = currentRow + row;
            }
        }

        private void ComplexReportSetBodySummary(ExcelWorksheet ws,
                                                 IEnumerable<Dictionary<string, dynamic>> data,
                                                 List<string> titles,
                                                 int currentRow,
                                                 List<string> notSummaryTitles)
        {
            #region Total Data Caculate

            var totaldata = new Dictionary<string, dynamic>();

            for (var i = 0; i < data.Count(); i++)
            {
                var bt = data.ElementAt(i);

                var getValue = bt["Key"];

                if (!getValue.Contains("Total"))
                {
                    foreach (var item in bt)
                    {
                        var value = item.Value;
                        var colName = item.Key;

                        var getItem = totaldata.TryGetValue(colName, out var total);

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
            }

            #endregion

            #region Total Data Export

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
                        if (!(item.Value is string))
                        {
                            ws.Cells[currentRow, colIdx].Value = value;
                        }
                    }

                    ws.Cells[currentRow, colIdx].Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                    ws.Cells[currentRow, colIdx].Style.Border.Top.Style = ExcelBorderStyle.Medium;

                    ws.Cells[currentRow, colIdx].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, colIdx].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFFF80"));


                }
            }

            #endregion

        }

        private void ComplexReportSetCustomizeBodySummary(ExcelWorksheet ws,
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
                        if (value != null)
                        {
                            ws.Cells[currentRow, colIdx].Value = value;

                            ws.Cells[currentRow, colIdx].Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                            ws.Cells[currentRow, colIdx].Style.Border.Top.Style = ExcelBorderStyle.Medium;

                            ws.Cells[currentRow, colIdx].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[currentRow, colIdx].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFFF80"));
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #endregion


    }

}
