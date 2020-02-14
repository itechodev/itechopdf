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

        public void SetHeader(HeaderFooter header)
        {
            PrintSettings.Header = header;
        }

        public void SetFooter(HeaderFooter footer)
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
