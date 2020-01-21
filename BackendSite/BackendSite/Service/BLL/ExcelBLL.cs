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
    public class ExcelBLL
    {
        public ExcelBLL()
        {

        }

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
                    ws.Cells[currentRow, j + 1].Value = propValue;
                    if (propValue != null && propValue.GetType().Name == "DateTime")
                    {
                        ws.Cells[currentRow, j + 1].Style.Numberformat.Format = "mm/dd/yyyy h:mm:ss\\ AM/PM";
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
    }
}
