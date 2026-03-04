/*

   GENERATED SERVICE FOR THE CONTACTCONTACT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ContactContact table.

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
import { RelationshipTypeData } from './relationship-type.service';
import { ContactContactChangeHistoryService, ContactContactChangeHistoryData } from './contact-contact-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ContactContactQueryParameters {
    contactId: bigint | number | null | undefined = null;
    relatedContactId: bigint | number | null | undefined = null;
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
export class ContactContactSubmitData {
    id!: bigint | number;
    contactId!: bigint | number;
    relatedContactId!: bigint | number;
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

export class ContactContactBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ContactContactChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `contactContact.ContactContactChildren$` — use with `| async` in templates
//        • Promise:    `contactContact.ContactContactChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="contactContact.ContactContactChildren$ | async"`), or
//        • Access the promise getter (`contactContact.ContactContactChildren` or `await contactContact.ContactContactChildren`)
//    - Simply reading `contactContact.ContactContactChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await contactContact.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ContactContactData {
    id!: bigint | number;
    contactId!: bigint | number;
    relatedContactId!: bigint | number;
    isPrimary!: boolean;
    relationshipTypeId!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    relationshipType: RelationshipTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    relatedContact: ContactData | null | undefined = null;            // Navigation property with non-standard field name (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _contactContactChangeHistories: ContactContactChangeHistoryData[] | null = null;
    private _contactContactChangeHistoriesPromise: Promise<ContactContactChangeHistoryData[]> | null  = null;
    private _contactContactChangeHistoriesSubject = new BehaviorSubject<ContactContactChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ContactContactData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ContactContactData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ContactContactData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ContactContactChangeHistories$ = this._contactContactChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactContactChangeHistories === null && this._contactContactChangeHistoriesPromise === null) {
            this.loadContactContactChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _contactContactChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ContactContactChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._contactContactChangeHistoriesCount$ === null) {
            this._contactContactChangeHistoriesCount$ = ContactContactChangeHistoryService.Instance.GetContactContactChangeHistoriesRowCount({contactContactId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._contactContactChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ContactContactData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.contactContact.Reload();
  //
  //  Non Async:
  //
  //     contactContact[0].Reload().then(x => {
  //        this.contactContact = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ContactContactService.Instance.GetContactContact(this.id, includeRelations)
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
     this._contactContactChangeHistories = null;
     this._contactContactChangeHistoriesPromise = null;
     this._contactContactChangeHistoriesSubject.next(null);
     this._contactContactChangeHistoriesCount$ = null;

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
     * Gets the ContactContactChangeHistories for this ContactContact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contactContact.ContactContactChangeHistories.then(contactContacts => { ... })
     *   or
     *   await this.contactContact.contactContacts
     *
    */
    public get ContactContactChangeHistories(): Promise<ContactContactChangeHistoryData[]> {
        if (this._contactContactChangeHistories !== null) {
            return Promise.resolve(this._contactContactChangeHistories);
        }

        if (this._contactContactChangeHistoriesPromise !== null) {
            return this._contactContactChangeHistoriesPromise;
        }

        // Start the load
        this.loadContactContactChangeHistories();

        return this._contactContactChangeHistoriesPromise!;
    }



    private loadContactContactChangeHistories(): void {

        this._contactContactChangeHistoriesPromise = lastValueFrom(
            ContactContactService.Instance.GetContactContactChangeHistoriesForContactContact(this.id)
        )
        .then(ContactContactChangeHistories => {
            this._contactContactChangeHistories = ContactContactChangeHistories ?? [];
            this._contactContactChangeHistoriesSubject.next(this._contactContactChangeHistories);
            return this._contactContactChangeHistories;
         })
        .catch(err => {
            this._contactContactChangeHistories = [];
            this._contactContactChangeHistoriesSubject.next(this._contactContactChangeHistories);
            throw err;
        })
        .finally(() => {
            this._contactContactChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContactContactChangeHistory. Call after mutations to force refresh.
     */
    public ClearContactContactChangeHistoriesCache(): void {
        this._contactContactChangeHistories = null;
        this._contactContactChangeHistoriesPromise = null;
        this._contactContactChangeHistoriesSubject.next(this._contactContactChangeHistories);      // Emit to observable
    }

    public get HasContactContactChangeHistories(): Promise<boolean> {
        return this.ContactContactChangeHistories.then(contactContactChangeHistories => contactContactChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (contactContact.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await contactContact.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ContactContactData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ContactContactData>> {
        const info = await lastValueFrom(
            ContactContactService.Instance.GetContactContactChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ContactContactData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ContactContactData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ContactContactSubmitData {
        return ContactContactService.Instance.ConvertToContactContactSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ContactContactService extends SecureEndpointBase {

    private static _instance: ContactContactService;
    private listCache: Map<string, Observable<Array<ContactContactData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ContactContactBasicListData>>>;
    private recordCache: Map<string, Observable<ContactContactData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private contactContactChangeHistoryService: ContactContactChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ContactContactData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ContactContactBasicListData>>>();
        this.recordCache = new Map<string, Observable<ContactContactData>>();

        ContactContactService._instance = this;
    }

    public static get Instance(): ContactContactService {
      return ContactContactService._instance;
    }


    public ClearListCaches(config: ContactContactQueryParameters | null = null) {

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


    public ConvertToContactContactSubmitData(data: ContactContactData): ContactContactSubmitData {

        let output = new ContactContactSubmitData();

        output.id = data.id;
        output.contactId = data.contactId;
        output.relatedContactId = data.relatedContactId;
        output.isPrimary = data.isPrimary;
        output.relationshipTypeId = data.relationshipTypeId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetContactContact(id: bigint | number, includeRelations: boolean = true) : Observable<ContactContactData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const contactContact$ = this.requestContactContact(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContactContact", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, contactContact$);

            return contactContact$;
        }

        return this.recordCache.get(configHash) as Observable<ContactContactData>;
    }

    private requestContactContact(id: bigint | number, includeRelations: boolean = true) : Observable<ContactContactData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContactContactData>(this.baseUrl + 'api/ContactContact/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveContactContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestContactContact(id, includeRelations));
            }));
    }

    public GetContactContactList(config: ContactContactQueryParameters | any = null) : Observable<Array<ContactContactData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const contactContactList$ = this.requestContactContactList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContactContact list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, contactContactList$);

            return contactContactList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ContactContactData>>;
    }


    private requestContactContactList(config: ContactContactQueryParameters | any) : Observable <Array<ContactContactData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContactContactData>>(this.baseUrl + 'api/ContactContacts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveContactContactList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestContactContactList(config));
            }));
    }

    public GetContactContactsRowCount(config: ContactContactQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const contactContactsRowCount$ = this.requestContactContactsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContactContacts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, contactContactsRowCount$);

            return contactContactsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestContactContactsRowCount(config: ContactContactQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ContactContacts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContactContactsRowCount(config));
            }));
    }

    public GetContactContactsBasicListData(config: ContactContactQueryParameters | any = null) : Observable<Array<ContactContactBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const contactContactsBasicListData$ = this.requestContactContactsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContactContacts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, contactContactsBasicListData$);

            return contactContactsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ContactContactBasicListData>>;
    }


    private requestContactContactsBasicListData(config: ContactContactQueryParameters | any) : Observable<Array<ContactContactBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContactContactBasicListData>>(this.baseUrl + 'api/ContactContacts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContactContactsBasicListData(config));
            }));

    }


    public PutContactContact(id: bigint | number, contactContact: ContactContactSubmitData) : Observable<ContactContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ContactContactData>(this.baseUrl + 'api/ContactContact/' + id.toString(), contactContact, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutContactContact(id, contactContact));
            }));
    }


    public PostContactContact(contactContact: ContactContactSubmitData) : Observable<ContactContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ContactContactData>(this.baseUrl + 'api/ContactContact', contactContact, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactContact(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostContactContact(contactContact));
            }));
    }

  
    public DeleteContactContact(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ContactContact/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteContactContact(id));
            }));
    }

    public RollbackContactContact(id: bigint | number, versionNumber: bigint | number) : Observable<ContactContactData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ContactContactData>(this.baseUrl + 'api/ContactContact/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackContactContact(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ContactContact.
     */
    public GetContactContactChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ContactContactData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ContactContactData>>(this.baseUrl + 'api/ContactContact/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetContactContactChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ContactContact.
     */
    public GetContactContactAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ContactContactData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ContactContactData>[]>(this.baseUrl + 'api/ContactContact/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetContactContactAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ContactContact.
     */
    public GetContactContactVersion(id: bigint | number, version: number): Observable<ContactContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContactContactData>(this.baseUrl + 'api/ContactContact/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveContactContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetContactContactVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ContactContact at a specific point in time.
     */
    public GetContactContactStateAtTime(id: bigint | number, time: string): Observable<ContactContactData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContactContactData>(this.baseUrl + 'api/ContactContact/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveContactContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetContactContactStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ContactContactQueryParameters | any): string {

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

    public userIsSchedulerContactContactReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerContactContactReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ContactContacts
        //
        if (userIsSchedulerContactContactReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerContactContactReader = user.readPermission >= 1;
            } else {
                userIsSchedulerContactContactReader = false;
            }
        }

        return userIsSchedulerContactContactReader;
    }


    public userIsSchedulerContactContactWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerContactContactWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ContactContacts
        //
        if (userIsSchedulerContactContactWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerContactContactWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerContactContactWriter = false;
          }      
        }

        return userIsSchedulerContactContactWriter;
    }

    public GetContactContactChangeHistoriesForContactContact(contactContactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactContactChangeHistoryData[]> {
        return this.contactContactChangeHistoryService.GetContactContactChangeHistoryList({
            contactContactId: contactContactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ContactContactData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ContactContactData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ContactContactTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveContactContact(raw: any): ContactContactData {
    if (!raw) return raw;

    //
    // Create a ContactContactData object instance with correct prototype
    //
    const revived = Object.create(ContactContactData.prototype) as ContactContactData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._contactContactChangeHistories = null;
    (revived as any)._contactContactChangeHistoriesPromise = null;
    (revived as any)._contactContactChangeHistoriesSubject = new BehaviorSubject<ContactContactChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadContactContactXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ContactContactChangeHistories$ = (revived as any)._contactContactChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactContactChangeHistories === null && (revived as any)._contactContactChangeHistoriesPromise === null) {
                (revived as any).loadContactContactChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._contactContactChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ContactContactData> | null>(null);

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

  private ReviveContactContactList(rawList: any[]): ContactContactData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveContactContact(raw));
  }

}
