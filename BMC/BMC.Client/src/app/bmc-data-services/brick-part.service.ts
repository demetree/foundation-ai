/*

   GENERATED SERVICE FOR THE BRICKPART TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the BrickPart table.

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
import { PartTypeData } from './part-type.service';
import { BrickCategoryData } from './brick-category.service';
import { BrickPartChangeHistoryService, BrickPartChangeHistoryData } from './brick-part-change-history.service';
import { BrickPartConnectorService, BrickPartConnectorData } from './brick-part-connector.service';
import { BrickPartColourService, BrickPartColourData } from './brick-part-colour.service';
import { PartSubFileReferenceService, PartSubFileReferenceData } from './part-sub-file-reference.service';
import { PlacedBrickService, PlacedBrickData } from './placed-brick.service';
import { ModelStepPartService, ModelStepPartData } from './model-step-part.service';
import { LegoSetPartService, LegoSetPartData } from './lego-set-part.service';
import { BrickPartRelationshipService, BrickPartRelationshipData } from './brick-part-relationship.service';
import { BrickElementService, BrickElementData } from './brick-element.service';
import { UserCollectionPartService, UserCollectionPartData } from './user-collection-part.service';
import { UserWishlistItemService, UserWishlistItemData } from './user-wishlist-item.service';
import { UserPartListItemService, UserPartListItemData } from './user-part-list-item.service';
import { UserLostPartService, UserLostPartData } from './user-lost-part.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class BrickPartQueryParameters {
    name: string | null | undefined = null;
    rebrickablePartNum: string | null | undefined = null;
    rebrickablePartUrl: string | null | undefined = null;
    rebrickableImgUrl: string | null | undefined = null;
    ldrawPartId: string | null | undefined = null;
    bricklinkId: string | null | undefined = null;
    brickowlId: string | null | undefined = null;
    legoDesignId: string | null | undefined = null;
    ldrawTitle: string | null | undefined = null;
    ldrawCategory: string | null | undefined = null;
    partTypeId: bigint | number | null | undefined = null;
    keywords: string | null | undefined = null;
    author: string | null | undefined = null;
    brickCategoryId: bigint | number | null | undefined = null;
    widthLdu: number | null | undefined = null;
    heightLdu: number | null | undefined = null;
    depthLdu: number | null | undefined = null;
    massGrams: number | null | undefined = null;
    momentOfInertiaX: number | null | undefined = null;
    momentOfInertiaY: number | null | undefined = null;
    momentOfInertiaZ: number | null | undefined = null;
    frictionCoefficient: number | null | undefined = null;
    materialType: string | null | undefined = null;
    centerOfMassX: number | null | undefined = null;
    centerOfMassY: number | null | undefined = null;
    centerOfMassZ: number | null | undefined = null;
    geometryFileName: string | null | undefined = null;
    geometrySize: bigint | number | null | undefined = null;
    geometryMimeType: string | null | undefined = null;
    geometryFileFormat: string | null | undefined = null;
    geometryOriginalFileName: string | null | undefined = null;
    boundingBoxMinX: number | null | undefined = null;
    boundingBoxMinY: number | null | undefined = null;
    boundingBoxMinZ: number | null | undefined = null;
    boundingBoxMaxX: number | null | undefined = null;
    boundingBoxMaxY: number | null | undefined = null;
    boundingBoxMaxZ: number | null | undefined = null;
    subFileCount: bigint | number | null | undefined = null;
    polygonCount: bigint | number | null | undefined = null;
    toothCount: bigint | number | null | undefined = null;
    gearRatio: number | null | undefined = null;
    lastModifiedDate: string | null | undefined = null;        // ISO 8601 (full datetime)
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
export class BrickPartSubmitData {
    id!: bigint | number;
    name!: string;
    rebrickablePartNum!: string;
    rebrickablePartUrl: string | null = null;
    rebrickableImgUrl: string | null = null;
    ldrawPartId: string | null = null;
    bricklinkId: string | null = null;
    brickowlId: string | null = null;
    legoDesignId: string | null = null;
    ldrawTitle: string | null = null;
    ldrawCategory: string | null = null;
    partTypeId!: bigint | number;
    keywords: string | null = null;
    author: string | null = null;
    brickCategoryId!: bigint | number;
    widthLdu: number | null = null;
    heightLdu: number | null = null;
    depthLdu: number | null = null;
    massGrams: number | null = null;
    momentOfInertiaX: number | null = null;
    momentOfInertiaY: number | null = null;
    momentOfInertiaZ: number | null = null;
    frictionCoefficient: number | null = null;
    materialType: string | null = null;
    centerOfMassX: number | null = null;
    centerOfMassY: number | null = null;
    centerOfMassZ: number | null = null;
    geometryFileName: string | null = null;
    geometrySize: bigint | number | null = null;
    geometryData: string | null = null;
    geometryMimeType: string | null = null;
    geometryFileFormat: string | null = null;
    geometryOriginalFileName: string | null = null;
    boundingBoxMinX: number | null = null;
    boundingBoxMinY: number | null = null;
    boundingBoxMinZ: number | null = null;
    boundingBoxMaxX: number | null = null;
    boundingBoxMaxY: number | null = null;
    boundingBoxMaxZ: number | null = null;
    subFileCount: bigint | number | null = null;
    polygonCount: bigint | number | null = null;
    toothCount: bigint | number | null = null;
    gearRatio: number | null = null;
    lastModifiedDate: string | null = null;     // ISO 8601 (full datetime)
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

export class BrickPartBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. BrickPartChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `brickPart.BrickPartChildren$` — use with `| async` in templates
//        • Promise:    `brickPart.BrickPartChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="brickPart.BrickPartChildren$ | async"`), or
//        • Access the promise getter (`brickPart.BrickPartChildren` or `await brickPart.BrickPartChildren`)
//    - Simply reading `brickPart.BrickPartChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await brickPart.Reload()` to refresh the entire object and clear all lazy caches.
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
export class BrickPartData {
    id!: bigint | number;
    name!: string;
    rebrickablePartNum!: string;
    rebrickablePartUrl!: string | null;
    rebrickableImgUrl!: string | null;
    ldrawPartId!: string | null;
    bricklinkId!: string | null;
    brickowlId!: string | null;
    legoDesignId!: string | null;
    ldrawTitle!: string | null;
    ldrawCategory!: string | null;
    partTypeId!: bigint | number;
    keywords!: string | null;
    author!: string | null;
    brickCategoryId!: bigint | number;
    widthLdu!: number | null;
    heightLdu!: number | null;
    depthLdu!: number | null;
    massGrams!: number | null;
    momentOfInertiaX!: number | null;
    momentOfInertiaY!: number | null;
    momentOfInertiaZ!: number | null;
    frictionCoefficient!: number | null;
    materialType!: string | null;
    centerOfMassX!: number | null;
    centerOfMassY!: number | null;
    centerOfMassZ!: number | null;
    geometryFileName!: string | null;
    geometrySize!: bigint | number;
    geometryData!: string | null;
    geometryMimeType!: string | null;
    geometryFileFormat!: string | null;
    geometryOriginalFileName!: string | null;
    boundingBoxMinX!: number | null;
    boundingBoxMinY!: number | null;
    boundingBoxMinZ!: number | null;
    boundingBoxMaxX!: number | null;
    boundingBoxMaxY!: number | null;
    boundingBoxMaxZ!: number | null;
    subFileCount!: bigint | number;
    polygonCount!: bigint | number;
    toothCount!: bigint | number;
    gearRatio!: number | null;
    lastModifiedDate!: string | null;   // ISO 8601 (full datetime)
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    brickCategory: BrickCategoryData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    partType: PartTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _brickPartChangeHistories: BrickPartChangeHistoryData[] | null = null;
    private _brickPartChangeHistoriesPromise: Promise<BrickPartChangeHistoryData[]> | null  = null;
    private _brickPartChangeHistoriesSubject = new BehaviorSubject<BrickPartChangeHistoryData[] | null>(null);

                
    private _brickPartConnectors: BrickPartConnectorData[] | null = null;
    private _brickPartConnectorsPromise: Promise<BrickPartConnectorData[]> | null  = null;
    private _brickPartConnectorsSubject = new BehaviorSubject<BrickPartConnectorData[] | null>(null);

                
    private _brickPartColours: BrickPartColourData[] | null = null;
    private _brickPartColoursPromise: Promise<BrickPartColourData[]> | null  = null;
    private _brickPartColoursSubject = new BehaviorSubject<BrickPartColourData[] | null>(null);

                
    private _partSubFileReferenceParentBrickParts: PartSubFileReferenceData[] | null = null;
    private _partSubFileReferenceParentBrickPartsPromise: Promise<PartSubFileReferenceData[]> | null  = null;
    private _partSubFileReferenceParentBrickPartsSubject = new BehaviorSubject<PartSubFileReferenceData[] | null>(null);
                    
    private _partSubFileReferenceReferencedBrickParts: PartSubFileReferenceData[] | null = null;
    private _partSubFileReferenceReferencedBrickPartsPromise: Promise<PartSubFileReferenceData[]> | null  = null;
    private _partSubFileReferenceReferencedBrickPartsSubject = new BehaviorSubject<PartSubFileReferenceData[] | null>(null);
                    
    private _placedBricks: PlacedBrickData[] | null = null;
    private _placedBricksPromise: Promise<PlacedBrickData[]> | null  = null;
    private _placedBricksSubject = new BehaviorSubject<PlacedBrickData[] | null>(null);

                
    private _modelStepParts: ModelStepPartData[] | null = null;
    private _modelStepPartsPromise: Promise<ModelStepPartData[]> | null  = null;
    private _modelStepPartsSubject = new BehaviorSubject<ModelStepPartData[] | null>(null);

                
    private _legoSetParts: LegoSetPartData[] | null = null;
    private _legoSetPartsPromise: Promise<LegoSetPartData[]> | null  = null;
    private _legoSetPartsSubject = new BehaviorSubject<LegoSetPartData[] | null>(null);

                
    private _brickPartRelationshipChildBrickParts: BrickPartRelationshipData[] | null = null;
    private _brickPartRelationshipChildBrickPartsPromise: Promise<BrickPartRelationshipData[]> | null  = null;
    private _brickPartRelationshipChildBrickPartsSubject = new BehaviorSubject<BrickPartRelationshipData[] | null>(null);
                    
    private _brickPartRelationshipParentBrickParts: BrickPartRelationshipData[] | null = null;
    private _brickPartRelationshipParentBrickPartsPromise: Promise<BrickPartRelationshipData[]> | null  = null;
    private _brickPartRelationshipParentBrickPartsSubject = new BehaviorSubject<BrickPartRelationshipData[] | null>(null);
                    
    private _brickElements: BrickElementData[] | null = null;
    private _brickElementsPromise: Promise<BrickElementData[]> | null  = null;
    private _brickElementsSubject = new BehaviorSubject<BrickElementData[] | null>(null);

                
    private _userCollectionParts: UserCollectionPartData[] | null = null;
    private _userCollectionPartsPromise: Promise<UserCollectionPartData[]> | null  = null;
    private _userCollectionPartsSubject = new BehaviorSubject<UserCollectionPartData[] | null>(null);

                
    private _userWishlistItems: UserWishlistItemData[] | null = null;
    private _userWishlistItemsPromise: Promise<UserWishlistItemData[]> | null  = null;
    private _userWishlistItemsSubject = new BehaviorSubject<UserWishlistItemData[] | null>(null);

                
    private _userPartListItems: UserPartListItemData[] | null = null;
    private _userPartListItemsPromise: Promise<UserPartListItemData[]> | null  = null;
    private _userPartListItemsSubject = new BehaviorSubject<UserPartListItemData[] | null>(null);

                
    private _userLostParts: UserLostPartData[] | null = null;
    private _userLostPartsPromise: Promise<UserLostPartData[]> | null  = null;
    private _userLostPartsSubject = new BehaviorSubject<UserLostPartData[] | null>(null);

                


    //
    // Version history lazy-loading cache for current version metadata
    //
    private _currentVersionInfo: VersionInformation<BrickPartData> | null = null;
    private _currentVersionInfoPromise: Promise<VersionInformation<BrickPartData>> | null = null;
    private _currentVersionInfoSubject = new BehaviorSubject<VersionInformation<BrickPartData> | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public BrickPartChangeHistories$ = this._brickPartChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickPartChangeHistories === null && this._brickPartChangeHistoriesPromise === null) {
            this.loadBrickPartChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _brickPartChangeHistoriesCount$: Observable<bigint | number> | null = null;
    public get BrickPartChangeHistoriesCount$(): Observable<bigint | number> {
        if (this._brickPartChangeHistoriesCount$ === null) {
            this._brickPartChangeHistoriesCount$ = BrickPartChangeHistoryService.Instance.GetBrickPartChangeHistoriesRowCount({brickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._brickPartChangeHistoriesCount$;
    }



    public BrickPartConnectors$ = this._brickPartConnectorsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickPartConnectors === null && this._brickPartConnectorsPromise === null) {
            this.loadBrickPartConnectors(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _brickPartConnectorsCount$: Observable<bigint | number> | null = null;
    public get BrickPartConnectorsCount$(): Observable<bigint | number> {
        if (this._brickPartConnectorsCount$ === null) {
            this._brickPartConnectorsCount$ = BrickPartConnectorService.Instance.GetBrickPartConnectorsRowCount({brickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._brickPartConnectorsCount$;
    }



    public BrickPartColours$ = this._brickPartColoursSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickPartColours === null && this._brickPartColoursPromise === null) {
            this.loadBrickPartColours(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _brickPartColoursCount$: Observable<bigint | number> | null = null;
    public get BrickPartColoursCount$(): Observable<bigint | number> {
        if (this._brickPartColoursCount$ === null) {
            this._brickPartColoursCount$ = BrickPartColourService.Instance.GetBrickPartColoursRowCount({brickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._brickPartColoursCount$;
    }



    public PartSubFileReferenceParentBrickParts$ = this._partSubFileReferenceParentBrickPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._partSubFileReferenceParentBrickParts === null && this._partSubFileReferenceParentBrickPartsPromise === null) {
            this.loadPartSubFileReferenceParentBrickParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _partSubFileReferenceParentBrickPartsCount$: Observable<bigint | number> | null = null;
    public get PartSubFileReferenceParentBrickPartsCount$(): Observable<bigint | number> {
        if (this._partSubFileReferenceParentBrickPartsCount$ === null) {
            this._partSubFileReferenceParentBrickPartsCount$ = PartSubFileReferenceService.Instance.GetPartSubFileReferencesRowCount({parentBrickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._partSubFileReferenceParentBrickPartsCount$;
    }


    public PartSubFileReferenceReferencedBrickParts$ = this._partSubFileReferenceReferencedBrickPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._partSubFileReferenceReferencedBrickParts === null && this._partSubFileReferenceReferencedBrickPartsPromise === null) {
            this.loadPartSubFileReferenceReferencedBrickParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _partSubFileReferenceReferencedBrickPartsCount$: Observable<bigint | number> | null = null;
    public get PartSubFileReferenceReferencedBrickPartsCount$(): Observable<bigint | number> {
        if (this._partSubFileReferenceReferencedBrickPartsCount$ === null) {
            this._partSubFileReferenceReferencedBrickPartsCount$ = PartSubFileReferenceService.Instance.GetPartSubFileReferencesRowCount({referencedBrickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._partSubFileReferenceReferencedBrickPartsCount$;
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
            this._placedBricksCount$ = PlacedBrickService.Instance.GetPlacedBricksRowCount({brickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._placedBricksCount$;
    }



    public ModelStepParts$ = this._modelStepPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._modelStepParts === null && this._modelStepPartsPromise === null) {
            this.loadModelStepParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _modelStepPartsCount$: Observable<bigint | number> | null = null;
    public get ModelStepPartsCount$(): Observable<bigint | number> {
        if (this._modelStepPartsCount$ === null) {
            this._modelStepPartsCount$ = ModelStepPartService.Instance.GetModelStepPartsRowCount({brickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._modelStepPartsCount$;
    }



    public LegoSetParts$ = this._legoSetPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._legoSetParts === null && this._legoSetPartsPromise === null) {
            this.loadLegoSetParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _legoSetPartsCount$: Observable<bigint | number> | null = null;
    public get LegoSetPartsCount$(): Observable<bigint | number> {
        if (this._legoSetPartsCount$ === null) {
            this._legoSetPartsCount$ = LegoSetPartService.Instance.GetLegoSetPartsRowCount({brickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._legoSetPartsCount$;
    }



    public BrickPartRelationshipChildBrickParts$ = this._brickPartRelationshipChildBrickPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickPartRelationshipChildBrickParts === null && this._brickPartRelationshipChildBrickPartsPromise === null) {
            this.loadBrickPartRelationshipChildBrickParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _brickPartRelationshipChildBrickPartsCount$: Observable<bigint | number> | null = null;
    public get BrickPartRelationshipChildBrickPartsCount$(): Observable<bigint | number> {
        if (this._brickPartRelationshipChildBrickPartsCount$ === null) {
            this._brickPartRelationshipChildBrickPartsCount$ = BrickPartRelationshipService.Instance.GetBrickPartRelationshipsRowCount({childBrickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._brickPartRelationshipChildBrickPartsCount$;
    }


    public BrickPartRelationshipParentBrickParts$ = this._brickPartRelationshipParentBrickPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickPartRelationshipParentBrickParts === null && this._brickPartRelationshipParentBrickPartsPromise === null) {
            this.loadBrickPartRelationshipParentBrickParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _brickPartRelationshipParentBrickPartsCount$: Observable<bigint | number> | null = null;
    public get BrickPartRelationshipParentBrickPartsCount$(): Observable<bigint | number> {
        if (this._brickPartRelationshipParentBrickPartsCount$ === null) {
            this._brickPartRelationshipParentBrickPartsCount$ = BrickPartRelationshipService.Instance.GetBrickPartRelationshipsRowCount({parentBrickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._brickPartRelationshipParentBrickPartsCount$;
    }


    public BrickElements$ = this._brickElementsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._brickElements === null && this._brickElementsPromise === null) {
            this.loadBrickElements(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _brickElementsCount$: Observable<bigint | number> | null = null;
    public get BrickElementsCount$(): Observable<bigint | number> {
        if (this._brickElementsCount$ === null) {
            this._brickElementsCount$ = BrickElementService.Instance.GetBrickElementsRowCount({brickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._brickElementsCount$;
    }



    public UserCollectionParts$ = this._userCollectionPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userCollectionParts === null && this._userCollectionPartsPromise === null) {
            this.loadUserCollectionParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userCollectionPartsCount$: Observable<bigint | number> | null = null;
    public get UserCollectionPartsCount$(): Observable<bigint | number> {
        if (this._userCollectionPartsCount$ === null) {
            this._userCollectionPartsCount$ = UserCollectionPartService.Instance.GetUserCollectionPartsRowCount({brickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userCollectionPartsCount$;
    }



    public UserWishlistItems$ = this._userWishlistItemsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userWishlistItems === null && this._userWishlistItemsPromise === null) {
            this.loadUserWishlistItems(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userWishlistItemsCount$: Observable<bigint | number> | null = null;
    public get UserWishlistItemsCount$(): Observable<bigint | number> {
        if (this._userWishlistItemsCount$ === null) {
            this._userWishlistItemsCount$ = UserWishlistItemService.Instance.GetUserWishlistItemsRowCount({brickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userWishlistItemsCount$;
    }



    public UserPartListItems$ = this._userPartListItemsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userPartListItems === null && this._userPartListItemsPromise === null) {
            this.loadUserPartListItems(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userPartListItemsCount$: Observable<bigint | number> | null = null;
    public get UserPartListItemsCount$(): Observable<bigint | number> {
        if (this._userPartListItemsCount$ === null) {
            this._userPartListItemsCount$ = UserPartListItemService.Instance.GetUserPartListItemsRowCount({brickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userPartListItemsCount$;
    }



    public UserLostParts$ = this._userLostPartsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._userLostParts === null && this._userLostPartsPromise === null) {
            this.loadUserLostParts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _userLostPartsCount$: Observable<bigint | number> | null = null;
    public get UserLostPartsCount$(): Observable<bigint | number> {
        if (this._userLostPartsCount$ === null) {
            this._userLostPartsCount$ = UserLostPartService.Instance.GetUserLostPartsRowCount({brickPartId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._userLostPartsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any BrickPartData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.brickPart.Reload();
  //
  //  Non Async:
  //
  //     brickPart[0].Reload().then(x => {
  //        this.brickPart = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      BrickPartService.Instance.GetBrickPart(this.id, includeRelations)
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
     this._brickPartChangeHistories = null;
     this._brickPartChangeHistoriesPromise = null;
     this._brickPartChangeHistoriesSubject.next(null);
     this._brickPartChangeHistoriesCount$ = null;

     this._brickPartConnectors = null;
     this._brickPartConnectorsPromise = null;
     this._brickPartConnectorsSubject.next(null);
     this._brickPartConnectorsCount$ = null;

     this._brickPartColours = null;
     this._brickPartColoursPromise = null;
     this._brickPartColoursSubject.next(null);
     this._brickPartColoursCount$ = null;

     this._partSubFileReferenceParentBrickParts = null;
     this._partSubFileReferenceParentBrickPartsPromise = null;
     this._partSubFileReferenceParentBrickPartsSubject.next(null);
     this._partSubFileReferenceParentBrickPartsCount$ = null;

     this._partSubFileReferenceReferencedBrickParts = null;
     this._partSubFileReferenceReferencedBrickPartsPromise = null;
     this._partSubFileReferenceReferencedBrickPartsSubject.next(null);
     this._partSubFileReferenceReferencedBrickPartsCount$ = null;

     this._placedBricks = null;
     this._placedBricksPromise = null;
     this._placedBricksSubject.next(null);
     this._placedBricksCount$ = null;

     this._modelStepParts = null;
     this._modelStepPartsPromise = null;
     this._modelStepPartsSubject.next(null);
     this._modelStepPartsCount$ = null;

     this._legoSetParts = null;
     this._legoSetPartsPromise = null;
     this._legoSetPartsSubject.next(null);
     this._legoSetPartsCount$ = null;

     this._brickPartRelationshipChildBrickParts = null;
     this._brickPartRelationshipChildBrickPartsPromise = null;
     this._brickPartRelationshipChildBrickPartsSubject.next(null);
     this._brickPartRelationshipChildBrickPartsCount$ = null;

     this._brickPartRelationshipParentBrickParts = null;
     this._brickPartRelationshipParentBrickPartsPromise = null;
     this._brickPartRelationshipParentBrickPartsSubject.next(null);
     this._brickPartRelationshipParentBrickPartsCount$ = null;

     this._brickElements = null;
     this._brickElementsPromise = null;
     this._brickElementsSubject.next(null);
     this._brickElementsCount$ = null;

     this._userCollectionParts = null;
     this._userCollectionPartsPromise = null;
     this._userCollectionPartsSubject.next(null);
     this._userCollectionPartsCount$ = null;

     this._userWishlistItems = null;
     this._userWishlistItemsPromise = null;
     this._userWishlistItemsSubject.next(null);
     this._userWishlistItemsCount$ = null;

     this._userPartListItems = null;
     this._userPartListItemsPromise = null;
     this._userPartListItemsSubject.next(null);
     this._userPartListItemsCount$ = null;

     this._userLostParts = null;
     this._userLostPartsPromise = null;
     this._userLostPartsSubject.next(null);
     this._userLostPartsCount$ = null;

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
     * Gets the BrickPartChangeHistories for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.BrickPartChangeHistories.then(brickParts => { ... })
     *   or
     *   await this.brickPart.brickParts
     *
    */
    public get BrickPartChangeHistories(): Promise<BrickPartChangeHistoryData[]> {
        if (this._brickPartChangeHistories !== null) {
            return Promise.resolve(this._brickPartChangeHistories);
        }

        if (this._brickPartChangeHistoriesPromise !== null) {
            return this._brickPartChangeHistoriesPromise;
        }

        // Start the load
        this.loadBrickPartChangeHistories();

        return this._brickPartChangeHistoriesPromise!;
    }



    private loadBrickPartChangeHistories(): void {

        this._brickPartChangeHistoriesPromise = lastValueFrom(
            BrickPartService.Instance.GetBrickPartChangeHistoriesForBrickPart(this.id)
        )
        .then(BrickPartChangeHistories => {
            this._brickPartChangeHistories = BrickPartChangeHistories ?? [];
            this._brickPartChangeHistoriesSubject.next(this._brickPartChangeHistories);
            return this._brickPartChangeHistories;
         })
        .catch(err => {
            this._brickPartChangeHistories = [];
            this._brickPartChangeHistoriesSubject.next(this._brickPartChangeHistories);
            throw err;
        })
        .finally(() => {
            this._brickPartChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BrickPartChangeHistory. Call after mutations to force refresh.
     */
    public ClearBrickPartChangeHistoriesCache(): void {
        this._brickPartChangeHistories = null;
        this._brickPartChangeHistoriesPromise = null;
        this._brickPartChangeHistoriesSubject.next(this._brickPartChangeHistories);      // Emit to observable
    }

    public get HasBrickPartChangeHistories(): Promise<boolean> {
        return this.BrickPartChangeHistories.then(brickPartChangeHistories => brickPartChangeHistories.length > 0);
    }


    /**
     *
     * Gets the BrickPartConnectors for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.BrickPartConnectors.then(brickParts => { ... })
     *   or
     *   await this.brickPart.brickParts
     *
    */
    public get BrickPartConnectors(): Promise<BrickPartConnectorData[]> {
        if (this._brickPartConnectors !== null) {
            return Promise.resolve(this._brickPartConnectors);
        }

        if (this._brickPartConnectorsPromise !== null) {
            return this._brickPartConnectorsPromise;
        }

        // Start the load
        this.loadBrickPartConnectors();

        return this._brickPartConnectorsPromise!;
    }



    private loadBrickPartConnectors(): void {

        this._brickPartConnectorsPromise = lastValueFrom(
            BrickPartService.Instance.GetBrickPartConnectorsForBrickPart(this.id)
        )
        .then(BrickPartConnectors => {
            this._brickPartConnectors = BrickPartConnectors ?? [];
            this._brickPartConnectorsSubject.next(this._brickPartConnectors);
            return this._brickPartConnectors;
         })
        .catch(err => {
            this._brickPartConnectors = [];
            this._brickPartConnectorsSubject.next(this._brickPartConnectors);
            throw err;
        })
        .finally(() => {
            this._brickPartConnectorsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BrickPartConnector. Call after mutations to force refresh.
     */
    public ClearBrickPartConnectorsCache(): void {
        this._brickPartConnectors = null;
        this._brickPartConnectorsPromise = null;
        this._brickPartConnectorsSubject.next(this._brickPartConnectors);      // Emit to observable
    }

    public get HasBrickPartConnectors(): Promise<boolean> {
        return this.BrickPartConnectors.then(brickPartConnectors => brickPartConnectors.length > 0);
    }


    /**
     *
     * Gets the BrickPartColours for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.BrickPartColours.then(brickParts => { ... })
     *   or
     *   await this.brickPart.brickParts
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
            BrickPartService.Instance.GetBrickPartColoursForBrickPart(this.id)
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
     * Gets the PartSubFileReferenceParentBrickParts for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.PartSubFileReferenceParentBrickParts.then(parentBrickParts => { ... })
     *   or
     *   await this.brickPart.parentBrickParts
     *
    */
    public get PartSubFileReferenceParentBrickParts(): Promise<PartSubFileReferenceData[]> {
        if (this._partSubFileReferenceParentBrickParts !== null) {
            return Promise.resolve(this._partSubFileReferenceParentBrickParts);
        }

        if (this._partSubFileReferenceParentBrickPartsPromise !== null) {
            return this._partSubFileReferenceParentBrickPartsPromise;
        }

        // Start the load
        this.loadPartSubFileReferenceParentBrickParts();

        return this._partSubFileReferenceParentBrickPartsPromise!;
    }



    private loadPartSubFileReferenceParentBrickParts(): void {

        this._partSubFileReferenceParentBrickPartsPromise = lastValueFrom(
            BrickPartService.Instance.GetPartSubFileReferenceParentBrickPartsForBrickPart(this.id)
        )
        .then(PartSubFileReferenceParentBrickParts => {
            this._partSubFileReferenceParentBrickParts = PartSubFileReferenceParentBrickParts ?? [];
            this._partSubFileReferenceParentBrickPartsSubject.next(this._partSubFileReferenceParentBrickParts);
            return this._partSubFileReferenceParentBrickParts;
         })
        .catch(err => {
            this._partSubFileReferenceParentBrickParts = [];
            this._partSubFileReferenceParentBrickPartsSubject.next(this._partSubFileReferenceParentBrickParts);
            throw err;
        })
        .finally(() => {
            this._partSubFileReferenceParentBrickPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PartSubFileReferenceParentBrickPart. Call after mutations to force refresh.
     */
    public ClearPartSubFileReferenceParentBrickPartsCache(): void {
        this._partSubFileReferenceParentBrickParts = null;
        this._partSubFileReferenceParentBrickPartsPromise = null;
        this._partSubFileReferenceParentBrickPartsSubject.next(this._partSubFileReferenceParentBrickParts);      // Emit to observable
    }

    public get HasPartSubFileReferenceParentBrickParts(): Promise<boolean> {
        return this.PartSubFileReferenceParentBrickParts.then(partSubFileReferenceParentBrickParts => partSubFileReferenceParentBrickParts.length > 0);
    }


    /**
     *
     * Gets the PartSubFileReferenceReferencedBrickParts for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.PartSubFileReferenceReferencedBrickParts.then(referencedBrickParts => { ... })
     *   or
     *   await this.brickPart.referencedBrickParts
     *
    */
    public get PartSubFileReferenceReferencedBrickParts(): Promise<PartSubFileReferenceData[]> {
        if (this._partSubFileReferenceReferencedBrickParts !== null) {
            return Promise.resolve(this._partSubFileReferenceReferencedBrickParts);
        }

        if (this._partSubFileReferenceReferencedBrickPartsPromise !== null) {
            return this._partSubFileReferenceReferencedBrickPartsPromise;
        }

        // Start the load
        this.loadPartSubFileReferenceReferencedBrickParts();

        return this._partSubFileReferenceReferencedBrickPartsPromise!;
    }



    private loadPartSubFileReferenceReferencedBrickParts(): void {

        this._partSubFileReferenceReferencedBrickPartsPromise = lastValueFrom(
            BrickPartService.Instance.GetPartSubFileReferenceReferencedBrickPartsForBrickPart(this.id)
        )
        .then(PartSubFileReferenceReferencedBrickParts => {
            this._partSubFileReferenceReferencedBrickParts = PartSubFileReferenceReferencedBrickParts ?? [];
            this._partSubFileReferenceReferencedBrickPartsSubject.next(this._partSubFileReferenceReferencedBrickParts);
            return this._partSubFileReferenceReferencedBrickParts;
         })
        .catch(err => {
            this._partSubFileReferenceReferencedBrickParts = [];
            this._partSubFileReferenceReferencedBrickPartsSubject.next(this._partSubFileReferenceReferencedBrickParts);
            throw err;
        })
        .finally(() => {
            this._partSubFileReferenceReferencedBrickPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached PartSubFileReferenceReferencedBrickPart. Call after mutations to force refresh.
     */
    public ClearPartSubFileReferenceReferencedBrickPartsCache(): void {
        this._partSubFileReferenceReferencedBrickParts = null;
        this._partSubFileReferenceReferencedBrickPartsPromise = null;
        this._partSubFileReferenceReferencedBrickPartsSubject.next(this._partSubFileReferenceReferencedBrickParts);      // Emit to observable
    }

    public get HasPartSubFileReferenceReferencedBrickParts(): Promise<boolean> {
        return this.PartSubFileReferenceReferencedBrickParts.then(partSubFileReferenceReferencedBrickParts => partSubFileReferenceReferencedBrickParts.length > 0);
    }


    /**
     *
     * Gets the PlacedBricks for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.PlacedBricks.then(brickParts => { ... })
     *   or
     *   await this.brickPart.brickParts
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
            BrickPartService.Instance.GetPlacedBricksForBrickPart(this.id)
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
     * Gets the ModelStepParts for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.ModelStepParts.then(brickParts => { ... })
     *   or
     *   await this.brickPart.brickParts
     *
    */
    public get ModelStepParts(): Promise<ModelStepPartData[]> {
        if (this._modelStepParts !== null) {
            return Promise.resolve(this._modelStepParts);
        }

        if (this._modelStepPartsPromise !== null) {
            return this._modelStepPartsPromise;
        }

        // Start the load
        this.loadModelStepParts();

        return this._modelStepPartsPromise!;
    }



    private loadModelStepParts(): void {

        this._modelStepPartsPromise = lastValueFrom(
            BrickPartService.Instance.GetModelStepPartsForBrickPart(this.id)
        )
        .then(ModelStepParts => {
            this._modelStepParts = ModelStepParts ?? [];
            this._modelStepPartsSubject.next(this._modelStepParts);
            return this._modelStepParts;
         })
        .catch(err => {
            this._modelStepParts = [];
            this._modelStepPartsSubject.next(this._modelStepParts);
            throw err;
        })
        .finally(() => {
            this._modelStepPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ModelStepPart. Call after mutations to force refresh.
     */
    public ClearModelStepPartsCache(): void {
        this._modelStepParts = null;
        this._modelStepPartsPromise = null;
        this._modelStepPartsSubject.next(this._modelStepParts);      // Emit to observable
    }

    public get HasModelStepParts(): Promise<boolean> {
        return this.ModelStepParts.then(modelStepParts => modelStepParts.length > 0);
    }


    /**
     *
     * Gets the LegoSetParts for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.LegoSetParts.then(brickParts => { ... })
     *   or
     *   await this.brickPart.brickParts
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
            BrickPartService.Instance.GetLegoSetPartsForBrickPart(this.id)
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
     * Gets the BrickPartRelationshipChildBrickParts for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.BrickPartRelationshipChildBrickParts.then(childBrickParts => { ... })
     *   or
     *   await this.brickPart.childBrickParts
     *
    */
    public get BrickPartRelationshipChildBrickParts(): Promise<BrickPartRelationshipData[]> {
        if (this._brickPartRelationshipChildBrickParts !== null) {
            return Promise.resolve(this._brickPartRelationshipChildBrickParts);
        }

        if (this._brickPartRelationshipChildBrickPartsPromise !== null) {
            return this._brickPartRelationshipChildBrickPartsPromise;
        }

        // Start the load
        this.loadBrickPartRelationshipChildBrickParts();

        return this._brickPartRelationshipChildBrickPartsPromise!;
    }



    private loadBrickPartRelationshipChildBrickParts(): void {

        this._brickPartRelationshipChildBrickPartsPromise = lastValueFrom(
            BrickPartService.Instance.GetBrickPartRelationshipChildBrickPartsForBrickPart(this.id)
        )
        .then(BrickPartRelationshipChildBrickParts => {
            this._brickPartRelationshipChildBrickParts = BrickPartRelationshipChildBrickParts ?? [];
            this._brickPartRelationshipChildBrickPartsSubject.next(this._brickPartRelationshipChildBrickParts);
            return this._brickPartRelationshipChildBrickParts;
         })
        .catch(err => {
            this._brickPartRelationshipChildBrickParts = [];
            this._brickPartRelationshipChildBrickPartsSubject.next(this._brickPartRelationshipChildBrickParts);
            throw err;
        })
        .finally(() => {
            this._brickPartRelationshipChildBrickPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BrickPartRelationshipChildBrickPart. Call after mutations to force refresh.
     */
    public ClearBrickPartRelationshipChildBrickPartsCache(): void {
        this._brickPartRelationshipChildBrickParts = null;
        this._brickPartRelationshipChildBrickPartsPromise = null;
        this._brickPartRelationshipChildBrickPartsSubject.next(this._brickPartRelationshipChildBrickParts);      // Emit to observable
    }

    public get HasBrickPartRelationshipChildBrickParts(): Promise<boolean> {
        return this.BrickPartRelationshipChildBrickParts.then(brickPartRelationshipChildBrickParts => brickPartRelationshipChildBrickParts.length > 0);
    }


    /**
     *
     * Gets the BrickPartRelationshipParentBrickParts for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.BrickPartRelationshipParentBrickParts.then(parentBrickParts => { ... })
     *   or
     *   await this.brickPart.parentBrickParts
     *
    */
    public get BrickPartRelationshipParentBrickParts(): Promise<BrickPartRelationshipData[]> {
        if (this._brickPartRelationshipParentBrickParts !== null) {
            return Promise.resolve(this._brickPartRelationshipParentBrickParts);
        }

        if (this._brickPartRelationshipParentBrickPartsPromise !== null) {
            return this._brickPartRelationshipParentBrickPartsPromise;
        }

        // Start the load
        this.loadBrickPartRelationshipParentBrickParts();

        return this._brickPartRelationshipParentBrickPartsPromise!;
    }



    private loadBrickPartRelationshipParentBrickParts(): void {

        this._brickPartRelationshipParentBrickPartsPromise = lastValueFrom(
            BrickPartService.Instance.GetBrickPartRelationshipParentBrickPartsForBrickPart(this.id)
        )
        .then(BrickPartRelationshipParentBrickParts => {
            this._brickPartRelationshipParentBrickParts = BrickPartRelationshipParentBrickParts ?? [];
            this._brickPartRelationshipParentBrickPartsSubject.next(this._brickPartRelationshipParentBrickParts);
            return this._brickPartRelationshipParentBrickParts;
         })
        .catch(err => {
            this._brickPartRelationshipParentBrickParts = [];
            this._brickPartRelationshipParentBrickPartsSubject.next(this._brickPartRelationshipParentBrickParts);
            throw err;
        })
        .finally(() => {
            this._brickPartRelationshipParentBrickPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached BrickPartRelationshipParentBrickPart. Call after mutations to force refresh.
     */
    public ClearBrickPartRelationshipParentBrickPartsCache(): void {
        this._brickPartRelationshipParentBrickParts = null;
        this._brickPartRelationshipParentBrickPartsPromise = null;
        this._brickPartRelationshipParentBrickPartsSubject.next(this._brickPartRelationshipParentBrickParts);      // Emit to observable
    }

    public get HasBrickPartRelationshipParentBrickParts(): Promise<boolean> {
        return this.BrickPartRelationshipParentBrickParts.then(brickPartRelationshipParentBrickParts => brickPartRelationshipParentBrickParts.length > 0);
    }


    /**
     *
     * Gets the BrickElements for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.BrickElements.then(brickParts => { ... })
     *   or
     *   await this.brickPart.brickParts
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
            BrickPartService.Instance.GetBrickElementsForBrickPart(this.id)
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
     * Gets the UserCollectionParts for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.UserCollectionParts.then(brickParts => { ... })
     *   or
     *   await this.brickPart.brickParts
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
            BrickPartService.Instance.GetUserCollectionPartsForBrickPart(this.id)
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
     * Gets the UserWishlistItems for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.UserWishlistItems.then(brickParts => { ... })
     *   or
     *   await this.brickPart.brickParts
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
            BrickPartService.Instance.GetUserWishlistItemsForBrickPart(this.id)
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
     *
     * Gets the UserPartListItems for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.UserPartListItems.then(brickParts => { ... })
     *   or
     *   await this.brickPart.brickParts
     *
    */
    public get UserPartListItems(): Promise<UserPartListItemData[]> {
        if (this._userPartListItems !== null) {
            return Promise.resolve(this._userPartListItems);
        }

        if (this._userPartListItemsPromise !== null) {
            return this._userPartListItemsPromise;
        }

        // Start the load
        this.loadUserPartListItems();

        return this._userPartListItemsPromise!;
    }



    private loadUserPartListItems(): void {

        this._userPartListItemsPromise = lastValueFrom(
            BrickPartService.Instance.GetUserPartListItemsForBrickPart(this.id)
        )
        .then(UserPartListItems => {
            this._userPartListItems = UserPartListItems ?? [];
            this._userPartListItemsSubject.next(this._userPartListItems);
            return this._userPartListItems;
         })
        .catch(err => {
            this._userPartListItems = [];
            this._userPartListItemsSubject.next(this._userPartListItems);
            throw err;
        })
        .finally(() => {
            this._userPartListItemsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserPartListItem. Call after mutations to force refresh.
     */
    public ClearUserPartListItemsCache(): void {
        this._userPartListItems = null;
        this._userPartListItemsPromise = null;
        this._userPartListItemsSubject.next(this._userPartListItems);      // Emit to observable
    }

    public get HasUserPartListItems(): Promise<boolean> {
        return this.UserPartListItems.then(userPartListItems => userPartListItems.length > 0);
    }


    /**
     *
     * Gets the UserLostParts for this BrickPart.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.brickPart.UserLostParts.then(brickParts => { ... })
     *   or
     *   await this.brickPart.brickParts
     *
    */
    public get UserLostParts(): Promise<UserLostPartData[]> {
        if (this._userLostParts !== null) {
            return Promise.resolve(this._userLostParts);
        }

        if (this._userLostPartsPromise !== null) {
            return this._userLostPartsPromise;
        }

        // Start the load
        this.loadUserLostParts();

        return this._userLostPartsPromise!;
    }



    private loadUserLostParts(): void {

        this._userLostPartsPromise = lastValueFrom(
            BrickPartService.Instance.GetUserLostPartsForBrickPart(this.id)
        )
        .then(UserLostParts => {
            this._userLostParts = UserLostParts ?? [];
            this._userLostPartsSubject.next(this._userLostParts);
            return this._userLostParts;
         })
        .catch(err => {
            this._userLostParts = [];
            this._userLostPartsSubject.next(this._userLostParts);
            throw err;
        })
        .finally(() => {
            this._userLostPartsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached UserLostPart. Call after mutations to force refresh.
     */
    public ClearUserLostPartsCache(): void {
        this._userLostParts = null;
        this._userLostPartsPromise = null;
        this._userLostPartsSubject.next(this._userLostParts);      // Emit to observable
    }

    public get HasUserLostParts(): Promise<boolean> {
        return this.UserLostParts.then(userLostParts => userLostParts.length > 0);
    }




    //
    // Version History — Lazy-loading observable for current version metadata
    //
    // Usage examples:
    //   Template: {{ (brickPart.CurrentVersionInfo$ | async)?.userName }}
    //   Code:     const info = await brickPart.CurrentVersionInfo;
    //
    public CurrentVersionInfo$ = this._currentVersionInfoSubject.asObservable().pipe(
        tap(() => {
            if (this._currentVersionInfo === null && this._currentVersionInfoPromise === null) {
                this.loadCurrentVersionInfo();
            }
        }),
        shareReplay(1)
    );


    public get CurrentVersionInfo(): Promise<VersionInformation<BrickPartData>> {
        if (this._currentVersionInfoPromise === null) {
            this._currentVersionInfoPromise = this.loadCurrentVersionInfo();
        }
        return this._currentVersionInfoPromise;
    }


    private async loadCurrentVersionInfo(): Promise<VersionInformation<BrickPartData>> {
        const info = await lastValueFrom(
            BrickPartService.Instance.GetBrickPartChangeMetadata(this.id, this.versionNumber as number)
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
     * Updates the state of this BrickPartData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this BrickPartData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): BrickPartSubmitData {
        return BrickPartService.Instance.ConvertToBrickPartSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class BrickPartService extends SecureEndpointBase {

    private static _instance: BrickPartService;
    private listCache: Map<string, Observable<Array<BrickPartData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<BrickPartBasicListData>>>;
    private recordCache: Map<string, Observable<BrickPartData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private brickPartChangeHistoryService: BrickPartChangeHistoryService,
        private brickPartConnectorService: BrickPartConnectorService,
        private brickPartColourService: BrickPartColourService,
        private partSubFileReferenceService: PartSubFileReferenceService,
        private placedBrickService: PlacedBrickService,
        private modelStepPartService: ModelStepPartService,
        private legoSetPartService: LegoSetPartService,
        private brickPartRelationshipService: BrickPartRelationshipService,
        private brickElementService: BrickElementService,
        private userCollectionPartService: UserCollectionPartService,
        private userWishlistItemService: UserWishlistItemService,
        private userPartListItemService: UserPartListItemService,
        private userLostPartService: UserLostPartService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<BrickPartData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<BrickPartBasicListData>>>();
        this.recordCache = new Map<string, Observable<BrickPartData>>();

        BrickPartService._instance = this;
    }

    public static get Instance(): BrickPartService {
      return BrickPartService._instance;
    }


    public ClearListCaches(config: BrickPartQueryParameters | null = null) {

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


    public ConvertToBrickPartSubmitData(data: BrickPartData): BrickPartSubmitData {

        let output = new BrickPartSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.rebrickablePartNum = data.rebrickablePartNum;
        output.rebrickablePartUrl = data.rebrickablePartUrl;
        output.rebrickableImgUrl = data.rebrickableImgUrl;
        output.ldrawPartId = data.ldrawPartId;
        output.bricklinkId = data.bricklinkId;
        output.brickowlId = data.brickowlId;
        output.legoDesignId = data.legoDesignId;
        output.ldrawTitle = data.ldrawTitle;
        output.ldrawCategory = data.ldrawCategory;
        output.partTypeId = data.partTypeId;
        output.keywords = data.keywords;
        output.author = data.author;
        output.brickCategoryId = data.brickCategoryId;
        output.widthLdu = data.widthLdu;
        output.heightLdu = data.heightLdu;
        output.depthLdu = data.depthLdu;
        output.massGrams = data.massGrams;
        output.momentOfInertiaX = data.momentOfInertiaX;
        output.momentOfInertiaY = data.momentOfInertiaY;
        output.momentOfInertiaZ = data.momentOfInertiaZ;
        output.frictionCoefficient = data.frictionCoefficient;
        output.materialType = data.materialType;
        output.centerOfMassX = data.centerOfMassX;
        output.centerOfMassY = data.centerOfMassY;
        output.centerOfMassZ = data.centerOfMassZ;
        output.geometryFileName = data.geometryFileName;
        output.geometrySize = data.geometrySize;
        output.geometryData = data.geometryData;
        output.geometryMimeType = data.geometryMimeType;
        output.geometryFileFormat = data.geometryFileFormat;
        output.geometryOriginalFileName = data.geometryOriginalFileName;
        output.boundingBoxMinX = data.boundingBoxMinX;
        output.boundingBoxMinY = data.boundingBoxMinY;
        output.boundingBoxMinZ = data.boundingBoxMinZ;
        output.boundingBoxMaxX = data.boundingBoxMaxX;
        output.boundingBoxMaxY = data.boundingBoxMaxY;
        output.boundingBoxMaxZ = data.boundingBoxMaxZ;
        output.subFileCount = data.subFileCount;
        output.polygonCount = data.polygonCount;
        output.toothCount = data.toothCount;
        output.gearRatio = data.gearRatio;
        output.lastModifiedDate = data.lastModifiedDate;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetBrickPart(id: bigint | number, includeRelations: boolean = true) : Observable<BrickPartData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const brickPart$ = this.requestBrickPart(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickPart", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, brickPart$);

            return brickPart$;
        }

        return this.recordCache.get(configHash) as Observable<BrickPartData>;
    }

    private requestBrickPart(id: bigint | number, includeRelations: boolean = true) : Observable<BrickPartData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickPartData>(this.baseUrl + 'api/BrickPart/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveBrickPart(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickPart(id, includeRelations));
            }));
    }

    public GetBrickPartList(config: BrickPartQueryParameters | any = null) : Observable<Array<BrickPartData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const brickPartList$ = this.requestBrickPartList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickPart list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, brickPartList$);

            return brickPartList$;
        }

        return this.listCache.get(configHash) as Observable<Array<BrickPartData>>;
    }


    private requestBrickPartList(config: BrickPartQueryParameters | any) : Observable <Array<BrickPartData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickPartData>>(this.baseUrl + 'api/BrickParts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveBrickPartList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestBrickPartList(config));
            }));
    }

    public GetBrickPartsRowCount(config: BrickPartQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const brickPartsRowCount$ = this.requestBrickPartsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get BrickParts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, brickPartsRowCount$);

            return brickPartsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestBrickPartsRowCount(config: BrickPartQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/BrickParts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickPartsRowCount(config));
            }));
    }

    public GetBrickPartsBasicListData(config: BrickPartQueryParameters | any = null) : Observable<Array<BrickPartBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const brickPartsBasicListData$ = this.requestBrickPartsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get BrickParts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, brickPartsBasicListData$);

            return brickPartsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<BrickPartBasicListData>>;
    }


    private requestBrickPartsBasicListData(config: BrickPartQueryParameters | any) : Observable<Array<BrickPartBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<BrickPartBasicListData>>(this.baseUrl + 'api/BrickParts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestBrickPartsBasicListData(config));
            }));

    }


    public PutBrickPart(id: bigint | number, brickPart: BrickPartSubmitData) : Observable<BrickPartData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickPartData>(this.baseUrl + 'api/BrickPart/' + id.toString(), brickPart, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickPart(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutBrickPart(id, brickPart));
            }));
    }


    public PostBrickPart(brickPart: BrickPartSubmitData) : Observable<BrickPartData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<BrickPartData>(this.baseUrl + 'api/BrickPart', brickPart, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickPart(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostBrickPart(brickPart));
            }));
    }

  
    public DeleteBrickPart(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/BrickPart/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteBrickPart(id));
            }));
    }

    public RollbackBrickPart(id: bigint | number, versionNumber: bigint | number) : Observable<BrickPartData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<BrickPartData>(this.baseUrl + 'api/BrickPart/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveBrickPart(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackBrickPart(id, versionNumber));
        }));
    }


    /**
     * Gets version metadata for a specific version of a BrickPart.
     */
    public GetBrickPartChangeMetadata(id: bigint | number, versionNumber?: number): Observable<VersionInformation<BrickPartData>> {

        let queryParams = new HttpParams();

        if (versionNumber !== undefined && versionNumber !== null) {
            queryParams = queryParams.append('versionNumber', versionNumber.toString());
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<BrickPartData>>(this.baseUrl + 'api/BrickPart/' + id.toString() + '/ChangeMetadata', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetBrickPartChangeMetadata(id, versionNumber));
            })
        );
    }


    /**
     * Gets the full audit history of a BrickPart.
     */
    public GetBrickPartAuditHistory(id: bigint | number, includeData: boolean = false): Observable<VersionInformation<BrickPartData>[]> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('includeData', includeData.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<VersionInformation<BrickPartData>[]>(this.baseUrl + 'api/BrickPart/' + id.toString() + '/AuditHistory', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.GetBrickPartAuditHistory(id, includeData));
            })
        );
    }


    /**
     * Gets a specific historical version of a BrickPart.
     */
    public GetBrickPartVersion(id: bigint | number, version: number): Observable<BrickPartData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickPartData>(this.baseUrl + 'api/BrickPart/' + id.toString() + '/Version/' + version.toString(), {
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveBrickPart(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetBrickPartVersion(id, version));
            })
        );
    }


    /**
     * Gets the state of a BrickPart at a specific point in time.
     */
    public GetBrickPartStateAtTime(id: bigint | number, time: string): Observable<BrickPartData> {

        let queryParams = new HttpParams();
        queryParams = queryParams.append('time', time);

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<BrickPartData>(this.baseUrl + 'api/BrickPart/' + id.toString() + '/StateAtTime', {
            params: queryParams,
            headers: authenticationHeaders
        }).pipe(
            map(raw => this.ReviveBrickPart(raw)),
            catchError(error => {
                return this.handleError(error, () => this.GetBrickPartStateAtTime(id, time));
            })
        );
    }


    private getConfigHash(config: BrickPartQueryParameters | any): string {

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

    public userIsBMCBrickPartReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsBMCBrickPartReader = this.authService.isBMCReader;

        //
        // Next test to see if the user has a high enough read permission level to read from BMC.BrickParts
        //
        if (userIsBMCBrickPartReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsBMCBrickPartReader = user.readPermission >= 1;
            } else {
                userIsBMCBrickPartReader = false;
            }
        }

        return userIsBMCBrickPartReader;
    }


    public userIsBMCBrickPartWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsBMCBrickPartWriter = this.authService.isBMCReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to BMC.BrickParts
        //
        if (userIsBMCBrickPartWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsBMCBrickPartWriter = user.writePermission >= 50;
          } else {
            userIsBMCBrickPartWriter = false;
          }      
        }

        return userIsBMCBrickPartWriter;
    }

    public GetBrickPartChangeHistoriesForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickPartChangeHistoryData[]> {
        return this.brickPartChangeHistoryService.GetBrickPartChangeHistoryList({
            brickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBrickPartConnectorsForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickPartConnectorData[]> {
        return this.brickPartConnectorService.GetBrickPartConnectorList({
            brickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBrickPartColoursForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickPartColourData[]> {
        return this.brickPartColourService.GetBrickPartColourList({
            brickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPartSubFileReferenceParentBrickPartsForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PartSubFileReferenceData[]> {
        return this.partSubFileReferenceService.GetPartSubFileReferenceList({
            parentBrickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPartSubFileReferenceReferencedBrickPartsForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PartSubFileReferenceData[]> {
        return this.partSubFileReferenceService.GetPartSubFileReferenceList({
            referencedBrickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPlacedBricksForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PlacedBrickData[]> {
        return this.placedBrickService.GetPlacedBrickList({
            brickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetModelStepPartsForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ModelStepPartData[]> {
        return this.modelStepPartService.GetModelStepPartList({
            brickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetLegoSetPartsForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<LegoSetPartData[]> {
        return this.legoSetPartService.GetLegoSetPartList({
            brickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBrickPartRelationshipChildBrickPartsForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickPartRelationshipData[]> {
        return this.brickPartRelationshipService.GetBrickPartRelationshipList({
            childBrickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBrickPartRelationshipParentBrickPartsForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickPartRelationshipData[]> {
        return this.brickPartRelationshipService.GetBrickPartRelationshipList({
            parentBrickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetBrickElementsForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<BrickElementData[]> {
        return this.brickElementService.GetBrickElementList({
            brickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserCollectionPartsForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserCollectionPartData[]> {
        return this.userCollectionPartService.GetUserCollectionPartList({
            brickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserWishlistItemsForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserWishlistItemData[]> {
        return this.userWishlistItemService.GetUserWishlistItemList({
            brickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserPartListItemsForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserPartListItemData[]> {
        return this.userPartListItemService.GetUserPartListItemList({
            brickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetUserLostPartsForBrickPart(brickPartId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<UserLostPartData[]> {
        return this.userLostPartService.GetUserLostPartList({
            brickPartId: brickPartId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full BrickPartData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the BrickPartData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when BrickPartTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveBrickPart(raw: any): BrickPartData {
    if (!raw) return raw;

    //
    // Create a BrickPartData object instance with correct prototype
    //
    const revived = Object.create(BrickPartData.prototype) as BrickPartData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._brickPartChangeHistories = null;
    (revived as any)._brickPartChangeHistoriesPromise = null;
    (revived as any)._brickPartChangeHistoriesSubject = new BehaviorSubject<BrickPartChangeHistoryData[] | null>(null);

    (revived as any)._brickPartConnectors = null;
    (revived as any)._brickPartConnectorsPromise = null;
    (revived as any)._brickPartConnectorsSubject = new BehaviorSubject<BrickPartConnectorData[] | null>(null);

    (revived as any)._brickPartColours = null;
    (revived as any)._brickPartColoursPromise = null;
    (revived as any)._brickPartColoursSubject = new BehaviorSubject<BrickPartColourData[] | null>(null);

    (revived as any)._partSubFileReferenceParentBrickParts = null;
    (revived as any)._partSubFileReferenceParentBrickPartsPromise = null;
    (revived as any)._partSubFileReferenceParentBrickPartsSubject = new BehaviorSubject<PartSubFileReferenceData[] | null>(null);

    (revived as any)._partSubFileReferenceReferencedBrickParts = null;
    (revived as any)._partSubFileReferenceReferencedBrickPartsPromise = null;
    (revived as any)._partSubFileReferenceReferencedBrickPartsSubject = new BehaviorSubject<PartSubFileReferenceData[] | null>(null);

    (revived as any)._placedBricks = null;
    (revived as any)._placedBricksPromise = null;
    (revived as any)._placedBricksSubject = new BehaviorSubject<PlacedBrickData[] | null>(null);

    (revived as any)._modelStepParts = null;
    (revived as any)._modelStepPartsPromise = null;
    (revived as any)._modelStepPartsSubject = new BehaviorSubject<ModelStepPartData[] | null>(null);

    (revived as any)._legoSetParts = null;
    (revived as any)._legoSetPartsPromise = null;
    (revived as any)._legoSetPartsSubject = new BehaviorSubject<LegoSetPartData[] | null>(null);

    (revived as any)._brickPartRelationshipChildBrickParts = null;
    (revived as any)._brickPartRelationshipChildBrickPartsPromise = null;
    (revived as any)._brickPartRelationshipChildBrickPartsSubject = new BehaviorSubject<BrickPartRelationshipData[] | null>(null);

    (revived as any)._brickPartRelationshipParentBrickParts = null;
    (revived as any)._brickPartRelationshipParentBrickPartsPromise = null;
    (revived as any)._brickPartRelationshipParentBrickPartsSubject = new BehaviorSubject<BrickPartRelationshipData[] | null>(null);

    (revived as any)._brickElements = null;
    (revived as any)._brickElementsPromise = null;
    (revived as any)._brickElementsSubject = new BehaviorSubject<BrickElementData[] | null>(null);

    (revived as any)._userCollectionParts = null;
    (revived as any)._userCollectionPartsPromise = null;
    (revived as any)._userCollectionPartsSubject = new BehaviorSubject<UserCollectionPartData[] | null>(null);

    (revived as any)._userWishlistItems = null;
    (revived as any)._userWishlistItemsPromise = null;
    (revived as any)._userWishlistItemsSubject = new BehaviorSubject<UserWishlistItemData[] | null>(null);

    (revived as any)._userPartListItems = null;
    (revived as any)._userPartListItemsPromise = null;
    (revived as any)._userPartListItemsSubject = new BehaviorSubject<UserPartListItemData[] | null>(null);

    (revived as any)._userLostParts = null;
    (revived as any)._userLostPartsPromise = null;
    (revived as any)._userLostPartsSubject = new BehaviorSubject<UserLostPartData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadBrickPartXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).BrickPartChangeHistories$ = (revived as any)._brickPartChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickPartChangeHistories === null && (revived as any)._brickPartChangeHistoriesPromise === null) {
                (revived as any).loadBrickPartChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._brickPartChangeHistoriesCount$ = null;


    (revived as any).BrickPartConnectors$ = (revived as any)._brickPartConnectorsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickPartConnectors === null && (revived as any)._brickPartConnectorsPromise === null) {
                (revived as any).loadBrickPartConnectors();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._brickPartConnectorsCount$ = null;


    (revived as any).BrickPartColours$ = (revived as any)._brickPartColoursSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickPartColours === null && (revived as any)._brickPartColoursPromise === null) {
                (revived as any).loadBrickPartColours();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._brickPartColoursCount$ = null;


    (revived as any).PartSubFileReferenceParentBrickParts$ = (revived as any)._partSubFileReferenceParentBrickPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._partSubFileReferenceParentBrickParts === null && (revived as any)._partSubFileReferenceParentBrickPartsPromise === null) {
                (revived as any).loadPartSubFileReferenceParentBrickParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._partSubFileReferenceParentBrickPartsCount$ = null;


    (revived as any).PartSubFileReferenceReferencedBrickParts$ = (revived as any)._partSubFileReferenceReferencedBrickPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._partSubFileReferenceReferencedBrickParts === null && (revived as any)._partSubFileReferenceReferencedBrickPartsPromise === null) {
                (revived as any).loadPartSubFileReferenceReferencedBrickParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._partSubFileReferenceReferencedBrickPartsCount$ = null;


    (revived as any).PlacedBricks$ = (revived as any)._placedBricksSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._placedBricks === null && (revived as any)._placedBricksPromise === null) {
                (revived as any).loadPlacedBricks();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._placedBricksCount$ = null;


    (revived as any).ModelStepParts$ = (revived as any)._modelStepPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._modelStepParts === null && (revived as any)._modelStepPartsPromise === null) {
                (revived as any).loadModelStepParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._modelStepPartsCount$ = null;


    (revived as any).LegoSetParts$ = (revived as any)._legoSetPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._legoSetParts === null && (revived as any)._legoSetPartsPromise === null) {
                (revived as any).loadLegoSetParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._legoSetPartsCount$ = null;


    (revived as any).BrickPartRelationshipChildBrickParts$ = (revived as any)._brickPartRelationshipChildBrickPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickPartRelationshipChildBrickParts === null && (revived as any)._brickPartRelationshipChildBrickPartsPromise === null) {
                (revived as any).loadBrickPartRelationshipChildBrickParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._brickPartRelationshipChildBrickPartsCount$ = null;


    (revived as any).BrickPartRelationshipParentBrickParts$ = (revived as any)._brickPartRelationshipParentBrickPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickPartRelationshipParentBrickParts === null && (revived as any)._brickPartRelationshipParentBrickPartsPromise === null) {
                (revived as any).loadBrickPartRelationshipParentBrickParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._brickPartRelationshipParentBrickPartsCount$ = null;


    (revived as any).BrickElements$ = (revived as any)._brickElementsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._brickElements === null && (revived as any)._brickElementsPromise === null) {
                (revived as any).loadBrickElements();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._brickElementsCount$ = null;


    (revived as any).UserCollectionParts$ = (revived as any)._userCollectionPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userCollectionParts === null && (revived as any)._userCollectionPartsPromise === null) {
                (revived as any).loadUserCollectionParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userCollectionPartsCount$ = null;


    (revived as any).UserWishlistItems$ = (revived as any)._userWishlistItemsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userWishlistItems === null && (revived as any)._userWishlistItemsPromise === null) {
                (revived as any).loadUserWishlistItems();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userWishlistItemsCount$ = null;


    (revived as any).UserPartListItems$ = (revived as any)._userPartListItemsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userPartListItems === null && (revived as any)._userPartListItemsPromise === null) {
                (revived as any).loadUserPartListItems();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userPartListItemsCount$ = null;


    (revived as any).UserLostParts$ = (revived as any)._userLostPartsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._userLostParts === null && (revived as any)._userLostPartsPromise === null) {
                (revived as any).loadUserLostParts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._userLostPartsCount$ = null;



    //
    // Version history metadata cache and observable
    //
    (revived as any)._currentVersionInfo = null;
    (revived as any)._currentVersionInfoPromise = null;
    (revived as any)._currentVersionInfoSubject = new BehaviorSubject<VersionInformation<BrickPartData> | null>(null);

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

  private ReviveBrickPartList(rawList: any[]): BrickPartData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveBrickPart(raw));
  }

}
