/*

   GENERATED SERVICE FOR THE RESOURCEQUALIFICATION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ResourceQualification table.

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
import { ResourceData } from './resource.service';
import { QualificationData } from './qualification.service';
import { ResourceQualificationChangeHistoryService, ResourceQualificationChangeHistoryData } from './resource-qualification-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ResourceQualificationQueryParameters {
    resourceId: bigint | number | null | undefined = null;
    qualificationId: bigint | number | null | undefined = null;
    issueDate: string | null | undefined = null;        // ISO 8601
    expiryDate: string | null | undefined = null;        // ISO 8601
    issuer: string | null | undefined = null;
    notes: string | null | undefined = null;
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
export class ResourceQualificationSubmitData {
    id!: bigint | number;
    resourceId!: bigint | number;
    qualificationId!: bigint | number;
    issueDate: string | null = null;     // ISO 8601
    expiryDate: string | null = null;     // ISO 8601
    issuer: string | null = null;
    notes: string | null = null;
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

export class ResourceQualificationBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ResourceQualificationChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `resourceQualification.ResourceQualificationChildren$` — use with `| async` in templates
//        • Promise:    `resourceQualification.ResourceQualificationChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="resourceQualification.ResourceQualificationChildren$ | async"`), or
//        • Access the promise getter (`resourceQualification.ResourceQualificationChildren` or `await resourceQualification.ResourceQualificationChildren`)
//    - Simply reading `resourceQualification.ResourceQualificationChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await resourceQualification.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ResourceQualificationData {
    id!: bigint | number;
    resourceId!: bigint | number;
    qualificationId!: bigint | number;
    issueDate!: string | null;   // ISO 8601
    expiryDate!: string | null;   // ISO 8601
    issuer!: string | null;
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    qualification: QualificationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _resourceQualificationChangeHistories: ResourceQualificationChangeHistoryData[] | null = null;
    private _resourceQualificationChangeHistoriesPromise: Promise<ResourceQualificationChangeHistoryData[]> | null  = null;
    private _resourceQualificationChangeHistoriesSubject = new BehaviorSubject<ResourceQualificationChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ResourceQualificationData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ResourceQualificationData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ResourceQualificationData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ResourceQualificationChangeHistories$ = this._resourceQualificationChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resourceQualificationChangeHistories === null && this._resourceQualificationChangeHistoriesPromise === null) {
            this.loadResourceQualificationChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ResourceQualificationChangeHistoriesCount$ = ResourceQualificationChangeHistoryService.Instance.GetResourceQualificationChangeHistoriesRowCount({resourceQualificationId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ResourceQualificationData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.resourceQualification.Reload();
  //
  //  Non Async:
  //
  //     resourceQualification[0].Reload().then(x => {
  //        this.resourceQualification = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ResourceQualificationService.Instance.GetResourceQualification(this.id, includeRelations)
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
     this._resourceQualificationChangeHistories = null;
     this._resourceQualificationChangeHistoriesPromise = null;
     this._resourceQualificationChangeHistoriesSubject.next(null);

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
     * Gets the ResourceQualificationChangeHistories for this ResourceQualification.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resourceQualification.ResourceQualificationChangeHistories.then(resourceQualifications => { ... })
     *   or
     *   await this.resourceQualification.resourceQualifications
     *
    */
    public get ResourceQualificationChangeHistories(): Promise<ResourceQualificationChangeHistoryData[]> {
        if (this._resourceQualificationChangeHistories !== null) {
            return Promise.resolve(this._resourceQualificationChangeHistories);
        }

        if (this._resourceQualificationChangeHistoriesPromise !== null) {
            return this._resourceQualificationChangeHistoriesPromise;
        }

        // Start the load
        this.loadResourceQualificationChangeHistories();

        return this._resourceQualificationChangeHistoriesPromise!;
    }



    private loadResourceQualificationChangeHistories(): void {

        this._resourceQualificationChangeHistoriesPromise = lastValueFrom(
            ResourceQualificationService.Instance.GetResourceQualificationChangeHistoriesForResourceQualification(this.id)
        )
        .then(ResourceQualificationChangeHistories => {
            this._resourceQualificationChangeHistories = ResourceQualificationChangeHistories ?? [];
            this._resourceQualificationChangeHistoriesSubject.next(this._resourceQualificationChangeHistories);
            return this._resourceQualificationChangeHistories;
         })
        .catch(err => {
            this._resourceQualificationChangeHistories = [];
            this._resourceQualificationChangeHistoriesSubject.next(this._resourceQualificationChangeHistories);
            throw err;
        })
        .finally(() => {
            this._resourceQualificationChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ResourceQualificationChangeHistory. Call after mutations to force refresh.
     */
    public ClearResourceQualificationChangeHistoriesCache(): void {
        this._resourceQualificationChangeHistories = null;
        this._resourceQualificationChangeHistoriesPromise = null;
        this._resourceQualificationChangeHistoriesSubject.next(this._resourceQualificationChangeHistories);      // Emit to observable
    }

    public get HasResourceQualificationChangeHistories(): Promise<boolean> {
        return this.ResourceQualificationChangeHistories.then(resourceQualificationChangeHistories => resourceQualificationChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (resourceQualification.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await resourceQualification.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ResourceQualificationData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ResourceQualificationData>> {
        const info = await lastValueFrom(
            ResourceQualificationService.Instance.GetResourceQualificationChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ResourceQualificationData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ResourceQualificationData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ResourceQualificationSubmitData {
        return ResourceQualificationService.Instance.ConvertToResourceQualificationSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ResourceQualificationService extends SecureEndpointBase {

    private static _instance: ResourceQualificationService;
    private listCache: Map<string, Observable<Array<ResourceQualificationData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ResourceQualificationBasicListData>>>;
    private recordCache: Map<string, Observable<ResourceQualificationData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private resourceQualificationChangeHistoryService: ResourceQualificationChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ResourceQualificationData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ResourceQualificationBasicListData>>>();
        this.recordCache = new Map<string, Observable<ResourceQualificationData>>();

        ResourceQualificationService._instance = this;
    }

    public static get Instance(): ResourceQualificationService {
      return ResourceQualificationService._instance;
    }


    public ClearListCaches(config: ResourceQualificationQueryParameters | null = null) {

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


    public ConvertToResourceQualificationSubmitData(data: ResourceQualificationData): ResourceQualificationSubmitData {

        let output = new ResourceQualificationSubmitData();

        output.id = data.id;
        output.resourceId = data.resourceId;
        output.qualificationId = data.qualificationId;
        output.issueDate = data.issueDate;
        output.expiryDate = data.expiryDate;
        output.issuer = data.issuer;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetResourceQualification(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceQualificationData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const resourceQualification$ = this.requestResourceQualification(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceQualification", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, resourceQualification$);

            return resourceQualification$;
        }

        return this.recordCache.get(configHash) as Observable<ResourceQualificationData>;
    }

    private requestResourceQualification(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceQualificationData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceQualificationData>(this.baseUrl + 'api/ResourceQualification/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveResourceQualification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceQualification(id, includeRelations));
            }));
    }

    public GetResourceQualificationList(config: ResourceQualificationQueryParameters | any = null) : Observable<Array<ResourceQualificationData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const resourceQualificationList$ = this.requestResourceQualificationList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceQualification list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, resourceQualificationList$);

            return resourceQualificationList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ResourceQualificationData>>;
    }


    private requestResourceQualificationList(config: ResourceQualificationQueryParameters | any) : Observable <Array<ResourceQualificationData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceQualificationData>>(this.baseUrl + 'api/ResourceQualifications', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveResourceQualificationList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceQualificationList(config));
            }));
    }

    public GetResourceQualificationsRowCount(config: ResourceQualificationQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const resourceQualificationsRowCount$ = this.requestResourceQualificationsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceQualifications row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, resourceQualificationsRowCount$);

            return resourceQualificationsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestResourceQualificationsRowCount(config: ResourceQualificationQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ResourceQualifications/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceQualificationsRowCount(config));
            }));
    }

    public GetResourceQualificationsBasicListData(config: ResourceQualificationQueryParameters | any = null) : Observable<Array<ResourceQualificationBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const resourceQualificationsBasicListData$ = this.requestResourceQualificationsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceQualifications basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, resourceQualificationsBasicListData$);

            return resourceQualificationsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ResourceQualificationBasicListData>>;
    }


    private requestResourceQualificationsBasicListData(config: ResourceQualificationQueryParameters | any) : Observable<Array<ResourceQualificationBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceQualificationBasicListData>>(this.baseUrl + 'api/ResourceQualifications/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceQualificationsBasicListData(config));
            }));

    }


    public PutResourceQualification(id: bigint | number, resourceQualification: ResourceQualificationSubmitData) : Observable<ResourceQualificationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceQualificationData>(this.baseUrl + 'api/ResourceQualification/' + id.toString(), resourceQualification, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceQualification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutResourceQualification(id, resourceQualification));
            }));
    }


    public PostResourceQualification(resourceQualification: ResourceQualificationSubmitData) : Observable<ResourceQualificationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ResourceQualificationData>(this.baseUrl + 'api/ResourceQualification', resourceQualification, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceQualification(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostResourceQualification(resourceQualification));
            }));
    }

  
    public DeleteResourceQualification(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ResourceQualification/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteResourceQualification(id));
            }));
    }

    public RollbackResourceQualification(id: bigint | number, versionNumber: bigint | number) : Observable<ResourceQualificationData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceQualificationData>(this.baseUrl + 'api/ResourceQualification/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceQualification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackResourceQualification(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ResourceQualification.
     */
    public GetResourceQualificationChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ResourceQualificationData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ResourceQualificationData>>(this.baseUrl + 'api/ResourceQualification/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetResourceQualificationChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ResourceQualification.
     */
    public GetResourceQualificationAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ResourceQualificationData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ResourceQualificationData>[]>(this.baseUrl + 'api/ResourceQualification/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetResourceQualificationAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ResourceQualification.
     */
    public GetResourceQualificationVersion(id: bigint | number, version: number): Observable<ResourceQualificationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceQualificationData>(this.baseUrl + 'api/ResourceQualification/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveResourceQualification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetResourceQualificationVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ResourceQualification at a specific point in time.
     */
    public GetResourceQualificationStateAtTime(id: bigint | number, time: string): Observable<ResourceQualificationData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceQualificationData>(this.baseUrl + 'api/ResourceQualification/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveResourceQualification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetResourceQualificationStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ResourceQualificationQueryParameters | any): string {

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

    public userIsSchedulerResourceQualificationReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerResourceQualificationReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ResourceQualifications
        //
        if (userIsSchedulerResourceQualificationReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerResourceQualificationReader = user.readPermission >= 1;
            } else {
                userIsSchedulerResourceQualificationReader = false;
            }
        }

        return userIsSchedulerResourceQualificationReader;
    }


    public userIsSchedulerResourceQualificationWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerResourceQualificationWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ResourceQualifications
        //
        if (userIsSchedulerResourceQualificationWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerResourceQualificationWriter = user.writePermission >= 30;
          } else {
            userIsSchedulerResourceQualificationWriter = false;
          }      
        }

        return userIsSchedulerResourceQualificationWriter;
    }

    public GetResourceQualificationChangeHistoriesForResourceQualification(resourceQualificationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceQualificationChangeHistoryData[]> {
        return this.resourceQualificationChangeHistoryService.GetResourceQualificationChangeHistoryList({
            resourceQualificationId: resourceQualificationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ResourceQualificationData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ResourceQualificationData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ResourceQualificationTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveResourceQualification(raw: any): ResourceQualificationData {
    if (!raw) return raw;

    //
    // Create a ResourceQualificationData object instance with correct prototype
    //
    const revived = Object.create(ResourceQualificationData.prototype) as ResourceQualificationData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._resourceQualificationChangeHistories = null;
    (revived as any)._resourceQualificationChangeHistoriesPromise = null;
    (revived as any)._resourceQualificationChangeHistoriesSubject = new BehaviorSubject<ResourceQualificationChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadResourceQualificationXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ResourceQualificationChangeHistories$ = (revived as any)._resourceQualificationChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resourceQualificationChangeHistories === null && (revived as any)._resourceQualificationChangeHistoriesPromise === null) {
                (revived as any).loadResourceQualificationChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ResourceQualificationChangeHistoriesCount$ = ResourceQualificationChangeHistoryService.Instance.GetResourceQualificationChangeHistoriesRowCount({resourceQualificationId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ResourceQualificationData> | null>(null);

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

  private ReviveResourceQualificationList(rawList: any[]): ResourceQualificationData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveResourceQualification(raw));
  }

}
