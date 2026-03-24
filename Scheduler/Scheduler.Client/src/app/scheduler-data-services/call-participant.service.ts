/*

   GENERATED SERVICE FOR THE CALLPARTICIPANT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the CallParticipant table.

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
import { CallData } from './call.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class CallParticipantQueryParameters {
    callId: bigint | number | null | undefined = null;
    userId: bigint | number | null | undefined = null;
    role: string | null | undefined = null;
    status: string | null | undefined = null;
    joinedDateTime: string | null | undefined = null;        // ISO 8601 (full datetime)
    leftDateTime: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class CallParticipantSubmitData {
    id!: bigint | number;
    callId!: bigint | number;
    userId!: bigint | number;
    role!: string;
    status!: string;
    joinedDateTime: string | null = null;     // ISO 8601 (full datetime)
    leftDateTime: string | null = null;     // ISO 8601 (full datetime)
    active!: boolean;
    deleted!: boolean;
}


export class CallParticipantBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. CallParticipantChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `callParticipant.CallParticipantChildren$` — use with `| async` in templates
//        • Promise:    `callParticipant.CallParticipantChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="callParticipant.CallParticipantChildren$ | async"`), or
//        • Access the promise getter (`callParticipant.CallParticipantChildren` or `await callParticipant.CallParticipantChildren`)
//    - Simply reading `callParticipant.CallParticipantChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await callParticipant.Reload()` to refresh the entire object and clear all lazy caches.
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
export class CallParticipantData {
    id!: bigint | number;
    callId!: bigint | number;
    userId!: bigint | number;
    role!: string;
    status!: string;
    joinedDateTime!: string | null;   // ISO 8601 (full datetime)
    leftDateTime!: string | null;   // ISO 8601 (full datetime)
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    call: CallData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //

  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any CallParticipantData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.callParticipant.Reload();
  //
  //  Non Async:
  //
  //     callParticipant[0].Reload().then(x => {
  //        this.callParticipant = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      CallParticipantService.Instance.GetCallParticipant(this.id, includeRelations)
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
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //


    /**
     * Updates the state of this CallParticipantData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this CallParticipantData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): CallParticipantSubmitData {
        return CallParticipantService.Instance.ConvertToCallParticipantSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class CallParticipantService extends SecureEndpointBase {

    private static _instance: CallParticipantService;
    private listCache: Map<string, Observable<Array<CallParticipantData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<CallParticipantBasicListData>>>;
    private recordCache: Map<string, Observable<CallParticipantData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<CallParticipantData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<CallParticipantBasicListData>>>();
        this.recordCache = new Map<string, Observable<CallParticipantData>>();

        CallParticipantService._instance = this;
    }

    public static get Instance(): CallParticipantService {
      return CallParticipantService._instance;
    }


    public ClearListCaches(config: CallParticipantQueryParameters | null = null) {

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


    public ConvertToCallParticipantSubmitData(data: CallParticipantData): CallParticipantSubmitData {

        let output = new CallParticipantSubmitData();

        output.id = data.id;
        output.callId = data.callId;
        output.userId = data.userId;
        output.role = data.role;
        output.status = data.status;
        output.joinedDateTime = data.joinedDateTime;
        output.leftDateTime = data.leftDateTime;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetCallParticipant(id: bigint | number, includeRelations: boolean = true) : Observable<CallParticipantData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const callParticipant$ = this.requestCallParticipant(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get CallParticipant", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, callParticipant$);

            return callParticipant$;
        }

        return this.recordCache.get(configHash) as Observable<CallParticipantData>;
    }

    private requestCallParticipant(id: bigint | number, includeRelations: boolean = true) : Observable<CallParticipantData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CallParticipantData>(this.baseUrl + 'api/CallParticipant/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveCallParticipant(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestCallParticipant(id, includeRelations));
            }));
    }

    public GetCallParticipantList(config: CallParticipantQueryParameters | any = null) : Observable<Array<CallParticipantData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const callParticipantList$ = this.requestCallParticipantList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get CallParticipant list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, callParticipantList$);

            return callParticipantList$;
        }

        return this.listCache.get(configHash) as Observable<Array<CallParticipantData>>;
    }


    private requestCallParticipantList(config: CallParticipantQueryParameters | any) : Observable <Array<CallParticipantData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CallParticipantData>>(this.baseUrl + 'api/CallParticipants', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveCallParticipantList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestCallParticipantList(config));
            }));
    }

    public GetCallParticipantsRowCount(config: CallParticipantQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const callParticipantsRowCount$ = this.requestCallParticipantsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get CallParticipants row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, callParticipantsRowCount$);

            return callParticipantsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestCallParticipantsRowCount(config: CallParticipantQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/CallParticipants/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCallParticipantsRowCount(config));
            }));
    }

    public GetCallParticipantsBasicListData(config: CallParticipantQueryParameters | any = null) : Observable<Array<CallParticipantBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const callParticipantsBasicListData$ = this.requestCallParticipantsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get CallParticipants basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, callParticipantsBasicListData$);

            return callParticipantsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<CallParticipantBasicListData>>;
    }


    private requestCallParticipantsBasicListData(config: CallParticipantQueryParameters | any) : Observable<Array<CallParticipantBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CallParticipantBasicListData>>(this.baseUrl + 'api/CallParticipants/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCallParticipantsBasicListData(config));
            }));

    }


    public PutCallParticipant(id: bigint | number, callParticipant: CallParticipantSubmitData) : Observable<CallParticipantData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CallParticipantData>(this.baseUrl + 'api/CallParticipant/' + id.toString(), callParticipant, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCallParticipant(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutCallParticipant(id, callParticipant));
            }));
    }


    public PostCallParticipant(callParticipant: CallParticipantSubmitData) : Observable<CallParticipantData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<CallParticipantData>(this.baseUrl + 'api/CallParticipant', callParticipant, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCallParticipant(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostCallParticipant(callParticipant));
            }));
    }

  
    public DeleteCallParticipant(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/CallParticipant/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteCallParticipant(id));
            }));
    }


    private getConfigHash(config: CallParticipantQueryParameters | any): string {

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

    public userIsSchedulerCallParticipantReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerCallParticipantReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.CallParticipants
        //
        if (userIsSchedulerCallParticipantReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerCallParticipantReader = user.readPermission >= 50;
            } else {
                userIsSchedulerCallParticipantReader = false;
            }
        }

        return userIsSchedulerCallParticipantReader;
    }


    public userIsSchedulerCallParticipantWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerCallParticipantWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.CallParticipants
        //
        if (userIsSchedulerCallParticipantWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerCallParticipantWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerCallParticipantWriter = false;
          }      
        }

        return userIsSchedulerCallParticipantWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full CallParticipantData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the CallParticipantData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when CallParticipantTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveCallParticipant(raw: any): CallParticipantData {
    if (!raw) return raw;

    //
    // Create a CallParticipantData object instance with correct prototype
    //
    const revived = Object.create(CallParticipantData.prototype) as CallParticipantData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //

    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadCallParticipantXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveCallParticipantList(rawList: any[]): CallParticipantData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveCallParticipant(raw));
  }

}
