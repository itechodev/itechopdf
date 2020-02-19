using System;

namespace ItechoPdf
{

    public class PdfDocument
    {
        public LoadSettings LoadSettings { get; set; } = new LoadSettings();
        public PrintSettings PrintSettings { get; set; } = new PrintSettings();
        public PdfSource Source { get; }

        public PdfDocument(PdfSource source)
        {
            Source = source;
            // Default margin to 1 inch
            PrintSettings.Margins.Set(1, 1, 1, 1, Unit.Inches);
            PrintSettings.PrintBackground = true;
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

        public void SetHeader(PdfSource source, double height, double? spacing = null, bool? line = null)
        {
            SetHeader(new HtmlHeaderFooter(source)
            {   
                Height = height,
                Spacing = spacing,
                Line = line
            });
        }

        public void SetFooter(PdfSource source, double height, double? spacing = null, bool? line = null)
        {
            SetFooter(new HtmlHeaderFooter(source)
            {   
                Height = height,
                Spacing = spacing,
                Line = line
            });
        }
        
        public void SetFooter(string left, string center, string right, int? fontSize = null, string fontName = null)
        {
            SetFooter(new StandardHeaderFooter(left, center, right, fontSize, fontName));
        }
 
        public void SetHeader(string left, string center, string right, int? fontSize = null, string fontName = null)
        {
            SetHeader(new StandardHeaderFooter(left, center, right, fontSize, fontName));
        }

        private void SetHeader(HeaderFooter header)
        {
            PrintSettings.Header = header;
        }

        private void SetFooter(HeaderFooter footer)
        {
            PrintSettings.Footer = footer;
        }
    
        public void Configure(Action<PrintSettings> print, Action<LoadSettings> load = null)
        {
            print?.Invoke(PrintSettings);
            load?.Invoke(LoadSettings);
        }
    }
}
