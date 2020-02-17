namespace wkpdftoxcorelib
{
    public class PdfSourceHtml : PdfSource
    {
        public string BaseUrl { get; }

        public PdfSourceHtml(string html, string baseUrl = null)
        {
            Html = html;
            BaseUrl = baseUrl;
        }

        public string Html { get; }
    }
}
