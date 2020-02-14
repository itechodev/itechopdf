namespace wkpdftoxcorelib
{
    public class PdfSourceHtml : PdfSource
    {
        public PdfSourceHtml(string html)
        {
            Html = html;
        }

        public string Html { get; }
    }
}
