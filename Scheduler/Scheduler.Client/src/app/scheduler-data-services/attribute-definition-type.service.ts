/*

   GENERATED SERVICE FOR THE ATTRIBUTEDEFINITIONTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AttributeDefinitionType table.

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
import { AttributeDefinitionService, AttributeDefinitionData } from './attribute-definition.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AttributeDefinitionTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class AttributeDefinitionTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class AttributeDefinitionTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AttributeDefinitionTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `attributeDefinitionType.AttributeDefinitionTypeChildren$` — use with `| async` in templates
//        • Promise:    `attributeDefinitionType.AttributeDefinitionTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="attributeDefinitionType.AttributeDefinitionTypeChildren$ | async"`), or
//        • Access the promise getter (`attributeDefinitionType.AttributeDefinitionTypeChildren` or `await attributeDefinitionType.AttributeDefinitionTypeChildren`)
//    - Simply reading `attributeDefinitionType.AttributeDefinitionTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await attributeDefinitionType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AttributeDefinitionTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _attributeDefinitions: AttributeDefinitionData[] | null = null;
    private _attributeDefinitionsPromise: Promise<AttributeDefinitionData[]> | null  = null;
    private _attributeDefinitionsSubject = new BehaviorSubject<AttributeDefinitionData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public AttributeDefinitions$ = this._attributeDefinitionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._attributeDefinitions === null && this._attributeDefinitionsPromise === null) {
            this.loadAttributeDefinitions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public AttributeDefinitionsCount$ = AttributeDefinitionService.Instance.GetAttributeDefinitionsRowCount({attributeDefinitionTypeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AttributeDefinitionTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.attributeDefinitionType.Reload();
  //
  //  Non Async:
  //
  //     attributeDefinitionType[0].Reload().then(x => {
  //        this.attributeDefinitionType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AttributeDefinitionTypeService.Instance.GetAttributeDefinitionType(this.id, includeRelations)
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
     this._attributeDefinitions = null;
     this._attributeDefinitionsPromise = null;
     this._attributeDefinitionsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the AttributeDefinitions for this AttributeDefinitionType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.attributeDefinitionType.AttributeDefinitions.then(attributeDefinitionTypes => { ... })
     *   or
     *   await this.attributeDefinitionType.attributeDefinitionTypes
     *
    */
    public get AttributeDefinitions(): Promise<AttributeDefinitionData[]> {
        if (this._attributeDefinitions !== null) {
            return Promise.resolve(this._attributeDefinitions);
        }

        if (this._attributeDefinitionsPromise !== null) {
            return this._attributeDefinitionsPromise;
        }

        // Start the load
        this.loadAttributeDefinitions();

        return this._attributeDefinitionsPromise!;
    }



    private loadAttributeDefinitions(): void {

        this._attributeDefinitionsPromise = lastValueFrom(
            AttributeDefinitionTypeService.Instance.GetAttributeDefinitionsForAttributeDefinitionType(this.id)
        )
        .then(AttributeDefinitions => {
            this._attributeDefinitions = AttributeDefinitions ?? [];
            this._attributeDefinitionsSubject.next(this._attributeDefinitions);
            return this._attributeDefinitions;
         })
        .catch(err => {
            this._attributeDefinitions = [];
            this._attributeDefinitionsSubject.next(this._attributeDefinitions);
            throw err;
        })
        .finally(() => {
            this._attributeDefinitionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached AttributeDefinition. Call after mutations to force refresh.
     */
    public ClearAttributeDefinitionsCache(): void {
        this._attributeDefinitions = null;
        this._attributeDefinitionsPromise = null;
        this._attributeDefinitionsSubject.next(this._attributeDefinitions);      // Emit to observable
    }

    public get HasAttributeDefinitions(): Promise<boolean> {
        return this.AttributeDefinitions.then(attributeDefinitions => attributeDefinitions.length > 0);
    }




    /**
     * Updates the state of this AttributeDefinitionTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AttributeDefinitionTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AttributeDefinitionTypeSubmitData {
        return AttributeDefinitionTypeService.Instance.ConvertToAttributeDefinitionTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AttributeDefinitionTypeService extends SecureEndpointBase {

    private static _instance: AttributeDefinitionTypeService;
    private listCache: Map<string, Observable<Array<AttributeDefinitionTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AttributeDefinitionTypeBasicListData>>>;
    private recordCache: Map<string, Observable<AttributeDefinitionTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private attributeDefinitionService: AttributeDefinitionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AttributeDefinitionTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AttributeDefinitionTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<AttributeDefinitionTypeData>>();

        AttributeDefinitionTypeService._instance = this;
    }

    public static get Instance(): AttributeDefinitionTypeService {
      return AttributeDefinitionTypeService._instance;
    }


    public ClearListCaches(config: AttributeDefinitionTypeQueryParameters | null = null) {

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


    public ConvertToAttributeDefinitionTypeSubmitData(data: AttributeDefinitionTypeData): AttributeDefinitionTypeSubmitData {

        let output = new AttributeDefinitionTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetAttributeDefinitionType(id: bigint | number, includeRelations: boolean = true) : Observable<AttributeDefinitionTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const attributeDefinitionType$ = this.requestAttributeDefinitionType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AttributeDefinitionType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, attributeDefinitionType$);

            return attributeDefinitionType$;
        }

        return this.recordCache.get(configHash) as Observable<AttributeDefinitionTypeData>;
    }

    private requestAttributeDefinitionType(id: bigint | number, includeRelations: boolean = true) : Observable<AttributeDefinitionTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AttributeDefinitionTypeData>(this.baseUrl + 'api/AttributeDefinitionType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAttributeDefinitionType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAttributeDefinitionType(id, includeRelations));
            }));
    }

    public GetAttributeDefinitionTypeList(config: AttributeDefinitionTypeQueryParameters | any = null) : Observable<Array<AttributeDefinitionTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const attributeDefinitionTypeList$ = this.requestAttributeDefinitionTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AttributeDefinitionType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, attributeDefinitionTypeList$);

            return attributeDefinitionTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AttributeDefinitionTypeData>>;
    }


    private requestAttributeDefinitionTypeList(config: AttributeDefinitionTypeQueryParameters | any) : Observable <Array<AttributeDefinitionTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AttributeDefinitionTypeData>>(this.baseUrl + 'api/AttributeDefinitionTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAttributeDefinitionTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAttributeDefinitionTypeList(config));
            }));
    }

    public GetAttributeDefinitionTypesRowCount(config: AttributeDefinitionTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const attributeDefinitionTypesRowCount$ = this.requestAttributeDefinitionTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AttributeDefinitionTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, attributeDefinitionTypesRowCount$);

            return attributeDefinitionTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAttributeDefinitionTypesRowCount(config: AttributeDefinitionTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AttributeDefinitionTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAttributeDefinitionTypesRowCount(config));
            }));
    }

    public GetAttributeDefinitionTypesBasicListData(config: AttributeDefinitionTypeQueryParameters | any = null) : Observable<Array<AttributeDefinitionTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const attributeDefinitionTypesBasicListData$ = this.requestAttributeDefinitionTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AttributeDefinitionTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, attributeDefinitionTypesBasicListData$);

            return attributeDefinitionTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AttributeDefinitionTypeBasicListData>>;
    }


    private requestAttributeDefinitionTypesBasicListData(config: AttributeDefinitionTypeQueryParameters | any) : Observable<Array<AttributeDefinitionTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AttributeDefinitionTypeBasicListData>>(this.baseUrl + 'api/AttributeDefinitionTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAttributeDefinitionTypesBasicListData(config));
            }));

    }


    public PutAttributeDefinitionType(id: bigint | number, attributeDefinitionType: AttributeDefinitionTypeSubmitData) : Observable<AttributeDefinitionTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AttributeDefinitionTypeData>(this.baseUrl + 'api/AttributeDefinitionType/' + id.toString(), attributeDefinitionType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAttributeDefinitionType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAttributeDefinitionType(id, attributeDefinitionType));
            }));
    }


    public PostAttributeDefinitionType(attributeDefinitionType: AttributeDefinitionTypeSubmitData) : Observable<AttributeDefinitionTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AttributeDefinitionTypeData>(this.baseUrl + 'api/AttributeDefinitionType', attributeDefinitionType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAttributeDefinitionType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAttributeDefinitionType(attributeDefinitionType));
            }));
    }

  
    public DeleteAttributeDefinitionType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AttributeDefinitionType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAttributeDefinitionType(id));
            }));
    }


    private getConfigHash(config: AttributeDefinitionTypeQueryParameters | any): string {

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

    public userIsSchedulerAttributeDefinitionTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerAttributeDefinitionTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.AttributeDefinitionTypes
        //
        if (userIsSchedulerAttributeDefinitionTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerAttributeDefinitionTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerAttributeDefinitionTypeReader = false;
            }
        }

        return userIsSchedulerAttributeDefinitionTypeReader;
    }


    public userIsSchedulerAttributeDefinitionTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerAttributeDefinitionTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.AttributeDefinitionTypes
        //
        if (userIsSchedulerAttributeDefinitionTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerAttributeDefinitionTypeWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerAttributeDefinitionTypeWriter = false;
          }      
        }

        return userIsSchedulerAttributeDefinitionTypeWriter;
    }

    public GetAttributeDefinitionsForAttributeDefinitionType(attributeDefinitionTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AttributeDefinitionData[]> {
        return this.attributeDefinitionService.GetAttributeDefinitionList({
            attributeDefinitionTypeId: attributeDefinitionTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AttributeDefinitionTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AttributeDefinitionTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AttributeDefinitionTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAttributeDefinitionType(raw: any): AttributeDefinitionTypeData {
    if (!raw) return raw;

    //
    // Create a AttributeDefinitionTypeData object instance with correct prototype
    //
    const revived = Object.create(AttributeDefinitionTypeData.prototype) as AttributeDefinitionTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._attributeDefinitions = null;
    (revived as any)._attributeDefinitionsPromise = null;
    (revived as any)._attributeDefinitionsSubject = new BehaviorSubject<AttributeDefinitionData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAttributeDefinitionTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).AttributeDefinitions$ = (revived as any)._attributeDefinitionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._attributeDefinitions === null && (revived as any)._attributeDefinitionsPromise === null) {
                (revived as any).loadAttributeDefinitions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).AttributeDefinitionsCount$ = AttributeDefinitionService.Instance.GetAttributeDefinitionsRowCount({attributeDefinitionTypeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveAttributeDefinitionTypeList(rawList: any[]): AttributeDefinitionTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAttributeDefinitionType(raw));
  }

}
