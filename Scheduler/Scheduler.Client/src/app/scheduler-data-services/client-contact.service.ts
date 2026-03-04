/*

   GENERATED SERVICE FOR THE CLIENTCONTACT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ClientContact table.

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
import { ClientData } from './client.service';
import { ContactData } from './contact.service';
import { RelationshipTypeData } from './relationship-type.service';
import { ClientContactChangeHistoryService, ClientContactChangeHistoryData } from './client-contact-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ClientContactQueryParameters {
    clientId: bigint | number | null | undefined = null;
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
export class ClientContactSubmitData {
    id!: bigint | number;
    clientId!: bigint | number;
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

export class ClientContactBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ClientContactChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `clientContact.ClientContactChildren$` — use with `| async` in templates
//        • Promise:    `clientContact.ClientContactChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="clientContact.ClientContactChildren$ | async"`), or
//        • Access the promise getter (`clientContact.ClientContactChildren` or `await clientContact.ClientContactChildren`)
//    - Simply reading `clientContact.ClientContactChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await clientContact.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ClientContactData {
    id!: bigint | number;
    clientId!: bigint | number;
    contactId!: bigint | number;
    isPrimary!: boolean;
    relationshipTypeId!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    client: ClientData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    contact: ContactData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    relationshipType: RelationshipTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _clientContactChangeHistories: ClientContactChangeHistoryData[] | null = null;
    private _clientContactChangeHistoriesPromise: Promise<ClientContactChangeHistoryData[]> | null  = null;
    private _clientContactChangeHistoriesSubject = new BehaviorSubject<ClientContactChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ClientContactData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ClientContactData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ClientContactData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ClientContactChangeHistories$ = this._clientContactChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._clientContactChangeHistories === null && this._clientContactChangeHistoriesPromise === null) {
            this.loadClientContactChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _clientContactChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ClientContactChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._clientContactChangeHistoriesCount$ === null) {
            this._clientContactChangeHistoriesCount$ = ClientContactChangeHistoryService.Instance.GetClientContactChangeHistoriesRowCount({clientContactId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._clientContactChangeHistoriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ClientContactData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.clientContact.Reload();
  //
  //  Non Async:
  //
  //     clientContact[0].Reload().then(x => {
  //        this.clientContact = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ClientContactService.Instance.GetClientContact(this.id, includeRelations)
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
     this._clientContactChangeHistories = null;
     this._clientContactChangeHistoriesPromise = null;
     this._clientContactChangeHistoriesSubject.next(null);
     this._clientContactChangeHistoriesCount$ = null;

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
     * Gets the ClientContactChangeHistories for this ClientContact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.clientContact.ClientContactChangeHistories.then(clientContacts => { ... })
     *   or
     *   await this.clientContact.clientContacts
     *
    */
    public get ClientContactChangeHistories(): Promise<ClientContactChangeHistoryData[]> {
        if (this._clientContactChangeHistories !== null) {
            return Promise.resolve(this._clientContactChangeHistories);
        }

        if (this._clientContactChangeHistoriesPromise !== null) {
            return this._clientContactChangeHistoriesPromise;
        }

        // Start the load
        this.loadClientContactChangeHistories();

        return this._clientContactChangeHistoriesPromise!;
    }



    private loadClientContactChangeHistories(): void {

        this._clientContactChangeHistoriesPromise = lastValueFrom(
            ClientContactService.Instance.GetClientContactChangeHistoriesForClientContact(this.id)
        )
        .then(ClientContactChangeHistories => {
            this._clientContactChangeHistories = ClientContactChangeHistories ?? [];
            this._clientContactChangeHistoriesSubject.next(this._clientContactChangeHistories);
            return this._clientContactChangeHistories;
         })
        .catch(err => {
            this._clientContactChangeHistories = [];
            this._clientContactChangeHistoriesSubject.next(this._clientContactChangeHistories);
            throw err;
        })
        .finally(() => {
            this._clientContactChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ClientContactChangeHistory. Call after mutations to force refresh.
     */
    public ClearClientContactChangeHistoriesCache(): void {
        this._clientContactChangeHistories = null;
        this._clientContactChangeHistoriesPromise = null;
        this._clientContactChangeHistoriesSubject.next(this._clientContactChangeHistories);      // Emit to observable
    }

    public get HasClientContactChangeHistories(): Promise<boolean> {
        return this.ClientContactChangeHistories.then(clientContactChangeHistories => clientContactChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (clientContact.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await clientContact.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ClientContactData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ClientContactData>> {
        const info = await lastValueFrom(
            ClientContactService.Instance.GetClientContactChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ClientContactData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ClientContactData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ClientContactSubmitData {
        return ClientContactService.Instance.ConvertToClientContactSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ClientContactService extends SecureEndpointBase {

    private static _instance: ClientContactService;
    private listCache: Map<string, Observable<Array<ClientContactData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ClientContactBasicListData>>>;
    private recordCache: Map<string, Observable<ClientContactData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private clientContactChangeHistoryService: ClientContactChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ClientContactData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ClientContactBasicListData>>>();
        this.recordCache = new Map<string, Observable<ClientContactData>>();

        ClientContactService._instance = this;
    }

    public static get Instance(): ClientContactService {
      return ClientContactService._instance;
    }


    public ClearListCaches(config: ClientContactQueryParameters | null = null) {

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


    public ConvertToClientContactSubmitData(data: ClientContactData): ClientContactSubmitData {

        let output = new ClientContactSubmitData();

        output.id = data.id;
        output.clientId = data.clientId;
        output.contactId = data.contactId;
        output.isPrimary = data.isPrimary;
        output.relationshipTypeId = data.relationshipTypeId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetClientContact(id: bigint | number, includeRelations: boolean = true) : Observable<ClientContactData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const clientContact$ = this.requestClientContact(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ClientContact", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, clientContact$);

            return clientContact$;
        }

        return this.recordCache.get(configHash) as Observable<ClientContactData>;
    }

    private requestClientContact(id: bigint | number, includeRelations: boolean = true) : Observable<ClientContactData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ClientContactData>(this.baseUrl + 'api/ClientContact/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveClientContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestClientContact(id, includeRelations));
            }));
    }

    public GetClientContactList(config: ClientContactQueryParameters | any = null) : Observable<Array<ClientContactData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const clientContactList$ = this.requestClientContactList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ClientContact list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, clientContactList$);

            return clientContactList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ClientContactData>>;
    }


    private requestClientContactList(config: ClientContactQueryParameters | any) : Observable <Array<ClientContactData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ClientContactData>>(this.baseUrl + 'api/ClientContacts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveClientContactList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestClientContactList(config));
            }));
    }

    public GetClientContactsRowCount(config: ClientContactQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const clientContactsRowCount$ = this.requestClientContactsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ClientContacts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, clientContactsRowCount$);

            return clientContactsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestClientContactsRowCount(config: ClientContactQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ClientContacts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestClientContactsRowCount(config));
            }));
    }

    public GetClientContactsBasicListData(config: ClientContactQueryParameters | any = null) : Observable<Array<ClientContactBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const clientContactsBasicListData$ = this.requestClientContactsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ClientContacts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, clientContactsBasicListData$);

            return clientContactsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ClientContactBasicListData>>;
    }


    private requestClientContactsBasicListData(config: ClientContactQueryParameters | any) : Observable<Array<ClientContactBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ClientContactBasicListData>>(this.baseUrl + 'api/ClientContacts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestClientContactsBasicListData(config));
            }));

    }


    public PutClientContact(id: bigint | number, clientContact: ClientContactSubmitData) : Observable<ClientContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ClientContactData>(this.baseUrl + 'api/ClientContact/' + id.toString(), clientContact, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveClientContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutClientContact(id, clientContact));
            }));
    }


    public PostClientContact(clientContact: ClientContactSubmitData) : Observable<ClientContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ClientContactData>(this.baseUrl + 'api/ClientContact', clientContact, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveClientContact(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostClientContact(clientContact));
            }));
    }

  
    public DeleteClientContact(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ClientContact/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteClientContact(id));
            }));
    }

    public RollbackClientContact(id: bigint | number, versionNumber: bigint | number) : Observable<ClientContactData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ClientContactData>(this.baseUrl + 'api/ClientContact/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveClientContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackClientContact(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ClientContact.
     */
    public GetClientContactChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ClientContactData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ClientContactData>>(this.baseUrl + 'api/ClientContact/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetClientContactChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ClientContact.
     */
    public GetClientContactAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ClientContactData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ClientContactData>[]>(this.baseUrl + 'api/ClientContact/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetClientContactAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ClientContact.
     */
    public GetClientContactVersion(id: bigint | number, version: number): Observable<ClientContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ClientContactData>(this.baseUrl + 'api/ClientContact/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveClientContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetClientContactVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ClientContact at a specific point in time.
     */
    public GetClientContactStateAtTime(id: bigint | number, time: string): Observable<ClientContactData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ClientContactData>(this.baseUrl + 'api/ClientContact/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveClientContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetClientContactStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ClientContactQueryParameters | any): string {

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

    public userIsSchedulerClientContactReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerClientContactReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ClientContacts
        //
        if (userIsSchedulerClientContactReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerClientContactReader = user.readPermission >= 1;
            } else {
                userIsSchedulerClientContactReader = false;
            }
        }

        return userIsSchedulerClientContactReader;
    }


    public userIsSchedulerClientContactWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerClientContactWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ClientContacts
        //
        if (userIsSchedulerClientContactWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerClientContactWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerClientContactWriter = false;
          }      
        }

        return userIsSchedulerClientContactWriter;
    }

    public GetClientContactChangeHistoriesForClientContact(clientContactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ClientContactChangeHistoryData[]> {
        return this.clientContactChangeHistoryService.GetClientContactChangeHistoryList({
            clientContactId: clientContactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ClientContactData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ClientContactData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ClientContactTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveClientContact(raw: any): ClientContactData {
    if (!raw) return raw;

    //
    // Create a ClientContactData object instance with correct prototype
    //
    const revived = Object.create(ClientContactData.prototype) as ClientContactData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._clientContactChangeHistories = null;
    (revived as any)._clientContactChangeHistoriesPromise = null;
    (revived as any)._clientContactChangeHistoriesSubject = new BehaviorSubject<ClientContactChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadClientContactXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ClientContactChangeHistories$ = (revived as any)._clientContactChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._clientContactChangeHistories === null && (revived as any)._clientContactChangeHistoriesPromise === null) {
                (revived as any).loadClientContactChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._clientContactChangeHistoriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ClientContactData> | null>(null);

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

  private ReviveClientContactList(rawList: any[]): ClientContactData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveClientContact(raw));
  }

}
