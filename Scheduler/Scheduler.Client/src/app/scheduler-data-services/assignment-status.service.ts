/*

   GENERATED SERVICE FOR THE ASSIGNMENTSTATUS TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AssignmentStatus table.

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
import { EventResourceAssignmentService, EventResourceAssignmentData } from './event-resource-assignment.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AssignmentStatusQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    color: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class AssignmentStatusSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    color: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class AssignmentStatusBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AssignmentStatusChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `assignmentStatus.AssignmentStatusChildren$` — use with `| async` in templates
//        • Promise:    `assignmentStatus.AssignmentStatusChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="assignmentStatus.AssignmentStatusChildren$ | async"`), or
//        • Access the promise getter (`assignmentStatus.AssignmentStatusChildren` or `await assignmentStatus.AssignmentStatusChildren`)
//    - Simply reading `assignmentStatus.AssignmentStatusChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await assignmentStatus.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AssignmentStatusData {
    id!: bigint | number;
    name!: string;
    description!: string;
    color!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _eventResourceAssignments: EventResourceAssignmentData[] | null = null;
    private _eventResourceAssignmentsPromise: Promise<EventResourceAssignmentData[]> | null  = null;
    private _eventResourceAssignmentsSubject = new BehaviorSubject<EventResourceAssignmentData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public EventResourceAssignments$ = this._eventResourceAssignmentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventResourceAssignments === null && this._eventResourceAssignmentsPromise === null) {
            this.loadEventResourceAssignments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _eventResourceAssignmentsCount$: Observable<bigint | number> | null = null;
    public get EventResourceAssignmentsCount$(): Observable<bigint | number> {
        if (this._eventResourceAssignmentsCount$ === null) {
            this._eventResourceAssignmentsCount$ = EventResourceAssignmentService.Instance.GetEventResourceAssignmentsRowCount({assignmentStatusId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._eventResourceAssignmentsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AssignmentStatusData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.assignmentStatus.Reload();
  //
  //  Non Async:
  //
  //     assignmentStatus[0].Reload().then(x => {
  //        this.assignmentStatus = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AssignmentStatusService.Instance.GetAssignmentStatus(this.id, includeRelations)
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
     this._eventResourceAssignments = null;
     this._eventResourceAssignmentsPromise = null;
     this._eventResourceAssignmentsSubject.next(null);
     this._eventResourceAssignmentsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the EventResourceAssignments for this AssignmentStatus.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.assignmentStatus.EventResourceAssignments.then(assignmentStatuses => { ... })
     *   or
     *   await this.assignmentStatus.assignmentStatuses
     *
    */
    public get EventResourceAssignments(): Promise<EventResourceAssignmentData[]> {
        if (this._eventResourceAssignments !== null) {
            return Promise.resolve(this._eventResourceAssignments);
        }

        if (this._eventResourceAssignmentsPromise !== null) {
            return this._eventResourceAssignmentsPromise;
        }

        // Start the load
        this.loadEventResourceAssignments();

        return this._eventResourceAssignmentsPromise!;
    }



    private loadEventResourceAssignments(): void {

        this._eventResourceAssignmentsPromise = lastValueFrom(
            AssignmentStatusService.Instance.GetEventResourceAssignmentsForAssignmentStatus(this.id)
        )
        .then(EventResourceAssignments => {
            this._eventResourceAssignments = EventResourceAssignments ?? [];
            this._eventResourceAssignmentsSubject.next(this._eventResourceAssignments);
            return this._eventResourceAssignments;
         })
        .catch(err => {
            this._eventResourceAssignments = [];
            this._eventResourceAssignmentsSubject.next(this._eventResourceAssignments);
            throw err;
        })
        .finally(() => {
            this._eventResourceAssignmentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventResourceAssignment. Call after mutations to force refresh.
     */
    public ClearEventResourceAssignmentsCache(): void {
        this._eventResourceAssignments = null;
        this._eventResourceAssignmentsPromise = null;
        this._eventResourceAssignmentsSubject.next(this._eventResourceAssignments);      // Emit to observable
    }

    public get HasEventResourceAssignments(): Promise<boolean> {
        return this.EventResourceAssignments.then(eventResourceAssignments => eventResourceAssignments.length > 0);
    }




    /**
     * Updates the state of this AssignmentStatusData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AssignmentStatusData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AssignmentStatusSubmitData {
        return AssignmentStatusService.Instance.ConvertToAssignmentStatusSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AssignmentStatusService extends SecureEndpointBase {

    private static _instance: AssignmentStatusService;
    private listCache: Map<string, Observable<Array<AssignmentStatusData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AssignmentStatusBasicListData>>>;
    private recordCache: Map<string, Observable<AssignmentStatusData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private eventResourceAssignmentService: EventResourceAssignmentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AssignmentStatusData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AssignmentStatusBasicListData>>>();
        this.recordCache = new Map<string, Observable<AssignmentStatusData>>();

        AssignmentStatusService._instance = this;
    }

    public static get Instance(): AssignmentStatusService {
      return AssignmentStatusService._instance;
    }


    public ClearListCaches(config: AssignmentStatusQueryParameters | null = null) {

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


    public ConvertToAssignmentStatusSubmitData(data: AssignmentStatusData): AssignmentStatusSubmitData {

        let output = new AssignmentStatusSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.color = data.color;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetAssignmentStatus(id: bigint | number, includeRelations: boolean = true) : Observable<AssignmentStatusData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const assignmentStatus$ = this.requestAssignmentStatus(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AssignmentStatus", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, assignmentStatus$);

            return assignmentStatus$;
        }

        return this.recordCache.get(configHash) as Observable<AssignmentStatusData>;
    }

    private requestAssignmentStatus(id: bigint | number, includeRelations: boolean = true) : Observable<AssignmentStatusData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AssignmentStatusData>(this.baseUrl + 'api/AssignmentStatus/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAssignmentStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAssignmentStatus(id, includeRelations));
            }));
    }

    public GetAssignmentStatusList(config: AssignmentStatusQueryParameters | any = null) : Observable<Array<AssignmentStatusData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const assignmentStatusList$ = this.requestAssignmentStatusList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AssignmentStatus list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, assignmentStatusList$);

            return assignmentStatusList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AssignmentStatusData>>;
    }


    private requestAssignmentStatusList(config: AssignmentStatusQueryParameters | any) : Observable <Array<AssignmentStatusData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AssignmentStatusData>>(this.baseUrl + 'api/AssignmentStatuses', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAssignmentStatusList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAssignmentStatusList(config));
            }));
    }

    public GetAssignmentStatusesRowCount(config: AssignmentStatusQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const assignmentStatusesRowCount$ = this.requestAssignmentStatusesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AssignmentStatuses row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, assignmentStatusesRowCount$);

            return assignmentStatusesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAssignmentStatusesRowCount(config: AssignmentStatusQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AssignmentStatuses/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAssignmentStatusesRowCount(config));
            }));
    }

    public GetAssignmentStatusesBasicListData(config: AssignmentStatusQueryParameters | any = null) : Observable<Array<AssignmentStatusBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const assignmentStatusesBasicListData$ = this.requestAssignmentStatusesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AssignmentStatuses basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, assignmentStatusesBasicListData$);

            return assignmentStatusesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AssignmentStatusBasicListData>>;
    }


    private requestAssignmentStatusesBasicListData(config: AssignmentStatusQueryParameters | any) : Observable<Array<AssignmentStatusBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AssignmentStatusBasicListData>>(this.baseUrl + 'api/AssignmentStatuses/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAssignmentStatusesBasicListData(config));
            }));

    }


    public PutAssignmentStatus(id: bigint | number, assignmentStatus: AssignmentStatusSubmitData) : Observable<AssignmentStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AssignmentStatusData>(this.baseUrl + 'api/AssignmentStatus/' + id.toString(), assignmentStatus, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAssignmentStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAssignmentStatus(id, assignmentStatus));
            }));
    }


    public PostAssignmentStatus(assignmentStatus: AssignmentStatusSubmitData) : Observable<AssignmentStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AssignmentStatusData>(this.baseUrl + 'api/AssignmentStatus', assignmentStatus, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAssignmentStatus(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAssignmentStatus(assignmentStatus));
            }));
    }

  
    public DeleteAssignmentStatus(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AssignmentStatus/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAssignmentStatus(id));
            }));
    }


    private getConfigHash(config: AssignmentStatusQueryParameters | any): string {

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

    public userIsSchedulerAssignmentStatusReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerAssignmentStatusReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.AssignmentStatuses
        //
        if (userIsSchedulerAssignmentStatusReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerAssignmentStatusReader = user.readPermission >= 1;
            } else {
                userIsSchedulerAssignmentStatusReader = false;
            }
        }

        return userIsSchedulerAssignmentStatusReader;
    }


    public userIsSchedulerAssignmentStatusWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerAssignmentStatusWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.AssignmentStatuses
        //
        if (userIsSchedulerAssignmentStatusWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerAssignmentStatusWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerAssignmentStatusWriter = false;
          }      
        }

        return userIsSchedulerAssignmentStatusWriter;
    }

    public GetEventResourceAssignmentsForAssignmentStatus(assignmentStatusId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventResourceAssignmentData[]> {
        return this.eventResourceAssignmentService.GetEventResourceAssignmentList({
            assignmentStatusId: assignmentStatusId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AssignmentStatusData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AssignmentStatusData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AssignmentStatusTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAssignmentStatus(raw: any): AssignmentStatusData {
    if (!raw) return raw;

    //
    // Create a AssignmentStatusData object instance with correct prototype
    //
    const revived = Object.create(AssignmentStatusData.prototype) as AssignmentStatusData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._eventResourceAssignments = null;
    (revived as any)._eventResourceAssignmentsPromise = null;
    (revived as any)._eventResourceAssignmentsSubject = new BehaviorSubject<EventResourceAssignmentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAssignmentStatusXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).EventResourceAssignments$ = (revived as any)._eventResourceAssignmentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventResourceAssignments === null && (revived as any)._eventResourceAssignmentsPromise === null) {
                (revived as any).loadEventResourceAssignments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._eventResourceAssignmentsCount$ = null;



    return revived;
  }

  private ReviveAssignmentStatusList(rawList: any[]): AssignmentStatusData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAssignmentStatus(raw));
  }

}
