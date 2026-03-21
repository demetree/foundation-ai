// ============================================================================
//
// PingService.cs — ICMP ping with statistics.
//
// Provides single-ping and multi-ping (with aggregated statistics) methods
// using System.Net.NetworkInformation.Ping.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using Foundation.Networking.Watchtower.Configuration;

namespace Foundation.Networking.Watchtower.Services
{
    /// <summary>
    ///
    /// ICMP ping service with single-ping and multi-ping (statistics) methods.
    ///
    /// </summary>
    public class PingService
    {
        private readonly WatchtowerConfiguration _config;


        public PingService(WatchtowerConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Sends a single ICMP ping to the specified host.
        /// </summary>
        public async Task<PingResult> PingAsync(string host, CancellationToken cancellationToken = default)
        {
            return await PingAsync(host, _config.PingTimeoutMs, _config.PingTtl, cancellationToken);
        }


        /// <summary>
        /// Sends a single ICMP ping to the specified host with custom timeout and TTL.
        /// </summary>
        public async Task<PingResult> PingAsync(string host, int timeoutMs, int ttl, CancellationToken cancellationToken = default)
        {
            PingResult result = new PingResult
            {
                Host = host,
                TimestampUtc = DateTime.UtcNow
            };

            try
            {
                using (Ping pinger = new Ping())
                {
                    byte[] buffer = new byte[_config.PingBufferSize];
                    PingOptions options = new PingOptions(ttl, true);

                    PingReply reply = await pinger.SendPingAsync(host, timeoutMs, buffer, options);

                    result.Status = reply.Status;
                    result.Success = (reply.Status == IPStatus.Success);

                    if (result.Success == true)
                    {
                        result.RoundTripTimeMs = reply.RoundtripTime;
                        result.Address = reply.Address?.ToString() ?? string.Empty;
                        result.Ttl = reply.Options?.Ttl ?? 0;
                        result.BufferSize = reply.Buffer?.Length ?? 0;
                    }
                    else
                    {
                        result.ErrorMessage = reply.Status.ToString();

                        if (reply.Address != null)
                        {
                            result.Address = reply.Address.ToString();
                        }
                    }
                }
            }
            catch (PingException ex)
            {
                result.Success = false;
                result.Status = IPStatus.Unknown;
                result.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Status = IPStatus.Unknown;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Sends multiple ICMP pings to the specified host and returns aggregated statistics.
        /// </summary>
        public async Task<PingStatistics> PingWithStatisticsAsync(string host, int count = 4, CancellationToken cancellationToken = default)
        {
            return await PingWithStatisticsAsync(host, count, _config.PingTimeoutMs, _config.PingTtl, cancellationToken);
        }


        /// <summary>
        /// Sends multiple ICMP pings with custom settings and returns aggregated statistics.
        /// </summary>
        public async Task<PingStatistics> PingWithStatisticsAsync(string host, int count, int timeoutMs, int ttl, CancellationToken cancellationToken = default)
        {
            PingStatistics stats = new PingStatistics
            {
                Host = host,
                Sent = count
            };

            List<long> rttValues = new List<long>();

            for (int i = 0; i < count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                PingResult result = await PingAsync(host, timeoutMs, ttl, cancellationToken);
                stats.Results.Add(result);

                if (result.Success == true)
                {
                    stats.Received++;
                    rttValues.Add(result.RoundTripTimeMs);
                }
                else
                {
                    stats.Lost++;
                }

                //
                // Small delay between pings to avoid flooding
                //
                if (i < count - 1)
                {
                    await Task.Delay(100, cancellationToken);
                }
            }

            //
            // Calculate statistics
            //
            stats.LossPercent = stats.Sent > 0 ? ((double)stats.Lost / stats.Sent) * 100.0 : 0;

            if (rttValues.Count > 0)
            {
                stats.MinRttMs = rttValues.Min();
                stats.MaxRttMs = rttValues.Max();
                stats.AvgRttMs = rttValues.Average();

                //
                // Calculate jitter (standard deviation)
                //
                if (rttValues.Count > 1)
                {
                    double mean = stats.AvgRttMs;
                    double sumSquaredDiffs = 0;

                    foreach (long rtt in rttValues)
                    {
                        double diff = rtt - mean;
                        sumSquaredDiffs += diff * diff;
                    }

                    stats.JitterMs = Math.Sqrt(sumSquaredDiffs / rttValues.Count);
                }
            }

            return stats;
        }
    }
}
