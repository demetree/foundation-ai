/*

   GENERATED SERVICE FOR THE BRICKELEMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickElement table.

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
import { BrickPartData } from './brick-part.service';
import { BrickColourData } from './brick-colour.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BrickElementQueryParameters {
    elementId: string | null | undefined = null;
    brickPartId: bigint | number | null | undefined = null;
    brickColourId: bigint | number | null | undefined = null;
    designId: string | null | undefined = null;
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
export class BrickElementSubmitData {
    id!: bigint | number;
    elementId!: string;
    brickPartId!: bigint | number;
    brickColourId!: bigint | number;
    designId: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BrickElementBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickElementChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickElement.BrickElementChildren$` — use with `| async` in templates
//        • Promise:    `brickElement.BrickElementChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickElement.BrickElementChildren$ | async"`), or
//        • Access the promise getter (`brickElement.BrickElementChildren` or `await brickElement.BrickElementChildren`)
//    - Simply reading `brickElement.BrickElementChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickElement.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickElementData {
    id!: bigint | number;
    elementId!: string;
    brickPartId!: bigint | number;
    brickColourId!: bigint | number;
    designId!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    brickColour: BrickColourData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    brickPart: BrickPartData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any BrickElementData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickElement.Reload();
  //
  //  Non Async:
  //
  //     brickElement[0].Reload().then(x => {
  //        this.brickElement = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickElementService.Instance.GetBrickElement(this.id, includeRelations)
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
     * Updates the state of this BrickElementData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickElementData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickElementSubmitData {
        return BrickElementService.Instance.ConvertToBrickElementSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickElementService extends SecureEndpointBase {

    private static _instance: BrickElementService;
    private listCache: Map<string, Observable<Array<BrickElementData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickElementBasicListData>>>;
    private recordCache: Map<string, Observable<BrickElementData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickElementData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickElementBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickElementData>>();

        BrickElementService._instance = this;
    }

    public static get Instance(): BrickElementService {
      return BrickElementService._instance;
    }


    public ClearListCaches(config: BrickElementQueryParameters | null = null) {

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


    public ConvertToBrickElementSubmitData(data: BrickElementData): BrickElementSubmitData {

        let output = new BrickElementSubmitData();

        output.id = data.id;
        output.elementId = data.elementId;
        output.brickPartId = data.brickPartId;
        output.brickColourId = data.brickColourId;
        output.designId = data.designId;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBrickElement(id: bigint | number, includeRelations: boolean = true) : Observable<BrickElementData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickElement$ = this.requestBrickElement(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickElement", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickElement$);

            return brickElement$;
        }

        return this.recordCache.get(configHash) as Observable<BrickElementData>;
    }

    private requestBrickElement(id: bigint | number, includeRelations: boolean = true) : Observable<BrickElementData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickElementData>(this.baseUrl + 'api/BrickElement/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickElement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickElement(id, includeRelations));
            }));
    }

    public GetBrickElementList(config: BrickElementQueryParameters | any = null) : Observable<Array<BrickElementData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickElementList$ = this.requestBrickElementList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickElement list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickElementList$);

            return brickElementList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickElementData>>;
    }


    private requestBrickElementList(config: BrickElementQueryParameters | any) : Observable <Array<BrickElementData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickElementData>>(this.baseUrl + 'api/BrickElements', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickElementList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickElementList(config));
            }));
    }

    public GetBrickElementsRowCount(config: BrickElementQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickElementsRowCount$ = this.requestBrickElementsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickElements row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickElementsRowCount$);

            return brickElementsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickElementsRowCount(config: BrickElementQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickElements/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickElementsRowCount(config));
            }));
    }

    public GetBrickElementsBasicListData(config: BrickElementQueryParameters | any = null) : Observable<Array<BrickElementBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickElementsBasicListData$ = this.requestBrickElementsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickElements basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickElementsBasicListData$);

            return brickElementsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickElementBasicListData>>;
    }


    private requestBrickElementsBasicListData(config: BrickElementQueryParameters | any) : Observable<Array<BrickElementBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickElementBasicListData>>(this.baseUrl + 'api/BrickElements/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickElementsBasicListData(config));
            }));

    }


    public PutBrickElement(id: bigint | number, brickElement: BrickElementSubmitData) : Observable<BrickElementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickElementData>(this.baseUrl + 'api/BrickElement/' + id.toString(), brickElement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickElement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickElement(id, brickElement));
            }));
    }


    public PostBrickElement(brickElement: BrickElementSubmitData) : Observable<BrickElementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickElementData>(this.baseUrl + 'api/BrickElement', brickElement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickElement(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickElement(brickElement));
            }));
    }

  
    public DeleteBrickElement(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickElement/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickElement(id));
            }));
    }


    private getConfigHash(config: BrickElementQueryParameters | any): string {

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

    public userIsBMCBrickElementReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickElementReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickElements
        //
        if (userIsBMCBrickElementReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickElementReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickElementReader = false;
            }
        }

        return userIsBMCBrickElementReader;
    }


    public userIsBMCBrickElementWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickElementWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickElements
        //
        if (userIsBMCBrickElementWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickElementWriter = user.writePermission >= 255;
          } else {
            userIsBMCBrickElementWriter = false;
          }      
        }

        return userIsBMCBrickElementWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full BrickElementData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickElementData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickElementTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickElement(raw: any): BrickElementData {
    if (!raw) return raw;

    //
    // Create a BrickElementData object instance with correct prototype
    //
    const revived = Object.create(BrickElementData.prototype) as BrickElementData;

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
    // 2. But private methods (loadBrickElementXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveBrickElementList(rawList: any[]): BrickElementData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickElement(raw));
  }

}
