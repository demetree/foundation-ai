/*

   GENERATED SERVICE FOR THE CONTACTSUBMISSION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ContactSubmission table.

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

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ContactSubmissionQueryParameters {
    Id: bigint | number | null | undefined = null;
    Name: string | null | undefined = null;
    Email: string | null | undefined = null;
    Subject: string | null | undefined = null;
    Message: string | null | undefined = null;
    SubmittedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    IsRead: boolean | null | undefined = null;
    IsArchived: boolean | null | undefined = null;
    AdminNotes: string | null | undefined = null;
    ObjectGuid: string | null | undefined = null;
    Active: boolean | null | undefined = null;
    Deleted: boolean | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class ContactSubmissionSubmitData {
    Id: bigint | number | null = null;
    Name: string | null = null;
    Email: string | null = null;
    Subject: string | null = null;
    Message: string | null = null;
    SubmittedDate: string | null = null;     // ISO 8601 (full datetime)
    IsRead: boolean | null = null;
    IsArchived: boolean | null = null;
    AdminNotes: string | null = null;
    Active: boolean | null = null;
    Deleted: boolean | null = null;
}


export class ContactSubmissionBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ContactSubmissionChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `contactSubmission.ContactSubmissionChildren$` — use with `| async` in templates
//        • Promise:    `contactSubmission.ContactSubmissionChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="contactSubmission.ContactSubmissionChildren$ | async"`), or
//        • Access the promise getter (`contactSubmission.ContactSubmissionChildren` or `await contactSubmission.ContactSubmissionChildren`)
//    - Simply reading `contactSubmission.ContactSubmissionChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await contactSubmission.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ContactSubmissionData {
    Id!: bigint | number;
    Name!: string | null;
    Email!: string | null;
    Subject!: string | null;
    Message!: string | null;
    SubmittedDate!: string | null;   // ISO 8601 (full datetime)
    IsRead!: boolean | null;
    IsArchived!: boolean | null;
    AdminNotes!: string | null;
    ObjectGuid!: string | null;
    Active!: boolean | null;
    Deleted!: boolean | null;

    //
    // Private lazy-loading caches for related collections
    //

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //

  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ContactSubmissionData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.contactSubmission.Reload();
  //
  //  Non Async:
  //
  //     contactSubmission[0].Reload().then(x => {
  //        this.contactSubmission = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ContactSubmissionService.Instance.GetContactSubmission(this.id, includeRelations)
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
  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //


    /**
     * Updates the state of this ContactSubmissionData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ContactSubmissionData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ContactSubmissionSubmitData {
        return ContactSubmissionService.Instance.ConvertToContactSubmissionSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ContactSubmissionService extends SecureEndpointBase {

    private static _instance: ContactSubmissionService;
    private listCache: Map<string, Observable<Array<ContactSubmissionData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ContactSubmissionBasicListData>>>;
    private recordCache: Map<string, Observable<ContactSubmissionData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ContactSubmissionData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ContactSubmissionBasicListData>>>();
        this.recordCache = new Map<string, Observable<ContactSubmissionData>>();

        ContactSubmissionService._instance = this;
    }

    public static get Instance(): ContactSubmissionService {
      return ContactSubmissionService._instance;
    }


    public ClearListCaches(config: ContactSubmissionQueryParameters | null = null) {

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


    public ConvertToContactSubmissionSubmitData(data: ContactSubmissionData): ContactSubmissionSubmitData {

        let output = new ContactSubmissionSubmitData();

        output.Id = data.Id;
        output.Name = data.Name;
        output.Email = data.Email;
        output.Subject = data.Subject;
        output.Message = data.Message;
        output.SubmittedDate = data.SubmittedDate;
        output.IsRead = data.IsRead;
        output.IsArchived = data.IsArchived;
        output.AdminNotes = data.AdminNotes;
        output.Active = data.Active;
        output.Deleted = data.Deleted;

        return output;
    }

    public GetContactSubmission(id: bigint | number, includeRelations: boolean = true) : Observable<ContactSubmissionData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const contactSubmission$ = this.requestContactSubmission(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContactSubmission", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, contactSubmission$);

            return contactSubmission$;
        }

        return this.recordCache.get(configHash) as Observable<ContactSubmissionData>;
    }

    private requestContactSubmission(id: bigint | number, includeRelations: boolean = true) : Observable<ContactSubmissionData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContactSubmissionData>(this.baseUrl + 'api/ContactSubmission/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveContactSubmission(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestContactSubmission(id, includeRelations));
            }));
    }

    public GetContactSubmissionList(config: ContactSubmissionQueryParameters | any = null) : Observable<Array<ContactSubmissionData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const contactSubmissionList$ = this.requestContactSubmissionList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContactSubmission list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, contactSubmissionList$);

            return contactSubmissionList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ContactSubmissionData>>;
    }


    private requestContactSubmissionList(config: ContactSubmissionQueryParameters | any) : Observable <Array<ContactSubmissionData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContactSubmissionData>>(this.baseUrl + 'api/ContactSubmissions', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveContactSubmissionList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestContactSubmissionList(config));
            }));
    }

    public GetContactSubmissionsRowCount(config: ContactSubmissionQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const contactSubmissionsRowCount$ = this.requestContactSubmissionsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ContactSubmissions row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, contactSubmissionsRowCount$);

            return contactSubmissionsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestContactSubmissionsRowCount(config: ContactSubmissionQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ContactSubmissions/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContactSubmissionsRowCount(config));
            }));
    }

    public GetContactSubmissionsBasicListData(config: ContactSubmissionQueryParameters | any = null) : Observable<Array<ContactSubmissionBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const contactSubmissionsBasicListData$ = this.requestContactSubmissionsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ContactSubmissions basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, contactSubmissionsBasicListData$);

            return contactSubmissionsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ContactSubmissionBasicListData>>;
    }


    private requestContactSubmissionsBasicListData(config: ContactSubmissionQueryParameters | any) : Observable<Array<ContactSubmissionBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContactSubmissionBasicListData>>(this.baseUrl + 'api/ContactSubmissions/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContactSubmissionsBasicListData(config));
            }));

    }


    public PutContactSubmission(id: bigint | number, contactSubmission: ContactSubmissionSubmitData) : Observable<ContactSubmissionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ContactSubmissionData>(this.baseUrl + 'api/ContactSubmission/' + id.toString(), contactSubmission, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactSubmission(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutContactSubmission(id, contactSubmission));
            }));
    }


    public PostContactSubmission(contactSubmission: ContactSubmissionSubmitData) : Observable<ContactSubmissionData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ContactSubmissionData>(this.baseUrl + 'api/ContactSubmission', contactSubmission, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContactSubmission(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostContactSubmission(contactSubmission));
            }));
    }

  
    public DeleteContactSubmission(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ContactSubmission/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteContactSubmission(id));
            }));
    }


    private getConfigHash(config: ContactSubmissionQueryParameters | any): string {

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

    public userIsCommunityContactSubmissionReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsCommunityContactSubmissionReader = this.authService.isCommunityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Community.ContactSubmissions
        //
        if (userIsCommunityContactSubmissionReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsCommunityContactSubmissionReader = user.readPermission >= 100;
            } else {
                userIsCommunityContactSubmissionReader = false;
            }
        }

        return userIsCommunityContactSubmissionReader;
    }


    public userIsCommunityContactSubmissionWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsCommunityContactSubmissionWriter = this.authService.isCommunityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Community.ContactSubmissions
        //
        if (userIsCommunityContactSubmissionWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsCommunityContactSubmissionWriter = user.writePermission >= 1;
          } else {
            userIsCommunityContactSubmissionWriter = false;
          }      
        }

        return userIsCommunityContactSubmissionWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ContactSubmissionData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ContactSubmissionData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ContactSubmissionTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveContactSubmission(raw: any): ContactSubmissionData {
    if (!raw) return raw;

    //
    // Create a ContactSubmissionData object instance with correct prototype
    //
    const revived = Object.create(ContactSubmissionData.prototype) as ContactSubmissionData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //

    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadContactSubmissionXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveContactSubmissionList(rawList: any[]): ContactSubmissionData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveContactSubmission(raw));
  }

}
