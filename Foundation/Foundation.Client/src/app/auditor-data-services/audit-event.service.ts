/*

   GENERATED SERVICE FOR THE AUDITEVENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AuditEvent table.

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
import { AuditUserData } from './audit-user.service';
import { AuditSessionData } from './audit-session.service';
import { AuditTypeData } from './audit-type.service';
import { AuditAccessTypeData } from './audit-access-type.service';
import { AuditSourceData } from './audit-source.service';
import { AuditUserAgentData } from './audit-user-agent.service';
import { AuditModuleData } from './audit-module.service';
import { AuditModuleEntityData } from './audit-module-entity.service';
import { AuditResourceData } from './audit-resource.service';
import { AuditHostSystemData } from './audit-host-system.service';
import { AuditEventEntityStateService, AuditEventEntityStateData } from './audit-event-entity-state.service';
import { AuditEventErrorMessageService, AuditEventErrorMessageData } from './audit-event-error-message.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AuditEventQueryParameters {
    startTime: string | null | undefined = null;        // ISO 8601 (full datetime)
    stopTime: string | null | undefined = null;        // ISO 8601 (full datetime)
    completedSuccessfully: boolean | null | undefined = null;
    auditUserId: bigint | number | null | undefined = null;
    auditSessionId: bigint | number | null | undefined = null;
    auditTypeId: bigint | number | null | undefined = null;
    auditAccessTypeId: bigint | number | null | undefined = null;
    auditSourceId: bigint | number | null | undefined = null;
    auditUserAgentId: bigint | number | null | undefined = null;
    auditModuleId: bigint | number | null | undefined = null;
    auditModuleEntityId: bigint | number | null | undefined = null;
    auditResourceId: bigint | number | null | undefined = null;
    auditHostSystemId: bigint | number | null | undefined = null;
    primaryKey: string | null | undefined = null;
    threadId: bigint | number | null | undefined = null;
    message: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class AuditEventSubmitData {
    id!: bigint | number;
    startTime!: string;      // ISO 8601 (full datetime)
    stopTime!: string;      // ISO 8601 (full datetime)
    completedSuccessfully!: boolean;
    auditUserId!: bigint | number;
    auditSessionId!: bigint | number;
    auditTypeId!: bigint | number;
    auditAccessTypeId!: bigint | number;
    auditSourceId!: bigint | number;
    auditUserAgentId!: bigint | number;
    auditModuleId!: bigint | number;
    auditModuleEntityId!: bigint | number;
    auditResourceId!: bigint | number;
    auditHostSystemId!: bigint | number;
    primaryKey: string | null = null;
    threadId: bigint | number | null = null;
    message!: string;
}


export class AuditEventBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AuditEventChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `auditEvent.AuditEventChildren$` — use with `| async` in templates
//        • Promise:    `auditEvent.AuditEventChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="auditEvent.AuditEventChildren$ | async"`), or
//        • Access the promise getter (`auditEvent.AuditEventChildren` or `await auditEvent.AuditEventChildren`)
//    - Simply reading `auditEvent.AuditEventChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await auditEvent.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AuditEventData {
    id!: bigint | number;
    startTime!: string;      // ISO 8601 (full datetime)
    stopTime!: string;      // ISO 8601 (full datetime)
    completedSuccessfully!: boolean;
    auditUserId!: bigint | number;
    auditSessionId!: bigint | number;
    auditTypeId!: bigint | number;
    auditAccessTypeId!: bigint | number;
    auditSourceId!: bigint | number;
    auditUserAgentId!: bigint | number;
    auditModuleId!: bigint | number;
    auditModuleEntityId!: bigint | number;
    auditResourceId!: bigint | number;
    auditHostSystemId!: bigint | number;
    primaryKey!: string | null;
    threadId!: bigint | number;
    message!: string;
    auditAccessType: AuditAccessTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    auditHostSystem: AuditHostSystemData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    auditModule: AuditModuleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    auditModuleEntity: AuditModuleEntityData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    auditResource: AuditResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    auditSession: AuditSessionData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    auditSource: AuditSourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    auditType: AuditTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    auditUser: AuditUserData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    auditUserAgent: AuditUserAgentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _auditEventEntityStates: AuditEventEntityStateData[] | null = null;
    private _auditEventEntityStatesPromise: Promise<AuditEventEntityStateData[]> | null  = null;
    private _auditEventEntityStatesSubject = new BehaviorSubject<AuditEventEntityStateData[] | null>(null);

                
    private _auditEventErrorMessages: AuditEventErrorMessageData[] | null = null;
    private _auditEventErrorMessagesPromise: Promise<AuditEventErrorMessageData[]> | null  = null;
    private _auditEventErrorMessagesSubject = new BehaviorSubject<AuditEventErrorMessageData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public AuditEventEntityStates$ = this._auditEventEntityStatesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._auditEventEntityStates === null && this._auditEventEntityStatesPromise === null) {
            this.loadAuditEventEntityStates(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _auditEventEntityStatesCount$: Observable<bigint | number> | null = null;
    public get AuditEventEntityStatesCount$(): Observable<bigint | number> {
        if (this._auditEventEntityStatesCount$ === null) {
            this._auditEventEntityStatesCount$ = AuditEventEntityStateService.Instance.GetAuditEventEntityStatesRowCount({auditEventId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._auditEventEntityStatesCount$;
    }



    public AuditEventErrorMessages$ = this._auditEventErrorMessagesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._auditEventErrorMessages === null && this._auditEventErrorMessagesPromise === null) {
            this.loadAuditEventErrorMessages(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _auditEventErrorMessagesCount$: Observable<bigint | number> | null = null;
    public get AuditEventErrorMessagesCount$(): Observable<bigint | number> {
        if (this._auditEventErrorMessagesCount$ === null) {
            this._auditEventErrorMessagesCount$ = AuditEventErrorMessageService.Instance.GetAuditEventErrorMessagesRowCount({auditEventId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._auditEventErrorMessagesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AuditEventData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.auditEvent.Reload();
  //
  //  Non Async:
  //
  //     auditEvent[0].Reload().then(x => {
  //        this.auditEvent = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AuditEventService.Instance.GetAuditEvent(this.id, includeRelations)
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
     this._auditEventEntityStates = null;
     this._auditEventEntityStatesPromise = null;
     this._auditEventEntityStatesSubject.next(null);
     this._auditEventEntityStatesCount$ = null;

     this._auditEventErrorMessages = null;
     this._auditEventErrorMessagesPromise = null;
     this._auditEventErrorMessagesSubject.next(null);
     this._auditEventErrorMessagesCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the AuditEventEntityStates for this AuditEvent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.auditEvent.AuditEventEntityStates.then(auditEvents => { ... })
     *   or
     *   await this.auditEvent.auditEvents
     *
    */
    public get AuditEventEntityStates(): Promise<AuditEventEntityStateData[]> {
        if (this._auditEventEntityStates !== null) {
            return Promise.resolve(this._auditEventEntityStates);
        }

        if (this._auditEventEntityStatesPromise !== null) {
            return this._auditEventEntityStatesPromise;
        }

        // Start the load
        this.loadAuditEventEntityStates();

        return this._auditEventEntityStatesPromise!;
    }



    private loadAuditEventEntityStates(): void {

        this._auditEventEntityStatesPromise = lastValueFrom(
            AuditEventService.Instance.GetAuditEventEntityStatesForAuditEvent(this.id)
        )
        .then(AuditEventEntityStates => {
            this._auditEventEntityStates = AuditEventEntityStates ?? [];
            this._auditEventEntityStatesSubject.next(this._auditEventEntityStates);
            return this._auditEventEntityStates;
         })
        .catch(err => {
            this._auditEventEntityStates = [];
            this._auditEventEntityStatesSubject.next(this._auditEventEntityStates);
            throw err;
        })
        .finally(() => {
            this._auditEventEntityStatesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached AuditEventEntityState. Call after mutations to force refresh.
     */
    public ClearAuditEventEntityStatesCache(): void {
        this._auditEventEntityStates = null;
        this._auditEventEntityStatesPromise = null;
        this._auditEventEntityStatesSubject.next(this._auditEventEntityStates);      // Emit to observable
    }

    public get HasAuditEventEntityStates(): Promise<boolean> {
        return this.AuditEventEntityStates.then(auditEventEntityStates => auditEventEntityStates.length > 0);
    }


    /**
     *
     * Gets the AuditEventErrorMessages for this AuditEvent.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.auditEvent.AuditEventErrorMessages.then(auditEvents => { ... })
     *   or
     *   await this.auditEvent.auditEvents
     *
    */
    public get AuditEventErrorMessages(): Promise<AuditEventErrorMessageData[]> {
        if (this._auditEventErrorMessages !== null) {
            return Promise.resolve(this._auditEventErrorMessages);
        }

        if (this._auditEventErrorMessagesPromise !== null) {
            return this._auditEventErrorMessagesPromise;
        }

        // Start the load
        this.loadAuditEventErrorMessages();

        return this._auditEventErrorMessagesPromise!;
    }



    private loadAuditEventErrorMessages(): void {

        this._auditEventErrorMessagesPromise = lastValueFrom(
            AuditEventService.Instance.GetAuditEventErrorMessagesForAuditEvent(this.id)
        )
        .then(AuditEventErrorMessages => {
            this._auditEventErrorMessages = AuditEventErrorMessages ?? [];
            this._auditEventErrorMessagesSubject.next(this._auditEventErrorMessages);
            return this._auditEventErrorMessages;
         })
        .catch(err => {
            this._auditEventErrorMessages = [];
            this._auditEventErrorMessagesSubject.next(this._auditEventErrorMessages);
            throw err;
        })
        .finally(() => {
            this._auditEventErrorMessagesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached AuditEventErrorMessage. Call after mutations to force refresh.
     */
    public ClearAuditEventErrorMessagesCache(): void {
        this._auditEventErrorMessages = null;
        this._auditEventErrorMessagesPromise = null;
        this._auditEventErrorMessagesSubject.next(this._auditEventErrorMessages);      // Emit to observable
    }

    public get HasAuditEventErrorMessages(): Promise<boolean> {
        return this.AuditEventErrorMessages.then(auditEventErrorMessages => auditEventErrorMessages.length > 0);
    }




    /**
     * Updates the state of this AuditEventData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AuditEventData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AuditEventSubmitData {
        return AuditEventService.Instance.ConvertToAuditEventSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AuditEventService extends SecureEndpointBase {

    private static _instance: AuditEventService;
    private listCache: Map<string, Observable<Array<AuditEventData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AuditEventBasicListData>>>;
    private recordCache: Map<string, Observable<AuditEventData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private auditEventEntityStateService: AuditEventEntityStateService,
        private auditEventErrorMessageService: AuditEventErrorMessageService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AuditEventData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AuditEventBasicListData>>>();
        this.recordCache = new Map<string, Observable<AuditEventData>>();

        AuditEventService._instance = this;
    }

    public static get Instance(): AuditEventService {
      return AuditEventService._instance;
    }


    public ClearListCaches(config: AuditEventQueryParameters | null = null) {

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


    public ConvertToAuditEventSubmitData(data: AuditEventData): AuditEventSubmitData {

        let output = new AuditEventSubmitData();

        output.id = data.id;
        output.startTime = data.startTime;
        output.stopTime = data.stopTime;
        output.completedSuccessfully = data.completedSuccessfully;
        output.auditUserId = data.auditUserId;
        output.auditSessionId = data.auditSessionId;
        output.auditTypeId = data.auditTypeId;
        output.auditAccessTypeId = data.auditAccessTypeId;
        output.auditSourceId = data.auditSourceId;
        output.auditUserAgentId = data.auditUserAgentId;
        output.auditModuleId = data.auditModuleId;
        output.auditModuleEntityId = data.auditModuleEntityId;
        output.auditResourceId = data.auditResourceId;
        output.auditHostSystemId = data.auditHostSystemId;
        output.primaryKey = data.primaryKey;
        output.threadId = data.threadId;
        output.message = data.message;

        return output;
    }

    public GetAuditEvent(id: bigint | number, includeRelations: boolean = true) : Observable<AuditEventData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const auditEvent$ = this.requestAuditEvent(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditEvent", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, auditEvent$);

            return auditEvent$;
        }

        return this.recordCache.get(configHash) as Observable<AuditEventData>;
    }

    private requestAuditEvent(id: bigint | number, includeRelations: boolean = true) : Observable<AuditEventData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AuditEventData>(this.baseUrl + 'api/AuditEvent/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAuditEvent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditEvent(id, includeRelations));
            }));
    }

    public GetAuditEventList(config: AuditEventQueryParameters | any = null) : Observable<Array<AuditEventData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const auditEventList$ = this.requestAuditEventList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditEvent list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, auditEventList$);

            return auditEventList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AuditEventData>>;
    }


    private requestAuditEventList(config: AuditEventQueryParameters | any) : Observable <Array<AuditEventData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditEventData>>(this.baseUrl + 'api/AuditEvents', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAuditEventList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditEventList(config));
            }));
    }

    public GetAuditEventsRowCount(config: AuditEventQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const auditEventsRowCount$ = this.requestAuditEventsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditEvents row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, auditEventsRowCount$);

            return auditEventsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAuditEventsRowCount(config: AuditEventQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AuditEvents/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditEventsRowCount(config));
            }));
    }

    public GetAuditEventsBasicListData(config: AuditEventQueryParameters | any = null) : Observable<Array<AuditEventBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const auditEventsBasicListData$ = this.requestAuditEventsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditEvents basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, auditEventsBasicListData$);

            return auditEventsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AuditEventBasicListData>>;
    }


    private requestAuditEventsBasicListData(config: AuditEventQueryParameters | any) : Observable<Array<AuditEventBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditEventBasicListData>>(this.baseUrl + 'api/AuditEvents/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditEventsBasicListData(config));
            }));

    }


    public PutAuditEvent(id: bigint | number, auditEvent: AuditEventSubmitData) : Observable<AuditEventData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AuditEventData>(this.baseUrl + 'api/AuditEvent/' + id.toString(), auditEvent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditEvent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAuditEvent(id, auditEvent));
            }));
    }


    public PostAuditEvent(auditEvent: AuditEventSubmitData) : Observable<AuditEventData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AuditEventData>(this.baseUrl + 'api/AuditEvent', auditEvent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditEvent(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAuditEvent(auditEvent));
            }));
    }

  
    public DeleteAuditEvent(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AuditEvent/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAuditEvent(id));
            }));
    }


    private getConfigHash(config: AuditEventQueryParameters | any): string {

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

    public userIsAuditorAuditEventReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAuditorAuditEventReader = this.authService.isAuditorReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Auditor.AuditEvents
        //
        if (userIsAuditorAuditEventReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAuditorAuditEventReader = user.readPermission >= 0;
            } else {
                userIsAuditorAuditEventReader = false;
            }
        }

        return userIsAuditorAuditEventReader;
    }


    public userIsAuditorAuditEventWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAuditorAuditEventWriter = this.authService.isAuditorReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Auditor.AuditEvents
        //
        if (userIsAuditorAuditEventWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAuditorAuditEventWriter = user.writePermission >= 0;
          } else {
            userIsAuditorAuditEventWriter = false;
          }      
        }

        return userIsAuditorAuditEventWriter;
    }

    public GetAuditEventEntityStatesForAuditEvent(auditEventId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AuditEventEntityStateData[]> {
        return this.auditEventEntityStateService.GetAuditEventEntityStateList({
            auditEventId: auditEventId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetAuditEventErrorMessagesForAuditEvent(auditEventId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AuditEventErrorMessageData[]> {
        return this.auditEventErrorMessageService.GetAuditEventErrorMessageList({
            auditEventId: auditEventId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AuditEventData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AuditEventData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AuditEventTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAuditEvent(raw: any): AuditEventData {
    if (!raw) return raw;

    //
    // Create a AuditEventData object instance with correct prototype
    //
    const revived = Object.create(AuditEventData.prototype) as AuditEventData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._auditEventEntityStates = null;
    (revived as any)._auditEventEntityStatesPromise = null;
    (revived as any)._auditEventEntityStatesSubject = new BehaviorSubject<AuditEventEntityStateData[] | null>(null);

    (revived as any)._auditEventErrorMessages = null;
    (revived as any)._auditEventErrorMessagesPromise = null;
    (revived as any)._auditEventErrorMessagesSubject = new BehaviorSubject<AuditEventErrorMessageData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAuditEventXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).AuditEventEntityStates$ = (revived as any)._auditEventEntityStatesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._auditEventEntityStates === null && (revived as any)._auditEventEntityStatesPromise === null) {
                (revived as any).loadAuditEventEntityStates();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._auditEventEntityStatesCount$ = null;


    (revived as any).AuditEventErrorMessages$ = (revived as any)._auditEventErrorMessagesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._auditEventErrorMessages === null && (revived as any)._auditEventErrorMessagesPromise === null) {
                (revived as any).loadAuditEventErrorMessages();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._auditEventErrorMessagesCount$ = null;



    return revived;
  }

  private ReviveAuditEventList(rawList: any[]): AuditEventData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAuditEvent(raw));
  }

}
