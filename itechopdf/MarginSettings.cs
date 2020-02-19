using System.Globalization;

namespace ItechoPdf
{
    public class MarginSettings
    {
        // always in mm as the header and footer sizes are measures in mm

        public double? Top { get; set; }

        public double? Bottom { get; set; }

        public double? Left { get; set; }

        public double? Right { get; set; }

        public MarginSettings()
        {
        }

        public MarginSettings(double top, double right, double bottom, double left, Unit unit)
        {
            Set(top, right, bottom, left, unit);
        }

        public void Set(double top, double right, double bottom, double left, Unit unit)
        {
            Top = ConvertToMM(top, unit);
            Bottom = ConvertToMM(bottom, unit);
            Left = ConvertToMM(left, unit);
            Right = ConvertToMM(right, unit);
        }

        private double ConvertToMM(double value, Unit unit)
        {
            switch (unit)
            {
                case Unit.Inches:
                    return value * 25.4;
                case Unit.Centimeters:
                    return value * 10;
                case Unit.Millimeters:
                default:
                    return value;
            }
        }

        public string GetMarginValue(double? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return value.Value.ToString("0.##", CultureInfo.InvariantCulture) + "mm";
        }
    }
    
}
