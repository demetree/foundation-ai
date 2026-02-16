//
// Audit Buffer Service Registration
//
// Extension method for IServiceCollection to register the audit event
// write-ahead buffer services and wire the BufferEventDelegate on AuditEngine.
//
// AI-assisted development - February 2026
//
using System;
using Foundation.Auditor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Web.Services
{
    public static class AuditBufferServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the durable audit event buffer services.
        /// When <c>AuditorMode</c> is set to <c>DurableLocalBuffer</c>,
        /// audit events are persisted to a local SQLite store and flushed
        /// to SQL Server by a background worker.
        /// </summary>
        public static IServiceCollection AddAuditBuffer(this IServiceCollection services)
        {
            // Register the buffer as a singleton so all callers share the same SQLite connection
            services.AddSingleton<IAuditEventBuffer, AuditEventBuffer>();

            // Register the background flush worker
            services.AddHostedService<AuditBufferFlushWorker>();

            // Wire the AuditEngine delegate via an IStartupFilter so it happens
            // after the service provider is built but before the first request
            services.AddTransient<IStartupFilter, AuditBufferStartupWiring>();

            return services;
        }
    }


    /// <summary>
    /// Startup filter that wires the AuditEngine.BufferEventDelegate
    /// once the service provider is available.
    /// </summary>
    internal class AuditBufferStartupWiring : IStartupFilter
    {
        private readonly IAuditEventBuffer _buffer;

        public AuditBufferStartupWiring(IAuditEventBuffer buffer)
        {
            _buffer = buffer;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            // Wire the delegate so AuditEngine can buffer events
            AuditEngine.BufferEventDelegate = async (e) =>
            {
                await _buffer.BufferEventAsync(e);
            };

            return next;
        }
    }
}
