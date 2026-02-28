/*

   GENERATED SERVICE FOR THE PARTSUBFILEREFERENCE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PartSubFileReference table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PartSubFileReferenceQueryParameters {
    parentBrickPartId: bigint | number | null | undefined = null;
    referencedBrickPartId: bigint | number | null | undefined = null;
    referencedFileName: string | null | undefined = null;
    transformMatrix: string | null | undefined = null;
    colorCode: bigint | number | null | undefined = null;
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
export class PartSubFileReferenceSubmitData {
    id!: bigint | number;
    parentBrickPartId!: bigint | number;
    referencedBrickPartId: bigint | number | null = null;
    referencedFileName!: string;
    transformMatrix!: string;
    colorCode: bigint | number | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class PartSubFileReferenceBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PartSubFileReferenceChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `partSubFileReference.PartSubFileReferenceChildren$` — use with `| async` in templates
//        • Promise:    `partSubFileReference.PartSubFileReferenceChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="partSubFileReference.PartSubFileReferenceChildren$ | async"`), or
//        • Access the promise getter (`partSubFileReference.PartSubFileReferenceChildren` or `await partSubFileReference.PartSubFileReferenceChildren`)
//    - Simply reading `partSubFileReference.PartSubFileReferenceChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await partSubFileReference.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PartSubFileReferenceData {
    id!: bigint | number;
    parentBrickPartId!: bigint | number;
    referencedBrickPartId!: bigint | number;
    referencedFileName!: string;
    transformMatrix!: string;
    colorCode!: bigint | number;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    parentBrickPart: BrickPartData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)
    referencedBrickPart: BrickPartData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any PartSubFileReferenceData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.partSubFileReference.Reload();
  //
  //  Non Async:
  //
  //     partSubFileReference[0].Reload().then(x => {
  //        this.partSubFileReference = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PartSubFileReferenceService.Instance.GetPartSubFileReference(this.id, includeRelations)
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
     * Updates the state of this PartSubFileReferenceData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PartSubFileReferenceData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PartSubFileReferenceSubmitData {
        return PartSubFileReferenceService.Instance.ConvertToPartSubFileReferenceSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PartSubFileReferenceService extends SecureEndpointBase {

    private static _instance: PartSubFileReferenceService;
    private listCache: Map<string, Observable<Array<PartSubFileReferenceData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PartSubFileReferenceBasicListData>>>;
    private recordCache: Map<string, Observable<PartSubFileReferenceData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PartSubFileReferenceData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PartSubFileReferenceBasicListData>>>();
        this.recordCache = new Map<string, Observable<PartSubFileReferenceData>>();

        PartSubFileReferenceService._instance = this;
    }

    public static get Instance(): PartSubFileReferenceService {
      return PartSubFileReferenceService._instance;
    }


    public ClearListCaches(config: PartSubFileReferenceQueryParameters | null = null) {

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


    public ConvertToPartSubFileReferenceSubmitData(data: PartSubFileReferenceData): PartSubFileReferenceSubmitData {

        let output = new PartSubFileReferenceSubmitData();

        output.id = data.id;
        output.parentBrickPartId = data.parentBrickPartId;
        output.referencedBrickPartId = data.referencedBrickPartId;
        output.referencedFileName = data.referencedFileName;
        output.transformMatrix = data.transformMatrix;
        output.colorCode = data.colorCode;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPartSubFileReference(id: bigint | number, includeRelations: boolean = true) : Observable<PartSubFileReferenceData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const partSubFileReference$ = this.requestPartSubFileReference(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PartSubFileReference", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, partSubFileReference$);

            return partSubFileReference$;
        }

        return this.recordCache.get(configHash) as Observable<PartSubFileReferenceData>;
    }

    private requestPartSubFileReference(id: bigint | number, includeRelations: boolean = true) : Observable<PartSubFileReferenceData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PartSubFileReferenceData>(this.baseUrl + 'api/PartSubFileReference/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePartSubFileReference(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPartSubFileReference(id, includeRelations));
            }));
    }

    public GetPartSubFileReferenceList(config: PartSubFileReferenceQueryParameters | any = null) : Observable<Array<PartSubFileReferenceData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const partSubFileReferenceList$ = this.requestPartSubFileReferenceList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PartSubFileReference list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, partSubFileReferenceList$);

            return partSubFileReferenceList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PartSubFileReferenceData>>;
    }


    private requestPartSubFileReferenceList(config: PartSubFileReferenceQueryParameters | any) : Observable <Array<PartSubFileReferenceData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PartSubFileReferenceData>>(this.baseUrl + 'api/PartSubFileReferences', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePartSubFileReferenceList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPartSubFileReferenceList(config));
            }));
    }

    public GetPartSubFileReferencesRowCount(config: PartSubFileReferenceQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const partSubFileReferencesRowCount$ = this.requestPartSubFileReferencesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PartSubFileReferences row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, partSubFileReferencesRowCount$);

            return partSubFileReferencesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPartSubFileReferencesRowCount(config: PartSubFileReferenceQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PartSubFileReferences/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPartSubFileReferencesRowCount(config));
            }));
    }

    public GetPartSubFileReferencesBasicListData(config: PartSubFileReferenceQueryParameters | any = null) : Observable<Array<PartSubFileReferenceBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const partSubFileReferencesBasicListData$ = this.requestPartSubFileReferencesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PartSubFileReferences basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, partSubFileReferencesBasicListData$);

            return partSubFileReferencesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PartSubFileReferenceBasicListData>>;
    }


    private requestPartSubFileReferencesBasicListData(config: PartSubFileReferenceQueryParameters | any) : Observable<Array<PartSubFileReferenceBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PartSubFileReferenceBasicListData>>(this.baseUrl + 'api/PartSubFileReferences/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPartSubFileReferencesBasicListData(config));
            }));

    }


    public PutPartSubFileReference(id: bigint | number, partSubFileReference: PartSubFileReferenceSubmitData) : Observable<PartSubFileReferenceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PartSubFileReferenceData>(this.baseUrl + 'api/PartSubFileReference/' + id.toString(), partSubFileReference, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePartSubFileReference(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPartSubFileReference(id, partSubFileReference));
            }));
    }


    public PostPartSubFileReference(partSubFileReference: PartSubFileReferenceSubmitData) : Observable<PartSubFileReferenceData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PartSubFileReferenceData>(this.baseUrl + 'api/PartSubFileReference', partSubFileReference, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePartSubFileReference(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPartSubFileReference(partSubFileReference));
            }));
    }

  
    public DeletePartSubFileReference(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PartSubFileReference/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePartSubFileReference(id));
            }));
    }


    private getConfigHash(config: PartSubFileReferenceQueryParameters | any): string {

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

    public userIsBMCPartSubFileReferenceReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCPartSubFileReferenceReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.PartSubFileReferences
        //
        if (userIsBMCPartSubFileReferenceReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCPartSubFileReferenceReader = user.readPermission >= 1;
            } else {
                userIsBMCPartSubFileReferenceReader = false;
            }
        }

        return userIsBMCPartSubFileReferenceReader;
    }


    public userIsBMCPartSubFileReferenceWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCPartSubFileReferenceWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.PartSubFileReferences
        //
        if (userIsBMCPartSubFileReferenceWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCPartSubFileReferenceWriter = user.writePermission >= 50;
          } else {
            userIsBMCPartSubFileReferenceWriter = false;
          }      
        }

        return userIsBMCPartSubFileReferenceWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full PartSubFileReferenceData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PartSubFileReferenceData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PartSubFileReferenceTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePartSubFileReference(raw: any): PartSubFileReferenceData {
    if (!raw) return raw;

    //
    // Create a PartSubFileReferenceData object instance with correct prototype
    //
    const revived = Object.create(PartSubFileReferenceData.prototype) as PartSubFileReferenceData;

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
    // 2. But private methods (loadPartSubFileReferenceXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private RevivePartSubFileReferenceList(rawList: any[]): PartSubFileReferenceData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePartSubFileReference(raw));
  }

}
