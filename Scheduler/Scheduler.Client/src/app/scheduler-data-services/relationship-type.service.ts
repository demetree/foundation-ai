/*

   GENERATED SERVICE FOR THE RELATIONSHIPTYPE TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the RelationshipType table.

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
import { IconData } from './icon.service';
import { ContactContactService, ContactContactData } from './contact-contact.service';
import { OfficeContactService, OfficeContactData } from './office-contact.service';
import { ClientContactService, ClientContactData } from './client-contact.service';
import { SchedulingTargetContactService, SchedulingTargetContactData } from './scheduling-target-contact.service';
import { ResourceContactService, ResourceContactData } from './resource-contact.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class RelationshipTypeQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    isEmergencyEligible: boolean | null | undefined = null;
    sequence: bigint | number | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
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
export class RelationshipTypeSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isEmergencyEligible!: boolean;
    sequence: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class RelationshipTypeBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. RelationshipTypeChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `relationshipType.RelationshipTypeChildren$` — use with `| async` in templates
//        • Promise:    `relationshipType.RelationshipTypeChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="relationshipType.RelationshipTypeChildren$ | async"`), or
//        • Access the promise getter (`relationshipType.RelationshipTypeChildren` or `await relationshipType.RelationshipTypeChildren`)
//    - Simply reading `relationshipType.RelationshipTypeChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await relationshipType.Reload()` to refresh the entire object and clear all lazy caches.
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
export class RelationshipTypeData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isEmergencyEligible!: boolean;
    sequence!: bigint | number;
    iconId!: bigint | number;
    color!: string | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _contactContacts: ContactContactData[] | null = null;
    private _contactContactsPromise: Promise<ContactContactData[]> | null  = null;
    private _contactContactsSubject = new BehaviorSubject<ContactContactData[] | null>(null);

                
    private _officeContacts: OfficeContactData[] | null = null;
    private _officeContactsPromise: Promise<OfficeContactData[]> | null  = null;
    private _officeContactsSubject = new BehaviorSubject<OfficeContactData[] | null>(null);

                
    private _clientContacts: ClientContactData[] | null = null;
    private _clientContactsPromise: Promise<ClientContactData[]> | null  = null;
    private _clientContactsSubject = new BehaviorSubject<ClientContactData[] | null>(null);

                
    private _schedulingTargetContacts: SchedulingTargetContactData[] | null = null;
    private _schedulingTargetContactsPromise: Promise<SchedulingTargetContactData[]> | null  = null;
    private _schedulingTargetContactsSubject = new BehaviorSubject<SchedulingTargetContactData[] | null>(null);

                
    private _resourceContacts: ResourceContactData[] | null = null;
    private _resourceContactsPromise: Promise<ResourceContactData[]> | null  = null;
    private _resourceContactsSubject = new BehaviorSubject<ResourceContactData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ContactContacts$ = this._contactContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactContacts === null && this._contactContactsPromise === null) {
            this.loadContactContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _contactContactsCount$: Observable<bigint | number> | null = null;
    public get ContactContactsCount$(): Observable<bigint | number> {
        if (this._contactContactsCount$ === null) {
            this._contactContactsCount$ = ContactContactService.Instance.GetContactContactsRowCount({relationshipTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._contactContactsCount$;
    }



    public OfficeContacts$ = this._officeContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._officeContacts === null && this._officeContactsPromise === null) {
            this.loadOfficeContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _officeContactsCount$: Observable<bigint | number> | null = null;
    public get OfficeContactsCount$(): Observable<bigint | number> {
        if (this._officeContactsCount$ === null) {
            this._officeContactsCount$ = OfficeContactService.Instance.GetOfficeContactsRowCount({relationshipTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._officeContactsCount$;
    }



    public ClientContacts$ = this._clientContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._clientContacts === null && this._clientContactsPromise === null) {
            this.loadClientContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _clientContactsCount$: Observable<bigint | number> | null = null;
    public get ClientContactsCount$(): Observable<bigint | number> {
        if (this._clientContactsCount$ === null) {
            this._clientContactsCount$ = ClientContactService.Instance.GetClientContactsRowCount({relationshipTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._clientContactsCount$;
    }



    public SchedulingTargetContacts$ = this._schedulingTargetContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetContacts === null && this._schedulingTargetContactsPromise === null) {
            this.loadSchedulingTargetContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _schedulingTargetContactsCount$: Observable<bigint | number> | null = null;
    public get SchedulingTargetContactsCount$(): Observable<bigint | number> {
        if (this._schedulingTargetContactsCount$ === null) {
            this._schedulingTargetContactsCount$ = SchedulingTargetContactService.Instance.GetSchedulingTargetContactsRowCount({relationshipTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._schedulingTargetContactsCount$;
    }



    public ResourceContacts$ = this._resourceContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resourceContacts === null && this._resourceContactsPromise === null) {
            this.loadResourceContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _resourceContactsCount$: Observable<bigint | number> | null = null;
    public get ResourceContactsCount$(): Observable<bigint | number> {
        if (this._resourceContactsCount$ === null) {
            this._resourceContactsCount$ = ResourceContactService.Instance.GetResourceContactsRowCount({relationshipTypeId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._resourceContactsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any RelationshipTypeData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.relationshipType.Reload();
  //
  //  Non Async:
  //
  //     relationshipType[0].Reload().then(x => {
  //        this.relationshipType = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      RelationshipTypeService.Instance.GetRelationshipType(this.id, includeRelations)
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
     this._contactContacts = null;
     this._contactContactsPromise = null;
     this._contactContactsSubject.next(null);
     this._contactContactsCount$ = null;

     this._officeContacts = null;
     this._officeContactsPromise = null;
     this._officeContactsSubject.next(null);
     this._officeContactsCount$ = null;

     this._clientContacts = null;
     this._clientContactsPromise = null;
     this._clientContactsSubject.next(null);
     this._clientContactsCount$ = null;

     this._schedulingTargetContacts = null;
     this._schedulingTargetContactsPromise = null;
     this._schedulingTargetContactsSubject.next(null);
     this._schedulingTargetContactsCount$ = null;

     this._resourceContacts = null;
     this._resourceContactsPromise = null;
     this._resourceContactsSubject.next(null);
     this._resourceContactsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ContactContacts for this RelationshipType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.relationshipType.ContactContacts.then(relationshipTypes => { ... })
     *   or
     *   await this.relationshipType.relationshipTypes
     *
    */
    public get ContactContacts(): Promise<ContactContactData[]> {
        if (this._contactContacts !== null) {
            return Promise.resolve(this._contactContacts);
        }

        if (this._contactContactsPromise !== null) {
            return this._contactContactsPromise;
        }

        // Start the load
        this.loadContactContacts();

        return this._contactContactsPromise!;
    }



    private loadContactContacts(): void {

        this._contactContactsPromise = lastValueFrom(
            RelationshipTypeService.Instance.GetContactContactsForRelationshipType(this.id)
        )
        .then(ContactContacts => {
            this._contactContacts = ContactContacts ?? [];
            this._contactContactsSubject.next(this._contactContacts);
            return this._contactContacts;
         })
        .catch(err => {
            this._contactContacts = [];
            this._contactContactsSubject.next(this._contactContacts);
            throw err;
        })
        .finally(() => {
            this._contactContactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContactContact. Call after mutations to force refresh.
     */
    public ClearContactContactsCache(): void {
        this._contactContacts = null;
        this._contactContactsPromise = null;
        this._contactContactsSubject.next(this._contactContacts);      // Emit to observable
    }

    public get HasContactContacts(): Promise<boolean> {
        return this.ContactContacts.then(contactContacts => contactContacts.length > 0);
    }


    /**
     *
     * Gets the OfficeContacts for this RelationshipType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.relationshipType.OfficeContacts.then(relationshipTypes => { ... })
     *   or
     *   await this.relationshipType.relationshipTypes
     *
    */
    public get OfficeContacts(): Promise<OfficeContactData[]> {
        if (this._officeContacts !== null) {
            return Promise.resolve(this._officeContacts);
        }

        if (this._officeContactsPromise !== null) {
            return this._officeContactsPromise;
        }

        // Start the load
        this.loadOfficeContacts();

        return this._officeContactsPromise!;
    }



    private loadOfficeContacts(): void {

        this._officeContactsPromise = lastValueFrom(
            RelationshipTypeService.Instance.GetOfficeContactsForRelationshipType(this.id)
        )
        .then(OfficeContacts => {
            this._officeContacts = OfficeContacts ?? [];
            this._officeContactsSubject.next(this._officeContacts);
            return this._officeContacts;
         })
        .catch(err => {
            this._officeContacts = [];
            this._officeContactsSubject.next(this._officeContacts);
            throw err;
        })
        .finally(() => {
            this._officeContactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached OfficeContact. Call after mutations to force refresh.
     */
    public ClearOfficeContactsCache(): void {
        this._officeContacts = null;
        this._officeContactsPromise = null;
        this._officeContactsSubject.next(this._officeContacts);      // Emit to observable
    }

    public get HasOfficeContacts(): Promise<boolean> {
        return this.OfficeContacts.then(officeContacts => officeContacts.length > 0);
    }


    /**
     *
     * Gets the ClientContacts for this RelationshipType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.relationshipType.ClientContacts.then(relationshipTypes => { ... })
     *   or
     *   await this.relationshipType.relationshipTypes
     *
    */
    public get ClientContacts(): Promise<ClientContactData[]> {
        if (this._clientContacts !== null) {
            return Promise.resolve(this._clientContacts);
        }

        if (this._clientContactsPromise !== null) {
            return this._clientContactsPromise;
        }

        // Start the load
        this.loadClientContacts();

        return this._clientContactsPromise!;
    }



    private loadClientContacts(): void {

        this._clientContactsPromise = lastValueFrom(
            RelationshipTypeService.Instance.GetClientContactsForRelationshipType(this.id)
        )
        .then(ClientContacts => {
            this._clientContacts = ClientContacts ?? [];
            this._clientContactsSubject.next(this._clientContacts);
            return this._clientContacts;
         })
        .catch(err => {
            this._clientContacts = [];
            this._clientContactsSubject.next(this._clientContacts);
            throw err;
        })
        .finally(() => {
            this._clientContactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ClientContact. Call after mutations to force refresh.
     */
    public ClearClientContactsCache(): void {
        this._clientContacts = null;
        this._clientContactsPromise = null;
        this._clientContactsSubject.next(this._clientContacts);      // Emit to observable
    }

    public get HasClientContacts(): Promise<boolean> {
        return this.ClientContacts.then(clientContacts => clientContacts.length > 0);
    }


    /**
     *
     * Gets the SchedulingTargetContacts for this RelationshipType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.relationshipType.SchedulingTargetContacts.then(relationshipTypes => { ... })
     *   or
     *   await this.relationshipType.relationshipTypes
     *
    */
    public get SchedulingTargetContacts(): Promise<SchedulingTargetContactData[]> {
        if (this._schedulingTargetContacts !== null) {
            return Promise.resolve(this._schedulingTargetContacts);
        }

        if (this._schedulingTargetContactsPromise !== null) {
            return this._schedulingTargetContactsPromise;
        }

        // Start the load
        this.loadSchedulingTargetContacts();

        return this._schedulingTargetContactsPromise!;
    }



    private loadSchedulingTargetContacts(): void {

        this._schedulingTargetContactsPromise = lastValueFrom(
            RelationshipTypeService.Instance.GetSchedulingTargetContactsForRelationshipType(this.id)
        )
        .then(SchedulingTargetContacts => {
            this._schedulingTargetContacts = SchedulingTargetContacts ?? [];
            this._schedulingTargetContactsSubject.next(this._schedulingTargetContacts);
            return this._schedulingTargetContacts;
         })
        .catch(err => {
            this._schedulingTargetContacts = [];
            this._schedulingTargetContactsSubject.next(this._schedulingTargetContacts);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetContactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTargetContact. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetContactsCache(): void {
        this._schedulingTargetContacts = null;
        this._schedulingTargetContactsPromise = null;
        this._schedulingTargetContactsSubject.next(this._schedulingTargetContacts);      // Emit to observable
    }

    public get HasSchedulingTargetContacts(): Promise<boolean> {
        return this.SchedulingTargetContacts.then(schedulingTargetContacts => schedulingTargetContacts.length > 0);
    }


    /**
     *
     * Gets the ResourceContacts for this RelationshipType.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.relationshipType.ResourceContacts.then(relationshipTypes => { ... })
     *   or
     *   await this.relationshipType.relationshipTypes
     *
    */
    public get ResourceContacts(): Promise<ResourceContactData[]> {
        if (this._resourceContacts !== null) {
            return Promise.resolve(this._resourceContacts);
        }

        if (this._resourceContactsPromise !== null) {
            return this._resourceContactsPromise;
        }

        // Start the load
        this.loadResourceContacts();

        return this._resourceContactsPromise!;
    }



    private loadResourceContacts(): void {

        this._resourceContactsPromise = lastValueFrom(
            RelationshipTypeService.Instance.GetResourceContactsForRelationshipType(this.id)
        )
        .then(ResourceContacts => {
            this._resourceContacts = ResourceContacts ?? [];
            this._resourceContactsSubject.next(this._resourceContacts);
            return this._resourceContacts;
         })
        .catch(err => {
            this._resourceContacts = [];
            this._resourceContactsSubject.next(this._resourceContacts);
            throw err;
        })
        .finally(() => {
            this._resourceContactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ResourceContact. Call after mutations to force refresh.
     */
    public ClearResourceContactsCache(): void {
        this._resourceContacts = null;
        this._resourceContactsPromise = null;
        this._resourceContactsSubject.next(this._resourceContacts);      // Emit to observable
    }

    public get HasResourceContacts(): Promise<boolean> {
        return this.ResourceContacts.then(resourceContacts => resourceContacts.length > 0);
    }




    /**
     * Updates the state of this RelationshipTypeData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this RelationshipTypeData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): RelationshipTypeSubmitData {
        return RelationshipTypeService.Instance.ConvertToRelationshipTypeSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class RelationshipTypeService extends SecureEndpointBase {

    private static _instance: RelationshipTypeService;
    private listCache: Map<string, Observable<Array<RelationshipTypeData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<RelationshipTypeBasicListData>>>;
    private recordCache: Map<string, Observable<RelationshipTypeData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private contactContactService: ContactContactService,
        private officeContactService: OfficeContactService,
        private clientContactService: ClientContactService,
        private schedulingTargetContactService: SchedulingTargetContactService,
        private resourceContactService: ResourceContactService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<RelationshipTypeData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<RelationshipTypeBasicListData>>>();
        this.recordCache = new Map<string, Observable<RelationshipTypeData>>();

        RelationshipTypeService._instance = this;
    }

    public static get Instance(): RelationshipTypeService {
      return RelationshipTypeService._instance;
    }


    public ClearListCaches(config: RelationshipTypeQueryParameters | null = null) {

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


    public ConvertToRelationshipTypeSubmitData(data: RelationshipTypeData): RelationshipTypeSubmitData {

        let output = new RelationshipTypeSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.isEmergencyEligible = data.isEmergencyEligible;
        output.sequence = data.sequence;
        output.iconId = data.iconId;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetRelationshipType(id: bigint | number, includeRelations: boolean = true) : Observable<RelationshipTypeData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const relationshipType$ = this.requestRelationshipType(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RelationshipType", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, relationshipType$);

            return relationshipType$;
        }

        return this.recordCache.get(configHash) as Observable<RelationshipTypeData>;
    }

    private requestRelationshipType(id: bigint | number, includeRelations: boolean = true) : Observable<RelationshipTypeData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<RelationshipTypeData>(this.baseUrl + 'api/RelationshipType/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveRelationshipType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestRelationshipType(id, includeRelations));
            }));
    }

    public GetRelationshipTypeList(config: RelationshipTypeQueryParameters | any = null) : Observable<Array<RelationshipTypeData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const relationshipTypeList$ = this.requestRelationshipTypeList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RelationshipType list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, relationshipTypeList$);

            return relationshipTypeList$;
        }

        return this.listCache.get(configHash) as Observable<Array<RelationshipTypeData>>;
    }


    private requestRelationshipTypeList(config: RelationshipTypeQueryParameters | any) : Observable <Array<RelationshipTypeData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RelationshipTypeData>>(this.baseUrl + 'api/RelationshipTypes', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveRelationshipTypeList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestRelationshipTypeList(config));
            }));
    }

    public GetRelationshipTypesRowCount(config: RelationshipTypeQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const relationshipTypesRowCount$ = this.requestRelationshipTypesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get RelationshipTypes row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, relationshipTypesRowCount$);

            return relationshipTypesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestRelationshipTypesRowCount(config: RelationshipTypeQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/RelationshipTypes/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRelationshipTypesRowCount(config));
            }));
    }

    public GetRelationshipTypesBasicListData(config: RelationshipTypeQueryParameters | any = null) : Observable<Array<RelationshipTypeBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const relationshipTypesBasicListData$ = this.requestRelationshipTypesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get RelationshipTypes basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, relationshipTypesBasicListData$);

            return relationshipTypesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<RelationshipTypeBasicListData>>;
    }


    private requestRelationshipTypesBasicListData(config: RelationshipTypeQueryParameters | any) : Observable<Array<RelationshipTypeBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<RelationshipTypeBasicListData>>(this.baseUrl + 'api/RelationshipTypes/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestRelationshipTypesBasicListData(config));
            }));

    }


    public PutRelationshipType(id: bigint | number, relationshipType: RelationshipTypeSubmitData) : Observable<RelationshipTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<RelationshipTypeData>(this.baseUrl + 'api/RelationshipType/' + id.toString(), relationshipType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRelationshipType(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutRelationshipType(id, relationshipType));
            }));
    }


    public PostRelationshipType(relationshipType: RelationshipTypeSubmitData) : Observable<RelationshipTypeData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<RelationshipTypeData>(this.baseUrl + 'api/RelationshipType', relationshipType, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveRelationshipType(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostRelationshipType(relationshipType));
            }));
    }

  
    public DeleteRelationshipType(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/RelationshipType/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteRelationshipType(id));
            }));
    }


    private getConfigHash(config: RelationshipTypeQueryParameters | any): string {

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

    public userIsSchedulerRelationshipTypeReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerRelationshipTypeReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.RelationshipTypes
        //
        if (userIsSchedulerRelationshipTypeReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerRelationshipTypeReader = user.readPermission >= 1;
            } else {
                userIsSchedulerRelationshipTypeReader = false;
            }
        }

        return userIsSchedulerRelationshipTypeReader;
    }


    public userIsSchedulerRelationshipTypeWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerRelationshipTypeWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.RelationshipTypes
        //
        if (userIsSchedulerRelationshipTypeWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerRelationshipTypeWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerRelationshipTypeWriter = false;
          }      
        }

        return userIsSchedulerRelationshipTypeWriter;
    }

    public GetContactContactsForRelationshipType(relationshipTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactContactData[]> {
        return this.contactContactService.GetContactContactList({
            relationshipTypeId: relationshipTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetOfficeContactsForRelationshipType(relationshipTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<OfficeContactData[]> {
        return this.officeContactService.GetOfficeContactList({
            relationshipTypeId: relationshipTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetClientContactsForRelationshipType(relationshipTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ClientContactData[]> {
        return this.clientContactService.GetClientContactList({
            relationshipTypeId: relationshipTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetContactsForRelationshipType(relationshipTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetContactData[]> {
        return this.schedulingTargetContactService.GetSchedulingTargetContactList({
            relationshipTypeId: relationshipTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetResourceContactsForRelationshipType(relationshipTypeId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceContactData[]> {
        return this.resourceContactService.GetResourceContactList({
            relationshipTypeId: relationshipTypeId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full RelationshipTypeData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the RelationshipTypeData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when RelationshipTypeTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveRelationshipType(raw: any): RelationshipTypeData {
    if (!raw) return raw;

    //
    // Create a RelationshipTypeData object instance with correct prototype
    //
    const revived = Object.create(RelationshipTypeData.prototype) as RelationshipTypeData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._contactContacts = null;
    (revived as any)._contactContactsPromise = null;
    (revived as any)._contactContactsSubject = new BehaviorSubject<ContactContactData[] | null>(null);

    (revived as any)._officeContacts = null;
    (revived as any)._officeContactsPromise = null;
    (revived as any)._officeContactsSubject = new BehaviorSubject<OfficeContactData[] | null>(null);

    (revived as any)._clientContacts = null;
    (revived as any)._clientContactsPromise = null;
    (revived as any)._clientContactsSubject = new BehaviorSubject<ClientContactData[] | null>(null);

    (revived as any)._schedulingTargetContacts = null;
    (revived as any)._schedulingTargetContactsPromise = null;
    (revived as any)._schedulingTargetContactsSubject = new BehaviorSubject<SchedulingTargetContactData[] | null>(null);

    (revived as any)._resourceContacts = null;
    (revived as any)._resourceContactsPromise = null;
    (revived as any)._resourceContactsSubject = new BehaviorSubject<ResourceContactData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadRelationshipTypeXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ContactContacts$ = (revived as any)._contactContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactContacts === null && (revived as any)._contactContactsPromise === null) {
                (revived as any).loadContactContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._contactContactsCount$ = null;


    (revived as any).OfficeContacts$ = (revived as any)._officeContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._officeContacts === null && (revived as any)._officeContactsPromise === null) {
                (revived as any).loadOfficeContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._officeContactsCount$ = null;


    (revived as any).ClientContacts$ = (revived as any)._clientContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._clientContacts === null && (revived as any)._clientContactsPromise === null) {
                (revived as any).loadClientContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._clientContactsCount$ = null;


    (revived as any).SchedulingTargetContacts$ = (revived as any)._schedulingTargetContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetContacts === null && (revived as any)._schedulingTargetContactsPromise === null) {
                (revived as any).loadSchedulingTargetContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._schedulingTargetContactsCount$ = null;


    (revived as any).ResourceContacts$ = (revived as any)._resourceContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resourceContacts === null && (revived as any)._resourceContactsPromise === null) {
                (revived as any).loadResourceContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._resourceContactsCount$ = null;



    return revived;
  }

  private ReviveRelationshipTypeList(rawList: any[]): RelationshipTypeData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveRelationshipType(raw));
  }

}
