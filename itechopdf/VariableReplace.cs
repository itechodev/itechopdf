namespace ItechoPdf
{
    public class VariableReplace
    {
        public VariableReplace(string name, string replace, VariableAlign align)
        {
            Name = name;
            Replace = replace;
        }

        public string Name { get; set; }
        public string Replace { get; set; }
        public VariableAlign Align { get; set; }
    }
}
