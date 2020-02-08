using System.Globalization;

namespace wkpdftoxcorelib
{
    public class MarginSettings
    {
        public Unit Unit { get; set; }

        public double? Top { get; set; }

        public double? Bottom { get; set; }

        public double? Left { get; set; }

        public double? Right { get; set; }

        public MarginSettings(Unit unit = Unit.Millimeters)
        {
            Unit = unit;
        }

        public MarginSettings(double top, double right, double bottom, double left, Unit unit) : this(unit)
        {
            Set(top, right, bottom, left, unit);
        }

        public void Set(double top, double right, double bottom, double left, Unit unit)
        {
            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
            Unit = unit;
        }

        public string GetMarginValue(double? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            string strUnit = "in";

            switch (Unit)
            {
                case Unit.Inches: strUnit = "in";
                    break;
                case Unit.Millimeters: strUnit = "mm";
                    break;
                case Unit.Centimeters: strUnit = "cm";
                    break;
                default: strUnit = "in";
                    break;
            }

            return value.Value.ToString("0.##", CultureInfo.InvariantCulture) + strUnit;
        }
    }
    
}
