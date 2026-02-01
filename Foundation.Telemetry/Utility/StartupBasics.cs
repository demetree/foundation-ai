using Foundation.Auditor.Controllers.WebAPI;
using Foundation.Security.Controllers.WebAPI;
using Foundation.Telemetry.Controllers.WebAPI;
using System;
using System.Collections.Generic;

namespace Foundation.Web.Utility
{
    public class TelemetryStartupBasics
    {
        public static void AddTelemetryWebAPIControllers(List<Type> controllers)
        {
            //
            // Custom Telemetry controller
            //
            controllers.Add(typeof(Foundation.Controllers.WebAPI.TelemetryController));  // Telemetry historical data

            //
            // Start of code generated controller list for Telemetry module
            //
            controllers.Add(typeof(TelemetryApplicationsController));
            controllers.Add(typeof(TelemetryApplicationMetricsController));
            controllers.Add(typeof(TelemetryCollectionRunsController));
            controllers.Add(typeof(TelemetryDatabaseHealthsController));
            controllers.Add(typeof(TelemetryDiskHealthsController));
            controllers.Add(typeof(TelemetryErrorEventsController));
            controllers.Add(typeof(TelemetryLogErrorsController));
            controllers.Add(typeof(TelemetryNetworkHealthsController));
            controllers.Add(typeof(TelemetrySessionSnapshotsController));
            controllers.Add(typeof(TelemetrySnapshotsController));
            //
            // End of code generated controller list for Telemetry module
            //
        }


    }
}
