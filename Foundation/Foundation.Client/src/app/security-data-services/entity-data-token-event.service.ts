/*

   GENERATED SERVICE FOR THE ENTITYDATATOKENEVENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the EntityDataTokenEvent table.

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
import { EntityDataTokenData } from './entity-data-token.service';
import { EntityDataTokenEventTypeData } from './entity-data-token-event-type.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class EntityDataTokenEventQueryParameters {
    entityDataTokenId: bigint | number | null | undefined = null;
    entityDataTokenEventTypeId: bigint | number | null | undefined = null;
    timeStamp: string | null | undefined = null;        // ISO 8601 (full datetime)
    comments: string | null | undefined = null;
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
export class EntityDataTokenEventSubmitData {
    id!: bigint | number;
    entityDataTokenId!: bigint | number;
    entityDataTokenEventTypeId!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    comments: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class EntityDataTokenEventBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. EntityDataTokenEventChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `entityDataTokenEvent.EntityDataTokenEventChildren$` — use with `| async` in templates
//        • Promise:    `entityDataTokenEvent.EntityDataTokenEventChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="entityDataTokenEvent.EntityDataTokenEventChildren$ | async"`), or
//        • Access the promise getter (`entityDataTokenEvent.EntityDataTokenEventChildren` or `await entityDataTokenEvent.EntityDataTokenEventChildren`)
//    - Simply reading `entityDataTokenEvent.EntityDataTokenEventChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await entityDataTokenEvent.Reload()` to refresh the entire object and clear all lazy caches.
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
export class EntityDataTokenEventData {
    id!: bigint | number;
    entityDataTokenId!: bigint | number;
    entityDataTokenEventTypeId!: bigint | number;
    timeStamp!: string;      // ISO 8601 (full datetime)
    comments!: string | null;
    active!: boolean;
    deleted!: boolean;
    entityDataToken: EntityDataTokenData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    entityDataTokenEventType: EntityDataTokenEventTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any EntityDataTokenEventData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.entityDataTokenEvent.Reload();
  //
  //  Non Async:
  //
  //     entityDataTokenEvent[0].Reload().then(x => {
  //        this.entityDataTokenEvent = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      EntityDataTokenEventService.Instance.GetEntityDataTokenEvent(this.id, includeRelations)
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
     * Updates the state of this EntityDataTokenEventData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this EntityDataTokenEventData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): EntityDataTokenEventSubmitData {
        return EntityDataTokenEventService.Instance.ConvertToEntityDataTokenEventSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class EntityDataTokenEventService extends SecureEndpointBase {

    private static _instance: EntityDataTokenEventService;
    private listCache: Map<string, Observable<Array<EntityDataTokenEventData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<EntityDataTokenEventBasicListData>>>;
    private recordCache: Map<string, Observable<EntityDataTokenEventData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<EntityDataTokenEventData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<EntityDataTokenEventBasicListData>>>();
        this.recordCache = new Map<string, Observable<EntityDataTokenEventData>>();

        EntityDataTokenEventService._instance = this;
    }

    public static get Instance(): EntityDataTokenEventService {
      return EntityDataTokenEventService._instance;
    }


    public ClearListCaches(config: EntityDataTokenEventQueryParameters | null = null) {

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


    public ConvertToEntityDataTokenEventSubmitData(data: EntityDataTokenEventData): EntityDataTokenEventSubmitData {

        let output = new EntityDataTokenEventSubmitData();

        output.id = data.id;
        output.entityDataTokenId = data.entityDataTokenId;
        output.entityDataTokenEventTypeId = data.entityDataTokenEventTypeId;
        output.timeStamp = data.timeStamp;
        output.comments = data.comments;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetEntityDataTokenEvent(id: bigint | number, includeRelations: boolean = true) : Observable<EntityDataTokenEventData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const entityDataTokenEvent$ = this.requestEntityDataTokenEvent(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EntityDataTokenEvent", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, entityDataTokenEvent$);

            return entityDataTokenEvent$;
        }

        return this.recordCache.get(configHash) as Observable<EntityDataTokenEventData>;
    }

    private requestEntityDataTokenEvent(id: bigint | number, includeRelations: boolean = true) : Observable<EntityDataTokenEventData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<EntityDataTokenEventData>(this.baseUrl + 'api/EntityDataTokenEvent/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveEntityDataTokenEvent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestEntityDataTokenEvent(id, includeRelations));
            }));
    }

    public GetEntityDataTokenEventList(config: EntityDataTokenEventQueryParameters | any = null) : Observable<Array<EntityDataTokenEventData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const entityDataTokenEventList$ = this.requestEntityDataTokenEventList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EntityDataTokenEvent list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, entityDataTokenEventList$);

            return entityDataTokenEventList$;
        }

        return this.listCache.get(configHash) as Observable<Array<EntityDataTokenEventData>>;
    }


    private requestEntityDataTokenEventList(config: EntityDataTokenEventQueryParameters | any) : Observable <Array<EntityDataTokenEventData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EntityDataTokenEventData>>(this.baseUrl + 'api/EntityDataTokenEvents', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveEntityDataTokenEventList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestEntityDataTokenEventList(config));
            }));
    }

    public GetEntityDataTokenEventsRowCount(config: EntityDataTokenEventQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const entityDataTokenEventsRowCount$ = this.requestEntityDataTokenEventsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get EntityDataTokenEvents row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, entityDataTokenEventsRowCount$);

            return entityDataTokenEventsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestEntityDataTokenEventsRowCount(config: EntityDataTokenEventQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/EntityDataTokenEvents/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEntityDataTokenEventsRowCount(config));
            }));
    }

    public GetEntityDataTokenEventsBasicListData(config: EntityDataTokenEventQueryParameters | any = null) : Observable<Array<EntityDataTokenEventBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const entityDataTokenEventsBasicListData$ = this.requestEntityDataTokenEventsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get EntityDataTokenEvents basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, entityDataTokenEventsBasicListData$);

            return entityDataTokenEventsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<EntityDataTokenEventBasicListData>>;
    }


    private requestEntityDataTokenEventsBasicListData(config: EntityDataTokenEventQueryParameters | any) : Observable<Array<EntityDataTokenEventBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<EntityDataTokenEventBasicListData>>(this.baseUrl + 'api/EntityDataTokenEvents/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestEntityDataTokenEventsBasicListData(config));
            }));

    }


    public PutEntityDataTokenEvent(id: bigint | number, entityDataTokenEvent: EntityDataTokenEventSubmitData) : Observable<EntityDataTokenEventData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<EntityDataTokenEventData>(this.baseUrl + 'api/EntityDataTokenEvent/' + id.toString(), entityDataTokenEvent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEntityDataTokenEvent(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutEntityDataTokenEvent(id, entityDataTokenEvent));
            }));
    }


    public PostEntityDataTokenEvent(entityDataTokenEvent: EntityDataTokenEventSubmitData) : Observable<EntityDataTokenEventData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<EntityDataTokenEventData>(this.baseUrl + 'api/EntityDataTokenEvent', entityDataTokenEvent, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveEntityDataTokenEvent(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostEntityDataTokenEvent(entityDataTokenEvent));
            }));
    }

  
    public DeleteEntityDataTokenEvent(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/EntityDataTokenEvent/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteEntityDataTokenEvent(id));
            }));
    }


    private getConfigHash(config: EntityDataTokenEventQueryParameters | any): string {

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

    public userIsSecurityEntityDataTokenEventReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecurityEntityDataTokenEventReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.EntityDataTokenEvents
        //
        if (userIsSecurityEntityDataTokenEventReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecurityEntityDataTokenEventReader = user.readPermission >= 1;
            } else {
                userIsSecurityEntityDataTokenEventReader = false;
            }
        }

        return userIsSecurityEntityDataTokenEventReader;
    }


    public userIsSecurityEntityDataTokenEventWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecurityEntityDataTokenEventWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.EntityDataTokenEvents
        //
        if (userIsSecurityEntityDataTokenEventWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecurityEntityDataTokenEventWriter = user.writePermission >= 255;
          } else {
            userIsSecurityEntityDataTokenEventWriter = false;
          }      
        }

        return userIsSecurityEntityDataTokenEventWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full EntityDataTokenEventData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the EntityDataTokenEventData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when EntityDataTokenEventTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveEntityDataTokenEvent(raw: any): EntityDataTokenEventData {
    if (!raw) return raw;

    //
    // Create a EntityDataTokenEventData object instance with correct prototype
    //
    const revived = Object.create(EntityDataTokenEventData.prototype) as EntityDataTokenEventData;

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
    // 2. But private methods (loadEntityDataTokenEventXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveEntityDataTokenEventList(rawList: any[]): EntityDataTokenEventData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveEntityDataTokenEvent(raw));
  }

}
