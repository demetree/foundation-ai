/*

   GENERATED SERVICE FOR THE INCIDENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Incident table.

   It should suffice for many workflows and data access needs, but if anything more is needed, then extend this in a 
   custom version or add an additional targeted helper service.

*/
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, BehaviorSubject, catchError, throwError, lastValueFrom, map } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';
import { ServiceData } from './service.service';
import { SeverityTypeData } from './severity-type.service';
import { IncidentStatusTypeData } from './incident-status-type.service';
import { EscalationRuleData } from './escalation-rule.service';
import { IncidentChangeHistoryService, IncidentChangeHistoryData } from './incident-change-history.service';
import { IncidentTimelineEventService, IncidentTimelineEventData } from './incident-timeline-event.service';
import { IncidentNoteService, IncidentNoteData } from './incident-note.service';
import { IncidentNotificationService, IncidentNotificationData } from './incident-notification.service';
import { WebhookDeliveryAttemptService, WebhookDeliveryAttemptData } from './webhook-delivery-attempt.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class IncidentQueryParameters {
    incidentKey: string | null | undefined = null;
    serviceId: bigint | number | null | undefined = null;
    title: string | null | undefined = null;
    description: string | null | undefined = null;
    severityTypeId: bigint | number | null | undefined = null;
    incidentStatusTypeId: bigint | number | null | undefined = null;
    createdAt: string | null | undefined = null;        // ISO 8601
    escalationRuleId: bigint | number | null | undefined = null;
    currentRepeatCount: bigint | number | null | undefined = null;
    nextEscalationAt: string | null | undefined = null;        // ISO 8601
    acknowledgedAt: string | null | undefined = null;        // ISO 8601
    resolvedAt: string | null | undefined = null;        // ISO 8601
    currentAssigneeObjectGuid: string | null | undefined = null;
    sourcePayloadJson: string | null | undefined = null;
    versionNumber: bigint | number | null | undefined = null;
    objectGuid: string | null | undefined = null;
    active: boolean | null | undefined = null;
    deleted: boolean | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class IncidentSubmitData {
    id!: bigint | number;
    incidentKey!: string;
    serviceId!: bigint | number;
    title!: string;
    description: string | null = null;
    severityTypeId!: bigint | number;
    incidentStatusTypeId!: bigint | number;
    createdAt!: string;      // ISO 8601
    escalationRuleId: bigint | number | null = null;
    currentRepeatCount: bigint | number | null = null;
    nextEscalationAt: string | null = null;     // ISO 8601
    acknowledgedAt: string | null = null;     // ISO 8601
    resolvedAt: string | null = null;     // ISO 8601
    currentAssigneeObjectGuid: string | null = null;
    sourcePayloadJson: string | null = null;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}



//
// Version history information returned from version history API endpoints.
// Matches server-side VersionInformation<T> structure.
//
export interface VersionInformationUser {
    id: bigint | number;
    userName: string;
    firstName: string | null;
    middleName: string | null;
    lastName: string | null;
}

export interface VersionInformation<T> {
    timeStamp: string;           // ISO 8601
    user: VersionInformationUser;
    versionNumber: number;
    data: T | null;
}

export class IncidentBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. IncidentChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `incident.IncidentChildren$` — use with `| async` in templates
//        • Promise:    `incident.IncidentChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="incident.IncidentChildren$ | async"`), or
//        • Access the promise getter (`incident.IncidentChildren` or `await incident.IncidentChildren`)
//    - Simply reading `incident.IncidentChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await incident.Reload()` to refresh the entire object and clear all lazy caches.
//    - Useful after mutations or when navigating into a navigation property.
//
// 5. **Cache clearing**:
//    - Use `ClearXCache()` methods after mutations to force fresh data on next access.
//
// 6. **Nav Properties**: if loaded with 'includeRelations = true' will be data objects of their appropriate types in data only.  They
//     will need to be 'Revived' and 'Reloaded' to access their nav properties, or lazy load their children.
//
// 7. **Dates are typed as strings**: because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z");
//
export class IncidentData {
    id!: bigint | number;
    incidentKey!: string;
    serviceId!: bigint | number;
    title!: string;
    description!: string | null;
    severityTypeId!: bigint | number;
    incidentStatusTypeId!: bigint | number;
    createdAt!: string;      // ISO 8601
    escalationRuleId!: bigint | number;
    currentRepeatCount!: bigint | number;
    nextEscalationAt!: string | null;   // ISO 8601
    acknowledgedAt!: string | null;   // ISO 8601
    resolvedAt!: string | null;   // ISO 8601
    currentAssigneeObjectGuid!: string | null;
    sourcePayloadJson!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    escalationRule: EscalationRuleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    incidentStatusType: IncidentStatusTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    service: ServiceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    severityType: SeverityTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _incidentChangeHistories: IncidentChangeHistoryData[] | null = null;
    private _incidentChangeHistoriesPromise: Promise<IncidentChangeHistoryData[]> | null  = null;
    private _incidentChangeHistoriesSubject = new BehaviorSubject<IncidentChangeHistoryData[] | null>(null);

                
    private _incidentTimelineEvents: IncidentTimelineEventData[] | null = null;
    private _incidentTimelineEventsPromise: Promise<IncidentTimelineEventData[]> | null  = null;
    private _incidentTimelineEventsSubject = new BehaviorSubject<IncidentTimelineEventData[] | null>(null);

                
    private _incidentNotes: IncidentNoteData[] | null = null;
    private _incidentNotesPromise: Promise<IncidentNoteData[]> | null  = null;
    private _incidentNotesSubject = new BehaviorSubject<IncidentNoteData[] | null>(null);

                
    private _incidentNotifications: IncidentNotificationData[] | null = null;
    private _incidentNotificationsPromise: Promise<IncidentNotificationData[]> | null  = null;
    private _incidentNotificationsSubject = new BehaviorSubject<IncidentNotificationData[] | null>(null);

                
    private _webhookDeliveryAttempts: WebhookDeliveryAttemptData[] | null = null;
    private _webhookDeliveryAttemptsPromise: Promise<WebhookDeliveryAttemptData[]> | null  = null;
    private _webhookDeliveryAttemptsSubject = new BehaviorSubject<WebhookDeliveryAttemptData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<IncidentData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<IncidentData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<IncidentData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public IncidentChangeHistories$ = this._incidentChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._incidentChangeHistories === null && this._incidentChangeHistoriesPromise === null) {
            this.loadIncidentChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public IncidentChangeHistoriesCount$ = IncidentChangeHistoryService.Instance.GetIncidentChangeHistoriesRowCount({incidentId: this.id,
      active: true,
      deleted: false
    });



    public IncidentTimelineEvents$ = this._incidentTimelineEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._incidentTimelineEvents === null && this._incidentTimelineEventsPromise === null) {
            this.loadIncidentTimelineEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public IncidentTimelineEventsCount$ = IncidentTimelineEventService.Instance.GetIncidentTimelineEventsRowCount({incidentId: this.id,
      active: true,
      deleted: false
    });



    public IncidentNotes$ = this._incidentNotesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._incidentNotes === null && this._incidentNotesPromise === null) {
            this.loadIncidentNotes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public IncidentNotesCount$ = IncidentNoteService.Instance.GetIncidentNotesRowCount({incidentId: this.id,
      active: true,
      deleted: false
    });



    public IncidentNotifications$ = this._incidentNotificationsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._incidentNotifications === null && this._incidentNotificationsPromise === null) {
            this.loadIncidentNotifications(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public IncidentNotificationsCount$ = IncidentNotificationService.Instance.GetIncidentNotificationsRowCount({incidentId: this.id,
      active: true,
      deleted: false
    });



    public WebhookDeliveryAttempts$ = this._webhookDeliveryAttemptsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._webhookDeliveryAttempts === null && this._webhookDeliveryAttemptsPromise === null) {
            this.loadWebhookDeliveryAttempts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public WebhookDeliveryAttemptsCount$ = WebhookDeliveryAttemptService.Instance.GetWebhookDeliveryAttemptsRowCount({incidentId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any IncidentData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.incident.Reload();
  //
  //  Non Async:
  //
  //     incident[0].Reload().then(x => {
  //        this.incident = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      IncidentService.Instance.GetIncident(this.id, includeRelations)
    );

    // Merge fresh data into this instance (preserves reference)
    this.UpdateFrom(fresh as this);

    // Clear all lazy caches to force re-load on next access
    this.clearAllLazyCaches();

    return this;
  }


  private clearAllLazyCaches(): void {
     //
     // Reset every collection cache and notify subscribers
     //
     this._incidentChangeHistories = null;
     this._incidentChangeHistoriesPromise = null;
     this._incidentChangeHistoriesSubject.next(null);

     this._incidentTimelineEvents = null;
     this._incidentTimelineEventsPromise = null;
     this._incidentTimelineEventsSubject.next(null);

     this._incidentNotes = null;
     this._incidentNotesPromise = null;
     this._incidentNotesSubject.next(null);

     this._incidentNotifications = null;
     this._incidentNotificationsPromise = null;
     this._incidentNotificationsSubject.next(null);

     this._webhookDeliveryAttempts = null;
     this._webhookDeliveryAttemptsPromise = null;
     this._webhookDeliveryAttemptsSubject.next(null);

     this._currentVersionInfo = null;
     this._currentVersionInfoPromise = null;
     this._currentVersionInfoSubject.next(null);
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the IncidentChangeHistories for this Incident.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.incident.IncidentChangeHistories.then(incidents => { ... })
     *   or
     *   await this.incident.incidents
     *
    */
    public get IncidentChangeHistories(): Promise<IncidentChangeHistoryData[]> {
        if (this._incidentChangeHistories !== null) {
            return Promise.resolve(this._incidentChangeHistories);
        }

        if (this._incidentChangeHistoriesPromise !== null) {
            return this._incidentChangeHistoriesPromise;
        }

        // Start the load
        this.loadIncidentChangeHistories();

        return this._incidentChangeHistoriesPromise!;
    }



    private loadIncidentChangeHistories(): void {

        this._incidentChangeHistoriesPromise = lastValueFrom(
            IncidentService.Instance.GetIncidentChangeHistoriesForIncident(this.id)
        )
        .then(IncidentChangeHistories => {
            this._incidentChangeHistories = IncidentChangeHistories ?? [];
            this._incidentChangeHistoriesSubject.next(this._incidentChangeHistories);
            return this._incidentChangeHistories;
         })
        .catch(err => {
            this._incidentChangeHistories = [];
            this._incidentChangeHistoriesSubject.next(this._incidentChangeHistories);
            throw err;
        })
        .finally(() => {
            this._incidentChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached IncidentChangeHistory. Call after mutations to force refresh.
     */
    public ClearIncidentChangeHistoriesCache(): void {
        this._incidentChangeHistories = null;
        this._incidentChangeHistoriesPromise = null;
        this._incidentChangeHistoriesSubject.next(this._incidentChangeHistories);      // Emit to observable
    }

    public get HasIncidentChangeHistories(): Promise<boolean> {
        return this.IncidentChangeHistories.then(incidentChangeHistories => incidentChangeHistories.length > 0);
    }


    /**
     *
     * Gets the IncidentTimelineEvents for this Incident.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.incident.IncidentTimelineEvents.then(incidents => { ... })
     *   or
     *   await this.incident.incidents
     *
    */
    public get IncidentTimelineEvents(): Promise<IncidentTimelineEventData[]> {
        if (this._incidentTimelineEvents !== null) {
            return Promise.resolve(this._incidentTimelineEvents);
        }

        if (this._incidentTimelineEventsPromise !== null) {
            return this._incidentTimelineEventsPromise;
        }

        // Start the load
        this.loadIncidentTimelineEvents();

        return this._incidentTimelineEventsPromise!;
    }



    private loadIncidentTimelineEvents(): void {

        this._incidentTimelineEventsPromise = lastValueFrom(
            IncidentService.Instance.GetIncidentTimelineEventsForIncident(this.id)
        )
        .then(IncidentTimelineEvents => {
            this._incidentTimelineEvents = IncidentTimelineEvents ?? [];
            this._incidentTimelineEventsSubject.next(this._incidentTimelineEvents);
            return this._incidentTimelineEvents;
         })
        .catch(err => {
            this._incidentTimelineEvents = [];
            this._incidentTimelineEventsSubject.next(this._incidentTimelineEvents);
            throw err;
        })
        .finally(() => {
            this._incidentTimelineEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached IncidentTimelineEvent. Call after mutations to force refresh.
     */
    public ClearIncidentTimelineEventsCache(): void {
        this._incidentTimelineEvents = null;
        this._incidentTimelineEventsPromise = null;
        this._incidentTimelineEventsSubject.next(this._incidentTimelineEvents);      // Emit to observable
    }

    public get HasIncidentTimelineEvents(): Promise<boolean> {
        return this.IncidentTimelineEvents.then(incidentTimelineEvents => incidentTimelineEvents.length > 0);
    }


    /**
     *
     * Gets the IncidentNotes for this Incident.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.incident.IncidentNotes.then(incidents => { ... })
     *   or
     *   await this.incident.incidents
     *
    */
    public get IncidentNotes(): Promise<IncidentNoteData[]> {
        if (this._incidentNotes !== null) {
            return Promise.resolve(this._incidentNotes);
        }

        if (this._incidentNotesPromise !== null) {
            return this._incidentNotesPromise;
        }

        // Start the load
        this.loadIncidentNotes();

        return this._incidentNotesPromise!;
    }



    private loadIncidentNotes(): void {

        this._incidentNotesPromise = lastValueFrom(
            IncidentService.Instance.GetIncidentNotesForIncident(this.id)
        )
        .then(IncidentNotes => {
            this._incidentNotes = IncidentNotes ?? [];
            this._incidentNotesSubject.next(this._incidentNotes);
            return this._incidentNotes;
         })
        .catch(err => {
            this._incidentNotes = [];
            this._incidentNotesSubject.next(this._incidentNotes);
            throw err;
        })
        .finally(() => {
            this._incidentNotesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached IncidentNote. Call after mutations to force refresh.
     */
    public ClearIncidentNotesCache(): void {
        this._incidentNotes = null;
        this._incidentNotesPromise = null;
        this._incidentNotesSubject.next(this._incidentNotes);      // Emit to observable
    }

    public get HasIncidentNotes(): Promise<boolean> {
        return this.IncidentNotes.then(incidentNotes => incidentNotes.length > 0);
    }


    /**
     *
     * Gets the IncidentNotifications for this Incident.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.incident.IncidentNotifications.then(incidents => { ... })
     *   or
     *   await this.incident.incidents
     *
    */
    public get IncidentNotifications(): Promise<IncidentNotificationData[]> {
        if (this._incidentNotifications !== null) {
            return Promise.resolve(this._incidentNotifications);
        }

        if (this._incidentNotificationsPromise !== null) {
            return this._incidentNotificationsPromise;
        }

        // Start the load
        this.loadIncidentNotifications();

        return this._incidentNotificationsPromise!;
    }



    private loadIncidentNotifications(): void {

        this._incidentNotificationsPromise = lastValueFrom(
            IncidentService.Instance.GetIncidentNotificationsForIncident(this.id)
        )
        .then(IncidentNotifications => {
            this._incidentNotifications = IncidentNotifications ?? [];
            this._incidentNotificationsSubject.next(this._incidentNotifications);
            return this._incidentNotifications;
         })
        .catch(err => {
            this._incidentNotifications = [];
            this._incidentNotificationsSubject.next(this._incidentNotifications);
            throw err;
        })
        .finally(() => {
            this._incidentNotificationsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached IncidentNotification. Call after mutations to force refresh.
     */
    public ClearIncidentNotificationsCache(): void {
        this._incidentNotifications = null;
        this._incidentNotificationsPromise = null;
        this._incidentNotificationsSubject.next(this._incidentNotifications);      // Emit to observable
    }

    public get HasIncidentNotifications(): Promise<boolean> {
        return this.IncidentNotifications.then(incidentNotifications => incidentNotifications.length > 0);
    }


    /**
     *
     * Gets the WebhookDeliveryAttempts for this Incident.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.incident.WebhookDeliveryAttempts.then(incidents => { ... })
     *   or
     *   await this.incident.incidents
     *
    */
    public get WebhookDeliveryAttempts(): Promise<WebhookDeliveryAttemptData[]> {
        if (this._webhookDeliveryAttempts !== null) {
            return Promise.resolve(this._webhookDeliveryAttempts);
        }

        if (this._webhookDeliveryAttemptsPromise !== null) {
            return this._webhookDeliveryAttemptsPromise;
        }

        // Start the load
        this.loadWebhookDeliveryAttempts();

        return this._webhookDeliveryAttemptsPromise!;
    }



    private loadWebhookDeliveryAttempts(): void {

        this._webhookDeliveryAttemptsPromise = lastValueFrom(
            IncidentService.Instance.GetWebhookDeliveryAttemptsForIncident(this.id)
        )
        .then(WebhookDeliveryAttempts => {
            this._webhookDeliveryAttempts = WebhookDeliveryAttempts ?? [];
            this._webhookDeliveryAttemptsSubject.next(this._webhookDeliveryAttempts);
            return this._webhookDeliveryAttempts;
         })
        .catch(err => {
            this._webhookDeliveryAttempts = [];
            this._webhookDeliveryAttemptsSubject.next(this._webhookDeliveryAttempts);
            throw err;
        })
        .finally(() => {
            this._webhookDeliveryAttemptsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached WebhookDeliveryAttempt. Call after mutations to force refresh.
     */
    public ClearWebhookDeliveryAttemptsCache(): void {
        this._webhookDeliveryAttempts = null;
        this._webhookDeliveryAttemptsPromise = null;
        this._webhookDeliveryAttemptsSubject.next(this._webhookDeliveryAttempts);      // Emit to observable
    }

    public get HasWebhookDeliveryAttempts(): Promise<boolean> {
        return this.WebhookDeliveryAttempts.then(webhookDeliveryAttempts => webhookDeliveryAttempts.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (incident.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await incident.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<IncidentData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<IncidentData>> {
        const info = await lastValueFrom(
            IncidentService.Instance.GetIncidentChangeMetadata(this.id, this.versionNumber as number)
        );
        this._currentVersionInfo = info;
        this._currentVersionInfoSubject.next(info);
        return info;
    }


    public ClearCurrentVersionInfoCache(): void {
        this._currentVersionInfo = null;
        this._currentVersionInfoPromise = null;
        this._currentVersionInfoSubject.next(null);
    }



    /**
     * Updates the state of this IncidentData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this IncidentData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): IncidentSubmitData {
        return IncidentService.Instance.ConvertToIncidentSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class IncidentService extends SecureEndpointBase {

    private static _instance: IncidentService;
    private listCache: Map<string, Observable<Array<IncidentData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<IncidentBasicListData>>>;
    private recordCache: Map<string, Observable<IncidentData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private incidentChangeHistoryService: IncidentChangeHistoryService,
        private incidentTimelineEventService: IncidentTimelineEventService,
        private incidentNoteService: IncidentNoteService,
        private incidentNotificationService: IncidentNotificationService,
        private webhookDeliveryAttemptService: WebhookDeliveryAttemptService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<IncidentData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<IncidentBasicListData>>>();
        this.recordCache = new Map<string, Observable<IncidentData>>();

        IncidentService._instance = this;
    }

    public static get Instance(): IncidentService {
      return IncidentService._instance;
    }


    public ClearListCaches(config: IncidentQueryParameters | null = null) {

        const configHash = this.getConfigHash(config);

        if (this.listCache.has(configHash)) {
          this.listCache.delete(configHash);
        }

        if (this.rowCountCache.has(configHash)) {
            this.rowCountCache.delete(configHash);
        }

        if (this.basicListDataCache.has(configHash)) {
            this.basicListDataCache.delete(configHash);
        }
    }


    public ClearRecordCache(id: bigint | number, includeRelations: boolean = true) {

        const configHash = this.utilityService.hashCode(`_${id}_${includeRelations}`);

        if (this.recordCache.has(configHash)) {
            this.recordCache.delete(configHash);
        }
    }


    public ClearAllCaches() {
        this.listCache.clear();
        this.rowCountCache.clear();
        this.basicListDataCache.clear();
        this.recordCache.clear();
    }


    public ConvertToIncidentSubmitData(data: IncidentData): IncidentSubmitData {

        let output = new IncidentSubmitData();

        output.id = data.id;
        output.incidentKey = data.incidentKey;
        output.serviceId = data.serviceId;
        output.title = data.title;
        output.description = data.description;
        output.severityTypeId = data.severityTypeId;
        output.incidentStatusTypeId = data.incidentStatusTypeId;
        output.createdAt = data.createdAt;
        output.escalationRuleId = data.escalationRuleId;
        output.currentRepeatCount = data.currentRepeatCount;
        output.nextEscalationAt = data.nextEscalationAt;
        output.acknowledgedAt = data.acknowledgedAt;
        output.resolvedAt = data.resolvedAt;
        output.currentAssigneeObjectGuid = data.currentAssigneeObjectGuid;
        output.sourcePayloadJson = data.sourcePayloadJson;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetIncident(id: bigint | number, includeRelations: boolean = true) : Observable<IncidentData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const incident$ = this.requestIncident(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Incident", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, incident$);

            return incident$;
        }

        return this.recordCache.get(configHash) as Observable<IncidentData>;
    }

    private requestIncident(id: bigint | number, includeRelations: boolean = true) : Observable<IncidentData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentData>(this.baseUrl + 'api/Incident/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveIncident(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestIncident(id, includeRelations));
            }));
    }

    public GetIncidentList(config: IncidentQueryParameters | any = null) : Observable<Array<IncidentData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const incidentList$ = this.requestIncidentList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Incident list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, incidentList$);

            return incidentList$;
        }

        return this.listCache.get(configHash) as Observable<Array<IncidentData>>;
    }


    private requestIncidentList(config: IncidentQueryParameters | any) : Observable <Array<IncidentData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IncidentData>>(this.baseUrl + 'api/Incidents', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveIncidentList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentList(config));
            }));
    }

    public GetIncidentsRowCount(config: IncidentQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const incidentsRowCount$ = this.requestIncidentsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Incidents row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, incidentsRowCount$);

            return incidentsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestIncidentsRowCount(config: IncidentQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Incidents/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentsRowCount(config));
            }));
    }

    public GetIncidentsBasicListData(config: IncidentQueryParameters | any = null) : Observable<Array<IncidentBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const incidentsBasicListData$ = this.requestIncidentsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Incidents basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, incidentsBasicListData$);

            return incidentsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<IncidentBasicListData>>;
    }


    private requestIncidentsBasicListData(config: IncidentQueryParameters | any) : Observable<Array<IncidentBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IncidentBasicListData>>(this.baseUrl + 'api/Incidents/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentsBasicListData(config));
            }));

    }


    public PutIncident(id: bigint | number, incident: IncidentSubmitData) : Observable<IncidentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IncidentData>(this.baseUrl + 'api/Incident/' + id.toString(), incident, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncident(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutIncident(id, incident));
            }));
    }


    public PostIncident(incident: IncidentSubmitData) : Observable<IncidentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<IncidentData>(this.baseUrl + 'api/Incident', incident, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncident(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostIncident(incident));
            }));
    }

  
    public DeleteIncident(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Incident/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteIncident(id));
            }));
    }

    public RollbackIncident(id: bigint | number, versionNumber: bigint | number) : Observable<IncidentData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IncidentData>(this.baseUrl + 'api/Incident/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncident(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackIncident(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Incident.
     */
    public GetIncidentChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<IncidentData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<IncidentData>>(this.baseUrl + 'api/Incident/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetIncidentChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Incident.
     */
    public GetIncidentAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<IncidentData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<IncidentData>[]>(this.baseUrl + 'api/Incident/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetIncidentAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Incident.
     */
    public GetIncidentVersion(id: bigint | number, version: number): Observable<IncidentData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentData>(this.baseUrl + 'api/Incident/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveIncident(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetIncidentVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Incident at a specific point in time.
     */
    public GetIncidentStateAtTime(id: bigint | number, time: string): Observable<IncidentData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentData>(this.baseUrl + 'api/Incident/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveIncident(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetIncidentStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: IncidentQueryParameters | any): string {

        if (!config) {
            return '_';
        }

        // Normalize the config object, excluding null and undefined properties
        const normalizedConfig = Object.keys(config)
            .sort() // Ensure consistent property order
            .reduce((obj: any, key: string) => {
                if (config[key] != null) { // Exclude null and undefined
                    obj[key] = config[key];
                }
                return obj;
            }, {});

        if (Object.keys(normalizedConfig).length > 0) {
            return this.utilityService.hashCode(JSON.stringify(normalizedConfig));
        }

        return '_';
    }

    public userIsAlertingIncidentReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingIncidentReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.Incidents
        //
        if (userIsAlertingIncidentReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingIncidentReader = user.readPermission >= 0;
            } else {
                userIsAlertingIncidentReader = false;
            }
        }

        return userIsAlertingIncidentReader;
    }


    public userIsAlertingIncidentWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingIncidentWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.Incidents
        //
        if (userIsAlertingIncidentWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingIncidentWriter = user.writePermission >= 0;
          } else {
            userIsAlertingIncidentWriter = false;
          }      
        }

        return userIsAlertingIncidentWriter;
    }

    public GetIncidentChangeHistoriesForIncident(incidentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IncidentChangeHistoryData[]> {
        return this.incidentChangeHistoryService.GetIncidentChangeHistoryList({
            incidentId: incidentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetIncidentTimelineEventsForIncident(incidentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IncidentTimelineEventData[]> {
        return this.incidentTimelineEventService.GetIncidentTimelineEventList({
            incidentId: incidentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetIncidentNotesForIncident(incidentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IncidentNoteData[]> {
        return this.incidentNoteService.GetIncidentNoteList({
            incidentId: incidentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetIncidentNotificationsForIncident(incidentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IncidentNotificationData[]> {
        return this.incidentNotificationService.GetIncidentNotificationList({
            incidentId: incidentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetWebhookDeliveryAttemptsForIncident(incidentId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<WebhookDeliveryAttemptData[]> {
        return this.webhookDeliveryAttemptService.GetWebhookDeliveryAttemptList({
            incidentId: incidentId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full IncidentData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the IncidentData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when IncidentTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveIncident(raw: any): IncidentData {
    if (!raw) return raw;

    //
    // Create a IncidentData object instance with correct prototype
    //
    const revived = Object.create(IncidentData.prototype) as IncidentData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._incidentChangeHistories = null;
    (revived as any)._incidentChangeHistoriesPromise = null;
    (revived as any)._incidentChangeHistoriesSubject = new BehaviorSubject<IncidentChangeHistoryData[] | null>(null);

    (revived as any)._incidentTimelineEvents = null;
    (revived as any)._incidentTimelineEventsPromise = null;
    (revived as any)._incidentTimelineEventsSubject = new BehaviorSubject<IncidentTimelineEventData[] | null>(null);

    (revived as any)._incidentNotes = null;
    (revived as any)._incidentNotesPromise = null;
    (revived as any)._incidentNotesSubject = new BehaviorSubject<IncidentNoteData[] | null>(null);

    (revived as any)._incidentNotifications = null;
    (revived as any)._incidentNotificationsPromise = null;
    (revived as any)._incidentNotificationsSubject = new BehaviorSubject<IncidentNotificationData[] | null>(null);

    (revived as any)._webhookDeliveryAttempts = null;
    (revived as any)._webhookDeliveryAttemptsPromise = null;
    (revived as any)._webhookDeliveryAttemptsSubject = new BehaviorSubject<WebhookDeliveryAttemptData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadIncidentXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).IncidentChangeHistories$ = (revived as any)._incidentChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._incidentChangeHistories === null && (revived as any)._incidentChangeHistoriesPromise === null) {
                (revived as any).loadIncidentChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).IncidentChangeHistoriesCount$ = IncidentChangeHistoryService.Instance.GetIncidentChangeHistoriesRowCount({incidentId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).IncidentTimelineEvents$ = (revived as any)._incidentTimelineEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._incidentTimelineEvents === null && (revived as any)._incidentTimelineEventsPromise === null) {
                (revived as any).loadIncidentTimelineEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).IncidentTimelineEventsCount$ = IncidentTimelineEventService.Instance.GetIncidentTimelineEventsRowCount({incidentId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).IncidentNotes$ = (revived as any)._incidentNotesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._incidentNotes === null && (revived as any)._incidentNotesPromise === null) {
                (revived as any).loadIncidentNotes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).IncidentNotesCount$ = IncidentNoteService.Instance.GetIncidentNotesRowCount({incidentId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).IncidentNotifications$ = (revived as any)._incidentNotificationsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._incidentNotifications === null && (revived as any)._incidentNotificationsPromise === null) {
                (revived as any).loadIncidentNotifications();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).IncidentNotificationsCount$ = IncidentNotificationService.Instance.GetIncidentNotificationsRowCount({incidentId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).WebhookDeliveryAttempts$ = (revived as any)._webhookDeliveryAttemptsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._webhookDeliveryAttempts === null && (revived as any)._webhookDeliveryAttemptsPromise === null) {
                (revived as any).loadWebhookDeliveryAttempts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).WebhookDeliveryAttemptsCount$ = WebhookDeliveryAttemptService.Instance.GetWebhookDeliveryAttemptsRowCount({incidentId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<IncidentData> | null>(null);

    (revived as any).CurrentVersionInfo$ = (revived as any)._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if ((revived as any)._currentVersionInfo === null && (revived as any)._currentVersionInfoPromise === null) {
                (revived as any).loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    return revived;
  }

  private ReviveIncidentList(rawList: any[]): IncidentData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveIncident(raw));
  }

}
