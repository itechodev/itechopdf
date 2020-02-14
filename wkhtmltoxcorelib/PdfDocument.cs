using System;

namespace wkpdftoxcorelib
{

    public class PdfDocument
    {
        public LoadSettings LoadSettings { get; set; } = new LoadSettings();
        public PrintSettings PrintSettings { get; set; } = new PrintSettings();
        public PdfSource Source { get; }

        public PdfDocument(PdfSource source)
        {
            Source = source;
        }

        public void AddStandardHeader(string left, string center, string right, double? spacing = null, bool line = true, int? fontSize = null, string fontName = null)
        {
            PrintSettings.Header = new StandardHeaderFooter
            {
                Center = center,
                FontName = fontName,
                FontSize = fontSize,
                Left = left,
                Line = line,
                Right = right,
                Spacing = spacing
            };
        }

        public void SetHeader(PdfSource source, double? height = null, double? spacing = null, bool line = false)
        {
            if (source is PdfSourceFile file)
            {
                PrintSettings.Header = new FileHeaderFooter(file.Path) 
                {
                    Height = height,
                    Spacing = spacing,
                    Line = line
                };
            }
            if (source is PdfSourceHtml html)
            {
                PrintSettings.Header = new HtmlHeaderFooter(html.Html)
                {
                    Height = height,
                    Spacing = spacing,
                    Line = line
                };
            }
        }

        public void SetFooter(PdfSource source, double? height = null, double? spacing = null, bool line = false)
        {
            if (source is PdfSourceFile file)
            {
                PrintSettings.Footer = new FileHeaderFooter(file.Path) 
                {
                    Height = height,
                    Spacing = spacing,
                    Line = line
                };
            }
            if (source is PdfSourceHtml html)
            {
                PrintSettings.Footer = new HtmlHeaderFooter(html.Html)
                {
                    Height = height,
                    Spacing = spacing,
                    Line = line
                };
            }
        }
        
        public void Configure(Action<PrintSettings> print, Action<LoadSettings> load = null)
        {
            print?.Invoke(PrintSettings);
            load?.Invoke(LoadSettings);
        }
    }
}
