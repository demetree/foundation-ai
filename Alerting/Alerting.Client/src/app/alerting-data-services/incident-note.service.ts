/*

   GENERATED SERVICE FOR THE INCIDENTNOTE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the IncidentNote table.

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
import { IncidentData } from './incident.service';
import { IncidentNoteChangeHistoryService, IncidentNoteChangeHistoryData } from './incident-note-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class IncidentNoteQueryParameters {
    incidentId: bigint | number | null | undefined = null;
    authorObjectGuid: string | null | undefined = null;
    createdAt: string | null | undefined = null;        // ISO 8601
    content: string | null | undefined = null;
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
export class IncidentNoteSubmitData {
    id!: bigint | number;
    incidentId!: bigint | number;
    authorObjectGuid!: string;
    createdAt!: string;      // ISO 8601
    content!: string;
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

export class IncidentNoteBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. IncidentNoteChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `incidentNote.IncidentNoteChildren$` — use with `| async` in templates
//        • Promise:    `incidentNote.IncidentNoteChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="incidentNote.IncidentNoteChildren$ | async"`), or
//        • Access the promise getter (`incidentNote.IncidentNoteChildren` or `await incidentNote.IncidentNoteChildren`)
//    - Simply reading `incidentNote.IncidentNoteChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await incidentNote.Reload()` to refresh the entire object and clear all lazy caches.
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
export class IncidentNoteData {
    id!: bigint | number;
    incidentId!: bigint | number;
    authorObjectGuid!: string;
    createdAt!: string;      // ISO 8601
    content!: string;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    incident: IncidentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _incidentNoteChangeHistories: IncidentNoteChangeHistoryData[] | null = null;
    private _incidentNoteChangeHistoriesPromise: Promise<IncidentNoteChangeHistoryData[]> | null  = null;
    private _incidentNoteChangeHistoriesSubject = new BehaviorSubject<IncidentNoteChangeHistoryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<IncidentNoteData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<IncidentNoteData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<IncidentNoteData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public IncidentNoteChangeHistories$ = this._incidentNoteChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._incidentNoteChangeHistories === null && this._incidentNoteChangeHistoriesPromise === null) {
            this.loadIncidentNoteChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public IncidentNoteChangeHistoriesCount$ = IncidentNoteChangeHistoryService.Instance.GetIncidentNoteChangeHistoriesRowCount({incidentNoteId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any IncidentNoteData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.incidentNote.Reload();
  //
  //  Non Async:
  //
  //     incidentNote[0].Reload().then(x => {
  //        this.incidentNote = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      IncidentNoteService.Instance.GetIncidentNote(this.id, includeRelations)
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
     this._incidentNoteChangeHistories = null;
     this._incidentNoteChangeHistoriesPromise = null;
     this._incidentNoteChangeHistoriesSubject.next(null);

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
     * Gets the IncidentNoteChangeHistories for this IncidentNote.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.incidentNote.IncidentNoteChangeHistories.then(incidentNotes => { ... })
     *   or
     *   await this.incidentNote.incidentNotes
     *
    */
    public get IncidentNoteChangeHistories(): Promise<IncidentNoteChangeHistoryData[]> {
        if (this._incidentNoteChangeHistories !== null) {
            return Promise.resolve(this._incidentNoteChangeHistories);
        }

        if (this._incidentNoteChangeHistoriesPromise !== null) {
            return this._incidentNoteChangeHistoriesPromise;
        }

        // Start the load
        this.loadIncidentNoteChangeHistories();

        return this._incidentNoteChangeHistoriesPromise!;
    }



    private loadIncidentNoteChangeHistories(): void {

        this._incidentNoteChangeHistoriesPromise = lastValueFrom(
            IncidentNoteService.Instance.GetIncidentNoteChangeHistoriesForIncidentNote(this.id)
        )
        .then(IncidentNoteChangeHistories => {
            this._incidentNoteChangeHistories = IncidentNoteChangeHistories ?? [];
            this._incidentNoteChangeHistoriesSubject.next(this._incidentNoteChangeHistories);
            return this._incidentNoteChangeHistories;
         })
        .catch(err => {
            this._incidentNoteChangeHistories = [];
            this._incidentNoteChangeHistoriesSubject.next(this._incidentNoteChangeHistories);
            throw err;
        })
        .finally(() => {
            this._incidentNoteChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached IncidentNoteChangeHistory. Call after mutations to force refresh.
     */
    public ClearIncidentNoteChangeHistoriesCache(): void {
        this._incidentNoteChangeHistories = null;
        this._incidentNoteChangeHistoriesPromise = null;
        this._incidentNoteChangeHistoriesSubject.next(this._incidentNoteChangeHistories);      // Emit to observable
    }

    public get HasIncidentNoteChangeHistories(): Promise<boolean> {
        return this.IncidentNoteChangeHistories.then(incidentNoteChangeHistories => incidentNoteChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (incidentNote.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await incidentNote.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<IncidentNoteData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<IncidentNoteData>> {
        const info = await lastValueFrom(
            IncidentNoteService.Instance.GetIncidentNoteChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this IncidentNoteData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this IncidentNoteData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): IncidentNoteSubmitData {
        return IncidentNoteService.Instance.ConvertToIncidentNoteSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class IncidentNoteService extends SecureEndpointBase {

    private static _instance: IncidentNoteService;
    private listCache: Map<string, Observable<Array<IncidentNoteData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<IncidentNoteBasicListData>>>;
    private recordCache: Map<string, Observable<IncidentNoteData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private incidentNoteChangeHistoryService: IncidentNoteChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<IncidentNoteData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<IncidentNoteBasicListData>>>();
        this.recordCache = new Map<string, Observable<IncidentNoteData>>();

        IncidentNoteService._instance = this;
    }

    public static get Instance(): IncidentNoteService {
      return IncidentNoteService._instance;
    }


    public ClearListCaches(config: IncidentNoteQueryParameters | null = null) {

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


    public ConvertToIncidentNoteSubmitData(data: IncidentNoteData): IncidentNoteSubmitData {

        let output = new IncidentNoteSubmitData();

        output.id = data.id;
        output.incidentId = data.incidentId;
        output.authorObjectGuid = data.authorObjectGuid;
        output.createdAt = data.createdAt;
        output.content = data.content;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetIncidentNote(id: bigint | number, includeRelations: boolean = true) : Observable<IncidentNoteData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const incidentNote$ = this.requestIncidentNote(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IncidentNote", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, incidentNote$);

            return incidentNote$;
        }

        return this.recordCache.get(configHash) as Observable<IncidentNoteData>;
    }

    private requestIncidentNote(id: bigint | number, includeRelations: boolean = true) : Observable<IncidentNoteData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentNoteData>(this.baseUrl + 'api/IncidentNote/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveIncidentNote(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentNote(id, includeRelations));
            }));
    }

    public GetIncidentNoteList(config: IncidentNoteQueryParameters | any = null) : Observable<Array<IncidentNoteData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const incidentNoteList$ = this.requestIncidentNoteList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IncidentNote list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, incidentNoteList$);

            return incidentNoteList$;
        }

        return this.listCache.get(configHash) as Observable<Array<IncidentNoteData>>;
    }


    private requestIncidentNoteList(config: IncidentNoteQueryParameters | any) : Observable <Array<IncidentNoteData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IncidentNoteData>>(this.baseUrl + 'api/IncidentNotes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveIncidentNoteList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentNoteList(config));
            }));
    }

    public GetIncidentNotesRowCount(config: IncidentNoteQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const incidentNotesRowCount$ = this.requestIncidentNotesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get IncidentNotes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, incidentNotesRowCount$);

            return incidentNotesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestIncidentNotesRowCount(config: IncidentNoteQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/IncidentNotes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentNotesRowCount(config));
            }));
    }

    public GetIncidentNotesBasicListData(config: IncidentNoteQueryParameters | any = null) : Observable<Array<IncidentNoteBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const incidentNotesBasicListData$ = this.requestIncidentNotesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get IncidentNotes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, incidentNotesBasicListData$);

            return incidentNotesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<IncidentNoteBasicListData>>;
    }


    private requestIncidentNotesBasicListData(config: IncidentNoteQueryParameters | any) : Observable<Array<IncidentNoteBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IncidentNoteBasicListData>>(this.baseUrl + 'api/IncidentNotes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIncidentNotesBasicListData(config));
            }));

    }


    public PutIncidentNote(id: bigint | number, incidentNote: IncidentNoteSubmitData) : Observable<IncidentNoteData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IncidentNoteData>(this.baseUrl + 'api/IncidentNote/' + id.toString(), incidentNote, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncidentNote(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutIncidentNote(id, incidentNote));
            }));
    }


    public PostIncidentNote(incidentNote: IncidentNoteSubmitData) : Observable<IncidentNoteData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<IncidentNoteData>(this.baseUrl + 'api/IncidentNote', incidentNote, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncidentNote(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostIncidentNote(incidentNote));
            }));
    }

  
    public DeleteIncidentNote(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/IncidentNote/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteIncidentNote(id));
            }));
    }

    public RollbackIncidentNote(id: bigint | number, versionNumber: bigint | number) : Observable<IncidentNoteData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IncidentNoteData>(this.baseUrl + 'api/IncidentNote/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIncidentNote(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackIncidentNote(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a IncidentNote.
     */
    public GetIncidentNoteChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<IncidentNoteData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<IncidentNoteData>>(this.baseUrl + 'api/IncidentNote/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetIncidentNoteChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a IncidentNote.
     */
    public GetIncidentNoteAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<IncidentNoteData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<IncidentNoteData>[]>(this.baseUrl + 'api/IncidentNote/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetIncidentNoteAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a IncidentNote.
     */
    public GetIncidentNoteVersion(id: bigint | number, version: number): Observable<IncidentNoteData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentNoteData>(this.baseUrl + 'api/IncidentNote/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveIncidentNote(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetIncidentNoteVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a IncidentNote at a specific point in time.
     */
    public GetIncidentNoteStateAtTime(id: bigint | number, time: string): Observable<IncidentNoteData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IncidentNoteData>(this.baseUrl + 'api/IncidentNote/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveIncidentNote(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetIncidentNoteStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: IncidentNoteQueryParameters | any): string {

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

    public userIsAlertingIncidentNoteReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsAlertingIncidentNoteReader = this.authService.isAlertingReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Alerting.IncidentNotes
        //
        if (userIsAlertingIncidentNoteReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsAlertingIncidentNoteReader = user.readPermission >= 0;
            } else {
                userIsAlertingIncidentNoteReader = false;
            }
        }

        return userIsAlertingIncidentNoteReader;
    }


    public userIsAlertingIncidentNoteWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsAlertingIncidentNoteWriter = this.authService.isAlertingReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Alerting.IncidentNotes
        //
        if (userIsAlertingIncidentNoteWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsAlertingIncidentNoteWriter = user.writePermission >= 0;
          } else {
            userIsAlertingIncidentNoteWriter = false;
          }      
        }

        return userIsAlertingIncidentNoteWriter;
    }

    public GetIncidentNoteChangeHistoriesForIncidentNote(incidentNoteId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<IncidentNoteChangeHistoryData[]> {
        return this.incidentNoteChangeHistoryService.GetIncidentNoteChangeHistoryList({
            incidentNoteId: incidentNoteId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full IncidentNoteData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the IncidentNoteData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when IncidentNoteTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveIncidentNote(raw: any): IncidentNoteData {
    if (!raw) return raw;

    //
    // Create a IncidentNoteData object instance with correct prototype
    //
    const revived = Object.create(IncidentNoteData.prototype) as IncidentNoteData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._incidentNoteChangeHistories = null;
    (revived as any)._incidentNoteChangeHistoriesPromise = null;
    (revived as any)._incidentNoteChangeHistoriesSubject = new BehaviorSubject<IncidentNoteChangeHistoryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadIncidentNoteXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).IncidentNoteChangeHistories$ = (revived as any)._incidentNoteChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._incidentNoteChangeHistories === null && (revived as any)._incidentNoteChangeHistoriesPromise === null) {
                (revived as any).loadIncidentNoteChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).IncidentNoteChangeHistoriesCount$ = IncidentNoteChangeHistoryService.Instance.GetIncidentNoteChangeHistoriesRowCount({incidentNoteId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<IncidentNoteData> | null>(null);

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

  private ReviveIncidentNoteList(rawList: any[]): IncidentNoteData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveIncidentNote(raw));
  }

}
