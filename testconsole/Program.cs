
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using wkpdftoxcorelib;

namespace testconsole
{
    class Program
    {
        static void Create(int i)
        {
            var renderer = new PdfRenderer();
            Console.WriteLine("WkHTML version:" + renderer.GetVersion());

            var cover = new PdfDocument(PdfSource.FromFile("res/cover.html"));
            cover.Configure(print => {
                print.DPI = 300;
                print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
            });
            
            var content = new PdfDocument(PdfSource.FromHtml($"This PDF is created using thread #{i}"));
            content.Configure(print => 
            {
                print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
                print.DPI = 300;
            });
            content.SetHeader(PdfSource.FromFile("res/header.html"), 25, 5);
            content.SetFooter(PdfSource.FromFile("res/footer.html"), 25, 5);
            
            renderer.Add(cover);
            renderer.Add(content);

            renderer.RenderToFile($"output-{i}.pdf");
        }

      
        static void Main(string[] args)
        {
            // No data is available for encoding 1252
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      
            Parallel.For(0, 2, i => {
                Create(i);
            });
        }
    }
}

