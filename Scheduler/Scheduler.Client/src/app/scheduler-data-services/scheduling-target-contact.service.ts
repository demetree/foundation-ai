/*

   GENERATED SERVICE FOR THE SCHEDULINGTARGETCONTACT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the SchedulingTargetContact table.

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
import { SchedulingTargetData } from './scheduling-target.service';
import { ContactData } from './contact.service';
import { RelationshipTypeData } from './relationship-type.service';
import { SchedulingTargetContactChangeHistoryService, SchedulingTargetContactChangeHistoryData } from './scheduling-target-contact-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SchedulingTargetContactQueryParameters {
    schedulingTargetId: bigint | number | null | undefined = null;
    contactId: bigint | number | null | undefined = null;
    isPrimary: boolean | null | undefined = null;
    relationshipTypeId: bigint | number | null | undefined = null;
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
export class SchedulingTargetContactSubmitData {
    id!: bigint | number;
    schedulingTargetId!: bigint | number;
    contactId!: bigint | number;
    isPrimary!: boolean;
    relationshipTypeId!: bigint | number;
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

export class SchedulingTargetContactBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SchedulingTargetContactChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `schedulingTargetContact.SchedulingTargetContactChildren$` — use with `| async` in templates
//        • Promise:    `schedulingTargetContact.SchedulingTargetContactChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="schedulingTargetContact.SchedulingTargetContactChildren$ | async"`), or
//        • Access the promise getter (`schedulingTargetContact.SchedulingTargetContactChildren` or `await schedulingTargetContact.SchedulingTargetContactChildren`)
//    - Simply reading `schedulingTargetContact.SchedulingTargetContactChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await schedulingTargetContact.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SchedulingTargetContactData {
    id!: bigint | number;
    schedulingTargetId!: bigint | number;
    contactId!: bigint | number;
    isPrimary!: boolean;
    relationshipTypeId!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    relationshipType: RelationshipTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    schedulingTarget: SchedulingTargetData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _schedulingTargetContactChangeHistories: SchedulingTargetContactChangeHistoryData[] | null = null;
    private _schedulingTargetContactChangeHistoriesPromise: Promise<SchedulingTargetContactChangeHistoryData[]> | null  = null;
    private _schedulingTargetContactChangeHistoriesSubject = new BehaviorSubject<SchedulingTargetContactChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<SchedulingTargetContactData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<SchedulingTargetContactData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SchedulingTargetContactData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SchedulingTargetContactChangeHistories$ = this._schedulingTargetContactChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetContactChangeHistories === null && this._schedulingTargetContactChangeHistoriesPromise === null) {
            this.loadSchedulingTargetContactChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SchedulingTargetContactChangeHistoriesCount$ = SchedulingTargetContactChangeHistoryService.Instance.GetSchedulingTargetContactChangeHistoriesRowCount({schedulingTargetContactId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SchedulingTargetContactData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.schedulingTargetContact.Reload();
  //
  //  Non Async:
  //
  //     schedulingTargetContact[0].Reload().then(x => {
  //        this.schedulingTargetContact = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SchedulingTargetContactService.Instance.GetSchedulingTargetContact(this.id, includeRelations)
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
     this._schedulingTargetContactChangeHistories = null;
     this._schedulingTargetContactChangeHistoriesPromise = null;
     this._schedulingTargetContactChangeHistoriesSubject.next(null);

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
     * Gets the SchedulingTargetContactChangeHistories for this SchedulingTargetContact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.schedulingTargetContact.SchedulingTargetContactChangeHistories.then(schedulingTargetContacts => { ... })
     *   or
     *   await this.schedulingTargetContact.schedulingTargetContacts
     *
    */
    public get SchedulingTargetContactChangeHistories(): Promise<SchedulingTargetContactChangeHistoryData[]> {
        if (this._schedulingTargetContactChangeHistories !== null) {
            return Promise.resolve(this._schedulingTargetContactChangeHistories);
        }

        if (this._schedulingTargetContactChangeHistoriesPromise !== null) {
            return this._schedulingTargetContactChangeHistoriesPromise;
        }

        // Start the load
        this.loadSchedulingTargetContactChangeHistories();

        return this._schedulingTargetContactChangeHistoriesPromise!;
    }



    private loadSchedulingTargetContactChangeHistories(): void {

        this._schedulingTargetContactChangeHistoriesPromise = lastValueFrom(
            SchedulingTargetContactService.Instance.GetSchedulingTargetContactChangeHistoriesForSchedulingTargetContact(this.id)
        )
        .then(SchedulingTargetContactChangeHistories => {
            this._schedulingTargetContactChangeHistories = SchedulingTargetContactChangeHistories ?? [];
            this._schedulingTargetContactChangeHistoriesSubject.next(this._schedulingTargetContactChangeHistories);
            return this._schedulingTargetContactChangeHistories;
         })
        .catch(err => {
            this._schedulingTargetContactChangeHistories = [];
            this._schedulingTargetContactChangeHistoriesSubject.next(this._schedulingTargetContactChangeHistories);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetContactChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTargetContactChangeHistory. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetContactChangeHistoriesCache(): void {
        this._schedulingTargetContactChangeHistories = null;
        this._schedulingTargetContactChangeHistoriesPromise = null;
        this._schedulingTargetContactChangeHistoriesSubject.next(this._schedulingTargetContactChangeHistories);      // Emit to observable
    }

    public get HasSchedulingTargetContactChangeHistories(): Promise<boolean> {
        return this.SchedulingTargetContactChangeHistories.then(schedulingTargetContactChangeHistories => schedulingTargetContactChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (schedulingTargetContact.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await schedulingTargetContact.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<SchedulingTargetContactData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<SchedulingTargetContactData>> {
        const info = await lastValueFrom(
            SchedulingTargetContactService.Instance.GetSchedulingTargetContactChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this SchedulingTargetContactData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SchedulingTargetContactData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SchedulingTargetContactSubmitData {
        return SchedulingTargetContactService.Instance.ConvertToSchedulingTargetContactSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SchedulingTargetContactService extends SecureEndpointBase {

    private static _instance: SchedulingTargetContactService;
    private listCache: Map<string, Observable<Array<SchedulingTargetContactData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SchedulingTargetContactBasicListData>>>;
    private recordCache: Map<string, Observable<SchedulingTargetContactData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private schedulingTargetContactChangeHistoryService: SchedulingTargetContactChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SchedulingTargetContactData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SchedulingTargetContactBasicListData>>>();
        this.recordCache = new Map<string, Observable<SchedulingTargetContactData>>();

        SchedulingTargetContactService._instance = this;
    }

    public static get Instance(): SchedulingTargetContactService {
      return SchedulingTargetContactService._instance;
    }


    public ClearListCaches(config: SchedulingTargetContactQueryParameters | null = null) {

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


    public ConvertToSchedulingTargetContactSubmitData(data: SchedulingTargetContactData): SchedulingTargetContactSubmitData {

        let output = new SchedulingTargetContactSubmitData();

        output.id = data.id;
        output.schedulingTargetId = data.schedulingTargetId;
        output.contactId = data.contactId;
        output.isPrimary = data.isPrimary;
        output.relationshipTypeId = data.relationshipTypeId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSchedulingTargetContact(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetContactData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const schedulingTargetContact$ = this.requestSchedulingTargetContact(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetContact", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, schedulingTargetContact$);

            return schedulingTargetContact$;
        }

        return this.recordCache.get(configHash) as Observable<SchedulingTargetContactData>;
    }

    private requestSchedulingTargetContact(id: bigint | number, includeRelations: boolean = true) : Observable<SchedulingTargetContactData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetContactData>(this.baseUrl + 'api/SchedulingTargetContact/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSchedulingTargetContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetContact(id, includeRelations));
            }));
    }

    public GetSchedulingTargetContactList(config: SchedulingTargetContactQueryParameters | any = null) : Observable<Array<SchedulingTargetContactData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const schedulingTargetContactList$ = this.requestSchedulingTargetContactList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetContact list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, schedulingTargetContactList$);

            return schedulingTargetContactList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SchedulingTargetContactData>>;
    }


    private requestSchedulingTargetContactList(config: SchedulingTargetContactQueryParameters | any) : Observable <Array<SchedulingTargetContactData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetContactData>>(this.baseUrl + 'api/SchedulingTargetContacts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSchedulingTargetContactList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetContactList(config));
            }));
    }

    public GetSchedulingTargetContactsRowCount(config: SchedulingTargetContactQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const schedulingTargetContactsRowCount$ = this.requestSchedulingTargetContactsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetContacts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, schedulingTargetContactsRowCount$);

            return schedulingTargetContactsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSchedulingTargetContactsRowCount(config: SchedulingTargetContactQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SchedulingTargetContacts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetContactsRowCount(config));
            }));
    }

    public GetSchedulingTargetContactsBasicListData(config: SchedulingTargetContactQueryParameters | any = null) : Observable<Array<SchedulingTargetContactBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const schedulingTargetContactsBasicListData$ = this.requestSchedulingTargetContactsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SchedulingTargetContacts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, schedulingTargetContactsBasicListData$);

            return schedulingTargetContactsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SchedulingTargetContactBasicListData>>;
    }


    private requestSchedulingTargetContactsBasicListData(config: SchedulingTargetContactQueryParameters | any) : Observable<Array<SchedulingTargetContactBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SchedulingTargetContactBasicListData>>(this.baseUrl + 'api/SchedulingTargetContacts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSchedulingTargetContactsBasicListData(config));
            }));

    }


    public PutSchedulingTargetContact(id: bigint | number, schedulingTargetContact: SchedulingTargetContactSubmitData) : Observable<SchedulingTargetContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SchedulingTargetContactData>(this.baseUrl + 'api/SchedulingTargetContact/' + id.toString(), schedulingTargetContact, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSchedulingTargetContact(id, schedulingTargetContact));
            }));
    }


    public PostSchedulingTargetContact(schedulingTargetContact: SchedulingTargetContactSubmitData) : Observable<SchedulingTargetContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SchedulingTargetContactData>(this.baseUrl + 'api/SchedulingTargetContact', schedulingTargetContact, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetContact(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSchedulingTargetContact(schedulingTargetContact));
            }));
    }

  
    public DeleteSchedulingTargetContact(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SchedulingTargetContact/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSchedulingTargetContact(id));
            }));
    }

    public RollbackSchedulingTargetContact(id: bigint | number, versionNumber: bigint | number) : Observable<SchedulingTargetContactData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SchedulingTargetContactData>(this.baseUrl + 'api/SchedulingTargetContact/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSchedulingTargetContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackSchedulingTargetContact(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a SchedulingTargetContact.
     */
    public GetSchedulingTargetContactChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<SchedulingTargetContactData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SchedulingTargetContactData>>(this.baseUrl + 'api/SchedulingTargetContact/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSchedulingTargetContactChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a SchedulingTargetContact.
     */
    public GetSchedulingTargetContactAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<SchedulingTargetContactData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<SchedulingTargetContactData>[]>(this.baseUrl + 'api/SchedulingTargetContact/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetSchedulingTargetContactAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a SchedulingTargetContact.
     */
    public GetSchedulingTargetContactVersion(id: bigint | number, version: number): Observable<SchedulingTargetContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetContactData>(this.baseUrl + 'api/SchedulingTargetContact/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSchedulingTargetContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSchedulingTargetContactVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a SchedulingTargetContact at a specific point in time.
     */
    public GetSchedulingTargetContactStateAtTime(id: bigint | number, time: string): Observable<SchedulingTargetContactData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SchedulingTargetContactData>(this.baseUrl + 'api/SchedulingTargetContact/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveSchedulingTargetContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetSchedulingTargetContactStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: SchedulingTargetContactQueryParameters | any): string {

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

    public userIsSchedulerSchedulingTargetContactReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerSchedulingTargetContactReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.SchedulingTargetContacts
        //
        if (userIsSchedulerSchedulingTargetContactReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerSchedulingTargetContactReader = user.readPermission >= 1;
            } else {
                userIsSchedulerSchedulingTargetContactReader = false;
            }
        }

        return userIsSchedulerSchedulingTargetContactReader;
    }


    public userIsSchedulerSchedulingTargetContactWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerSchedulingTargetContactWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.SchedulingTargetContacts
        //
        if (userIsSchedulerSchedulingTargetContactWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerSchedulingTargetContactWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerSchedulingTargetContactWriter = false;
          }      
        }

        return userIsSchedulerSchedulingTargetContactWriter;
    }

    public GetSchedulingTargetContactChangeHistoriesForSchedulingTargetContact(schedulingTargetContactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetContactChangeHistoryData[]> {
        return this.schedulingTargetContactChangeHistoryService.GetSchedulingTargetContactChangeHistoryList({
            schedulingTargetContactId: schedulingTargetContactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SchedulingTargetContactData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SchedulingTargetContactData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SchedulingTargetContactTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSchedulingTargetContact(raw: any): SchedulingTargetContactData {
    if (!raw) return raw;

    //
    // Create a SchedulingTargetContactData object instance with correct prototype
    //
    const revived = Object.create(SchedulingTargetContactData.prototype) as SchedulingTargetContactData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._schedulingTargetContactChangeHistories = null;
    (revived as any)._schedulingTargetContactChangeHistoriesPromise = null;
    (revived as any)._schedulingTargetContactChangeHistoriesSubject = new BehaviorSubject<SchedulingTargetContactChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSchedulingTargetContactXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SchedulingTargetContactChangeHistories$ = (revived as any)._schedulingTargetContactChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetContactChangeHistories === null && (revived as any)._schedulingTargetContactChangeHistoriesPromise === null) {
                (revived as any).loadSchedulingTargetContactChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SchedulingTargetContactChangeHistoriesCount$ = SchedulingTargetContactChangeHistoryService.Instance.GetSchedulingTargetContactChangeHistoriesRowCount({schedulingTargetContactId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<SchedulingTargetContactData> | null>(null);

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

  private ReviveSchedulingTargetContactList(rawList: any[]): SchedulingTargetContactData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSchedulingTargetContact(raw));
  }

}
