/*

   GENERATED SERVICE FOR THE PLACEDBRICK TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PlacedBrick table.

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
import { ProjectData } from './project.service';
import { BrickPartData } from './brick-part.service';
import { BrickColourData } from './brick-colour.service';
import { PlacedBrickChangeHistoryService, PlacedBrickChangeHistoryData } from './placed-brick-change-history.service';
import { SubmodelPlacedBrickService, SubmodelPlacedBrickData } from './submodel-placed-brick.service';
import { BuildStepPartService, BuildStepPartData } from './build-step-part.service';
import { BuildStepAnnotationService, BuildStepAnnotationData } from './build-step-annotation.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PlacedBrickQueryParameters {
    projectId: bigint | number | null | undefined = null;
    brickPartId: bigint | number | null | undefined = null;
    brickColourId: bigint | number | null | undefined = null;
    positionX: number | null | undefined = null;
    positionY: number | null | undefined = null;
    positionZ: number | null | undefined = null;
    rotationX: number | null | undefined = null;
    rotationY: number | null | undefined = null;
    rotationZ: number | null | undefined = null;
    rotationW: number | null | undefined = null;
    buildStepNumber: bigint | number | null | undefined = null;
    isHidden: boolean | null | undefined = null;
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
export class PlacedBrickSubmitData {
    id!: bigint | number;
    projectId!: bigint | number;
    brickPartId!: bigint | number;
    brickColourId!: bigint | number;
    positionX: number | null = null;
    positionY: number | null = null;
    positionZ: number | null = null;
    rotationX: number | null = null;
    rotationY: number | null = null;
    rotationZ: number | null = null;
    rotationW: number | null = null;
    buildStepNumber: bigint | number | null = null;
    isHidden!: boolean;
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

export class PlacedBrickBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PlacedBrickChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `placedBrick.PlacedBrickChildren$` — use with `| async` in templates
//        • Promise:    `placedBrick.PlacedBrickChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="placedBrick.PlacedBrickChildren$ | async"`), or
//        • Access the promise getter (`placedBrick.PlacedBrickChildren` or `await placedBrick.PlacedBrickChildren`)
//    - Simply reading `placedBrick.PlacedBrickChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await placedBrick.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PlacedBrickData {
    id!: bigint | number;
    projectId!: bigint | number;
    brickPartId!: bigint | number;
    brickColourId!: bigint | number;
    positionX!: number | null;
    positionY!: number | null;
    positionZ!: number | null;
    rotationX!: number | null;
    rotationY!: number | null;
    rotationZ!: number | null;
    rotationW!: number | null;
    buildStepNumber!: bigint | number;
    isHidden!: boolean;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    brickColour: BrickColourData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    brickPart: BrickPartData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    project: ProjectData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _placedBrickChangeHistories: PlacedBrickChangeHistoryData[] | null = null;
    private _placedBrickChangeHistoriesPromise: Promise<PlacedBrickChangeHistoryData[]> | null  = null;
    private _placedBrickChangeHistoriesSubject = new BehaviorSubject<PlacedBrickChangeHistoryData[] | null>(null);

                
    private _submodelPlacedBricks: SubmodelPlacedBrickData[] | null = null;
    private _submodelPlacedBricksPromise: Promise<SubmodelPlacedBrickData[]> | null  = null;
    private _submodelPlacedBricksSubject = new BehaviorSubject<SubmodelPlacedBrickData[] | null>(null);

                
    private _buildStepParts: BuildStepPartData[] | null = null;
    private _buildStepPartsPromise: Promise<BuildStepPartData[]> | null  = null;
    private _buildStepPartsSubject = new BehaviorSubject<BuildStepPartData[] | null>(null);

                
    private _buildStepAnnotations: BuildStepAnnotationData[] | null = null;
    private _buildStepAnnotationsPromise: Promise<BuildStepAnnotationData[]> | null  = null;
    private _buildStepAnnotationsSubject = new BehaviorSubject<BuildStepAnnotationData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<PlacedBrickData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<PlacedBrickData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PlacedBrickData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public PlacedBrickChangeHistories$ = this._placedBrickChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._placedBrickChangeHistories === null && this._placedBrickChangeHistoriesPromise === null) {
            this.loadPlacedBrickChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public PlacedBrickChangeHistoriesCount$ = PlacedBrickChangeHistoryService.Instance.GetPlacedBrickChangeHistoriesRowCount({placedBrickId: this.id,
      active: true,
      deleted: false
    });



    public SubmodelPlacedBricks$ = this._submodelPlacedBricksSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._submodelPlacedBricks === null && this._submodelPlacedBricksPromise === null) {
            this.loadSubmodelPlacedBricks(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SubmodelPlacedBricksCount$ = SubmodelPlacedBrickService.Instance.GetSubmodelPlacedBricksRowCount({placedBrickId: this.id,
      active: true,
      deleted: false
    });



    public BuildStepParts$ = this._buildStepPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._buildStepParts === null && this._buildStepPartsPromise === null) {
            this.loadBuildStepParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public BuildStepPartsCount$ = BuildStepPartService.Instance.GetBuildStepPartsRowCount({placedBrickId: this.id,
      active: true,
      deleted: false
    });



    public BuildStepAnnotations$ = this._buildStepAnnotationsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._buildStepAnnotations === null && this._buildStepAnnotationsPromise === null) {
            this.loadBuildStepAnnotations(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public BuildStepAnnotationsCount$ = BuildStepAnnotationService.Instance.GetBuildStepAnnotationsRowCount({placedBrickId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PlacedBrickData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.placedBrick.Reload();
  //
  //  Non Async:
  //
  //     placedBrick[0].Reload().then(x => {
  //        this.placedBrick = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PlacedBrickService.Instance.GetPlacedBrick(this.id, includeRelations)
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
     this._placedBrickChangeHistories = null;
     this._placedBrickChangeHistoriesPromise = null;
     this._placedBrickChangeHistoriesSubject.next(null);

     this._submodelPlacedBricks = null;
     this._submodelPlacedBricksPromise = null;
     this._submodelPlacedBricksSubject.next(null);

     this._buildStepParts = null;
     this._buildStepPartsPromise = null;
     this._buildStepPartsSubject.next(null);

     this._buildStepAnnotations = null;
     this._buildStepAnnotationsPromise = null;
     this._buildStepAnnotationsSubject.next(null);

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
     * Gets the PlacedBrickChangeHistories for this PlacedBrick.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.placedBrick.PlacedBrickChangeHistories.then(placedBricks => { ... })
     *   or
     *   await this.placedBrick.placedBricks
     *
    */
    public get PlacedBrickChangeHistories(): Promise<PlacedBrickChangeHistoryData[]> {
        if (this._placedBrickChangeHistories !== null) {
            return Promise.resolve(this._placedBrickChangeHistories);
        }

        if (this._placedBrickChangeHistoriesPromise !== null) {
            return this._placedBrickChangeHistoriesPromise;
        }

        // Start the load
        this.loadPlacedBrickChangeHistories();

        return this._placedBrickChangeHistoriesPromise!;
    }



    private loadPlacedBrickChangeHistories(): void {

        this._placedBrickChangeHistoriesPromise = lastValueFrom(
            PlacedBrickService.Instance.GetPlacedBrickChangeHistoriesForPlacedBrick(this.id)
        )
        .then(PlacedBrickChangeHistories => {
            this._placedBrickChangeHistories = PlacedBrickChangeHistories ?? [];
            this._placedBrickChangeHistoriesSubject.next(this._placedBrickChangeHistories);
            return this._placedBrickChangeHistories;
         })
        .catch(err => {
            this._placedBrickChangeHistories = [];
            this._placedBrickChangeHistoriesSubject.next(this._placedBrickChangeHistories);
            throw err;
        })
        .finally(() => {
            this._placedBrickChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PlacedBrickChangeHistory. Call after mutations to force refresh.
     */
    public ClearPlacedBrickChangeHistoriesCache(): void {
        this._placedBrickChangeHistories = null;
        this._placedBrickChangeHistoriesPromise = null;
        this._placedBrickChangeHistoriesSubject.next(this._placedBrickChangeHistories);      // Emit to observable
    }

    public get HasPlacedBrickChangeHistories(): Promise<boolean> {
        return this.PlacedBrickChangeHistories.then(placedBrickChangeHistories => placedBrickChangeHistories.length > 0);
    }


    /**
     *
     * Gets the SubmodelPlacedBricks for this PlacedBrick.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.placedBrick.SubmodelPlacedBricks.then(placedBricks => { ... })
     *   or
     *   await this.placedBrick.placedBricks
     *
    */
    public get SubmodelPlacedBricks(): Promise<SubmodelPlacedBrickData[]> {
        if (this._submodelPlacedBricks !== null) {
            return Promise.resolve(this._submodelPlacedBricks);
        }

        if (this._submodelPlacedBricksPromise !== null) {
            return this._submodelPlacedBricksPromise;
        }

        // Start the load
        this.loadSubmodelPlacedBricks();

        return this._submodelPlacedBricksPromise!;
    }



    private loadSubmodelPlacedBricks(): void {

        this._submodelPlacedBricksPromise = lastValueFrom(
            PlacedBrickService.Instance.GetSubmodelPlacedBricksForPlacedBrick(this.id)
        )
        .then(SubmodelPlacedBricks => {
            this._submodelPlacedBricks = SubmodelPlacedBricks ?? [];
            this._submodelPlacedBricksSubject.next(this._submodelPlacedBricks);
            return this._submodelPlacedBricks;
         })
        .catch(err => {
            this._submodelPlacedBricks = [];
            this._submodelPlacedBricksSubject.next(this._submodelPlacedBricks);
            throw err;
        })
        .finally(() => {
            this._submodelPlacedBricksPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SubmodelPlacedBrick. Call after mutations to force refresh.
     */
    public ClearSubmodelPlacedBricksCache(): void {
        this._submodelPlacedBricks = null;
        this._submodelPlacedBricksPromise = null;
        this._submodelPlacedBricksSubject.next(this._submodelPlacedBricks);      // Emit to observable
    }

    public get HasSubmodelPlacedBricks(): Promise<boolean> {
        return this.SubmodelPlacedBricks.then(submodelPlacedBricks => submodelPlacedBricks.length > 0);
    }


    /**
     *
     * Gets the BuildStepParts for this PlacedBrick.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.placedBrick.BuildStepParts.then(placedBricks => { ... })
     *   or
     *   await this.placedBrick.placedBricks
     *
    */
    public get BuildStepParts(): Promise<BuildStepPartData[]> {
        if (this._buildStepParts !== null) {
            return Promise.resolve(this._buildStepParts);
        }

        if (this._buildStepPartsPromise !== null) {
            return this._buildStepPartsPromise;
        }

        // Start the load
        this.loadBuildStepParts();

        return this._buildStepPartsPromise!;
    }



    private loadBuildStepParts(): void {

        this._buildStepPartsPromise = lastValueFrom(
            PlacedBrickService.Instance.GetBuildStepPartsForPlacedBrick(this.id)
        )
        .then(BuildStepParts => {
            this._buildStepParts = BuildStepParts ?? [];
            this._buildStepPartsSubject.next(this._buildStepParts);
            return this._buildStepParts;
         })
        .catch(err => {
            this._buildStepParts = [];
            this._buildStepPartsSubject.next(this._buildStepParts);
            throw err;
        })
        .finally(() => {
            this._buildStepPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BuildStepPart. Call after mutations to force refresh.
     */
    public ClearBuildStepPartsCache(): void {
        this._buildStepParts = null;
        this._buildStepPartsPromise = null;
        this._buildStepPartsSubject.next(this._buildStepParts);      // Emit to observable
    }

    public get HasBuildStepParts(): Promise<boolean> {
        return this.BuildStepParts.then(buildStepParts => buildStepParts.length > 0);
    }


    /**
     *
     * Gets the BuildStepAnnotations for this PlacedBrick.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.placedBrick.BuildStepAnnotations.then(placedBricks => { ... })
     *   or
     *   await this.placedBrick.placedBricks
     *
    */
    public get BuildStepAnnotations(): Promise<BuildStepAnnotationData[]> {
        if (this._buildStepAnnotations !== null) {
            return Promise.resolve(this._buildStepAnnotations);
        }

        if (this._buildStepAnnotationsPromise !== null) {
            return this._buildStepAnnotationsPromise;
        }

        // Start the load
        this.loadBuildStepAnnotations();

        return this._buildStepAnnotationsPromise!;
    }



    private loadBuildStepAnnotations(): void {

        this._buildStepAnnotationsPromise = lastValueFrom(
            PlacedBrickService.Instance.GetBuildStepAnnotationsForPlacedBrick(this.id)
        )
        .then(BuildStepAnnotations => {
            this._buildStepAnnotations = BuildStepAnnotations ?? [];
            this._buildStepAnnotationsSubject.next(this._buildStepAnnotations);
            return this._buildStepAnnotations;
         })
        .catch(err => {
            this._buildStepAnnotations = [];
            this._buildStepAnnotationsSubject.next(this._buildStepAnnotations);
            throw err;
        })
        .finally(() => {
            this._buildStepAnnotationsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BuildStepAnnotation. Call after mutations to force refresh.
     */
    public ClearBuildStepAnnotationsCache(): void {
        this._buildStepAnnotations = null;
        this._buildStepAnnotationsPromise = null;
        this._buildStepAnnotationsSubject.next(this._buildStepAnnotations);      // Emit to observable
    }

    public get HasBuildStepAnnotations(): Promise<boolean> {
        return this.BuildStepAnnotations.then(buildStepAnnotations => buildStepAnnotations.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (placedBrick.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await placedBrick.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<PlacedBrickData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<PlacedBrickData>> {
        const info = await lastValueFrom(
            PlacedBrickService.Instance.GetPlacedBrickChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this PlacedBrickData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PlacedBrickData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PlacedBrickSubmitData {
        return PlacedBrickService.Instance.ConvertToPlacedBrickSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PlacedBrickService extends SecureEndpointBase {

    private static _instance: PlacedBrickService;
    private listCache: Map<string, Observable<Array<PlacedBrickData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PlacedBrickBasicListData>>>;
    private recordCache: Map<string, Observable<PlacedBrickData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private placedBrickChangeHistoryService: PlacedBrickChangeHistoryService,
        private submodelPlacedBrickService: SubmodelPlacedBrickService,
        private buildStepPartService: BuildStepPartService,
        private buildStepAnnotationService: BuildStepAnnotationService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PlacedBrickData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PlacedBrickBasicListData>>>();
        this.recordCache = new Map<string, Observable<PlacedBrickData>>();

        PlacedBrickService._instance = this;
    }

    public static get Instance(): PlacedBrickService {
      return PlacedBrickService._instance;
    }


    public ClearListCaches(config: PlacedBrickQueryParameters | null = null) {

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


    public ConvertToPlacedBrickSubmitData(data: PlacedBrickData): PlacedBrickSubmitData {

        let output = new PlacedBrickSubmitData();

        output.id = data.id;
        output.projectId = data.projectId;
        output.brickPartId = data.brickPartId;
        output.brickColourId = data.brickColourId;
        output.positionX = data.positionX;
        output.positionY = data.positionY;
        output.positionZ = data.positionZ;
        output.rotationX = data.rotationX;
        output.rotationY = data.rotationY;
        output.rotationZ = data.rotationZ;
        output.rotationW = data.rotationW;
        output.buildStepNumber = data.buildStepNumber;
        output.isHidden = data.isHidden;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPlacedBrick(id: bigint | number, includeRelations: boolean = true) : Observable<PlacedBrickData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const placedBrick$ = this.requestPlacedBrick(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PlacedBrick", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, placedBrick$);

            return placedBrick$;
        }

        return this.recordCache.get(configHash) as Observable<PlacedBrickData>;
    }

    private requestPlacedBrick(id: bigint | number, includeRelations: boolean = true) : Observable<PlacedBrickData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PlacedBrickData>(this.baseUrl + 'api/PlacedBrick/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePlacedBrick(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPlacedBrick(id, includeRelations));
            }));
    }

    public GetPlacedBrickList(config: PlacedBrickQueryParameters | any = null) : Observable<Array<PlacedBrickData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const placedBrickList$ = this.requestPlacedBrickList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PlacedBrick list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, placedBrickList$);

            return placedBrickList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PlacedBrickData>>;
    }


    private requestPlacedBrickList(config: PlacedBrickQueryParameters | any) : Observable <Array<PlacedBrickData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PlacedBrickData>>(this.baseUrl + 'api/PlacedBricks', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePlacedBrickList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPlacedBrickList(config));
            }));
    }

    public GetPlacedBricksRowCount(config: PlacedBrickQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const placedBricksRowCount$ = this.requestPlacedBricksRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PlacedBricks row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, placedBricksRowCount$);

            return placedBricksRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPlacedBricksRowCount(config: PlacedBrickQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PlacedBricks/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPlacedBricksRowCount(config));
            }));
    }

    public GetPlacedBricksBasicListData(config: PlacedBrickQueryParameters | any = null) : Observable<Array<PlacedBrickBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const placedBricksBasicListData$ = this.requestPlacedBricksBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PlacedBricks basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, placedBricksBasicListData$);

            return placedBricksBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PlacedBrickBasicListData>>;
    }


    private requestPlacedBricksBasicListData(config: PlacedBrickQueryParameters | any) : Observable<Array<PlacedBrickBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PlacedBrickBasicListData>>(this.baseUrl + 'api/PlacedBricks/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPlacedBricksBasicListData(config));
            }));

    }


    public PutPlacedBrick(id: bigint | number, placedBrick: PlacedBrickSubmitData) : Observable<PlacedBrickData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PlacedBrickData>(this.baseUrl + 'api/PlacedBrick/' + id.toString(), placedBrick, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePlacedBrick(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPlacedBrick(id, placedBrick));
            }));
    }


    public PostPlacedBrick(placedBrick: PlacedBrickSubmitData) : Observable<PlacedBrickData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PlacedBrickData>(this.baseUrl + 'api/PlacedBrick', placedBrick, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePlacedBrick(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPlacedBrick(placedBrick));
            }));
    }

  
    public DeletePlacedBrick(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PlacedBrick/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePlacedBrick(id));
            }));
    }

    public RollbackPlacedBrick(id: bigint | number, versionNumber: bigint | number) : Observable<PlacedBrickData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PlacedBrickData>(this.baseUrl + 'api/PlacedBrick/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePlacedBrick(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackPlacedBrick(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a PlacedBrick.
     */
    public GetPlacedBrickChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<PlacedBrickData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PlacedBrickData>>(this.baseUrl + 'api/PlacedBrick/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPlacedBrickChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a PlacedBrick.
     */
    public GetPlacedBrickAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<PlacedBrickData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PlacedBrickData>[]>(this.baseUrl + 'api/PlacedBrick/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPlacedBrickAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a PlacedBrick.
     */
    public GetPlacedBrickVersion(id: bigint | number, version: number): Observable<PlacedBrickData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PlacedBrickData>(this.baseUrl + 'api/PlacedBrick/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePlacedBrick(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPlacedBrickVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a PlacedBrick at a specific point in time.
     */
    public GetPlacedBrickStateAtTime(id: bigint | number, time: string): Observable<PlacedBrickData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PlacedBrickData>(this.baseUrl + 'api/PlacedBrick/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePlacedBrick(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPlacedBrickStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: PlacedBrickQueryParameters | any): string {

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

    public userIsBMCPlacedBrickReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCPlacedBrickReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.PlacedBricks
        //
        if (userIsBMCPlacedBrickReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCPlacedBrickReader = user.readPermission >= 1;
            } else {
                userIsBMCPlacedBrickReader = false;
            }
        }

        return userIsBMCPlacedBrickReader;
    }


    public userIsBMCPlacedBrickWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCPlacedBrickWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.PlacedBricks
        //
        if (userIsBMCPlacedBrickWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCPlacedBrickWriter = user.writePermission >= 1;
          } else {
            userIsBMCPlacedBrickWriter = false;
          }      
        }

        return userIsBMCPlacedBrickWriter;
    }

    public GetPlacedBrickChangeHistoriesForPlacedBrick(placedBrickId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PlacedBrickChangeHistoryData[]> {
        return this.placedBrickChangeHistoryService.GetPlacedBrickChangeHistoryList({
            placedBrickId: placedBrickId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSubmodelPlacedBricksForPlacedBrick(placedBrickId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SubmodelPlacedBrickData[]> {
        return this.submodelPlacedBrickService.GetSubmodelPlacedBrickList({
            placedBrickId: placedBrickId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBuildStepPartsForPlacedBrick(placedBrickId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BuildStepPartData[]> {
        return this.buildStepPartService.GetBuildStepPartList({
            placedBrickId: placedBrickId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBuildStepAnnotationsForPlacedBrick(placedBrickId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BuildStepAnnotationData[]> {
        return this.buildStepAnnotationService.GetBuildStepAnnotationList({
            placedBrickId: placedBrickId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PlacedBrickData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PlacedBrickData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PlacedBrickTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePlacedBrick(raw: any): PlacedBrickData {
    if (!raw) return raw;

    //
    // Create a PlacedBrickData object instance with correct prototype
    //
    const revived = Object.create(PlacedBrickData.prototype) as PlacedBrickData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._placedBrickChangeHistories = null;
    (revived as any)._placedBrickChangeHistoriesPromise = null;
    (revived as any)._placedBrickChangeHistoriesSubject = new BehaviorSubject<PlacedBrickChangeHistoryData[] | null>(null);

    (revived as any)._submodelPlacedBricks = null;
    (revived as any)._submodelPlacedBricksPromise = null;
    (revived as any)._submodelPlacedBricksSubject = new BehaviorSubject<SubmodelPlacedBrickData[] | null>(null);

    (revived as any)._buildStepParts = null;
    (revived as any)._buildStepPartsPromise = null;
    (revived as any)._buildStepPartsSubject = new BehaviorSubject<BuildStepPartData[] | null>(null);

    (revived as any)._buildStepAnnotations = null;
    (revived as any)._buildStepAnnotationsPromise = null;
    (revived as any)._buildStepAnnotationsSubject = new BehaviorSubject<BuildStepAnnotationData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadPlacedBrickXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).PlacedBrickChangeHistories$ = (revived as any)._placedBrickChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._placedBrickChangeHistories === null && (revived as any)._placedBrickChangeHistoriesPromise === null) {
                (revived as any).loadPlacedBrickChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).PlacedBrickChangeHistoriesCount$ = PlacedBrickChangeHistoryService.Instance.GetPlacedBrickChangeHistoriesRowCount({placedBrickId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SubmodelPlacedBricks$ = (revived as any)._submodelPlacedBricksSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._submodelPlacedBricks === null && (revived as any)._submodelPlacedBricksPromise === null) {
                (revived as any).loadSubmodelPlacedBricks();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SubmodelPlacedBricksCount$ = SubmodelPlacedBrickService.Instance.GetSubmodelPlacedBricksRowCount({placedBrickId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).BuildStepParts$ = (revived as any)._buildStepPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._buildStepParts === null && (revived as any)._buildStepPartsPromise === null) {
                (revived as any).loadBuildStepParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).BuildStepPartsCount$ = BuildStepPartService.Instance.GetBuildStepPartsRowCount({placedBrickId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).BuildStepAnnotations$ = (revived as any)._buildStepAnnotationsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._buildStepAnnotations === null && (revived as any)._buildStepAnnotationsPromise === null) {
                (revived as any).loadBuildStepAnnotations();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).BuildStepAnnotationsCount$ = BuildStepAnnotationService.Instance.GetBuildStepAnnotationsRowCount({placedBrickId: (revived as any).id,
      active: true,
      deleted: false
    });




    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PlacedBrickData> | null>(null);

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

  private RevivePlacedBrickList(rawList: any[]): PlacedBrickData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePlacedBrick(raw));
  }

}
