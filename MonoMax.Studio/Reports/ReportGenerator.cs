using MonoMax.Studio.Contracts;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static void GenerateExcelReport(string template, IEnumerable<INode> nodes, string reportFile = "")
        {
            if (!File.Exists(template))
                throw new FileNotFoundException($"{template}");

            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Reports");

            
            var fn = string.IsNullOrEmpty(reportFile) ? $"report_{Guid.NewGuid()}" : reportFile;
            fn += ".xlsx";

            var genReportFi = new FileInfo(Path.Combine(dir, fn));
            var preferredVendor = "emco";

            using (var ePckg = new ExcelPackage(genReportFi, new FileInfo(template)))
            {
                var wb = ePckg.Workbook;
                var ws = wb.Worksheets.First();
                var rowTemplate = wb.Names["ItemRow_template"];
                var startCol = rowTemplate.Start.Column;
                var startRow = rowTemplate.Start.Row;
                var imgColIdx = wb.Names["ItemIcon"].Start.Column;

                var rowHeight = ws.Row(startRow).Height;
                
                var headerColIdx = wb.Names["ItemHeader"].Start.Column;
                var noteColIdx = wb.Names["ItemNote"].Start.Column;
                var r = startRow;
                var margin = 2;

                foreach (var item in nodes)
                {
                    var indentStr = CreateIndentPrefix("-", item.TreeDepth);
                    
                    ws.Cells[r, headerColIdx].Value = $"{indentStr} {item.Header}";
                    ws.InsertRow(r + 1, 1, startRow);

                    ws.Row(r).Height = rowHeight;

                    if(item.Ids != null && item.Ids.Count > 0)
                    {
                        var vendor = string.Empty;
                        var itemId = string.Empty;

                        if (item.Ids.ContainsKey(preferredVendor))
                            vendor = preferredVendor;
                        else
                            vendor = item.Ids.First().Key;


                        itemId = item.Ids[vendor];
                    }


                    if (!string.IsNullOrEmpty(item.ImageKey))
                    {
                        var imgSz = 64;
                        var imgFile = AssetRepository.GetImagePath(item.ImageKey, imgSz);

                        if (File.Exists(imgFile))
                        {
                            var factorD = 40.0d / imgSz;
                            var factor = Convert.ToInt32(factorD * 100.0d);
                            var pic = ws.Drawings.AddPicture($"{Guid.NewGuid()}", new FileInfo(imgFile));
                            pic.SetSize(factor);
                            pic.SetPosition(r-1, 0, imgColIdx-1, 0);
                        }
                    }


                    r++;
                }
                ws.DeleteRow(r);

                var distinctNodes = nodes.Distinct(new NodeEqualityComparer());

                ws = wb.Worksheets[2];
                var qutColumnIdx = wb.Names["SumItemQuantity"].Start.Column;
                startCol = wb.Names["SumItemRow_template"].Start.Column;
                startRow = wb.Names["SumItemRow_template"].Start.Row;
                imgColIdx = wb.Names["SumItemIcon"].Start.Column;
                rowHeight = ws.Row(startRow).Height;
                headerColIdx = wb.Names["SumItemHeader"].Start.Column;
                noteColIdx = wb.Names["SumItemVendor"].Start.Column;
                r = startRow;

                foreach (var item in distinctNodes)
                {
                    ws.Cells[r, headerColIdx].Value = item.Header;
                    ws.InsertRow(r + 1, 1, startRow);
                    ws.Row(r).Height = rowHeight;

                    ws.Cells[r, qutColumnIdx].Value = nodes.Count(x => x.GetCompareValue() == item.GetCompareValue());

                    if (!string.IsNullOrEmpty(item.ImageKey))
                    {
                        var imgSz = 64;
                        var imgFile = AssetRepository.GetImagePath(item.ImageKey, imgSz);

                        if (File.Exists(imgFile))
                        {
                            var factorD = 40.0d / imgSz;
                            var factor = Convert.ToInt32(factorD * 100.0d);
                            var pic = ws.Drawings.AddPicture($"{Guid.NewGuid()}", new FileInfo(imgFile));
                            pic.SetSize(factor);
                            pic.SetPosition(r - 1, 0, imgColIdx - 1, 0);
                        }
                    }

                    r++;
                }
                ws.DeleteRow(r);

                ws = null;
                wb = null;
                ePckg.Save();
            }

            Process.Start(genReportFi.FullName);

        }
    }
}
