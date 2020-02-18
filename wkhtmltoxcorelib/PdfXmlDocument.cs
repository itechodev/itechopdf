using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace wkpdftoxcorelib
{
    [XmlRoot("pdf-renderer")]
    public class PdfXmlRenderer
    {
        
        [XmlElement("pdf-document")]
        public PdfXmlDocument[] Documents { get; set; }
    }

    [XmlRoot("pdf-document")]
    public class PdfXmlDocument
    {
        [XmlAttribute("dpi")]
        public int Dpi { get; set; }


        [XmlAttribute("orientation")]
        public Orientation Orientation { get; set; }
        // etc.

        [XmlElement("pdf-header")]
        public PdfXmlHeaderFooter Header { get; set; }
     
        [XmlElement("pdf-footer")]
        public PdfXmlHeaderFooter Footer { get; set; }

        [XmlElement("pdf-html")]
        public XmlPdfContent Html { get; set; }
    }

    public class PdfXmlHeaderFooter
    {
        [XmlAttribute("line")]
        public bool Line { get; set; }
        
        [XmlAttribute("spacing")]
        public double Spacing { get; set; }

        [XmlAttribute("height")]
        public double Height { get; set; }

        [XmlElement("pdf-html")]
        public XmlPdfContent Html { get; set; }
    }

    
    public class XmlPdfContent : IXmlSerializable
    {
        public string MarkupContent { get; private set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            MarkupContent = reader.ReadInnerXml();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }

}
