using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using Lepilina.Models;
using System.Collections.Generic;
using System.IO;
using iText.Kernel.Font;

namespace Lepilina.Controllers
{
    public class PdfExportService
    {
        public MemoryStream CreateSalesReport(int totalSalesCount, List<CategorySalesData> categorySalesData)
        {
            var stream = new MemoryStream(); 

            var writer = new PdfWriter(stream);
            writer.SetCloseStream(false);

            using (var pdf = new PdfDocument(writer))
            {
                var document = new Document(pdf);

                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA); 
                document.SetFont(font);

                var title = new Paragraph("Отчет по продажам")
                    .SetFontSize(20)
                    .SetTextAlignment(TextAlignment.CENTER);

                var totalSalesParagraph = new Paragraph($"Общее количество продаж: {totalSalesCount}");
                document.Add(title);
                document.Add(totalSalesParagraph);

                var table = new Table(2);
                table.AddHeaderCell("Категория");
                table.AddHeaderCell("Количество");

                foreach (var category in categorySalesData)
                {
                    if (!string.IsNullOrEmpty(category.CategoryName) && category.Count >= 0)
                    {
                        table.AddCell(category.CategoryName); 
                        table.AddCell(category.Count.ToString()); 
                    }
                    else
                    {
                        Console.WriteLine($"Некорректные данные: CategoryName={category.CategoryName}, Count={category.Count}");
                    }
                }

                document.Add(table);

                document.Close(); 
            }

            stream.Position = 0; 
            return stream; 
        }

    }
}
