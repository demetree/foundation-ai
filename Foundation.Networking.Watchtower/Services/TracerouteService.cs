// ============================================================================
//
// TracerouteService.cs — Hop-by-hop network path discovery.
//
// Implements traceroute by sending ICMP pings with incrementing TTL values.
// Each hop that returns TTL Expired reveals a node in the network path.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using Foundation.Networking.Watchtower.Configuration;

namespace Foundation.Networking.Watchtower.Services
{
    /// <summary>
    ///
    /// Traceroute service — discovers the network path to a destination
    /// by sending ICMP pings with incrementing TTL values.
    ///
    /// </summary>
    public class TracerouteService
    {
        private readonly WatchtowerConfiguration _config;


        public TracerouteService(WatchtowerConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Performs a traceroute to the specified host.
        /// </summary>
        public async Task<TracerouteResult> TraceAsync(string host, CancellationToken cancellationToken = default)
        {
            return await TraceAsync(host, _config.TracerouteMaxHops, _config.TracerouteTimeoutMs, cancellationToken);
        }


        /// <summary>
        /// Performs a traceroute to the specified host with custom settings.
        /// </summary>
        public async Task<TracerouteResult> TraceAsync(string host, int maxHops, int timeoutMs, CancellationToken cancellationToken = default)
        {
            TracerouteResult result = new TracerouteResult
            {
                Host = host,
                TimestampUtc = DateTime.UtcNow
            };

            //
            // Resolve the destination address first
            //
            IPAddress destinationAddress = null;

            try
            {
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(host);

                if (addresses.Length > 0)
                {
                    destinationAddress = addresses[0];
                    result.DestinationAddress = destinationAddress.ToString();
                }
                else
                {
                    return result;
                }
            }
            catch
            {
                return result;
            }

            //
            // Trace by incrementing TTL
            //
            byte[] buffer = new byte[_config.PingBufferSize];

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                TraceHop hop = new TraceHop
                {
                    HopNumber = ttl
                };

                try
                {
                    using (Ping pinger = new Ping())
                    {
                        PingOptions options = new PingOptions(ttl, true);
                        PingReply reply = await pinger.SendPingAsync(host, timeoutMs, buffer, options);

                        if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                        {
                            hop.Responded = true;
                            hop.Address = reply.Address?.ToString() ?? string.Empty;
                            hop.RoundTripTimeMs = reply.RoundtripTime;

                            //
                            // Try reverse DNS lookup
                            //
                            if (reply.Address != null)
                            {
                                try
                                {
                                    IPHostEntry hostEntry = await Dns.GetHostEntryAsync(reply.Address);
                                    hop.Hostname = hostEntry.HostName;
                                }
                                catch
                                {
                                    // Reverse DNS not available — that's fine
                                    hop.Hostname = string.Empty;
                                }
                            }

                            //
                            // Check if we reached the destination
                            //
                            if (reply.Status == IPStatus.Success)
                            {
                                hop.IsDestination = true;
                                result.ReachedDestination = true;
                            }
                        }
                        else
                        {
                            //
                            // Hop timed out or returned an error
                            //
                            hop.Responded = false;
                            hop.Address = "*";
                            hop.RoundTripTimeMs = -1;
                        }
                    }
                }
                catch
                {
                    hop.Responded = false;
                    hop.Address = "*";
                    hop.RoundTripTimeMs = -1;
                }

                result.Hops.Add(hop);

                //
                // Stop if we reached the destination
                //
                if (hop.IsDestination == true)
                {
                    break;
                }
            }

            result.TotalHops = result.Hops.Count;

            return result;
        }
    }
}
