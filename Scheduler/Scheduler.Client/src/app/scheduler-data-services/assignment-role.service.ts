/*

   GENERATED SERVICE FOR THE ASSIGNMENTROLE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the AssignmentRole table.

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
import { AssignmentRoleQualificationRequirementService, AssignmentRoleQualificationRequirementData } from './assignment-role-qualification-requirement.service';
import { RateSheetService, RateSheetData } from './rate-sheet.service';
import { CrewMemberService, CrewMemberData } from './crew-member.service';
import { EventResourceAssignmentService, EventResourceAssignmentData } from './event-resource-assignment.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class AssignmentRoleQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class AssignmentRoleSubmitData {
    id!: bigint | number;
    name!: string;
    description: string | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class AssignmentRoleBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. AssignmentRoleChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `assignmentRole.AssignmentRoleChildren$` — use with `| async` in templates
//        • Promise:    `assignmentRole.AssignmentRoleChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="assignmentRole.AssignmentRoleChildren$ | async"`), or
//        • Access the promise getter (`assignmentRole.AssignmentRoleChildren` or `await assignmentRole.AssignmentRoleChildren`)
//    - Simply reading `assignmentRole.AssignmentRoleChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await assignmentRole.Reload()` to refresh the entire object and clear all lazy caches.
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
export class AssignmentRoleData {
    id!: bigint | number;
    name!: string;
    description!: string | null;
    iconId!: bigint | number;
    color!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _assignmentRoleQualificationRequirements: AssignmentRoleQualificationRequirementData[] | null = null;
    private _assignmentRoleQualificationRequirementsPromise: Promise<AssignmentRoleQualificationRequirementData[]> | null  = null;
    private _assignmentRoleQualificationRequirementsSubject = new BehaviorSubject<AssignmentRoleQualificationRequirementData[] | null>(null);

    private _rateSheets: RateSheetData[] | null = null;
    private _rateSheetsPromise: Promise<RateSheetData[]> | null  = null;
    private _rateSheetsSubject = new BehaviorSubject<RateSheetData[] | null>(null);

    private _crewMembers: CrewMemberData[] | null = null;
    private _crewMembersPromise: Promise<CrewMemberData[]> | null  = null;
    private _crewMembersSubject = new BehaviorSubject<CrewMemberData[] | null>(null);

    private _eventResourceAssignments: EventResourceAssignmentData[] | null = null;
    private _eventResourceAssignmentsPromise: Promise<EventResourceAssignmentData[]> | null  = null;
    private _eventResourceAssignmentsSubject = new BehaviorSubject<EventResourceAssignmentData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public AssignmentRoleQualificationRequirements$ = this._assignmentRoleQualificationRequirementsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._assignmentRoleQualificationRequirements === null && this._assignmentRoleQualificationRequirementsPromise === null) {
            this.loadAssignmentRoleQualificationRequirements(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public AssignmentRoleQualificationRequirementsCount$ = AssignmentRoleService.Instance.GetAssignmentRolesRowCount({assignmentRoleId: this.id,
      active: true,
      deleted: false
    });



    public RateSheets$ = this._rateSheetsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._rateSheets === null && this._rateSheetsPromise === null) {
            this.loadRateSheets(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public RateSheetsCount$ = AssignmentRoleService.Instance.GetAssignmentRolesRowCount({assignmentRoleId: this.id,
      active: true,
      deleted: false
    });



    public CrewMembers$ = this._crewMembersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._crewMembers === null && this._crewMembersPromise === null) {
            this.loadCrewMembers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public CrewMembersCount$ = AssignmentRoleService.Instance.GetAssignmentRolesRowCount({assignmentRoleId: this.id,
      active: true,
      deleted: false
    });



    public EventResourceAssignments$ = this._eventResourceAssignmentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventResourceAssignments === null && this._eventResourceAssignmentsPromise === null) {
            this.loadEventResourceAssignments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public EventResourceAssignmentsCount$ = AssignmentRoleService.Instance.GetAssignmentRolesRowCount({assignmentRoleId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any AssignmentRoleData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.assignmentRole.Reload();
  //
  //  Non Async:
  //
  //     assignmentRole[0].Reload().then(x => {
  //        this.assignmentRole = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      AssignmentRoleService.Instance.GetAssignmentRole(this.id, includeRelations)
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
     this._assignmentRoleQualificationRequirements = null;
     this._assignmentRoleQualificationRequirementsPromise = null;
     this._assignmentRoleQualificationRequirementsSubject.next(null);

     this._rateSheets = null;
     this._rateSheetsPromise = null;
     this._rateSheetsSubject.next(null);

     this._crewMembers = null;
     this._crewMembersPromise = null;
     this._crewMembersSubject.next(null);

     this._eventResourceAssignments = null;
     this._eventResourceAssignmentsPromise = null;
     this._eventResourceAssignmentsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the AssignmentRoleQualificationRequirements for this AssignmentRole.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.assignmentRole.AssignmentRoleQualificationRequirements.then(assignmentRoleQualificationRequirements => { ... })
     *   or
     *   await this.assignmentRole.AssignmentRoleQualificationRequirements
     *
    */
    public get AssignmentRoleQualificationRequirements(): Promise<AssignmentRoleQualificationRequirementData[]> {
        if (this._assignmentRoleQualificationRequirements !== null) {
            return Promise.resolve(this._assignmentRoleQualificationRequirements);
        }

        if (this._assignmentRoleQualificationRequirementsPromise !== null) {
            return this._assignmentRoleQualificationRequirementsPromise;
        }

        // Start the load
        this.loadAssignmentRoleQualificationRequirements();

        return this._assignmentRoleQualificationRequirementsPromise!;
    }



    private loadAssignmentRoleQualificationRequirements(): void {

        this._assignmentRoleQualificationRequirementsPromise = lastValueFrom(
            AssignmentRoleService.Instance.GetAssignmentRoleQualificationRequirementsForAssignmentRole(this.id)
        )
        .then(assignmentRoleQualificationRequirements => {
            this._assignmentRoleQualificationRequirements = assignmentRoleQualificationRequirements ?? [];
            this._assignmentRoleQualificationRequirementsSubject.next(this._assignmentRoleQualificationRequirements);
            return this._assignmentRoleQualificationRequirements;
         })
        .catch(err => {
            this._assignmentRoleQualificationRequirements = [];
            this._assignmentRoleQualificationRequirementsSubject.next(this._assignmentRoleQualificationRequirements);
            throw err;
        })
        .finally(() => {
            this._assignmentRoleQualificationRequirementsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached AssignmentRoleQualificationRequirement. Call after mutations to force refresh.
     */
    public ClearAssignmentRoleQualificationRequirementsCache(): void {
        this._assignmentRoleQualificationRequirements = null;
        this._assignmentRoleQualificationRequirementsPromise = null;
        this._assignmentRoleQualificationRequirementsSubject.next(this._assignmentRoleQualificationRequirements);      // Emit to observable
    }

    public get HasAssignmentRoleQualificationRequirements(): Promise<boolean> {
        return this.AssignmentRoleQualificationRequirements.then(assignmentRoleQualificationRequirements => assignmentRoleQualificationRequirements.length > 0);
    }


    /**
     *
     * Gets the RateSheets for this AssignmentRole.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.assignmentRole.RateSheets.then(rateSheets => { ... })
     *   or
     *   await this.assignmentRole.RateSheets
     *
    */
    public get RateSheets(): Promise<RateSheetData[]> {
        if (this._rateSheets !== null) {
            return Promise.resolve(this._rateSheets);
        }

        if (this._rateSheetsPromise !== null) {
            return this._rateSheetsPromise;
        }

        // Start the load
        this.loadRateSheets();

        return this._rateSheetsPromise!;
    }



    private loadRateSheets(): void {

        this._rateSheetsPromise = lastValueFrom(
            AssignmentRoleService.Instance.GetRateSheetsForAssignmentRole(this.id)
        )
        .then(rateSheets => {
            this._rateSheets = rateSheets ?? [];
            this._rateSheetsSubject.next(this._rateSheets);
            return this._rateSheets;
         })
        .catch(err => {
            this._rateSheets = [];
            this._rateSheetsSubject.next(this._rateSheets);
            throw err;
        })
        .finally(() => {
            this._rateSheetsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached RateSheet. Call after mutations to force refresh.
     */
    public ClearRateSheetsCache(): void {
        this._rateSheets = null;
        this._rateSheetsPromise = null;
        this._rateSheetsSubject.next(this._rateSheets);      // Emit to observable
    }

    public get HasRateSheets(): Promise<boolean> {
        return this.RateSheets.then(rateSheets => rateSheets.length > 0);
    }


    /**
     *
     * Gets the CrewMembers for this AssignmentRole.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.assignmentRole.CrewMembers.then(crewMembers => { ... })
     *   or
     *   await this.assignmentRole.CrewMembers
     *
    */
    public get CrewMembers(): Promise<CrewMemberData[]> {
        if (this._crewMembers !== null) {
            return Promise.resolve(this._crewMembers);
        }

        if (this._crewMembersPromise !== null) {
            return this._crewMembersPromise;
        }

        // Start the load
        this.loadCrewMembers();

        return this._crewMembersPromise!;
    }



    private loadCrewMembers(): void {

        this._crewMembersPromise = lastValueFrom(
            AssignmentRoleService.Instance.GetCrewMembersForAssignmentRole(this.id)
        )
        .then(crewMembers => {
            this._crewMembers = crewMembers ?? [];
            this._crewMembersSubject.next(this._crewMembers);
            return this._crewMembers;
         })
        .catch(err => {
            this._crewMembers = [];
            this._crewMembersSubject.next(this._crewMembers);
            throw err;
        })
        .finally(() => {
            this._crewMembersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached CrewMember. Call after mutations to force refresh.
     */
    public ClearCrewMembersCache(): void {
        this._crewMembers = null;
        this._crewMembersPromise = null;
        this._crewMembersSubject.next(this._crewMembers);      // Emit to observable
    }

    public get HasCrewMembers(): Promise<boolean> {
        return this.CrewMembers.then(crewMembers => crewMembers.length > 0);
    }


    /**
     *
     * Gets the EventResourceAssignments for this AssignmentRole.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.assignmentRole.EventResourceAssignments.then(eventResourceAssignments => { ... })
     *   or
     *   await this.assignmentRole.EventResourceAssignments
     *
    */
    public get EventResourceAssignments(): Promise<EventResourceAssignmentData[]> {
        if (this._eventResourceAssignments !== null) {
            return Promise.resolve(this._eventResourceAssignments);
        }

        if (this._eventResourceAssignmentsPromise !== null) {
            return this._eventResourceAssignmentsPromise;
        }

        // Start the load
        this.loadEventResourceAssignments();

        return this._eventResourceAssignmentsPromise!;
    }



    private loadEventResourceAssignments(): void {

        this._eventResourceAssignmentsPromise = lastValueFrom(
            AssignmentRoleService.Instance.GetEventResourceAssignmentsForAssignmentRole(this.id)
        )
        .then(eventResourceAssignments => {
            this._eventResourceAssignments = eventResourceAssignments ?? [];
            this._eventResourceAssignmentsSubject.next(this._eventResourceAssignments);
            return this._eventResourceAssignments;
         })
        .catch(err => {
            this._eventResourceAssignments = [];
            this._eventResourceAssignmentsSubject.next(this._eventResourceAssignments);
            throw err;
        })
        .finally(() => {
            this._eventResourceAssignmentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventResourceAssignment. Call after mutations to force refresh.
     */
    public ClearEventResourceAssignmentsCache(): void {
        this._eventResourceAssignments = null;
        this._eventResourceAssignmentsPromise = null;
        this._eventResourceAssignmentsSubject.next(this._eventResourceAssignments);      // Emit to observable
    }

    public get HasEventResourceAssignments(): Promise<boolean> {
        return this.EventResourceAssignments.then(eventResourceAssignments => eventResourceAssignments.length > 0);
    }




    /**
     * Updates the state of this AssignmentRoleData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this AssignmentRoleData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): AssignmentRoleSubmitData {
        return AssignmentRoleService.Instance.ConvertToAssignmentRoleSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class AssignmentRoleService extends SecureEndpointBase {

    private static _instance: AssignmentRoleService;
    private listCache: Map<string, Observable<Array<AssignmentRoleData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<AssignmentRoleBasicListData>>>;
    private recordCache: Map<string, Observable<AssignmentRoleData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private assignmentRoleQualificationRequirementService: AssignmentRoleQualificationRequirementService,
        private rateSheetService: RateSheetService,
        private crewMemberService: CrewMemberService,
        private eventResourceAssignmentService: EventResourceAssignmentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<AssignmentRoleData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<AssignmentRoleBasicListData>>>();
        this.recordCache = new Map<string, Observable<AssignmentRoleData>>();

        AssignmentRoleService._instance = this;
    }

    public static get Instance(): AssignmentRoleService {
      return AssignmentRoleService._instance;
    }


    public ClearListCaches(config: AssignmentRoleQueryParameters | null = null) {

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


    public ConvertToAssignmentRoleSubmitData(data: AssignmentRoleData): AssignmentRoleSubmitData {

        let output = new AssignmentRoleSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.iconId = data.iconId;
        output.color = data.color;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetAssignmentRole(id: bigint | number, includeRelations: boolean = true) : Observable<AssignmentRoleData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const assignmentRole$ = this.requestAssignmentRole(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AssignmentRole", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, assignmentRole$);

            return assignmentRole$;
        }

        return this.recordCache.get(configHash) as Observable<AssignmentRoleData>;
    }

    private requestAssignmentRole(id: bigint | number, includeRelations: boolean = true) : Observable<AssignmentRoleData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<AssignmentRoleData>(this.baseUrl + 'api/AssignmentRole/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveAssignmentRole(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestAssignmentRole(id, includeRelations));
            }));
    }

    public GetAssignmentRoleList(config: AssignmentRoleQueryParameters | any = null) : Observable<Array<AssignmentRoleData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const assignmentRoleList$ = this.requestAssignmentRoleList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AssignmentRole list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, assignmentRoleList$);

            return assignmentRoleList$;
        }

        return this.listCache.get(configHash) as Observable<Array<AssignmentRoleData>>;
    }


    private requestAssignmentRoleList(config: AssignmentRoleQueryParameters | any) : Observable <Array<AssignmentRoleData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AssignmentRoleData>>(this.baseUrl + 'api/AssignmentRoles', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveAssignmentRoleList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestAssignmentRoleList(config));
            }));
    }

    public GetAssignmentRolesRowCount(config: AssignmentRoleQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const assignmentRolesRowCount$ = this.requestAssignmentRolesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get AssignmentRoles row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, assignmentRolesRowCount$);

            return assignmentRolesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestAssignmentRolesRowCount(config: AssignmentRoleQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/AssignmentRoles/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAssignmentRolesRowCount(config));
            }));
    }

    public GetAssignmentRolesBasicListData(config: AssignmentRoleQueryParameters | any = null) : Observable<Array<AssignmentRoleBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const assignmentRolesBasicListData$ = this.requestAssignmentRolesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get AssignmentRoles basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, assignmentRolesBasicListData$);

            return assignmentRolesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<AssignmentRoleBasicListData>>;
    }


    private requestAssignmentRolesBasicListData(config: AssignmentRoleQueryParameters | any) : Observable<Array<AssignmentRoleBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<AssignmentRoleBasicListData>>(this.baseUrl + 'api/AssignmentRoles/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestAssignmentRolesBasicListData(config));
            }));

    }


    public PutAssignmentRole(id: bigint | number, assignmentRole: AssignmentRoleSubmitData) : Observable<AssignmentRoleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<AssignmentRoleData>(this.baseUrl + 'api/AssignmentRole/' + id.toString(), assignmentRole, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAssignmentRole(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutAssignmentRole(id, assignmentRole));
            }));
    }


    public PostAssignmentRole(assignmentRole: AssignmentRoleSubmitData) : Observable<AssignmentRoleData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<AssignmentRoleData>(this.baseUrl + 'api/AssignmentRole', assignmentRole, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveAssignmentRole(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostAssignmentRole(assignmentRole));
            }));
    }

  
    public DeleteAssignmentRole(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/AssignmentRole/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteAssignmentRole(id));
            }));
    }


    private getConfigHash(config: AssignmentRoleQueryParameters | any): string {

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

    public userIsSchedulerAssignmentRoleReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerAssignmentRoleReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.AssignmentRoles
        //
        if (userIsSchedulerAssignmentRoleReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerAssignmentRoleReader = user.readPermission >= 1;
            } else {
                userIsSchedulerAssignmentRoleReader = false;
            }
        }

        return userIsSchedulerAssignmentRoleReader;
    }


    public userIsSchedulerAssignmentRoleWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerAssignmentRoleWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.AssignmentRoles
        //
        if (userIsSchedulerAssignmentRoleWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerAssignmentRoleWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerAssignmentRoleWriter = false;
          }      
        }

        return userIsSchedulerAssignmentRoleWriter;
    }

    public GetAssignmentRoleQualificationRequirementsForAssignmentRole(assignmentRoleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AssignmentRoleQualificationRequirementData[]> {
        return this.assignmentRoleQualificationRequirementService.GetAssignmentRoleQualificationRequirementList({
            assignmentRoleId: assignmentRoleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetRateSheetsForAssignmentRole(assignmentRoleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<RateSheetData[]> {
        return this.rateSheetService.GetRateSheetList({
            assignmentRoleId: assignmentRoleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetCrewMembersForAssignmentRole(assignmentRoleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CrewMemberData[]> {
        return this.crewMemberService.GetCrewMemberList({
            assignmentRoleId: assignmentRoleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventResourceAssignmentsForAssignmentRole(assignmentRoleId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventResourceAssignmentData[]> {
        return this.eventResourceAssignmentService.GetEventResourceAssignmentList({
            assignmentRoleId: assignmentRoleId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full AssignmentRoleData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the AssignmentRoleData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when AssignmentRoleTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveAssignmentRole(raw: any): AssignmentRoleData {
    if (!raw) return raw;

    //
    // Create a AssignmentRoleData object instance with correct prototype
    //
    const revived = Object.create(AssignmentRoleData.prototype) as AssignmentRoleData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._assignmentRoleQualificationRequirements = null;
    (revived as any)._assignmentRoleQualificationRequirementsPromise = null;
    (revived as any)._assignmentRoleQualificationRequirementsSubject = new BehaviorSubject<AssignmentRoleQualificationRequirementData[] | null>(null);

    (revived as any)._rateSheets = null;
    (revived as any)._rateSheetsPromise = null;
    (revived as any)._rateSheetsSubject = new BehaviorSubject<RateSheetData[] | null>(null);

    (revived as any)._crewMembers = null;
    (revived as any)._crewMembersPromise = null;
    (revived as any)._crewMembersSubject = new BehaviorSubject<CrewMemberData[] | null>(null);

    (revived as any)._eventResourceAssignments = null;
    (revived as any)._eventResourceAssignmentsPromise = null;
    (revived as any)._eventResourceAssignmentsSubject = new BehaviorSubject<EventResourceAssignmentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadAssignmentRoleXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).AssignmentRoleQualificationRequirements$ = (revived as any)._assignmentRoleQualificationRequirementsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._assignmentRoleQualificationRequirements === null && (revived as any)._assignmentRoleQualificationRequirementsPromise === null) {
                (revived as any).loadAssignmentRoleQualificationRequirements();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).AssignmentRoleQualificationRequirementsCount$ = AssignmentRoleQualificationRequirementService.Instance.GetAssignmentRoleQualificationRequirementsRowCount({assignmentRoleId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).RateSheets$ = (revived as any)._rateSheetsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._rateSheets === null && (revived as any)._rateSheetsPromise === null) {
                (revived as any).loadRateSheets();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).RateSheetsCount$ = RateSheetService.Instance.GetRateSheetsRowCount({assignmentRoleId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).CrewMembers$ = (revived as any)._crewMembersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._crewMembers === null && (revived as any)._crewMembersPromise === null) {
                (revived as any).loadCrewMembers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).CrewMembersCount$ = CrewMemberService.Instance.GetCrewMembersRowCount({assignmentRoleId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).EventResourceAssignments$ = (revived as any)._eventResourceAssignmentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventResourceAssignments === null && (revived as any)._eventResourceAssignmentsPromise === null) {
                (revived as any).loadEventResourceAssignments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).EventResourceAssignmentsCount$ = EventResourceAssignmentService.Instance.GetEventResourceAssignmentsRowCount({assignmentRoleId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveAssignmentRoleList(rawList: any[]): AssignmentRoleData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveAssignmentRole(raw));
  }

}
