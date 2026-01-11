using Foundation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Foundation.HubConfig
{
    public class Hub<T> : Microsoft.AspNetCore.SignalR.Hub<T> where T : class
    {
        private static Logger _hubLogger = null;

        public static void SetHubLogger(Logger logger)
        {
            _hubLogger = logger;
        }

        public static Logger GetHubLogger()
        {
            return _hubLogger;
        }


        protected Logger _logger;

        public Hub()
        {
            _logger = GetHubLogger();
        }


        public static class SignalRClientHandler
        {
            public static ConcurrentDictionary<string, HashSet<string>> ConnectedIds = new ConcurrentDictionary<string, HashSet<string>>();
        }

        public override Task OnConnectedAsync()
        {
            string typeName = this.GetType().Name;

            HashSet<string> connections;

            if (SignalRClientHandler.ConnectedIds.TryGetValue(typeName, out connections) == true)
            {
                connections.Add(Context.ConnectionId);
            }
            else
            {
                connections = new HashSet<string>();

                connections.Add(Context.ConnectionId);

                SignalRClientHandler.ConnectedIds.TryAdd(typeName, connections);
            }


            //
            // Log when we have a first connection
            //
            if (connections.Count == 1)
            {
                string message = "SignalR Hub received first connection for " + typeName;
                _logger?.LogDebug(message);

                if (System.Diagnostics.Debugger.IsAttached == true)
                {
                    System.Diagnostics.Debug.WriteLine(message);
                }
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            string typeName = this.GetType().Name;

            HashSet<string> connections;

            if (SignalRClientHandler.ConnectedIds.TryGetValue(typeName, out connections) == true)
            {
                connections.Remove(Context.ConnectionId);
            }


            //
            // Log when we have no more connections
            //
            if (connections.Count == 0)
            {
                string message = "SignalR Hub has no more connections for " + typeName;

                _logger?.LogDebug(message);

                if (System.Diagnostics.Debugger.IsAttached == true)
                {
                    System.Diagnostics.Debug.WriteLine(message);
                }
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
