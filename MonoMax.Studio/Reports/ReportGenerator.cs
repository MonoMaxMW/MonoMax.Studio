using MonoMax.Studio.Contracts;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio.Reports
{
    public static class ReportGenerator
    {

        public static string CreateIndentPrefix(string chars, int amount)
        {
            var result = string.Empty;

            if(amount > 0)
            {
                for (int i = 0; i < amount; i++)
                    result += chars;
            }

            return result;
        }

        public static string GetImage(string imageKey, int size)
        {
            var baseDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            var imgPath = Path.Combine(baseDir, "Data", "img", imageKey + $"_{size}.png");

            return imgPath;
        }

        public static void GenerateExcelReport(string template, IEnumerable<INode> nodes, string reportFile = "")
        {
            if (!File.Exists(template))
                throw new FileNotFoundException($"{template}");

            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Reports");

            
            var fn = string.IsNullOrEmpty(reportFile) ? $"report_{Guid.NewGuid()}" : reportFile;
            fn += ".xlsx";

            var fi = new FileInfo(Path.Combine(dir, fn));

            using (var ePckg = new ExcelPackage(fi, new FileInfo(template)))
            {
                var wb = ePckg.Workbook;
                var ws = wb.Worksheets.First();
                var startCol = wb.Names["ItemRow_template"].Start.Column;
                var startRow = wb.Names["ItemRow_template"].Start.Row;
                var imgStartCol = wb.Names["ItemIcon"].Start.Column;
                var rowHeight = ws.Row(startRow).Height;
                
                var headerCol = wb.Names["ItemHeader"].Start.Column;
                var vendorCol = wb.Names["ItemVendor"].Start.Column;
                var r = startRow;

                foreach (var item in nodes)
                {
                    var indentStr = CreateIndentPrefix("..", item.TreeDepth);
                    

                    ws.Cells[r, headerCol].Value = $"{indentStr} {item.Header}";

                    ws.InsertRow(r+1, 1, startRow);
                    ws.Row(r).Height = rowHeight;


                    if (!string.IsNullOrEmpty(item.ImageKey))
                    {
                        var imgFile = GetImage(item.ImageKey, 64);

                        if (File.Exists(imgFile))
                        {
                            var width = ws.Column(imgStartCol).Width;
                            var factorD = 36.0d / 64.0d;
                            var factor = Convert.ToInt32(factorD * 100.0d);
                            var pic = ws.Drawings.AddPicture($"{Guid.NewGuid()}", new FileInfo(imgFile));
                            pic.SetSize(factor);

                            var iw = pic.Image.Width * factorD;
                            var ih = pic.Image.Height * factorD;
                            var x = Convert.ToInt32((40.0 - iw) * 0.5d);
                            var y = Convert.ToInt32((40.0 - ih) * 0.5d);



                            pic.SetPosition(r-1, y, imgStartCol-1, x);
                        }
                    }


                    r++;
                }
                ws.DeleteRow(r);

                ws = null;
                wb = null;
                ePckg.Save();
            }

        }
    }
}
