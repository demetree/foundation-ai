/*

   GENERATED SERVICE FOR THE CHARGESTATUS TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the ChargeStatus table.

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
import { ChargeStatusChangeHistoryService, ChargeStatusChangeHistoryData } from './charge-status-change-history.service';
import { EventChargeService, EventChargeData } from './event-charge.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ChargeStatusQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    color: string | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class ChargeStatusSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    color: string | null = null;
    sequence: bigint | number | null = null;
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

export class ChargeStatusBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ChargeStatusChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `chargeStatus.ChargeStatusChildren$` — use with `| async` in templates
//        • Promise:    `chargeStatus.ChargeStatusChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="chargeStatus.ChargeStatusChildren$ | async"`), or
//        • Access the promise getter (`chargeStatus.ChargeStatusChildren` or `await chargeStatus.ChargeStatusChildren`)
//    - Simply reading `chargeStatus.ChargeStatusChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await chargeStatus.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ChargeStatusData {
    id!: bigint | number;
    name!: string;
    description!: string;
    color!: string | null;
    sequence!: bigint | number;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _chargeStatusChangeHistories: ChargeStatusChangeHistoryData[] | null = null;
    private _chargeStatusChangeHistoriesPromise: Promise<ChargeStatusChangeHistoryData[]> | null  = null;
    private _chargeStatusChangeHistoriesSubject = new BehaviorSubject<ChargeStatusChangeHistoryData[] | null>(null);

                
    private _eventCharges: EventChargeData[] | null = null;
    private _eventChargesPromise: Promise<EventChargeData[]> | null  = null;
    private _eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ChargeStatusData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ChargeStatusData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ChargeStatusData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ChargeStatusChangeHistories$ = this._chargeStatusChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._chargeStatusChangeHistories === null && this._chargeStatusChangeHistoriesPromise === null) {
            this.loadChargeStatusChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _chargeStatusChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ChargeStatusChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._chargeStatusChangeHistoriesCount$ === null) {
            this._chargeStatusChangeHistoriesCount$ = ChargeStatusChangeHistoryService.Instance.GetChargeStatusChangeHistoriesRowCount({chargeStatusId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._chargeStatusChangeHistoriesCount$;
    }



    public EventCharges$ = this._eventChargesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._eventCharges === null && this._eventChargesPromise === null) {
            this.loadEventCharges(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _eventChargesCount$: Observable<bigint | number> | null = null;
    public get EventChargesCount$(): Observable<bigint | number> {
        if (this._eventChargesCount$ === null) {
            this._eventChargesCount$ = EventChargeService.Instance.GetEventChargesRowCount({chargeStatusId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._eventChargesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ChargeStatusData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.chargeStatus.Reload();
  //
  //  Non Async:
  //
  //     chargeStatus[0].Reload().then(x => {
  //        this.chargeStatus = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ChargeStatusService.Instance.GetChargeStatus(this.id, includeRelations)
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
     this._chargeStatusChangeHistories = null;
     this._chargeStatusChangeHistoriesPromise = null;
     this._chargeStatusChangeHistoriesSubject.next(null);
     this._chargeStatusChangeHistoriesCount$ = null;

     this._eventCharges = null;
     this._eventChargesPromise = null;
     this._eventChargesSubject.next(null);
     this._eventChargesCount$ = null;

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
     * Gets the ChargeStatusChangeHistories for this ChargeStatus.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.chargeStatus.ChargeStatusChangeHistories.then(chargeStatuses => { ... })
     *   or
     *   await this.chargeStatus.chargeStatuses
     *
    */
    public get ChargeStatusChangeHistories(): Promise<ChargeStatusChangeHistoryData[]> {
        if (this._chargeStatusChangeHistories !== null) {
            return Promise.resolve(this._chargeStatusChangeHistories);
        }

        if (this._chargeStatusChangeHistoriesPromise !== null) {
            return this._chargeStatusChangeHistoriesPromise;
        }

        // Start the load
        this.loadChargeStatusChangeHistories();

        return this._chargeStatusChangeHistoriesPromise!;
    }



    private loadChargeStatusChangeHistories(): void {

        this._chargeStatusChangeHistoriesPromise = lastValueFrom(
            ChargeStatusService.Instance.GetChargeStatusChangeHistoriesForChargeStatus(this.id)
        )
        .then(ChargeStatusChangeHistories => {
            this._chargeStatusChangeHistories = ChargeStatusChangeHistories ?? [];
            this._chargeStatusChangeHistoriesSubject.next(this._chargeStatusChangeHistories);
            return this._chargeStatusChangeHistories;
         })
        .catch(err => {
            this._chargeStatusChangeHistories = [];
            this._chargeStatusChangeHistoriesSubject.next(this._chargeStatusChangeHistories);
            throw err;
        })
        .finally(() => {
            this._chargeStatusChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ChargeStatusChangeHistory. Call after mutations to force refresh.
     */
    public ClearChargeStatusChangeHistoriesCache(): void {
        this._chargeStatusChangeHistories = null;
        this._chargeStatusChangeHistoriesPromise = null;
        this._chargeStatusChangeHistoriesSubject.next(this._chargeStatusChangeHistories);      // Emit to observable
    }

    public get HasChargeStatusChangeHistories(): Promise<boolean> {
        return this.ChargeStatusChangeHistories.then(chargeStatusChangeHistories => chargeStatusChangeHistories.length > 0);
    }


    /**
     *
     * Gets the EventCharges for this ChargeStatus.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.chargeStatus.EventCharges.then(chargeStatuses => { ... })
     *   or
     *   await this.chargeStatus.chargeStatuses
     *
    */
    public get EventCharges(): Promise<EventChargeData[]> {
        if (this._eventCharges !== null) {
            return Promise.resolve(this._eventCharges);
        }

        if (this._eventChargesPromise !== null) {
            return this._eventChargesPromise;
        }

        // Start the load
        this.loadEventCharges();

        return this._eventChargesPromise!;
    }



    private loadEventCharges(): void {

        this._eventChargesPromise = lastValueFrom(
            ChargeStatusService.Instance.GetEventChargesForChargeStatus(this.id)
        )
        .then(EventCharges => {
            this._eventCharges = EventCharges ?? [];
            this._eventChargesSubject.next(this._eventCharges);
            return this._eventCharges;
         })
        .catch(err => {
            this._eventCharges = [];
            this._eventChargesSubject.next(this._eventCharges);
            throw err;
        })
        .finally(() => {
            this._eventChargesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached EventCharge. Call after mutations to force refresh.
     */
    public ClearEventChargesCache(): void {
        this._eventCharges = null;
        this._eventChargesPromise = null;
        this._eventChargesSubject.next(this._eventCharges);      // Emit to observable
    }

    public get HasEventCharges(): Promise<boolean> {
        return this.EventCharges.then(eventCharges => eventCharges.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (chargeStatus.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await chargeStatus.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ChargeStatusData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ChargeStatusData>> {
        const info = await lastValueFrom(
            ChargeStatusService.Instance.GetChargeStatusChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ChargeStatusData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ChargeStatusData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ChargeStatusSubmitData {
        return ChargeStatusService.Instance.ConvertToChargeStatusSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ChargeStatusService extends SecureEndpointBase {

    private static _instance: ChargeStatusService;
    private listCache: Map<string, Observable<Array<ChargeStatusData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ChargeStatusBasicListData>>>;
    private recordCache: Map<string, Observable<ChargeStatusData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private chargeStatusChangeHistoryService: ChargeStatusChangeHistoryService,
        private eventChargeService: EventChargeService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ChargeStatusData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ChargeStatusBasicListData>>>();
        this.recordCache = new Map<string, Observable<ChargeStatusData>>();

        ChargeStatusService._instance = this;
    }

    public static get Instance(): ChargeStatusService {
      return ChargeStatusService._instance;
    }


    public ClearListCaches(config: ChargeStatusQueryParameters | null = null) {

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


    public ConvertToChargeStatusSubmitData(data: ChargeStatusData): ChargeStatusSubmitData {

        let output = new ChargeStatusSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.color = data.color;
        output.sequence = data.sequence;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetChargeStatus(id: bigint | number, includeRelations: boolean = true) : Observable<ChargeStatusData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const chargeStatus$ = this.requestChargeStatus(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ChargeStatus", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, chargeStatus$);

            return chargeStatus$;
        }

        return this.recordCache.get(configHash) as Observable<ChargeStatusData>;
    }

    private requestChargeStatus(id: bigint | number, includeRelations: boolean = true) : Observable<ChargeStatusData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ChargeStatusData>(this.baseUrl + 'api/ChargeStatus/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveChargeStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestChargeStatus(id, includeRelations));
            }));
    }

    public GetChargeStatusList(config: ChargeStatusQueryParameters | any = null) : Observable<Array<ChargeStatusData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const chargeStatusList$ = this.requestChargeStatusList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ChargeStatus list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, chargeStatusList$);

            return chargeStatusList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ChargeStatusData>>;
    }


    private requestChargeStatusList(config: ChargeStatusQueryParameters | any) : Observable <Array<ChargeStatusData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ChargeStatusData>>(this.baseUrl + 'api/ChargeStatuses', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveChargeStatusList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestChargeStatusList(config));
            }));
    }

    public GetChargeStatusesRowCount(config: ChargeStatusQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const chargeStatusesRowCount$ = this.requestChargeStatusesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get ChargeStatuses row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, chargeStatusesRowCount$);

            return chargeStatusesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestChargeStatusesRowCount(config: ChargeStatusQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/ChargeStatuses/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestChargeStatusesRowCount(config));
            }));
    }

    public GetChargeStatusesBasicListData(config: ChargeStatusQueryParameters | any = null) : Observable<Array<ChargeStatusBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const chargeStatusesBasicListData$ = this.requestChargeStatusesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get ChargeStatuses basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, chargeStatusesBasicListData$);

            return chargeStatusesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ChargeStatusBasicListData>>;
    }


    private requestChargeStatusesBasicListData(config: ChargeStatusQueryParameters | any) : Observable<Array<ChargeStatusBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ChargeStatusBasicListData>>(this.baseUrl + 'api/ChargeStatuses/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestChargeStatusesBasicListData(config));
            }));

    }


    public PutChargeStatus(id: bigint | number, chargeStatus: ChargeStatusSubmitData) : Observable<ChargeStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ChargeStatusData>(this.baseUrl + 'api/ChargeStatus/' + id.toString(), chargeStatus, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveChargeStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutChargeStatus(id, chargeStatus));
            }));
    }


    public PostChargeStatus(chargeStatus: ChargeStatusSubmitData) : Observable<ChargeStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ChargeStatusData>(this.baseUrl + 'api/ChargeStatus', chargeStatus, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveChargeStatus(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostChargeStatus(chargeStatus));
            }));
    }

  
    public DeleteChargeStatus(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/ChargeStatus/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteChargeStatus(id));
            }));
    }

    public RollbackChargeStatus(id: bigint | number, versionNumber: bigint | number) : Observable<ChargeStatusData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ChargeStatusData>(this.baseUrl + 'api/ChargeStatus/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveChargeStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackChargeStatus(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a ChargeStatus.
     */
    public GetChargeStatusChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ChargeStatusData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ChargeStatusData>>(this.baseUrl + 'api/ChargeStatus/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetChargeStatusChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a ChargeStatus.
     */
    public GetChargeStatusAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ChargeStatusData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ChargeStatusData>[]>(this.baseUrl + 'api/ChargeStatus/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetChargeStatusAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a ChargeStatus.
     */
    public GetChargeStatusVersion(id: bigint | number, version: number): Observable<ChargeStatusData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ChargeStatusData>(this.baseUrl + 'api/ChargeStatus/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveChargeStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetChargeStatusVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a ChargeStatus at a specific point in time.
     */
    public GetChargeStatusStateAtTime(id: bigint | number, time: string): Observable<ChargeStatusData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ChargeStatusData>(this.baseUrl + 'api/ChargeStatus/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveChargeStatus(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetChargeStatusStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ChargeStatusQueryParameters | any): string {

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

    public userIsSchedulerChargeStatusReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerChargeStatusReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.ChargeStatuses
        //
        if (userIsSchedulerChargeStatusReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerChargeStatusReader = user.readPermission >= 1;
            } else {
                userIsSchedulerChargeStatusReader = false;
            }
        }

        return userIsSchedulerChargeStatusReader;
    }


    public userIsSchedulerChargeStatusWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerChargeStatusWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.ChargeStatuses
        //
        if (userIsSchedulerChargeStatusWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerChargeStatusWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerChargeStatusWriter = false;
          }      
        }

        return userIsSchedulerChargeStatusWriter;
    }

    public GetChargeStatusChangeHistoriesForChargeStatus(chargeStatusId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ChargeStatusChangeHistoryData[]> {
        return this.chargeStatusChangeHistoryService.GetChargeStatusChangeHistoryList({
            chargeStatusId: chargeStatusId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEventChargesForChargeStatus(chargeStatusId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EventChargeData[]> {
        return this.eventChargeService.GetEventChargeList({
            chargeStatusId: chargeStatusId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ChargeStatusData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ChargeStatusData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ChargeStatusTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveChargeStatus(raw: any): ChargeStatusData {
    if (!raw) return raw;

    //
    // Create a ChargeStatusData object instance with correct prototype
    //
    const revived = Object.create(ChargeStatusData.prototype) as ChargeStatusData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._chargeStatusChangeHistories = null;
    (revived as any)._chargeStatusChangeHistoriesPromise = null;
    (revived as any)._chargeStatusChangeHistoriesSubject = new BehaviorSubject<ChargeStatusChangeHistoryData[] | null>(null);

    (revived as any)._eventCharges = null;
    (revived as any)._eventChargesPromise = null;
    (revived as any)._eventChargesSubject = new BehaviorSubject<EventChargeData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadChargeStatusXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ChargeStatusChangeHistories$ = (revived as any)._chargeStatusChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._chargeStatusChangeHistories === null && (revived as any)._chargeStatusChangeHistoriesPromise === null) {
                (revived as any).loadChargeStatusChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._chargeStatusChangeHistoriesCount$ = null;


    (revived as any).EventCharges$ = (revived as any)._eventChargesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._eventCharges === null && (revived as any)._eventChargesPromise === null) {
                (revived as any).loadEventCharges();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._eventChargesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ChargeStatusData> | null>(null);

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

  private ReviveChargeStatusList(rawList: any[]): ChargeStatusData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveChargeStatus(raw));
  }

}
