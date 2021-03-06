﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ItechoPdf;
using ItechoPdf.Core;
using PdfSharp.Drawing;
using PdfSharp.Pdf;


namespace testconsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // No data is available for encoding 1252
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var renderer = new PdfRenderer(settings => {
                // Set global settings for all documents rendered through this service
                settings.DPI = 300;
                settings.Margins.Set(5, 5, 0, 5, Unit.Millimeters);
                settings.PaperSize = PaperKind.A4;
                settings.Orientation = Orientation.Landscape;
            });
            
            Console.WriteLine("WkHTML version:" + renderer.GetVersion());

            var doc = renderer.AddDocument(null, settings => 
            {
                settings.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
            });
            doc.AddCSS(PdfSource.FromFile("pages/tailwind.min.css"));
            doc.AddCSS(PdfSource.FromHtml(@"
    html, body, .bb {
        height: 100%;
        width: 100%;
    }
    .bb {              
        background: url(pages/cover.jpg);
        background-size: cover;
        position: fixed;
        left: 0px;
        top: 0px;
        margin: -2cm 0 0 -97mm;
    }"));
            var watch = new Stopwatch();
            watch.Start();

            doc.AddPage(PdfSource.FromFile("pages/cover.html"));
        
            var content = renderer.AddDocument();
            content.AddCSS(PdfSource.FromFile("pages/tailwind.min.css"));
            
            // Set header and footer for all pages in this document.
            // Can be overriden for each indivdual page
            // Or you can use the variables resolver to inject dynamic content into your headers / footers
            content.SetFooter(15, PdfSource.FromFile("pages/footer.html"));
            content.SetHeader(30, PdfSource.FromFile("pages/header.html"));

            content.VariableResolver = (vars) => {
                var r = new Random();
                var color = $"rgb({r.Next(256)}, {r.Next(256)}, {r.Next(256)})";
                return new List<VariableReplace> 
                { 
                    new VariableReplace("size", $"<span style=\"color:{color};\">{vars.Page * 123.59}</span>")
                };
            };
            
            content.AddPage(PdfSource.FromFile("pages/PlayField-0.html"));
            content.AddPage(PdfSource.FromFile("pages/PlayField-1.html"), PdfSource.Empty());
            content.AddPage(PdfSource.FromFile("pages/PlayField-2.html"));
            content.AddPage(PdfSource.FromFile("pages/PlayField-3.html"), PdfSource.Empty());
            content.AddPage(PdfSource.FromFile("pages/PlayField-4.html"));
            content.AddPage(PdfSource.FromFile("pages/PlayField-5.html"), PdfSource.Empty());
            content.AddPage(PdfSource.FromFile("pages/PlayField-6.html"));
            content.AddPage(PdfSource.FromFile("pages/PlayField-7.html"), PdfSource.Empty());
            content.AddPage(PdfSource.FromFile("pages/PlayField-8.html"));
            content.AddPage(PdfSource.FromFile("pages/PlayField-9.html"), PdfSource.Empty());
            content.AddPage(PdfSource.FromFile("pages/PlayField-10.html"));
             
            content.AddPage(PdfSource.FromFile("pages/summary.html"), PdfSource.Empty());

            var bytes = renderer.RenderToBytes();
            Console.WriteLine($"PDF generation took {watch.ElapsedMilliseconds}ms");
            File.WriteAllBytes("output.pdf", bytes);
        }
    }
}

