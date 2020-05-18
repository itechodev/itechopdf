namespace ItechoPdf
{
    // Contains what is regarded as a variable.
    // eg "[page] of [pages]. Page and pages are both variables with respective values
    public class VariableReplace
    {
        public VariableReplace(string name, string replace)
        {
            Name = name;
            Replace = replace;
        }

        public VariableReplace(string name, int replace)
        {
            Name = name;
            Replace = replace.ToString();
        }

        public string Name { get; set; }
        public string Replace { get; set; }
    }
}
