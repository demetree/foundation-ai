/*

   GENERATED SERVICE FOR THE VOLUNTEERGROUPMEMBER TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the VolunteerGroupMember table.

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
import { VolunteerGroupData } from './volunteer-group.service';
import { ResourceData } from './resource.service';
import { AssignmentRoleData } from './assignment-role.service';
import { VolunteerGroupMemberChangeHistoryService, VolunteerGroupMemberChangeHistoryData } from './volunteer-group-member-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class VolunteerGroupMemberQueryParameters {
    volunteerGroupId: bigint | number | null | undefined = null;
    resourceId: bigint | number | null | undefined = null;
    assignmentRoleId: bigint | number | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    joinedDate: string | null | undefined = null;        // Date only (YYYY-MM-DD)
    leftDate: string | null | undefined = null;        // Date only (YYYY-MM-DD)
    notes: string | null | undefined = null;
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
export class VolunteerGroupMemberSubmitData {
    id!: bigint | number;
    volunteerGroupId!: bigint | number;
    resourceId!: bigint | number;
    assignmentRoleId: bigint | number | null = null;
    sequence!: bigint | number;
    joinedDate: string | null = null;     // Date only (YYYY-MM-DD)
    leftDate: string | null = null;     // Date only (YYYY-MM-DD)
    notes: string | null = null;
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

export class VolunteerGroupMemberBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. VolunteerGroupMemberChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `volunteerGroupMember.VolunteerGroupMemberChildren$` — use with `| async` in templates
//        • Promise:    `volunteerGroupMember.VolunteerGroupMemberChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="volunteerGroupMember.VolunteerGroupMemberChildren$ | async"`), or
//        • Access the promise getter (`volunteerGroupMember.VolunteerGroupMemberChildren` or `await volunteerGroupMember.VolunteerGroupMemberChildren`)
//    - Simply reading `volunteerGroupMember.VolunteerGroupMemberChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await volunteerGroupMember.Reload()` to refresh the entire object and clear all lazy caches.
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
export class VolunteerGroupMemberData {
    id!: bigint | number;
    volunteerGroupId!: bigint | number;
    resourceId!: bigint | number;
    assignmentRoleId!: bigint | number;
    sequence!: bigint | number;
    joinedDate!: string | null;   // Date only (YYYY-MM-DD)
    leftDate!: string | null;   // Date only (YYYY-MM-DD)
    notes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    assignmentRole: AssignmentRoleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    volunteerGroup: VolunteerGroupData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _volunteerGroupMemberChangeHistories: VolunteerGroupMemberChangeHistoryData[] | null = null;
    private _volunteerGroupMemberChangeHistoriesPromise: Promise<VolunteerGroupMemberChangeHistoryData[]> | null  = null;
    private _volunteerGroupMemberChangeHistoriesSubject = new BehaviorSubject<VolunteerGroupMemberChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<VolunteerGroupMemberData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<VolunteerGroupMemberData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<VolunteerGroupMemberData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public VolunteerGroupMemberChangeHistories$ = this._volunteerGroupMemberChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._volunteerGroupMemberChangeHistories === null && this._volunteerGroupMemberChangeHistoriesPromise === null) {
            this.loadVolunteerGroupMemberChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _volunteerGroupMemberChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get VolunteerGroupMemberChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._volunteerGroupMemberChangeHistoriesCount$ === null) {
            this._volunteerGroupMemberChangeHistoriesCount$ = VolunteerGroupMemberChangeHistoryService.Instance.GetVolunteerGroupMemberChangeHistoriesRowCount({volunteerGroupMemberId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._volunteerGroupMemberChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any VolunteerGroupMemberData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.volunteerGroupMember.Reload();
  //
  //  Non Async:
  //
  //     volunteerGroupMember[0].Reload().then(x => {
  //        this.volunteerGroupMember = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      VolunteerGroupMemberService.Instance.GetVolunteerGroupMember(this.id, includeRelations)
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
     this._volunteerGroupMemberChangeHistories = null;
     this._volunteerGroupMemberChangeHistoriesPromise = null;
     this._volunteerGroupMemberChangeHistoriesSubject.next(null);
     this._volunteerGroupMemberChangeHistoriesCount$ = null;

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
     * Gets the VolunteerGroupMemberChangeHistories for this VolunteerGroupMember.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.volunteerGroupMember.VolunteerGroupMemberChangeHistories.then(volunteerGroupMembers => { ... })
     *   or
     *   await this.volunteerGroupMember.volunteerGroupMembers
     *
    */
    public get VolunteerGroupMemberChangeHistories(): Promise<VolunteerGroupMemberChangeHistoryData[]> {
        if (this._volunteerGroupMemberChangeHistories !== null) {
            return Promise.resolve(this._volunteerGroupMemberChangeHistories);
        }

        if (this._volunteerGroupMemberChangeHistoriesPromise !== null) {
            return this._volunteerGroupMemberChangeHistoriesPromise;
        }

        // Start the load
        this.loadVolunteerGroupMemberChangeHistories();

        return this._volunteerGroupMemberChangeHistoriesPromise!;
    }



    private loadVolunteerGroupMemberChangeHistories(): void {

        this._volunteerGroupMemberChangeHistoriesPromise = lastValueFrom(
            VolunteerGroupMemberService.Instance.GetVolunteerGroupMemberChangeHistoriesForVolunteerGroupMember(this.id)
        )
        .then(VolunteerGroupMemberChangeHistories => {
            this._volunteerGroupMemberChangeHistories = VolunteerGroupMemberChangeHistories ?? [];
            this._volunteerGroupMemberChangeHistoriesSubject.next(this._volunteerGroupMemberChangeHistories);
            return this._volunteerGroupMemberChangeHistories;
         })
        .catch(err => {
            this._volunteerGroupMemberChangeHistories = [];
            this._volunteerGroupMemberChangeHistoriesSubject.next(this._volunteerGroupMemberChangeHistories);
            throw err;
        })
        .finally(() => {
            this._volunteerGroupMemberChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached VolunteerGroupMemberChangeHistory. Call after mutations to force refresh.
     */
    public ClearVolunteerGroupMemberChangeHistoriesCache(): void {
        this._volunteerGroupMemberChangeHistories = null;
        this._volunteerGroupMemberChangeHistoriesPromise = null;
        this._volunteerGroupMemberChangeHistoriesSubject.next(this._volunteerGroupMemberChangeHistories);      // Emit to observable
    }

    public get HasVolunteerGroupMemberChangeHistories(): Promise<boolean> {
        return this.VolunteerGroupMemberChangeHistories.then(volunteerGroupMemberChangeHistories => volunteerGroupMemberChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (volunteerGroupMember.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await volunteerGroupMember.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<VolunteerGroupMemberData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<VolunteerGroupMemberData>> {
        const info = await lastValueFrom(
            VolunteerGroupMemberService.Instance.GetVolunteerGroupMemberChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this VolunteerGroupMemberData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this VolunteerGroupMemberData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): VolunteerGroupMemberSubmitData {
        return VolunteerGroupMemberService.Instance.ConvertToVolunteerGroupMemberSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class VolunteerGroupMemberService extends SecureEndpointBase {

    private static _instance: VolunteerGroupMemberService;
    private listCache: Map<string, Observable<Array<VolunteerGroupMemberData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<VolunteerGroupMemberBasicListData>>>;
    private recordCache: Map<string, Observable<VolunteerGroupMemberData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private volunteerGroupMemberChangeHistoryService: VolunteerGroupMemberChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<VolunteerGroupMemberData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<VolunteerGroupMemberBasicListData>>>();
        this.recordCache = new Map<string, Observable<VolunteerGroupMemberData>>();

        VolunteerGroupMemberService._instance = this;
    }

    public static get Instance(): VolunteerGroupMemberService {
      return VolunteerGroupMemberService._instance;
    }


    public ClearListCaches(config: VolunteerGroupMemberQueryParameters | null = null) {

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


    public ConvertToVolunteerGroupMemberSubmitData(data: VolunteerGroupMemberData): VolunteerGroupMemberSubmitData {

        let output = new VolunteerGroupMemberSubmitData();

        output.id = data.id;
        output.volunteerGroupId = data.volunteerGroupId;
        output.resourceId = data.resourceId;
        output.assignmentRoleId = data.assignmentRoleId;
        output.sequence = data.sequence;
        output.joinedDate = data.joinedDate;
        output.leftDate = data.leftDate;
        output.notes = data.notes;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetVolunteerGroupMember(id: bigint | number, includeRelations: boolean = true) : Observable<VolunteerGroupMemberData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const volunteerGroupMember$ = this.requestVolunteerGroupMember(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerGroupMember", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, volunteerGroupMember$);

            return volunteerGroupMember$;
        }

        return this.recordCache.get(configHash) as Observable<VolunteerGroupMemberData>;
    }

    private requestVolunteerGroupMember(id: bigint | number, includeRelations: boolean = true) : Observable<VolunteerGroupMemberData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VolunteerGroupMemberData>(this.baseUrl + 'api/VolunteerGroupMember/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveVolunteerGroupMember(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerGroupMember(id, includeRelations));
            }));
    }

    public GetVolunteerGroupMemberList(config: VolunteerGroupMemberQueryParameters | any = null) : Observable<Array<VolunteerGroupMemberData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const volunteerGroupMemberList$ = this.requestVolunteerGroupMemberList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerGroupMember list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, volunteerGroupMemberList$);

            return volunteerGroupMemberList$;
        }

        return this.listCache.get(configHash) as Observable<Array<VolunteerGroupMemberData>>;
    }


    private requestVolunteerGroupMemberList(config: VolunteerGroupMemberQueryParameters | any) : Observable <Array<VolunteerGroupMemberData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<VolunteerGroupMemberData>>(this.baseUrl + 'api/VolunteerGroupMembers', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveVolunteerGroupMemberList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerGroupMemberList(config));
            }));
    }

    public GetVolunteerGroupMembersRowCount(config: VolunteerGroupMemberQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const volunteerGroupMembersRowCount$ = this.requestVolunteerGroupMembersRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerGroupMembers row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, volunteerGroupMembersRowCount$);

            return volunteerGroupMembersRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestVolunteerGroupMembersRowCount(config: VolunteerGroupMemberQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/VolunteerGroupMembers/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerGroupMembersRowCount(config));
            }));
    }

    public GetVolunteerGroupMembersBasicListData(config: VolunteerGroupMemberQueryParameters | any = null) : Observable<Array<VolunteerGroupMemberBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const volunteerGroupMembersBasicListData$ = this.requestVolunteerGroupMembersBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerGroupMembers basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, volunteerGroupMembersBasicListData$);

            return volunteerGroupMembersBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<VolunteerGroupMemberBasicListData>>;
    }


    private requestVolunteerGroupMembersBasicListData(config: VolunteerGroupMemberQueryParameters | any) : Observable<Array<VolunteerGroupMemberBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<VolunteerGroupMemberBasicListData>>(this.baseUrl + 'api/VolunteerGroupMembers/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerGroupMembersBasicListData(config));
            }));

    }


    public PutVolunteerGroupMember(id: bigint | number, volunteerGroupMember: VolunteerGroupMemberSubmitData) : Observable<VolunteerGroupMemberData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<VolunteerGroupMemberData>(this.baseUrl + 'api/VolunteerGroupMember/' + id.toString(), volunteerGroupMember, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveVolunteerGroupMember(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutVolunteerGroupMember(id, volunteerGroupMember));
            }));
    }


    public PostVolunteerGroupMember(volunteerGroupMember: VolunteerGroupMemberSubmitData) : Observable<VolunteerGroupMemberData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<VolunteerGroupMemberData>(this.baseUrl + 'api/VolunteerGroupMember', volunteerGroupMember, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveVolunteerGroupMember(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostVolunteerGroupMember(volunteerGroupMember));
            }));
    }

  
    public DeleteVolunteerGroupMember(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/VolunteerGroupMember/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteVolunteerGroupMember(id));
            }));
    }

    public RollbackVolunteerGroupMember(id: bigint | number, versionNumber: bigint | number) : Observable<VolunteerGroupMemberData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<VolunteerGroupMemberData>(this.baseUrl + 'api/VolunteerGroupMember/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveVolunteerGroupMember(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackVolunteerGroupMember(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a VolunteerGroupMember.
     */
    public GetVolunteerGroupMemberChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<VolunteerGroupMemberData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<VolunteerGroupMemberData>>(this.baseUrl + 'api/VolunteerGroupMember/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetVolunteerGroupMemberChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a VolunteerGroupMember.
     */
    public GetVolunteerGroupMemberAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<VolunteerGroupMemberData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<VolunteerGroupMemberData>[]>(this.baseUrl + 'api/VolunteerGroupMember/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetVolunteerGroupMemberAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a VolunteerGroupMember.
     */
    public GetVolunteerGroupMemberVersion(id: bigint | number, version: number): Observable<VolunteerGroupMemberData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VolunteerGroupMemberData>(this.baseUrl + 'api/VolunteerGroupMember/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveVolunteerGroupMember(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetVolunteerGroupMemberVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a VolunteerGroupMember at a specific point in time.
     */
    public GetVolunteerGroupMemberStateAtTime(id: bigint | number, time: string): Observable<VolunteerGroupMemberData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VolunteerGroupMemberData>(this.baseUrl + 'api/VolunteerGroupMember/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveVolunteerGroupMember(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetVolunteerGroupMemberStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: VolunteerGroupMemberQueryParameters | any): string {

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

    public userIsSchedulerVolunteerGroupMemberReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerVolunteerGroupMemberReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.VolunteerGroupMembers
        //
        if (userIsSchedulerVolunteerGroupMemberReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerVolunteerGroupMemberReader = user.readPermission >= 1;
            } else {
                userIsSchedulerVolunteerGroupMemberReader = false;
            }
        }

        return userIsSchedulerVolunteerGroupMemberReader;
    }


    public userIsSchedulerVolunteerGroupMemberWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerVolunteerGroupMemberWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.VolunteerGroupMembers
        //
        if (userIsSchedulerVolunteerGroupMemberWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerVolunteerGroupMemberWriter = user.writePermission >= 40;
          } else {
            userIsSchedulerVolunteerGroupMemberWriter = false;
          }      
        }

        return userIsSchedulerVolunteerGroupMemberWriter;
    }

    public GetVolunteerGroupMemberChangeHistoriesForVolunteerGroupMember(volunteerGroupMemberId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<VolunteerGroupMemberChangeHistoryData[]> {
        return this.volunteerGroupMemberChangeHistoryService.GetVolunteerGroupMemberChangeHistoryList({
            volunteerGroupMemberId: volunteerGroupMemberId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full VolunteerGroupMemberData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the VolunteerGroupMemberData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when VolunteerGroupMemberTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveVolunteerGroupMember(raw: any): VolunteerGroupMemberData {
    if (!raw) return raw;

    //
    // Create a VolunteerGroupMemberData object instance with correct prototype
    //
    const revived = Object.create(VolunteerGroupMemberData.prototype) as VolunteerGroupMemberData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._volunteerGroupMemberChangeHistories = null;
    (revived as any)._volunteerGroupMemberChangeHistoriesPromise = null;
    (revived as any)._volunteerGroupMemberChangeHistoriesSubject = new BehaviorSubject<VolunteerGroupMemberChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadVolunteerGroupMemberXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).VolunteerGroupMemberChangeHistories$ = (revived as any)._volunteerGroupMemberChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._volunteerGroupMemberChangeHistories === null && (revived as any)._volunteerGroupMemberChangeHistoriesPromise === null) {
                (revived as any).loadVolunteerGroupMemberChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._volunteerGroupMemberChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<VolunteerGroupMemberData> | null>(null);

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

  private ReviveVolunteerGroupMemberList(rawList: any[]): VolunteerGroupMemberData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveVolunteerGroupMember(raw));
  }

}
