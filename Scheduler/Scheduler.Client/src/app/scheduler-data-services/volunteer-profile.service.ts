/*

   GENERATED SERVICE FOR THE VOLUNTEERPROFILE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the VolunteerProfile table.

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
import { ResourceData } from './resource.service';
import { VolunteerStatusData } from './volunteer-status.service';
import { ConstituentData } from './constituent.service';
import { IconData } from './icon.service';
import { VolunteerProfileChangeHistoryService, VolunteerProfileChangeHistoryData } from './volunteer-profile-change-history.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class VolunteerProfileQueryParameters {
    resourceId: bigint | number | null | undefined = null;
    volunteerStatusId: bigint | number | null | undefined = null;
    onboardedDate: string | null | undefined = null;        // ISO 8601
    inactiveSince: string | null | undefined = null;        // ISO 8601
    totalHoursServed: number | null | undefined = null;
    lastActivityDate: string | null | undefined = null;        // ISO 8601
    backgroundCheckCompleted: boolean | null | undefined = null;
    backgroundCheckDate: string | null | undefined = null;        // ISO 8601
    backgroundCheckExpiry: string | null | undefined = null;        // ISO 8601
    confidentialityAgreementSigned: boolean | null | undefined = null;
    confidentialityAgreementDate: string | null | undefined = null;        // ISO 8601
    availabilityPreferences: string | null | undefined = null;
    interestsAndSkillsNotes: string | null | undefined = null;
    emergencyContactNotes: string | null | undefined = null;
    constituentId: bigint | number | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
    attributes: string | null | undefined = null;
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
export class VolunteerProfileSubmitData {
    id!: bigint | number;
    resourceId!: bigint | number;
    volunteerStatusId!: bigint | number;
    onboardedDate: string | null = null;     // ISO 8601
    inactiveSince: string | null = null;     // ISO 8601
    totalHoursServed: number | null = null;
    lastActivityDate: string | null = null;     // ISO 8601
    backgroundCheckCompleted!: boolean;
    backgroundCheckDate: string | null = null;     // ISO 8601
    backgroundCheckExpiry: string | null = null;     // ISO 8601
    confidentialityAgreementSigned!: boolean;
    confidentialityAgreementDate: string | null = null;     // ISO 8601
    availabilityPreferences: string | null = null;
    interestsAndSkillsNotes: string | null = null;
    emergencyContactNotes: string | null = null;
    constituentId: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    attributes: string | null = null;
    objectGuid: string = '00000000-0000-0000-0000-000000000000';
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

export class VolunteerProfileBasicListData {
    id!: bigint | number;
    name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. VolunteerProfileChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `volunteerProfile.VolunteerProfileChildren$` — use with `| async` in templates
//        • Promise:    `volunteerProfile.VolunteerProfileChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="volunteerProfile.VolunteerProfileChildren$ | async"`), or
//        • Access the promise getter (`volunteerProfile.VolunteerProfileChildren` or `await volunteerProfile.VolunteerProfileChildren`)
//    - Simply reading `volunteerProfile.VolunteerProfileChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await volunteerProfile.Reload()` to refresh the entire object and clear all lazy caches.
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
export class VolunteerProfileData {
    id!: bigint | number;
    resourceId!: bigint | number;
    volunteerStatusId!: bigint | number;
    onboardedDate!: string | null;   // ISO 8601
    inactiveSince!: string | null;   // ISO 8601
    totalHoursServed!: number | null;
    lastActivityDate!: string | null;   // ISO 8601
    backgroundCheckCompleted!: boolean;
    backgroundCheckDate!: string | null;   // ISO 8601
    backgroundCheckExpiry!: string | null;   // ISO 8601
    confidentialityAgreementSigned!: boolean;
    confidentialityAgreementDate!: string | null;   // ISO 8601
    availabilityPreferences!: string | null;
    interestsAndSkillsNotes!: string | null;
    emergencyContactNotes!: string | null;
    constituentId!: bigint | number;
    iconId!: bigint | number;
    color!: string | null;
    attributes!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    constituent: ConstituentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    resource: ResourceData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    volunteerStatus: VolunteerStatusData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _volunteerProfileChangeHistories: VolunteerProfileChangeHistoryData[] | null = null;
    private _volunteerProfileChangeHistoriesPromise: Promise<VolunteerProfileChangeHistoryData[]> | null = null;
    private _volunteerProfileChangeHistoriesSubject = new BehaviorSubject<VolunteerProfileChangeHistoryData[] | null>(null);




    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<VolunteerProfileData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<VolunteerProfileData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<VolunteerProfileData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public VolunteerProfileChangeHistories$ = this._volunteerProfileChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
            if (this._volunteerProfileChangeHistories === null && this._volunteerProfileChangeHistoriesPromise === null) {
                this.loadVolunteerProfileChangeHistories(); // Private method to start fetch
            }
        }),
        shareReplay(1) // Cache last emit
    );


    public VolunteerProfileChangeHistoriesCount$ = VolunteerProfileChangeHistoryService.Instance.GetVolunteerProfileChangeHistoriesRowCount({
        volunteerProfileId: this.id,
        active: true,
        deleted: false
    });




    //
    // Full reload — refreshes the entire object and clears all lazy caches 
    //
    // Promise based reload method to allow rebuilding of any VolunteerProfileData object with all of it's relations on demand.  Useful for navigating into nav property
    // objects and getting full state after put or post that may not have returned all nav properties.
    //
    // Usage examples:;
    //
    //  Async:
    //   await this.volunteerProfile.Reload();
    //
    //  Non Async:
    //
    //     volunteerProfile[0].Reload().then(x => {
    //        this.volunteerProfile = x;
    //    });
    //
    public async Reload(includeRelations: boolean = true): Promise<this> {

        const fresh = await lastValueFrom(
            VolunteerProfileService.Instance.GetVolunteerProfile(this.id, includeRelations)
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
        this._volunteerProfileChangeHistories = null;
        this._volunteerProfileChangeHistoriesPromise = null;
        this._volunteerProfileChangeHistoriesSubject.next(null);

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
     * Gets the VolunteerProfileChangeHistories for this VolunteerProfile.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.volunteerProfile.VolunteerProfileChangeHistories.then(volunteerProfiles => { ... })
     *   or
     *   await this.volunteerProfile.volunteerProfiles
     *
    */
    public get VolunteerProfileChangeHistories(): Promise<VolunteerProfileChangeHistoryData[]> {
        if (this._volunteerProfileChangeHistories !== null) {
            return Promise.resolve(this._volunteerProfileChangeHistories);
        }

        if (this._volunteerProfileChangeHistoriesPromise !== null) {
            return this._volunteerProfileChangeHistoriesPromise;
        }

        // Start the load
        this.loadVolunteerProfileChangeHistories();

        return this._volunteerProfileChangeHistoriesPromise!;
    }



    private loadVolunteerProfileChangeHistories(): void {

        this._volunteerProfileChangeHistoriesPromise = lastValueFrom(
            VolunteerProfileService.Instance.GetVolunteerProfileChangeHistoriesForVolunteerProfile(this.id)
        )
            .then(VolunteerProfileChangeHistories => {
                this._volunteerProfileChangeHistories = VolunteerProfileChangeHistories ?? [];
                this._volunteerProfileChangeHistoriesSubject.next(this._volunteerProfileChangeHistories);
                return this._volunteerProfileChangeHistories;
            })
            .catch(err => {
                this._volunteerProfileChangeHistories = [];
                this._volunteerProfileChangeHistoriesSubject.next(this._volunteerProfileChangeHistories);
                throw err;
            })
            .finally(() => {
                this._volunteerProfileChangeHistoriesPromise = null; // Allow retry if needed
            });
    }

    /**
     * Clears the cached VolunteerProfileChangeHistory. Call after mutations to force refresh.
     */
    public ClearVolunteerProfileChangeHistoriesCache(): void {
        this._volunteerProfileChangeHistories = null;
        this._volunteerProfileChangeHistoriesPromise = null;
        this._volunteerProfileChangeHistoriesSubject.next(this._volunteerProfileChangeHistories);      // Emit to observable
    }

    public get HasVolunteerProfileChangeHistories(): Promise<boolean> {
        return this.VolunteerProfileChangeHistories.then(volunteerProfileChangeHistories => volunteerProfileChangeHistories.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (volunteerProfile.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await volunteerProfile.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<VolunteerProfileData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<VolunteerProfileData>> {
        const info = await lastValueFrom(
            VolunteerProfileService.Instance.GetVolunteerProfileChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this VolunteerProfileData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this VolunteerProfileData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): VolunteerProfileSubmitData {
        return VolunteerProfileService.Instance.ConvertToVolunteerProfileSubmitData(this);
    }
}


@Injectable({
    providedIn: 'root'
})
export class VolunteerProfileService extends SecureEndpointBase {

    private static _instance: VolunteerProfileService;
    private listCache: Map<string, Observable<Array<VolunteerProfileData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<VolunteerProfileBasicListData>>>;
    private recordCache: Map<string, Observable<VolunteerProfileData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private volunteerProfileChangeHistoryService: VolunteerProfileChangeHistoryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<VolunteerProfileData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<VolunteerProfileBasicListData>>>();
        this.recordCache = new Map<string, Observable<VolunteerProfileData>>();

        VolunteerProfileService._instance = this;
    }

    public static get Instance(): VolunteerProfileService {
        return VolunteerProfileService._instance;
    }


    public ClearListCaches(config: VolunteerProfileQueryParameters | null = null) {

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


    public ConvertToVolunteerProfileSubmitData(data: VolunteerProfileData): VolunteerProfileSubmitData {

        let output = new VolunteerProfileSubmitData();

        output.id = data.id;
        output.resourceId = data.resourceId;
        output.volunteerStatusId = data.volunteerStatusId;
        output.onboardedDate = data.onboardedDate;
        output.inactiveSince = data.inactiveSince;
        output.totalHoursServed = data.totalHoursServed;
        output.lastActivityDate = data.lastActivityDate;
        output.backgroundCheckCompleted = data.backgroundCheckCompleted;
        output.backgroundCheckDate = data.backgroundCheckDate;
        output.backgroundCheckExpiry = data.backgroundCheckExpiry;
        output.confidentialityAgreementSigned = data.confidentialityAgreementSigned;
        output.confidentialityAgreementDate = data.confidentialityAgreementDate;
        output.availabilityPreferences = data.availabilityPreferences;
        output.interestsAndSkillsNotes = data.interestsAndSkillsNotes;
        output.emergencyContactNotes = data.emergencyContactNotes;
        output.constituentId = data.constituentId;
        output.iconId = data.iconId;
        output.color = data.color;
        output.attributes = data.attributes;
        output.objectGuid = data.objectGuid;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetVolunteerProfile(id: bigint | number, includeRelations: boolean = true): Observable<VolunteerProfileData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const volunteerProfile$ = this.requestVolunteerProfile(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerProfile", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, volunteerProfile$);

            return volunteerProfile$;
        }

        return this.recordCache.get(configHash) as Observable<VolunteerProfileData>;
    }

    private requestVolunteerProfile(id: bigint | number, includeRelations: boolean = true): Observable<VolunteerProfileData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VolunteerProfileData>(this.baseUrl + 'api/VolunteerProfile/' + id.toString(), {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveVolunteerProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerProfile(id, includeRelations));
            }));
    }

    public GetVolunteerProfileList(config: VolunteerProfileQueryParameters | any = null): Observable<Array<VolunteerProfileData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const volunteerProfileList$ = this.requestVolunteerProfileList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerProfile list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, volunteerProfileList$);

            return volunteerProfileList$;
        }

        return this.listCache.get(configHash) as Observable<Array<VolunteerProfileData>>;
    }


    private requestVolunteerProfileList(config: VolunteerProfileQueryParameters | any): Observable<Array<VolunteerProfileData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<VolunteerProfileData>>(this.baseUrl + 'api/VolunteerProfiles', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(rawList => this.ReviveVolunteerProfileList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerProfileList(config));
            }));
    }

    public GetVolunteerProfilesRowCount(config: VolunteerProfileQueryParameters | any = null): Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const volunteerProfilesRowCount$ = this.requestVolunteerProfilesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerProfiles row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, volunteerProfilesRowCount$);

            return volunteerProfilesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestVolunteerProfilesRowCount(config: VolunteerProfileQueryParameters | any): Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/VolunteerProfiles/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerProfilesRowCount(config));
            }));
    }

    public GetVolunteerProfilesBasicListData(config: VolunteerProfileQueryParameters | any = null): Observable<Array<VolunteerProfileBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const volunteerProfilesBasicListData$ = this.requestVolunteerProfilesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get VolunteerProfiles basic list data", error);

                    return throwError(() => error);
                })
            );

            this.basicListDataCache.set(configHash, volunteerProfilesBasicListData$);

            return volunteerProfilesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<VolunteerProfileBasicListData>>;
    }


    private requestVolunteerProfilesBasicListData(config: VolunteerProfileQueryParameters | any): Observable<Array<VolunteerProfileBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<VolunteerProfileBasicListData>>(this.baseUrl + 'api/VolunteerProfiles/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestVolunteerProfilesBasicListData(config));
            }));

    }


    public PutVolunteerProfile(id: bigint | number, volunteerProfile: VolunteerProfileSubmitData): Observable<VolunteerProfileData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<VolunteerProfileData>(this.baseUrl + 'api/VolunteerProfile/' + id.toString(), volunteerProfile, { headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveVolunteerProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutVolunteerProfile(id, volunteerProfile));
            }));
    }


    public PostVolunteerProfile(volunteerProfile: VolunteerProfileSubmitData): Observable<VolunteerProfileData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<VolunteerProfileData>(this.baseUrl + 'api/VolunteerProfile', volunteerProfile, { headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveVolunteerProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PostVolunteerProfile(volunteerProfile));
            }));
    }


    public DeleteVolunteerProfile(id: bigint | number): Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/VolunteerProfile/' + id.toString(), { headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteVolunteerProfile(id));
            }));
    }

    public RollbackVolunteerProfile(id: bigint | number, versionNumber: bigint | number): Observable<VolunteerProfileData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<VolunteerProfileData>(this.baseUrl + 'api/VolunteerProfile/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveVolunteerProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackVolunteerProfile(id, versionNumber));
            }));
    }


    /**
     * Gets version metadata for a specific version of a VolunteerProfile.
     */
    public GetVolunteerProfileChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<VolunteerProfileData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<VolunteerProfileData>>(this.baseUrl + 'api/VolunteerProfile/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetVolunteerProfileChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a VolunteerProfile.
     */
    public GetVolunteerProfileAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<VolunteerProfileData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<VolunteerProfileData>[]>(this.baseUrl + 'api/VolunteerProfile/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetVolunteerProfileAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a VolunteerProfile.
     */
    public GetVolunteerProfileVersion(id: bigint | number, version: number): Observable<VolunteerProfileData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VolunteerProfileData>(this.baseUrl + 'api/VolunteerProfile/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveVolunteerProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetVolunteerProfileVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a VolunteerProfile at a specific point in time.
     */
    public GetVolunteerProfileStateAtTime(id: bigint | number, time: string): Observable<VolunteerProfileData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VolunteerProfileData>(this.baseUrl + 'api/VolunteerProfile/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveVolunteerProfile(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetVolunteerProfileStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: VolunteerProfileQueryParameters | any): string {

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

    public userIsSchedulerVolunteerProfileReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerVolunteerProfileReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.VolunteerProfiles
        //
        if (userIsSchedulerVolunteerProfileReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerVolunteerProfileReader = user.readPermission >= 1;
            } else {
                userIsSchedulerVolunteerProfileReader = false;
            }
        }

        return userIsSchedulerVolunteerProfileReader;
    }


    public userIsSchedulerVolunteerProfileWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerVolunteerProfileWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.VolunteerProfiles
        //
        if (userIsSchedulerVolunteerProfileWriter == true) {
            let user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerVolunteerProfileWriter = user.writePermission >= 40;
            } else {
                userIsSchedulerVolunteerProfileWriter = false;
            }
        }

        return userIsSchedulerVolunteerProfileWriter;
    }

    public GetVolunteerProfileChangeHistoriesForVolunteerProfile(volunteerProfileId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<VolunteerProfileChangeHistoryData[]> {
        return this.volunteerProfileChangeHistoryService.GetVolunteerProfileChangeHistoryList({
            volunteerProfileId: volunteerProfileId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    /**
      *
      * Revives a plain object from the server into a full VolunteerProfileData instance.
      *
      * This is critical for the lazy-loading pattern to work correctly.
      *
      * When the server returns JSON, it is a plain object with no prototype methods
      * or observable properties. This method:
      * 1. Re-attaches the VolunteerProfileData prototype
      * 2. Copies all properties from the raw object
      * 3. Re-initializes all private caches and BehaviorSubjects
      * 4. Re-creates all public observable properties ($ suffixed) with their
      *    original tap() triggers that initiate lazy loading on first subscription
      *
      * Without this, revived objects would not trigger loads when VolunteerProfileTags$ etc.
      * are subscribed to in templates.
      *
      */
    public ReviveVolunteerProfile(raw: any): VolunteerProfileData {
        if (!raw) return raw;

        //
        // Create a VolunteerProfileData object instance with correct prototype
        //
        const revived = Object.create(VolunteerProfileData.prototype) as VolunteerProfileData;

        //
        // Copy all raw properties
        //
        Object.assign(revived, raw);

        //
        // Explicitly initialize all private caches
        // This ensures the getters work correctly on revived objects
        //
        (revived as any)._volunteerProfileChangeHistories = null;
        (revived as any)._volunteerProfileChangeHistoriesPromise = null;
        (revived as any)._volunteerProfileChangeHistoriesSubject = new BehaviorSubject<VolunteerProfileChangeHistoryData[] | null>(null);


        //
        // Re-attach ALL public observables with their lazy-load tap() triggers
        // This mirrors the original class definition exactly
        //
        //
        // Re-create all public observables with their lazy-load triggers
        // We use 'as any' because:
        // 1. The revived object has the correct prototype
        // 2. But private methods (loadVolunteerProfileXYZ, etc.) are not accessible via the typed variable
        // 3. This is a controlled revival context — safe and necessary
        //
        (revived as any).VolunteerProfileChangeHistories$ = (revived as any)._volunteerProfileChangeHistoriesSubject.asObservable().pipe(
            tap(() => {
                if ((revived as any)._volunteerProfileChangeHistories === null && (revived as any)._volunteerProfileChangeHistoriesPromise === null) {
                    (revived as any).loadVolunteerProfileChangeHistories();        // Need to cast to any to invoke private load method
                }
            }),
            shareReplay(1)
        );

        (revived as any).VolunteerProfileChangeHistoriesCount$ = VolunteerProfileChangeHistoryService.Instance.GetVolunteerProfileChangeHistoriesRowCount({
            volunteerProfileId: (revived as any).id,
            active: true,
            deleted: false
        });




        //
        // Version history metadata cache and observable
        //
        (revived as any)._currentVersionInfo = null;
        (revived as any)._currentVersionInfoPromise = null;
        (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<VolunteerProfileData> | null>(null);

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

    private ReviveVolunteerProfileList(rawList: any[]): VolunteerProfileData[] {

        if (!rawList) {
            return [];
        }

        return rawList.map(raw => this.ReviveVolunteerProfile(raw));
    }

}
