using System.Drawing;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.interaction.annotations;

namespace ItechoPdf
{
    // The original anchor rect in which text should be written 
    // Any other blocks intersection with the rect will be removed.
    internal class ReplaceRect
    {
        public ReplaceRect(RectangleF rect, string text, XAlignmentEnum xAlignment, Annotation annotation)
        {
            AlreadyStamp = false;
            Rect = rect;
            Text = text;
            XAlignment = xAlignment;
            Annotation = annotation;
        }

        public bool AlreadyStamp { get; set; }
        public RectangleF Rect { get; }
        public string Text { get; }
        public XAlignmentEnum XAlignment { get; set; }
        // Fixed Annotation.Box
        public Annotation Annotation { get; set; }
    }
}
