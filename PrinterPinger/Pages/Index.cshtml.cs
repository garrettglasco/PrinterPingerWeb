using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OfficeOpenXml;


namespace PrinterPinger.Pages
{
    public class IndexModel : PageModel
    {
        public List<Dictionary<string, string>> IpAddresses { get; set; }

        public IndexModel()
        {
            // Initialize the list in the constructor
            IpAddresses = new List<Dictionary<string, string>>();
        }

        public void OnGet()
        {
            // This method is called when the page is requested via HTTP GET
            // You can put initialization logic here if needed
        }

        public void OnPost(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path.GetTempFileName(); // Save the file to a temporary location
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Process the Excel file
                ProcessExcelFile(filePath);
            }
        }

        private void ProcessExcelFile(string filePath)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                // Assuming the first row contains headers
                var rowCount = worksheet.Dimension.Rows;
                var colCount = worksheet.Dimension.Columns;

                for (int row = 2; row <= rowCount; row++)
                {
                    var ipAddressDict = new Dictionary<string, string>();

                    for (int col = 1; col <= colCount; col++)
                    {
                        var columnName = worksheet.Cells[1, col].Value?.ToString();
                        var cellValue = worksheet.Cells[row, col].Value?.ToString();

                        ipAddressDict[columnName] = cellValue;
                    }

                    IpAddresses.Add(ipAddressDict);
                }
            }
        }

        public IActionResult OnGetPing(string ipAddress)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(ipAddress);
                    return new ContentResult { Content = reply.Status.ToString(), ContentType = "text/plain" };
                }
            }
            catch (Exception ex)
            {
                return new ContentResult { Content = $"Error: {ex.Message}", ContentType = "text/plain" };
            }
        }
    }
}
