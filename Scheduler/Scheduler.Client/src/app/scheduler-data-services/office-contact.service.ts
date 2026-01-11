/*

   GENERATED SERVICE FOR THE OFFICECONTACT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the OfficeContact table.

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
import { OfficeData } from './office.service';
import { ContactData } from './contact.service';
import { RelationshipTypeData } from './relationship-type.service';
import { OfficeContactChangeHistoryService, OfficeContactChangeHistoryData } from './office-contact-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class OfficeContactQueryParameters {
    officeId: bigint | number | null | undefined = null;
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
export class OfficeContactSubmitData {
    id!: bigint | number;
    officeId!: bigint | number;
    contactId!: bigint | number;
    isPrimary!: boolean;
    relationshipTypeId!: bigint | number;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class OfficeContactBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. OfficeContactChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `officeContact.OfficeContactChildren$` — use with `| async` in templates
//        • Promise:    `officeContact.OfficeContactChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="officeContact.OfficeContactChildren$ | async"`), or
//        • Access the promise getter (`officeContact.OfficeContactChildren` or `await officeContact.OfficeContactChildren`)
//    - Simply reading `officeContact.OfficeContactChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await officeContact.Reload()` to refresh the entire object and clear all lazy caches.
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
export class OfficeContactData {
    id!: bigint | number;
    officeId!: bigint | number;
    contactId!: bigint | number;
    isPrimary!: boolean;
    relationshipTypeId!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    office: OfficeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    relationshipType: RelationshipTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _officeContactChangeHistories: OfficeContactChangeHistoryData[] | null = null;
    private _officeContactChangeHistoriesPromise: Promise<OfficeContactChangeHistoryData[]> | null  = null;
    private _officeContactChangeHistoriesSubject = new BehaviorSubject<OfficeContactChangeHistoryData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public OfficeContactChangeHistories$ = this._officeContactChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._officeContactChangeHistories === null && this._officeContactChangeHistoriesPromise === null) {
            this.loadOfficeContactChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public OfficeContactChangeHistoriesCount$ = OfficeContactService.Instance.GetOfficeContactsRowCount({officeContactId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any OfficeContactData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.officeContact.Reload();
  //
  //  Non Async:
  //
  //     officeContact[0].Reload().then(x => {
  //        this.officeContact = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      OfficeContactService.Instance.GetOfficeContact(this.id, includeRelations)
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
     this._officeContactChangeHistories = null;
     this._officeContactChangeHistoriesPromise = null;
     this._officeContactChangeHistoriesSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the OfficeContactChangeHistories for this OfficeContact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.officeContact.OfficeContactChangeHistories.then(officeContactChangeHistories => { ... })
     *   or
     *   await this.officeContact.OfficeContactChangeHistories
     *
    */
    public get OfficeContactChangeHistories(): Promise<OfficeContactChangeHistoryData[]> {
        if (this._officeContactChangeHistories !== null) {
            return Promise.resolve(this._officeContactChangeHistories);
        }

        if (this._officeContactChangeHistoriesPromise !== null) {
            return this._officeContactChangeHistoriesPromise;
        }

        // Start the load
        this.loadOfficeContactChangeHistories();

        return this._officeContactChangeHistoriesPromise!;
    }



    private loadOfficeContactChangeHistories(): void {

        this._officeContactChangeHistoriesPromise = lastValueFrom(
            OfficeContactService.Instance.GetOfficeContactChangeHistoriesForOfficeContact(this.id)
        )
        .then(officeContactChangeHistories => {
            this._officeContactChangeHistories = officeContactChangeHistories ?? [];
            this._officeContactChangeHistoriesSubject.next(this._officeContactChangeHistories);
            return this._officeContactChangeHistories;
         })
        .catch(err => {
            this._officeContactChangeHistories = [];
            this._officeContactChangeHistoriesSubject.next(this._officeContactChangeHistories);
            throw err;
        })
        .finally(() => {
            this._officeContactChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached OfficeContactChangeHistory. Call after mutations to force refresh.
     */
    public ClearOfficeContactChangeHistoriesCache(): void {
        this._officeContactChangeHistories = null;
        this._officeContactChangeHistoriesPromise = null;
        this._officeContactChangeHistoriesSubject.next(this._officeContactChangeHistories);      // Emit to observable
    }

    public get HasOfficeContactChangeHistories(): Promise<boolean> {
        return this.OfficeContactChangeHistories.then(officeContactChangeHistories => officeContactChangeHistories.length > 0);
    }




    /**
     * Updates the state of this OfficeContactData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this OfficeContactData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): OfficeContactSubmitData {
        return OfficeContactService.Instance.ConvertToOfficeContactSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class OfficeContactService extends SecureEndpointBase {

    private static _instance: OfficeContactService;
    private listCache: Map<string, Observable<Array<OfficeContactData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<OfficeContactBasicListData>>>;
    private recordCache: Map<string, Observable<OfficeContactData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private officeContactChangeHistoryService: OfficeContactChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<OfficeContactData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<OfficeContactBasicListData>>>();
        this.recordCache = new Map<string, Observable<OfficeContactData>>();

        OfficeContactService._instance = this;
    }

    public static get Instance(): OfficeContactService {
      return OfficeContactService._instance;
    }


    public ClearListCaches(config: OfficeContactQueryParameters | null = null) {

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


    public ConvertToOfficeContactSubmitData(data: OfficeContactData): OfficeContactSubmitData {

        let output = new OfficeContactSubmitData();

        output.id = data.id;
        output.officeId = data.officeId;
        output.contactId = data.contactId;
        output.isPrimary = data.isPrimary;
        output.relationshipTypeId = data.relationshipTypeId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetOfficeContact(id: bigint | number, includeRelations: boolean = true) : Observable<OfficeContactData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const officeContact$ = this.requestOfficeContact(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get OfficeContact", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, officeContact$);

            return officeContact$;
        }

        return this.recordCache.get(configHash) as Observable<OfficeContactData>;
    }

    private requestOfficeContact(id: bigint | number, includeRelations: boolean = true) : Observable<OfficeContactData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<OfficeContactData>(this.baseUrl + 'api/OfficeContact/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveOfficeContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeContact(id, includeRelations));
            }));
    }

    public GetOfficeContactList(config: OfficeContactQueryParameters | any = null) : Observable<Array<OfficeContactData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const officeContactList$ = this.requestOfficeContactList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get OfficeContact list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, officeContactList$);

            return officeContactList$;
        }

        return this.listCache.get(configHash) as Observable<Array<OfficeContactData>>;
    }


    private requestOfficeContactList(config: OfficeContactQueryParameters | any) : Observable <Array<OfficeContactData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<OfficeContactData>>(this.baseUrl + 'api/OfficeContacts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveOfficeContactList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeContactList(config));
            }));
    }

    public GetOfficeContactsRowCount(config: OfficeContactQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const officeContactsRowCount$ = this.requestOfficeContactsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get OfficeContacts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, officeContactsRowCount$);

            return officeContactsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestOfficeContactsRowCount(config: OfficeContactQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/OfficeContacts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeContactsRowCount(config));
            }));
    }

    public GetOfficeContactsBasicListData(config: OfficeContactQueryParameters | any = null) : Observable<Array<OfficeContactBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const officeContactsBasicListData$ = this.requestOfficeContactsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get OfficeContacts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, officeContactsBasicListData$);

            return officeContactsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<OfficeContactBasicListData>>;
    }


    private requestOfficeContactsBasicListData(config: OfficeContactQueryParameters | any) : Observable<Array<OfficeContactBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<OfficeContactBasicListData>>(this.baseUrl + 'api/OfficeContacts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestOfficeContactsBasicListData(config));
            }));

    }


    public PutOfficeContact(id: bigint | number, officeContact: OfficeContactSubmitData) : Observable<OfficeContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<OfficeContactData>(this.baseUrl + 'api/OfficeContact/' + id.toString(), officeContact, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOfficeContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutOfficeContact(id, officeContact));
            }));
    }


    public PostOfficeContact(officeContact: OfficeContactSubmitData) : Observable<OfficeContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<OfficeContactData>(this.baseUrl + 'api/OfficeContact', officeContact, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOfficeContact(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostOfficeContact(officeContact));
            }));
    }

  
    public DeleteOfficeContact(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/OfficeContact/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteOfficeContact(id));
            }));
    }

    public RollbackOfficeContact(id: bigint | number, versionNumber: bigint | number) : Observable<OfficeContactData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<OfficeContactData>(this.baseUrl + 'api/OfficeContact/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveOfficeContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackOfficeContact(id, versionNumber));
        }));
    }

    private getConfigHash(config: OfficeContactQueryParameters | any): string {

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

    public userIsSchedulerOfficeContactReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerOfficeContactReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.OfficeContacts
        //
        if (userIsSchedulerOfficeContactReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerOfficeContactReader = user.readPermission >= 1;
            } else {
                userIsSchedulerOfficeContactReader = false;
            }
        }

        return userIsSchedulerOfficeContactReader;
    }


    public userIsSchedulerOfficeContactWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerOfficeContactWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.OfficeContacts
        //
        if (userIsSchedulerOfficeContactWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerOfficeContactWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerOfficeContactWriter = false;
          }      
        }

        return userIsSchedulerOfficeContactWriter;
    }

    public GetOfficeContactChangeHistoriesForOfficeContact(officeContactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<OfficeContactChangeHistoryData[]> {
        return this.officeContactChangeHistoryService.GetOfficeContactChangeHistoryList({
            officeContactId: officeContactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full OfficeContactData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the OfficeContactData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when OfficeContactTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveOfficeContact(raw: any): OfficeContactData {
    if (!raw) return raw;

    //
    // Create a OfficeContactData object instance with correct prototype
    //
    const revived = Object.create(OfficeContactData.prototype) as OfficeContactData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._officeContactChangeHistories = null;
    (revived as any)._officeContactChangeHistoriesPromise = null;
    (revived as any)._officeContactChangeHistoriesSubject = new BehaviorSubject<OfficeContactChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadOfficeContactXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).OfficeContactChangeHistories$ = (revived as any)._officeContactChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._officeContactChangeHistories === null && (revived as any)._officeContactChangeHistoriesPromise === null) {
                (revived as any).loadOfficeContactChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).OfficeContactChangeHistoriesCount$ = OfficeContactChangeHistoryService.Instance.GetOfficeContactChangeHistoriesRowCount({officeContactId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveOfficeContactList(rawList: any[]): OfficeContactData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveOfficeContact(raw));
  }

}
