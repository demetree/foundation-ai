// ============================================================================
//
// PortScannerService.cs — TCP port scanner with service identification.
//
// Performs TCP connect scans against specified port ranges with configurable
// concurrency.  Optionally reads service banners from open ports.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Foundation.Networking.Watchtower.Configuration;

namespace Foundation.Networking.Watchtower.Services
{
    /// <summary>
    ///
    /// TCP port scanner with concurrent scanning and service identification.
    ///
    /// </summary>
    public class PortScannerService
    {
        private readonly WatchtowerConfiguration _config;

        //
        // Well-known port to service name mapping
        //
        private static readonly Dictionary<int, string> WellKnownPorts = new Dictionary<int, string>
        {
            { 21, "FTP" },
            { 22, "SSH" },
            { 23, "Telnet" },
            { 25, "SMTP" },
            { 53, "DNS" },
            { 80, "HTTP" },
            { 110, "POP3" },
            { 143, "IMAP" },
            { 443, "HTTPS" },
            { 465, "SMTPS" },
            { 587, "SMTP Submission" },
            { 993, "IMAPS" },
            { 995, "POP3S" },
            { 1433, "SQL Server" },
            { 1521, "Oracle" },
            { 3306, "MySQL" },
            { 3389, "RDP" },
            { 3478, "STUN/TURN" },
            { 5432, "PostgreSQL" },
            { 5349, "STUN/TURN TLS" },
            { 5672, "AMQP" },
            { 6379, "Redis" },
            { 8080, "HTTP Proxy" },
            { 8443, "HTTPS Alt" },
            { 10100, "Foundation HTTP" },
            { 10101, "Foundation HTTPS" },
            { 27017, "MongoDB" }
        };


        public PortScannerService(WatchtowerConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Scans a list of specific ports on the given host.
        /// </summary>
        public async Task<PortScanReport> ScanPortsAsync(string host, List<int> ports, CancellationToken cancellationToken = default)
        {
            return await ScanPortsAsync(host, ports, _config.PortScanTimeoutMs, cancellationToken);
        }


        /// <summary>
        /// Scans a list of specific ports on the given host with custom timeout.
        /// </summary>
        public async Task<PortScanReport> ScanPortsAsync(string host, List<int> ports, int timeoutMs, CancellationToken cancellationToken = default)
        {
            Stopwatch sw = Stopwatch.StartNew();

            PortScanReport report = new PortScanReport
            {
                Host = host,
                TimestampUtc = DateTime.UtcNow
            };

            //
            // Resolve the host address
            //
            try
            {
                IPAddress[] addresses = await Dns.GetHostAddressesAsync(host);

                if (addresses.Length > 0)
                {
                    report.ResolvedAddress = addresses[0].ToString();
                }
            }
            catch
            {
                report.ResolvedAddress = host;
            }

            //
            // Scan ports with bounded concurrency
            //
            ConcurrentBag<PortScanResult> results = new ConcurrentBag<PortScanResult>();
            SemaphoreSlim semaphore = new SemaphoreSlim(_config.PortScanMaxConcurrency);

            List<Task> scanTasks = new List<Task>();

            foreach (int port in ports)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await semaphore.WaitAsync(cancellationToken);

                Task task = Task.Run(async () =>
                {
                    try
                    {
                        PortScanResult result = await ScanSinglePortAsync(host, port, timeoutMs, cancellationToken);
                        results.Add(result);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);

                scanTasks.Add(task);
            }

            await Task.WhenAll(scanTasks);

            //
            // Sort results by port number
            //
            report.Results = results.OrderBy(r => r.Port).ToList();
            report.OpenPortCount = report.Results.Count(r => r.IsOpen == true);
            report.ClosedPortCount = report.Results.Count(r => r.IsOpen == false);

            sw.Stop();
            report.DurationMs = sw.ElapsedMilliseconds;

            return report;
        }


        /// <summary>
        /// Scans a range of ports on the given host.
        /// </summary>
        public async Task<PortScanReport> ScanRangeAsync(string host, int startPort, int endPort, CancellationToken cancellationToken = default)
        {
            List<int> ports = new List<int>();

            for (int port = startPort; port <= endPort; port++)
            {
                ports.Add(port);
            }

            return await ScanPortsAsync(host, ports, cancellationToken);
        }


        /// <summary>
        /// Scans common well-known ports on the given host.
        /// </summary>
        public async Task<PortScanReport> ScanCommonPortsAsync(string host, CancellationToken cancellationToken = default)
        {
            return await ScanPortsAsync(host, WellKnownPorts.Keys.ToList(), cancellationToken);
        }


        // ── Internal ──────────────────────────────────────────────────────


        /// <summary>
        /// Scans a single TCP port.
        /// </summary>
        private async Task<PortScanResult> ScanSinglePortAsync(string host, int port, int timeoutMs, CancellationToken cancellationToken)
        {
            PortScanResult result = new PortScanResult
            {
                Host = host,
                Port = port
            };

            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    //
                    // Attempt TCP connection with timeout
                    //
                    Task connectTask = client.ConnectAsync(host, port);
                    Task completedTask = await Task.WhenAny(connectTask, Task.Delay(timeoutMs, cancellationToken));

                    if (completedTask == connectTask && connectTask.IsCompletedSuccessfully)
                    {
                        sw.Stop();

                        result.IsOpen = true;
                        result.ConnectionTimeMs = sw.ElapsedMilliseconds;

                        //
                        // Try to identify the service
                        //
                        if (WellKnownPorts.TryGetValue(port, out string serviceName))
                        {
                            result.ServiceName = serviceName;
                        }

                        //
                        // Try to read a banner (with a short timeout)
                        //
                        try
                        {
                            NetworkStream stream = client.GetStream();
                            stream.ReadTimeout = 500;

                            if (stream.DataAvailable == true)
                            {
                                byte[] bannerBuffer = new byte[256];
                                int bytesRead = await stream.ReadAsync(bannerBuffer, 0, bannerBuffer.Length, cancellationToken);

                                if (bytesRead > 0)
                                {
                                    result.Banner = Encoding.ASCII.GetString(bannerBuffer, 0, bytesRead).Trim();
                                }
                            }
                        }
                        catch
                        {
                            // Banner read failed — not critical
                        }
                    }
                    else
                    {
                        //
                        // Connection timed out or was cancelled
                        //
                        result.IsOpen = false;
                    }
                }
            }
            catch
            {
                //
                // Connection refused or other error — port is closed/filtered
                //
                result.IsOpen = false;
            }

            return result;
        }
    }
}
