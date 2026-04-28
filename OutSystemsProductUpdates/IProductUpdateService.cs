using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.OSProductUpdatesProvider
{
    /// <summary>
    /// Represents a single OutSystems Product Update entry.
    /// Maps to an OutSystems Structure.
    /// </summary>
    [OSStructure(Description = "Represents a single OutSystems Product Update entry.")]
    public struct ProductUpdate
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        
        [OSStructureField(Description = "The date the update was published.", DataType = OSDataType.Date)]
        public DateTime PublishDate { get; set; }
    }

    /// <summary>
    /// Service to fetch OutSystems Product Updates via high-performance web scraping.
    /// </summary>
    [OSInterface(Description = "Service to fetch OutSystems Product Updates via high-performance web scraping.", Name = "OSProductUpdatesProvider", IconResourceName = "OutSystems.OSProductUpdatesProvider.Resources.os_product_updates.png")]
    public interface IOSProductUpdatesProvider
    {
        /// <summary>
        /// Fetches product updates for a specific date from a specified OutSystems product updates page.
        /// </summary>
        /// <param name="targetDate">The date to filter results.</param>
        /// <param name="productUpdatesUrl">The URL of the OutSystems product updates page (e.g., https://www.outsystems.com/product-updates/).</param>
        /// <returns>A collection of matching product updates.</returns>
        [OSAction(Description = "Fetches product updates for a specific date from a specified OutSystems product updates page.")]
        IEnumerable<ProductUpdate> GetOSProductUpdatesByDate(DateTime targetDate, string productUpdatesUrl);
    }
}
