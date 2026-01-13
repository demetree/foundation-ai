/*

   GENERATED SERVICE FOR THE CONTACTTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ContactType table.

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
import { ContactService, ContactData } from './contact.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ContactTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class ContactTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class ContactTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ContactTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `contactType.ContactTypeChildren$` — use with `| async` in templates
//        • Promise:    `contactType.ContactTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="contactType.ContactTypeChildren$ | async"`), or
//        • Access the promise getter (`contactType.ContactTypeChildren` or `await contactType.ContactTypeChildren`)
//    - Simply reading `contactType.ContactTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await contactType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ContactTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence!: bigint | number;
    iconId!: bigint | number;
    color!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _contacts: ContactData[] | null = null;
    private _contactsPromise: Promise<ContactData[]> | null  = null;
    private _contactsSubject = new BehaviorSubject<ContactData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Contacts$ = this._contactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contacts === null && this._contactsPromise === null) {
            this.loadContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ContactsCount$ = ContactService.Instance.GetContactsRowCount({contactTypeId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ContactTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.contactType.Reload();
  //
  //  Non Async:
  //
  //     contactType[0].Reload().then(x => {
  //        this.contactType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ContactTypeService.Instance.GetContactType(this.id, includeRelations)
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
     this._contacts = null;
     this._contactsPromise = null;
     this._contactsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Contacts for this ContactType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contactType.Contacts.then(contactTypes => { ... })
     *   or
     *   await this.contactType.contactTypes
     *
    */
    public get Contacts(): Promise<ContactData[]> {
        if (this._contacts !== null) {
            return Promise.resolve(this._contacts);
        }

        if (this._contactsPromise !== null) {
            return this._contactsPromise;
        }

        // Start the load
        this.loadContacts();

        return this._contactsPromise!;
    }



    private loadContacts(): void {

        this._contactsPromise = lastValueFrom(
            ContactTypeService.Instance.GetContactsForContactType(this.id)
        )
        .then(Contacts => {
            this._contacts = Contacts ?? [];
            this._contactsSubject.next(this._contacts);
            return this._contacts;
         })
        .catch(err => {
            this._contacts = [];
            this._contactsSubject.next(this._contacts);
            throw err;
        })
        .finally(() => {
            this._contactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Contact. Call after mutations to force refresh.
     */
    public ClearContactsCache(): void {
        this._contacts = null;
        this._contactsPromise = null;
        this._contactsSubject.next(this._contacts);      // Emit to observable
    }

    public get HasContacts(): Promise<boolean> {
        return this.Contacts.then(contacts => contacts.length > 0);
    }




    /**
     * Updates the state of this ContactTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ContactTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ContactTypeSubmitData {
        return ContactTypeService.Instance.ConvertToContactTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ContactTypeService extends SecureEndpointBase {

    private static _instance: ContactTypeService;
    private listCache: Map<string, Observable<Array<ContactTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ContactTypeBasicListData>>>;
    private recordCache: Map<string, Observable<ContactTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private contactService: ContactService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ContactTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ContactTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<ContactTypeData>>();

        ContactTypeService._instance = this;
    }

    public static get Instance(): ContactTypeService {
      return ContactTypeService._instance;
    }


    public ClearListCaches(config: ContactTypeQueryParameters | null = null) {

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


    public ConvertToContactTypeSubmitData(data: ContactTypeData): ContactTypeSubmitData {

        let output = new ContactTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.iconId = data.iconId;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetContactType(id: bigint | number, includeRelations: boolean = true) : Observable<ContactTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const contactType$ = this.requestContactType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContactType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, contactType$);

            return contactType$;
        }

        return this.recordCache.get(configHash) as Observable<ContactTypeData>;
    }

    private requestContactType(id: bigint | number, includeRelations: boolean = true) : Observable<ContactTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContactTypeData>(this.baseUrl + 'api/ContactType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveContactType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestContactType(id, includeRelations));
            }));
    }

    public GetContactTypeList(config: ContactTypeQueryParameters | any = null) : Observable<Array<ContactTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const contactTypeList$ = this.requestContactTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContactType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, contactTypeList$);

            return contactTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ContactTypeData>>;
    }


    private requestContactTypeList(config: ContactTypeQueryParameters | any) : Observable <Array<ContactTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContactTypeData>>(this.baseUrl + 'api/ContactTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveContactTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestContactTypeList(config));
            }));
    }

    public GetContactTypesRowCount(config: ContactTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const contactTypesRowCount$ = this.requestContactTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContactTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, contactTypesRowCount$);

            return contactTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestContactTypesRowCount(config: ContactTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ContactTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContactTypesRowCount(config));
            }));
    }

    public GetContactTypesBasicListData(config: ContactTypeQueryParameters | any = null) : Observable<Array<ContactTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const contactTypesBasicListData$ = this.requestContactTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContactTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, contactTypesBasicListData$);

            return contactTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ContactTypeBasicListData>>;
    }


    private requestContactTypesBasicListData(config: ContactTypeQueryParameters | any) : Observable<Array<ContactTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContactTypeBasicListData>>(this.baseUrl + 'api/ContactTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContactTypesBasicListData(config));
            }));

    }


    public PutContactType(id: bigint | number, contactType: ContactTypeSubmitData) : Observable<ContactTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ContactTypeData>(this.baseUrl + 'api/ContactType/' + id.toString(), contactType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutContactType(id, contactType));
            }));
    }


    public PostContactType(contactType: ContactTypeSubmitData) : Observable<ContactTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ContactTypeData>(this.baseUrl + 'api/ContactType', contactType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostContactType(contactType));
            }));
    }

  
    public DeleteContactType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ContactType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteContactType(id));
            }));
    }


    private getConfigHash(config: ContactTypeQueryParameters | any): string {

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

    public userIsSchedulerContactTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerContactTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ContactTypes
        //
        if (userIsSchedulerContactTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerContactTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerContactTypeReader = false;
            }
        }

        return userIsSchedulerContactTypeReader;
    }


    public userIsSchedulerContactTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerContactTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ContactTypes
        //
        if (userIsSchedulerContactTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerContactTypeWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerContactTypeWriter = false;
          }      
        }

        return userIsSchedulerContactTypeWriter;
    }

    public GetContactsForContactType(contactTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactData[]> {
        return this.contactService.GetContactList({
            contactTypeId: contactTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ContactTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ContactTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ContactTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveContactType(raw: any): ContactTypeData {
    if (!raw) return raw;

    //
    // Create a ContactTypeData object instance with correct prototype
    //
    const revived = Object.create(ContactTypeData.prototype) as ContactTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._contacts = null;
    (revived as any)._contactsPromise = null;
    (revived as any)._contactsSubject = new BehaviorSubject<ContactData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadContactTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Contacts$ = (revived as any)._contactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contacts === null && (revived as any)._contactsPromise === null) {
                (revived as any).loadContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ContactsCount$ = ContactService.Instance.GetContactsRowCount({contactTypeId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveContactTypeList(rawList: any[]): ContactTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveContactType(raw));
  }

}
