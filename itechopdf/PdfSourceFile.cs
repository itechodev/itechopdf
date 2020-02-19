namespace ItechoPdf
{
    public class PdfSourceFile : PdfSource
    {
        public PdfSourceFile(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
