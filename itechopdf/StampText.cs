using System;
using System.Collections.Generic;
using System.Drawing;

namespace ItechoPdf
{
    internal class StampTexts
    {
        public List<StampText> Texts { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public StampTexts()
        {
            Texts = new List<StampText>();
        }
        
        internal void Add(StampText item)
        {
            Texts.Add(item);
        }
    }
    
    internal class StampText
    {
        public StampText(PointF point, string text)
        {
            Point = point;
            Text = text;
        }

        public PointF Point { get; set; }
        public string Text { get; set; }
    }
}
