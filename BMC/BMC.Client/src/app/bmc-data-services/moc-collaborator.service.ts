/*

   GENERATED SERVICE FOR THE MOCCOLLABORATOR TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the MocCollaborator table.

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
import { PublishedMocData } from './published-moc.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class MocCollaboratorQueryParameters {
    publishedMocId: bigint | number | null | undefined = null;
    collaboratorTenantGuid: string | null | undefined = null;
    accessLevel: string | null | undefined = null;
    invitedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    acceptedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    isAccepted: boolean | null | undefined = null;
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
export class MocCollaboratorSubmitData {
    id!: bigint | number;
    publishedMocId!: bigint | number;
    collaboratorTenantGuid!: string;
    accessLevel!: string;
    invitedDate!: string;      // ISO 8601 (full datetime)
    acceptedDate: string | null = null;     // ISO 8601 (full datetime)
    isAccepted!: boolean;
    active!: boolean;
    deleted!: boolean;
}


export class MocCollaboratorBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. MocCollaboratorChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `mocCollaborator.MocCollaboratorChildren$` — use with `| async` in templates
//        • Promise:    `mocCollaborator.MocCollaboratorChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="mocCollaborator.MocCollaboratorChildren$ | async"`), or
//        • Access the promise getter (`mocCollaborator.MocCollaboratorChildren` or `await mocCollaborator.MocCollaboratorChildren`)
//    - Simply reading `mocCollaborator.MocCollaboratorChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await mocCollaborator.Reload()` to refresh the entire object and clear all lazy caches.
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
export class MocCollaboratorData {
    id!: bigint | number;
    publishedMocId!: bigint | number;
    collaboratorTenantGuid!: string;
    accessLevel!: string;
    invitedDate!: string;      // ISO 8601 (full datetime)
    acceptedDate!: string | null;   // ISO 8601 (full datetime)
    isAccepted!: boolean;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    publishedMoc: PublishedMocData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any MocCollaboratorData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.mocCollaborator.Reload();
  //
  //  Non Async:
  //
  //     mocCollaborator[0].Reload().then(x => {
  //        this.mocCollaborator = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      MocCollaboratorService.Instance.GetMocCollaborator(this.id, includeRelations)
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
     * Updates the state of this MocCollaboratorData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this MocCollaboratorData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): MocCollaboratorSubmitData {
        return MocCollaboratorService.Instance.ConvertToMocCollaboratorSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class MocCollaboratorService extends SecureEndpointBase {

    private static _instance: MocCollaboratorService;
    private listCache: Map<string, Observable<Array<MocCollaboratorData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<MocCollaboratorBasicListData>>>;
    private recordCache: Map<string, Observable<MocCollaboratorData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<MocCollaboratorData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<MocCollaboratorBasicListData>>>();
        this.recordCache = new Map<string, Observable<MocCollaboratorData>>();

        MocCollaboratorService._instance = this;
    }

    public static get Instance(): MocCollaboratorService {
      return MocCollaboratorService._instance;
    }


    public ClearListCaches(config: MocCollaboratorQueryParameters | null = null) {

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


    public ConvertToMocCollaboratorSubmitData(data: MocCollaboratorData): MocCollaboratorSubmitData {

        let output = new MocCollaboratorSubmitData();

        output.id = data.id;
        output.publishedMocId = data.publishedMocId;
        output.collaboratorTenantGuid = data.collaboratorTenantGuid;
        output.accessLevel = data.accessLevel;
        output.invitedDate = data.invitedDate;
        output.acceptedDate = data.acceptedDate;
        output.isAccepted = data.isAccepted;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetMocCollaborator(id: bigint | number, includeRelations: boolean = true) : Observable<MocCollaboratorData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const mocCollaborator$ = this.requestMocCollaborator(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MocCollaborator", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, mocCollaborator$);

            return mocCollaborator$;
        }

        return this.recordCache.get(configHash) as Observable<MocCollaboratorData>;
    }

    private requestMocCollaborator(id: bigint | number, includeRelations: boolean = true) : Observable<MocCollaboratorData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<MocCollaboratorData>(this.baseUrl + 'api/MocCollaborator/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveMocCollaborator(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestMocCollaborator(id, includeRelations));
            }));
    }

    public GetMocCollaboratorList(config: MocCollaboratorQueryParameters | any = null) : Observable<Array<MocCollaboratorData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const mocCollaboratorList$ = this.requestMocCollaboratorList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MocCollaborator list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, mocCollaboratorList$);

            return mocCollaboratorList$;
        }

        return this.listCache.get(configHash) as Observable<Array<MocCollaboratorData>>;
    }


    private requestMocCollaboratorList(config: MocCollaboratorQueryParameters | any) : Observable <Array<MocCollaboratorData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MocCollaboratorData>>(this.baseUrl + 'api/MocCollaborators', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveMocCollaboratorList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestMocCollaboratorList(config));
            }));
    }

    public GetMocCollaboratorsRowCount(config: MocCollaboratorQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const mocCollaboratorsRowCount$ = this.requestMocCollaboratorsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get MocCollaborators row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, mocCollaboratorsRowCount$);

            return mocCollaboratorsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestMocCollaboratorsRowCount(config: MocCollaboratorQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/MocCollaborators/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMocCollaboratorsRowCount(config));
            }));
    }

    public GetMocCollaboratorsBasicListData(config: MocCollaboratorQueryParameters | any = null) : Observable<Array<MocCollaboratorBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const mocCollaboratorsBasicListData$ = this.requestMocCollaboratorsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get MocCollaborators basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, mocCollaboratorsBasicListData$);

            return mocCollaboratorsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<MocCollaboratorBasicListData>>;
    }


    private requestMocCollaboratorsBasicListData(config: MocCollaboratorQueryParameters | any) : Observable<Array<MocCollaboratorBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<MocCollaboratorBasicListData>>(this.baseUrl + 'api/MocCollaborators/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestMocCollaboratorsBasicListData(config));
            }));

    }


    public PutMocCollaborator(id: bigint | number, mocCollaborator: MocCollaboratorSubmitData) : Observable<MocCollaboratorData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<MocCollaboratorData>(this.baseUrl + 'api/MocCollaborator/' + id.toString(), mocCollaborator, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMocCollaborator(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutMocCollaborator(id, mocCollaborator));
            }));
    }


    public PostMocCollaborator(mocCollaborator: MocCollaboratorSubmitData) : Observable<MocCollaboratorData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<MocCollaboratorData>(this.baseUrl + 'api/MocCollaborator', mocCollaborator, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveMocCollaborator(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostMocCollaborator(mocCollaborator));
            }));
    }

  
    public DeleteMocCollaborator(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/MocCollaborator/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteMocCollaborator(id));
            }));
    }


    private getConfigHash(config: MocCollaboratorQueryParameters | any): string {

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

    public userIsBMCMocCollaboratorReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCMocCollaboratorReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.MocCollaborators
        //
        if (userIsBMCMocCollaboratorReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCMocCollaboratorReader = user.readPermission >= 1;
            } else {
                userIsBMCMocCollaboratorReader = false;
            }
        }

        return userIsBMCMocCollaboratorReader;
    }


    public userIsBMCMocCollaboratorWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCMocCollaboratorWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.MocCollaborators
        //
        if (userIsBMCMocCollaboratorWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCMocCollaboratorWriter = user.writePermission >= 1;
          } else {
            userIsBMCMocCollaboratorWriter = false;
          }      
        }

        return userIsBMCMocCollaboratorWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full MocCollaboratorData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the MocCollaboratorData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when MocCollaboratorTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveMocCollaborator(raw: any): MocCollaboratorData {
    if (!raw) return raw;

    //
    // Create a MocCollaboratorData object instance with correct prototype
    //
    const revived = Object.create(MocCollaboratorData.prototype) as MocCollaboratorData;

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
    // 2. But private methods (loadMocCollaboratorXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveMocCollaboratorList(rawList: any[]): MocCollaboratorData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveMocCollaborator(raw));
  }

}
