namespace wkpdftoxcorelib.Settings
{
    public abstract class HeaderFooter
    {
        /// <summary>
        /// Whether a line should be printed above the footer. Default = false
        /// </summary>
        public bool? Line { get; set; }

        /// <summary>
        /// The amount of space to put between the footer and the content, e.g. "1.8". Be aware that if this is too large the footer will be printed outside the pdf document. This can be corrected with the margin.bottom setting. Default = 0.00
        /// </summary>
        public double? Spacing { get; set; }

    }
    
}
