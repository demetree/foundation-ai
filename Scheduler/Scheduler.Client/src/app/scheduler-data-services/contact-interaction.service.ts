/*

   GENERATED SERVICE FOR THE CONTACTINTERACTION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ContactInteraction table.

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
import { ContactData } from './contact.service';
import { InteractionTypeData } from './interaction-type.service';
import { ScheduledEventData } from './scheduled-event.service';
import { PriorityData } from './priority.service';
import { ContactInteractionChangeHistoryService, ContactInteractionChangeHistoryData } from './contact-interaction-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ContactInteractionQueryParameters {
    contactId: bigint | number | null | undefined = null;
    initiatingContactId: bigint | number | null | undefined = null;
    interactionTypeId: bigint | number | null | undefined = null;
    scheduledEventId: bigint | number | null | undefined = null;
    startTime: string | null | undefined = null;        // ISO 8601
    endTime: string | null | undefined = null;        // ISO 8601
    notes: string | null | undefined = null;
    location: string | null | undefined = null;
    priorityId: bigint | number | null | undefined = null;
    externalId: string | null | undefined = null;
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
export class ContactInteractionSubmitData {
    id!: bigint | number;
    contactId!: bigint | number;
    initiatingContactId: bigint | number | null = null;
    interactionTypeId!: bigint | number;
    scheduledEventId: bigint | number | null = null;
    startTime!: string;      // ISO 8601
    endTime: string | null = null;     // ISO 8601
    notes: string | null = null;
    location: string | null = null;
    priorityId: bigint | number | null = null;
    externalId: string | null = null;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}



//
// Version history information returned from version history API endpoints.
// Matches server-side VersionInformation<T> structure.
//
export interface VersionInformation<T> {
    timeStamp: string;           // ISO 8601
    userId: bigint | number;
    userName: string;
    versionNumber: number;
    data: T | null;
}

export class ContactInteractionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ContactInteractionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `contactInteraction.ContactInteractionChildren$` — use with `| async` in templates
//        • Promise:    `contactInteraction.ContactInteractionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="contactInteraction.ContactInteractionChildren$ | async"`), or
//        • Access the promise getter (`contactInteraction.ContactInteractionChildren` or `await contactInteraction.ContactInteractionChildren`)
//    - Simply reading `contactInteraction.ContactInteractionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await contactInteraction.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ContactInteractionData {
    id!: bigint | number;
    contactId!: bigint | number;
    initiatingContactId!: bigint | number;
    interactionTypeId!: bigint | number;
    scheduledEventId!: bigint | number;
    startTime!: string;      // ISO 8601
    endTime!: string | null;   // ISO 8601
    notes!: string | null;
    location!: string | null;
    priorityId!: bigint | number;
    externalId!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    interactionType: InteractionTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    priority: PriorityData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    scheduledEvent: ScheduledEventData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    initiatingContact: ContactData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _contactInteractionChangeHistories: ContactInteractionChangeHistoryData[] | null = null;
    private _contactInteractionChangeHistoriesPromise: Promise<ContactInteractionChangeHistoryData[]> | null  = null;
    private _contactInteractionChangeHistoriesSubject = new BehaviorSubject<ContactInteractionChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ContactInteractionData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ContactInteractionData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ContactInteractionData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ContactInteractionChangeHistories$ = this._contactInteractionChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactInteractionChangeHistories === null && this._contactInteractionChangeHistoriesPromise === null) {
            this.loadContactInteractionChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ContactInteractionChangeHistoriesCount$ = ContactInteractionChangeHistoryService.Instance.GetContactInteractionChangeHistoriesRowCount({contactInteractionId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ContactInteractionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.contactInteraction.Reload();
  //
  //  Non Async:
  //
  //     contactInteraction[0].Reload().then(x => {
  //        this.contactInteraction = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ContactInteractionService.Instance.GetContactInteraction(this.id, includeRelations)
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
     this._contactInteractionChangeHistories = null;
     this._contactInteractionChangeHistoriesPromise = null;
     this._contactInteractionChangeHistoriesSubject.next(null);

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
     * Gets the ContactInteractionChangeHistories for this ContactInteraction.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contactInteraction.ContactInteractionChangeHistories.then(contactInteractions => { ... })
     *   or
     *   await this.contactInteraction.contactInteractions
     *
    */
    public get ContactInteractionChangeHistories(): Promise<ContactInteractionChangeHistoryData[]> {
        if (this._contactInteractionChangeHistories !== null) {
            return Promise.resolve(this._contactInteractionChangeHistories);
        }

        if (this._contactInteractionChangeHistoriesPromise !== null) {
            return this._contactInteractionChangeHistoriesPromise;
        }

        // Start the load
        this.loadContactInteractionChangeHistories();

        return this._contactInteractionChangeHistoriesPromise!;
    }



    private loadContactInteractionChangeHistories(): void {

        this._contactInteractionChangeHistoriesPromise = lastValueFrom(
            ContactInteractionService.Instance.GetContactInteractionChangeHistoriesForContactInteraction(this.id)
        )
        .then(ContactInteractionChangeHistories => {
            this._contactInteractionChangeHistories = ContactInteractionChangeHistories ?? [];
            this._contactInteractionChangeHistoriesSubject.next(this._contactInteractionChangeHistories);
            return this._contactInteractionChangeHistories;
         })
        .catch(err => {
            this._contactInteractionChangeHistories = [];
            this._contactInteractionChangeHistoriesSubject.next(this._contactInteractionChangeHistories);
            throw err;
        })
        .finally(() => {
            this._contactInteractionChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContactInteractionChangeHistory. Call after mutations to force refresh.
     */
    public ClearContactInteractionChangeHistoriesCache(): void {
        this._contactInteractionChangeHistories = null;
        this._contactInteractionChangeHistoriesPromise = null;
        this._contactInteractionChangeHistoriesSubject.next(this._contactInteractionChangeHistories);      // Emit to observable
    }

    public get HasContactInteractionChangeHistories(): Promise<boolean> {
        return this.ContactInteractionChangeHistories.then(contactInteractionChangeHistories => contactInteractionChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (contactInteraction.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await contactInteraction.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ContactInteractionData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ContactInteractionData>> {
        const info = await lastValueFrom(
            ContactInteractionService.Instance.GetContactInteractionChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ContactInteractionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ContactInteractionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ContactInteractionSubmitData {
        return ContactInteractionService.Instance.ConvertToContactInteractionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ContactInteractionService extends SecureEndpointBase {

    private static _instance: ContactInteractionService;
    private listCache: Map<string, Observable<Array<ContactInteractionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ContactInteractionBasicListData>>>;
    private recordCache: Map<string, Observable<ContactInteractionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private contactInteractionChangeHistoryService: ContactInteractionChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ContactInteractionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ContactInteractionBasicListData>>>();
        this.recordCache = new Map<string, Observable<ContactInteractionData>>();

        ContactInteractionService._instance = this;
    }

    public static get Instance(): ContactInteractionService {
      return ContactInteractionService._instance;
    }


    public ClearListCaches(config: ContactInteractionQueryParameters | null = null) {

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


    public ConvertToContactInteractionSubmitData(data: ContactInteractionData): ContactInteractionSubmitData {

        let output = new ContactInteractionSubmitData();

        output.id = data.id;
        output.contactId = data.contactId;
        output.initiatingContactId = data.initiatingContactId;
        output.interactionTypeId = data.interactionTypeId;
        output.scheduledEventId = data.scheduledEventId;
        output.startTime = data.startTime;
        output.endTime = data.endTime;
        output.notes = data.notes;
        output.location = data.location;
        output.priorityId = data.priorityId;
        output.externalId = data.externalId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetContactInteraction(id: bigint | number, includeRelations: boolean = true) : Observable<ContactInteractionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const contactInteraction$ = this.requestContactInteraction(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContactInteraction", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, contactInteraction$);

            return contactInteraction$;
        }

        return this.recordCache.get(configHash) as Observable<ContactInteractionData>;
    }

    private requestContactInteraction(id: bigint | number, includeRelations: boolean = true) : Observable<ContactInteractionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContactInteractionData>(this.baseUrl + 'api/ContactInteraction/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveContactInteraction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestContactInteraction(id, includeRelations));
            }));
    }

    public GetContactInteractionList(config: ContactInteractionQueryParameters | any = null) : Observable<Array<ContactInteractionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const contactInteractionList$ = this.requestContactInteractionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContactInteraction list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, contactInteractionList$);

            return contactInteractionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ContactInteractionData>>;
    }


    private requestContactInteractionList(config: ContactInteractionQueryParameters | any) : Observable <Array<ContactInteractionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContactInteractionData>>(this.baseUrl + 'api/ContactInteractions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveContactInteractionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestContactInteractionList(config));
            }));
    }

    public GetContactInteractionsRowCount(config: ContactInteractionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const contactInteractionsRowCount$ = this.requestContactInteractionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContactInteractions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, contactInteractionsRowCount$);

            return contactInteractionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestContactInteractionsRowCount(config: ContactInteractionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ContactInteractions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContactInteractionsRowCount(config));
            }));
    }

    public GetContactInteractionsBasicListData(config: ContactInteractionQueryParameters | any = null) : Observable<Array<ContactInteractionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const contactInteractionsBasicListData$ = this.requestContactInteractionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContactInteractions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, contactInteractionsBasicListData$);

            return contactInteractionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ContactInteractionBasicListData>>;
    }


    private requestContactInteractionsBasicListData(config: ContactInteractionQueryParameters | any) : Observable<Array<ContactInteractionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContactInteractionBasicListData>>(this.baseUrl + 'api/ContactInteractions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContactInteractionsBasicListData(config));
            }));

    }


    public PutContactInteraction(id: bigint | number, contactInteraction: ContactInteractionSubmitData) : Observable<ContactInteractionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ContactInteractionData>(this.baseUrl + 'api/ContactInteraction/' + id.toString(), contactInteraction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactInteraction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutContactInteraction(id, contactInteraction));
            }));
    }


    public PostContactInteraction(contactInteraction: ContactInteractionSubmitData) : Observable<ContactInteractionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ContactInteractionData>(this.baseUrl + 'api/ContactInteraction', contactInteraction, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactInteraction(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostContactInteraction(contactInteraction));
            }));
    }

  
    public DeleteContactInteraction(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ContactInteraction/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteContactInteraction(id));
            }));
    }

    public RollbackContactInteraction(id: bigint | number, versionNumber: bigint | number) : Observable<ContactInteractionData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ContactInteractionData>(this.baseUrl + 'api/ContactInteraction/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactInteraction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackContactInteraction(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ContactInteraction.
     */
    public GetContactInteractionChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ContactInteractionData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ContactInteractionData>>(this.baseUrl + 'api/ContactInteraction/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetContactInteractionChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ContactInteraction.
     */
    public GetContactInteractionAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ContactInteractionData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ContactInteractionData>[]>(this.baseUrl + 'api/ContactInteraction/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetContactInteractionAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ContactInteraction.
     */
    public GetContactInteractionVersion(id: bigint | number, version: number): Observable<ContactInteractionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContactInteractionData>(this.baseUrl + 'api/ContactInteraction/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveContactInteraction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetContactInteractionVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ContactInteraction at a specific point in time.
     */
    public GetContactInteractionStateAtTime(id: bigint | number, time: string): Observable<ContactInteractionData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContactInteractionData>(this.baseUrl + 'api/ContactInteraction/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveContactInteraction(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetContactInteractionStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ContactInteractionQueryParameters | any): string {

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

    public userIsSchedulerContactInteractionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerContactInteractionReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ContactInteractions
        //
        if (userIsSchedulerContactInteractionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerContactInteractionReader = user.readPermission >= 1;
            } else {
                userIsSchedulerContactInteractionReader = false;
            }
        }

        return userIsSchedulerContactInteractionReader;
    }


    public userIsSchedulerContactInteractionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerContactInteractionWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ContactInteractions
        //
        if (userIsSchedulerContactInteractionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerContactInteractionWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerContactInteractionWriter = false;
          }      
        }

        return userIsSchedulerContactInteractionWriter;
    }

    public GetContactInteractionChangeHistoriesForContactInteraction(contactInteractionId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactInteractionChangeHistoryData[]> {
        return this.contactInteractionChangeHistoryService.GetContactInteractionChangeHistoryList({
            contactInteractionId: contactInteractionId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ContactInteractionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ContactInteractionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ContactInteractionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveContactInteraction(raw: any): ContactInteractionData {
    if (!raw) return raw;

    //
    // Create a ContactInteractionData object instance with correct prototype
    //
    const revived = Object.create(ContactInteractionData.prototype) as ContactInteractionData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._contactInteractionChangeHistories = null;
    (revived as any)._contactInteractionChangeHistoriesPromise = null;
    (revived as any)._contactInteractionChangeHistoriesSubject = new BehaviorSubject<ContactInteractionChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadContactInteractionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ContactInteractionChangeHistories$ = (revived as any)._contactInteractionChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactInteractionChangeHistories === null && (revived as any)._contactInteractionChangeHistoriesPromise === null) {
                (revived as any).loadContactInteractionChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ContactInteractionChangeHistoriesCount$ = ContactInteractionChangeHistoryService.Instance.GetContactInteractionChangeHistoriesRowCount({contactInteractionId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveContactInteractionList(rawList: any[]): ContactInteractionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveContactInteraction(raw));
  }

}
