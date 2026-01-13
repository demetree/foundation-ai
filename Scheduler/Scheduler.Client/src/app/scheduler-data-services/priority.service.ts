/*

   GENERATED SERVICE FOR THE PRIORITY TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Priority table.

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
import { TagService, TagData } from './tag.service';
import { ScheduledEventTemplateService, ScheduledEventTemplateData } from './scheduled-event-template.service';
import { ScheduledEventService, ScheduledEventData } from './scheduled-event.service';
import { ContactInteractionService, ContactInteractionData } from './contact-interaction.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class PriorityQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
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
export class PrioritySubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    sequence: bigint | number | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class PriorityBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. PriorityChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `priority.PriorityChildren$` — use with `| async` in templates
//        • Promise:    `priority.PriorityChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="priority.PriorityChildren$ | async"`), or
//        • Access the promise getter (`priority.PriorityChildren` or `await priority.PriorityChildren`)
//    - Simply reading `priority.PriorityChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await priority.Reload()` to refresh the entire object and clear all lazy caches.
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
export class PriorityData {
    id!: bigint | number;
    name!: string;
    description!: string;
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
    private _tags: TagData[] | null = null;
    private _tagsPromise: Promise<TagData[]> | null  = null;
    private _tagsSubject = new BehaviorSubject<TagData[] | null>(null);

                
    private _scheduledEventTemplates: ScheduledEventTemplateData[] | null = null;
    private _scheduledEventTemplatesPromise: Promise<ScheduledEventTemplateData[]> | null  = null;
    private _scheduledEventTemplatesSubject = new BehaviorSubject<ScheduledEventTemplateData[] | null>(null);

                
    private _scheduledEvents: ScheduledEventData[] | null = null;
    private _scheduledEventsPromise: Promise<ScheduledEventData[]> | null  = null;
    private _scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

                
    private _contactInteractions: ContactInteractionData[] | null = null;
    private _contactInteractionsPromise: Promise<ContactInteractionData[]> | null  = null;
    private _contactInteractionsSubject = new BehaviorSubject<ContactInteractionData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public Tags$ = this._tagsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._tags === null && this._tagsPromise === null) {
            this.loadTags(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public TagsCount$ = TagService.Instance.GetTagsRowCount({priorityId: this.id,
      active: true,
      deleted: false
    });



    public ScheduledEventTemplates$ = this._scheduledEventTemplatesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventTemplates === null && this._scheduledEventTemplatesPromise === null) {
            this.loadScheduledEventTemplates(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventTemplatesCount$ = ScheduledEventTemplateService.Instance.GetScheduledEventTemplatesRowCount({priorityId: this.id,
      active: true,
      deleted: false
    });



    public ScheduledEvents$ = this._scheduledEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEvents === null && this._scheduledEventsPromise === null) {
            this.loadScheduledEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ScheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({priorityId: this.id,
      active: true,
      deleted: false
    });



    public ContactInteractions$ = this._contactInteractionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactInteractions === null && this._contactInteractionsPromise === null) {
            this.loadContactInteractions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ContactInteractionsCount$ = ContactInteractionService.Instance.GetContactInteractionsRowCount({priorityId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any PriorityData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.priority.Reload();
  //
  //  Non Async:
  //
  //     priority[0].Reload().then(x => {
  //        this.priority = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      PriorityService.Instance.GetPriority(this.id, includeRelations)
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
     this._tags = null;
     this._tagsPromise = null;
     this._tagsSubject.next(null);

     this._scheduledEventTemplates = null;
     this._scheduledEventTemplatesPromise = null;
     this._scheduledEventTemplatesSubject.next(null);

     this._scheduledEvents = null;
     this._scheduledEventsPromise = null;
     this._scheduledEventsSubject.next(null);

     this._contactInteractions = null;
     this._contactInteractionsPromise = null;
     this._contactInteractionsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the Tags for this Priority.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.priority.Tags.then(priorities => { ... })
     *   or
     *   await this.priority.priorities
     *
    */
    public get Tags(): Promise<TagData[]> {
        if (this._tags !== null) {
            return Promise.resolve(this._tags);
        }

        if (this._tagsPromise !== null) {
            return this._tagsPromise;
        }

        // Start the load
        this.loadTags();

        return this._tagsPromise!;
    }



    private loadTags(): void {

        this._tagsPromise = lastValueFrom(
            PriorityService.Instance.GetTagsForPriority(this.id)
        )
        .then(Tags => {
            this._tags = Tags ?? [];
            this._tagsSubject.next(this._tags);
            return this._tags;
         })
        .catch(err => {
            this._tags = [];
            this._tagsSubject.next(this._tags);
            throw err;
        })
        .finally(() => {
            this._tagsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Tag. Call after mutations to force refresh.
     */
    public ClearTagsCache(): void {
        this._tags = null;
        this._tagsPromise = null;
        this._tagsSubject.next(this._tags);      // Emit to observable
    }

    public get HasTags(): Promise<boolean> {
        return this.Tags.then(tags => tags.length > 0);
    }


    /**
     *
     * Gets the ScheduledEventTemplates for this Priority.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.priority.ScheduledEventTemplates.then(priorities => { ... })
     *   or
     *   await this.priority.priorities
     *
    */
    public get ScheduledEventTemplates(): Promise<ScheduledEventTemplateData[]> {
        if (this._scheduledEventTemplates !== null) {
            return Promise.resolve(this._scheduledEventTemplates);
        }

        if (this._scheduledEventTemplatesPromise !== null) {
            return this._scheduledEventTemplatesPromise;
        }

        // Start the load
        this.loadScheduledEventTemplates();

        return this._scheduledEventTemplatesPromise!;
    }



    private loadScheduledEventTemplates(): void {

        this._scheduledEventTemplatesPromise = lastValueFrom(
            PriorityService.Instance.GetScheduledEventTemplatesForPriority(this.id)
        )
        .then(ScheduledEventTemplates => {
            this._scheduledEventTemplates = ScheduledEventTemplates ?? [];
            this._scheduledEventTemplatesSubject.next(this._scheduledEventTemplates);
            return this._scheduledEventTemplates;
         })
        .catch(err => {
            this._scheduledEventTemplates = [];
            this._scheduledEventTemplatesSubject.next(this._scheduledEventTemplates);
            throw err;
        })
        .finally(() => {
            this._scheduledEventTemplatesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventTemplate. Call after mutations to force refresh.
     */
    public ClearScheduledEventTemplatesCache(): void {
        this._scheduledEventTemplates = null;
        this._scheduledEventTemplatesPromise = null;
        this._scheduledEventTemplatesSubject.next(this._scheduledEventTemplates);      // Emit to observable
    }

    public get HasScheduledEventTemplates(): Promise<boolean> {
        return this.ScheduledEventTemplates.then(scheduledEventTemplates => scheduledEventTemplates.length > 0);
    }


    /**
     *
     * Gets the ScheduledEvents for this Priority.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.priority.ScheduledEvents.then(priorities => { ... })
     *   or
     *   await this.priority.priorities
     *
    */
    public get ScheduledEvents(): Promise<ScheduledEventData[]> {
        if (this._scheduledEvents !== null) {
            return Promise.resolve(this._scheduledEvents);
        }

        if (this._scheduledEventsPromise !== null) {
            return this._scheduledEventsPromise;
        }

        // Start the load
        this.loadScheduledEvents();

        return this._scheduledEventsPromise!;
    }



    private loadScheduledEvents(): void {

        this._scheduledEventsPromise = lastValueFrom(
            PriorityService.Instance.GetScheduledEventsForPriority(this.id)
        )
        .then(ScheduledEvents => {
            this._scheduledEvents = ScheduledEvents ?? [];
            this._scheduledEventsSubject.next(this._scheduledEvents);
            return this._scheduledEvents;
         })
        .catch(err => {
            this._scheduledEvents = [];
            this._scheduledEventsSubject.next(this._scheduledEvents);
            throw err;
        })
        .finally(() => {
            this._scheduledEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEvent. Call after mutations to force refresh.
     */
    public ClearScheduledEventsCache(): void {
        this._scheduledEvents = null;
        this._scheduledEventsPromise = null;
        this._scheduledEventsSubject.next(this._scheduledEvents);      // Emit to observable
    }

    public get HasScheduledEvents(): Promise<boolean> {
        return this.ScheduledEvents.then(scheduledEvents => scheduledEvents.length > 0);
    }


    /**
     *
     * Gets the ContactInteractions for this Priority.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.priority.ContactInteractions.then(priorities => { ... })
     *   or
     *   await this.priority.priorities
     *
    */
    public get ContactInteractions(): Promise<ContactInteractionData[]> {
        if (this._contactInteractions !== null) {
            return Promise.resolve(this._contactInteractions);
        }

        if (this._contactInteractionsPromise !== null) {
            return this._contactInteractionsPromise;
        }

        // Start the load
        this.loadContactInteractions();

        return this._contactInteractionsPromise!;
    }



    private loadContactInteractions(): void {

        this._contactInteractionsPromise = lastValueFrom(
            PriorityService.Instance.GetContactInteractionsForPriority(this.id)
        )
        .then(ContactInteractions => {
            this._contactInteractions = ContactInteractions ?? [];
            this._contactInteractionsSubject.next(this._contactInteractions);
            return this._contactInteractions;
         })
        .catch(err => {
            this._contactInteractions = [];
            this._contactInteractionsSubject.next(this._contactInteractions);
            throw err;
        })
        .finally(() => {
            this._contactInteractionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContactInteraction. Call after mutations to force refresh.
     */
    public ClearContactInteractionsCache(): void {
        this._contactInteractions = null;
        this._contactInteractionsPromise = null;
        this._contactInteractionsSubject.next(this._contactInteractions);      // Emit to observable
    }

    public get HasContactInteractions(): Promise<boolean> {
        return this.ContactInteractions.then(contactInteractions => contactInteractions.length > 0);
    }




    /**
     * Updates the state of this PriorityData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this PriorityData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): PrioritySubmitData {
        return PriorityService.Instance.ConvertToPrioritySubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class PriorityService extends SecureEndpointBase {

    private static _instance: PriorityService;
    private listCache: Map<string, Observable<Array<PriorityData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<PriorityBasicListData>>>;
    private recordCache: Map<string, Observable<PriorityData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private tagService: TagService,
        private scheduledEventTemplateService: ScheduledEventTemplateService,
        private scheduledEventService: ScheduledEventService,
        private contactInteractionService: ContactInteractionService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<PriorityData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<PriorityBasicListData>>>();
        this.recordCache = new Map<string, Observable<PriorityData>>();

        PriorityService._instance = this;
    }

    public static get Instance(): PriorityService {
      return PriorityService._instance;
    }


    public ClearListCaches(config: PriorityQueryParameters | null = null) {

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


    public ConvertToPrioritySubmitData(data: PriorityData): PrioritySubmitData {

        let output = new PrioritySubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.sequence = data.sequence;
        output.iconId = data.iconId;
        output.color = data.color;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetPriority(id: bigint | number, includeRelations: boolean = true) : Observable<PriorityData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const priority$ = this.requestPriority(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Priority", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, priority$);

            return priority$;
        }

        return this.recordCache.get(configHash) as Observable<PriorityData>;
    }

    private requestPriority(id: bigint | number, includeRelations: boolean = true) : Observable<PriorityData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<PriorityData>(this.baseUrl + 'api/Priority/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.RevivePriority(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestPriority(id, includeRelations));
            }));
    }

    public GetPriorityList(config: PriorityQueryParameters | any = null) : Observable<Array<PriorityData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const priorityList$ = this.requestPriorityList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Priority list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, priorityList$);

            return priorityList$;
        }

        return this.listCache.get(configHash) as Observable<Array<PriorityData>>;
    }


    private requestPriorityList(config: PriorityQueryParameters | any) : Observable <Array<PriorityData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PriorityData>>(this.baseUrl + 'api/Priorities', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.RevivePriorityList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestPriorityList(config));
            }));
    }

    public GetPrioritiesRowCount(config: PriorityQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const prioritiesRowCount$ = this.requestPrioritiesRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Priorities row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, prioritiesRowCount$);

            return prioritiesRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestPrioritiesRowCount(config: PriorityQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Priorities/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPrioritiesRowCount(config));
            }));
    }

    public GetPrioritiesBasicListData(config: PriorityQueryParameters | any = null) : Observable<Array<PriorityBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const prioritiesBasicListData$ = this.requestPrioritiesBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Priorities basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, prioritiesBasicListData$);

            return prioritiesBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<PriorityBasicListData>>;
    }


    private requestPrioritiesBasicListData(config: PriorityQueryParameters | any) : Observable<Array<PriorityBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<PriorityBasicListData>>(this.baseUrl + 'api/Priorities/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestPrioritiesBasicListData(config));
            }));

    }


    public PutPriority(id: bigint | number, priority: PrioritySubmitData) : Observable<PriorityData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<PriorityData>(this.baseUrl + 'api/Priority/' + id.toString(), priority, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePriority(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutPriority(id, priority));
            }));
    }


    public PostPriority(priority: PrioritySubmitData) : Observable<PriorityData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<PriorityData>(this.baseUrl + 'api/Priority', priority, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.RevivePriority(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostPriority(priority));
            }));
    }

  
    public DeletePriority(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Priority/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeletePriority(id));
            }));
    }


    private getConfigHash(config: PriorityQueryParameters | any): string {

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

    public userIsSchedulerPriorityReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerPriorityReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Priorities
        //
        if (userIsSchedulerPriorityReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerPriorityReader = user.readPermission >= 1;
            } else {
                userIsSchedulerPriorityReader = false;
            }
        }

        return userIsSchedulerPriorityReader;
    }


    public userIsSchedulerPriorityWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerPriorityWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Priorities
        //
        if (userIsSchedulerPriorityWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerPriorityWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerPriorityWriter = false;
          }      
        }

        return userIsSchedulerPriorityWriter;
    }

    public GetTagsForPriority(priorityId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TagData[]> {
        return this.tagService.GetTagList({
            priorityId: priorityId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventTemplatesForPriority(priorityId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventTemplateData[]> {
        return this.scheduledEventTemplateService.GetScheduledEventTemplateList({
            priorityId: priorityId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventsForPriority(priorityId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventData[]> {
        return this.scheduledEventService.GetScheduledEventList({
            priorityId: priorityId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetContactInteractionsForPriority(priorityId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactInteractionData[]> {
        return this.contactInteractionService.GetContactInteractionList({
            priorityId: priorityId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full PriorityData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the PriorityData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when PriorityTags$ etc.
   * are subscribed to in templates.
   *
   */
  public RevivePriority(raw: any): PriorityData {
    if (!raw) return raw;

    //
    // Create a PriorityData object instance with correct prototype
    //
    const revived = Object.create(PriorityData.prototype) as PriorityData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._tags = null;
    (revived as any)._tagsPromise = null;
    (revived as any)._tagsSubject = new BehaviorSubject<TagData[] | null>(null);

    (revived as any)._scheduledEventTemplates = null;
    (revived as any)._scheduledEventTemplatesPromise = null;
    (revived as any)._scheduledEventTemplatesSubject = new BehaviorSubject<ScheduledEventTemplateData[] | null>(null);

    (revived as any)._scheduledEvents = null;
    (revived as any)._scheduledEventsPromise = null;
    (revived as any)._scheduledEventsSubject = new BehaviorSubject<ScheduledEventData[] | null>(null);

    (revived as any)._contactInteractions = null;
    (revived as any)._contactInteractionsPromise = null;
    (revived as any)._contactInteractionsSubject = new BehaviorSubject<ContactInteractionData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadPriorityXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).Tags$ = (revived as any)._tagsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._tags === null && (revived as any)._tagsPromise === null) {
                (revived as any).loadTags();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).TagsCount$ = TagService.Instance.GetTagsRowCount({priorityId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduledEventTemplates$ = (revived as any)._scheduledEventTemplatesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventTemplates === null && (revived as any)._scheduledEventTemplatesPromise === null) {
                (revived as any).loadScheduledEventTemplates();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventTemplatesCount$ = ScheduledEventTemplateService.Instance.GetScheduledEventTemplatesRowCount({priorityId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ScheduledEvents$ = (revived as any)._scheduledEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEvents === null && (revived as any)._scheduledEventsPromise === null) {
                (revived as any).loadScheduledEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ScheduledEventsCount$ = ScheduledEventService.Instance.GetScheduledEventsRowCount({priorityId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ContactInteractions$ = (revived as any)._contactInteractionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactInteractions === null && (revived as any)._contactInteractionsPromise === null) {
                (revived as any).loadContactInteractions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ContactInteractionsCount$ = ContactInteractionService.Instance.GetContactInteractionsRowCount({priorityId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private RevivePriorityList(rawList: any[]): PriorityData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.RevivePriority(raw));
  }

}
