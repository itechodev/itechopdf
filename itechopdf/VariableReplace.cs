namespace ItechoPdf
{
    public class VariableReplace
    {
        public VariableReplace(string name, string replace)
        {
            Name = name;
            Replace = replace;
        }

        public string Name { get; set; }
        public string Replace { get; set; }
    }
}
