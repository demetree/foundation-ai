import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, BehaviorSubject, catchError, throwError, lastValueFrom, map  } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';
import { AuditEventData } from './audit-event.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AuditEventErrorMessageQueryParameters {
    auditEventId: bigint | number | null | undefined = null;
    errorMessage: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class AuditEventErrorMessageSubmitData {
    id!: bigint | number;
    auditEventId!: bigint | number;
    errorMessage!: string;
}


export class AuditEventErrorMessageBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AuditEventErrorMessageChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `auditEventErrorMessage.AuditEventErrorMessageChildren$` — use with `| async` in templates
//        • Promise:    `auditEventErrorMessage.AuditEventErrorMessageChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="auditEventErrorMessage.AuditEventErrorMessageChildren$ | async"`), or
//        • Access the promise getter (`auditEventErrorMessage.AuditEventErrorMessageChildren` or `await auditEventErrorMessage.AuditEventErrorMessageChildren`)
//    - Simply reading `auditEventErrorMessage.AuditEventErrorMessageChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await auditEventErrorMessage.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AuditEventErrorMessageData {
    id!: bigint | number;
    auditEventId!: bigint | number;
    errorMessage!: string;
    auditEvent: AuditEventData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any AuditEventErrorMessageData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.auditEventErrorMessage.Reload();
  //
  //  Non Async:
  //
  //     auditEventErrorMessage[0].Reload().then(x => {
  //        this.auditEventErrorMessage = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AuditEventErrorMessageService.Instance.GetAuditEventErrorMessage(this.id, includeRelations)
    );

    // Merge fresh data into this instance (preserves reference)
    this.UpdateFrom(fresh as this);

    // Clear all lazy caches to force re-load on next access
    this.clearAllLazyCaches();

    return this;
  }


  private clearAllLazyCaches(): void {
     // Reset every collection cache and notify subscribers
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //


    /**
     * Updates the state of this AuditEventErrorMessageData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AuditEventErrorMessageData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AuditEventErrorMessageSubmitData {
        return AuditEventErrorMessageService.Instance.ConvertToAuditEventErrorMessageSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AuditEventErrorMessageService extends SecureEndpointBase {

    private static _instance: AuditEventErrorMessageService;
    private listCache: Map<string, Observable<Array<AuditEventErrorMessageData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AuditEventErrorMessageBasicListData>>>;
    private recordCache: Map<string, Observable<AuditEventErrorMessageData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AuditEventErrorMessageData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AuditEventErrorMessageBasicListData>>>();
        this.recordCache = new Map<string, Observable<AuditEventErrorMessageData>>();

        AuditEventErrorMessageService._instance = this;
    }

    public static get Instance(): AuditEventErrorMessageService {
      return AuditEventErrorMessageService._instance;
    }


    public ClearListCaches(config: AuditEventErrorMessageQueryParameters | null = null) {

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


    public ConvertToAuditEventErrorMessageSubmitData(data: AuditEventErrorMessageData): AuditEventErrorMessageSubmitData {

        let output = new AuditEventErrorMessageSubmitData();

        output.id = data.id;
        output.auditEventId = data.auditEventId;
        output.errorMessage = data.errorMessage;

        return output;
    }

    public GetAuditEventErrorMessage(id: bigint | number, includeRelations: boolean = true) : Observable<AuditEventErrorMessageData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const auditEventErrorMessage$ = this.requestAuditEventErrorMessage(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditEventErrorMessage", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, auditEventErrorMessage$);

            return auditEventErrorMessage$;
        }

        return this.recordCache.get(configHash) as Observable<AuditEventErrorMessageData>;
    }

    private requestAuditEventErrorMessage(id: bigint | number, includeRelations: boolean = true) : Observable<AuditEventErrorMessageData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AuditEventErrorMessageData>(this.baseUrl + 'api/AuditEventErrorMessage/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAuditEventErrorMessage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditEventErrorMessage(id, includeRelations));
            }));
    }

    public GetAuditEventErrorMessageList(config: AuditEventErrorMessageQueryParameters | any = null) : Observable<Array<AuditEventErrorMessageData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const auditEventErrorMessageList$ = this.requestAuditEventErrorMessageList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditEventErrorMessage list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, auditEventErrorMessageList$);

            return auditEventErrorMessageList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AuditEventErrorMessageData>>;
    }


    private requestAuditEventErrorMessageList(config: AuditEventErrorMessageQueryParameters | any) : Observable <Array<AuditEventErrorMessageData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditEventErrorMessageData>>(this.baseUrl + 'api/AuditEventErrorMessages', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAuditEventErrorMessageList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAuditEventErrorMessageList(config));
            }));
    }

    public GetAuditEventErrorMessagesRowCount(config: AuditEventErrorMessageQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const auditEventErrorMessagesRowCount$ = this.requestAuditEventErrorMessagesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AuditEventErrorMessages row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, auditEventErrorMessagesRowCount$);

            return auditEventErrorMessagesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAuditEventErrorMessagesRowCount(config: AuditEventErrorMessageQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AuditEventErrorMessages/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditEventErrorMessagesRowCount(config));
            }));
    }

    public GetAuditEventErrorMessagesBasicListData(config: AuditEventErrorMessageQueryParameters | any = null) : Observable<Array<AuditEventErrorMessageBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const auditEventErrorMessagesBasicListData$ = this.requestAuditEventErrorMessagesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AuditEventErrorMessages basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, auditEventErrorMessagesBasicListData$);

            return auditEventErrorMessagesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AuditEventErrorMessageBasicListData>>;
    }


    private requestAuditEventErrorMessagesBasicListData(config: AuditEventErrorMessageQueryParameters | any) : Observable<Array<AuditEventErrorMessageBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AuditEventErrorMessageBasicListData>>(this.baseUrl + 'api/AuditEventErrorMessages/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAuditEventErrorMessagesBasicListData(config));
            }));

    }


    public PutAuditEventErrorMessage(id: bigint | number, auditEventErrorMessage: AuditEventErrorMessageSubmitData) : Observable<AuditEventErrorMessageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AuditEventErrorMessageData>(this.baseUrl + 'api/AuditEventErrorMessage/' + id.toString(), auditEventErrorMessage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditEventErrorMessage(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAuditEventErrorMessage(id, auditEventErrorMessage));
            }));
    }


    public PostAuditEventErrorMessage(auditEventErrorMessage: AuditEventErrorMessageSubmitData) : Observable<AuditEventErrorMessageData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AuditEventErrorMessageData>(this.baseUrl + 'api/AuditEventErrorMessage', auditEventErrorMessage, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAuditEventErrorMessage(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAuditEventErrorMessage(auditEventErrorMessage));
            }));
    }

  
    public DeleteAuditEventErrorMessage(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AuditEventErrorMessage/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAuditEventErrorMessage(id));
            }));
    }


    private getConfigHash(config: AuditEventErrorMessageQueryParameters | any): string {

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

    public userIsAuditorAuditEventErrorMessageReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAuditorAuditEventErrorMessageReader = this.authService.isAuditorReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Auditor.AuditEventErrorMessages
        //
        if (userIsAuditorAuditEventErrorMessageReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAuditorAuditEventErrorMessageReader = user.readPermission >= 0;
            } else {
                userIsAuditorAuditEventErrorMessageReader = false;
            }
        }

        return userIsAuditorAuditEventErrorMessageReader;
    }


    public userIsAuditorAuditEventErrorMessageWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAuditorAuditEventErrorMessageWriter = this.authService.isAuditorReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Auditor.AuditEventErrorMessages
        //
        if (userIsAuditorAuditEventErrorMessageWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAuditorAuditEventErrorMessageWriter = user.writePermission >= 0;
          } else {
            userIsAuditorAuditEventErrorMessageWriter = false;
          }      
        }

        return userIsAuditorAuditEventErrorMessageWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full AuditEventErrorMessageData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AuditEventErrorMessageData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AuditEventErrorMessageTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAuditEventErrorMessage(raw: any): AuditEventErrorMessageData {
    if (!raw) return raw;

    //
    // Create a AuditEventErrorMessageData object instance with correct prototype
    //
    const revived = Object.create(AuditEventErrorMessageData.prototype) as AuditEventErrorMessageData;

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
    // 2. But private methods (loadAuditEventErrorMessageXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveAuditEventErrorMessageList(rawList: any[]): AuditEventErrorMessageData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAuditEventErrorMessage(raw));
  }

}
