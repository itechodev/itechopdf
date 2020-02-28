using System.Drawing;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.interaction.annotations;

namespace ItechoPdf
{
    internal class ReplaceRect
    {
        public ReplaceRect(RectangleF rect, VariableReplace replacement, XAlignmentEnum xAlignment, Annotation annotation)
        {
            AlreadyStamp = false;
            Rect = rect;
            Replacement = replacement;
            XAlignment = xAlignment;
            Annotation = annotation;
        }

        public bool AlreadyStamp { get; set; }
        public RectangleF Rect { get; }
        public VariableReplace Replacement { get; }
        public XAlignmentEnum XAlignment { get; set; }
        // Fixed Annotation.Box
        public Annotation Annotation { get; set; }

        

    }
}
