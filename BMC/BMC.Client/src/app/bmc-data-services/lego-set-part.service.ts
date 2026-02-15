/*

   GENERATED SERVICE FOR THE LEGOSETPART TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the LegoSetPart table.

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
import { LegoSetData } from './lego-set.service';
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
export class LegoSetPartQueryParameters {
    legoSetId: bigint | number | null | undefined = null;
    brickPartId: bigint | number | null | undefined = null;
    brickColourId: bigint | number | null | undefined = null;
    quantity: bigint | number | null | undefined = null;
    isSpare: boolean | null | undefined = null;
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
export class LegoSetPartSubmitData {
    id!: bigint | number;
    legoSetId!: bigint | number;
    brickPartId!: bigint | number;
    brickColourId!: bigint | number;
    quantity: bigint | number | null = null;
    isSpare!: boolean;
    active!: boolean;
    deleted!: boolean;
}


export class LegoSetPartBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. LegoSetPartChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `legoSetPart.LegoSetPartChildren$` — use with `| async` in templates
//        • Promise:    `legoSetPart.LegoSetPartChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="legoSetPart.LegoSetPartChildren$ | async"`), or
//        • Access the promise getter (`legoSetPart.LegoSetPartChildren` or `await legoSetPart.LegoSetPartChildren`)
//    - Simply reading `legoSetPart.LegoSetPartChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await legoSetPart.Reload()` to refresh the entire object and clear all lazy caches.
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
export class LegoSetPartData {
    id!: bigint | number;
    legoSetId!: bigint | number;
    brickPartId!: bigint | number;
    brickColourId!: bigint | number;
    quantity!: bigint | number;
    isSpare!: boolean;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    brickColour: BrickColourData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    brickPart: BrickPartData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    legoSet: LegoSetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any LegoSetPartData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.legoSetPart.Reload();
  //
  //  Non Async:
  //
  //     legoSetPart[0].Reload().then(x => {
  //        this.legoSetPart = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      LegoSetPartService.Instance.GetLegoSetPart(this.id, includeRelations)
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
     * Updates the state of this LegoSetPartData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this LegoSetPartData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): LegoSetPartSubmitData {
        return LegoSetPartService.Instance.ConvertToLegoSetPartSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class LegoSetPartService extends SecureEndpointBase {

    private static _instance: LegoSetPartService;
    private listCache: Map<string, Observable<Array<LegoSetPartData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<LegoSetPartBasicListData>>>;
    private recordCache: Map<string, Observable<LegoSetPartData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<LegoSetPartData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<LegoSetPartBasicListData>>>();
        this.recordCache = new Map<string, Observable<LegoSetPartData>>();

        LegoSetPartService._instance = this;
    }

    public static get Instance(): LegoSetPartService {
      return LegoSetPartService._instance;
    }


    public ClearListCaches(config: LegoSetPartQueryParameters | null = null) {

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


    public ConvertToLegoSetPartSubmitData(data: LegoSetPartData): LegoSetPartSubmitData {

        let output = new LegoSetPartSubmitData();

        output.id = data.id;
        output.legoSetId = data.legoSetId;
        output.brickPartId = data.brickPartId;
        output.brickColourId = data.brickColourId;
        output.quantity = data.quantity;
        output.isSpare = data.isSpare;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetLegoSetPart(id: bigint | number, includeRelations: boolean = true) : Observable<LegoSetPartData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const legoSetPart$ = this.requestLegoSetPart(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LegoSetPart", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, legoSetPart$);

            return legoSetPart$;
        }

        return this.recordCache.get(configHash) as Observable<LegoSetPartData>;
    }

    private requestLegoSetPart(id: bigint | number, includeRelations: boolean = true) : Observable<LegoSetPartData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<LegoSetPartData>(this.baseUrl + 'api/LegoSetPart/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveLegoSetPart(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetPart(id, includeRelations));
            }));
    }

    public GetLegoSetPartList(config: LegoSetPartQueryParameters | any = null) : Observable<Array<LegoSetPartData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const legoSetPartList$ = this.requestLegoSetPartList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LegoSetPart list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, legoSetPartList$);

            return legoSetPartList$;
        }

        return this.listCache.get(configHash) as Observable<Array<LegoSetPartData>>;
    }


    private requestLegoSetPartList(config: LegoSetPartQueryParameters | any) : Observable <Array<LegoSetPartData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LegoSetPartData>>(this.baseUrl + 'api/LegoSetParts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveLegoSetPartList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetPartList(config));
            }));
    }

    public GetLegoSetPartsRowCount(config: LegoSetPartQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const legoSetPartsRowCount$ = this.requestLegoSetPartsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get LegoSetParts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, legoSetPartsRowCount$);

            return legoSetPartsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestLegoSetPartsRowCount(config: LegoSetPartQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/LegoSetParts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetPartsRowCount(config));
            }));
    }

    public GetLegoSetPartsBasicListData(config: LegoSetPartQueryParameters | any = null) : Observable<Array<LegoSetPartBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const legoSetPartsBasicListData$ = this.requestLegoSetPartsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get LegoSetParts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, legoSetPartsBasicListData$);

            return legoSetPartsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<LegoSetPartBasicListData>>;
    }


    private requestLegoSetPartsBasicListData(config: LegoSetPartQueryParameters | any) : Observable<Array<LegoSetPartBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<LegoSetPartBasicListData>>(this.baseUrl + 'api/LegoSetParts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestLegoSetPartsBasicListData(config));
            }));

    }


    public PutLegoSetPart(id: bigint | number, legoSetPart: LegoSetPartSubmitData) : Observable<LegoSetPartData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<LegoSetPartData>(this.baseUrl + 'api/LegoSetPart/' + id.toString(), legoSetPart, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLegoSetPart(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutLegoSetPart(id, legoSetPart));
            }));
    }


    public PostLegoSetPart(legoSetPart: LegoSetPartSubmitData) : Observable<LegoSetPartData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<LegoSetPartData>(this.baseUrl + 'api/LegoSetPart', legoSetPart, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveLegoSetPart(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostLegoSetPart(legoSetPart));
            }));
    }

  
    public DeleteLegoSetPart(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/LegoSetPart/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteLegoSetPart(id));
            }));
    }


    private getConfigHash(config: LegoSetPartQueryParameters | any): string {

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

    public userIsBMCLegoSetPartReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCLegoSetPartReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.LegoSetParts
        //
        if (userIsBMCLegoSetPartReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCLegoSetPartReader = user.readPermission >= 1;
            } else {
                userIsBMCLegoSetPartReader = false;
            }
        }

        return userIsBMCLegoSetPartReader;
    }


    public userIsBMCLegoSetPartWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCLegoSetPartWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.LegoSetParts
        //
        if (userIsBMCLegoSetPartWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCLegoSetPartWriter = user.writePermission >= 255;
          } else {
            userIsBMCLegoSetPartWriter = false;
          }      
        }

        return userIsBMCLegoSetPartWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full LegoSetPartData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the LegoSetPartData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when LegoSetPartTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveLegoSetPart(raw: any): LegoSetPartData {
    if (!raw) return raw;

    //
    // Create a LegoSetPartData object instance with correct prototype
    //
    const revived = Object.create(LegoSetPartData.prototype) as LegoSetPartData;

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
    // 2. But private methods (loadLegoSetPartXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveLegoSetPartList(rawList: any[]): LegoSetPartData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveLegoSetPart(raw));
  }

}
