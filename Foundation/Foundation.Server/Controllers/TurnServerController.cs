// ============================================================================
//
// TurnServerController.cs — Admin API for the TURN server.
//
// Provides endpoints for monitoring server status, viewing active allocations,
// and force-removing allocations.
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Foundation.Security;
using Foundation.Networking.Coturn.Server;

namespace Foundation.Server.Controllers
{
    [ApiController]
    [Route("api/turn")]
    [Authorize]
    public class TurnServerController : SecureWebAPIController
    {
        private readonly TurnServer _turnServer;
        private readonly ILogger<TurnServerController> _logger;


        public TurnServerController(TurnServer turnServer,
                                     ILogger<TurnServerController> logger) : base("Foundation", "TurnServer")
        {
            _turnServer = turnServer;
            _logger = logger;
        }


        // ── Status ────────────────────────────────────────────────────────


        /// <summary>
        /// Get the current TURN server status.
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(typeof(TurnServerStatusResponse), 200)]
        public IActionResult GetStatus()
        {
            try
            {
                TurnServerStatusResponse response = new TurnServerStatusResponse
                {
                    IsRunning = _turnServer.IsRunning,
                    UdpEndpoint = _turnServer.ListenEndPoint?.ToString(),
                    TcpEndpoint = _turnServer.TcpListener?.ListenEndPoint?.ToString(),
                    TlsEndpoint = _turnServer.TlsListener?.ListenEndPoint?.ToString(),
                    AllocationCount = _turnServer.Allocations.AllocationCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get TURN server status");
                return StatusCode(500, new { Error = "Failed to retrieve TURN server status." });
            }
        }


        // ── Allocations ───────────────────────────────────────────────────


        /// <summary>
        /// Get all active TURN allocations.
        /// </summary>
        [HttpGet("allocations")]
        [ProducesResponseType(typeof(TurnAllocationsResponse), 200)]
        public IActionResult GetAllocations()
        {
            try
            {
                List<TurnAllocationSummary> allocations = new List<TurnAllocationSummary>();

                foreach (var kvp in _turnServer.Allocations.GetAllAllocations())
                {
                    FiveTuple ft = kvp.Key;
                    TurnAllocation alloc = kvp.Value;

                    allocations.Add(new TurnAllocationSummary
                    {
                        FiveTuple = ft.ToString(),
                        Username = alloc.Username,
                        RelayPort = alloc.RelayPort,
                        LifetimeSeconds = alloc.LifetimeSeconds,
                        ExpiresAtUtc = alloc.ExpiresAtUtc,
                        PermissionCount = alloc.Permissions.Count,
                        ChannelCount = alloc.Channels.Count,
                        IsExpired = alloc.IsExpired()
                    });
                }

                TurnAllocationsResponse response = new TurnAllocationsResponse
                {
                    Allocations = allocations,
                    TotalCount = allocations.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get TURN allocations");
                return StatusCode(500, new { Error = "Failed to retrieve allocations." });
            }
        }


        // ── Response Models ───────────────────────────────────────────────


        public class TurnServerStatusResponse
        {
            public bool IsRunning { get; set; }
            public string UdpEndpoint { get; set; }
            public string TcpEndpoint { get; set; }
            public string TlsEndpoint { get; set; }
            public int AllocationCount { get; set; }
        }


        public class TurnAllocationsResponse
        {
            public List<TurnAllocationSummary> Allocations { get; set; }
            public int TotalCount { get; set; }
        }


        public class TurnAllocationSummary
        {
            public string FiveTuple { get; set; }
            public string Username { get; set; }
            public int RelayPort { get; set; }
            public int LifetimeSeconds { get; set; }
            public DateTime ExpiresAtUtc { get; set; }
            public int PermissionCount { get; set; }
            public int ChannelCount { get; set; }
            public bool IsExpired { get; set; }
        }
    }
}
