/*

   GENERATED SERVICE FOR THE RESOURCETYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ResourceType table.

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
import { IconData } from './icon.service';
import { ResourceService, ResourceData } from './resource.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ResourceTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    isBillable: boolean | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class ResourceTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isBillable: boolean | null = null;
    sequence: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ResourceTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ResourceTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `resourceType.ResourceTypeChildren$` — use with `| async` in templates
//        • Promise:    `resourceType.ResourceTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="resourceType.ResourceTypeChildren$ | async"`), or
//        • Access the promise getter (`resourceType.ResourceTypeChildren` or `await resourceType.ResourceTypeChildren`)
//    - Simply reading `resourceType.ResourceTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await resourceType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ResourceTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isBillable!: boolean | null;
    sequence!: bigint | number;
    iconId!: bigint | number;
    color!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _resources: ResourceData[] | null = null;
    private _resourcesPromise: Promise<ResourceData[]> | null  = null;
    private _resourcesSubject = new BehaviorSubject<ResourceData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Resources$ = this._resourcesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resources === null && this._resourcesPromise === null) {
            this.loadResources(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _resourcesCount$: Observable<bigint | number> | null = null;
    public get ResourcesCount$(): Observable<bigint | number> {
        if (this._resourcesCount$ === null) {
            this._resourcesCount$ = ResourceService.Instance.GetResourcesRowCount({resourceTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._resourcesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ResourceTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.resourceType.Reload();
  //
  //  Non Async:
  //
  //     resourceType[0].Reload().then(x => {
  //        this.resourceType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ResourceTypeService.Instance.GetResourceType(this.id, includeRelations)
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
     this._resources = null;
     this._resourcesPromise = null;
     this._resourcesSubject.next(null);
     this._resourcesCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Resources for this ResourceType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resourceType.Resources.then(resourceTypes => { ... })
     *   or
     *   await this.resourceType.resourceTypes
     *
    */
    public get Resources(): Promise<ResourceData[]> {
        if (this._resources !== null) {
            return Promise.resolve(this._resources);
        }

        if (this._resourcesPromise !== null) {
            return this._resourcesPromise;
        }

        // Start the load
        this.loadResources();

        return this._resourcesPromise!;
    }



    private loadResources(): void {

        this._resourcesPromise = lastValueFrom(
            ResourceTypeService.Instance.GetResourcesForResourceType(this.id)
        )
        .then(Resources => {
            this._resources = Resources ?? [];
            this._resourcesSubject.next(this._resources);
            return this._resources;
         })
        .catch(err => {
            this._resources = [];
            this._resourcesSubject.next(this._resources);
            throw err;
        })
        .finally(() => {
            this._resourcesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Resource. Call after mutations to force refresh.
     */
    public ClearResourcesCache(): void {
        this._resources = null;
        this._resourcesPromise = null;
        this._resourcesSubject.next(this._resources);      // Emit to observable
    }

    public get HasResources(): Promise<boolean> {
        return this.Resources.then(resources => resources.length > 0);
    }




    /**
     * Updates the state of this ResourceTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ResourceTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ResourceTypeSubmitData {
        return ResourceTypeService.Instance.ConvertToResourceTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ResourceTypeService extends SecureEndpointBase {

    private static _instance: ResourceTypeService;
    private listCache: Map<string, Observable<Array<ResourceTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ResourceTypeBasicListData>>>;
    private recordCache: Map<string, Observable<ResourceTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private resourceService: ResourceService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ResourceTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ResourceTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<ResourceTypeData>>();

        ResourceTypeService._instance = this;
    }

    public static get Instance(): ResourceTypeService {
      return ResourceTypeService._instance;
    }


    public ClearListCaches(config: ResourceTypeQueryParameters | null = null) {

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


    public ConvertToResourceTypeSubmitData(data: ResourceTypeData): ResourceTypeSubmitData {

        let output = new ResourceTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.isBillable = data.isBillable;
        output.sequence = data.sequence;
        output.iconId = data.iconId;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetResourceType(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const resourceType$ = this.requestResourceType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, resourceType$);

            return resourceType$;
        }

        return this.recordCache.get(configHash) as Observable<ResourceTypeData>;
    }

    private requestResourceType(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceTypeData>(this.baseUrl + 'api/ResourceType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveResourceType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceType(id, includeRelations));
            }));
    }

    public GetResourceTypeList(config: ResourceTypeQueryParameters | any = null) : Observable<Array<ResourceTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const resourceTypeList$ = this.requestResourceTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, resourceTypeList$);

            return resourceTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ResourceTypeData>>;
    }


    private requestResourceTypeList(config: ResourceTypeQueryParameters | any) : Observable <Array<ResourceTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceTypeData>>(this.baseUrl + 'api/ResourceTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveResourceTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceTypeList(config));
            }));
    }

    public GetResourceTypesRowCount(config: ResourceTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const resourceTypesRowCount$ = this.requestResourceTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, resourceTypesRowCount$);

            return resourceTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestResourceTypesRowCount(config: ResourceTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ResourceTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceTypesRowCount(config));
            }));
    }

    public GetResourceTypesBasicListData(config: ResourceTypeQueryParameters | any = null) : Observable<Array<ResourceTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const resourceTypesBasicListData$ = this.requestResourceTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, resourceTypesBasicListData$);

            return resourceTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ResourceTypeBasicListData>>;
    }


    private requestResourceTypesBasicListData(config: ResourceTypeQueryParameters | any) : Observable<Array<ResourceTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceTypeBasicListData>>(this.baseUrl + 'api/ResourceTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceTypesBasicListData(config));
            }));

    }


    public PutResourceType(id: bigint | number, resourceType: ResourceTypeSubmitData) : Observable<ResourceTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceTypeData>(this.baseUrl + 'api/ResourceType/' + id.toString(), resourceType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutResourceType(id, resourceType));
            }));
    }


    public PostResourceType(resourceType: ResourceTypeSubmitData) : Observable<ResourceTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ResourceTypeData>(this.baseUrl + 'api/ResourceType', resourceType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostResourceType(resourceType));
            }));
    }

  
    public DeleteResourceType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ResourceType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteResourceType(id));
            }));
    }


    private getConfigHash(config: ResourceTypeQueryParameters | any): string {

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

    public userIsSchedulerResourceTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerResourceTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ResourceTypes
        //
        if (userIsSchedulerResourceTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerResourceTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerResourceTypeReader = false;
            }
        }

        return userIsSchedulerResourceTypeReader;
    }


    public userIsSchedulerResourceTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerResourceTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ResourceTypes
        //
        if (userIsSchedulerResourceTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerResourceTypeWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerResourceTypeWriter = false;
          }      
        }

        return userIsSchedulerResourceTypeWriter;
    }

    public GetResourcesForResourceType(resourceTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceData[]> {
        return this.resourceService.GetResourceList({
            resourceTypeId: resourceTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ResourceTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ResourceTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ResourceTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveResourceType(raw: any): ResourceTypeData {
    if (!raw) return raw;

    //
    // Create a ResourceTypeData object instance with correct prototype
    //
    const revived = Object.create(ResourceTypeData.prototype) as ResourceTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._resources = null;
    (revived as any)._resourcesPromise = null;
    (revived as any)._resourcesSubject = new BehaviorSubject<ResourceData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadResourceTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Resources$ = (revived as any)._resourcesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resources === null && (revived as any)._resourcesPromise === null) {
                (revived as any).loadResources();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._resourcesCount$ = null;



    return revived;
  }

  private ReviveResourceTypeList(rawList: any[]): ResourceTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveResourceType(raw));
  }

}
