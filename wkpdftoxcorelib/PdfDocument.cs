using System;

namespace wkpdftoxcorelib
{

    public class PdfDocument
    {
        public LoadSettings LoadSettings { get; set; } = new LoadSettings();
        public PrintSettings PrintSettings { get; set; } = new PrintSettings();
        public string HtmlContent { get; private set; }
        public string FileContent { get; private set; }

        public PdfDocument()
        {
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

        public void AddHtmlHeader(string html, double? height = null, double? spacing = null, bool line = false)
        {
            PrintSettings.Header = new HtmlHeaderFooter(html) 
            {
                Height = height,
                Spacing = spacing,
                Line = line
            };
        }
        
        public void AddFileHeader(string filePath, double? height = null, double? spacing = null, bool line = false)
        {
            PrintSettings.Header = new FileHeaderFooter(filePath) 
            {
                Height = height,
                Spacing = spacing,
                Line = line
            };
        }

        public void AddHtmlFooter(string html, double? height = null, double? spacing = null, bool line = false)
        {
            PrintSettings.Footer = new HtmlHeaderFooter(html) 
            {
                Height = height,
                Spacing = spacing,
                Line = line
            };
        }
        
        public void AddFileFooter(string filePath, double? height = null, double? spacing = null, bool line = false)
        {
            PrintSettings.Footer = new FileHeaderFooter(filePath) 
            {
                Height = height,
                Spacing = spacing,
                Line = line
            };
        }

        public void Configure(Action<PrintSettings> print, Action<LoadSettings> load = null)
        {
            print?.Invoke(PrintSettings);
            load?.Invoke(LoadSettings);
        }

        public void FromHtml(string html)
        {
            if (!String.IsNullOrEmpty(FileContent))
            {
                throw new Exception($"Source already to file {FileContent}. Create another document.");
            }
            HtmlContent = html;
        }

        public void FromFile(string filename)
        {
            if (!String.IsNullOrEmpty(HtmlContent))
            {
                throw new Exception($"Source already set as HTML. Create another document.");
            }
            FileContent = filename;
        }

    }
}
