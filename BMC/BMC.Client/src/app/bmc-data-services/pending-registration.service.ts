/*

   GENERATED SERVICE FOR THE PENDINGREGISTRATION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PendingRegistration table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PendingRegistrationQueryParameters {
    accountName: string | null | undefined = null;
    emailAddress: string | null | undefined = null;
    displayName: string | null | undefined = null;
    passwordHash: string | null | undefined = null;
    verificationCode: string | null | undefined = null;
    codeExpiresAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    verificationAttempts: bigint | number | null | undefined = null;
    status: string | null | undefined = null;
    createdAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    verifiedAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    provisionedAt: string | null | undefined = null;        // ISO 8601 (full datetime)
    ipAddress: string | null | undefined = null;
    userAgent: string | null | undefined = null;
    verificationChannel: string | null | undefined = null;
    failureReason: string | null | undefined = null;
    provisionedSecurityUserId: bigint | number | null | undefined = null;
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
export class PendingRegistrationSubmitData {
    id!: bigint | number;
    accountName!: string;
    emailAddress!: string;
    displayName: string | null = null;
    passwordHash!: string;
    verificationCode!: string;
    codeExpiresAt!: string;      // ISO 8601 (full datetime)
    verificationAttempts!: bigint | number;
    status!: string;
    createdAt!: string;      // ISO 8601 (full datetime)
    verifiedAt: string | null = null;     // ISO 8601 (full datetime)
    provisionedAt: string | null = null;     // ISO 8601 (full datetime)
    ipAddress: string | null = null;
    userAgent: string | null = null;
    verificationChannel: string | null = null;
    failureReason: string | null = null;
    provisionedSecurityUserId: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class PendingRegistrationBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PendingRegistrationChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `pendingRegistration.PendingRegistrationChildren$` — use with `| async` in templates
//        • Promise:    `pendingRegistration.PendingRegistrationChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="pendingRegistration.PendingRegistrationChildren$ | async"`), or
//        • Access the promise getter (`pendingRegistration.PendingRegistrationChildren` or `await pendingRegistration.PendingRegistrationChildren`)
//    - Simply reading `pendingRegistration.PendingRegistrationChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await pendingRegistration.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PendingRegistrationData {
    id!: bigint | number;
    accountName!: string;
    emailAddress!: string;
    displayName!: string | null;
    passwordHash!: string;
    verificationCode!: string;
    codeExpiresAt!: string;      // ISO 8601 (full datetime)
    verificationAttempts!: bigint | number;
    status!: string;
    createdAt!: string;      // ISO 8601 (full datetime)
    verifiedAt!: string | null;   // ISO 8601 (full datetime)
    provisionedAt!: string | null;   // ISO 8601 (full datetime)
    ipAddress!: string | null;
    userAgent!: string | null;
    verificationChannel!: string | null;
    failureReason!: string | null;
    provisionedSecurityUserId!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

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
  // Promise based reload method to allow rebuilding of any PendingRegistrationData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.pendingRegistration.Reload();
  //
  //  Non Async:
  //
  //     pendingRegistration[0].Reload().then(x => {
  //        this.pendingRegistration = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PendingRegistrationService.Instance.GetPendingRegistration(this.id, includeRelations)
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
     * Updates the state of this PendingRegistrationData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PendingRegistrationData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PendingRegistrationSubmitData {
        return PendingRegistrationService.Instance.ConvertToPendingRegistrationSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PendingRegistrationService extends SecureEndpointBase {

    private static _instance: PendingRegistrationService;
    private listCache: Map<string, Observable<Array<PendingRegistrationData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PendingRegistrationBasicListData>>>;
    private recordCache: Map<string, Observable<PendingRegistrationData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PendingRegistrationData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PendingRegistrationBasicListData>>>();
        this.recordCache = new Map<string, Observable<PendingRegistrationData>>();

        PendingRegistrationService._instance = this;
    }

    public static get Instance(): PendingRegistrationService {
      return PendingRegistrationService._instance;
    }


    public ClearListCaches(config: PendingRegistrationQueryParameters | null = null) {

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


    public ConvertToPendingRegistrationSubmitData(data: PendingRegistrationData): PendingRegistrationSubmitData {

        let output = new PendingRegistrationSubmitData();

        output.id = data.id;
        output.accountName = data.accountName;
        output.emailAddress = data.emailAddress;
        output.displayName = data.displayName;
        output.passwordHash = data.passwordHash;
        output.verificationCode = data.verificationCode;
        output.codeExpiresAt = data.codeExpiresAt;
        output.verificationAttempts = data.verificationAttempts;
        output.status = data.status;
        output.createdAt = data.createdAt;
        output.verifiedAt = data.verifiedAt;
        output.provisionedAt = data.provisionedAt;
        output.ipAddress = data.ipAddress;
        output.userAgent = data.userAgent;
        output.verificationChannel = data.verificationChannel;
        output.failureReason = data.failureReason;
        output.provisionedSecurityUserId = data.provisionedSecurityUserId;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPendingRegistration(id: bigint | number, includeRelations: boolean = true) : Observable<PendingRegistrationData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const pendingRegistration$ = this.requestPendingRegistration(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PendingRegistration", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, pendingRegistration$);

            return pendingRegistration$;
        }

        return this.recordCache.get(configHash) as Observable<PendingRegistrationData>;
    }

    private requestPendingRegistration(id: bigint | number, includeRelations: boolean = true) : Observable<PendingRegistrationData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PendingRegistrationData>(this.baseUrl + 'api/PendingRegistration/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePendingRegistration(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPendingRegistration(id, includeRelations));
            }));
    }

    public GetPendingRegistrationList(config: PendingRegistrationQueryParameters | any = null) : Observable<Array<PendingRegistrationData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const pendingRegistrationList$ = this.requestPendingRegistrationList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PendingRegistration list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, pendingRegistrationList$);

            return pendingRegistrationList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PendingRegistrationData>>;
    }


    private requestPendingRegistrationList(config: PendingRegistrationQueryParameters | any) : Observable <Array<PendingRegistrationData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PendingRegistrationData>>(this.baseUrl + 'api/PendingRegistrations', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePendingRegistrationList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPendingRegistrationList(config));
            }));
    }

    public GetPendingRegistrationsRowCount(config: PendingRegistrationQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const pendingRegistrationsRowCount$ = this.requestPendingRegistrationsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PendingRegistrations row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, pendingRegistrationsRowCount$);

            return pendingRegistrationsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPendingRegistrationsRowCount(config: PendingRegistrationQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PendingRegistrations/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPendingRegistrationsRowCount(config));
            }));
    }

    public GetPendingRegistrationsBasicListData(config: PendingRegistrationQueryParameters | any = null) : Observable<Array<PendingRegistrationBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const pendingRegistrationsBasicListData$ = this.requestPendingRegistrationsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PendingRegistrations basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, pendingRegistrationsBasicListData$);

            return pendingRegistrationsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PendingRegistrationBasicListData>>;
    }


    private requestPendingRegistrationsBasicListData(config: PendingRegistrationQueryParameters | any) : Observable<Array<PendingRegistrationBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PendingRegistrationBasicListData>>(this.baseUrl + 'api/PendingRegistrations/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPendingRegistrationsBasicListData(config));
            }));

    }


    public PutPendingRegistration(id: bigint | number, pendingRegistration: PendingRegistrationSubmitData) : Observable<PendingRegistrationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PendingRegistrationData>(this.baseUrl + 'api/PendingRegistration/' + id.toString(), pendingRegistration, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePendingRegistration(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPendingRegistration(id, pendingRegistration));
            }));
    }


    public PostPendingRegistration(pendingRegistration: PendingRegistrationSubmitData) : Observable<PendingRegistrationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PendingRegistrationData>(this.baseUrl + 'api/PendingRegistration', pendingRegistration, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePendingRegistration(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPendingRegistration(pendingRegistration));
            }));
    }

  
    public DeletePendingRegistration(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PendingRegistration/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePendingRegistration(id));
            }));
    }


    private getConfigHash(config: PendingRegistrationQueryParameters | any): string {

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

    public userIsBMCPendingRegistrationReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCPendingRegistrationReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.PendingRegistrations
        //
        if (userIsBMCPendingRegistrationReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCPendingRegistrationReader = user.readPermission >= 100;
            } else {
                userIsBMCPendingRegistrationReader = false;
            }
        }

        return userIsBMCPendingRegistrationReader;
    }


    public userIsBMCPendingRegistrationWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCPendingRegistrationWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.PendingRegistrations
        //
        if (userIsBMCPendingRegistrationWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCPendingRegistrationWriter = user.writePermission >= 255;
          } else {
            userIsBMCPendingRegistrationWriter = false;
          }      
        }

        return userIsBMCPendingRegistrationWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full PendingRegistrationData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PendingRegistrationData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PendingRegistrationTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePendingRegistration(raw: any): PendingRegistrationData {
    if (!raw) return raw;

    //
    // Create a PendingRegistrationData object instance with correct prototype
    //
    const revived = Object.create(PendingRegistrationData.prototype) as PendingRegistrationData;

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
    // 2. But private methods (loadPendingRegistrationXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private RevivePendingRegistrationList(rawList: any[]): PendingRegistrationData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePendingRegistration(raw));
  }

}
