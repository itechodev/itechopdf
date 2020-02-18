using System.IO;

namespace wkpdftoxcorelib
{
    public abstract class PdfSource
    {
        public static PdfSourceFile FromFile(string path)
        {
            return new PdfSourceFile(path);
        }

        public static PdfSourceHtml FromStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return new PdfSourceHtml(reader.ReadToEnd());
            }
        }

        public static PdfSource FromHtml(string html, string baseUrl = null)
        {
            return new PdfSourceHtml(html, baseUrl);
        }
    }
}
