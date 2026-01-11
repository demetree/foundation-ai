using System;
using System.Threading.Tasks;

namespace Foundation.HubConfig
{
    public enum AlertSeverity
    {
        _default = 0,           // Map this to LogLevel.Debug, and trace
        info = 1,               // Map this to LogLevel.Info  
        success = 2,            // Map this to LogLevel.System
        error = 3,              // Map this to LogLevel.Error
        warn = 4,               // Map this to LogLevel.Warm
        wait = 5                // Map this to LogLevel.Exception with stickiness
    }


    public class AlertDetails
    {
        public string title { get; set; }
        public string message { get; set; }
        public string details { get; set; }
        public AlertSeverity severity { get; set; }
        public DateTime date { get; set; }
        public bool sticky { get; set; }

        public AlertDetails(string message)
        {
            this.message = message;
            this.details = null;
            this.title = "Information";
            this.severity = AlertSeverity.info;
            this.date = DateTime.UtcNow;
            this.sticky = true;
        }
    }

    public interface IAlertHub
    {
        /// <summary>
        /// 
        /// Sends an Alert message.
        /// 
        /// </summary>
        /// <param name="message">The message.</param>
        Task SendAlert(AlertDetails message);
    }


    public class AlertHub : Foundation.HubConfig.Hub<IAlertHub>
    {

    }
}
