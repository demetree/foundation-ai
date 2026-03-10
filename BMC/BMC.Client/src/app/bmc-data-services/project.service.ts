/*

   GENERATED SERVICE FOR THE PROJECT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Project table.

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
import { ProjectChangeHistoryService, ProjectChangeHistoryData } from './project-change-history.service';
import { PlacedBrickService, PlacedBrickData } from './placed-brick.service';
import { BrickConnectionService, BrickConnectionData } from './brick-connection.service';
import { SubmodelService, SubmodelData } from './submodel.service';
import { ProjectTagAssignmentService, ProjectTagAssignmentData } from './project-tag-assignment.service';
import { ProjectCameraPresetService, ProjectCameraPresetData } from './project-camera-preset.service';
import { ProjectReferenceImageService, ProjectReferenceImageData } from './project-reference-image.service';
import { ModelDocumentService, ModelDocumentData } from './model-document.service';
import { BuildManualService, BuildManualData } from './build-manual.service';
import { ProjectRenderService, ProjectRenderData } from './project-render.service';
import { ProjectExportService, ProjectExportData } from './project-export.service';
import { PublishedMocService, PublishedMocData } from './published-moc.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ProjectQueryParameters {
    userId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    notes: string | null | undefined = null;
    thumbnailImagePath: string | null | undefined = null;
    partCount: bigint | number | null | undefined = null;
    lastBuildDate: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class ProjectSubmitData {
    id!: bigint | number;
    userId: bigint | number | null = null;
    name!: string;
    description!: string;
    notes: string | null = null;
    thumbnailImagePath: string | null = null;
    thumbnailData: string | null = null;
    partCount: bigint | number | null = null;
    lastBuildDate: string | null = null;     // ISO 8601 (full datetime)
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

export class ProjectBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ProjectChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `project.ProjectChildren$` — use with `| async` in templates
//        • Promise:    `project.ProjectChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="project.ProjectChildren$ | async"`), or
//        • Access the promise getter (`project.ProjectChildren` or `await project.ProjectChildren`)
//    - Simply reading `project.ProjectChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await project.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ProjectData {
    id!: bigint | number;
    userId!: bigint | number;
    name!: string;
    description!: string;
    notes!: string | null;
    thumbnailImagePath!: string | null;
    thumbnailData!: string | null;
    partCount!: bigint | number;
    lastBuildDate!: string | null;   // ISO 8601 (full datetime)
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _projectChangeHistories: ProjectChangeHistoryData[] | null = null;
    private _projectChangeHistoriesPromise: Promise<ProjectChangeHistoryData[]> | null  = null;
    private _projectChangeHistoriesSubject = new BehaviorSubject<ProjectChangeHistoryData[] | null>(null);

                
    private _placedBricks: PlacedBrickData[] | null = null;
    private _placedBricksPromise: Promise<PlacedBrickData[]> | null  = null;
    private _placedBricksSubject = new BehaviorSubject<PlacedBrickData[] | null>(null);

                
    private _brickConnections: BrickConnectionData[] | null = null;
    private _brickConnectionsPromise: Promise<BrickConnectionData[]> | null  = null;
    private _brickConnectionsSubject = new BehaviorSubject<BrickConnectionData[] | null>(null);

                
    private _submodels: SubmodelData[] | null = null;
    private _submodelsPromise: Promise<SubmodelData[]> | null  = null;
    private _submodelsSubject = new BehaviorSubject<SubmodelData[] | null>(null);

                
    private _projectTagAssignments: ProjectTagAssignmentData[] | null = null;
    private _projectTagAssignmentsPromise: Promise<ProjectTagAssignmentData[]> | null  = null;
    private _projectTagAssignmentsSubject = new BehaviorSubject<ProjectTagAssignmentData[] | null>(null);

                
    private _projectCameraPresets: ProjectCameraPresetData[] | null = null;
    private _projectCameraPresetsPromise: Promise<ProjectCameraPresetData[]> | null  = null;
    private _projectCameraPresetsSubject = new BehaviorSubject<ProjectCameraPresetData[] | null>(null);

                
    private _projectReferenceImages: ProjectReferenceImageData[] | null = null;
    private _projectReferenceImagesPromise: Promise<ProjectReferenceImageData[]> | null  = null;
    private _projectReferenceImagesSubject = new BehaviorSubject<ProjectReferenceImageData[] | null>(null);

                
    private _modelDocuments: ModelDocumentData[] | null = null;
    private _modelDocumentsPromise: Promise<ModelDocumentData[]> | null  = null;
    private _modelDocumentsSubject = new BehaviorSubject<ModelDocumentData[] | null>(null);

                
    private _buildManuals: BuildManualData[] | null = null;
    private _buildManualsPromise: Promise<BuildManualData[]> | null  = null;
    private _buildManualsSubject = new BehaviorSubject<BuildManualData[] | null>(null);

                
    private _projectRenders: ProjectRenderData[] | null = null;
    private _projectRendersPromise: Promise<ProjectRenderData[]> | null  = null;
    private _projectRendersSubject = new BehaviorSubject<ProjectRenderData[] | null>(null);

                
    private _projectExports: ProjectExportData[] | null = null;
    private _projectExportsPromise: Promise<ProjectExportData[]> | null  = null;
    private _projectExportsSubject = new BehaviorSubject<ProjectExportData[] | null>(null);

                
    private _publishedMocs: PublishedMocData[] | null = null;
    private _publishedMocsPromise: Promise<PublishedMocData[]> | null  = null;
    private _publishedMocsSubject = new BehaviorSubject<PublishedMocData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<ProjectData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<ProjectData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ProjectData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ProjectChangeHistories$ = this._projectChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._projectChangeHistories === null && this._projectChangeHistoriesPromise === null) {
            this.loadProjectChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _projectChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get ProjectChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._projectChangeHistoriesCount$ === null) {
            this._projectChangeHistoriesCount$ = ProjectChangeHistoryService.Instance.GetProjectChangeHistoriesRowCount({projectId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._projectChangeHistoriesCount$;
    }



    public PlacedBricks$ = this._placedBricksSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._placedBricks === null && this._placedBricksPromise === null) {
            this.loadPlacedBricks(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _placedBricksCount$: Observable<bigint | number> | null = null;
    public get PlacedBricksCount$(): Observable<bigint | number> {
        if (this._placedBricksCount$ === null) {
            this._placedBricksCount$ = PlacedBrickService.Instance.GetPlacedBricksRowCount({projectId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._placedBricksCount$;
    }



    public BrickConnections$ = this._brickConnectionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickConnections === null && this._brickConnectionsPromise === null) {
            this.loadBrickConnections(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _brickConnectionsCount$: Observable<bigint | number> | null = null;
    public get BrickConnectionsCount$(): Observable<bigint | number> {
        if (this._brickConnectionsCount$ === null) {
            this._brickConnectionsCount$ = BrickConnectionService.Instance.GetBrickConnectionsRowCount({projectId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._brickConnectionsCount$;
    }



    public Submodels$ = this._submodelsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._submodels === null && this._submodelsPromise === null) {
            this.loadSubmodels(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _submodelsCount$: Observable<bigint | number> | null = null;
    public get SubmodelsCount$(): Observable<bigint | number> {
        if (this._submodelsCount$ === null) {
            this._submodelsCount$ = SubmodelService.Instance.GetSubmodelsRowCount({projectId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._submodelsCount$;
    }



    public ProjectTagAssignments$ = this._projectTagAssignmentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._projectTagAssignments === null && this._projectTagAssignmentsPromise === null) {
            this.loadProjectTagAssignments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _projectTagAssignmentsCount$: Observable<bigint | number> | null = null;
    public get ProjectTagAssignmentsCount$(): Observable<bigint | number> {
        if (this._projectTagAssignmentsCount$ === null) {
            this._projectTagAssignmentsCount$ = ProjectTagAssignmentService.Instance.GetProjectTagAssignmentsRowCount({projectId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._projectTagAssignmentsCount$;
    }



    public ProjectCameraPresets$ = this._projectCameraPresetsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._projectCameraPresets === null && this._projectCameraPresetsPromise === null) {
            this.loadProjectCameraPresets(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _projectCameraPresetsCount$: Observable<bigint | number> | null = null;
    public get ProjectCameraPresetsCount$(): Observable<bigint | number> {
        if (this._projectCameraPresetsCount$ === null) {
            this._projectCameraPresetsCount$ = ProjectCameraPresetService.Instance.GetProjectCameraPresetsRowCount({projectId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._projectCameraPresetsCount$;
    }



    public ProjectReferenceImages$ = this._projectReferenceImagesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._projectReferenceImages === null && this._projectReferenceImagesPromise === null) {
            this.loadProjectReferenceImages(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _projectReferenceImagesCount$: Observable<bigint | number> | null = null;
    public get ProjectReferenceImagesCount$(): Observable<bigint | number> {
        if (this._projectReferenceImagesCount$ === null) {
            this._projectReferenceImagesCount$ = ProjectReferenceImageService.Instance.GetProjectReferenceImagesRowCount({projectId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._projectReferenceImagesCount$;
    }



    public ModelDocuments$ = this._modelDocumentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._modelDocuments === null && this._modelDocumentsPromise === null) {
            this.loadModelDocuments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _modelDocumentsCount$: Observable<bigint | number> | null = null;
    public get ModelDocumentsCount$(): Observable<bigint | number> {
        if (this._modelDocumentsCount$ === null) {
            this._modelDocumentsCount$ = ModelDocumentService.Instance.GetModelDocumentsRowCount({projectId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._modelDocumentsCount$;
    }



    public BuildManuals$ = this._buildManualsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._buildManuals === null && this._buildManualsPromise === null) {
            this.loadBuildManuals(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _buildManualsCount$: Observable<bigint | number> | null = null;
    public get BuildManualsCount$(): Observable<bigint | number> {
        if (this._buildManualsCount$ === null) {
            this._buildManualsCount$ = BuildManualService.Instance.GetBuildManualsRowCount({projectId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._buildManualsCount$;
    }



    public ProjectRenders$ = this._projectRendersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._projectRenders === null && this._projectRendersPromise === null) {
            this.loadProjectRenders(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _projectRendersCount$: Observable<bigint | number> | null = null;
    public get ProjectRendersCount$(): Observable<bigint | number> {
        if (this._projectRendersCount$ === null) {
            this._projectRendersCount$ = ProjectRenderService.Instance.GetProjectRendersRowCount({projectId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._projectRendersCount$;
    }



    public ProjectExports$ = this._projectExportsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._projectExports === null && this._projectExportsPromise === null) {
            this.loadProjectExports(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _projectExportsCount$: Observable<bigint | number> | null = null;
    public get ProjectExportsCount$(): Observable<bigint | number> {
        if (this._projectExportsCount$ === null) {
            this._projectExportsCount$ = ProjectExportService.Instance.GetProjectExportsRowCount({projectId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._projectExportsCount$;
    }



    public PublishedMocs$ = this._publishedMocsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._publishedMocs === null && this._publishedMocsPromise === null) {
            this.loadPublishedMocs(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _publishedMocsCount$: Observable<bigint | number> | null = null;
    public get PublishedMocsCount$(): Observable<bigint | number> {
        if (this._publishedMocsCount$ === null) {
            this._publishedMocsCount$ = PublishedMocService.Instance.GetPublishedMocsRowCount({projectId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._publishedMocsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ProjectData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.project.Reload();
  //
  //  Non Async:
  //
  //     project[0].Reload().then(x => {
  //        this.project = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ProjectService.Instance.GetProject(this.id, includeRelations)
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
     this._projectChangeHistories = null;
     this._projectChangeHistoriesPromise = null;
     this._projectChangeHistoriesSubject.next(null);
     this._projectChangeHistoriesCount$ = null;

     this._placedBricks = null;
     this._placedBricksPromise = null;
     this._placedBricksSubject.next(null);
     this._placedBricksCount$ = null;

     this._brickConnections = null;
     this._brickConnectionsPromise = null;
     this._brickConnectionsSubject.next(null);
     this._brickConnectionsCount$ = null;

     this._submodels = null;
     this._submodelsPromise = null;
     this._submodelsSubject.next(null);
     this._submodelsCount$ = null;

     this._projectTagAssignments = null;
     this._projectTagAssignmentsPromise = null;
     this._projectTagAssignmentsSubject.next(null);
     this._projectTagAssignmentsCount$ = null;

     this._projectCameraPresets = null;
     this._projectCameraPresetsPromise = null;
     this._projectCameraPresetsSubject.next(null);
     this._projectCameraPresetsCount$ = null;

     this._projectReferenceImages = null;
     this._projectReferenceImagesPromise = null;
     this._projectReferenceImagesSubject.next(null);
     this._projectReferenceImagesCount$ = null;

     this._modelDocuments = null;
     this._modelDocumentsPromise = null;
     this._modelDocumentsSubject.next(null);
     this._modelDocumentsCount$ = null;

     this._buildManuals = null;
     this._buildManualsPromise = null;
     this._buildManualsSubject.next(null);
     this._buildManualsCount$ = null;

     this._projectRenders = null;
     this._projectRendersPromise = null;
     this._projectRendersSubject.next(null);
     this._projectRendersCount$ = null;

     this._projectExports = null;
     this._projectExportsPromise = null;
     this._projectExportsSubject.next(null);
     this._projectExportsCount$ = null;

     this._publishedMocs = null;
     this._publishedMocsPromise = null;
     this._publishedMocsSubject.next(null);
     this._publishedMocsCount$ = null;

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
     * Gets the ProjectChangeHistories for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.ProjectChangeHistories.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get ProjectChangeHistories(): Promise<ProjectChangeHistoryData[]> {
        if (this._projectChangeHistories !== null) {
            return Promise.resolve(this._projectChangeHistories);
        }

        if (this._projectChangeHistoriesPromise !== null) {
            return this._projectChangeHistoriesPromise;
        }

        // Start the load
        this.loadProjectChangeHistories();

        return this._projectChangeHistoriesPromise!;
    }



    private loadProjectChangeHistories(): void {

        this._projectChangeHistoriesPromise = lastValueFrom(
            ProjectService.Instance.GetProjectChangeHistoriesForProject(this.id)
        )
        .then(ProjectChangeHistories => {
            this._projectChangeHistories = ProjectChangeHistories ?? [];
            this._projectChangeHistoriesSubject.next(this._projectChangeHistories);
            return this._projectChangeHistories;
         })
        .catch(err => {
            this._projectChangeHistories = [];
            this._projectChangeHistoriesSubject.next(this._projectChangeHistories);
            throw err;
        })
        .finally(() => {
            this._projectChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ProjectChangeHistory. Call after mutations to force refresh.
     */
    public ClearProjectChangeHistoriesCache(): void {
        this._projectChangeHistories = null;
        this._projectChangeHistoriesPromise = null;
        this._projectChangeHistoriesSubject.next(this._projectChangeHistories);      // Emit to observable
    }

    public get HasProjectChangeHistories(): Promise<boolean> {
        return this.ProjectChangeHistories.then(projectChangeHistories => projectChangeHistories.length > 0);
    }


    /**
     *
     * Gets the PlacedBricks for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.PlacedBricks.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get PlacedBricks(): Promise<PlacedBrickData[]> {
        if (this._placedBricks !== null) {
            return Promise.resolve(this._placedBricks);
        }

        if (this._placedBricksPromise !== null) {
            return this._placedBricksPromise;
        }

        // Start the load
        this.loadPlacedBricks();

        return this._placedBricksPromise!;
    }



    private loadPlacedBricks(): void {

        this._placedBricksPromise = lastValueFrom(
            ProjectService.Instance.GetPlacedBricksForProject(this.id)
        )
        .then(PlacedBricks => {
            this._placedBricks = PlacedBricks ?? [];
            this._placedBricksSubject.next(this._placedBricks);
            return this._placedBricks;
         })
        .catch(err => {
            this._placedBricks = [];
            this._placedBricksSubject.next(this._placedBricks);
            throw err;
        })
        .finally(() => {
            this._placedBricksPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PlacedBrick. Call after mutations to force refresh.
     */
    public ClearPlacedBricksCache(): void {
        this._placedBricks = null;
        this._placedBricksPromise = null;
        this._placedBricksSubject.next(this._placedBricks);      // Emit to observable
    }

    public get HasPlacedBricks(): Promise<boolean> {
        return this.PlacedBricks.then(placedBricks => placedBricks.length > 0);
    }


    /**
     *
     * Gets the BrickConnections for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.BrickConnections.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get BrickConnections(): Promise<BrickConnectionData[]> {
        if (this._brickConnections !== null) {
            return Promise.resolve(this._brickConnections);
        }

        if (this._brickConnectionsPromise !== null) {
            return this._brickConnectionsPromise;
        }

        // Start the load
        this.loadBrickConnections();

        return this._brickConnectionsPromise!;
    }



    private loadBrickConnections(): void {

        this._brickConnectionsPromise = lastValueFrom(
            ProjectService.Instance.GetBrickConnectionsForProject(this.id)
        )
        .then(BrickConnections => {
            this._brickConnections = BrickConnections ?? [];
            this._brickConnectionsSubject.next(this._brickConnections);
            return this._brickConnections;
         })
        .catch(err => {
            this._brickConnections = [];
            this._brickConnectionsSubject.next(this._brickConnections);
            throw err;
        })
        .finally(() => {
            this._brickConnectionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BrickConnection. Call after mutations to force refresh.
     */
    public ClearBrickConnectionsCache(): void {
        this._brickConnections = null;
        this._brickConnectionsPromise = null;
        this._brickConnectionsSubject.next(this._brickConnections);      // Emit to observable
    }

    public get HasBrickConnections(): Promise<boolean> {
        return this.BrickConnections.then(brickConnections => brickConnections.length > 0);
    }


    /**
     *
     * Gets the Submodels for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.Submodels.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get Submodels(): Promise<SubmodelData[]> {
        if (this._submodels !== null) {
            return Promise.resolve(this._submodels);
        }

        if (this._submodelsPromise !== null) {
            return this._submodelsPromise;
        }

        // Start the load
        this.loadSubmodels();

        return this._submodelsPromise!;
    }



    private loadSubmodels(): void {

        this._submodelsPromise = lastValueFrom(
            ProjectService.Instance.GetSubmodelsForProject(this.id)
        )
        .then(Submodels => {
            this._submodels = Submodels ?? [];
            this._submodelsSubject.next(this._submodels);
            return this._submodels;
         })
        .catch(err => {
            this._submodels = [];
            this._submodelsSubject.next(this._submodels);
            throw err;
        })
        .finally(() => {
            this._submodelsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Submodel. Call after mutations to force refresh.
     */
    public ClearSubmodelsCache(): void {
        this._submodels = null;
        this._submodelsPromise = null;
        this._submodelsSubject.next(this._submodels);      // Emit to observable
    }

    public get HasSubmodels(): Promise<boolean> {
        return this.Submodels.then(submodels => submodels.length > 0);
    }


    /**
     *
     * Gets the ProjectTagAssignments for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.ProjectTagAssignments.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get ProjectTagAssignments(): Promise<ProjectTagAssignmentData[]> {
        if (this._projectTagAssignments !== null) {
            return Promise.resolve(this._projectTagAssignments);
        }

        if (this._projectTagAssignmentsPromise !== null) {
            return this._projectTagAssignmentsPromise;
        }

        // Start the load
        this.loadProjectTagAssignments();

        return this._projectTagAssignmentsPromise!;
    }



    private loadProjectTagAssignments(): void {

        this._projectTagAssignmentsPromise = lastValueFrom(
            ProjectService.Instance.GetProjectTagAssignmentsForProject(this.id)
        )
        .then(ProjectTagAssignments => {
            this._projectTagAssignments = ProjectTagAssignments ?? [];
            this._projectTagAssignmentsSubject.next(this._projectTagAssignments);
            return this._projectTagAssignments;
         })
        .catch(err => {
            this._projectTagAssignments = [];
            this._projectTagAssignmentsSubject.next(this._projectTagAssignments);
            throw err;
        })
        .finally(() => {
            this._projectTagAssignmentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ProjectTagAssignment. Call after mutations to force refresh.
     */
    public ClearProjectTagAssignmentsCache(): void {
        this._projectTagAssignments = null;
        this._projectTagAssignmentsPromise = null;
        this._projectTagAssignmentsSubject.next(this._projectTagAssignments);      // Emit to observable
    }

    public get HasProjectTagAssignments(): Promise<boolean> {
        return this.ProjectTagAssignments.then(projectTagAssignments => projectTagAssignments.length > 0);
    }


    /**
     *
     * Gets the ProjectCameraPresets for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.ProjectCameraPresets.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get ProjectCameraPresets(): Promise<ProjectCameraPresetData[]> {
        if (this._projectCameraPresets !== null) {
            return Promise.resolve(this._projectCameraPresets);
        }

        if (this._projectCameraPresetsPromise !== null) {
            return this._projectCameraPresetsPromise;
        }

        // Start the load
        this.loadProjectCameraPresets();

        return this._projectCameraPresetsPromise!;
    }



    private loadProjectCameraPresets(): void {

        this._projectCameraPresetsPromise = lastValueFrom(
            ProjectService.Instance.GetProjectCameraPresetsForProject(this.id)
        )
        .then(ProjectCameraPresets => {
            this._projectCameraPresets = ProjectCameraPresets ?? [];
            this._projectCameraPresetsSubject.next(this._projectCameraPresets);
            return this._projectCameraPresets;
         })
        .catch(err => {
            this._projectCameraPresets = [];
            this._projectCameraPresetsSubject.next(this._projectCameraPresets);
            throw err;
        })
        .finally(() => {
            this._projectCameraPresetsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ProjectCameraPreset. Call after mutations to force refresh.
     */
    public ClearProjectCameraPresetsCache(): void {
        this._projectCameraPresets = null;
        this._projectCameraPresetsPromise = null;
        this._projectCameraPresetsSubject.next(this._projectCameraPresets);      // Emit to observable
    }

    public get HasProjectCameraPresets(): Promise<boolean> {
        return this.ProjectCameraPresets.then(projectCameraPresets => projectCameraPresets.length > 0);
    }


    /**
     *
     * Gets the ProjectReferenceImages for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.ProjectReferenceImages.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get ProjectReferenceImages(): Promise<ProjectReferenceImageData[]> {
        if (this._projectReferenceImages !== null) {
            return Promise.resolve(this._projectReferenceImages);
        }

        if (this._projectReferenceImagesPromise !== null) {
            return this._projectReferenceImagesPromise;
        }

        // Start the load
        this.loadProjectReferenceImages();

        return this._projectReferenceImagesPromise!;
    }



    private loadProjectReferenceImages(): void {

        this._projectReferenceImagesPromise = lastValueFrom(
            ProjectService.Instance.GetProjectReferenceImagesForProject(this.id)
        )
        .then(ProjectReferenceImages => {
            this._projectReferenceImages = ProjectReferenceImages ?? [];
            this._projectReferenceImagesSubject.next(this._projectReferenceImages);
            return this._projectReferenceImages;
         })
        .catch(err => {
            this._projectReferenceImages = [];
            this._projectReferenceImagesSubject.next(this._projectReferenceImages);
            throw err;
        })
        .finally(() => {
            this._projectReferenceImagesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ProjectReferenceImage. Call after mutations to force refresh.
     */
    public ClearProjectReferenceImagesCache(): void {
        this._projectReferenceImages = null;
        this._projectReferenceImagesPromise = null;
        this._projectReferenceImagesSubject.next(this._projectReferenceImages);      // Emit to observable
    }

    public get HasProjectReferenceImages(): Promise<boolean> {
        return this.ProjectReferenceImages.then(projectReferenceImages => projectReferenceImages.length > 0);
    }


    /**
     *
     * Gets the ModelDocuments for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.ModelDocuments.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get ModelDocuments(): Promise<ModelDocumentData[]> {
        if (this._modelDocuments !== null) {
            return Promise.resolve(this._modelDocuments);
        }

        if (this._modelDocumentsPromise !== null) {
            return this._modelDocumentsPromise;
        }

        // Start the load
        this.loadModelDocuments();

        return this._modelDocumentsPromise!;
    }



    private loadModelDocuments(): void {

        this._modelDocumentsPromise = lastValueFrom(
            ProjectService.Instance.GetModelDocumentsForProject(this.id)
        )
        .then(ModelDocuments => {
            this._modelDocuments = ModelDocuments ?? [];
            this._modelDocumentsSubject.next(this._modelDocuments);
            return this._modelDocuments;
         })
        .catch(err => {
            this._modelDocuments = [];
            this._modelDocumentsSubject.next(this._modelDocuments);
            throw err;
        })
        .finally(() => {
            this._modelDocumentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ModelDocument. Call after mutations to force refresh.
     */
    public ClearModelDocumentsCache(): void {
        this._modelDocuments = null;
        this._modelDocumentsPromise = null;
        this._modelDocumentsSubject.next(this._modelDocuments);      // Emit to observable
    }

    public get HasModelDocuments(): Promise<boolean> {
        return this.ModelDocuments.then(modelDocuments => modelDocuments.length > 0);
    }


    /**
     *
     * Gets the BuildManuals for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.BuildManuals.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get BuildManuals(): Promise<BuildManualData[]> {
        if (this._buildManuals !== null) {
            return Promise.resolve(this._buildManuals);
        }

        if (this._buildManualsPromise !== null) {
            return this._buildManualsPromise;
        }

        // Start the load
        this.loadBuildManuals();

        return this._buildManualsPromise!;
    }



    private loadBuildManuals(): void {

        this._buildManualsPromise = lastValueFrom(
            ProjectService.Instance.GetBuildManualsForProject(this.id)
        )
        .then(BuildManuals => {
            this._buildManuals = BuildManuals ?? [];
            this._buildManualsSubject.next(this._buildManuals);
            return this._buildManuals;
         })
        .catch(err => {
            this._buildManuals = [];
            this._buildManualsSubject.next(this._buildManuals);
            throw err;
        })
        .finally(() => {
            this._buildManualsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BuildManual. Call after mutations to force refresh.
     */
    public ClearBuildManualsCache(): void {
        this._buildManuals = null;
        this._buildManualsPromise = null;
        this._buildManualsSubject.next(this._buildManuals);      // Emit to observable
    }

    public get HasBuildManuals(): Promise<boolean> {
        return this.BuildManuals.then(buildManuals => buildManuals.length > 0);
    }


    /**
     *
     * Gets the ProjectRenders for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.ProjectRenders.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get ProjectRenders(): Promise<ProjectRenderData[]> {
        if (this._projectRenders !== null) {
            return Promise.resolve(this._projectRenders);
        }

        if (this._projectRendersPromise !== null) {
            return this._projectRendersPromise;
        }

        // Start the load
        this.loadProjectRenders();

        return this._projectRendersPromise!;
    }



    private loadProjectRenders(): void {

        this._projectRendersPromise = lastValueFrom(
            ProjectService.Instance.GetProjectRendersForProject(this.id)
        )
        .then(ProjectRenders => {
            this._projectRenders = ProjectRenders ?? [];
            this._projectRendersSubject.next(this._projectRenders);
            return this._projectRenders;
         })
        .catch(err => {
            this._projectRenders = [];
            this._projectRendersSubject.next(this._projectRenders);
            throw err;
        })
        .finally(() => {
            this._projectRendersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ProjectRender. Call after mutations to force refresh.
     */
    public ClearProjectRendersCache(): void {
        this._projectRenders = null;
        this._projectRendersPromise = null;
        this._projectRendersSubject.next(this._projectRenders);      // Emit to observable
    }

    public get HasProjectRenders(): Promise<boolean> {
        return this.ProjectRenders.then(projectRenders => projectRenders.length > 0);
    }


    /**
     *
     * Gets the ProjectExports for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.ProjectExports.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get ProjectExports(): Promise<ProjectExportData[]> {
        if (this._projectExports !== null) {
            return Promise.resolve(this._projectExports);
        }

        if (this._projectExportsPromise !== null) {
            return this._projectExportsPromise;
        }

        // Start the load
        this.loadProjectExports();

        return this._projectExportsPromise!;
    }



    private loadProjectExports(): void {

        this._projectExportsPromise = lastValueFrom(
            ProjectService.Instance.GetProjectExportsForProject(this.id)
        )
        .then(ProjectExports => {
            this._projectExports = ProjectExports ?? [];
            this._projectExportsSubject.next(this._projectExports);
            return this._projectExports;
         })
        .catch(err => {
            this._projectExports = [];
            this._projectExportsSubject.next(this._projectExports);
            throw err;
        })
        .finally(() => {
            this._projectExportsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ProjectExport. Call after mutations to force refresh.
     */
    public ClearProjectExportsCache(): void {
        this._projectExports = null;
        this._projectExportsPromise = null;
        this._projectExportsSubject.next(this._projectExports);      // Emit to observable
    }

    public get HasProjectExports(): Promise<boolean> {
        return this.ProjectExports.then(projectExports => projectExports.length > 0);
    }


    /**
     *
     * Gets the PublishedMocs for this Project.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.project.PublishedMocs.then(projects => { ... })
     *   or
     *   await this.project.projects
     *
    */
    public get PublishedMocs(): Promise<PublishedMocData[]> {
        if (this._publishedMocs !== null) {
            return Promise.resolve(this._publishedMocs);
        }

        if (this._publishedMocsPromise !== null) {
            return this._publishedMocsPromise;
        }

        // Start the load
        this.loadPublishedMocs();

        return this._publishedMocsPromise!;
    }



    private loadPublishedMocs(): void {

        this._publishedMocsPromise = lastValueFrom(
            ProjectService.Instance.GetPublishedMocsForProject(this.id)
        )
        .then(PublishedMocs => {
            this._publishedMocs = PublishedMocs ?? [];
            this._publishedMocsSubject.next(this._publishedMocs);
            return this._publishedMocs;
         })
        .catch(err => {
            this._publishedMocs = [];
            this._publishedMocsSubject.next(this._publishedMocs);
            throw err;
        })
        .finally(() => {
            this._publishedMocsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PublishedMoc. Call after mutations to force refresh.
     */
    public ClearPublishedMocsCache(): void {
        this._publishedMocs = null;
        this._publishedMocsPromise = null;
        this._publishedMocsSubject.next(this._publishedMocs);      // Emit to observable
    }

    public get HasPublishedMocs(): Promise<boolean> {
        return this.PublishedMocs.then(publishedMocs => publishedMocs.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (project.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await project.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<ProjectData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<ProjectData>> {
        const info = await lastValueFrom(
            ProjectService.Instance.GetProjectChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this ProjectData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ProjectData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ProjectSubmitData {
        return ProjectService.Instance.ConvertToProjectSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ProjectService extends SecureEndpointBase {

    private static _instance: ProjectService;
    private listCache: Map<string, Observable<Array<ProjectData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ProjectBasicListData>>>;
    private recordCache: Map<string, Observable<ProjectData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private projectChangeHistoryService: ProjectChangeHistoryService,
        private placedBrickService: PlacedBrickService,
        private brickConnectionService: BrickConnectionService,
        private submodelService: SubmodelService,
        private projectTagAssignmentService: ProjectTagAssignmentService,
        private projectCameraPresetService: ProjectCameraPresetService,
        private projectReferenceImageService: ProjectReferenceImageService,
        private modelDocumentService: ModelDocumentService,
        private buildManualService: BuildManualService,
        private projectRenderService: ProjectRenderService,
        private projectExportService: ProjectExportService,
        private publishedMocService: PublishedMocService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ProjectData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ProjectBasicListData>>>();
        this.recordCache = new Map<string, Observable<ProjectData>>();

        ProjectService._instance = this;
    }

    public static get Instance(): ProjectService {
      return ProjectService._instance;
    }


    public ClearListCaches(config: ProjectQueryParameters | null = null) {

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


    public ConvertToProjectSubmitData(data: ProjectData): ProjectSubmitData {

        let output = new ProjectSubmitData();

        output.id = data.id;
        output.userId = data.userId;
        output.name = data.name;
        output.description = data.description;
        output.notes = data.notes;
        output.thumbnailImagePath = data.thumbnailImagePath;
        output.thumbnailData = data.thumbnailData;
        output.partCount = data.partCount;
        output.lastBuildDate = data.lastBuildDate;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetProject(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const project$ = this.requestProject(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Project", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, project$);

            return project$;
        }

        return this.recordCache.get(configHash) as Observable<ProjectData>;
    }

    private requestProject(id: bigint | number, includeRelations: boolean = true) : Observable<ProjectData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ProjectData>(this.baseUrl + 'api/Project/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveProject(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestProject(id, includeRelations));
            }));
    }

    public GetProjectList(config: ProjectQueryParameters | any = null) : Observable<Array<ProjectData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const projectList$ = this.requestProjectList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Project list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, projectList$);

            return projectList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ProjectData>>;
    }


    private requestProjectList(config: ProjectQueryParameters | any) : Observable <Array<ProjectData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectData>>(this.baseUrl + 'api/Projects', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveProjectList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestProjectList(config));
            }));
    }

    public GetProjectsRowCount(config: ProjectQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const projectsRowCount$ = this.requestProjectsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Projects row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, projectsRowCount$);

            return projectsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestProjectsRowCount(config: ProjectQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Projects/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectsRowCount(config));
            }));
    }

    public GetProjectsBasicListData(config: ProjectQueryParameters | any = null) : Observable<Array<ProjectBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const projectsBasicListData$ = this.requestProjectsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Projects basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, projectsBasicListData$);

            return projectsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ProjectBasicListData>>;
    }


    private requestProjectsBasicListData(config: ProjectQueryParameters | any) : Observable<Array<ProjectBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ProjectBasicListData>>(this.baseUrl + 'api/Projects/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestProjectsBasicListData(config));
            }));

    }


    public PutProject(id: bigint | number, project: ProjectSubmitData) : Observable<ProjectData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ProjectData>(this.baseUrl + 'api/Project/' + id.toString(), project, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProject(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutProject(id, project));
            }));
    }


    public PostProject(project: ProjectSubmitData) : Observable<ProjectData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ProjectData>(this.baseUrl + 'api/Project', project, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProject(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostProject(project));
            }));
    }

  
    public DeleteProject(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Project/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteProject(id));
            }));
    }

    public RollbackProject(id: bigint | number, versionNumber: bigint | number) : Observable<ProjectData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ProjectData>(this.baseUrl + 'api/Project/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveProject(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackProject(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a Project.
     */
    public GetProjectChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<ProjectData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ProjectData>>(this.baseUrl + 'api/Project/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetProjectChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a Project.
     */
    public GetProjectAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<ProjectData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<ProjectData>[]>(this.baseUrl + 'api/Project/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetProjectAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a Project.
     */
    public GetProjectVersion(id: bigint | number, version: number): Observable<ProjectData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ProjectData>(this.baseUrl + 'api/Project/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveProject(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetProjectVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a Project at a specific point in time.
     */
    public GetProjectStateAtTime(id: bigint | number, time: string): Observable<ProjectData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ProjectData>(this.baseUrl + 'api/Project/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveProject(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetProjectStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: ProjectQueryParameters | any): string {

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

    public userIsBMCProjectReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCProjectReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.Projects
        //
        if (userIsBMCProjectReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCProjectReader = user.readPermission >= 1;
            } else {
                userIsBMCProjectReader = false;
            }
        }

        return userIsBMCProjectReader;
    }


    public userIsBMCProjectWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCProjectWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.Projects
        //
        if (userIsBMCProjectWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCProjectWriter = user.writePermission >= 1;
          } else {
            userIsBMCProjectWriter = false;
          }      
        }

        return userIsBMCProjectWriter;
    }

    public GetProjectChangeHistoriesForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ProjectChangeHistoryData[]> {
        return this.projectChangeHistoryService.GetProjectChangeHistoryList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPlacedBricksForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PlacedBrickData[]> {
        return this.placedBrickService.GetPlacedBrickList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBrickConnectionsForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickConnectionData[]> {
        return this.brickConnectionService.GetBrickConnectionList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSubmodelsForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SubmodelData[]> {
        return this.submodelService.GetSubmodelList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetProjectTagAssignmentsForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ProjectTagAssignmentData[]> {
        return this.projectTagAssignmentService.GetProjectTagAssignmentList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetProjectCameraPresetsForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ProjectCameraPresetData[]> {
        return this.projectCameraPresetService.GetProjectCameraPresetList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetProjectReferenceImagesForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ProjectReferenceImageData[]> {
        return this.projectReferenceImageService.GetProjectReferenceImageList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetModelDocumentsForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ModelDocumentData[]> {
        return this.modelDocumentService.GetModelDocumentList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBuildManualsForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BuildManualData[]> {
        return this.buildManualService.GetBuildManualList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetProjectRendersForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ProjectRenderData[]> {
        return this.projectRenderService.GetProjectRenderList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetProjectExportsForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ProjectExportData[]> {
        return this.projectExportService.GetProjectExportList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPublishedMocsForProject(projectId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PublishedMocData[]> {
        return this.publishedMocService.GetPublishedMocList({
            projectId: projectId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ProjectData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ProjectData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ProjectTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveProject(raw: any): ProjectData {
    if (!raw) return raw;

    //
    // Create a ProjectData object instance with correct prototype
    //
    const revived = Object.create(ProjectData.prototype) as ProjectData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._projectChangeHistories = null;
    (revived as any)._projectChangeHistoriesPromise = null;
    (revived as any)._projectChangeHistoriesSubject = new BehaviorSubject<ProjectChangeHistoryData[] | null>(null);

    (revived as any)._placedBricks = null;
    (revived as any)._placedBricksPromise = null;
    (revived as any)._placedBricksSubject = new BehaviorSubject<PlacedBrickData[] | null>(null);

    (revived as any)._brickConnections = null;
    (revived as any)._brickConnectionsPromise = null;
    (revived as any)._brickConnectionsSubject = new BehaviorSubject<BrickConnectionData[] | null>(null);

    (revived as any)._submodels = null;
    (revived as any)._submodelsPromise = null;
    (revived as any)._submodelsSubject = new BehaviorSubject<SubmodelData[] | null>(null);

    (revived as any)._projectTagAssignments = null;
    (revived as any)._projectTagAssignmentsPromise = null;
    (revived as any)._projectTagAssignmentsSubject = new BehaviorSubject<ProjectTagAssignmentData[] | null>(null);

    (revived as any)._projectCameraPresets = null;
    (revived as any)._projectCameraPresetsPromise = null;
    (revived as any)._projectCameraPresetsSubject = new BehaviorSubject<ProjectCameraPresetData[] | null>(null);

    (revived as any)._projectReferenceImages = null;
    (revived as any)._projectReferenceImagesPromise = null;
    (revived as any)._projectReferenceImagesSubject = new BehaviorSubject<ProjectReferenceImageData[] | null>(null);

    (revived as any)._modelDocuments = null;
    (revived as any)._modelDocumentsPromise = null;
    (revived as any)._modelDocumentsSubject = new BehaviorSubject<ModelDocumentData[] | null>(null);

    (revived as any)._buildManuals = null;
    (revived as any)._buildManualsPromise = null;
    (revived as any)._buildManualsSubject = new BehaviorSubject<BuildManualData[] | null>(null);

    (revived as any)._projectRenders = null;
    (revived as any)._projectRendersPromise = null;
    (revived as any)._projectRendersSubject = new BehaviorSubject<ProjectRenderData[] | null>(null);

    (revived as any)._projectExports = null;
    (revived as any)._projectExportsPromise = null;
    (revived as any)._projectExportsSubject = new BehaviorSubject<ProjectExportData[] | null>(null);

    (revived as any)._publishedMocs = null;
    (revived as any)._publishedMocsPromise = null;
    (revived as any)._publishedMocsSubject = new BehaviorSubject<PublishedMocData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadProjectXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ProjectChangeHistories$ = (revived as any)._projectChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._projectChangeHistories === null && (revived as any)._projectChangeHistoriesPromise === null) {
                (revived as any).loadProjectChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._projectChangeHistoriesCount$ = null;


    (revived as any).PlacedBricks$ = (revived as any)._placedBricksSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._placedBricks === null && (revived as any)._placedBricksPromise === null) {
                (revived as any).loadPlacedBricks();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._placedBricksCount$ = null;


    (revived as any).BrickConnections$ = (revived as any)._brickConnectionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickConnections === null && (revived as any)._brickConnectionsPromise === null) {
                (revived as any).loadBrickConnections();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._brickConnectionsCount$ = null;


    (revived as any).Submodels$ = (revived as any)._submodelsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._submodels === null && (revived as any)._submodelsPromise === null) {
                (revived as any).loadSubmodels();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._submodelsCount$ = null;


    (revived as any).ProjectTagAssignments$ = (revived as any)._projectTagAssignmentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._projectTagAssignments === null && (revived as any)._projectTagAssignmentsPromise === null) {
                (revived as any).loadProjectTagAssignments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._projectTagAssignmentsCount$ = null;


    (revived as any).ProjectCameraPresets$ = (revived as any)._projectCameraPresetsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._projectCameraPresets === null && (revived as any)._projectCameraPresetsPromise === null) {
                (revived as any).loadProjectCameraPresets();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._projectCameraPresetsCount$ = null;


    (revived as any).ProjectReferenceImages$ = (revived as any)._projectReferenceImagesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._projectReferenceImages === null && (revived as any)._projectReferenceImagesPromise === null) {
                (revived as any).loadProjectReferenceImages();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._projectReferenceImagesCount$ = null;


    (revived as any).ModelDocuments$ = (revived as any)._modelDocumentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._modelDocuments === null && (revived as any)._modelDocumentsPromise === null) {
                (revived as any).loadModelDocuments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._modelDocumentsCount$ = null;


    (revived as any).BuildManuals$ = (revived as any)._buildManualsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._buildManuals === null && (revived as any)._buildManualsPromise === null) {
                (revived as any).loadBuildManuals();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._buildManualsCount$ = null;


    (revived as any).ProjectRenders$ = (revived as any)._projectRendersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._projectRenders === null && (revived as any)._projectRendersPromise === null) {
                (revived as any).loadProjectRenders();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._projectRendersCount$ = null;


    (revived as any).ProjectExports$ = (revived as any)._projectExportsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._projectExports === null && (revived as any)._projectExportsPromise === null) {
                (revived as any).loadProjectExports();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._projectExportsCount$ = null;


    (revived as any).PublishedMocs$ = (revived as any)._publishedMocsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._publishedMocs === null && (revived as any)._publishedMocsPromise === null) {
                (revived as any).loadPublishedMocs();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._publishedMocsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<ProjectData> | null>(null);

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

  private ReviveProjectList(rawList: any[]): ProjectData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveProject(raw));
  }

}
