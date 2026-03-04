/*

   GENERATED SERVICE FOR THE ATTRIBUTEDEFINITIONENTITY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AttributeDefinitionEntity table.

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
export class AttributeDefinitionEntityQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class AttributeDefinitionEntitySubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    active!: boolean;
    deleted!: boolean;
}


export class AttributeDefinitionEntityBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AttributeDefinitionEntityChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `attributeDefinitionEntity.AttributeDefinitionEntityChildren$` — use with `| async` in templates
//        • Promise:    `attributeDefinitionEntity.AttributeDefinitionEntityChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="attributeDefinitionEntity.AttributeDefinitionEntityChildren$ | async"`), or
//        • Access the promise getter (`attributeDefinitionEntity.AttributeDefinitionEntityChildren` or `await attributeDefinitionEntity.AttributeDefinitionEntityChildren`)
//    - Simply reading `attributeDefinitionEntity.AttributeDefinitionEntityChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await attributeDefinitionEntity.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AttributeDefinitionEntityData {
    id!: bigint | number;
    name!: string;
    description!: string;
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


    private _attributeDefinitionsCount$: Observable<bigint | number> | null = null;
    public get AttributeDefinitionsCount$(): Observable<bigint | number> {
        if (this._attributeDefinitionsCount$ === null) {
            this._attributeDefinitionsCount$ = AttributeDefinitionService.Instance.GetAttributeDefinitionsRowCount({attributeDefinitionEntityId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._attributeDefinitionsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AttributeDefinitionEntityData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.attributeDefinitionEntity.Reload();
  //
  //  Non Async:
  //
  //     attributeDefinitionEntity[0].Reload().then(x => {
  //        this.attributeDefinitionEntity = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AttributeDefinitionEntityService.Instance.GetAttributeDefinitionEntity(this.id, includeRelations)
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
     this._attributeDefinitionsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the AttributeDefinitions for this AttributeDefinitionEntity.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.attributeDefinitionEntity.AttributeDefinitions.then(attributeDefinitionEntities => { ... })
     *   or
     *   await this.attributeDefinitionEntity.attributeDefinitionEntities
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
            AttributeDefinitionEntityService.Instance.GetAttributeDefinitionsForAttributeDefinitionEntity(this.id)
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
     * Updates the state of this AttributeDefinitionEntityData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AttributeDefinitionEntityData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AttributeDefinitionEntitySubmitData {
        return AttributeDefinitionEntityService.Instance.ConvertToAttributeDefinitionEntitySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AttributeDefinitionEntityService extends SecureEndpointBase {

    private static _instance: AttributeDefinitionEntityService;
    private listCache: Map<string, Observable<Array<AttributeDefinitionEntityData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AttributeDefinitionEntityBasicListData>>>;
    private recordCache: Map<string, Observable<AttributeDefinitionEntityData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private attributeDefinitionService: AttributeDefinitionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AttributeDefinitionEntityData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AttributeDefinitionEntityBasicListData>>>();
        this.recordCache = new Map<string, Observable<AttributeDefinitionEntityData>>();

        AttributeDefinitionEntityService._instance = this;
    }

    public static get Instance(): AttributeDefinitionEntityService {
      return AttributeDefinitionEntityService._instance;
    }


    public ClearListCaches(config: AttributeDefinitionEntityQueryParameters | null = null) {

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


    public ConvertToAttributeDefinitionEntitySubmitData(data: AttributeDefinitionEntityData): AttributeDefinitionEntitySubmitData {

        let output = new AttributeDefinitionEntitySubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetAttributeDefinitionEntity(id: bigint | number, includeRelations: boolean = true) : Observable<AttributeDefinitionEntityData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const attributeDefinitionEntity$ = this.requestAttributeDefinitionEntity(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AttributeDefinitionEntity", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, attributeDefinitionEntity$);

            return attributeDefinitionEntity$;
        }

        return this.recordCache.get(configHash) as Observable<AttributeDefinitionEntityData>;
    }

    private requestAttributeDefinitionEntity(id: bigint | number, includeRelations: boolean = true) : Observable<AttributeDefinitionEntityData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AttributeDefinitionEntityData>(this.baseUrl + 'api/AttributeDefinitionEntity/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAttributeDefinitionEntity(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAttributeDefinitionEntity(id, includeRelations));
            }));
    }

    public GetAttributeDefinitionEntityList(config: AttributeDefinitionEntityQueryParameters | any = null) : Observable<Array<AttributeDefinitionEntityData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const attributeDefinitionEntityList$ = this.requestAttributeDefinitionEntityList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AttributeDefinitionEntity list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, attributeDefinitionEntityList$);

            return attributeDefinitionEntityList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AttributeDefinitionEntityData>>;
    }


    private requestAttributeDefinitionEntityList(config: AttributeDefinitionEntityQueryParameters | any) : Observable <Array<AttributeDefinitionEntityData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AttributeDefinitionEntityData>>(this.baseUrl + 'api/AttributeDefinitionEntities', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAttributeDefinitionEntityList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAttributeDefinitionEntityList(config));
            }));
    }

    public GetAttributeDefinitionEntitiesRowCount(config: AttributeDefinitionEntityQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const attributeDefinitionEntitiesRowCount$ = this.requestAttributeDefinitionEntitiesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AttributeDefinitionEntities row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, attributeDefinitionEntitiesRowCount$);

            return attributeDefinitionEntitiesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAttributeDefinitionEntitiesRowCount(config: AttributeDefinitionEntityQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AttributeDefinitionEntities/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAttributeDefinitionEntitiesRowCount(config));
            }));
    }

    public GetAttributeDefinitionEntitiesBasicListData(config: AttributeDefinitionEntityQueryParameters | any = null) : Observable<Array<AttributeDefinitionEntityBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const attributeDefinitionEntitiesBasicListData$ = this.requestAttributeDefinitionEntitiesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AttributeDefinitionEntities basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, attributeDefinitionEntitiesBasicListData$);

            return attributeDefinitionEntitiesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AttributeDefinitionEntityBasicListData>>;
    }


    private requestAttributeDefinitionEntitiesBasicListData(config: AttributeDefinitionEntityQueryParameters | any) : Observable<Array<AttributeDefinitionEntityBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AttributeDefinitionEntityBasicListData>>(this.baseUrl + 'api/AttributeDefinitionEntities/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAttributeDefinitionEntitiesBasicListData(config));
            }));

    }


    public PutAttributeDefinitionEntity(id: bigint | number, attributeDefinitionEntity: AttributeDefinitionEntitySubmitData) : Observable<AttributeDefinitionEntityData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AttributeDefinitionEntityData>(this.baseUrl + 'api/AttributeDefinitionEntity/' + id.toString(), attributeDefinitionEntity, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAttributeDefinitionEntity(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAttributeDefinitionEntity(id, attributeDefinitionEntity));
            }));
    }


    public PostAttributeDefinitionEntity(attributeDefinitionEntity: AttributeDefinitionEntitySubmitData) : Observable<AttributeDefinitionEntityData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AttributeDefinitionEntityData>(this.baseUrl + 'api/AttributeDefinitionEntity', attributeDefinitionEntity, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAttributeDefinitionEntity(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAttributeDefinitionEntity(attributeDefinitionEntity));
            }));
    }

  
    public DeleteAttributeDefinitionEntity(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AttributeDefinitionEntity/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAttributeDefinitionEntity(id));
            }));
    }


    private getConfigHash(config: AttributeDefinitionEntityQueryParameters | any): string {

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

    public userIsSchedulerAttributeDefinitionEntityReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerAttributeDefinitionEntityReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.AttributeDefinitionEntities
        //
        if (userIsSchedulerAttributeDefinitionEntityReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerAttributeDefinitionEntityReader = user.readPermission >= 1;
            } else {
                userIsSchedulerAttributeDefinitionEntityReader = false;
            }
        }

        return userIsSchedulerAttributeDefinitionEntityReader;
    }


    public userIsSchedulerAttributeDefinitionEntityWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerAttributeDefinitionEntityWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.AttributeDefinitionEntities
        //
        if (userIsSchedulerAttributeDefinitionEntityWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerAttributeDefinitionEntityWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerAttributeDefinitionEntityWriter = false;
          }      
        }

        return userIsSchedulerAttributeDefinitionEntityWriter;
    }

    public GetAttributeDefinitionsForAttributeDefinitionEntity(attributeDefinitionEntityId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AttributeDefinitionData[]> {
        return this.attributeDefinitionService.GetAttributeDefinitionList({
            attributeDefinitionEntityId: attributeDefinitionEntityId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AttributeDefinitionEntityData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AttributeDefinitionEntityData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AttributeDefinitionEntityTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAttributeDefinitionEntity(raw: any): AttributeDefinitionEntityData {
    if (!raw) return raw;

    //
    // Create a AttributeDefinitionEntityData object instance with correct prototype
    //
    const revived = Object.create(AttributeDefinitionEntityData.prototype) as AttributeDefinitionEntityData;

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
    // 2. But private methods (loadAttributeDefinitionEntityXYZ, etc.) are not accessible via the typed variable
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

    (revived as any)._attributeDefinitionsCount$ = null;



    return revived;
  }

  private ReviveAttributeDefinitionEntityList(rawList: any[]): AttributeDefinitionEntityData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAttributeDefinitionEntity(raw));
  }

}
