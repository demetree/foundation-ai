/*

   GENERATED SERVICE FOR THE EXTERNALCOMMUNICATIONRECIPIENT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ExternalCommunicationRecipient table.

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
import { ExternalCommunicationData } from './external-communication.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ExternalCommunicationRecipientQueryParameters {
    externalCommunicationId: bigint | number | null | undefined = null;
    recipient: string | null | undefined = null;
    type: string | null | undefined = null;
    pageSize: bigint | number | null | undefined = null;
    pageNumber: bigint | number | null | undefined = null;
    includeRelations: boolean | null | undefined = null;
    anyStringContains: string | null | undefined = null;
}


//
// This class is for sending to the server for saving with.  It includes only the fields that are necessary for saving data.
//
export class ExternalCommunicationRecipientSubmitData {
    id!: bigint | number;
    externalCommunicationId: bigint | number | null = null;
    recipient: string | null = null;
    type: string | null = null;
}


export class ExternalCommunicationRecipientBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ExternalCommunicationRecipientChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `externalCommunicationRecipient.ExternalCommunicationRecipientChildren$` — use with `| async` in templates
//        • Promise:    `externalCommunicationRecipient.ExternalCommunicationRecipientChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="externalCommunicationRecipient.ExternalCommunicationRecipientChildren$ | async"`), or
//        • Access the promise getter (`externalCommunicationRecipient.ExternalCommunicationRecipientChildren` or `await externalCommunicationRecipient.ExternalCommunicationRecipientChildren`)
//    - Simply reading `externalCommunicationRecipient.ExternalCommunicationRecipientChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await externalCommunicationRecipient.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ExternalCommunicationRecipientData {
    id!: bigint | number;
    externalCommunicationId!: bigint | number;
    recipient!: string | null;
    type!: string | null;
    externalCommunication: ExternalCommunicationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

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
  // Promise based reload method to allow rebuilding of any ExternalCommunicationRecipientData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.externalCommunicationRecipient.Reload();
  //
  //  Non Async:
  //
  //     externalCommunicationRecipient[0].Reload().then(x => {
  //        this.externalCommunicationRecipient = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ExternalCommunicationRecipientService.Instance.GetExternalCommunicationRecipient(this.id, includeRelations)
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
     * Updates the state of this ExternalCommunicationRecipientData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ExternalCommunicationRecipientData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ExternalCommunicationRecipientSubmitData {
        return ExternalCommunicationRecipientService.Instance.ConvertToExternalCommunicationRecipientSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ExternalCommunicationRecipientService extends SecureEndpointBase {

    private static _instance: ExternalCommunicationRecipientService;
    private listCache: Map<string, Observable<Array<ExternalCommunicationRecipientData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ExternalCommunicationRecipientBasicListData>>>;
    private recordCache: Map<string, Observable<ExternalCommunicationRecipientData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ExternalCommunicationRecipientData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ExternalCommunicationRecipientBasicListData>>>();
        this.recordCache = new Map<string, Observable<ExternalCommunicationRecipientData>>();

        ExternalCommunicationRecipientService._instance = this;
    }

    public static get Instance(): ExternalCommunicationRecipientService {
      return ExternalCommunicationRecipientService._instance;
    }


    public ClearListCaches(config: ExternalCommunicationRecipientQueryParameters | null = null) {

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


    public ConvertToExternalCommunicationRecipientSubmitData(data: ExternalCommunicationRecipientData): ExternalCommunicationRecipientSubmitData {

        let output = new ExternalCommunicationRecipientSubmitData();

        output.id = data.id;
        output.externalCommunicationId = data.externalCommunicationId;
        output.recipient = data.recipient;
        output.type = data.type;

        return output;
    }

    public GetExternalCommunicationRecipient(id: bigint | number, includeRelations: boolean = true) : Observable<ExternalCommunicationRecipientData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const externalCommunicationRecipient$ = this.requestExternalCommunicationRecipient(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ExternalCommunicationRecipient", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, externalCommunicationRecipient$);

            return externalCommunicationRecipient$;
        }

        return this.recordCache.get(configHash) as Observable<ExternalCommunicationRecipientData>;
    }

    private requestExternalCommunicationRecipient(id: bigint | number, includeRelations: boolean = true) : Observable<ExternalCommunicationRecipientData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ExternalCommunicationRecipientData>(this.baseUrl + 'api/ExternalCommunicationRecipient/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveExternalCommunicationRecipient(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestExternalCommunicationRecipient(id, includeRelations));
            }));
    }

    public GetExternalCommunicationRecipientList(config: ExternalCommunicationRecipientQueryParameters | any = null) : Observable<Array<ExternalCommunicationRecipientData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const externalCommunicationRecipientList$ = this.requestExternalCommunicationRecipientList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ExternalCommunicationRecipient list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, externalCommunicationRecipientList$);

            return externalCommunicationRecipientList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ExternalCommunicationRecipientData>>;
    }


    private requestExternalCommunicationRecipientList(config: ExternalCommunicationRecipientQueryParameters | any) : Observable <Array<ExternalCommunicationRecipientData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ExternalCommunicationRecipientData>>(this.baseUrl + 'api/ExternalCommunicationRecipients', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveExternalCommunicationRecipientList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestExternalCommunicationRecipientList(config));
            }));
    }

    public GetExternalCommunicationRecipientsRowCount(config: ExternalCommunicationRecipientQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const externalCommunicationRecipientsRowCount$ = this.requestExternalCommunicationRecipientsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ExternalCommunicationRecipients row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, externalCommunicationRecipientsRowCount$);

            return externalCommunicationRecipientsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestExternalCommunicationRecipientsRowCount(config: ExternalCommunicationRecipientQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ExternalCommunicationRecipients/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestExternalCommunicationRecipientsRowCount(config));
            }));
    }

    public GetExternalCommunicationRecipientsBasicListData(config: ExternalCommunicationRecipientQueryParameters | any = null) : Observable<Array<ExternalCommunicationRecipientBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const externalCommunicationRecipientsBasicListData$ = this.requestExternalCommunicationRecipientsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ExternalCommunicationRecipients basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, externalCommunicationRecipientsBasicListData$);

            return externalCommunicationRecipientsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ExternalCommunicationRecipientBasicListData>>;
    }


    private requestExternalCommunicationRecipientsBasicListData(config: ExternalCommunicationRecipientQueryParameters | any) : Observable<Array<ExternalCommunicationRecipientBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ExternalCommunicationRecipientBasicListData>>(this.baseUrl + 'api/ExternalCommunicationRecipients/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestExternalCommunicationRecipientsBasicListData(config));
            }));

    }


    public PutExternalCommunicationRecipient(id: bigint | number, externalCommunicationRecipient: ExternalCommunicationRecipientSubmitData) : Observable<ExternalCommunicationRecipientData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ExternalCommunicationRecipientData>(this.baseUrl + 'api/ExternalCommunicationRecipient/' + id.toString(), externalCommunicationRecipient, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveExternalCommunicationRecipient(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutExternalCommunicationRecipient(id, externalCommunicationRecipient));
            }));
    }


    public PostExternalCommunicationRecipient(externalCommunicationRecipient: ExternalCommunicationRecipientSubmitData) : Observable<ExternalCommunicationRecipientData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ExternalCommunicationRecipientData>(this.baseUrl + 'api/ExternalCommunicationRecipient', externalCommunicationRecipient, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveExternalCommunicationRecipient(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostExternalCommunicationRecipient(externalCommunicationRecipient));
            }));
    }

  
    public DeleteExternalCommunicationRecipient(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ExternalCommunicationRecipient/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteExternalCommunicationRecipient(id));
            }));
    }


    private getConfigHash(config: ExternalCommunicationRecipientQueryParameters | any): string {

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

    public userIsAuditorExternalCommunicationRecipientReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAuditorExternalCommunicationRecipientReader = this.authService.isAuditorReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Auditor.ExternalCommunicationRecipients
        //
        if (userIsAuditorExternalCommunicationRecipientReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAuditorExternalCommunicationRecipientReader = user.readPermission >= 0;
            } else {
                userIsAuditorExternalCommunicationRecipientReader = false;
            }
        }

        return userIsAuditorExternalCommunicationRecipientReader;
    }


    public userIsAuditorExternalCommunicationRecipientWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAuditorExternalCommunicationRecipientWriter = this.authService.isAuditorReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Auditor.ExternalCommunicationRecipients
        //
        if (userIsAuditorExternalCommunicationRecipientWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAuditorExternalCommunicationRecipientWriter = user.writePermission >= 0;
          } else {
            userIsAuditorExternalCommunicationRecipientWriter = false;
          }      
        }

        return userIsAuditorExternalCommunicationRecipientWriter;
    }

 /**
   *
   * Revives a plain object from the server into a full ExternalCommunicationRecipientData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ExternalCommunicationRecipientData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ExternalCommunicationRecipientTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveExternalCommunicationRecipient(raw: any): ExternalCommunicationRecipientData {
    if (!raw) return raw;

    //
    // Create a ExternalCommunicationRecipientData object instance with correct prototype
    //
    const revived = Object.create(ExternalCommunicationRecipientData.prototype) as ExternalCommunicationRecipientData;

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
    // 2. But private methods (loadExternalCommunicationRecipientXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //

    return revived;
  }

  private ReviveExternalCommunicationRecipientList(rawList: any[]): ExternalCommunicationRecipientData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveExternalCommunicationRecipient(raw));
  }

}
