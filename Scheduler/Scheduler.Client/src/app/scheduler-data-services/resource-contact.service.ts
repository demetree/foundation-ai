/*

   GENERATED SERVICE FOR THE RESOURCECONTACT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ResourceContact table.

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
import { ContactData } from './contact.service';
import { RelationshipTypeData } from './relationship-type.service';
import { ResourceContactChangeHistoryService, ResourceContactChangeHistoryData } from './resource-contact-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ResourceContactQueryParameters {
    resourceId: bigint | number | null | undefined = null;
    contactId: bigint | number | null | undefined = null;
    isPrimary: boolean | null | undefined = null;
    relationshipTypeId: bigint | number | null | undefined = null;
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
export class ResourceContactSubmitData {
    id!: bigint | number;
    resourceId!: bigint | number;
    contactId!: bigint | number;
    isPrimary!: boolean;
    relationshipTypeId!: bigint | number;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class ResourceContactBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ResourceContactChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `resourceContact.ResourceContactChildren$` — use with `| async` in templates
//        • Promise:    `resourceContact.ResourceContactChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="resourceContact.ResourceContactChildren$ | async"`), or
//        • Access the promise getter (`resourceContact.ResourceContactChildren` or `await resourceContact.ResourceContactChildren`)
//    - Simply reading `resourceContact.ResourceContactChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await resourceContact.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ResourceContactData {
    id!: bigint | number;
    resourceId!: bigint | number;
    contactId!: bigint | number;
    isPrimary!: boolean;
    relationshipTypeId!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    relationshipType: RelationshipTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _resourceContactChangeHistories: ResourceContactChangeHistoryData[] | null = null;
    private _resourceContactChangeHistoriesPromise: Promise<ResourceContactChangeHistoryData[]> | null  = null;
    private _resourceContactChangeHistoriesSubject = new BehaviorSubject<ResourceContactChangeHistoryData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ResourceContactChangeHistories$ = this._resourceContactChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resourceContactChangeHistories === null && this._resourceContactChangeHistoriesPromise === null) {
            this.loadResourceContactChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ResourceContactChangeHistoriesCount$ = ResourceContactService.Instance.GetResourceContactsRowCount({resourceContactId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ResourceContactData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.resourceContact.Reload();
  //
  //  Non Async:
  //
  //     resourceContact[0].Reload().then(x => {
  //        this.resourceContact = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ResourceContactService.Instance.GetResourceContact(this.id, includeRelations)
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
     this._resourceContactChangeHistories = null;
     this._resourceContactChangeHistoriesPromise = null;
     this._resourceContactChangeHistoriesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ResourceContactChangeHistories for this ResourceContact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.resourceContact.ResourceContactChangeHistories.then(resourceContactChangeHistories => { ... })
     *   or
     *   await this.resourceContact.ResourceContactChangeHistories
     *
    */
    public get ResourceContactChangeHistories(): Promise<ResourceContactChangeHistoryData[]> {
        if (this._resourceContactChangeHistories !== null) {
            return Promise.resolve(this._resourceContactChangeHistories);
        }

        if (this._resourceContactChangeHistoriesPromise !== null) {
            return this._resourceContactChangeHistoriesPromise;
        }

        // Start the load
        this.loadResourceContactChangeHistories();

        return this._resourceContactChangeHistoriesPromise!;
    }



    private loadResourceContactChangeHistories(): void {

        this._resourceContactChangeHistoriesPromise = lastValueFrom(
            ResourceContactService.Instance.GetResourceContactChangeHistoriesForResourceContact(this.id)
        )
        .then(resourceContactChangeHistories => {
            this._resourceContactChangeHistories = resourceContactChangeHistories ?? [];
            this._resourceContactChangeHistoriesSubject.next(this._resourceContactChangeHistories);
            return this._resourceContactChangeHistories;
         })
        .catch(err => {
            this._resourceContactChangeHistories = [];
            this._resourceContactChangeHistoriesSubject.next(this._resourceContactChangeHistories);
            throw err;
        })
        .finally(() => {
            this._resourceContactChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ResourceContactChangeHistory. Call after mutations to force refresh.
     */
    public ClearResourceContactChangeHistoriesCache(): void {
        this._resourceContactChangeHistories = null;
        this._resourceContactChangeHistoriesPromise = null;
        this._resourceContactChangeHistoriesSubject.next(this._resourceContactChangeHistories);      // Emit to observable
    }

    public get HasResourceContactChangeHistories(): Promise<boolean> {
        return this.ResourceContactChangeHistories.then(resourceContactChangeHistories => resourceContactChangeHistories.length > 0);
    }




    /**
     * Updates the state of this ResourceContactData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ResourceContactData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ResourceContactSubmitData {
        return ResourceContactService.Instance.ConvertToResourceContactSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ResourceContactService extends SecureEndpointBase {

    private static _instance: ResourceContactService;
    private listCache: Map<string, Observable<Array<ResourceContactData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ResourceContactBasicListData>>>;
    private recordCache: Map<string, Observable<ResourceContactData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private resourceContactChangeHistoryService: ResourceContactChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ResourceContactData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ResourceContactBasicListData>>>();
        this.recordCache = new Map<string, Observable<ResourceContactData>>();

        ResourceContactService._instance = this;
    }

    public static get Instance(): ResourceContactService {
      return ResourceContactService._instance;
    }


    public ClearListCaches(config: ResourceContactQueryParameters | null = null) {

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


    public ConvertToResourceContactSubmitData(data: ResourceContactData): ResourceContactSubmitData {

        let output = new ResourceContactSubmitData();

        output.id = data.id;
        output.resourceId = data.resourceId;
        output.contactId = data.contactId;
        output.isPrimary = data.isPrimary;
        output.relationshipTypeId = data.relationshipTypeId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetResourceContact(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceContactData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const resourceContact$ = this.requestResourceContact(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceContact", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, resourceContact$);

            return resourceContact$;
        }

        return this.recordCache.get(configHash) as Observable<ResourceContactData>;
    }

    private requestResourceContact(id: bigint | number, includeRelations: boolean = true) : Observable<ResourceContactData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ResourceContactData>(this.baseUrl + 'api/ResourceContact/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveResourceContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceContact(id, includeRelations));
            }));
    }

    public GetResourceContactList(config: ResourceContactQueryParameters | any = null) : Observable<Array<ResourceContactData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const resourceContactList$ = this.requestResourceContactList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceContact list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, resourceContactList$);

            return resourceContactList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ResourceContactData>>;
    }


    private requestResourceContactList(config: ResourceContactQueryParameters | any) : Observable <Array<ResourceContactData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceContactData>>(this.baseUrl + 'api/ResourceContacts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveResourceContactList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestResourceContactList(config));
            }));
    }

    public GetResourceContactsRowCount(config: ResourceContactQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const resourceContactsRowCount$ = this.requestResourceContactsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ResourceContacts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, resourceContactsRowCount$);

            return resourceContactsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestResourceContactsRowCount(config: ResourceContactQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ResourceContacts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceContactsRowCount(config));
            }));
    }

    public GetResourceContactsBasicListData(config: ResourceContactQueryParameters | any = null) : Observable<Array<ResourceContactBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const resourceContactsBasicListData$ = this.requestResourceContactsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ResourceContacts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, resourceContactsBasicListData$);

            return resourceContactsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ResourceContactBasicListData>>;
    }


    private requestResourceContactsBasicListData(config: ResourceContactQueryParameters | any) : Observable<Array<ResourceContactBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ResourceContactBasicListData>>(this.baseUrl + 'api/ResourceContacts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestResourceContactsBasicListData(config));
            }));

    }


    public PutResourceContact(id: bigint | number, resourceContact: ResourceContactSubmitData) : Observable<ResourceContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceContactData>(this.baseUrl + 'api/ResourceContact/' + id.toString(), resourceContact, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutResourceContact(id, resourceContact));
            }));
    }


    public PostResourceContact(resourceContact: ResourceContactSubmitData) : Observable<ResourceContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ResourceContactData>(this.baseUrl + 'api/ResourceContact', resourceContact, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceContact(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostResourceContact(resourceContact));
            }));
    }

  
    public DeleteResourceContact(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ResourceContact/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteResourceContact(id));
            }));
    }

    public RollbackResourceContact(id: bigint | number, versionNumber: bigint | number) : Observable<ResourceContactData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ResourceContactData>(this.baseUrl + 'api/ResourceContact/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveResourceContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackResourceContact(id, versionNumber));
        }));
    }

    private getConfigHash(config: ResourceContactQueryParameters | any): string {

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

    public userIsSchedulerResourceContactReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerResourceContactReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ResourceContacts
        //
        if (userIsSchedulerResourceContactReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerResourceContactReader = user.readPermission >= 1;
            } else {
                userIsSchedulerResourceContactReader = false;
            }
        }

        return userIsSchedulerResourceContactReader;
    }


    public userIsSchedulerResourceContactWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerResourceContactWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ResourceContacts
        //
        if (userIsSchedulerResourceContactWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerResourceContactWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerResourceContactWriter = false;
          }      
        }

        return userIsSchedulerResourceContactWriter;
    }

    public GetResourceContactChangeHistoriesForResourceContact(resourceContactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceContactChangeHistoryData[]> {
        return this.resourceContactChangeHistoryService.GetResourceContactChangeHistoryList({
            resourceContactId: resourceContactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ResourceContactData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ResourceContactData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ResourceContactTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveResourceContact(raw: any): ResourceContactData {
    if (!raw) return raw;

    //
    // Create a ResourceContactData object instance with correct prototype
    //
    const revived = Object.create(ResourceContactData.prototype) as ResourceContactData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._resourceContactChangeHistories = null;
    (revived as any)._resourceContactChangeHistoriesPromise = null;
    (revived as any)._resourceContactChangeHistoriesSubject = new BehaviorSubject<ResourceContactChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadResourceContactXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ResourceContactChangeHistories$ = (revived as any)._resourceContactChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resourceContactChangeHistories === null && (revived as any)._resourceContactChangeHistoriesPromise === null) {
                (revived as any).loadResourceContactChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ResourceContactChangeHistoriesCount$ = ResourceContactChangeHistoryService.Instance.GetResourceContactChangeHistoriesRowCount({resourceContactId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveResourceContactList(rawList: any[]): ResourceContactData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveResourceContact(raw));
  }

}
