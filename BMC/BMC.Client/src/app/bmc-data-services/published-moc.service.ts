/*

   GENERATED SERVICE FOR THE PUBLISHEDMOC TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the PublishedMoc table.

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
import { PublishedMocChangeHistoryService, PublishedMocChangeHistoryData } from './published-moc-change-history.service';
import { MocVersionService, MocVersionData } from './moc-version.service';
import { PublishedMocImageService, PublishedMocImageData } from './published-moc-image.service';
import { MocForkService, MocForkData } from './moc-fork.service';
import { MocCollaboratorService, MocCollaboratorData } from './moc-collaborator.service';
import { MocLikeService, MocLikeData } from './moc-like.service';
import { MocCommentService, MocCommentData } from './moc-comment.service';
import { MocFavouriteService, MocFavouriteData } from './moc-favourite.service';
import { SharedInstructionService, SharedInstructionData } from './shared-instruction.service';
import { BuildChallengeEntryService, BuildChallengeEntryData } from './build-challenge-entry.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PublishedMocQueryParameters {
    projectId: bigint | number | null | undefined = null;
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    thumbnailImagePath: string | null | undefined = null;
    tags: string | null | undefined = null;
    isPublished: boolean | null | undefined = null;
    isFeatured: boolean | null | undefined = null;
    publishedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
    viewCount: bigint | number | null | undefined = null;
    likeCount: bigint | number | null | undefined = null;
    commentCount: bigint | number | null | undefined = null;
    favouriteCount: bigint | number | null | undefined = null;
    partCount: bigint | number | null | undefined = null;
    allowForking: boolean | null | undefined = null;
    visibility: string | null | undefined = null;
    forkCount: bigint | number | null | undefined = null;
    forkedFromMocId: bigint | number | null | undefined = null;
    licenseName: string | null | undefined = null;
    readmeMarkdown: string | null | undefined = null;
    slug: string | null | undefined = null;
    defaultBranchName: string | null | undefined = null;
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
export class PublishedMocSubmitData {
    id!: bigint | number;
    projectId!: bigint | number;
    name!: string;
    description: string | null = null;
    thumbnailImagePath: string | null = null;
    tags: string | null = null;
    isPublished!: boolean;
    isFeatured!: boolean;
    publishedDate: string | null = null;     // ISO 8601 (full datetime)
    viewCount!: bigint | number;
    likeCount!: bigint | number;
    commentCount!: bigint | number;
    favouriteCount!: bigint | number;
    partCount: bigint | number | null = null;
    allowForking!: boolean;
    visibility!: string;
    forkCount!: bigint | number;
    forkedFromMocId: bigint | number | null = null;
    licenseName: string | null = null;
    readmeMarkdown: string | null = null;
    slug: string | null = null;
    defaultBranchName: string | null = null;
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

export class PublishedMocBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PublishedMocChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `publishedMoc.PublishedMocChildren$` — use with `| async` in templates
//        • Promise:    `publishedMoc.PublishedMocChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="publishedMoc.PublishedMocChildren$ | async"`), or
//        • Access the promise getter (`publishedMoc.PublishedMocChildren` or `await publishedMoc.PublishedMocChildren`)
//    - Simply reading `publishedMoc.PublishedMocChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await publishedMoc.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PublishedMocData {
    id!: bigint | number;
    projectId!: bigint | number;
    name!: string;
    description!: string | null;
    thumbnailImagePath!: string | null;
    tags!: string | null;
    isPublished!: boolean;
    isFeatured!: boolean;
    publishedDate!: string | null;   // ISO 8601 (full datetime)
    viewCount!: bigint | number;
    likeCount!: bigint | number;
    commentCount!: bigint | number;
    favouriteCount!: bigint | number;
    partCount!: bigint | number;
    allowForking!: boolean;
    visibility!: string;
    forkCount!: bigint | number;
    forkedFromMocId!: bigint | number;
    licenseName!: string | null;
    readmeMarkdown!: string | null;
    slug!: string | null;
    defaultBranchName!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    project: ProjectData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    forkedFromMoc: PublishedMocData | null | undefined = null;            // Self referencing navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _publishedMocChangeHistories: PublishedMocChangeHistoryData[] | null = null;
    private _publishedMocChangeHistoriesPromise: Promise<PublishedMocChangeHistoryData[]> | null  = null;
    private _publishedMocChangeHistoriesSubject = new BehaviorSubject<PublishedMocChangeHistoryData[] | null>(null);

                
    private _mocVersions: MocVersionData[] | null = null;
    private _mocVersionsPromise: Promise<MocVersionData[]> | null  = null;
    private _mocVersionsSubject = new BehaviorSubject<MocVersionData[] | null>(null);

                
    private _publishedMocImages: PublishedMocImageData[] | null = null;
    private _publishedMocImagesPromise: Promise<PublishedMocImageData[]> | null  = null;
    private _publishedMocImagesSubject = new BehaviorSubject<PublishedMocImageData[] | null>(null);

                
    private _mocForkForkedMocs: MocForkData[] | null = null;
    private _mocForkForkedMocsPromise: Promise<MocForkData[]> | null  = null;
    private _mocForkForkedMocsSubject = new BehaviorSubject<MocForkData[] | null>(null);
                    
    private _mocForkSourceMocs: MocForkData[] | null = null;
    private _mocForkSourceMocsPromise: Promise<MocForkData[]> | null  = null;
    private _mocForkSourceMocsSubject = new BehaviorSubject<MocForkData[] | null>(null);
                    
    private _mocCollaborators: MocCollaboratorData[] | null = null;
    private _mocCollaboratorsPromise: Promise<MocCollaboratorData[]> | null  = null;
    private _mocCollaboratorsSubject = new BehaviorSubject<MocCollaboratorData[] | null>(null);

                
    private _mocLikes: MocLikeData[] | null = null;
    private _mocLikesPromise: Promise<MocLikeData[]> | null  = null;
    private _mocLikesSubject = new BehaviorSubject<MocLikeData[] | null>(null);

                
    private _mocComments: MocCommentData[] | null = null;
    private _mocCommentsPromise: Promise<MocCommentData[]> | null  = null;
    private _mocCommentsSubject = new BehaviorSubject<MocCommentData[] | null>(null);

                
    private _mocFavourites: MocFavouriteData[] | null = null;
    private _mocFavouritesPromise: Promise<MocFavouriteData[]> | null  = null;
    private _mocFavouritesSubject = new BehaviorSubject<MocFavouriteData[] | null>(null);

                
    private _sharedInstructions: SharedInstructionData[] | null = null;
    private _sharedInstructionsPromise: Promise<SharedInstructionData[]> | null  = null;
    private _sharedInstructionsSubject = new BehaviorSubject<SharedInstructionData[] | null>(null);

                
    private _buildChallengeEntries: BuildChallengeEntryData[] | null = null;
    private _buildChallengeEntriesPromise: Promise<BuildChallengeEntryData[]> | null  = null;
    private _buildChallengeEntriesSubject = new BehaviorSubject<BuildChallengeEntryData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<PublishedMocData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<PublishedMocData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PublishedMocData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public PublishedMocChangeHistories$ = this._publishedMocChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._publishedMocChangeHistories === null && this._publishedMocChangeHistoriesPromise === null) {
            this.loadPublishedMocChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _publishedMocChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get PublishedMocChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._publishedMocChangeHistoriesCount$ === null) {
            this._publishedMocChangeHistoriesCount$ = PublishedMocChangeHistoryService.Instance.GetPublishedMocChangeHistoriesRowCount({publishedMocId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._publishedMocChangeHistoriesCount$;
    }



    public MocVersions$ = this._mocVersionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._mocVersions === null && this._mocVersionsPromise === null) {
            this.loadMocVersions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _mocVersionsCount$: Observable<bigint | number> | null = null;
    public get MocVersionsCount$(): Observable<bigint | number> {
        if (this._mocVersionsCount$ === null) {
            this._mocVersionsCount$ = MocVersionService.Instance.GetMocVersionsRowCount({publishedMocId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._mocVersionsCount$;
    }



    public PublishedMocImages$ = this._publishedMocImagesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._publishedMocImages === null && this._publishedMocImagesPromise === null) {
            this.loadPublishedMocImages(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _publishedMocImagesCount$: Observable<bigint | number> | null = null;
    public get PublishedMocImagesCount$(): Observable<bigint | number> {
        if (this._publishedMocImagesCount$ === null) {
            this._publishedMocImagesCount$ = PublishedMocImageService.Instance.GetPublishedMocImagesRowCount({publishedMocId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._publishedMocImagesCount$;
    }



    public MocForkForkedMocs$ = this._mocForkForkedMocsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._mocForkForkedMocs === null && this._mocForkForkedMocsPromise === null) {
            this.loadMocForkForkedMocs(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _mocForkForkedMocsCount$: Observable<bigint | number> | null = null;
    public get MocForkForkedMocsCount$(): Observable<bigint | number> {
        if (this._mocForkForkedMocsCount$ === null) {
            this._mocForkForkedMocsCount$ = MocForkService.Instance.GetMocForksRowCount({forkedMocId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._mocForkForkedMocsCount$;
    }


    public MocForkSourceMocs$ = this._mocForkSourceMocsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._mocForkSourceMocs === null && this._mocForkSourceMocsPromise === null) {
            this.loadMocForkSourceMocs(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _mocForkSourceMocsCount$: Observable<bigint | number> | null = null;
    public get MocForkSourceMocsCount$(): Observable<bigint | number> {
        if (this._mocForkSourceMocsCount$ === null) {
            this._mocForkSourceMocsCount$ = MocForkService.Instance.GetMocForksRowCount({sourceMocId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._mocForkSourceMocsCount$;
    }


    public MocCollaborators$ = this._mocCollaboratorsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._mocCollaborators === null && this._mocCollaboratorsPromise === null) {
            this.loadMocCollaborators(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _mocCollaboratorsCount$: Observable<bigint | number> | null = null;
    public get MocCollaboratorsCount$(): Observable<bigint | number> {
        if (this._mocCollaboratorsCount$ === null) {
            this._mocCollaboratorsCount$ = MocCollaboratorService.Instance.GetMocCollaboratorsRowCount({publishedMocId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._mocCollaboratorsCount$;
    }



    public MocLikes$ = this._mocLikesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._mocLikes === null && this._mocLikesPromise === null) {
            this.loadMocLikes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _mocLikesCount$: Observable<bigint | number> | null = null;
    public get MocLikesCount$(): Observable<bigint | number> {
        if (this._mocLikesCount$ === null) {
            this._mocLikesCount$ = MocLikeService.Instance.GetMocLikesRowCount({publishedMocId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._mocLikesCount$;
    }



    public MocComments$ = this._mocCommentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._mocComments === null && this._mocCommentsPromise === null) {
            this.loadMocComments(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _mocCommentsCount$: Observable<bigint | number> | null = null;
    public get MocCommentsCount$(): Observable<bigint | number> {
        if (this._mocCommentsCount$ === null) {
            this._mocCommentsCount$ = MocCommentService.Instance.GetMocCommentsRowCount({publishedMocId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._mocCommentsCount$;
    }



    public MocFavourites$ = this._mocFavouritesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._mocFavourites === null && this._mocFavouritesPromise === null) {
            this.loadMocFavourites(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _mocFavouritesCount$: Observable<bigint | number> | null = null;
    public get MocFavouritesCount$(): Observable<bigint | number> {
        if (this._mocFavouritesCount$ === null) {
            this._mocFavouritesCount$ = MocFavouriteService.Instance.GetMocFavouritesRowCount({publishedMocId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._mocFavouritesCount$;
    }



    public SharedInstructions$ = this._sharedInstructionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._sharedInstructions === null && this._sharedInstructionsPromise === null) {
            this.loadSharedInstructions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _sharedInstructionsCount$: Observable<bigint | number> | null = null;
    public get SharedInstructionsCount$(): Observable<bigint | number> {
        if (this._sharedInstructionsCount$ === null) {
            this._sharedInstructionsCount$ = SharedInstructionService.Instance.GetSharedInstructionsRowCount({publishedMocId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._sharedInstructionsCount$;
    }



    public BuildChallengeEntries$ = this._buildChallengeEntriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._buildChallengeEntries === null && this._buildChallengeEntriesPromise === null) {
            this.loadBuildChallengeEntries(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _buildChallengeEntriesCount$: Observable<bigint | number> | null = null;
    public get BuildChallengeEntriesCount$(): Observable<bigint | number> {
        if (this._buildChallengeEntriesCount$ === null) {
            this._buildChallengeEntriesCount$ = BuildChallengeEntryService.Instance.GetBuildChallengeEntriesRowCount({publishedMocId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._buildChallengeEntriesCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PublishedMocData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.publishedMoc.Reload();
  //
  //  Non Async:
  //
  //     publishedMoc[0].Reload().then(x => {
  //        this.publishedMoc = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PublishedMocService.Instance.GetPublishedMoc(this.id, includeRelations)
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
     this._publishedMocChangeHistories = null;
     this._publishedMocChangeHistoriesPromise = null;
     this._publishedMocChangeHistoriesSubject.next(null);
     this._publishedMocChangeHistoriesCount$ = null;

     this._mocVersions = null;
     this._mocVersionsPromise = null;
     this._mocVersionsSubject.next(null);
     this._mocVersionsCount$ = null;

     this._publishedMocImages = null;
     this._publishedMocImagesPromise = null;
     this._publishedMocImagesSubject.next(null);
     this._publishedMocImagesCount$ = null;

     this._mocForkForkedMocs = null;
     this._mocForkForkedMocsPromise = null;
     this._mocForkForkedMocsSubject.next(null);
     this._mocForkForkedMocsCount$ = null;

     this._mocForkSourceMocs = null;
     this._mocForkSourceMocsPromise = null;
     this._mocForkSourceMocsSubject.next(null);
     this._mocForkSourceMocsCount$ = null;

     this._mocCollaborators = null;
     this._mocCollaboratorsPromise = null;
     this._mocCollaboratorsSubject.next(null);
     this._mocCollaboratorsCount$ = null;

     this._mocLikes = null;
     this._mocLikesPromise = null;
     this._mocLikesSubject.next(null);
     this._mocLikesCount$ = null;

     this._mocComments = null;
     this._mocCommentsPromise = null;
     this._mocCommentsSubject.next(null);
     this._mocCommentsCount$ = null;

     this._mocFavourites = null;
     this._mocFavouritesPromise = null;
     this._mocFavouritesSubject.next(null);
     this._mocFavouritesCount$ = null;

     this._sharedInstructions = null;
     this._sharedInstructionsPromise = null;
     this._sharedInstructionsSubject.next(null);
     this._sharedInstructionsCount$ = null;

     this._buildChallengeEntries = null;
     this._buildChallengeEntriesPromise = null;
     this._buildChallengeEntriesSubject.next(null);
     this._buildChallengeEntriesCount$ = null;

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
     * Gets the PublishedMocChangeHistories for this PublishedMoc.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.publishedMoc.PublishedMocChangeHistories.then(publishedMocs => { ... })
     *   or
     *   await this.publishedMoc.publishedMocs
     *
    */
    public get PublishedMocChangeHistories(): Promise<PublishedMocChangeHistoryData[]> {
        if (this._publishedMocChangeHistories !== null) {
            return Promise.resolve(this._publishedMocChangeHistories);
        }

        if (this._publishedMocChangeHistoriesPromise !== null) {
            return this._publishedMocChangeHistoriesPromise;
        }

        // Start the load
        this.loadPublishedMocChangeHistories();

        return this._publishedMocChangeHistoriesPromise!;
    }



    private loadPublishedMocChangeHistories(): void {

        this._publishedMocChangeHistoriesPromise = lastValueFrom(
            PublishedMocService.Instance.GetPublishedMocChangeHistoriesForPublishedMoc(this.id)
        )
        .then(PublishedMocChangeHistories => {
            this._publishedMocChangeHistories = PublishedMocChangeHistories ?? [];
            this._publishedMocChangeHistoriesSubject.next(this._publishedMocChangeHistories);
            return this._publishedMocChangeHistories;
         })
        .catch(err => {
            this._publishedMocChangeHistories = [];
            this._publishedMocChangeHistoriesSubject.next(this._publishedMocChangeHistories);
            throw err;
        })
        .finally(() => {
            this._publishedMocChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PublishedMocChangeHistory. Call after mutations to force refresh.
     */
    public ClearPublishedMocChangeHistoriesCache(): void {
        this._publishedMocChangeHistories = null;
        this._publishedMocChangeHistoriesPromise = null;
        this._publishedMocChangeHistoriesSubject.next(this._publishedMocChangeHistories);      // Emit to observable
    }

    public get HasPublishedMocChangeHistories(): Promise<boolean> {
        return this.PublishedMocChangeHistories.then(publishedMocChangeHistories => publishedMocChangeHistories.length > 0);
    }


    /**
     *
     * Gets the MocVersions for this PublishedMoc.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.publishedMoc.MocVersions.then(publishedMocs => { ... })
     *   or
     *   await this.publishedMoc.publishedMocs
     *
    */
    public get MocVersions(): Promise<MocVersionData[]> {
        if (this._mocVersions !== null) {
            return Promise.resolve(this._mocVersions);
        }

        if (this._mocVersionsPromise !== null) {
            return this._mocVersionsPromise;
        }

        // Start the load
        this.loadMocVersions();

        return this._mocVersionsPromise!;
    }



    private loadMocVersions(): void {

        this._mocVersionsPromise = lastValueFrom(
            PublishedMocService.Instance.GetMocVersionsForPublishedMoc(this.id)
        )
        .then(MocVersions => {
            this._mocVersions = MocVersions ?? [];
            this._mocVersionsSubject.next(this._mocVersions);
            return this._mocVersions;
         })
        .catch(err => {
            this._mocVersions = [];
            this._mocVersionsSubject.next(this._mocVersions);
            throw err;
        })
        .finally(() => {
            this._mocVersionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached MocVersion. Call after mutations to force refresh.
     */
    public ClearMocVersionsCache(): void {
        this._mocVersions = null;
        this._mocVersionsPromise = null;
        this._mocVersionsSubject.next(this._mocVersions);      // Emit to observable
    }

    public get HasMocVersions(): Promise<boolean> {
        return this.MocVersions.then(mocVersions => mocVersions.length > 0);
    }


    /**
     *
     * Gets the PublishedMocImages for this PublishedMoc.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.publishedMoc.PublishedMocImages.then(publishedMocs => { ... })
     *   or
     *   await this.publishedMoc.publishedMocs
     *
    */
    public get PublishedMocImages(): Promise<PublishedMocImageData[]> {
        if (this._publishedMocImages !== null) {
            return Promise.resolve(this._publishedMocImages);
        }

        if (this._publishedMocImagesPromise !== null) {
            return this._publishedMocImagesPromise;
        }

        // Start the load
        this.loadPublishedMocImages();

        return this._publishedMocImagesPromise!;
    }



    private loadPublishedMocImages(): void {

        this._publishedMocImagesPromise = lastValueFrom(
            PublishedMocService.Instance.GetPublishedMocImagesForPublishedMoc(this.id)
        )
        .then(PublishedMocImages => {
            this._publishedMocImages = PublishedMocImages ?? [];
            this._publishedMocImagesSubject.next(this._publishedMocImages);
            return this._publishedMocImages;
         })
        .catch(err => {
            this._publishedMocImages = [];
            this._publishedMocImagesSubject.next(this._publishedMocImages);
            throw err;
        })
        .finally(() => {
            this._publishedMocImagesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PublishedMocImage. Call after mutations to force refresh.
     */
    public ClearPublishedMocImagesCache(): void {
        this._publishedMocImages = null;
        this._publishedMocImagesPromise = null;
        this._publishedMocImagesSubject.next(this._publishedMocImages);      // Emit to observable
    }

    public get HasPublishedMocImages(): Promise<boolean> {
        return this.PublishedMocImages.then(publishedMocImages => publishedMocImages.length > 0);
    }


    /**
     *
     * Gets the MocForkForkedMocs for this PublishedMoc.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.publishedMoc.MocForkForkedMocs.then(forkedMocs => { ... })
     *   or
     *   await this.publishedMoc.forkedMocs
     *
    */
    public get MocForkForkedMocs(): Promise<MocForkData[]> {
        if (this._mocForkForkedMocs !== null) {
            return Promise.resolve(this._mocForkForkedMocs);
        }

        if (this._mocForkForkedMocsPromise !== null) {
            return this._mocForkForkedMocsPromise;
        }

        // Start the load
        this.loadMocForkForkedMocs();

        return this._mocForkForkedMocsPromise!;
    }



    private loadMocForkForkedMocs(): void {

        this._mocForkForkedMocsPromise = lastValueFrom(
            PublishedMocService.Instance.GetMocForkForkedMocsForPublishedMoc(this.id)
        )
        .then(MocForkForkedMocs => {
            this._mocForkForkedMocs = MocForkForkedMocs ?? [];
            this._mocForkForkedMocsSubject.next(this._mocForkForkedMocs);
            return this._mocForkForkedMocs;
         })
        .catch(err => {
            this._mocForkForkedMocs = [];
            this._mocForkForkedMocsSubject.next(this._mocForkForkedMocs);
            throw err;
        })
        .finally(() => {
            this._mocForkForkedMocsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached MocForkForkedMoc. Call after mutations to force refresh.
     */
    public ClearMocForkForkedMocsCache(): void {
        this._mocForkForkedMocs = null;
        this._mocForkForkedMocsPromise = null;
        this._mocForkForkedMocsSubject.next(this._mocForkForkedMocs);      // Emit to observable
    }

    public get HasMocForkForkedMocs(): Promise<boolean> {
        return this.MocForkForkedMocs.then(mocForkForkedMocs => mocForkForkedMocs.length > 0);
    }


    /**
     *
     * Gets the MocForkSourceMocs for this PublishedMoc.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.publishedMoc.MocForkSourceMocs.then(sourceMocs => { ... })
     *   or
     *   await this.publishedMoc.sourceMocs
     *
    */
    public get MocForkSourceMocs(): Promise<MocForkData[]> {
        if (this._mocForkSourceMocs !== null) {
            return Promise.resolve(this._mocForkSourceMocs);
        }

        if (this._mocForkSourceMocsPromise !== null) {
            return this._mocForkSourceMocsPromise;
        }

        // Start the load
        this.loadMocForkSourceMocs();

        return this._mocForkSourceMocsPromise!;
    }



    private loadMocForkSourceMocs(): void {

        this._mocForkSourceMocsPromise = lastValueFrom(
            PublishedMocService.Instance.GetMocForkSourceMocsForPublishedMoc(this.id)
        )
        .then(MocForkSourceMocs => {
            this._mocForkSourceMocs = MocForkSourceMocs ?? [];
            this._mocForkSourceMocsSubject.next(this._mocForkSourceMocs);
            return this._mocForkSourceMocs;
         })
        .catch(err => {
            this._mocForkSourceMocs = [];
            this._mocForkSourceMocsSubject.next(this._mocForkSourceMocs);
            throw err;
        })
        .finally(() => {
            this._mocForkSourceMocsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached MocForkSourceMoc. Call after mutations to force refresh.
     */
    public ClearMocForkSourceMocsCache(): void {
        this._mocForkSourceMocs = null;
        this._mocForkSourceMocsPromise = null;
        this._mocForkSourceMocsSubject.next(this._mocForkSourceMocs);      // Emit to observable
    }

    public get HasMocForkSourceMocs(): Promise<boolean> {
        return this.MocForkSourceMocs.then(mocForkSourceMocs => mocForkSourceMocs.length > 0);
    }


    /**
     *
     * Gets the MocCollaborators for this PublishedMoc.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.publishedMoc.MocCollaborators.then(publishedMocs => { ... })
     *   or
     *   await this.publishedMoc.publishedMocs
     *
    */
    public get MocCollaborators(): Promise<MocCollaboratorData[]> {
        if (this._mocCollaborators !== null) {
            return Promise.resolve(this._mocCollaborators);
        }

        if (this._mocCollaboratorsPromise !== null) {
            return this._mocCollaboratorsPromise;
        }

        // Start the load
        this.loadMocCollaborators();

        return this._mocCollaboratorsPromise!;
    }



    private loadMocCollaborators(): void {

        this._mocCollaboratorsPromise = lastValueFrom(
            PublishedMocService.Instance.GetMocCollaboratorsForPublishedMoc(this.id)
        )
        .then(MocCollaborators => {
            this._mocCollaborators = MocCollaborators ?? [];
            this._mocCollaboratorsSubject.next(this._mocCollaborators);
            return this._mocCollaborators;
         })
        .catch(err => {
            this._mocCollaborators = [];
            this._mocCollaboratorsSubject.next(this._mocCollaborators);
            throw err;
        })
        .finally(() => {
            this._mocCollaboratorsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached MocCollaborator. Call after mutations to force refresh.
     */
    public ClearMocCollaboratorsCache(): void {
        this._mocCollaborators = null;
        this._mocCollaboratorsPromise = null;
        this._mocCollaboratorsSubject.next(this._mocCollaborators);      // Emit to observable
    }

    public get HasMocCollaborators(): Promise<boolean> {
        return this.MocCollaborators.then(mocCollaborators => mocCollaborators.length > 0);
    }


    /**
     *
     * Gets the MocLikes for this PublishedMoc.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.publishedMoc.MocLikes.then(publishedMocs => { ... })
     *   or
     *   await this.publishedMoc.publishedMocs
     *
    */
    public get MocLikes(): Promise<MocLikeData[]> {
        if (this._mocLikes !== null) {
            return Promise.resolve(this._mocLikes);
        }

        if (this._mocLikesPromise !== null) {
            return this._mocLikesPromise;
        }

        // Start the load
        this.loadMocLikes();

        return this._mocLikesPromise!;
    }



    private loadMocLikes(): void {

        this._mocLikesPromise = lastValueFrom(
            PublishedMocService.Instance.GetMocLikesForPublishedMoc(this.id)
        )
        .then(MocLikes => {
            this._mocLikes = MocLikes ?? [];
            this._mocLikesSubject.next(this._mocLikes);
            return this._mocLikes;
         })
        .catch(err => {
            this._mocLikes = [];
            this._mocLikesSubject.next(this._mocLikes);
            throw err;
        })
        .finally(() => {
            this._mocLikesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached MocLike. Call after mutations to force refresh.
     */
    public ClearMocLikesCache(): void {
        this._mocLikes = null;
        this._mocLikesPromise = null;
        this._mocLikesSubject.next(this._mocLikes);      // Emit to observable
    }

    public get HasMocLikes(): Promise<boolean> {
        return this.MocLikes.then(mocLikes => mocLikes.length > 0);
    }


    /**
     *
     * Gets the MocComments for this PublishedMoc.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.publishedMoc.MocComments.then(publishedMocs => { ... })
     *   or
     *   await this.publishedMoc.publishedMocs
     *
    */
    public get MocComments(): Promise<MocCommentData[]> {
        if (this._mocComments !== null) {
            return Promise.resolve(this._mocComments);
        }

        if (this._mocCommentsPromise !== null) {
            return this._mocCommentsPromise;
        }

        // Start the load
        this.loadMocComments();

        return this._mocCommentsPromise!;
    }



    private loadMocComments(): void {

        this._mocCommentsPromise = lastValueFrom(
            PublishedMocService.Instance.GetMocCommentsForPublishedMoc(this.id)
        )
        .then(MocComments => {
            this._mocComments = MocComments ?? [];
            this._mocCommentsSubject.next(this._mocComments);
            return this._mocComments;
         })
        .catch(err => {
            this._mocComments = [];
            this._mocCommentsSubject.next(this._mocComments);
            throw err;
        })
        .finally(() => {
            this._mocCommentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached MocComment. Call after mutations to force refresh.
     */
    public ClearMocCommentsCache(): void {
        this._mocComments = null;
        this._mocCommentsPromise = null;
        this._mocCommentsSubject.next(this._mocComments);      // Emit to observable
    }

    public get HasMocComments(): Promise<boolean> {
        return this.MocComments.then(mocComments => mocComments.length > 0);
    }


    /**
     *
     * Gets the MocFavourites for this PublishedMoc.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.publishedMoc.MocFavourites.then(publishedMocs => { ... })
     *   or
     *   await this.publishedMoc.publishedMocs
     *
    */
    public get MocFavourites(): Promise<MocFavouriteData[]> {
        if (this._mocFavourites !== null) {
            return Promise.resolve(this._mocFavourites);
        }

        if (this._mocFavouritesPromise !== null) {
            return this._mocFavouritesPromise;
        }

        // Start the load
        this.loadMocFavourites();

        return this._mocFavouritesPromise!;
    }



    private loadMocFavourites(): void {

        this._mocFavouritesPromise = lastValueFrom(
            PublishedMocService.Instance.GetMocFavouritesForPublishedMoc(this.id)
        )
        .then(MocFavourites => {
            this._mocFavourites = MocFavourites ?? [];
            this._mocFavouritesSubject.next(this._mocFavourites);
            return this._mocFavourites;
         })
        .catch(err => {
            this._mocFavourites = [];
            this._mocFavouritesSubject.next(this._mocFavourites);
            throw err;
        })
        .finally(() => {
            this._mocFavouritesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached MocFavourite. Call after mutations to force refresh.
     */
    public ClearMocFavouritesCache(): void {
        this._mocFavourites = null;
        this._mocFavouritesPromise = null;
        this._mocFavouritesSubject.next(this._mocFavourites);      // Emit to observable
    }

    public get HasMocFavourites(): Promise<boolean> {
        return this.MocFavourites.then(mocFavourites => mocFavourites.length > 0);
    }


    /**
     *
     * Gets the SharedInstructions for this PublishedMoc.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.publishedMoc.SharedInstructions.then(publishedMocs => { ... })
     *   or
     *   await this.publishedMoc.publishedMocs
     *
    */
    public get SharedInstructions(): Promise<SharedInstructionData[]> {
        if (this._sharedInstructions !== null) {
            return Promise.resolve(this._sharedInstructions);
        }

        if (this._sharedInstructionsPromise !== null) {
            return this._sharedInstructionsPromise;
        }

        // Start the load
        this.loadSharedInstructions();

        return this._sharedInstructionsPromise!;
    }



    private loadSharedInstructions(): void {

        this._sharedInstructionsPromise = lastValueFrom(
            PublishedMocService.Instance.GetSharedInstructionsForPublishedMoc(this.id)
        )
        .then(SharedInstructions => {
            this._sharedInstructions = SharedInstructions ?? [];
            this._sharedInstructionsSubject.next(this._sharedInstructions);
            return this._sharedInstructions;
         })
        .catch(err => {
            this._sharedInstructions = [];
            this._sharedInstructionsSubject.next(this._sharedInstructions);
            throw err;
        })
        .finally(() => {
            this._sharedInstructionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SharedInstruction. Call after mutations to force refresh.
     */
    public ClearSharedInstructionsCache(): void {
        this._sharedInstructions = null;
        this._sharedInstructionsPromise = null;
        this._sharedInstructionsSubject.next(this._sharedInstructions);      // Emit to observable
    }

    public get HasSharedInstructions(): Promise<boolean> {
        return this.SharedInstructions.then(sharedInstructions => sharedInstructions.length > 0);
    }


    /**
     *
     * Gets the BuildChallengeEntries for this PublishedMoc.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.publishedMoc.BuildChallengeEntries.then(publishedMocs => { ... })
     *   or
     *   await this.publishedMoc.publishedMocs
     *
    */
    public get BuildChallengeEntries(): Promise<BuildChallengeEntryData[]> {
        if (this._buildChallengeEntries !== null) {
            return Promise.resolve(this._buildChallengeEntries);
        }

        if (this._buildChallengeEntriesPromise !== null) {
            return this._buildChallengeEntriesPromise;
        }

        // Start the load
        this.loadBuildChallengeEntries();

        return this._buildChallengeEntriesPromise!;
    }



    private loadBuildChallengeEntries(): void {

        this._buildChallengeEntriesPromise = lastValueFrom(
            PublishedMocService.Instance.GetBuildChallengeEntriesForPublishedMoc(this.id)
        )
        .then(BuildChallengeEntries => {
            this._buildChallengeEntries = BuildChallengeEntries ?? [];
            this._buildChallengeEntriesSubject.next(this._buildChallengeEntries);
            return this._buildChallengeEntries;
         })
        .catch(err => {
            this._buildChallengeEntries = [];
            this._buildChallengeEntriesSubject.next(this._buildChallengeEntries);
            throw err;
        })
        .finally(() => {
            this._buildChallengeEntriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BuildChallengeEntry. Call after mutations to force refresh.
     */
    public ClearBuildChallengeEntriesCache(): void {
        this._buildChallengeEntries = null;
        this._buildChallengeEntriesPromise = null;
        this._buildChallengeEntriesSubject.next(this._buildChallengeEntries);      // Emit to observable
    }

    public get HasBuildChallengeEntries(): Promise<boolean> {
        return this.BuildChallengeEntries.then(buildChallengeEntries => buildChallengeEntries.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (publishedMoc.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await publishedMoc.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<PublishedMocData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<PublishedMocData>> {
        const info = await lastValueFrom(
            PublishedMocService.Instance.GetPublishedMocChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this PublishedMocData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PublishedMocData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PublishedMocSubmitData {
        return PublishedMocService.Instance.ConvertToPublishedMocSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PublishedMocService extends SecureEndpointBase {

    private static _instance: PublishedMocService;
    private listCache: Map<string, Observable<Array<PublishedMocData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PublishedMocBasicListData>>>;
    private recordCache: Map<string, Observable<PublishedMocData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private publishedMocChangeHistoryService: PublishedMocChangeHistoryService,
        private mocVersionService: MocVersionService,
        private publishedMocImageService: PublishedMocImageService,
        private mocForkService: MocForkService,
        private mocCollaboratorService: MocCollaboratorService,
        private mocLikeService: MocLikeService,
        private mocCommentService: MocCommentService,
        private mocFavouriteService: MocFavouriteService,
        private sharedInstructionService: SharedInstructionService,
        private buildChallengeEntryService: BuildChallengeEntryService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PublishedMocData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PublishedMocBasicListData>>>();
        this.recordCache = new Map<string, Observable<PublishedMocData>>();

        PublishedMocService._instance = this;
    }

    public static get Instance(): PublishedMocService {
      return PublishedMocService._instance;
    }


    public ClearListCaches(config: PublishedMocQueryParameters | null = null) {

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


    public ConvertToPublishedMocSubmitData(data: PublishedMocData): PublishedMocSubmitData {

        let output = new PublishedMocSubmitData();

        output.id = data.id;
        output.projectId = data.projectId;
        output.name = data.name;
        output.description = data.description;
        output.thumbnailImagePath = data.thumbnailImagePath;
        output.tags = data.tags;
        output.isPublished = data.isPublished;
        output.isFeatured = data.isFeatured;
        output.publishedDate = data.publishedDate;
        output.viewCount = data.viewCount;
        output.likeCount = data.likeCount;
        output.commentCount = data.commentCount;
        output.favouriteCount = data.favouriteCount;
        output.partCount = data.partCount;
        output.allowForking = data.allowForking;
        output.visibility = data.visibility;
        output.forkCount = data.forkCount;
        output.forkedFromMocId = data.forkedFromMocId;
        output.licenseName = data.licenseName;
        output.readmeMarkdown = data.readmeMarkdown;
        output.slug = data.slug;
        output.defaultBranchName = data.defaultBranchName;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPublishedMoc(id: bigint | number, includeRelations: boolean = true) : Observable<PublishedMocData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const publishedMoc$ = this.requestPublishedMoc(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PublishedMoc", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, publishedMoc$);

            return publishedMoc$;
        }

        return this.recordCache.get(configHash) as Observable<PublishedMocData>;
    }

    private requestPublishedMoc(id: bigint | number, includeRelations: boolean = true) : Observable<PublishedMocData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PublishedMocData>(this.baseUrl + 'api/PublishedMoc/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePublishedMoc(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPublishedMoc(id, includeRelations));
            }));
    }

    public GetPublishedMocList(config: PublishedMocQueryParameters | any = null) : Observable<Array<PublishedMocData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const publishedMocList$ = this.requestPublishedMocList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PublishedMoc list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, publishedMocList$);

            return publishedMocList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PublishedMocData>>;
    }


    private requestPublishedMocList(config: PublishedMocQueryParameters | any) : Observable <Array<PublishedMocData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PublishedMocData>>(this.baseUrl + 'api/PublishedMocs', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePublishedMocList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPublishedMocList(config));
            }));
    }

    public GetPublishedMocsRowCount(config: PublishedMocQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const publishedMocsRowCount$ = this.requestPublishedMocsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get PublishedMocs row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, publishedMocsRowCount$);

            return publishedMocsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPublishedMocsRowCount(config: PublishedMocQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/PublishedMocs/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPublishedMocsRowCount(config));
            }));
    }

    public GetPublishedMocsBasicListData(config: PublishedMocQueryParameters | any = null) : Observable<Array<PublishedMocBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const publishedMocsBasicListData$ = this.requestPublishedMocsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get PublishedMocs basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, publishedMocsBasicListData$);

            return publishedMocsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PublishedMocBasicListData>>;
    }


    private requestPublishedMocsBasicListData(config: PublishedMocQueryParameters | any) : Observable<Array<PublishedMocBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PublishedMocBasicListData>>(this.baseUrl + 'api/PublishedMocs/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPublishedMocsBasicListData(config));
            }));

    }


    public PutPublishedMoc(id: bigint | number, publishedMoc: PublishedMocSubmitData) : Observable<PublishedMocData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PublishedMocData>(this.baseUrl + 'api/PublishedMoc/' + id.toString(), publishedMoc, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePublishedMoc(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPublishedMoc(id, publishedMoc));
            }));
    }


    public PostPublishedMoc(publishedMoc: PublishedMocSubmitData) : Observable<PublishedMocData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PublishedMocData>(this.baseUrl + 'api/PublishedMoc', publishedMoc, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePublishedMoc(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPublishedMoc(publishedMoc));
            }));
    }

  
    public DeletePublishedMoc(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/PublishedMoc/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePublishedMoc(id));
            }));
    }

    public RollbackPublishedMoc(id: bigint | number, versionNumber: bigint | number) : Observable<PublishedMocData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PublishedMocData>(this.baseUrl + 'api/PublishedMoc/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePublishedMoc(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackPublishedMoc(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a PublishedMoc.
     */
    public GetPublishedMocChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<PublishedMocData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PublishedMocData>>(this.baseUrl + 'api/PublishedMoc/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPublishedMocChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a PublishedMoc.
     */
    public GetPublishedMocAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<PublishedMocData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<PublishedMocData>[]>(this.baseUrl + 'api/PublishedMoc/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetPublishedMocAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a PublishedMoc.
     */
    public GetPublishedMocVersion(id: bigint | number, version: number): Observable<PublishedMocData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PublishedMocData>(this.baseUrl + 'api/PublishedMoc/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePublishedMoc(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPublishedMocVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a PublishedMoc at a specific point in time.
     */
    public GetPublishedMocStateAtTime(id: bigint | number, time: string): Observable<PublishedMocData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PublishedMocData>(this.baseUrl + 'api/PublishedMoc/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.RevivePublishedMoc(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetPublishedMocStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: PublishedMocQueryParameters | any): string {

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

    public userIsBMCPublishedMocReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCPublishedMocReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.PublishedMocs
        //
        if (userIsBMCPublishedMocReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCPublishedMocReader = user.readPermission >= 1;
            } else {
                userIsBMCPublishedMocReader = false;
            }
        }

        return userIsBMCPublishedMocReader;
    }


    public userIsBMCPublishedMocWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCPublishedMocWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.PublishedMocs
        //
        if (userIsBMCPublishedMocWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCPublishedMocWriter = user.writePermission >= 1;
          } else {
            userIsBMCPublishedMocWriter = false;
          }      
        }

        return userIsBMCPublishedMocWriter;
    }

    public GetPublishedMocChangeHistoriesForPublishedMoc(publishedMocId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PublishedMocChangeHistoryData[]> {
        return this.publishedMocChangeHistoryService.GetPublishedMocChangeHistoryList({
            publishedMocId: publishedMocId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetMocVersionsForPublishedMoc(publishedMocId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MocVersionData[]> {
        return this.mocVersionService.GetMocVersionList({
            publishedMocId: publishedMocId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPublishedMocImagesForPublishedMoc(publishedMocId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PublishedMocImageData[]> {
        return this.publishedMocImageService.GetPublishedMocImageList({
            publishedMocId: publishedMocId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetMocForkForkedMocsForPublishedMoc(publishedMocId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MocForkData[]> {
        return this.mocForkService.GetMocForkList({
            forkedMocId: publishedMocId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetMocForkSourceMocsForPublishedMoc(publishedMocId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MocForkData[]> {
        return this.mocForkService.GetMocForkList({
            sourceMocId: publishedMocId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetMocCollaboratorsForPublishedMoc(publishedMocId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MocCollaboratorData[]> {
        return this.mocCollaboratorService.GetMocCollaboratorList({
            publishedMocId: publishedMocId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetMocLikesForPublishedMoc(publishedMocId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MocLikeData[]> {
        return this.mocLikeService.GetMocLikeList({
            publishedMocId: publishedMocId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetMocCommentsForPublishedMoc(publishedMocId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MocCommentData[]> {
        return this.mocCommentService.GetMocCommentList({
            publishedMocId: publishedMocId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetMocFavouritesForPublishedMoc(publishedMocId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<MocFavouriteData[]> {
        return this.mocFavouriteService.GetMocFavouriteList({
            publishedMocId: publishedMocId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSharedInstructionsForPublishedMoc(publishedMocId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SharedInstructionData[]> {
        return this.sharedInstructionService.GetSharedInstructionList({
            publishedMocId: publishedMocId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBuildChallengeEntriesForPublishedMoc(publishedMocId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BuildChallengeEntryData[]> {
        return this.buildChallengeEntryService.GetBuildChallengeEntryList({
            publishedMocId: publishedMocId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PublishedMocData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PublishedMocData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PublishedMocTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePublishedMoc(raw: any): PublishedMocData {
    if (!raw) return raw;

    //
    // Create a PublishedMocData object instance with correct prototype
    //
    const revived = Object.create(PublishedMocData.prototype) as PublishedMocData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._publishedMocChangeHistories = null;
    (revived as any)._publishedMocChangeHistoriesPromise = null;
    (revived as any)._publishedMocChangeHistoriesSubject = new BehaviorSubject<PublishedMocChangeHistoryData[] | null>(null);

    (revived as any)._mocVersions = null;
    (revived as any)._mocVersionsPromise = null;
    (revived as any)._mocVersionsSubject = new BehaviorSubject<MocVersionData[] | null>(null);

    (revived as any)._publishedMocImages = null;
    (revived as any)._publishedMocImagesPromise = null;
    (revived as any)._publishedMocImagesSubject = new BehaviorSubject<PublishedMocImageData[] | null>(null);

    (revived as any)._mocForkForkedMocs = null;
    (revived as any)._mocForkForkedMocsPromise = null;
    (revived as any)._mocForkForkedMocsSubject = new BehaviorSubject<MocForkData[] | null>(null);

    (revived as any)._mocForkSourceMocs = null;
    (revived as any)._mocForkSourceMocsPromise = null;
    (revived as any)._mocForkSourceMocsSubject = new BehaviorSubject<MocForkData[] | null>(null);

    (revived as any)._mocCollaborators = null;
    (revived as any)._mocCollaboratorsPromise = null;
    (revived as any)._mocCollaboratorsSubject = new BehaviorSubject<MocCollaboratorData[] | null>(null);

    (revived as any)._mocLikes = null;
    (revived as any)._mocLikesPromise = null;
    (revived as any)._mocLikesSubject = new BehaviorSubject<MocLikeData[] | null>(null);

    (revived as any)._mocComments = null;
    (revived as any)._mocCommentsPromise = null;
    (revived as any)._mocCommentsSubject = new BehaviorSubject<MocCommentData[] | null>(null);

    (revived as any)._mocFavourites = null;
    (revived as any)._mocFavouritesPromise = null;
    (revived as any)._mocFavouritesSubject = new BehaviorSubject<MocFavouriteData[] | null>(null);

    (revived as any)._sharedInstructions = null;
    (revived as any)._sharedInstructionsPromise = null;
    (revived as any)._sharedInstructionsSubject = new BehaviorSubject<SharedInstructionData[] | null>(null);

    (revived as any)._buildChallengeEntries = null;
    (revived as any)._buildChallengeEntriesPromise = null;
    (revived as any)._buildChallengeEntriesSubject = new BehaviorSubject<BuildChallengeEntryData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadPublishedMocXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).PublishedMocChangeHistories$ = (revived as any)._publishedMocChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._publishedMocChangeHistories === null && (revived as any)._publishedMocChangeHistoriesPromise === null) {
                (revived as any).loadPublishedMocChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._publishedMocChangeHistoriesCount$ = null;


    (revived as any).MocVersions$ = (revived as any)._mocVersionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._mocVersions === null && (revived as any)._mocVersionsPromise === null) {
                (revived as any).loadMocVersions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._mocVersionsCount$ = null;


    (revived as any).PublishedMocImages$ = (revived as any)._publishedMocImagesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._publishedMocImages === null && (revived as any)._publishedMocImagesPromise === null) {
                (revived as any).loadPublishedMocImages();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._publishedMocImagesCount$ = null;


    (revived as any).MocForkForkedMocs$ = (revived as any)._mocForkForkedMocsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._mocForkForkedMocs === null && (revived as any)._mocForkForkedMocsPromise === null) {
                (revived as any).loadMocForkForkedMocs();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._mocForkForkedMocsCount$ = null;


    (revived as any).MocForkSourceMocs$ = (revived as any)._mocForkSourceMocsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._mocForkSourceMocs === null && (revived as any)._mocForkSourceMocsPromise === null) {
                (revived as any).loadMocForkSourceMocs();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._mocForkSourceMocsCount$ = null;


    (revived as any).MocCollaborators$ = (revived as any)._mocCollaboratorsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._mocCollaborators === null && (revived as any)._mocCollaboratorsPromise === null) {
                (revived as any).loadMocCollaborators();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._mocCollaboratorsCount$ = null;


    (revived as any).MocLikes$ = (revived as any)._mocLikesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._mocLikes === null && (revived as any)._mocLikesPromise === null) {
                (revived as any).loadMocLikes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._mocLikesCount$ = null;


    (revived as any).MocComments$ = (revived as any)._mocCommentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._mocComments === null && (revived as any)._mocCommentsPromise === null) {
                (revived as any).loadMocComments();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._mocCommentsCount$ = null;


    (revived as any).MocFavourites$ = (revived as any)._mocFavouritesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._mocFavourites === null && (revived as any)._mocFavouritesPromise === null) {
                (revived as any).loadMocFavourites();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._mocFavouritesCount$ = null;


    (revived as any).SharedInstructions$ = (revived as any)._sharedInstructionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._sharedInstructions === null && (revived as any)._sharedInstructionsPromise === null) {
                (revived as any).loadSharedInstructions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._sharedInstructionsCount$ = null;


    (revived as any).BuildChallengeEntries$ = (revived as any)._buildChallengeEntriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._buildChallengeEntries === null && (revived as any)._buildChallengeEntriesPromise === null) {
                (revived as any).loadBuildChallengeEntries();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._buildChallengeEntriesCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<PublishedMocData> | null>(null);

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

  private RevivePublishedMocList(rawList: any[]): PublishedMocData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePublishedMoc(raw));
  }

}
