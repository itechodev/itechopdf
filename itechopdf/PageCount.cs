namespace ItechoPdf
{
    public partial class PdfRenderer
    {
        public class PageCount
        {
            public int Overflow { get; set; }
            public int Document { get; set; }
            public int Page { get; set; }

            public int Overflows { get; set; }
            public int Documents { get; set; }
            public int Pages { get; set; }

            public PdfSharp.Pdf.PdfPage PdfPage { get; set; }
            public PdfDocument RenderDocument { get; set; }
            public PdfPage RenderPage { get; set; }
        }
    }
}
