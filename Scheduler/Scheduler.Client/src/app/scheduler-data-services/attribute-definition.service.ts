/*

   GENERATED SERVICE FOR THE ATTRIBUTEDEFINITION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AttributeDefinition table.

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
import { AttributeDefinitionEntityData } from './attribute-definition-entity.service';
import { AttributeDefinitionTypeData } from './attribute-definition-type.service';
import { AttributeDefinitionChangeHistoryService, AttributeDefinitionChangeHistoryData } from './attribute-definition-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AttributeDefinitionQueryParameters {
    attributeDefinitionEntityId: bigint | number | null | undefined = null;
    key: string | null | undefined = null;
    label: string | null | undefined = null;
    attributeDefinitionTypeId: bigint | number | null | undefined = null;
    options: string | null | undefined = null;
    isRequired: boolean | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class AttributeDefinitionSubmitData {
    id!: bigint | number;
    attributeDefinitionEntityId: bigint | number | null = null;
    key: string | null = null;
    label: string | null = null;
    attributeDefinitionTypeId: bigint | number | null = null;
    options: string | null = null;
    isRequired!: boolean;
    sequence: bigint | number | null = null;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class AttributeDefinitionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AttributeDefinitionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `attributeDefinition.AttributeDefinitionChildren$` — use with `| async` in templates
//        • Promise:    `attributeDefinition.AttributeDefinitionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="attributeDefinition.AttributeDefinitionChildren$ | async"`), or
//        • Access the promise getter (`attributeDefinition.AttributeDefinitionChildren` or `await attributeDefinition.AttributeDefinitionChildren`)
//    - Simply reading `attributeDefinition.AttributeDefinitionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await attributeDefinition.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AttributeDefinitionData {
    id!: bigint | number;
    attributeDefinitionEntityId!: bigint | number;
    key!: string | null;
    label!: string | null;
    attributeDefinitionTypeId!: bigint | number;
    options!: string | null;
    isRequired!: boolean;
    sequence!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    attributeDefinitionEntity: AttributeDefinitionEntityData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    attributeDefinitionType: AttributeDefinitionTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _attributeDefinitionChangeHistories: AttributeDefinitionChangeHistoryData[] | null = null;
    private _attributeDefinitionChangeHistoriesPromise: Promise<AttributeDefinitionChangeHistoryData[]> | null  = null;
    private _attributeDefinitionChangeHistoriesSubject = new BehaviorSubject<AttributeDefinitionChangeHistoryData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public AttributeDefinitionChangeHistories$ = this._attributeDefinitionChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._attributeDefinitionChangeHistories === null && this._attributeDefinitionChangeHistoriesPromise === null) {
            this.loadAttributeDefinitionChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public AttributeDefinitionChangeHistoriesCount$ = AttributeDefinitionService.Instance.GetAttributeDefinitionsRowCount({attributeDefinitionId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AttributeDefinitionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.attributeDefinition.Reload();
  //
  //  Non Async:
  //
  //     attributeDefinition[0].Reload().then(x => {
  //        this.attributeDefinition = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AttributeDefinitionService.Instance.GetAttributeDefinition(this.id, includeRelations)
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
     this._attributeDefinitionChangeHistories = null;
     this._attributeDefinitionChangeHistoriesPromise = null;
     this._attributeDefinitionChangeHistoriesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the AttributeDefinitionChangeHistories for this AttributeDefinition.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.attributeDefinition.AttributeDefinitionChangeHistories.then(attributeDefinitionChangeHistories => { ... })
     *   or
     *   await this.attributeDefinition.AttributeDefinitionChangeHistories
     *
    */
    public get AttributeDefinitionChangeHistories(): Promise<AttributeDefinitionChangeHistoryData[]> {
        if (this._attributeDefinitionChangeHistories !== null) {
            return Promise.resolve(this._attributeDefinitionChangeHistories);
        }

        if (this._attributeDefinitionChangeHistoriesPromise !== null) {
            return this._attributeDefinitionChangeHistoriesPromise;
        }

        // Start the load
        this.loadAttributeDefinitionChangeHistories();

        return this._attributeDefinitionChangeHistoriesPromise!;
    }



    private loadAttributeDefinitionChangeHistories(): void {

        this._attributeDefinitionChangeHistoriesPromise = lastValueFrom(
            AttributeDefinitionService.Instance.GetAttributeDefinitionChangeHistoriesForAttributeDefinition(this.id)
        )
        .then(attributeDefinitionChangeHistories => {
            this._attributeDefinitionChangeHistories = attributeDefinitionChangeHistories ?? [];
            this._attributeDefinitionChangeHistoriesSubject.next(this._attributeDefinitionChangeHistories);
            return this._attributeDefinitionChangeHistories;
         })
        .catch(err => {
            this._attributeDefinitionChangeHistories = [];
            this._attributeDefinitionChangeHistoriesSubject.next(this._attributeDefinitionChangeHistories);
            throw err;
        })
        .finally(() => {
            this._attributeDefinitionChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached AttributeDefinitionChangeHistory. Call after mutations to force refresh.
     */
    public ClearAttributeDefinitionChangeHistoriesCache(): void {
        this._attributeDefinitionChangeHistories = null;
        this._attributeDefinitionChangeHistoriesPromise = null;
        this._attributeDefinitionChangeHistoriesSubject.next(this._attributeDefinitionChangeHistories);      // Emit to observable
    }

    public get HasAttributeDefinitionChangeHistories(): Promise<boolean> {
        return this.AttributeDefinitionChangeHistories.then(attributeDefinitionChangeHistories => attributeDefinitionChangeHistories.length > 0);
    }




    /**
     * Updates the state of this AttributeDefinitionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AttributeDefinitionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AttributeDefinitionSubmitData {
        return AttributeDefinitionService.Instance.ConvertToAttributeDefinitionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AttributeDefinitionService extends SecureEndpointBase {

    private static _instance: AttributeDefinitionService;
    private listCache: Map<string, Observable<Array<AttributeDefinitionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AttributeDefinitionBasicListData>>>;
    private recordCache: Map<string, Observable<AttributeDefinitionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private attributeDefinitionChangeHistoryService: AttributeDefinitionChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AttributeDefinitionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AttributeDefinitionBasicListData>>>();
        this.recordCache = new Map<string, Observable<AttributeDefinitionData>>();

        AttributeDefinitionService._instance = this;
    }

    public static get Instance(): AttributeDefinitionService {
      return AttributeDefinitionService._instance;
    }


    public ClearListCaches(config: AttributeDefinitionQueryParameters | null = null) {

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


    public ConvertToAttributeDefinitionSubmitData(data: AttributeDefinitionData): AttributeDefinitionSubmitData {

        let output = new AttributeDefinitionSubmitData();

        output.id = data.id;
        output.attributeDefinitionEntityId = data.attributeDefinitionEntityId;
        output.key = data.key;
        output.label = data.label;
        output.attributeDefinitionTypeId = data.attributeDefinitionTypeId;
        output.options = data.options;
        output.isRequired = data.isRequired;
        output.sequence = data.sequence;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetAttributeDefinition(id: bigint | number, includeRelations: boolean = true) : Observable<AttributeDefinitionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const attributeDefinition$ = this.requestAttributeDefinition(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AttributeDefinition", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, attributeDefinition$);

            return attributeDefinition$;
        }

        return this.recordCache.get(configHash) as Observable<AttributeDefinitionData>;
    }

    private requestAttributeDefinition(id: bigint | number, includeRelations: boolean = true) : Observable<AttributeDefinitionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AttributeDefinitionData>(this.baseUrl + 'api/AttributeDefinition/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAttributeDefinition(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAttributeDefinition(id, includeRelations));
            }));
    }

    public GetAttributeDefinitionList(config: AttributeDefinitionQueryParameters | any = null) : Observable<Array<AttributeDefinitionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const attributeDefinitionList$ = this.requestAttributeDefinitionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AttributeDefinition list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, attributeDefinitionList$);

            return attributeDefinitionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AttributeDefinitionData>>;
    }


    private requestAttributeDefinitionList(config: AttributeDefinitionQueryParameters | any) : Observable <Array<AttributeDefinitionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AttributeDefinitionData>>(this.baseUrl + 'api/AttributeDefinitions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAttributeDefinitionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAttributeDefinitionList(config));
            }));
    }

    public GetAttributeDefinitionsRowCount(config: AttributeDefinitionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const attributeDefinitionsRowCount$ = this.requestAttributeDefinitionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AttributeDefinitions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, attributeDefinitionsRowCount$);

            return attributeDefinitionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAttributeDefinitionsRowCount(config: AttributeDefinitionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AttributeDefinitions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAttributeDefinitionsRowCount(config));
            }));
    }

    public GetAttributeDefinitionsBasicListData(config: AttributeDefinitionQueryParameters | any = null) : Observable<Array<AttributeDefinitionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const attributeDefinitionsBasicListData$ = this.requestAttributeDefinitionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AttributeDefinitions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, attributeDefinitionsBasicListData$);

            return attributeDefinitionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AttributeDefinitionBasicListData>>;
    }


    private requestAttributeDefinitionsBasicListData(config: AttributeDefinitionQueryParameters | any) : Observable<Array<AttributeDefinitionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AttributeDefinitionBasicListData>>(this.baseUrl + 'api/AttributeDefinitions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAttributeDefinitionsBasicListData(config));
            }));

    }


    public PutAttributeDefinition(id: bigint | number, attributeDefinition: AttributeDefinitionSubmitData) : Observable<AttributeDefinitionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AttributeDefinitionData>(this.baseUrl + 'api/AttributeDefinition/' + id.toString(), attributeDefinition, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAttributeDefinition(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAttributeDefinition(id, attributeDefinition));
            }));
    }


    public PostAttributeDefinition(attributeDefinition: AttributeDefinitionSubmitData) : Observable<AttributeDefinitionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AttributeDefinitionData>(this.baseUrl + 'api/AttributeDefinition', attributeDefinition, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAttributeDefinition(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAttributeDefinition(attributeDefinition));
            }));
    }

  
    public DeleteAttributeDefinition(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AttributeDefinition/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAttributeDefinition(id));
            }));
    }

    public RollbackAttributeDefinition(id: bigint | number, versionNumber: bigint | number) : Observable<AttributeDefinitionData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AttributeDefinitionData>(this.baseUrl + 'api/AttributeDefinition/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAttributeDefinition(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackAttributeDefinition(id, versionNumber));
        }));
    }

    private getConfigHash(config: AttributeDefinitionQueryParameters | any): string {

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

    public userIsSchedulerAttributeDefinitionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerAttributeDefinitionReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.AttributeDefinitions
        //
        if (userIsSchedulerAttributeDefinitionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerAttributeDefinitionReader = user.readPermission >= 1;
            } else {
                userIsSchedulerAttributeDefinitionReader = false;
            }
        }

        return userIsSchedulerAttributeDefinitionReader;
    }


    public userIsSchedulerAttributeDefinitionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerAttributeDefinitionWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.AttributeDefinitions
        //
        if (userIsSchedulerAttributeDefinitionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerAttributeDefinitionWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerAttributeDefinitionWriter = false;
          }      
        }

        return userIsSchedulerAttributeDefinitionWriter;
    }

    public GetAttributeDefinitionChangeHistoriesForAttributeDefinition(attributeDefinitionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AttributeDefinitionChangeHistoryData[]> {
        return this.attributeDefinitionChangeHistoryService.GetAttributeDefinitionChangeHistoryList({
            attributeDefinitionId: attributeDefinitionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AttributeDefinitionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AttributeDefinitionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AttributeDefinitionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAttributeDefinition(raw: any): AttributeDefinitionData {
    if (!raw) return raw;

    //
    // Create a AttributeDefinitionData object instance with correct prototype
    //
    const revived = Object.create(AttributeDefinitionData.prototype) as AttributeDefinitionData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._attributeDefinitionChangeHistories = null;
    (revived as any)._attributeDefinitionChangeHistoriesPromise = null;
    (revived as any)._attributeDefinitionChangeHistoriesSubject = new BehaviorSubject<AttributeDefinitionChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAttributeDefinitionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).AttributeDefinitionChangeHistories$ = (revived as any)._attributeDefinitionChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._attributeDefinitionChangeHistories === null && (revived as any)._attributeDefinitionChangeHistoriesPromise === null) {
                (revived as any).loadAttributeDefinitionChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).AttributeDefinitionChangeHistoriesCount$ = AttributeDefinitionChangeHistoryService.Instance.GetAttributeDefinitionChangeHistoriesRowCount({attributeDefinitionId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveAttributeDefinitionList(rawList: any[]): AttributeDefinitionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAttributeDefinition(raw));
  }

}
