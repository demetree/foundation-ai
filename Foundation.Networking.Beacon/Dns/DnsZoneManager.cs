// ============================================================================
//
// DnsZoneManager.cs — Internal DNS zone management.
//
// Manages internal DNS zone records for service name resolution
// within the Foundation infrastructure.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Foundation.Networking.Beacon.Configuration;

namespace Foundation.Networking.Beacon.Dns
{
    /// <summary>
    /// A DNS record in the zone.
    /// </summary>
    public class DnsRecord
    {
        public string Hostname { get; set; } = string.Empty;
        public string RecordType { get; set; } = "A";
        public string Value { get; set; } = string.Empty;
        public int TtlSeconds { get; set; } = 300;
        public int Priority { get; set; } = 10;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
    }


    /// <summary>
    ///
    /// Manages an internal DNS zone for Foundation service name resolution.
    ///
    /// </summary>
    public class DnsZoneManager
    {
        private readonly ConcurrentDictionary<string, List<DnsRecord>> _records;


        public DnsZoneManager()
        {
            _records = new ConcurrentDictionary<string, List<DnsRecord>>(StringComparer.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Loads zone entries from configuration.
        /// </summary>
        public DnsZoneManager(BeaconConfiguration config) : this()
        {
            if (config.ZoneEntries != null)
            {
                foreach (DnsZoneEntry entry in config.ZoneEntries)
                {
                    AddRecord(new DnsRecord
                    {
                        Hostname = entry.Hostname,
                        RecordType = entry.RecordType,
                        Value = entry.Value,
                        TtlSeconds = entry.TtlSeconds,
                        Priority = entry.Priority
                    });
                }
            }
        }


        /// <summary>
        /// Total number of records.
        /// </summary>
        public int RecordCount
        {
            get { return _records.Values.Sum(list => list.Count); }
        }


        /// <summary>
        /// Number of unique hostnames.
        /// </summary>
        public int HostnameCount => _records.Count;


        /// <summary>
        /// Adds a DNS record.
        /// </summary>
        public void AddRecord(DnsRecord record)
        {
            record.CreatedUtc = DateTime.UtcNow;
            record.UpdatedUtc = DateTime.UtcNow;

            string key = NormalizeHostname(record.Hostname);

            List<DnsRecord> records = _records.GetOrAdd(key, _ => new List<DnsRecord>());

            lock (records)
            {
                records.Add(record);
            }
        }


        /// <summary>
        /// Removes all records for a hostname.
        /// </summary>
        public bool RemoveHost(string hostname)
        {
            string key = NormalizeHostname(hostname);
            return _records.TryRemove(key, out _);
        }


        /// <summary>
        /// Removes a specific record type for a hostname.
        /// </summary>
        public int RemoveRecords(string hostname, string recordType)
        {
            string key = NormalizeHostname(hostname);

            if (_records.TryGetValue(key, out List<DnsRecord> records))
            {
                lock (records)
                {
                    int removed = records.RemoveAll(r =>
                        string.Equals(r.RecordType, recordType, StringComparison.OrdinalIgnoreCase));

                    if (records.Count == 0)
                    {
                        _records.TryRemove(key, out _);
                    }

                    return removed;
                }
            }

            return 0;
        }


        /// <summary>
        /// Queries records for a hostname, optionally filtered by record type.
        /// </summary>
        public List<DnsRecord> Query(string hostname, string recordType = null)
        {
            string key = NormalizeHostname(hostname);

            if (_records.TryGetValue(key, out List<DnsRecord> records))
            {
                lock (records)
                {
                    if (string.IsNullOrEmpty(recordType))
                    {
                        return records.ToList();
                    }

                    return records
                        .Where(r => string.Equals(r.RecordType, recordType, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }

            return new List<DnsRecord>();
        }


        /// <summary>
        /// Gets all records across all hostnames.
        /// </summary>
        public List<DnsRecord> GetAllRecords()
        {
            List<DnsRecord> all = new List<DnsRecord>();

            foreach (var kvp in _records)
            {
                lock (kvp.Value)
                {
                    all.AddRange(kvp.Value);
                }
            }

            return all;
        }


        /// <summary>
        /// Gets all unique hostnames.
        /// </summary>
        public List<string> GetHostnames()
        {
            return _records.Keys.ToList();
        }


        /// <summary>
        /// Updates all records for a hostname with a new value.
        /// </summary>
        public int UpdateRecords(string hostname, string recordType, string newValue)
        {
            string key = NormalizeHostname(hostname);
            int updated = 0;

            if (_records.TryGetValue(key, out List<DnsRecord> records))
            {
                lock (records)
                {
                    foreach (DnsRecord record in records)
                    {
                        if (string.Equals(record.RecordType, recordType, StringComparison.OrdinalIgnoreCase))
                        {
                            record.Value = newValue;
                            record.UpdatedUtc = DateTime.UtcNow;
                            updated++;
                        }
                    }
                }
            }

            return updated;
        }


        /// <summary>
        /// Clears all records.
        /// </summary>
        public void Clear()
        {
            _records.Clear();
        }


        // ── Internal ──────────────────────────────────────────────────────


        private static string NormalizeHostname(string hostname)
        {
            return hostname?.Trim().ToLowerInvariant() ?? string.Empty;
        }
    }
}
