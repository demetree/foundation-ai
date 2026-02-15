/*

   GENERATED SERVICE FOR THE BRICKCOLOUR TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickColour table.

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
import { ColourFinishData } from './colour-finish.service';
import { BrickPartColourService, BrickPartColourData } from './brick-part-colour.service';
import { PlacedBrickService, PlacedBrickData } from './placed-brick.service';
import { LegoSetPartService, LegoSetPartData } from './lego-set-part.service';
import { BrickElementService, BrickElementData } from './brick-element.service';
import { UserCollectionPartService, UserCollectionPartData } from './user-collection-part.service';
import { UserWishlistItemService, UserWishlistItemData } from './user-wishlist-item.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BrickColourQueryParameters {
    name: string | null | undefined = null;
    ldrawColourCode: bigint | number | null | undefined = null;
    hexRgb: string | null | undefined = null;
    hexEdgeColour: string | null | undefined = null;
    alpha: bigint | number | null | undefined = null;
    isTransparent: boolean | null | undefined = null;
    isMetallic: boolean | null | undefined = null;
    colourFinishId: bigint | number | null | undefined = null;
    luminance: bigint | number | null | undefined = null;
    legoColourId: bigint | number | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
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
export class BrickColourSubmitData {
    id!: bigint | number;
    name!: string;
    ldrawColourCode!: bigint | number;
    hexRgb: string | null = null;
    hexEdgeColour: string | null = null;
    alpha: bigint | number | null = null;
    isTransparent!: boolean;
    isMetallic!: boolean;
    colourFinishId!: bigint | number;
    luminance: bigint | number | null = null;
    legoColourId: bigint | number | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class BrickColourBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickColourChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickColour.BrickColourChildren$` — use with `| async` in templates
//        • Promise:    `brickColour.BrickColourChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickColour.BrickColourChildren$ | async"`), or
//        • Access the promise getter (`brickColour.BrickColourChildren` or `await brickColour.BrickColourChildren`)
//    - Simply reading `brickColour.BrickColourChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickColour.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickColourData {
    id!: bigint | number;
    name!: string;
    ldrawColourCode!: bigint | number;
    hexRgb!: string | null;
    hexEdgeColour!: string | null;
    alpha!: bigint | number;
    isTransparent!: boolean;
    isMetallic!: boolean;
    colourFinishId!: bigint | number;
    luminance!: bigint | number;
    legoColourId!: bigint | number;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    colourFinish: ColourFinishData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _brickPartColours: BrickPartColourData[] | null = null;
    private _brickPartColoursPromise: Promise<BrickPartColourData[]> | null  = null;
    private _brickPartColoursSubject = new BehaviorSubject<BrickPartColourData[] | null>(null);

                
    private _placedBricks: PlacedBrickData[] | null = null;
    private _placedBricksPromise: Promise<PlacedBrickData[]> | null  = null;
    private _placedBricksSubject = new BehaviorSubject<PlacedBrickData[] | null>(null);

                
    private _legoSetParts: LegoSetPartData[] | null = null;
    private _legoSetPartsPromise: Promise<LegoSetPartData[]> | null  = null;
    private _legoSetPartsSubject = new BehaviorSubject<LegoSetPartData[] | null>(null);

                
    private _brickElements: BrickElementData[] | null = null;
    private _brickElementsPromise: Promise<BrickElementData[]> | null  = null;
    private _brickElementsSubject = new BehaviorSubject<BrickElementData[] | null>(null);

                
    private _userCollectionParts: UserCollectionPartData[] | null = null;
    private _userCollectionPartsPromise: Promise<UserCollectionPartData[]> | null  = null;
    private _userCollectionPartsSubject = new BehaviorSubject<UserCollectionPartData[] | null>(null);

                
    private _userWishlistItems: UserWishlistItemData[] | null = null;
    private _userWishlistItemsPromise: Promise<UserWishlistItemData[]> | null  = null;
    private _userWishlistItemsSubject = new BehaviorSubject<UserWishlistItemData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public BrickPartColours$ = this._brickPartColoursSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickPartColours === null && this._brickPartColoursPromise === null) {
            this.loadBrickPartColours(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public BrickPartColoursCount$ = BrickPartColourService.Instance.GetBrickPartColoursRowCount({brickColourId: this.id,
      active: true,
      deleted: false
    });



    public PlacedBricks$ = this._placedBricksSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._placedBricks === null && this._placedBricksPromise === null) {
            this.loadPlacedBricks(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public PlacedBricksCount$ = PlacedBrickService.Instance.GetPlacedBricksRowCount({brickColourId: this.id,
      active: true,
      deleted: false
    });



    public LegoSetParts$ = this._legoSetPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._legoSetParts === null && this._legoSetPartsPromise === null) {
            this.loadLegoSetParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public LegoSetPartsCount$ = LegoSetPartService.Instance.GetLegoSetPartsRowCount({brickColourId: this.id,
      active: true,
      deleted: false
    });



    public BrickElements$ = this._brickElementsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickElements === null && this._brickElementsPromise === null) {
            this.loadBrickElements(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public BrickElementsCount$ = BrickElementService.Instance.GetBrickElementsRowCount({brickColourId: this.id,
      active: true,
      deleted: false
    });



    public UserCollectionParts$ = this._userCollectionPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userCollectionParts === null && this._userCollectionPartsPromise === null) {
            this.loadUserCollectionParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public UserCollectionPartsCount$ = UserCollectionPartService.Instance.GetUserCollectionPartsRowCount({brickColourId: this.id,
      active: true,
      deleted: false
    });



    public UserWishlistItems$ = this._userWishlistItemsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userWishlistItems === null && this._userWishlistItemsPromise === null) {
            this.loadUserWishlistItems(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public UserWishlistItemsCount$ = UserWishlistItemService.Instance.GetUserWishlistItemsRowCount({brickColourId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any BrickColourData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickColour.Reload();
  //
  //  Non Async:
  //
  //     brickColour[0].Reload().then(x => {
  //        this.brickColour = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickColourService.Instance.GetBrickColour(this.id, includeRelations)
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
     this._brickPartColours = null;
     this._brickPartColoursPromise = null;
     this._brickPartColoursSubject.next(null);

     this._placedBricks = null;
     this._placedBricksPromise = null;
     this._placedBricksSubject.next(null);

     this._legoSetParts = null;
     this._legoSetPartsPromise = null;
     this._legoSetPartsSubject.next(null);

     this._brickElements = null;
     this._brickElementsPromise = null;
     this._brickElementsSubject.next(null);

     this._userCollectionParts = null;
     this._userCollectionPartsPromise = null;
     this._userCollectionPartsSubject.next(null);

     this._userWishlistItems = null;
     this._userWishlistItemsPromise = null;
     this._userWishlistItemsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the BrickPartColours for this BrickColour.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickColour.BrickPartColours.then(brickColours => { ... })
     *   or
     *   await this.brickColour.brickColours
     *
    */
    public get BrickPartColours(): Promise<BrickPartColourData[]> {
        if (this._brickPartColours !== null) {
            return Promise.resolve(this._brickPartColours);
        }

        if (this._brickPartColoursPromise !== null) {
            return this._brickPartColoursPromise;
        }

        // Start the load
        this.loadBrickPartColours();

        return this._brickPartColoursPromise!;
    }



    private loadBrickPartColours(): void {

        this._brickPartColoursPromise = lastValueFrom(
            BrickColourService.Instance.GetBrickPartColoursForBrickColour(this.id)
        )
        .then(BrickPartColours => {
            this._brickPartColours = BrickPartColours ?? [];
            this._brickPartColoursSubject.next(this._brickPartColours);
            return this._brickPartColours;
         })
        .catch(err => {
            this._brickPartColours = [];
            this._brickPartColoursSubject.next(this._brickPartColours);
            throw err;
        })
        .finally(() => {
            this._brickPartColoursPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BrickPartColour. Call after mutations to force refresh.
     */
    public ClearBrickPartColoursCache(): void {
        this._brickPartColours = null;
        this._brickPartColoursPromise = null;
        this._brickPartColoursSubject.next(this._brickPartColours);      // Emit to observable
    }

    public get HasBrickPartColours(): Promise<boolean> {
        return this.BrickPartColours.then(brickPartColours => brickPartColours.length > 0);
    }


    /**
     *
     * Gets the PlacedBricks for this BrickColour.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickColour.PlacedBricks.then(brickColours => { ... })
     *   or
     *   await this.brickColour.brickColours
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
            BrickColourService.Instance.GetPlacedBricksForBrickColour(this.id)
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
     * Gets the LegoSetParts for this BrickColour.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickColour.LegoSetParts.then(brickColours => { ... })
     *   or
     *   await this.brickColour.brickColours
     *
    */
    public get LegoSetParts(): Promise<LegoSetPartData[]> {
        if (this._legoSetParts !== null) {
            return Promise.resolve(this._legoSetParts);
        }

        if (this._legoSetPartsPromise !== null) {
            return this._legoSetPartsPromise;
        }

        // Start the load
        this.loadLegoSetParts();

        return this._legoSetPartsPromise!;
    }



    private loadLegoSetParts(): void {

        this._legoSetPartsPromise = lastValueFrom(
            BrickColourService.Instance.GetLegoSetPartsForBrickColour(this.id)
        )
        .then(LegoSetParts => {
            this._legoSetParts = LegoSetParts ?? [];
            this._legoSetPartsSubject.next(this._legoSetParts);
            return this._legoSetParts;
         })
        .catch(err => {
            this._legoSetParts = [];
            this._legoSetPartsSubject.next(this._legoSetParts);
            throw err;
        })
        .finally(() => {
            this._legoSetPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached LegoSetPart. Call after mutations to force refresh.
     */
    public ClearLegoSetPartsCache(): void {
        this._legoSetParts = null;
        this._legoSetPartsPromise = null;
        this._legoSetPartsSubject.next(this._legoSetParts);      // Emit to observable
    }

    public get HasLegoSetParts(): Promise<boolean> {
        return this.LegoSetParts.then(legoSetParts => legoSetParts.length > 0);
    }


    /**
     *
     * Gets the BrickElements for this BrickColour.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickColour.BrickElements.then(brickColours => { ... })
     *   or
     *   await this.brickColour.brickColours
     *
    */
    public get BrickElements(): Promise<BrickElementData[]> {
        if (this._brickElements !== null) {
            return Promise.resolve(this._brickElements);
        }

        if (this._brickElementsPromise !== null) {
            return this._brickElementsPromise;
        }

        // Start the load
        this.loadBrickElements();

        return this._brickElementsPromise!;
    }



    private loadBrickElements(): void {

        this._brickElementsPromise = lastValueFrom(
            BrickColourService.Instance.GetBrickElementsForBrickColour(this.id)
        )
        .then(BrickElements => {
            this._brickElements = BrickElements ?? [];
            this._brickElementsSubject.next(this._brickElements);
            return this._brickElements;
         })
        .catch(err => {
            this._brickElements = [];
            this._brickElementsSubject.next(this._brickElements);
            throw err;
        })
        .finally(() => {
            this._brickElementsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BrickElement. Call after mutations to force refresh.
     */
    public ClearBrickElementsCache(): void {
        this._brickElements = null;
        this._brickElementsPromise = null;
        this._brickElementsSubject.next(this._brickElements);      // Emit to observable
    }

    public get HasBrickElements(): Promise<boolean> {
        return this.BrickElements.then(brickElements => brickElements.length > 0);
    }


    /**
     *
     * Gets the UserCollectionParts for this BrickColour.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickColour.UserCollectionParts.then(brickColours => { ... })
     *   or
     *   await this.brickColour.brickColours
     *
    */
    public get UserCollectionParts(): Promise<UserCollectionPartData[]> {
        if (this._userCollectionParts !== null) {
            return Promise.resolve(this._userCollectionParts);
        }

        if (this._userCollectionPartsPromise !== null) {
            return this._userCollectionPartsPromise;
        }

        // Start the load
        this.loadUserCollectionParts();

        return this._userCollectionPartsPromise!;
    }



    private loadUserCollectionParts(): void {

        this._userCollectionPartsPromise = lastValueFrom(
            BrickColourService.Instance.GetUserCollectionPartsForBrickColour(this.id)
        )
        .then(UserCollectionParts => {
            this._userCollectionParts = UserCollectionParts ?? [];
            this._userCollectionPartsSubject.next(this._userCollectionParts);
            return this._userCollectionParts;
         })
        .catch(err => {
            this._userCollectionParts = [];
            this._userCollectionPartsSubject.next(this._userCollectionParts);
            throw err;
        })
        .finally(() => {
            this._userCollectionPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserCollectionPart. Call after mutations to force refresh.
     */
    public ClearUserCollectionPartsCache(): void {
        this._userCollectionParts = null;
        this._userCollectionPartsPromise = null;
        this._userCollectionPartsSubject.next(this._userCollectionParts);      // Emit to observable
    }

    public get HasUserCollectionParts(): Promise<boolean> {
        return this.UserCollectionParts.then(userCollectionParts => userCollectionParts.length > 0);
    }


    /**
     *
     * Gets the UserWishlistItems for this BrickColour.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickColour.UserWishlistItems.then(brickColours => { ... })
     *   or
     *   await this.brickColour.brickColours
     *
    */
    public get UserWishlistItems(): Promise<UserWishlistItemData[]> {
        if (this._userWishlistItems !== null) {
            return Promise.resolve(this._userWishlistItems);
        }

        if (this._userWishlistItemsPromise !== null) {
            return this._userWishlistItemsPromise;
        }

        // Start the load
        this.loadUserWishlistItems();

        return this._userWishlistItemsPromise!;
    }



    private loadUserWishlistItems(): void {

        this._userWishlistItemsPromise = lastValueFrom(
            BrickColourService.Instance.GetUserWishlistItemsForBrickColour(this.id)
        )
        .then(UserWishlistItems => {
            this._userWishlistItems = UserWishlistItems ?? [];
            this._userWishlistItemsSubject.next(this._userWishlistItems);
            return this._userWishlistItems;
         })
        .catch(err => {
            this._userWishlistItems = [];
            this._userWishlistItemsSubject.next(this._userWishlistItems);
            throw err;
        })
        .finally(() => {
            this._userWishlistItemsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserWishlistItem. Call after mutations to force refresh.
     */
    public ClearUserWishlistItemsCache(): void {
        this._userWishlistItems = null;
        this._userWishlistItemsPromise = null;
        this._userWishlistItemsSubject.next(this._userWishlistItems);      // Emit to observable
    }

    public get HasUserWishlistItems(): Promise<boolean> {
        return this.UserWishlistItems.then(userWishlistItems => userWishlistItems.length > 0);
    }




    /**
     * Updates the state of this BrickColourData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickColourData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickColourSubmitData {
        return BrickColourService.Instance.ConvertToBrickColourSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickColourService extends SecureEndpointBase {

    private static _instance: BrickColourService;
    private listCache: Map<string, Observable<Array<BrickColourData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickColourBasicListData>>>;
    private recordCache: Map<string, Observable<BrickColourData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private brickPartColourService: BrickPartColourService,
        private placedBrickService: PlacedBrickService,
        private legoSetPartService: LegoSetPartService,
        private brickElementService: BrickElementService,
        private userCollectionPartService: UserCollectionPartService,
        private userWishlistItemService: UserWishlistItemService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickColourData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickColourBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickColourData>>();

        BrickColourService._instance = this;
    }

    public static get Instance(): BrickColourService {
      return BrickColourService._instance;
    }


    public ClearListCaches(config: BrickColourQueryParameters | null = null) {

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


    public ConvertToBrickColourSubmitData(data: BrickColourData): BrickColourSubmitData {

        let output = new BrickColourSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.ldrawColourCode = data.ldrawColourCode;
        output.hexRgb = data.hexRgb;
        output.hexEdgeColour = data.hexEdgeColour;
        output.alpha = data.alpha;
        output.isTransparent = data.isTransparent;
        output.isMetallic = data.isMetallic;
        output.colourFinishId = data.colourFinishId;
        output.luminance = data.luminance;
        output.legoColourId = data.legoColourId;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBrickColour(id: bigint | number, includeRelations: boolean = true) : Observable<BrickColourData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickColour$ = this.requestBrickColour(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickColour", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickColour$);

            return brickColour$;
        }

        return this.recordCache.get(configHash) as Observable<BrickColourData>;
    }

    private requestBrickColour(id: bigint | number, includeRelations: boolean = true) : Observable<BrickColourData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickColourData>(this.baseUrl + 'api/BrickColour/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickColour(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickColour(id, includeRelations));
            }));
    }

    public GetBrickColourList(config: BrickColourQueryParameters | any = null) : Observable<Array<BrickColourData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickColourList$ = this.requestBrickColourList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickColour list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickColourList$);

            return brickColourList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickColourData>>;
    }


    private requestBrickColourList(config: BrickColourQueryParameters | any) : Observable <Array<BrickColourData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickColourData>>(this.baseUrl + 'api/BrickColours', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickColourList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickColourList(config));
            }));
    }

    public GetBrickColoursRowCount(config: BrickColourQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickColoursRowCount$ = this.requestBrickColoursRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickColours row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickColoursRowCount$);

            return brickColoursRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickColoursRowCount(config: BrickColourQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickColours/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickColoursRowCount(config));
            }));
    }

    public GetBrickColoursBasicListData(config: BrickColourQueryParameters | any = null) : Observable<Array<BrickColourBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickColoursBasicListData$ = this.requestBrickColoursBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickColours basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickColoursBasicListData$);

            return brickColoursBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickColourBasicListData>>;
    }


    private requestBrickColoursBasicListData(config: BrickColourQueryParameters | any) : Observable<Array<BrickColourBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickColourBasicListData>>(this.baseUrl + 'api/BrickColours/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickColoursBasicListData(config));
            }));

    }


    public PutBrickColour(id: bigint | number, brickColour: BrickColourSubmitData) : Observable<BrickColourData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickColourData>(this.baseUrl + 'api/BrickColour/' + id.toString(), brickColour, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickColour(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickColour(id, brickColour));
            }));
    }


    public PostBrickColour(brickColour: BrickColourSubmitData) : Observable<BrickColourData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickColourData>(this.baseUrl + 'api/BrickColour', brickColour, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickColour(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickColour(brickColour));
            }));
    }

  
    public DeleteBrickColour(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickColour/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickColour(id));
            }));
    }


    private getConfigHash(config: BrickColourQueryParameters | any): string {

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

    public userIsBMCBrickColourReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickColourReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickColours
        //
        if (userIsBMCBrickColourReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickColourReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickColourReader = false;
            }
        }

        return userIsBMCBrickColourReader;
    }


    public userIsBMCBrickColourWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickColourWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickColours
        //
        if (userIsBMCBrickColourWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickColourWriter = user.writePermission >= 255;
          } else {
            userIsBMCBrickColourWriter = false;
          }      
        }

        return userIsBMCBrickColourWriter;
    }

    public GetBrickPartColoursForBrickColour(brickColourId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickPartColourData[]> {
        return this.brickPartColourService.GetBrickPartColourList({
            brickColourId: brickColourId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPlacedBricksForBrickColour(brickColourId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PlacedBrickData[]> {
        return this.placedBrickService.GetPlacedBrickList({
            brickColourId: brickColourId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetLegoSetPartsForBrickColour(brickColourId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<LegoSetPartData[]> {
        return this.legoSetPartService.GetLegoSetPartList({
            brickColourId: brickColourId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBrickElementsForBrickColour(brickColourId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickElementData[]> {
        return this.brickElementService.GetBrickElementList({
            brickColourId: brickColourId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserCollectionPartsForBrickColour(brickColourId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserCollectionPartData[]> {
        return this.userCollectionPartService.GetUserCollectionPartList({
            brickColourId: brickColourId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserWishlistItemsForBrickColour(brickColourId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserWishlistItemData[]> {
        return this.userWishlistItemService.GetUserWishlistItemList({
            brickColourId: brickColourId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full BrickColourData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickColourData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickColourTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickColour(raw: any): BrickColourData {
    if (!raw) return raw;

    //
    // Create a BrickColourData object instance with correct prototype
    //
    const revived = Object.create(BrickColourData.prototype) as BrickColourData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._brickPartColours = null;
    (revived as any)._brickPartColoursPromise = null;
    (revived as any)._brickPartColoursSubject = new BehaviorSubject<BrickPartColourData[] | null>(null);

    (revived as any)._placedBricks = null;
    (revived as any)._placedBricksPromise = null;
    (revived as any)._placedBricksSubject = new BehaviorSubject<PlacedBrickData[] | null>(null);

    (revived as any)._legoSetParts = null;
    (revived as any)._legoSetPartsPromise = null;
    (revived as any)._legoSetPartsSubject = new BehaviorSubject<LegoSetPartData[] | null>(null);

    (revived as any)._brickElements = null;
    (revived as any)._brickElementsPromise = null;
    (revived as any)._brickElementsSubject = new BehaviorSubject<BrickElementData[] | null>(null);

    (revived as any)._userCollectionParts = null;
    (revived as any)._userCollectionPartsPromise = null;
    (revived as any)._userCollectionPartsSubject = new BehaviorSubject<UserCollectionPartData[] | null>(null);

    (revived as any)._userWishlistItems = null;
    (revived as any)._userWishlistItemsPromise = null;
    (revived as any)._userWishlistItemsSubject = new BehaviorSubject<UserWishlistItemData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadBrickColourXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).BrickPartColours$ = (revived as any)._brickPartColoursSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickPartColours === null && (revived as any)._brickPartColoursPromise === null) {
                (revived as any).loadBrickPartColours();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).BrickPartColoursCount$ = BrickPartColourService.Instance.GetBrickPartColoursRowCount({brickColourId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).PlacedBricks$ = (revived as any)._placedBricksSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._placedBricks === null && (revived as any)._placedBricksPromise === null) {
                (revived as any).loadPlacedBricks();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).PlacedBricksCount$ = PlacedBrickService.Instance.GetPlacedBricksRowCount({brickColourId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).LegoSetParts$ = (revived as any)._legoSetPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._legoSetParts === null && (revived as any)._legoSetPartsPromise === null) {
                (revived as any).loadLegoSetParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).LegoSetPartsCount$ = LegoSetPartService.Instance.GetLegoSetPartsRowCount({brickColourId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).BrickElements$ = (revived as any)._brickElementsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickElements === null && (revived as any)._brickElementsPromise === null) {
                (revived as any).loadBrickElements();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).BrickElementsCount$ = BrickElementService.Instance.GetBrickElementsRowCount({brickColourId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).UserCollectionParts$ = (revived as any)._userCollectionPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userCollectionParts === null && (revived as any)._userCollectionPartsPromise === null) {
                (revived as any).loadUserCollectionParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).UserCollectionPartsCount$ = UserCollectionPartService.Instance.GetUserCollectionPartsRowCount({brickColourId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).UserWishlistItems$ = (revived as any)._userWishlistItemsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userWishlistItems === null && (revived as any)._userWishlistItemsPromise === null) {
                (revived as any).loadUserWishlistItems();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).UserWishlistItemsCount$ = UserWishlistItemService.Instance.GetUserWishlistItemsRowCount({brickColourId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveBrickColourList(rawList: any[]): BrickColourData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickColour(raw));
  }

}
