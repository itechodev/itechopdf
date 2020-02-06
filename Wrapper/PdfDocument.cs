using System;
using System.Collections.Generic;

namespace wkpdftoxcorelib.Wrapper
{
    public class PdfDocument
    {
        private List<string> _tempFiles = new List<string>();

        private LoadSettings LoadSettings { get; } = new LoadSettings();
        private PrintSettings PrintSettings { get; } = new PrintSettings();

        public void AddStandardHeader(string left, string center, string right, double? spacing = null, bool line = true, int? fontSize = null, string fontName = null)
        {
            new StandardHeaderFooter
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
            new HtmlHeaderFooter(html) 
            {
                Height = height,
                Spacing = spacing,
                Line = line
            };
        }
        
        public void AddFileHeader(string filePath, double? height = null, double? spacing = null, bool line = false)
        {
            new FileHeaderFooter(filePath) 
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

        }

        public void FromFile(string filename)
        {

        }

        public void AppendDocument(PdfDocument doc)
        {

        }

        public void PrependDocument(PdfDocument doc)
        {

        }

        public byte[] Render()
        {
            return null;
        }
    }
}
