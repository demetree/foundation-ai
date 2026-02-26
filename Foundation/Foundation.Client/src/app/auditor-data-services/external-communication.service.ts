/*

   GENERATED SERVICE FOR THE EXTERNALCOMMUNICATION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ExternalCommunication table.

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
import { AuditUserData } from './audit-user.service';
import { ExternalCommunicationRecipientService, ExternalCommunicationRecipientData } from './external-communication-recipient.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ExternalCommunicationQueryParameters {
    timeStamp: string | null | undefined = null;        // ISO 8601 (full datetime)
    auditUserId: bigint | number | null | undefined = null;
    communicationType: string | null | undefined = null;
    subject: string | null | undefined = null;
    message: string | null | undefined = null;
    completedSuccessfully: boolean | null | undefined = null;
    responseMessage: string | null | undefined = null;
    exceptionText: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class ExternalCommunicationSubmitData {
    id!: bigint | number;
    timeStamp: string | null = null;     // ISO 8601 (full datetime)
    auditUserId: bigint | number | null = null;
    communicationType: string | null = null;
    subject: string | null = null;
    message: string | null = null;
    completedSuccessfully!: boolean;
    responseMessage: string | null = null;
    exceptionText: string | null = null;
}


export class ExternalCommunicationBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ExternalCommunicationChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `externalCommunication.ExternalCommunicationChildren$` — use with `| async` in templates
//        • Promise:    `externalCommunication.ExternalCommunicationChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="externalCommunication.ExternalCommunicationChildren$ | async"`), or
//        • Access the promise getter (`externalCommunication.ExternalCommunicationChildren` or `await externalCommunication.ExternalCommunicationChildren`)
//    - Simply reading `externalCommunication.ExternalCommunicationChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await externalCommunication.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ExternalCommunicationData {
    id!: bigint | number;
    timeStamp!: string | null;   // ISO 8601 (full datetime)
    auditUserId!: bigint | number;
    communicationType!: string | null;
    subject!: string | null;
    message!: string | null;
    completedSuccessfully!: boolean;
    responseMessage!: string | null;
    exceptionText!: string | null;
    auditUser: AuditUserData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _externalCommunicationRecipients: ExternalCommunicationRecipientData[] | null = null;
    private _externalCommunicationRecipientsPromise: Promise<ExternalCommunicationRecipientData[]> | null  = null;
    private _externalCommunicationRecipientsSubject = new BehaviorSubject<ExternalCommunicationRecipientData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ExternalCommunicationRecipients$ = this._externalCommunicationRecipientsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._externalCommunicationRecipients === null && this._externalCommunicationRecipientsPromise === null) {
            this.loadExternalCommunicationRecipients(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _externalCommunicationRecipientsCount$: Observable<bigint | number> | null = null;
    public get ExternalCommunicationRecipientsCount$(): Observable<bigint | number> {
        if (this._externalCommunicationRecipientsCount$ === null) {
            this._externalCommunicationRecipientsCount$ = ExternalCommunicationRecipientService.Instance.GetExternalCommunicationRecipientsRowCount({externalCommunicationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._externalCommunicationRecipientsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ExternalCommunicationData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.externalCommunication.Reload();
  //
  //  Non Async:
  //
  //     externalCommunication[0].Reload().then(x => {
  //        this.externalCommunication = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ExternalCommunicationService.Instance.GetExternalCommunication(this.id, includeRelations)
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
     this._externalCommunicationRecipients = null;
     this._externalCommunicationRecipientsPromise = null;
     this._externalCommunicationRecipientsSubject.next(null);
     this._externalCommunicationRecipientsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ExternalCommunicationRecipients for this ExternalCommunication.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.externalCommunication.ExternalCommunicationRecipients.then(externalCommunications => { ... })
     *   or
     *   await this.externalCommunication.externalCommunications
     *
    */
    public get ExternalCommunicationRecipients(): Promise<ExternalCommunicationRecipientData[]> {
        if (this._externalCommunicationRecipients !== null) {
            return Promise.resolve(this._externalCommunicationRecipients);
        }

        if (this._externalCommunicationRecipientsPromise !== null) {
            return this._externalCommunicationRecipientsPromise;
        }

        // Start the load
        this.loadExternalCommunicationRecipients();

        return this._externalCommunicationRecipientsPromise!;
    }



    private loadExternalCommunicationRecipients(): void {

        this._externalCommunicationRecipientsPromise = lastValueFrom(
            ExternalCommunicationService.Instance.GetExternalCommunicationRecipientsForExternalCommunication(this.id)
        )
        .then(ExternalCommunicationRecipients => {
            this._externalCommunicationRecipients = ExternalCommunicationRecipients ?? [];
            this._externalCommunicationRecipientsSubject.next(this._externalCommunicationRecipients);
            return this._externalCommunicationRecipients;
         })
        .catch(err => {
            this._externalCommunicationRecipients = [];
            this._externalCommunicationRecipientsSubject.next(this._externalCommunicationRecipients);
            throw err;
        })
        .finally(() => {
            this._externalCommunicationRecipientsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ExternalCommunicationRecipient. Call after mutations to force refresh.
     */
    public ClearExternalCommunicationRecipientsCache(): void {
        this._externalCommunicationRecipients = null;
        this._externalCommunicationRecipientsPromise = null;
        this._externalCommunicationRecipientsSubject.next(this._externalCommunicationRecipients);      // Emit to observable
    }

    public get HasExternalCommunicationRecipients(): Promise<boolean> {
        return this.ExternalCommunicationRecipients.then(externalCommunicationRecipients => externalCommunicationRecipients.length > 0);
    }




    /**
     * Updates the state of this ExternalCommunicationData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ExternalCommunicationData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ExternalCommunicationSubmitData {
        return ExternalCommunicationService.Instance.ConvertToExternalCommunicationSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ExternalCommunicationService extends SecureEndpointBase {

    private static _instance: ExternalCommunicationService;
    private listCache: Map<string, Observable<Array<ExternalCommunicationData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ExternalCommunicationBasicListData>>>;
    private recordCache: Map<string, Observable<ExternalCommunicationData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private externalCommunicationRecipientService: ExternalCommunicationRecipientService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ExternalCommunicationData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ExternalCommunicationBasicListData>>>();
        this.recordCache = new Map<string, Observable<ExternalCommunicationData>>();

        ExternalCommunicationService._instance = this;
    }

    public static get Instance(): ExternalCommunicationService {
      return ExternalCommunicationService._instance;
    }


    public ClearListCaches(config: ExternalCommunicationQueryParameters | null = null) {

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


    public ConvertToExternalCommunicationSubmitData(data: ExternalCommunicationData): ExternalCommunicationSubmitData {

        let output = new ExternalCommunicationSubmitData();

        output.id = data.id;
        output.timeStamp = data.timeStamp;
        output.auditUserId = data.auditUserId;
        output.communicationType = data.communicationType;
        output.subject = data.subject;
        output.message = data.message;
        output.completedSuccessfully = data.completedSuccessfully;
        output.responseMessage = data.responseMessage;
        output.exceptionText = data.exceptionText;

        return output;
    }

    public GetExternalCommunication(id: bigint | number, includeRelations: boolean = true) : Observable<ExternalCommunicationData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const externalCommunication$ = this.requestExternalCommunication(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ExternalCommunication", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, externalCommunication$);

            return externalCommunication$;
        }

        return this.recordCache.get(configHash) as Observable<ExternalCommunicationData>;
    }

    private requestExternalCommunication(id: bigint | number, includeRelations: boolean = true) : Observable<ExternalCommunicationData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ExternalCommunicationData>(this.baseUrl + 'api/ExternalCommunication/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveExternalCommunication(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestExternalCommunication(id, includeRelations));
            }));
    }

    public GetExternalCommunicationList(config: ExternalCommunicationQueryParameters | any = null) : Observable<Array<ExternalCommunicationData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const externalCommunicationList$ = this.requestExternalCommunicationList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ExternalCommunication list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, externalCommunicationList$);

            return externalCommunicationList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ExternalCommunicationData>>;
    }


    private requestExternalCommunicationList(config: ExternalCommunicationQueryParameters | any) : Observable <Array<ExternalCommunicationData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ExternalCommunicationData>>(this.baseUrl + 'api/ExternalCommunications', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveExternalCommunicationList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestExternalCommunicationList(config));
            }));
    }

    public GetExternalCommunicationsRowCount(config: ExternalCommunicationQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const externalCommunicationsRowCount$ = this.requestExternalCommunicationsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ExternalCommunications row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, externalCommunicationsRowCount$);

            return externalCommunicationsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestExternalCommunicationsRowCount(config: ExternalCommunicationQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ExternalCommunications/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestExternalCommunicationsRowCount(config));
            }));
    }

    public GetExternalCommunicationsBasicListData(config: ExternalCommunicationQueryParameters | any = null) : Observable<Array<ExternalCommunicationBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const externalCommunicationsBasicListData$ = this.requestExternalCommunicationsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ExternalCommunications basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, externalCommunicationsBasicListData$);

            return externalCommunicationsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ExternalCommunicationBasicListData>>;
    }


    private requestExternalCommunicationsBasicListData(config: ExternalCommunicationQueryParameters | any) : Observable<Array<ExternalCommunicationBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ExternalCommunicationBasicListData>>(this.baseUrl + 'api/ExternalCommunications/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestExternalCommunicationsBasicListData(config));
            }));

    }


    public PutExternalCommunication(id: bigint | number, externalCommunication: ExternalCommunicationSubmitData) : Observable<ExternalCommunicationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ExternalCommunicationData>(this.baseUrl + 'api/ExternalCommunication/' + id.toString(), externalCommunication, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveExternalCommunication(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutExternalCommunication(id, externalCommunication));
            }));
    }


    public PostExternalCommunication(externalCommunication: ExternalCommunicationSubmitData) : Observable<ExternalCommunicationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ExternalCommunicationData>(this.baseUrl + 'api/ExternalCommunication', externalCommunication, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveExternalCommunication(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostExternalCommunication(externalCommunication));
            }));
    }

  
    public DeleteExternalCommunication(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ExternalCommunication/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteExternalCommunication(id));
            }));
    }


    private getConfigHash(config: ExternalCommunicationQueryParameters | any): string {

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

    public userIsAuditorExternalCommunicationReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAuditorExternalCommunicationReader = this.authService.isAuditorReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Auditor.ExternalCommunications
        //
        if (userIsAuditorExternalCommunicationReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAuditorExternalCommunicationReader = user.readPermission >= 0;
            } else {
                userIsAuditorExternalCommunicationReader = false;
            }
        }

        return userIsAuditorExternalCommunicationReader;
    }


    public userIsAuditorExternalCommunicationWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAuditorExternalCommunicationWriter = this.authService.isAuditorReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Auditor.ExternalCommunications
        //
        if (userIsAuditorExternalCommunicationWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAuditorExternalCommunicationWriter = user.writePermission >= 0;
          } else {
            userIsAuditorExternalCommunicationWriter = false;
          }      
        }

        return userIsAuditorExternalCommunicationWriter;
    }

    public GetExternalCommunicationRecipientsForExternalCommunication(externalCommunicationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ExternalCommunicationRecipientData[]> {
        return this.externalCommunicationRecipientService.GetExternalCommunicationRecipientList({
            externalCommunicationId: externalCommunicationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ExternalCommunicationData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ExternalCommunicationData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ExternalCommunicationTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveExternalCommunication(raw: any): ExternalCommunicationData {
    if (!raw) return raw;

    //
    // Create a ExternalCommunicationData object instance with correct prototype
    //
    const revived = Object.create(ExternalCommunicationData.prototype) as ExternalCommunicationData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._externalCommunicationRecipients = null;
    (revived as any)._externalCommunicationRecipientsPromise = null;
    (revived as any)._externalCommunicationRecipientsSubject = new BehaviorSubject<ExternalCommunicationRecipientData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadExternalCommunicationXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ExternalCommunicationRecipients$ = (revived as any)._externalCommunicationRecipientsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._externalCommunicationRecipients === null && (revived as any)._externalCommunicationRecipientsPromise === null) {
                (revived as any).loadExternalCommunicationRecipients();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._externalCommunicationRecipientsCount$ = null;



    return revived;
  }

  private ReviveExternalCommunicationList(rawList: any[]): ExternalCommunicationData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveExternalCommunication(raw));
  }

}
