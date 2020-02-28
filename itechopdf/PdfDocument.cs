using System;
using System.Collections.Generic;

namespace ItechoPdf
{
    public enum ResourcePlacement
    {
        Head,
        EndOfDocument
    }

    public enum ResourceType
    {
        StyleSheet,
        Javascript
    }

    public class PdfResource
    {
        public PdfResource(PdfSource source, ResourcePlacement placement, ResourceType type)
        {
            Source = source;
            Placement = placement;
            Type = type;
        }

        public PdfSource Source { get; set; }
        public ResourcePlacement Placement { get; set; }
        public ResourceType Type { get; set; }
    }

    public class PdfDocument
    {
        public PdfSource Source { get; }
        public PdfSettings Settings { get; private set; } = new PdfSettings();

        public HeaderFooter Header { get; private set; }
        public HeaderFooter Footer { get; private set; }
        public List<PdfResource> Resources { get; }

        public PdfDocument(PdfSource source, PdfSettings settings = null)
        {
            Source = source;
            Settings = settings ?? new PdfSettings();
        }

        public void AddCSS(PdfSource content, ResourcePlacement placement = ResourcePlacement.Head)
        {
            Resources.Add(new PdfResource(content, placement, ResourceType.StyleSheet));
        }

        public void AddJavascript(PdfSource content, ResourcePlacement placement = ResourcePlacement.EndOfDocument)
        {
            Resources.Add(new PdfResource(content, placement, ResourceType.Javascript));
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
        
        private void SetHeader(HeaderFooter header)
        {
            Header = header;
        }

        private void SetFooter(HeaderFooter footer)
        {
            Footer = footer;
        }
    
        public void Configure(Action<PdfSettings> settings)
        {
            settings?.Invoke(Settings);
        }
    }
}
