namespace Foundation.BMC.Services
{
    /// <summary>
    /// Configuration options for the MarketDataCache service.
    /// Bound to the "MarketDataCache" section in appsettings.json.
    ///
    /// Each source has its own TTL (in minutes) to allow independent tuning:
    /// - BrickLink: pricing data from last 6 months of sales — changes slowly
    /// - BrickEconomy: AI valuations updated daily — moderate refresh rate
    /// - BrickOwl: marketplace availability — changes more frequently
    /// </summary>
    public class MarketDataCacheOptions
    {
        public const string SectionName = "MarketDataCache";

        /// <summary>
        /// Cache TTL in minutes for BrickLink price guide responses.
        /// Default: 240 (4 hours).
        /// </summary>
        public int BrickLinkTtlMinutes { get; set; } = 240;

        /// <summary>
        /// Cache TTL in minutes for BrickEconomy valuation responses.
        /// Default: 240 (4 hours).
        /// </summary>
        public int BrickEconomyTtlMinutes { get; set; } = 240;

        /// <summary>
        /// Cache TTL in minutes for BrickOwl availability responses.
        /// Default: 60 (1 hour).
        /// </summary>
        public int BrickOwlTtlMinutes { get; set; } = 60;

        /// <summary>
        /// How often (in minutes) the background purge service runs to clean up expired entries.
        /// Default: 360 (6 hours).
        /// </summary>
        public int PurgeIntervalMinutes { get; set; } = 360;


        /// <summary>
        /// Returns the TTL in minutes for the given marketplace source name.
        /// Falls back to 240 minutes (4 hours) for unknown sources.
        /// </summary>
        public int GetTtlMinutes(string source)
        {
            return source switch
            {
                "BrickLink" => BrickLinkTtlMinutes,
                "BrickEconomy" => BrickEconomyTtlMinutes,
                "BrickOwl" => BrickOwlTtlMinutes,
                _ => 240
            };
        }
    }
}
