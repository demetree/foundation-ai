//
// IpAddressLocationManager — Orchestrates IP geolocation lookups with database caching
//
// This manager sits between the background worker and the IIpGeolocationService provider.
// It checks the IpAddressLocation cache table first, calls the provider only on cache misses,
// and stores results back to the database for future reuse.
//
// The manager also provides the batch resolution method used by IpAddressLocationWorker
// to asynchronously link LoginAttempt records to their geographic locations.
//
// AI-assisted development - February 2026
//

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Foundation.Security.Database;

namespace Foundation.Services
{
    /// <summary>
    ///
    /// Orchestrates IP-to-location lookups with a database-backed cache (IpAddressLocation table).
    ///
    /// Flow for each IP:
    /// 1. Check the IpAddressLocation table for an existing record
    /// 2. On cache miss, call the configured IIpGeolocationService provider
    /// 3. Store the result in the cache table for future reuse
    /// 4. Return the cached record's id for FK linking
    ///
    /// </summary>
    public class IpAddressLocationManager
    {
        private readonly IIpGeolocationService _geolocationService;
        private readonly ILogger<IpAddressLocationManager> _logger;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="geolocationService">The geolocation provider to use for cache misses</param>
        /// <param name="logger">Logger for recording activity and errors</param>
        public IpAddressLocationManager(
            IIpGeolocationService geolocationService,
            ILogger<IpAddressLocationManager> logger)
        {
            _geolocationService = geolocationService;
            _logger = logger;
        }


        /// <summary>
        ///
        /// Gets or creates an IpAddressLocation record for the given IP address.
        ///
        /// Checks the database cache first.  On cache miss, calls the geolocation provider
        /// and stores the result.  Returns the record's id for FK linking, or null if the
        /// lookup failed and no record could be created.
        ///
        /// </summary>
        /// <param name="ipAddress">The IP address to resolve</param>
        /// <returns>The IpAddressLocation record id, or null if resolution failed</returns>
        public async Task<int?> GetOrCreateLocationIdAsync(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress) == true)
            {
                return null;
            }

            try
            {
                //
                // Check the database cache first
                //
                int? existingId = await GetExistingLocationIdAsync(ipAddress);

                if (existingId.HasValue == true)
                {
                    return existingId.Value;
                }

                //
                // Cache miss — call the geolocation provider
                //
                IpGeolocationResult geoResult = await _geolocationService.LookupAsync(ipAddress);

                if (geoResult == null || geoResult.IsSuccess == false)
                {
                    _logger.LogDebug("Geolocation lookup failed for IP {IpAddress}: {Reason}",
                                     ipAddress,
                                     geoResult?.FailureReason ?? "null result");
                    return null;
                }

                //
                // Store the result in the cache table
                //
                int newId = await InsertLocationRecordAsync(ipAddress, geoResult);

                _logger.LogDebug("Created IpAddressLocation record {Id} for IP {IpAddress} ({City}, {Country})",
                                 newId, ipAddress, geoResult.City, geoResult.CountryName);

                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving geolocation for IP {IpAddress}", ipAddress);

                return null;
            }
        }


        /// <summary>
        ///
        /// Resolves all unlinked LoginAttempt records (where ipAddressLocationId IS NULL
        /// and ipAddress IS NOT NULL).
        ///
        /// Groups by unique IP to minimize provider calls, then batch-updates the
        /// LoginAttempt records with the resolved FK.
        ///
        /// </summary>
        /// <param name="maxRecords">Maximum number of unique IPs to process per batch</param>
        /// <returns>The number of LoginAttempt records that were linked</returns>
        public async Task<int> ResolveUnlinkedAttemptsAsync(int maxRecords = 40)
        {
            int totalLinked = 0;

            try
            {
                //
                // Get the distinct IP addresses from unlinked LoginAttempt records
                //
                List<string> unlinkedIps = await GetUnlinkedIpAddressesAsync(maxRecords);

                if (unlinkedIps.Count == 0)
                {
                    return 0;
                }

                _logger.LogDebug("Found {Count} unique unlinked IP addresses to resolve", unlinkedIps.Count);

                //
                // Resolve each unique IP
                //
                foreach (string ip in unlinkedIps)
                {
                    int? locationId = await GetOrCreateLocationIdAsync(ip);

                    if (locationId.HasValue == true)
                    {
                        //
                        // Link all LoginAttempt records with this IP to the resolved location
                        //
                        int linked = await LinkAttemptsToLocationAsync(ip, locationId.Value);

                        totalLinked += linked;
                    }
                }

                if (totalLinked > 0)
                {
                    _logger.LogInformation("Linked {Count} LoginAttempt records to geographic locations", totalLinked);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch resolution of unlinked login attempts");
            }

            return totalLinked;
        }


        /// <summary>
        /// Checks the IpAddressLocation table for an existing record matching the given IP.
        /// </summary>
        /// <param name="ipAddress">The IP address to look up</param>
        /// <returns>The record id if found, null otherwise</returns>
        private async Task<int?> GetExistingLocationIdAsync(string ipAddress)
        {
            using (SecurityContext db = new SecurityContext())
            {
                string sql = "SELECT TOP 1 id FROM [Security].[IpAddressLocation] WHERE ipAddress = @ipAddress AND active = 1 AND deleted = 0";

                using (SqlConnection connection = new SqlConnection(db.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ipAddress", ipAddress);

                        object result = await command.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Inserts a new IpAddressLocation record into the cache table.
        /// </summary>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="geoResult">The geolocation result to store</param>
        /// <returns>The id of the newly inserted record</returns>
        private async Task<int> InsertLocationRecordAsync(string ipAddress, IpGeolocationResult geoResult)
        {
            using (SecurityContext db = new SecurityContext())
            {
                string sql = @"
                    INSERT INTO [Security].[IpAddressLocation]
                        (ipAddress, countryCode, countryName, city, latitude, longitude, lastLookupDate, active, deleted)
                    OUTPUT INSERTED.id
                    VALUES
                        (@ipAddress, @countryCode, @countryName, @city, @latitude, @longitude, @lastLookupDate, 1, 0)";

                using (SqlConnection connection = new SqlConnection(db.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ipAddress", ipAddress);
                        command.Parameters.AddWithValue("@countryCode", (object)geoResult.CountryCode ?? DBNull.Value);
                        command.Parameters.AddWithValue("@countryName", (object)geoResult.CountryName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@city", (object)geoResult.City ?? DBNull.Value);
                        command.Parameters.AddWithValue("@latitude", geoResult.Latitude.HasValue ? (object)geoResult.Latitude.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@longitude", geoResult.Longitude.HasValue ? (object)geoResult.Longitude.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@lastLookupDate", DateTime.UtcNow);

                        object result = await command.ExecuteScalarAsync();

                        return Convert.ToInt32(result);
                    }
                }
            }
        }


        /// <summary>
        /// Gets distinct IP addresses from LoginAttempt records that have no linked IpAddressLocation.
        /// </summary>
        /// <param name="maxCount">Maximum number of distinct IPs to return</param>
        /// <returns>List of unique IP addresses needing resolution</returns>
        private async Task<List<string>> GetUnlinkedIpAddressesAsync(int maxCount)
        {
            List<string> ips = new List<string>();

            using (SecurityContext db = new SecurityContext())
            {
                string sql = @"
                    SELECT DISTINCT TOP (@maxCount) ipAddress
                    FROM [Security].[LoginAttempt]
                    WHERE ipAddressLocationId IS NULL
                      AND ipAddress IS NOT NULL
                      AND ipAddress <> ''
                      AND active = 1
                      AND deleted = 0";

                using (SqlConnection connection = new SqlConnection(db.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@maxCount", maxCount);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync() == true)
                            {
                                string ip = reader.GetString(0);

                                if (string.IsNullOrWhiteSpace(ip) == false)
                                {
                                    ips.Add(ip);
                                }
                            }
                        }
                    }
                }
            }

            return ips;
        }


        /// <summary>
        /// Links all LoginAttempt records with the given IP address to the specified IpAddressLocation record.
        /// </summary>
        /// <param name="ipAddress">The IP address to match</param>
        /// <param name="locationId">The IpAddressLocation id to set</param>
        /// <returns>The number of records updated</returns>
        private async Task<int> LinkAttemptsToLocationAsync(string ipAddress, int locationId)
        {
            using (SecurityContext db = new SecurityContext())
            {
                string sql = @"
                    UPDATE [Security].[LoginAttempt]
                    SET ipAddressLocationId = @locationId
                    WHERE ipAddress = @ipAddress
                      AND ipAddressLocationId IS NULL";

                using (SqlConnection connection = new SqlConnection(db.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@locationId", locationId);
                        command.Parameters.AddWithValue("@ipAddress", ipAddress);

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        return rowsAffected;
                    }
                }
            }
        }
    }
}
