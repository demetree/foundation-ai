/*

   GENERATED SERVICE FOR THE ASSIGNMENTROLEQUALIFICATIONREQUIREMENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AssignmentRoleQualificationRequirement table.

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
import { AssignmentRoleData } from './assignment-role.service';
import { QualificationData } from './qualification.service';
import { AssignmentRoleQualificationRequirementChangeHistoryService, AssignmentRoleQualificationRequirementChangeHistoryData } from './assignment-role-qualification-requirement-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AssignmentRoleQualificationRequirementQueryParameters {
    assignmentRoleId: bigint | number | null | undefined = null;
    qualificationId: bigint | number | null | undefined = null;
    isRequired: boolean | null | undefined = null;
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
export class AssignmentRoleQualificationRequirementSubmitData {
    id!: bigint | number;
    assignmentRoleId!: bigint | number;
    qualificationId!: bigint | number;
    isRequired!: boolean;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class AssignmentRoleQualificationRequirementBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AssignmentRoleQualificationRequirementChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `assignmentRoleQualificationRequirement.AssignmentRoleQualificationRequirementChildren$` — use with `| async` in templates
//        • Promise:    `assignmentRoleQualificationRequirement.AssignmentRoleQualificationRequirementChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="assignmentRoleQualificationRequirement.AssignmentRoleQualificationRequirementChildren$ | async"`), or
//        • Access the promise getter (`assignmentRoleQualificationRequirement.AssignmentRoleQualificationRequirementChildren` or `await assignmentRoleQualificationRequirement.AssignmentRoleQualificationRequirementChildren`)
//    - Simply reading `assignmentRoleQualificationRequirement.AssignmentRoleQualificationRequirementChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await assignmentRoleQualificationRequirement.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AssignmentRoleQualificationRequirementData {
    id!: bigint | number;
    assignmentRoleId!: bigint | number;
    qualificationId!: bigint | number;
    isRequired!: boolean;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    assignmentRole: AssignmentRoleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    qualification: QualificationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _assignmentRoleQualificationRequirementChangeHistories: AssignmentRoleQualificationRequirementChangeHistoryData[] | null = null;
    private _assignmentRoleQualificationRequirementChangeHistoriesPromise: Promise<AssignmentRoleQualificationRequirementChangeHistoryData[]> | null  = null;
    private _assignmentRoleQualificationRequirementChangeHistoriesSubject = new BehaviorSubject<AssignmentRoleQualificationRequirementChangeHistoryData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public AssignmentRoleQualificationRequirementChangeHistories$ = this._assignmentRoleQualificationRequirementChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._assignmentRoleQualificationRequirementChangeHistories === null && this._assignmentRoleQualificationRequirementChangeHistoriesPromise === null) {
            this.loadAssignmentRoleQualificationRequirementChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public AssignmentRoleQualificationRequirementChangeHistoriesCount$ = AssignmentRoleQualificationRequirementService.Instance.GetAssignmentRoleQualificationRequirementsRowCount({assignmentRoleQualificationRequirementId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AssignmentRoleQualificationRequirementData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.assignmentRoleQualificationRequirement.Reload();
  //
  //  Non Async:
  //
  //     assignmentRoleQualificationRequirement[0].Reload().then(x => {
  //        this.assignmentRoleQualificationRequirement = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AssignmentRoleQualificationRequirementService.Instance.GetAssignmentRoleQualificationRequirement(this.id, includeRelations)
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
     this._assignmentRoleQualificationRequirementChangeHistories = null;
     this._assignmentRoleQualificationRequirementChangeHistoriesPromise = null;
     this._assignmentRoleQualificationRequirementChangeHistoriesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the AssignmentRoleQualificationRequirementChangeHistories for this AssignmentRoleQualificationRequirement.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.assignmentRoleQualificationRequirement.AssignmentRoleQualificationRequirementChangeHistories.then(assignmentRoleQualificationRequirementChangeHistories => { ... })
     *   or
     *   await this.assignmentRoleQualificationRequirement.AssignmentRoleQualificationRequirementChangeHistories
     *
    */
    public get AssignmentRoleQualificationRequirementChangeHistories(): Promise<AssignmentRoleQualificationRequirementChangeHistoryData[]> {
        if (this._assignmentRoleQualificationRequirementChangeHistories !== null) {
            return Promise.resolve(this._assignmentRoleQualificationRequirementChangeHistories);
        }

        if (this._assignmentRoleQualificationRequirementChangeHistoriesPromise !== null) {
            return this._assignmentRoleQualificationRequirementChangeHistoriesPromise;
        }

        // Start the load
        this.loadAssignmentRoleQualificationRequirementChangeHistories();

        return this._assignmentRoleQualificationRequirementChangeHistoriesPromise!;
    }



    private loadAssignmentRoleQualificationRequirementChangeHistories(): void {

        this._assignmentRoleQualificationRequirementChangeHistoriesPromise = lastValueFrom(
            AssignmentRoleQualificationRequirementService.Instance.GetAssignmentRoleQualificationRequirementChangeHistoriesForAssignmentRoleQualificationRequirement(this.id)
        )
        .then(assignmentRoleQualificationRequirementChangeHistories => {
            this._assignmentRoleQualificationRequirementChangeHistories = assignmentRoleQualificationRequirementChangeHistories ?? [];
            this._assignmentRoleQualificationRequirementChangeHistoriesSubject.next(this._assignmentRoleQualificationRequirementChangeHistories);
            return this._assignmentRoleQualificationRequirementChangeHistories;
         })
        .catch(err => {
            this._assignmentRoleQualificationRequirementChangeHistories = [];
            this._assignmentRoleQualificationRequirementChangeHistoriesSubject.next(this._assignmentRoleQualificationRequirementChangeHistories);
            throw err;
        })
        .finally(() => {
            this._assignmentRoleQualificationRequirementChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached AssignmentRoleQualificationRequirementChangeHistory. Call after mutations to force refresh.
     */
    public ClearAssignmentRoleQualificationRequirementChangeHistoriesCache(): void {
        this._assignmentRoleQualificationRequirementChangeHistories = null;
        this._assignmentRoleQualificationRequirementChangeHistoriesPromise = null;
        this._assignmentRoleQualificationRequirementChangeHistoriesSubject.next(this._assignmentRoleQualificationRequirementChangeHistories);      // Emit to observable
    }

    public get HasAssignmentRoleQualificationRequirementChangeHistories(): Promise<boolean> {
        return this.AssignmentRoleQualificationRequirementChangeHistories.then(assignmentRoleQualificationRequirementChangeHistories => assignmentRoleQualificationRequirementChangeHistories.length > 0);
    }




    /**
     * Updates the state of this AssignmentRoleQualificationRequirementData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AssignmentRoleQualificationRequirementData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AssignmentRoleQualificationRequirementSubmitData {
        return AssignmentRoleQualificationRequirementService.Instance.ConvertToAssignmentRoleQualificationRequirementSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AssignmentRoleQualificationRequirementService extends SecureEndpointBase {

    private static _instance: AssignmentRoleQualificationRequirementService;
    private listCache: Map<string, Observable<Array<AssignmentRoleQualificationRequirementData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AssignmentRoleQualificationRequirementBasicListData>>>;
    private recordCache: Map<string, Observable<AssignmentRoleQualificationRequirementData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private assignmentRoleQualificationRequirementChangeHistoryService: AssignmentRoleQualificationRequirementChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AssignmentRoleQualificationRequirementData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AssignmentRoleQualificationRequirementBasicListData>>>();
        this.recordCache = new Map<string, Observable<AssignmentRoleQualificationRequirementData>>();

        AssignmentRoleQualificationRequirementService._instance = this;
    }

    public static get Instance(): AssignmentRoleQualificationRequirementService {
      return AssignmentRoleQualificationRequirementService._instance;
    }


    public ClearListCaches(config: AssignmentRoleQualificationRequirementQueryParameters | null = null) {

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


    public ConvertToAssignmentRoleQualificationRequirementSubmitData(data: AssignmentRoleQualificationRequirementData): AssignmentRoleQualificationRequirementSubmitData {

        let output = new AssignmentRoleQualificationRequirementSubmitData();

        output.id = data.id;
        output.assignmentRoleId = data.assignmentRoleId;
        output.qualificationId = data.qualificationId;
        output.isRequired = data.isRequired;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetAssignmentRoleQualificationRequirement(id: bigint | number, includeRelations: boolean = true) : Observable<AssignmentRoleQualificationRequirementData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const assignmentRoleQualificationRequirement$ = this.requestAssignmentRoleQualificationRequirement(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AssignmentRoleQualificationRequirement", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, assignmentRoleQualificationRequirement$);

            return assignmentRoleQualificationRequirement$;
        }

        return this.recordCache.get(configHash) as Observable<AssignmentRoleQualificationRequirementData>;
    }

    private requestAssignmentRoleQualificationRequirement(id: bigint | number, includeRelations: boolean = true) : Observable<AssignmentRoleQualificationRequirementData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AssignmentRoleQualificationRequirementData>(this.baseUrl + 'api/AssignmentRoleQualificationRequirement/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAssignmentRoleQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAssignmentRoleQualificationRequirement(id, includeRelations));
            }));
    }

    public GetAssignmentRoleQualificationRequirementList(config: AssignmentRoleQualificationRequirementQueryParameters | any = null) : Observable<Array<AssignmentRoleQualificationRequirementData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const assignmentRoleQualificationRequirementList$ = this.requestAssignmentRoleQualificationRequirementList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AssignmentRoleQualificationRequirement list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, assignmentRoleQualificationRequirementList$);

            return assignmentRoleQualificationRequirementList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AssignmentRoleQualificationRequirementData>>;
    }


    private requestAssignmentRoleQualificationRequirementList(config: AssignmentRoleQualificationRequirementQueryParameters | any) : Observable <Array<AssignmentRoleQualificationRequirementData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AssignmentRoleQualificationRequirementData>>(this.baseUrl + 'api/AssignmentRoleQualificationRequirements', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAssignmentRoleQualificationRequirementList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAssignmentRoleQualificationRequirementList(config));
            }));
    }

    public GetAssignmentRoleQualificationRequirementsRowCount(config: AssignmentRoleQualificationRequirementQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const assignmentRoleQualificationRequirementsRowCount$ = this.requestAssignmentRoleQualificationRequirementsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AssignmentRoleQualificationRequirements row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, assignmentRoleQualificationRequirementsRowCount$);

            return assignmentRoleQualificationRequirementsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAssignmentRoleQualificationRequirementsRowCount(config: AssignmentRoleQualificationRequirementQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AssignmentRoleQualificationRequirements/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAssignmentRoleQualificationRequirementsRowCount(config));
            }));
    }

    public GetAssignmentRoleQualificationRequirementsBasicListData(config: AssignmentRoleQualificationRequirementQueryParameters | any = null) : Observable<Array<AssignmentRoleQualificationRequirementBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const assignmentRoleQualificationRequirementsBasicListData$ = this.requestAssignmentRoleQualificationRequirementsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AssignmentRoleQualificationRequirements basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, assignmentRoleQualificationRequirementsBasicListData$);

            return assignmentRoleQualificationRequirementsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AssignmentRoleQualificationRequirementBasicListData>>;
    }


    private requestAssignmentRoleQualificationRequirementsBasicListData(config: AssignmentRoleQualificationRequirementQueryParameters | any) : Observable<Array<AssignmentRoleQualificationRequirementBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AssignmentRoleQualificationRequirementBasicListData>>(this.baseUrl + 'api/AssignmentRoleQualificationRequirements/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAssignmentRoleQualificationRequirementsBasicListData(config));
            }));

    }


    public PutAssignmentRoleQualificationRequirement(id: bigint | number, assignmentRoleQualificationRequirement: AssignmentRoleQualificationRequirementSubmitData) : Observable<AssignmentRoleQualificationRequirementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AssignmentRoleQualificationRequirementData>(this.baseUrl + 'api/AssignmentRoleQualificationRequirement/' + id.toString(), assignmentRoleQualificationRequirement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAssignmentRoleQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAssignmentRoleQualificationRequirement(id, assignmentRoleQualificationRequirement));
            }));
    }


    public PostAssignmentRoleQualificationRequirement(assignmentRoleQualificationRequirement: AssignmentRoleQualificationRequirementSubmitData) : Observable<AssignmentRoleQualificationRequirementData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AssignmentRoleQualificationRequirementData>(this.baseUrl + 'api/AssignmentRoleQualificationRequirement', assignmentRoleQualificationRequirement, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAssignmentRoleQualificationRequirement(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAssignmentRoleQualificationRequirement(assignmentRoleQualificationRequirement));
            }));
    }

  
    public DeleteAssignmentRoleQualificationRequirement(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AssignmentRoleQualificationRequirement/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAssignmentRoleQualificationRequirement(id));
            }));
    }

    public RollbackAssignmentRoleQualificationRequirement(id: bigint | number, versionNumber: bigint | number) : Observable<AssignmentRoleQualificationRequirementData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AssignmentRoleQualificationRequirementData>(this.baseUrl + 'api/AssignmentRoleQualificationRequirement/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAssignmentRoleQualificationRequirement(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackAssignmentRoleQualificationRequirement(id, versionNumber));
        }));
    }

    private getConfigHash(config: AssignmentRoleQualificationRequirementQueryParameters | any): string {

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

    public userIsSchedulerAssignmentRoleQualificationRequirementReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerAssignmentRoleQualificationRequirementReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.AssignmentRoleQualificationRequirements
        //
        if (userIsSchedulerAssignmentRoleQualificationRequirementReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerAssignmentRoleQualificationRequirementReader = user.readPermission >= 1;
            } else {
                userIsSchedulerAssignmentRoleQualificationRequirementReader = false;
            }
        }

        return userIsSchedulerAssignmentRoleQualificationRequirementReader;
    }


    public userIsSchedulerAssignmentRoleQualificationRequirementWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerAssignmentRoleQualificationRequirementWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.AssignmentRoleQualificationRequirements
        //
        if (userIsSchedulerAssignmentRoleQualificationRequirementWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerAssignmentRoleQualificationRequirementWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerAssignmentRoleQualificationRequirementWriter = false;
          }      
        }

        return userIsSchedulerAssignmentRoleQualificationRequirementWriter;
    }

    public GetAssignmentRoleQualificationRequirementChangeHistoriesForAssignmentRoleQualificationRequirement(assignmentRoleQualificationRequirementId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AssignmentRoleQualificationRequirementChangeHistoryData[]> {
        return this.assignmentRoleQualificationRequirementChangeHistoryService.GetAssignmentRoleQualificationRequirementChangeHistoryList({
            assignmentRoleQualificationRequirementId: assignmentRoleQualificationRequirementId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AssignmentRoleQualificationRequirementData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AssignmentRoleQualificationRequirementData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AssignmentRoleQualificationRequirementTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAssignmentRoleQualificationRequirement(raw: any): AssignmentRoleQualificationRequirementData {
    if (!raw) return raw;

    //
    // Create a AssignmentRoleQualificationRequirementData object instance with correct prototype
    //
    const revived = Object.create(AssignmentRoleQualificationRequirementData.prototype) as AssignmentRoleQualificationRequirementData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._assignmentRoleQualificationRequirementChangeHistories = null;
    (revived as any)._assignmentRoleQualificationRequirementChangeHistoriesPromise = null;
    (revived as any)._assignmentRoleQualificationRequirementChangeHistoriesSubject = new BehaviorSubject<AssignmentRoleQualificationRequirementChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAssignmentRoleQualificationRequirementXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).AssignmentRoleQualificationRequirementChangeHistories$ = (revived as any)._assignmentRoleQualificationRequirementChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._assignmentRoleQualificationRequirementChangeHistories === null && (revived as any)._assignmentRoleQualificationRequirementChangeHistoriesPromise === null) {
                (revived as any).loadAssignmentRoleQualificationRequirementChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).AssignmentRoleQualificationRequirementChangeHistoriesCount$ = AssignmentRoleQualificationRequirementChangeHistoryService.Instance.GetAssignmentRoleQualificationRequirementChangeHistoriesRowCount({assignmentRoleQualificationRequirementId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveAssignmentRoleQualificationRequirementList(rawList: any[]): AssignmentRoleQualificationRequirementData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAssignmentRoleQualificationRequirement(raw));
  }

}
