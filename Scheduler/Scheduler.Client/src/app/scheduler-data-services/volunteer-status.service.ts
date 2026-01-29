/*

   GENERATED SERVICE FOR THE VOLUNTEERSTATUS TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the VolunteerStatus table.

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
import { IconData } from './icon.service';
import { VolunteerProfileService, VolunteerProfileData } from './volunteer-profile.service';
import { VolunteerGroupService, VolunteerGroupData } from './volunteer-group.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class VolunteerStatusQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    isActive: boolean | null | undefined = null;
    preventsScheduling: boolean | null | undefined = null;
    requiresApproval: boolean | null | undefined = null;
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
export class VolunteerStatusSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    color: string | null = null;
    iconId: bigint | number | null = null;
    isActive: boolean | null = null;
    preventsScheduling!: boolean;
    requiresApproval!: boolean;
    active!: boolean;
    deleted!: boolean;
}


export class VolunteerStatusBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. VolunteerStatusChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `volunteerStatus.VolunteerStatusChildren$` — use with `| async` in templates
//        • Promise:    `volunteerStatus.VolunteerStatusChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="volunteerStatus.VolunteerStatusChildren$ | async"`), or
//        • Access the promise getter (`volunteerStatus.VolunteerStatusChildren` or `await volunteerStatus.VolunteerStatusChildren`)
//    - Simply reading `volunteerStatus.VolunteerStatusChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await volunteerStatus.Reload()` to refresh the entire object and clear all lazy caches.
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
export class VolunteerStatusData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence!: bigint | number;
    color!: string | null;
    iconId!: bigint | number;
    isActive!: boolean | null;
    preventsScheduling!: boolean;
    requiresApproval!: boolean;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _volunteerProfiles: VolunteerProfileData[] | null = null;
    private _volunteerProfilesPromise: Promise<VolunteerProfileData[]> | null  = null;
    private _volunteerProfilesSubject = new BehaviorSubject<VolunteerProfileData[] | null>(null);

                
    private _volunteerGroups: VolunteerGroupData[] | null = null;
    private _volunteerGroupsPromise: Promise<VolunteerGroupData[]> | null  = null;
    private _volunteerGroupsSubject = new BehaviorSubject<VolunteerGroupData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public VolunteerProfiles$ = this._volunteerProfilesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._volunteerProfiles === null && this._volunteerProfilesPromise === null) {
            this.loadVolunteerProfiles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public VolunteerProfilesCount$ = VolunteerProfileService.Instance.GetVolunteerProfilesRowCount({volunteerStatusId: this.id,
      active: true,
      deleted: false
    });



    public VolunteerGroups$ = this._volunteerGroupsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._volunteerGroups === null && this._volunteerGroupsPromise === null) {
            this.loadVolunteerGroups(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public VolunteerGroupsCount$ = VolunteerGroupService.Instance.GetVolunteerGroupsRowCount({volunteerStatusId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any VolunteerStatusData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.volunteerStatus.Reload();
  //
  //  Non Async:
  //
  //     volunteerStatus[0].Reload().then(x => {
  //        this.volunteerStatus = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      VolunteerStatusService.Instance.GetVolunteerStatus(this.id, includeRelations)
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
     this._volunteerProfiles = null;
     this._volunteerProfilesPromise = null;
     this._volunteerProfilesSubject.next(null);

     this._volunteerGroups = null;
     this._volunteerGroupsPromise = null;
     this._volunteerGroupsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the VolunteerProfiles for this VolunteerStatus.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.volunteerStatus.VolunteerProfiles.then(volunteerStatuses => { ... })
     *   or
     *   await this.volunteerStatus.volunteerStatuses
     *
    */
    public get VolunteerProfiles(): Promise<VolunteerProfileData[]> {
        if (this._volunteerProfiles !== null) {
            return Promise.resolve(this._volunteerProfiles);
        }

        if (this._volunteerProfilesPromise !== null) {
            return this._volunteerProfilesPromise;
        }

        // Start the load
        this.loadVolunteerProfiles();

        return this._volunteerProfilesPromise!;
    }



    private loadVolunteerProfiles(): void {

        this._volunteerProfilesPromise = lastValueFrom(
            VolunteerStatusService.Instance.GetVolunteerProfilesForVolunteerStatus(this.id)
        )
        .then(VolunteerProfiles => {
            this._volunteerProfiles = VolunteerProfiles ?? [];
            this._volunteerProfilesSubject.next(this._volunteerProfiles);
            return this._volunteerProfiles;
         })
        .catch(err => {
            this._volunteerProfiles = [];
            this._volunteerProfilesSubject.next(this._volunteerProfiles);
            throw err;
        })
        .finally(() => {
            this._volunteerProfilesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached VolunteerProfile. Call after mutations to force refresh.
     */
    public ClearVolunteerProfilesCache(): void {
        this._volunteerProfiles = null;
        this._volunteerProfilesPromise = null;
        this._volunteerProfilesSubject.next(this._volunteerProfiles);      // Emit to observable
    }

    public get HasVolunteerProfiles(): Promise<boolean> {
        return this.VolunteerProfiles.then(volunteerProfiles => volunteerProfiles.length > 0);
    }


    /**
     *
     * Gets the VolunteerGroups for this VolunteerStatus.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.volunteerStatus.VolunteerGroups.then(volunteerStatuses => { ... })
     *   or
     *   await this.volunteerStatus.volunteerStatuses
     *
    */
    public get VolunteerGroups(): Promise<VolunteerGroupData[]> {
        if (this._volunteerGroups !== null) {
            return Promise.resolve(this._volunteerGroups);
        }

        if (this._volunteerGroupsPromise !== null) {
            return this._volunteerGroupsPromise;
        }

        // Start the load
        this.loadVolunteerGroups();

        return this._volunteerGroupsPromise!;
    }



    private loadVolunteerGroups(): void {

        this._volunteerGroupsPromise = lastValueFrom(
            VolunteerStatusService.Instance.GetVolunteerGroupsForVolunteerStatus(this.id)
        )
        .then(VolunteerGroups => {
            this._volunteerGroups = VolunteerGroups ?? [];
            this._volunteerGroupsSubject.next(this._volunteerGroups);
            return this._volunteerGroups;
         })
        .catch(err => {
            this._volunteerGroups = [];
            this._volunteerGroupsSubject.next(this._volunteerGroups);
            throw err;
        })
        .finally(() => {
            this._volunteerGroupsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached VolunteerGroup. Call after mutations to force refresh.
     */
    public ClearVolunteerGroupsCache(): void {
        this._volunteerGroups = null;
        this._volunteerGroupsPromise = null;
        this._volunteerGroupsSubject.next(this._volunteerGroups);      // Emit to observable
    }

    public get HasVolunteerGroups(): Promise<boolean> {
        return this.VolunteerGroups.then(volunteerGroups => volunteerGroups.length > 0);
    }




    /**
     * Updates the state of this VolunteerStatusData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this VolunteerStatusData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): VolunteerStatusSubmitData {
        return VolunteerStatusService.Instance.ConvertToVolunteerStatusSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class VolunteerStatusService extends SecureEndpointBase {

    private static _instance: VolunteerStatusService;
    private listCache: Map<string, Observable<Array<VolunteerStatusData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<VolunteerStatusBasicListData>>>;
    private recordCache: Map<string, Observable<VolunteerStatusData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private volunteerProfileService: VolunteerProfileService,
        private volunteerGroupService: VolunteerGroupService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<VolunteerStatusData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<VolunteerStatusBasicListData>>>();
        this.recordCache = new Map<string, Observable<VolunteerStatusData>>();

        VolunteerStatusService._instance = this;
    }

    public static get Instance(): VolunteerStatusService {
      return VolunteerStatusService._instance;
    }


    public ClearListCaches(config: VolunteerStatusQueryParameters | null = null) {

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


    public ConvertToVolunteerStatusSubmitData(data: VolunteerStatusData): VolunteerStatusSubmitData {

        let output = new VolunteerStatusSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.color = data.color;
        output.iconId = data.iconId;
        output.isActive = data.isActive;
        output.preventsScheduling = data.preventsScheduling;
        output.requiresApproval = data.requiresApproval;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetVolunteerStatus(id: bigint | number, includeRelations: boolean = true) : Observable<VolunteerStatusData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const volunteerStatus$ = this.requestVolunteerStatus(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerStatus", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, volunteerStatus$);

            return volunteerStatus$;
        }

        return this.recordCache.get(configHash) as Observable<VolunteerStatusData>;
    }

    private requestVolunteerStatus(id: bigint | number, includeRelations: boolean = true) : Observable<VolunteerStatusData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VolunteerStatusData>(this.baseUrl + 'api/VolunteerStatus/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveVolunteerStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerStatus(id, includeRelations));
            }));
    }

    public GetVolunteerStatusList(config: VolunteerStatusQueryParameters | any = null) : Observable<Array<VolunteerStatusData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const volunteerStatusList$ = this.requestVolunteerStatusList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerStatus list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, volunteerStatusList$);

            return volunteerStatusList$;
        }

        return this.listCache.get(configHash) as Observable<Array<VolunteerStatusData>>;
    }


    private requestVolunteerStatusList(config: VolunteerStatusQueryParameters | any) : Observable <Array<VolunteerStatusData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<VolunteerStatusData>>(this.baseUrl + 'api/VolunteerStatuses', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveVolunteerStatusList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerStatusList(config));
            }));
    }

    public GetVolunteerStatusesRowCount(config: VolunteerStatusQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const volunteerStatusesRowCount$ = this.requestVolunteerStatusesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerStatuses row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, volunteerStatusesRowCount$);

            return volunteerStatusesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestVolunteerStatusesRowCount(config: VolunteerStatusQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/VolunteerStatuses/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerStatusesRowCount(config));
            }));
    }

    public GetVolunteerStatusesBasicListData(config: VolunteerStatusQueryParameters | any = null) : Observable<Array<VolunteerStatusBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const volunteerStatusesBasicListData$ = this.requestVolunteerStatusesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerStatuses basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, volunteerStatusesBasicListData$);

            return volunteerStatusesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<VolunteerStatusBasicListData>>;
    }


    private requestVolunteerStatusesBasicListData(config: VolunteerStatusQueryParameters | any) : Observable<Array<VolunteerStatusBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<VolunteerStatusBasicListData>>(this.baseUrl + 'api/VolunteerStatuses/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerStatusesBasicListData(config));
            }));

    }


    public PutVolunteerStatus(id: bigint | number, volunteerStatus: VolunteerStatusSubmitData) : Observable<VolunteerStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<VolunteerStatusData>(this.baseUrl + 'api/VolunteerStatus/' + id.toString(), volunteerStatus, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveVolunteerStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutVolunteerStatus(id, volunteerStatus));
            }));
    }


    public PostVolunteerStatus(volunteerStatus: VolunteerStatusSubmitData) : Observable<VolunteerStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<VolunteerStatusData>(this.baseUrl + 'api/VolunteerStatus', volunteerStatus, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveVolunteerStatus(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostVolunteerStatus(volunteerStatus));
            }));
    }

  
    public DeleteVolunteerStatus(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/VolunteerStatus/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteVolunteerStatus(id));
            }));
    }


    private getConfigHash(config: VolunteerStatusQueryParameters | any): string {

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

    public userIsSchedulerVolunteerStatusReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerVolunteerStatusReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.VolunteerStatuses
        //
        if (userIsSchedulerVolunteerStatusReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerVolunteerStatusReader = user.readPermission >= 1;
            } else {
                userIsSchedulerVolunteerStatusReader = false;
            }
        }

        return userIsSchedulerVolunteerStatusReader;
    }


    public userIsSchedulerVolunteerStatusWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerVolunteerStatusWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.VolunteerStatuses
        //
        if (userIsSchedulerVolunteerStatusWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerVolunteerStatusWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerVolunteerStatusWriter = false;
          }      
        }

        return userIsSchedulerVolunteerStatusWriter;
    }

    public GetVolunteerProfilesForVolunteerStatus(volunteerStatusId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<VolunteerProfileData[]> {
        return this.volunteerProfileService.GetVolunteerProfileList({
            volunteerStatusId: volunteerStatusId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetVolunteerGroupsForVolunteerStatus(volunteerStatusId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<VolunteerGroupData[]> {
        return this.volunteerGroupService.GetVolunteerGroupList({
            volunteerStatusId: volunteerStatusId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full VolunteerStatusData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the VolunteerStatusData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when VolunteerStatusTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveVolunteerStatus(raw: any): VolunteerStatusData {
    if (!raw) return raw;

    //
    // Create a VolunteerStatusData object instance with correct prototype
    //
    const revived = Object.create(VolunteerStatusData.prototype) as VolunteerStatusData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._volunteerProfiles = null;
    (revived as any)._volunteerProfilesPromise = null;
    (revived as any)._volunteerProfilesSubject = new BehaviorSubject<VolunteerProfileData[] | null>(null);

    (revived as any)._volunteerGroups = null;
    (revived as any)._volunteerGroupsPromise = null;
    (revived as any)._volunteerGroupsSubject = new BehaviorSubject<VolunteerGroupData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadVolunteerStatusXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).VolunteerProfiles$ = (revived as any)._volunteerProfilesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._volunteerProfiles === null && (revived as any)._volunteerProfilesPromise === null) {
                (revived as any).loadVolunteerProfiles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).VolunteerProfilesCount$ = VolunteerProfileService.Instance.GetVolunteerProfilesRowCount({volunteerStatusId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).VolunteerGroups$ = (revived as any)._volunteerGroupsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._volunteerGroups === null && (revived as any)._volunteerGroupsPromise === null) {
                (revived as any).loadVolunteerGroups();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).VolunteerGroupsCount$ = VolunteerGroupService.Instance.GetVolunteerGroupsRowCount({volunteerStatusId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveVolunteerStatusList(rawList: any[]): VolunteerStatusData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveVolunteerStatus(raw));
  }

}
