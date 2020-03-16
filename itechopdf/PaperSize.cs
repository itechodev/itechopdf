using System.Collections.Generic;

namespace ItechoPdf
{
    public class PaperSize
    {
        private const double inchToMM = 25.4;
        private static readonly Dictionary<PaperKind, PaperSize> dictionary = new Dictionary<PaperKind, PaperSize>()
        {
            // paper sizes from http://msdn.microsoft.com/en-us/library/system.drawing.printing.paperkind.aspx
            { PaperKind.Letter, new PaperSize(8.5 * inchToMM, 11 * inchToMM) },
            { PaperKind.Legal, new PaperSize(8.5 * inchToMM, 14 * inchToMM) },
            { PaperKind.A4, new PaperSize(210, 297) },
            { PaperKind.CSheet, new PaperSize(17 * inchToMM, 22 * inchToMM) },
            { PaperKind.DSheet, new PaperSize(22 * inchToMM, 34 * inchToMM) },
            { PaperKind.ESheet, new PaperSize(34 * inchToMM, 44 * inchToMM) },
            { PaperKind.LetterSmall, new PaperSize(8.5 * inchToMM, 11 * inchToMM) },
            { PaperKind.Tabloid, new PaperSize(11 * inchToMM, 17 * inchToMM) },
            { PaperKind.Ledger, new PaperSize(17 * inchToMM, 11 * inchToMM) },
            { PaperKind.Statement, new PaperSize(5.5 * inchToMM, 8.5 * inchToMM) },
            { PaperKind.Executive, new PaperSize(7.25 * inchToMM, 10.5 * inchToMM) },
            { PaperKind.A3, new PaperSize(297, 420) },
            { PaperKind.A4Small, new PaperSize(210, 297) },
            { PaperKind.A5, new PaperSize(148, 210) },
            { PaperKind.B4, new PaperSize(250, 353) },
            { PaperKind.B5, new PaperSize(176, 250) },
            { PaperKind.Folio, new PaperSize(8.5 * inchToMM, 13 * inchToMM) },
            { PaperKind.Quarto, new PaperSize(215, 275) },
            { PaperKind.Standard10x14, new PaperSize(10 * inchToMM, 14 * inchToMM) },
            { PaperKind.Standard11x17, new PaperSize(11 * inchToMM, 17 * inchToMM) },
            { PaperKind.Note, new PaperSize(8.5 * inchToMM, 11 * inchToMM) },
            { PaperKind.Number9Envelope, new PaperSize(3.875 * inchToMM, 8.875 * inchToMM) },
            { PaperKind.Number10Envelope, new PaperSize(4.125 * inchToMM, 9.5 * inchToMM) },
            { PaperKind.Number11Envelope, new PaperSize(4.5 * inchToMM, 10.375 * inchToMM) },
            { PaperKind.Number12Envelope, new PaperSize(4.75 * inchToMM, 11 * inchToMM) },
            { PaperKind.Number14Envelope, new PaperSize(5 * inchToMM, 11.5 * inchToMM) },
            { PaperKind.DLEnvelope, new PaperSize(110, 220) },
            { PaperKind.C5Envelope, new PaperSize(162, 229) },
            { PaperKind.C3Envelope, new PaperSize(324, 458) },
            { PaperKind.C4Envelope, new PaperSize(229, 324) },
            { PaperKind.C6Envelope, new PaperSize(114, 162) },
            { PaperKind.C65Envelope, new PaperSize(114, 229) },
            { PaperKind.B4Envelope, new PaperSize(250, 353) },
            { PaperKind.B5Envelope, new PaperSize(176, 250) },
            { PaperKind.B6Envelope, new PaperSize(176, 125) },
            { PaperKind.ItalyEnvelope, new PaperSize(110, 230) },
            { PaperKind.MonarchEnvelope, new PaperSize(3.875 * inchToMM, 7.5 * inchToMM) },
            { PaperKind.PersonalEnvelope, new PaperSize(3.625 * inchToMM, 6.5 * inchToMM) },
            { PaperKind.USStandardFanfold, new PaperSize(14.875 * inchToMM, 11 * inchToMM) },
            { PaperKind.GermanStandardFanfold, new PaperSize(8.5 * inchToMM, 12 * inchToMM) },
            { PaperKind.GermanLegalFanfold, new PaperSize(8.5 * inchToMM, 13 * inchToMM) },
            { PaperKind.IsoB4, new PaperSize(250, 353) },
            { PaperKind.JapanesePostcard, new PaperSize(100, 148) },
            { PaperKind.Standard9x11, new PaperSize(9 * inchToMM, 11 * inchToMM) },
            { PaperKind.Standard10x11, new PaperSize(10 * inchToMM, 11 * inchToMM) },
            { PaperKind.Standard15x11, new PaperSize(15 * inchToMM, 11 * inchToMM) },
            { PaperKind.InviteEnvelope, new PaperSize(220, 220) },
            { PaperKind.LetterExtra, new PaperSize(9.275 * inchToMM, 12 * inchToMM) },
            { PaperKind.LegalExtra, new PaperSize(9.275 * inchToMM, 15 * inchToMM) },
            { PaperKind.TabloidExtra, new PaperSize(11.69 * inchToMM, 18 * inchToMM) },
            { PaperKind.A4Extra, new PaperSize(236, 322) },
            { PaperKind.LetterTransverse, new PaperSize(8.275 * inchToMM, 11 * inchToMM) },
            { PaperKind.A4Transverse, new PaperSize(210, 297) },
            { PaperKind.LetterExtraTransverse, new PaperSize(9.275 * inchToMM, 12 * inchToMM) },
            { PaperKind.APlus, new PaperSize(227, 356) },
            { PaperKind.BPlus, new PaperSize(305, 487) },
            { PaperKind.LetterPlus, new PaperSize(8.5 * inchToMM, 12.69 * inchToMM) },
            { PaperKind.A4Plus, new PaperSize(210, 330) },
            { PaperKind.A5Transverse, new PaperSize(148, 210) },
            { PaperKind.B5Transverse, new PaperSize(182, 257) },
            { PaperKind.A3Extra, new PaperSize(322, 445) },
            { PaperKind.A5Extra, new PaperSize(174, 235) },
            { PaperKind.B5Extra, new PaperSize(201, 276) },
            { PaperKind.A2, new PaperSize(420, 594) },
            { PaperKind.A3Transverse, new PaperSize(297, 420) },
            { PaperKind.A3ExtraTransverse, new PaperSize(322, 445) },
            { PaperKind.JapaneseDoublePostcard, new PaperSize(200, 148) },
            { PaperKind.A6, new PaperSize(105, 148) },
            { PaperKind.LetterRotated, new PaperSize(11 * inchToMM, 8.5 * inchToMM) },
            { PaperKind.A3Rotated, new PaperSize(420, 297) },
            { PaperKind.A4Rotated, new PaperSize(297, 210) },
            { PaperKind.A5Rotated, new PaperSize(210, 148) },
            { PaperKind.B4JisRotated, new PaperSize(364, 257) },
            { PaperKind.B5JisRotated, new PaperSize(257, 182) },
            { PaperKind.JapanesePostcardRotated, new PaperSize(148, 100) },
            { PaperKind.JapaneseDoublePostcardRotated, new PaperSize(148, 200) },
            { PaperKind.A6Rotated, new PaperSize(148, 105) },
            { PaperKind.B6Jis, new PaperSize(128, 182) },
            { PaperKind.B6JisRotated, new PaperSize(182, 128) },
            { PaperKind.Standard12x11, new PaperSize(12 * inchToMM, 11 * inchToMM) },
            { PaperKind.Prc16K, new PaperSize(146, 215) },
            { PaperKind.Prc32K, new PaperSize(97, 151) },
            { PaperKind.Prc32KBig, new PaperSize(97, 151) },
            { PaperKind.PrcEnvelopeNumber1, new PaperSize(102, 165) },
            { PaperKind.PrcEnvelopeNumber2, new PaperSize(102, 176) },
            { PaperKind.PrcEnvelopeNumber3, new PaperSize(125, 176) },
            { PaperKind.PrcEnvelopeNumber4, new PaperSize(110, 208) },
            { PaperKind.PrcEnvelopeNumber5, new PaperSize(110, 220) },
            { PaperKind.PrcEnvelopeNumber6, new PaperSize(120, 230) },
            { PaperKind.PrcEnvelopeNumber7, new PaperSize(160, 230) },
            { PaperKind.PrcEnvelopeNumber8, new PaperSize(120, 309) },
            { PaperKind.PrcEnvelopeNumber9, new PaperSize(229, 324) },
            { PaperKind.PrcEnvelopeNumber10, new PaperSize(324, 458) },
            { PaperKind.Prc16KRotated, new PaperSize(146, 215) },
            { PaperKind.Prc32KRotated, new PaperSize(97, 151) },
            { PaperKind.Prc32KBigRotated, new PaperSize(97, 151) },
            { PaperKind.PrcEnvelopeNumber1Rotated, new PaperSize(165, 102) },
            { PaperKind.PrcEnvelopeNumber2Rotated, new PaperSize(176, 102) },
            { PaperKind.PrcEnvelopeNumber3Rotated, new PaperSize(176, 125) },
            { PaperKind.PrcEnvelopeNumber4Rotated, new PaperSize(208, 110) },
            { PaperKind.PrcEnvelopeNumber5Rotated, new PaperSize(220, 110) },
            { PaperKind.PrcEnvelopeNumber6Rotated, new PaperSize(230, 120) },
            { PaperKind.PrcEnvelopeNumber7Rotated, new PaperSize(230, 160) },
            { PaperKind.PrcEnvelopeNumber8Rotated, new PaperSize(309, 120) },
            { PaperKind.PrcEnvelopeNumber9Rotated, new PaperSize(324, 229) },
            { PaperKind.PrcEnvelopeNumber10Rotated, new PaperSize(458, 324) },
        };

        public PaperSize(double width, double height)
        {
            this.Width = width;
            this.Height = height;
        }

        public double Height { get; set; }

        public double Width { get; set; }

        public static implicit operator PaperSize(PaperKind paperKind)
        {
            return dictionary[paperKind];
        }
    }
    
}
