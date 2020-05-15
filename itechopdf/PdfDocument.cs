using System;
using System.Collections.Generic;
using System.IO;

namespace ItechoPdf
{
    public class PdfDocument
    {
        public List<PdfPage> Pages { get; } = new List<PdfPage>();
        public PdfSettings Settings { get; private set; } = new PdfSettings();
        public List<PdfResource> Resources { get; } = new List<PdfResource>();

        public int HeaderHeight { get; set; }
        public int FooterHeight { get; set; }
        public string BaseUrl { get; private set; }

        public PdfDocument(int headerHeightmm = 0, int footerHeightmm = 0, string baseUrl = null, PdfSettings settings = null)
        {
            HeaderHeight = headerHeightmm;
            FooterHeight = footerHeightmm;
            Settings = settings ?? new PdfSettings();
            BaseUrl = baseUrl ?? Environment.CurrentDirectory;
            // make sure baseUrl always ends with directory seperator
            if (!BaseUrl.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                BaseUrl += Path.DirectorySeparatorChar;
            }
        }

        public PdfPage AddPage(PdfSource content, PdfSource header = null, PdfSource footer = null)
        {
            var page = new PdfPage
            {
                Source = content,
                Header = header,
                Footer = footer
            };
            this.Pages.Add(page);
            return page;
        }

        public void AddCSS(PdfSource content, ResourcePlacement placement = ResourcePlacement.Head)
        {
            Resources.Add(new PdfResource(content, placement, ResourceType.StyleSheet));
        }

        public void AddJavascript(PdfSource content, ResourcePlacement placement = ResourcePlacement.EndOfDocument)
        {
            Resources.Add(new PdfResource(content, placement, ResourceType.Javascript));
        }
        
        public void Configure(Action<PdfSettings> settings)
        {
            settings?.Invoke(Settings);
        }
    }
}
