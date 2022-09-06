using System.Reflection;
using BackendSite.Attributes;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BackendSite.Service.BLL
{
    public class LiveReigstrationExcelBLL : ExcelBLL
    {
        public override byte[] Export<T>(IEnumerable<T> t, IEnumerable<PropertyInfo> propertyInfo = null, TimeSpan? offset = null)
        {
            using var ms = new MemoryStream();
            using var p = new ExcelPackage(ms);
            var ws = p.Workbook.Worksheets.Add(nameof(t));

            if (t.Any())
            {
                if (propertyInfo == null)
                {
                    propertyInfo = t.FirstOrDefault().GetType().GetProperties();
                }

                propertyInfo = propertyInfo.Where(x => !Attribute.IsDefined(x, typeof(ExportIgnoreAttribute)))
                    .ToArray();

                SetBody(ws, t, propertyInfo, offset, out var colCount);
                SetHeader(ws, propertyInfo.Select(p => p.Name), colCount);
            }

            p.Save();
            return ms.ToArray();
        }

        private void SetHeader(ExcelWorksheet ws, IEnumerable<string> titles, int extraColCount)
        {
            var idx = 1;
            var count = titles.Count();
            foreach (var title in titles)
            {
                if (idx == count)
                {
                    for (var i = 0; i < extraColCount; i++)
                    {
                        var cell = ws.Cells[1, idx + i];
                        cell.Value = $"{i + 1} {title}";
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                        ws.Column(idx + i).AutoFit();
                    }
                }
                else
                {
                    var cell = ws.Cells[1, idx];
                    cell.Value = title;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#B7DEE8"));
                    ws.Column(idx).AutoFit();
                    idx++;
                }
            }
        }

        private void SetBody<T>(ExcelWorksheet ws, IEnumerable<T> data, IEnumerable<PropertyInfo> propertyInfo, TimeSpan? offset, out int colCount)
        {
            var currentRow = 2;
            colCount = 0;
            for (var i = 0; i < data.Count(); i++)
            {
                var bt = data.ElementAt(i);

                for (var j = 0; j < propertyInfo.Count(); j++)
                {
                    var prop = propertyInfo.ElementAt(j);
                    var propValue = prop.GetValue(bt, null);

                    if (propValue != null && propValue.GetType().Name == "DateTimeOffset" &&
                        ((DateTimeOffset)propValue).Year > 1)
                    {
                        var time = (DateTimeOffset)propValue;
                        var value = offset != null ? time.ToOffset(offset.Value).DateTime : time.DateTime;
                        ws.Cells[currentRow, j + 1].Value = value;
                        ws.Cells[currentRow, j + 1].Style.Numberformat.Format = "mm/dd/yyyy h:mm:ss\\ AM/PM";
                    }
                    else if (propValue != null && propValue.GetType().Name == "List`1")
                    {
                        var listData = (List<string>)propValue;
                        var count = listData.Count;
                        if (count > colCount)
                            colCount = count;

                        var k = 1;
                        foreach (var dd in listData)
                        {
                            ws.Cells[currentRow, j + k].Value = dd;
                            k++;
                        }
                    }
                    else
                    {
                        ws.Cells[currentRow, j + 1].Value = propValue;
                    }
                }

                currentRow++;
            }
        }
    }
}