// ============================================================================
//
// DnsResolver.cs — DNS resolution service.
//
// Performs DNS lookups (forward, reverse, record-type queries) using
// the system resolver, exposing structured results.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Foundation.Networking.Beacon.Configuration;

namespace Foundation.Networking.Beacon.Dns
{
    /// <summary>
    /// Result of a DNS lookup.
    /// </summary>
    public class DnsLookupResult
    {
        public string Hostname { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public List<string> Addresses { get; set; } = new List<string>();
        public List<string> Aliases { get; set; } = new List<string>();
        public long QueryTimeMs { get; set; }
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }


    /// <summary>
    /// Result of a reverse DNS lookup.
    /// </summary>
    public class ReverseDnsResult
    {
        public string IpAddress { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Hostname { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public long QueryTimeMs { get; set; }
    }


    /// <summary>
    ///
    /// DNS resolver for forward and reverse lookups.
    ///
    /// </summary>
    public class DnsResolver
    {
        private readonly BeaconConfiguration _config;
        private readonly ILogger<DnsResolver> _logger;


        public DnsResolver(BeaconConfiguration config, ILogger<DnsResolver> logger)
        {
            _config = config;
            _logger = logger;
        }


        /// <summary>
        /// Resolves a hostname to IP addresses.
        /// </summary>
        public async Task<DnsLookupResult> ResolveAsync(string hostname, CancellationToken cancellationToken = default)
        {
            DnsLookupResult result = new DnsLookupResult
            {
                Hostname = hostname,
                TimestampUtc = DateTime.UtcNow
            };

            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    cts.CancelAfter(_config.DnsTimeoutMs);

                    IPHostEntry entry = await System.Net.Dns.GetHostEntryAsync(hostname);
                    sw.Stop();

                    result.Success = true;
                    result.QueryTimeMs = sw.ElapsedMilliseconds;
                    result.Addresses = entry.AddressList.Select(a => a.ToString()).ToList();
                    result.Aliases = entry.Aliases?.ToList() ?? new List<string>();
                }
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                result.Success = false;
                result.Error = "DNS query timed out";
                result.QueryTimeMs = sw.ElapsedMilliseconds;
            }
            catch (SocketException ex)
            {
                sw.Stop();
                result.Success = false;
                result.Error = ex.Message;
                result.QueryTimeMs = sw.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                sw.Stop();
                result.Success = false;
                result.Error = ex.Message;
                result.QueryTimeMs = sw.ElapsedMilliseconds;
            }

            return result;
        }


        /// <summary>
        /// Resolves multiple hostnames concurrently.
        /// </summary>
        public async Task<List<DnsLookupResult>> ResolveBatchAsync(IEnumerable<string> hostnames, CancellationToken cancellationToken = default)
        {
            List<Task<DnsLookupResult>> tasks = new List<Task<DnsLookupResult>>();

            foreach (string hostname in hostnames)
            {
                tasks.Add(ResolveAsync(hostname, cancellationToken));
            }

            DnsLookupResult[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }


        /// <summary>
        /// Performs a reverse DNS lookup (IP → hostname).
        /// </summary>
        public async Task<ReverseDnsResult> ReverseLookupAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            ReverseDnsResult result = new ReverseDnsResult
            {
                IpAddress = ipAddress
            };

            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (IPAddress.TryParse(ipAddress, out IPAddress addr) == false)
                {
                    sw.Stop();
                    result.Error = "Invalid IP address";
                    return result;
                }

                IPHostEntry entry = await System.Net.Dns.GetHostEntryAsync(addr);
                sw.Stop();

                result.Success = true;
                result.Hostname = entry.HostName;
                result.QueryTimeMs = sw.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                sw.Stop();
                result.Error = ex.Message;
                result.QueryTimeMs = sw.ElapsedMilliseconds;
            }

            return result;
        }
    }
}
