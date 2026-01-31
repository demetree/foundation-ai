/*

   GENERATED SERVICE FOR THE TELEMETRY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Telemetry table.

   It should suffice for many workflows and data access needs, but if anything more is needed, then extend this in a 
   custom version or add an additional targeted helper service.

*/
import {Injectable} from '@angular/core';
import {TelemetryApplicationService} from  './telemetry-application.service';
import {TelemetryApplicationMetricService} from  './telemetry-application-metric.service';
import {TelemetryCollectionRunService} from  './telemetry-collection-run.service';
import {TelemetryDatabaseHealthService} from  './telemetry-database-health.service';
import {TelemetryDiskHealthService} from  './telemetry-disk-health.service';
import {TelemetryErrorEventService} from  './telemetry-error-event.service';
import {TelemetryLogErrorService} from  './telemetry-log-error.service';
import {TelemetryNetworkHealthService} from  './telemetry-network-health.service';
import {TelemetrySessionSnapshotService} from  './telemetry-session-snapshot.service';
import {TelemetrySnapshotService} from  './telemetry-snapshot.service';


@Injectable({
  providedIn: 'root'
})
export class TelemetryDataServiceManagerService  {

    constructor(public telemetryApplicationService: TelemetryApplicationService
              , public telemetryApplicationMetricService: TelemetryApplicationMetricService
              , public telemetryCollectionRunService: TelemetryCollectionRunService
              , public telemetryDatabaseHealthService: TelemetryDatabaseHealthService
              , public telemetryDiskHealthService: TelemetryDiskHealthService
              , public telemetryErrorEventService: TelemetryErrorEventService
              , public telemetryLogErrorService: TelemetryLogErrorService
              , public telemetryNetworkHealthService: TelemetryNetworkHealthService
              , public telemetrySessionSnapshotService: TelemetrySessionSnapshotService
              , public telemetrySnapshotService: TelemetrySnapshotService
) { }  


    public ClearAllCaches() {

        this.telemetryApplicationService.ClearAllCaches();
        this.telemetryApplicationMetricService.ClearAllCaches();
        this.telemetryCollectionRunService.ClearAllCaches();
        this.telemetryDatabaseHealthService.ClearAllCaches();
        this.telemetryDiskHealthService.ClearAllCaches();
        this.telemetryErrorEventService.ClearAllCaches();
        this.telemetryLogErrorService.ClearAllCaches();
        this.telemetryNetworkHealthService.ClearAllCaches();
        this.telemetrySessionSnapshotService.ClearAllCaches();
        this.telemetrySnapshotService.ClearAllCaches();
    }
}