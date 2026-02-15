/*

   GENERATED SERVICE FOR THE CONTACTTAG TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ContactTag table.

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
import { TagData } from './tag.service';
import { ContactTagChangeHistoryService, ContactTagChangeHistoryData } from './contact-tag-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ContactTagQueryParameters {
    contactId: bigint | number | null | undefined = null;
    tagId: bigint | number | null | undefined = null;
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
export class ContactTagSubmitData {
    id!: bigint | number;
    contactId!: bigint | number;
    tagId!: bigint | number;
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

export class ContactTagBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ContactTagChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `contactTag.ContactTagChildren$` — use with `| async` in templates
//        • Promise:    `contactTag.ContactTagChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="contactTag.ContactTagChildren$ | async"`), or
//        • Access the promise getter (`contactTag.ContactTagChildren` or `await contactTag.ContactTagChildren`)
//    - Simply reading `contactTag.ContactTagChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await contactTag.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ContactTagData {
    id!: bigint | number;
    contactId!: bigint | number;
    tagId!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    tag: TagData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _contactTagChangeHistories: ContactTagChangeHistoryData[] | null = null;
    private _contactTagChangeHistoriesPromise: Promise<ContactTagChangeHistoryData[]> | null  = null;
    private _contactTagChangeHistoriesSubject = new BehaviorSubject<ContactTagChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ContactTagData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ContactTagData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ContactTagData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ContactTagChangeHistories$ = this._contactTagChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactTagChangeHistories === null && this._contactTagChangeHistoriesPromise === null) {
            this.loadContactTagChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ContactTagChangeHistoriesCount$ = ContactTagChangeHistoryService.Instance.GetContactTagChangeHistoriesRowCount({contactTagId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ContactTagData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.contactTag.Reload();
  //
  //  Non Async:
  //
  //     contactTag[0].Reload().then(x => {
  //        this.contactTag = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ContactTagService.Instance.GetContactTag(this.id, includeRelations)
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
     this._contactTagChangeHistories = null;
     this._contactTagChangeHistoriesPromise = null;
     this._contactTagChangeHistoriesSubject.next(null);

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
     * Gets the ContactTagChangeHistories for this ContactTag.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contactTag.ContactTagChangeHistories.then(contactTags => { ... })
     *   or
     *   await this.contactTag.contactTags
     *
    */
    public get ContactTagChangeHistories(): Promise<ContactTagChangeHistoryData[]> {
        if (this._contactTagChangeHistories !== null) {
            return Promise.resolve(this._contactTagChangeHistories);
        }

        if (this._contactTagChangeHistoriesPromise !== null) {
            return this._contactTagChangeHistoriesPromise;
        }

        // Start the load
        this.loadContactTagChangeHistories();

        return this._contactTagChangeHistoriesPromise!;
    }



    private loadContactTagChangeHistories(): void {

        this._contactTagChangeHistoriesPromise = lastValueFrom(
            ContactTagService.Instance.GetContactTagChangeHistoriesForContactTag(this.id)
        )
        .then(ContactTagChangeHistories => {
            this._contactTagChangeHistories = ContactTagChangeHistories ?? [];
            this._contactTagChangeHistoriesSubject.next(this._contactTagChangeHistories);
            return this._contactTagChangeHistories;
         })
        .catch(err => {
            this._contactTagChangeHistories = [];
            this._contactTagChangeHistoriesSubject.next(this._contactTagChangeHistories);
            throw err;
        })
        .finally(() => {
            this._contactTagChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContactTagChangeHistory. Call after mutations to force refresh.
     */
    public ClearContactTagChangeHistoriesCache(): void {
        this._contactTagChangeHistories = null;
        this._contactTagChangeHistoriesPromise = null;
        this._contactTagChangeHistoriesSubject.next(this._contactTagChangeHistories);      // Emit to observable
    }

    public get HasContactTagChangeHistories(): Promise<boolean> {
        return this.ContactTagChangeHistories.then(contactTagChangeHistories => contactTagChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (contactTag.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await contactTag.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ContactTagData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ContactTagData>> {
        const info = await lastValueFrom(
            ContactTagService.Instance.GetContactTagChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ContactTagData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ContactTagData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ContactTagSubmitData {
        return ContactTagService.Instance.ConvertToContactTagSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ContactTagService extends SecureEndpointBase {

    private static _instance: ContactTagService;
    private listCache: Map<string, Observable<Array<ContactTagData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ContactTagBasicListData>>>;
    private recordCache: Map<string, Observable<ContactTagData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private contactTagChangeHistoryService: ContactTagChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ContactTagData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ContactTagBasicListData>>>();
        this.recordCache = new Map<string, Observable<ContactTagData>>();

        ContactTagService._instance = this;
    }

    public static get Instance(): ContactTagService {
      return ContactTagService._instance;
    }


    public ClearListCaches(config: ContactTagQueryParameters | null = null) {

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


    public ConvertToContactTagSubmitData(data: ContactTagData): ContactTagSubmitData {

        let output = new ContactTagSubmitData();

        output.id = data.id;
        output.contactId = data.contactId;
        output.tagId = data.tagId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetContactTag(id: bigint | number, includeRelations: boolean = true) : Observable<ContactTagData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const contactTag$ = this.requestContactTag(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContactTag", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, contactTag$);

            return contactTag$;
        }

        return this.recordCache.get(configHash) as Observable<ContactTagData>;
    }

    private requestContactTag(id: bigint | number, includeRelations: boolean = true) : Observable<ContactTagData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContactTagData>(this.baseUrl + 'api/ContactTag/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveContactTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestContactTag(id, includeRelations));
            }));
    }

    public GetContactTagList(config: ContactTagQueryParameters | any = null) : Observable<Array<ContactTagData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const contactTagList$ = this.requestContactTagList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContactTag list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, contactTagList$);

            return contactTagList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ContactTagData>>;
    }


    private requestContactTagList(config: ContactTagQueryParameters | any) : Observable <Array<ContactTagData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContactTagData>>(this.baseUrl + 'api/ContactTags', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveContactTagList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestContactTagList(config));
            }));
    }

    public GetContactTagsRowCount(config: ContactTagQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const contactTagsRowCount$ = this.requestContactTagsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContactTags row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, contactTagsRowCount$);

            return contactTagsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestContactTagsRowCount(config: ContactTagQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ContactTags/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContactTagsRowCount(config));
            }));
    }

    public GetContactTagsBasicListData(config: ContactTagQueryParameters | any = null) : Observable<Array<ContactTagBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const contactTagsBasicListData$ = this.requestContactTagsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContactTags basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, contactTagsBasicListData$);

            return contactTagsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ContactTagBasicListData>>;
    }


    private requestContactTagsBasicListData(config: ContactTagQueryParameters | any) : Observable<Array<ContactTagBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContactTagBasicListData>>(this.baseUrl + 'api/ContactTags/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContactTagsBasicListData(config));
            }));

    }


    public PutContactTag(id: bigint | number, contactTag: ContactTagSubmitData) : Observable<ContactTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ContactTagData>(this.baseUrl + 'api/ContactTag/' + id.toString(), contactTag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutContactTag(id, contactTag));
            }));
    }


    public PostContactTag(contactTag: ContactTagSubmitData) : Observable<ContactTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ContactTagData>(this.baseUrl + 'api/ContactTag', contactTag, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactTag(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostContactTag(contactTag));
            }));
    }

  
    public DeleteContactTag(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ContactTag/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteContactTag(id));
            }));
    }

    public RollbackContactTag(id: bigint | number, versionNumber: bigint | number) : Observable<ContactTagData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ContactTagData>(this.baseUrl + 'api/ContactTag/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackContactTag(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ContactTag.
     */
    public GetContactTagChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ContactTagData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ContactTagData>>(this.baseUrl + 'api/ContactTag/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetContactTagChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ContactTag.
     */
    public GetContactTagAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ContactTagData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ContactTagData>[]>(this.baseUrl + 'api/ContactTag/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetContactTagAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ContactTag.
     */
    public GetContactTagVersion(id: bigint | number, version: number): Observable<ContactTagData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContactTagData>(this.baseUrl + 'api/ContactTag/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveContactTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetContactTagVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ContactTag at a specific point in time.
     */
    public GetContactTagStateAtTime(id: bigint | number, time: string): Observable<ContactTagData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContactTagData>(this.baseUrl + 'api/ContactTag/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveContactTag(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetContactTagStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ContactTagQueryParameters | any): string {

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

    public userIsSchedulerContactTagReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerContactTagReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ContactTags
        //
        if (userIsSchedulerContactTagReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerContactTagReader = user.readPermission >= 1;
            } else {
                userIsSchedulerContactTagReader = false;
            }
        }

        return userIsSchedulerContactTagReader;
    }


    public userIsSchedulerContactTagWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerContactTagWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ContactTags
        //
        if (userIsSchedulerContactTagWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerContactTagWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerContactTagWriter = false;
          }      
        }

        return userIsSchedulerContactTagWriter;
    }

    public GetContactTagChangeHistoriesForContactTag(contactTagId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactTagChangeHistoryData[]> {
        return this.contactTagChangeHistoryService.GetContactTagChangeHistoryList({
            contactTagId: contactTagId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ContactTagData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ContactTagData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ContactTagTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveContactTag(raw: any): ContactTagData {
    if (!raw) return raw;

    //
    // Create a ContactTagData object instance with correct prototype
    //
    const revived = Object.create(ContactTagData.prototype) as ContactTagData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._contactTagChangeHistories = null;
    (revived as any)._contactTagChangeHistoriesPromise = null;
    (revived as any)._contactTagChangeHistoriesSubject = new BehaviorSubject<ContactTagChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadContactTagXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ContactTagChangeHistories$ = (revived as any)._contactTagChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactTagChangeHistories === null && (revived as any)._contactTagChangeHistoriesPromise === null) {
                (revived as any).loadContactTagChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ContactTagChangeHistoriesCount$ = ContactTagChangeHistoryService.Instance.GetContactTagChangeHistoriesRowCount({contactTagId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ContactTagData> | null>(null);

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

  private ReviveContactTagList(rawList: any[]): ContactTagData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveContactTag(raw));
  }

}
