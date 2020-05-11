namespace ItechoPdf
{
    public enum ResourceType
    {
        StyleSheet,
        Javascript
    }

    public enum ResourcePlacement
    {
        Head,
        EndOfDocument
    }


    public class PdfResource
    {
        public PdfResource(PdfSource source, ResourcePlacement placement, ResourceType type)
        {
            Content = source;
            Placement = placement;
            Type = type;
        }

        public PdfSource Content { get; set; }
        public ResourcePlacement Placement { get; set; }
        public ResourceType Type { get; set; }
    }
}
