namespace ItechoPdf
{
    public enum VariableAlign
    {
        Left,
        Center, 
        Right,
    }
    
    public class VariableStyle
    {
        public VariableStyle(VariableAlign align, int maxDigits)
        {
            Align = align;
            MaxDigits = maxDigits;
        }

        public VariableAlign Align { get; set; } 
        public int MaxDigits { get; set; } 
        
    }
}