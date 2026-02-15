/*

   GENERATED SERVICE FOR THE CREWMEMBER TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the CrewMember table.

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
import { CrewData } from './crew.service';
import { ResourceData } from './resource.service';
import { AssignmentRoleData } from './assignment-role.service';
import { IconData } from './icon.service';
import { CrewMemberChangeHistoryService, CrewMemberChangeHistoryData } from './crew-member-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class CrewMemberQueryParameters {
    crewId: bigint | number | null | undefined = null;
    resourceId: bigint | number | null | undefined = null;
    assignmentRoleId: bigint | number | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class CrewMemberSubmitData {
    id!: bigint | number;
    crewId!: bigint | number;
    resourceId!: bigint | number;
    assignmentRoleId: bigint | number | null = null;
    sequence!: bigint | number;
    iconId: bigint | number | null = null;
    color: string | null = null;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}



//
// Version history information returned from version history API endpoints.
// Matches server-side VersionInformation<T> structure.
//
export interface VersionInformationUser {
    id: bigint | number;
    userName: string;
    firstName: string | null;
    middleName: string | null;
    lastName: string | null;
}

export interface VersionInformation<T> {
    timeStamp: string;           // ISO 8601
    user: VersionInformationUser;
    versionNumber: number;
    data: T | null;
}

export class CrewMemberBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. CrewMemberChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `crewMember.CrewMemberChildren$` — use with `| async` in templates
//        • Promise:    `crewMember.CrewMemberChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="crewMember.CrewMemberChildren$ | async"`), or
//        • Access the promise getter (`crewMember.CrewMemberChildren` or `await crewMember.CrewMemberChildren`)
//    - Simply reading `crewMember.CrewMemberChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await crewMember.Reload()` to refresh the entire object and clear all lazy caches.
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
export class CrewMemberData {
    id!: bigint | number;
    crewId!: bigint | number;
    resourceId!: bigint | number;
    assignmentRoleId!: bigint | number;
    sequence!: bigint | number;
    iconId!: bigint | number;
    color!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    assignmentRole: AssignmentRoleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    crew: CrewData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _crewMemberChangeHistories: CrewMemberChangeHistoryData[] | null = null;
    private _crewMemberChangeHistoriesPromise: Promise<CrewMemberChangeHistoryData[]> | null  = null;
    private _crewMemberChangeHistoriesSubject = new BehaviorSubject<CrewMemberChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<CrewMemberData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<CrewMemberData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<CrewMemberData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public CrewMemberChangeHistories$ = this._crewMemberChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._crewMemberChangeHistories === null && this._crewMemberChangeHistoriesPromise === null) {
            this.loadCrewMemberChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public CrewMemberChangeHistoriesCount$ = CrewMemberChangeHistoryService.Instance.GetCrewMemberChangeHistoriesRowCount({crewMemberId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any CrewMemberData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.crewMember.Reload();
  //
  //  Non Async:
  //
  //     crewMember[0].Reload().then(x => {
  //        this.crewMember = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      CrewMemberService.Instance.GetCrewMember(this.id, includeRelations)
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
     this._crewMemberChangeHistories = null;
     this._crewMemberChangeHistoriesPromise = null;
     this._crewMemberChangeHistoriesSubject.next(null);

     this._currentVersionInfo = null;
     this._currentVersionInfoPromise = null;
     this._currentVersionInfoSubject.next(null);
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the CrewMemberChangeHistories for this CrewMember.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.crewMember.CrewMemberChangeHistories.then(crewMembers => { ... })
     *   or
     *   await this.crewMember.crewMembers
     *
    */
    public get CrewMemberChangeHistories(): Promise<CrewMemberChangeHistoryData[]> {
        if (this._crewMemberChangeHistories !== null) {
            return Promise.resolve(this._crewMemberChangeHistories);
        }

        if (this._crewMemberChangeHistoriesPromise !== null) {
            return this._crewMemberChangeHistoriesPromise;
        }

        // Start the load
        this.loadCrewMemberChangeHistories();

        return this._crewMemberChangeHistoriesPromise!;
    }



    private loadCrewMemberChangeHistories(): void {

        this._crewMemberChangeHistoriesPromise = lastValueFrom(
            CrewMemberService.Instance.GetCrewMemberChangeHistoriesForCrewMember(this.id)
        )
        .then(CrewMemberChangeHistories => {
            this._crewMemberChangeHistories = CrewMemberChangeHistories ?? [];
            this._crewMemberChangeHistoriesSubject.next(this._crewMemberChangeHistories);
            return this._crewMemberChangeHistories;
         })
        .catch(err => {
            this._crewMemberChangeHistories = [];
            this._crewMemberChangeHistoriesSubject.next(this._crewMemberChangeHistories);
            throw err;
        })
        .finally(() => {
            this._crewMemberChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached CrewMemberChangeHistory. Call after mutations to force refresh.
     */
    public ClearCrewMemberChangeHistoriesCache(): void {
        this._crewMemberChangeHistories = null;
        this._crewMemberChangeHistoriesPromise = null;
        this._crewMemberChangeHistoriesSubject.next(this._crewMemberChangeHistories);      // Emit to observable
    }

    public get HasCrewMemberChangeHistories(): Promise<boolean> {
        return this.CrewMemberChangeHistories.then(crewMemberChangeHistories => crewMemberChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (crewMember.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await crewMember.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<CrewMemberData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<CrewMemberData>> {
        const info = await lastValueFrom(
            CrewMemberService.Instance.GetCrewMemberChangeMetadata(this.id, this.versionNumber as number)
        );
        this._currentVersionInfo = info;
        this._currentVersionInfoSubject.next(info);
        return info;
    }


    public ClearCurrentVersionInfoCache(): void {
        this._currentVersionInfo = null;
        this._currentVersionInfoPromise = null;
        this._currentVersionInfoSubject.next(null);
    }



    /**
     * Updates the state of this CrewMemberData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this CrewMemberData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): CrewMemberSubmitData {
        return CrewMemberService.Instance.ConvertToCrewMemberSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class CrewMemberService extends SecureEndpointBase {

    private static _instance: CrewMemberService;
    private listCache: Map<string, Observable<Array<CrewMemberData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<CrewMemberBasicListData>>>;
    private recordCache: Map<string, Observable<CrewMemberData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private crewMemberChangeHistoryService: CrewMemberChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<CrewMemberData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<CrewMemberBasicListData>>>();
        this.recordCache = new Map<string, Observable<CrewMemberData>>();

        CrewMemberService._instance = this;
    }

    public static get Instance(): CrewMemberService {
      return CrewMemberService._instance;
    }


    public ClearListCaches(config: CrewMemberQueryParameters | null = null) {

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


    public ConvertToCrewMemberSubmitData(data: CrewMemberData): CrewMemberSubmitData {

        let output = new CrewMemberSubmitData();

        output.id = data.id;
        output.crewId = data.crewId;
        output.resourceId = data.resourceId;
        output.assignmentRoleId = data.assignmentRoleId;
        output.sequence = data.sequence;
        output.iconId = data.iconId;
        output.color = data.color;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetCrewMember(id: bigint | number, includeRelations: boolean = true) : Observable<CrewMemberData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const crewMember$ = this.requestCrewMember(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get CrewMember", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, crewMember$);

            return crewMember$;
        }

        return this.recordCache.get(configHash) as Observable<CrewMemberData>;
    }

    private requestCrewMember(id: bigint | number, includeRelations: boolean = true) : Observable<CrewMemberData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CrewMemberData>(this.baseUrl + 'api/CrewMember/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveCrewMember(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestCrewMember(id, includeRelations));
            }));
    }

    public GetCrewMemberList(config: CrewMemberQueryParameters | any = null) : Observable<Array<CrewMemberData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const crewMemberList$ = this.requestCrewMemberList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get CrewMember list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, crewMemberList$);

            return crewMemberList$;
        }

        return this.listCache.get(configHash) as Observable<Array<CrewMemberData>>;
    }


    private requestCrewMemberList(config: CrewMemberQueryParameters | any) : Observable <Array<CrewMemberData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CrewMemberData>>(this.baseUrl + 'api/CrewMembers', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveCrewMemberList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestCrewMemberList(config));
            }));
    }

    public GetCrewMembersRowCount(config: CrewMemberQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const crewMembersRowCount$ = this.requestCrewMembersRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get CrewMembers row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, crewMembersRowCount$);

            return crewMembersRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestCrewMembersRowCount(config: CrewMemberQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/CrewMembers/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCrewMembersRowCount(config));
            }));
    }

    public GetCrewMembersBasicListData(config: CrewMemberQueryParameters | any = null) : Observable<Array<CrewMemberBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const crewMembersBasicListData$ = this.requestCrewMembersBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get CrewMembers basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, crewMembersBasicListData$);

            return crewMembersBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<CrewMemberBasicListData>>;
    }


    private requestCrewMembersBasicListData(config: CrewMemberQueryParameters | any) : Observable<Array<CrewMemberBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<CrewMemberBasicListData>>(this.baseUrl + 'api/CrewMembers/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestCrewMembersBasicListData(config));
            }));

    }


    public PutCrewMember(id: bigint | number, crewMember: CrewMemberSubmitData) : Observable<CrewMemberData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CrewMemberData>(this.baseUrl + 'api/CrewMember/' + id.toString(), crewMember, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCrewMember(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutCrewMember(id, crewMember));
            }));
    }


    public PostCrewMember(crewMember: CrewMemberSubmitData) : Observable<CrewMemberData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<CrewMemberData>(this.baseUrl + 'api/CrewMember', crewMember, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCrewMember(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostCrewMember(crewMember));
            }));
    }

  
    public DeleteCrewMember(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/CrewMember/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteCrewMember(id));
            }));
    }

    public RollbackCrewMember(id: bigint | number, versionNumber: bigint | number) : Observable<CrewMemberData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<CrewMemberData>(this.baseUrl + 'api/CrewMember/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveCrewMember(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackCrewMember(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a CrewMember.
     */
    public GetCrewMemberChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<CrewMemberData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<CrewMemberData>>(this.baseUrl + 'api/CrewMember/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetCrewMemberChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a CrewMember.
     */
    public GetCrewMemberAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<CrewMemberData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<CrewMemberData>[]>(this.baseUrl + 'api/CrewMember/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetCrewMemberAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a CrewMember.
     */
    public GetCrewMemberVersion(id: bigint | number, version: number): Observable<CrewMemberData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CrewMemberData>(this.baseUrl + 'api/CrewMember/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveCrewMember(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetCrewMemberVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a CrewMember at a specific point in time.
     */
    public GetCrewMemberStateAtTime(id: bigint | number, time: string): Observable<CrewMemberData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<CrewMemberData>(this.baseUrl + 'api/CrewMember/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveCrewMember(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetCrewMemberStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: CrewMemberQueryParameters | any): string {

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

    public userIsSchedulerCrewMemberReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerCrewMemberReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.CrewMembers
        //
        if (userIsSchedulerCrewMemberReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerCrewMemberReader = user.readPermission >= 1;
            } else {
                userIsSchedulerCrewMemberReader = false;
            }
        }

        return userIsSchedulerCrewMemberReader;
    }


    public userIsSchedulerCrewMemberWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerCrewMemberWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.CrewMembers
        //
        if (userIsSchedulerCrewMemberWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerCrewMemberWriter = user.writePermission >= 30;
          } else {
            userIsSchedulerCrewMemberWriter = false;
          }      
        }

        return userIsSchedulerCrewMemberWriter;
    }

    public GetCrewMemberChangeHistoriesForCrewMember(crewMemberId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CrewMemberChangeHistoryData[]> {
        return this.crewMemberChangeHistoryService.GetCrewMemberChangeHistoryList({
            crewMemberId: crewMemberId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full CrewMemberData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the CrewMemberData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when CrewMemberTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveCrewMember(raw: any): CrewMemberData {
    if (!raw) return raw;

    //
    // Create a CrewMemberData object instance with correct prototype
    //
    const revived = Object.create(CrewMemberData.prototype) as CrewMemberData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._crewMemberChangeHistories = null;
    (revived as any)._crewMemberChangeHistoriesPromise = null;
    (revived as any)._crewMemberChangeHistoriesSubject = new BehaviorSubject<CrewMemberChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadCrewMemberXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).CrewMemberChangeHistories$ = (revived as any)._crewMemberChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._crewMemberChangeHistories === null && (revived as any)._crewMemberChangeHistoriesPromise === null) {
                (revived as any).loadCrewMemberChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).CrewMemberChangeHistoriesCount$ = CrewMemberChangeHistoryService.Instance.GetCrewMemberChangeHistoriesRowCount({crewMemberId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<CrewMemberData> | null>(null);

    (revived as any).CurrentVersionInfo$ = (revived as any)._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if ((revived as any)._currentVersionInfo === null && (revived as any)._currentVersionInfoPromise === null) {
                (revived as any).loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    return revived;
  }

  private ReviveCrewMemberList(rawList: any[]): CrewMemberData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveCrewMember(raw));
  }

}
