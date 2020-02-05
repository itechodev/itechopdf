using System.IO;

namespace wkpdftoxcorelib
{
    public class PdfDocument
    {
        public PdfDocument(byte[] bytes)
        {
            Bytes = bytes;
        }

        public byte[] Bytes { get; }

        public void SaveToFile(string path)
        {
            File.WriteAllBytes(path, Bytes);
        }
    }
}
