/*

   GENERATED SERVICE FOR THE USERLOSTPART TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the UserLostPart table.

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
import { LegoSetData } from './lego-set.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class UserLostPartQueryParameters {
    brickPartId: bigint | number | null | undefined = null;
    brickColourId: bigint | number | null | undefined = null;
    legoSetId: bigint | number | null | undefined = null;
    lostQuantity: bigint | number | null | undefined = null;
    rebrickableInvPartId: bigint | number | null | undefined = null;
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
export class UserLostPartSubmitData {
    id!: bigint | number;
    brickPartId!: bigint | number;
    brickColourId!: bigint | number;
    legoSetId: bigint | number | null = null;
    lostQuantity!: bigint | number;
    rebrickableInvPartId: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class UserLostPartBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. UserLostPartChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `userLostPart.UserLostPartChildren$` — use with `| async` in templates
//        • Promise:    `userLostPart.UserLostPartChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="userLostPart.UserLostPartChildren$ | async"`), or
//        • Access the promise getter (`userLostPart.UserLostPartChildren` or `await userLostPart.UserLostPartChildren`)
//    - Simply reading `userLostPart.UserLostPartChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await userLostPart.Reload()` to refresh the entire object and clear all lazy caches.
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
export class UserLostPartData {
    id!: bigint | number;
    brickPartId!: bigint | number;
    brickColourId!: bigint | number;
    legoSetId!: bigint | number;
    lostQuantity!: bigint | number;
    rebrickableInvPartId!: bigint | number;
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
  // Promise based reload method to allow rebuilding of any UserLostPartData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.userLostPart.Reload();
  //
  //  Non Async:
  //
  //     userLostPart[0].Reload().then(x => {
  //        this.userLostPart = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      UserLostPartService.Instance.GetUserLostPart(this.id, includeRelations)
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
     * Updates the state of this UserLostPartData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this UserLostPartData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): UserLostPartSubmitData {
        return UserLostPartService.Instance.ConvertToUserLostPartSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class UserLostPartService extends SecureEndpointBase {

    private static _instance: UserLostPartService;
    private listCache: Map<string, Observable<Array<UserLostPartData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<UserLostPartBasicListData>>>;
    private recordCache: Map<string, Observable<UserLostPartData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<UserLostPartData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<UserLostPartBasicListData>>>();
        this.recordCache = new Map<string, Observable<UserLostPartData>>();

        UserLostPartService._instance = this;
    }

    public static get Instance(): UserLostPartService {
      return UserLostPartService._instance;
    }


    public ClearListCaches(config: UserLostPartQueryParameters | null = null) {

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


    public ConvertToUserLostPartSubmitData(data: UserLostPartData): UserLostPartSubmitData {

        let output = new UserLostPartSubmitData();

        output.id = data.id;
        output.brickPartId = data.brickPartId;
        output.brickColourId = data.brickColourId;
        output.legoSetId = data.legoSetId;
        output.lostQuantity = data.lostQuantity;
        output.rebrickableInvPartId = data.rebrickableInvPartId;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetUserLostPart(id: bigint | number, includeRelations: boolean = true) : Observable<UserLostPartData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const userLostPart$ = this.requestUserLostPart(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserLostPart", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, userLostPart$);

            return userLostPart$;
        }

        return this.recordCache.get(configHash) as Observable<UserLostPartData>;
    }

    private requestUserLostPart(id: bigint | number, includeRelations: boolean = true) : Observable<UserLostPartData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<UserLostPartData>(this.baseUrl + 'api/UserLostPart/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveUserLostPart(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserLostPart(id, includeRelations));
            }));
    }

    public GetUserLostPartList(config: UserLostPartQueryParameters | any = null) : Observable<Array<UserLostPartData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const userLostPartList$ = this.requestUserLostPartList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserLostPart list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, userLostPartList$);

            return userLostPartList$;
        }

        return this.listCache.get(configHash) as Observable<Array<UserLostPartData>>;
    }


    private requestUserLostPartList(config: UserLostPartQueryParameters | any) : Observable <Array<UserLostPartData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserLostPartData>>(this.baseUrl + 'api/UserLostParts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveUserLostPartList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestUserLostPartList(config));
            }));
    }

    public GetUserLostPartsRowCount(config: UserLostPartQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const userLostPartsRowCount$ = this.requestUserLostPartsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get UserLostParts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, userLostPartsRowCount$);

            return userLostPartsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestUserLostPartsRowCount(config: UserLostPartQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/UserLostParts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserLostPartsRowCount(config));
            }));
    }

    public GetUserLostPartsBasicListData(config: UserLostPartQueryParameters | any = null) : Observable<Array<UserLostPartBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const userLostPartsBasicListData$ = this.requestUserLostPartsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get UserLostParts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, userLostPartsBasicListData$);

            return userLostPartsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<UserLostPartBasicListData>>;
    }


    private requestUserLostPartsBasicListData(config: UserLostPartQueryParameters | any) : Observable<Array<UserLostPartBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<UserLostPartBasicListData>>(this.baseUrl + 'api/UserLostParts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestUserLostPartsBasicListData(config));
            }));

    }


    public PutUserLostPart(id: bigint | number, userLostPart: UserLostPartSubmitData) : Observable<UserLostPartData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<UserLostPartData>(this.baseUrl + 'api/UserLostPart/' + id.toString(), userLostPart, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserLostPart(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutUserLostPart(id, userLostPart));
            }));
    }


    public PostUserLostPart(userLostPart: UserLostPartSubmitData) : Observable<UserLostPartData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<UserLostPartData>(this.baseUrl + 'api/UserLostPart', userLostPart, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveUserLostPart(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostUserLostPart(userLostPart));
            }));
    }

  
    public DeleteUserLostPart(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/UserLostPart/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteUserLostPart(id));
            }));
    }


    private getConfigHash(config: UserLostPartQueryParameters | any): string {

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

    public userIsBMCUserLostPartReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCUserLostPartReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.UserLostParts
        //
        if (userIsBMCUserLostPartReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCUserLostPartReader = user.readPermission >= 1;
            } else {
                userIsBMCUserLostPartReader = false;
            }
        }

        return userIsBMCUserLostPartReader;
    }


    public userIsBMCUserLostPartWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCUserLostPartWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.UserLostParts
        //
        if (userIsBMCUserLostPartWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCUserLostPartWriter = user.writePermission >= 10;
          } else {
            userIsBMCUserLostPartWriter = false;
          }      
        }

        return userIsBMCUserLostPartWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full UserLostPartData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the UserLostPartData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when UserLostPartTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveUserLostPart(raw: any): UserLostPartData {
    if (!raw) return raw;

    //
    // Create a UserLostPartData object instance with correct prototype
    //
    const revived = Object.create(UserLostPartData.prototype) as UserLostPartData;

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
    // 2. But private methods (loadUserLostPartXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveUserLostPartList(rawList: any[]): UserLostPartData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveUserLostPart(raw));
  }

}
