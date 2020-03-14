namespace ItechoPdf
{
    // Contains what is regarded as a variable.
    // eg "[page] of [pages]. Page and pages are both variables with respective values
    internal class VariableReplace
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
