using System.Drawing;

namespace ItechoPdf
{
    internal class ReplaceRect
    {
        public bool AlreadyStamp { get; set; }
        public RectangleF Rect { get; }
        public VariableReplace Replacement { get; }

        public ReplaceRect(bool alreadyStamp, RectangleF rect, VariableReplace replacement)
        {
            AlreadyStamp = alreadyStamp;
            Rect = rect;
            Replacement = replacement;
        }

    }
}
