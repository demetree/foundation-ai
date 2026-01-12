/*

   GENERATED SERVICE FOR THE CONTACT TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Contact table.

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
import { ContactTypeData } from './contact-type.service';
import { SalutationData } from './salutation.service';
import { ContactMethodData } from './contact-method.service';
import { TimeZoneData } from './time-zone.service';
import { IconData } from './icon.service';
import { ContactChangeHistoryService, ContactChangeHistoryData } from './contact-change-history.service';
import { ContactTagService, ContactTagData } from './contact-tag.service';
import { ContactContactService, ContactContactData } from './contact-contact.service';
import { OfficeContactService, OfficeContactData } from './office-contact.service';
import { ClientContactService, ClientContactData } from './client-contact.service';
import { SchedulingTargetContactService, SchedulingTargetContactData } from './scheduling-target-contact.service';
import { ResourceContactService, ResourceContactData } from './resource-contact.service';
import { ContactInteractionService, ContactInteractionData } from './contact-interaction.service';
import { NotificationSubscriptionService, NotificationSubscriptionData } from './notification-subscription.service';
import { ConstituentService, ConstituentData } from './constituent.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class ContactQueryParameters {
    contactTypeId: bigint | number | null | undefined = null;
    firstName: string | null | undefined = null;
    middleName: string | null | undefined = null;
    lastName: string | null | undefined = null;
    salutationId: bigint | number | null | undefined = null;
    title: string | null | undefined = null;
    birthDate: string | null | undefined = null;        // ISO 8601
    company: string | null | undefined = null;
    email: string | null | undefined = null;
    phone: string | null | undefined = null;
    mobile: string | null | undefined = null;
    position: string | null | undefined = null;
    webSite: string | null | undefined = null;
    contactMethodId: bigint | number | null | undefined = null;
    notes: string | null | undefined = null;
    timeZoneId: bigint | number | null | undefined = null;
    attributes: string | null | undefined = null;
    iconId: bigint | number | null | undefined = null;
    color: string | null | undefined = null;
    avatarFileName: string | null | undefined = null;
    avatarSize: bigint | number | null | undefined = null;
    avatarMimeType: string | null | undefined = null;
    externalId: string | null | undefined = null;
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
export class ContactSubmitData {
    id!: bigint | number;
    contactTypeId!: bigint | number;
    firstName!: string;
    middleName: string | null = null;
    lastName!: string;
    salutationId: bigint | number | null = null;
    title: string | null = null;
    birthDate: string | null = null;     // ISO 8601
    company: string | null = null;
    email: string | null = null;
    phone: string | null = null;
    mobile: string | null = null;
    position: string | null = null;
    webSite: string | null = null;
    contactMethodId: bigint | number | null = null;
    notes: string | null = null;
    timeZoneId: bigint | number | null = null;
    attributes: string | null = null;
    iconId: bigint | number | null = null;
    color: string | null = null;
    avatarFileName: string | null = null;
    avatarSize: bigint | number | null = null;
    avatarData: string | null = null;
    avatarMimeType: string | null = null;
    externalId: string | null = null;
    versionNumber!: bigint | number;
    active!: boolean;
    deleted!: boolean;
}


export class ContactBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. ContactChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `contact.ContactChildren$` — use with `| async` in templates
//        • Promise:    `contact.ContactChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="contact.ContactChildren$ | async"`), or
//        • Access the promise getter (`contact.ContactChildren` or `await contact.ContactChildren`)
//    - Simply reading `contact.ContactChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await contact.Reload()` to refresh the entire object and clear all lazy caches.
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
export class ContactData {
    id!: bigint | number;
    contactTypeId!: bigint | number;
    firstName!: string;
    middleName!: string | null;
    lastName!: string;
    salutationId!: bigint | number;
    title!: string | null;
    birthDate!: string | null;   // ISO 8601
    company!: string | null;
    email!: string | null;
    phone!: string | null;
    mobile!: string | null;
    position!: string | null;
    webSite!: string | null;
    contactMethodId!: bigint | number;
    notes!: string | null;
    timeZoneId!: bigint | number;
    attributes!: string | null;
    iconId!: bigint | number;
    color!: string | null;
    avatarFileName!: string | null;
    avatarSize!: bigint | number;
    avatarData!: string | null;
    avatarMimeType!: string | null;
    externalId!: string | null;
    versionNumber!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    contactMethod: ContactMethodData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    contactType: ContactTypeData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    icon: IconData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    salutation: SalutationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    timeZone: TimeZoneData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _contactChangeHistories: ContactChangeHistoryData[] | null = null;
    private _contactChangeHistoriesPromise: Promise<ContactChangeHistoryData[]> | null  = null;
    private _contactChangeHistoriesSubject = new BehaviorSubject<ContactChangeHistoryData[] | null>(null);

    private _contactTags: ContactTagData[] | null = null;
    private _contactTagsPromise: Promise<ContactTagData[]> | null  = null;
    private _contactTagsSubject = new BehaviorSubject<ContactTagData[] | null>(null);

    private _contactContacts: ContactContactData[] | null = null;
    private _contactContactsPromise: Promise<ContactContactData[]> | null  = null;
    private _contactContactsSubject = new BehaviorSubject<ContactContactData[] | null>(null);

    private _relatedContacts: ContactContactData[] | null = null;
    private _relatedContactsPromise: Promise<ContactContactData[]> | null  = null;
    private _relatedContactsSubject = new BehaviorSubject<ContactContactData[] | null>(null);

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

    private _contactInteractions: ContactInteractionData[] | null = null;
    private _contactInteractionsPromise: Promise<ContactInteractionData[]> | null  = null;
    private _contactInteractionsSubject = new BehaviorSubject<ContactInteractionData[] | null>(null);

    private _initiatingContacts: ContactInteractionData[] | null = null;
    private _initiatingContactsPromise: Promise<ContactInteractionData[]> | null  = null;
    private _initiatingContactsSubject = new BehaviorSubject<ContactInteractionData[] | null>(null);

    private _notificationSubscriptions: NotificationSubscriptionData[] | null = null;
    private _notificationSubscriptionsPromise: Promise<NotificationSubscriptionData[]> | null  = null;
    private _notificationSubscriptionsSubject = new BehaviorSubject<NotificationSubscriptionData[] | null>(null);

    private _constituents: ConstituentData[] | null = null;
    private _constituentsPromise: Promise<ConstituentData[]> | null  = null;
    private _constituentsSubject = new BehaviorSubject<ConstituentData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ContactChangeHistories$ = this._contactChangeHistoriesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactChangeHistories === null && this._contactChangeHistoriesPromise === null) {
            this.loadContactChangeHistories(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ContactChangeHistoriesCount$ = ContactService.Instance.GetContactsRowCount({contactId: this.id,
      active: true,
      deleted: false
    });



    public ContactTags$ = this._contactTagsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactTags === null && this._contactTagsPromise === null) {
            this.loadContactTags(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ContactTagsCount$ = ContactService.Instance.GetContactsRowCount({contactId: this.id,
      active: true,
      deleted: false
    });



    public ContactContacts$ = this._contactContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactContacts === null && this._contactContactsPromise === null) {
            this.loadContactContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ContactContactsCount$ = ContactService.Instance.GetContactsRowCount({contactId: this.id,
      active: true,
      deleted: false
    });



    public RelatedContacts$ = this._relatedContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._relatedContacts === null && this._relatedContactsPromise === null) {
            this.loadRelatedContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public RelatedContactsCount$ = ContactService.Instance.GetContactsRowCount({contactId: this.id,
      active: true,
      deleted: false
    });



    public OfficeContacts$ = this._officeContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._officeContacts === null && this._officeContactsPromise === null) {
            this.loadOfficeContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public OfficeContactsCount$ = ContactService.Instance.GetContactsRowCount({contactId: this.id,
      active: true,
      deleted: false
    });



    public ClientContacts$ = this._clientContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._clientContacts === null && this._clientContactsPromise === null) {
            this.loadClientContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ClientContactsCount$ = ContactService.Instance.GetContactsRowCount({contactId: this.id,
      active: true,
      deleted: false
    });



    public SchedulingTargetContacts$ = this._schedulingTargetContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetContacts === null && this._schedulingTargetContactsPromise === null) {
            this.loadSchedulingTargetContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SchedulingTargetContactsCount$ = ContactService.Instance.GetContactsRowCount({contactId: this.id,
      active: true,
      deleted: false
    });



    public ResourceContacts$ = this._resourceContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resourceContacts === null && this._resourceContactsPromise === null) {
            this.loadResourceContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ResourceContactsCount$ = ContactService.Instance.GetContactsRowCount({contactId: this.id,
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

  
    public ContactInteractionsCount$ = ContactService.Instance.GetContactsRowCount({contactId: this.id,
      active: true,
      deleted: false
    });



    public InitiatingContacts$ = this._initiatingContactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._initiatingContacts === null && this._initiatingContactsPromise === null) {
            this.loadInitiatingContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public InitiatingContactsCount$ = ContactService.Instance.GetContactsRowCount({contactId: this.id,
      active: true,
      deleted: false
    });



    public NotificationSubscriptions$ = this._notificationSubscriptionsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._notificationSubscriptions === null && this._notificationSubscriptionsPromise === null) {
            this.loadNotificationSubscriptions(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public NotificationSubscriptionsCount$ = ContactService.Instance.GetContactsRowCount({contactId: this.id,
      active: true,
      deleted: false
    });



    public Constituents$ = this._constituentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._constituents === null && this._constituentsPromise === null) {
            this.loadConstituents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public ConstituentsCount$ = ContactService.Instance.GetContactsRowCount({contactId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any ContactData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.contact.Reload();
  //
  //  Non Async:
  //
  //     contact[0].Reload().then(x => {
  //        this.contact = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      ContactService.Instance.GetContact(this.id, includeRelations)
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
     this._contactChangeHistories = null;
     this._contactChangeHistoriesPromise = null;
     this._contactChangeHistoriesSubject.next(null);

     this._contactTags = null;
     this._contactTagsPromise = null;
     this._contactTagsSubject.next(null);

     this._contactContacts = null;
     this._contactContactsPromise = null;
     this._contactContactsSubject.next(null);

     this._relatedContacts = null;
     this._relatedContactsPromise = null;
     this._relatedContactsSubject.next(null);

     this._officeContacts = null;
     this._officeContactsPromise = null;
     this._officeContactsSubject.next(null);

     this._clientContacts = null;
     this._clientContactsPromise = null;
     this._clientContactsSubject.next(null);

     this._schedulingTargetContacts = null;
     this._schedulingTargetContactsPromise = null;
     this._schedulingTargetContactsSubject.next(null);

     this._resourceContacts = null;
     this._resourceContactsPromise = null;
     this._resourceContactsSubject.next(null);

     this._contactInteractions = null;
     this._contactInteractionsPromise = null;
     this._contactInteractionsSubject.next(null);

     this._initiatingContacts = null;
     this._initiatingContactsPromise = null;
     this._initiatingContactsSubject.next(null);

     this._notificationSubscriptions = null;
     this._notificationSubscriptionsPromise = null;
     this._notificationSubscriptionsSubject.next(null);

     this._constituents = null;
     this._constituentsPromise = null;
     this._constituentsSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ContactChangeHistories for this Contact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contact.ContactChangeHistories.then(contactChangeHistories => { ... })
     *   or
     *   await this.contact.ContactChangeHistories
     *
    */
    public get ContactChangeHistories(): Promise<ContactChangeHistoryData[]> {
        if (this._contactChangeHistories !== null) {
            return Promise.resolve(this._contactChangeHistories);
        }

        if (this._contactChangeHistoriesPromise !== null) {
            return this._contactChangeHistoriesPromise;
        }

        // Start the load
        this.loadContactChangeHistories();

        return this._contactChangeHistoriesPromise!;
    }



    private loadContactChangeHistories(): void {

        this._contactChangeHistoriesPromise = lastValueFrom(
            ContactService.Instance.GetContactChangeHistoriesForContact(this.id)
        )
        .then(contactChangeHistories => {
            this._contactChangeHistories = contactChangeHistories ?? [];
            this._contactChangeHistoriesSubject.next(this._contactChangeHistories);
            return this._contactChangeHistories;
         })
        .catch(err => {
            this._contactChangeHistories = [];
            this._contactChangeHistoriesSubject.next(this._contactChangeHistories);
            throw err;
        })
        .finally(() => {
            this._contactChangeHistoriesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContactChangeHistory. Call after mutations to force refresh.
     */
    public ClearContactChangeHistoriesCache(): void {
        this._contactChangeHistories = null;
        this._contactChangeHistoriesPromise = null;
        this._contactChangeHistoriesSubject.next(this._contactChangeHistories);      // Emit to observable
    }

    public get HasContactChangeHistories(): Promise<boolean> {
        return this.ContactChangeHistories.then(contactChangeHistories => contactChangeHistories.length > 0);
    }


    /**
     *
     * Gets the ContactTags for this Contact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contact.ContactTags.then(contactTags => { ... })
     *   or
     *   await this.contact.ContactTags
     *
    */
    public get ContactTags(): Promise<ContactTagData[]> {
        if (this._contactTags !== null) {
            return Promise.resolve(this._contactTags);
        }

        if (this._contactTagsPromise !== null) {
            return this._contactTagsPromise;
        }

        // Start the load
        this.loadContactTags();

        return this._contactTagsPromise!;
    }



    private loadContactTags(): void {

        this._contactTagsPromise = lastValueFrom(
            ContactService.Instance.GetContactTagsForContact(this.id)
        )
        .then(contactTags => {
            this._contactTags = contactTags ?? [];
            this._contactTagsSubject.next(this._contactTags);
            return this._contactTags;
         })
        .catch(err => {
            this._contactTags = [];
            this._contactTagsSubject.next(this._contactTags);
            throw err;
        })
        .finally(() => {
            this._contactTagsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContactTag. Call after mutations to force refresh.
     */
    public ClearContactTagsCache(): void {
        this._contactTags = null;
        this._contactTagsPromise = null;
        this._contactTagsSubject.next(this._contactTags);      // Emit to observable
    }

    public get HasContactTags(): Promise<boolean> {
        return this.ContactTags.then(contactTags => contactTags.length > 0);
    }


    /**
     *
     * Gets the ContactContacts for this Contact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contact.ContactContacts.then(contactContacts => { ... })
     *   or
     *   await this.contact.ContactContacts
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
            ContactService.Instance.GetContactContactsForContact(this.id)
        )
        .then(contactContacts => {
            this._contactContacts = contactContacts ?? [];
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
     * Gets the relatedContacts for this Contact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contact.relatedContacts.then(relatedContacts => { ... })
     *   or
     *   await this.contact.relatedContacts
     *
    */
    public get relatedContacts(): Promise<ContactContactData[]> {
        if (this._relatedContacts !== null) {
            return Promise.resolve(this._relatedContacts);
        }

        if (this._relatedContactsPromise !== null) {
            return this._relatedContactsPromise;
        }

        // Start the load
        this.loadRelatedContacts();

        return this._relatedContactsPromise!;
    }



    private loadRelatedContacts(): void {

        this._relatedContactsPromise = lastValueFrom(
            ContactService.Instance.GetRelatedContactsForContact(this.id)
        )
        .then(relatedContacts => {
            this._relatedContacts = relatedContacts ?? [];
            this._relatedContactsSubject.next(this._relatedContacts);
            return this._relatedContacts;
         })
        .catch(err => {
            this._relatedContacts = [];
            this._relatedContactsSubject.next(this._relatedContacts);
            throw err;
        })
        .finally(() => {
            this._relatedContactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached relatedContact. Call after mutations to force refresh.
     */
    public ClearRelatedContactsCache(): void {
        this._relatedContacts = null;
        this._relatedContactsPromise = null;
        this._relatedContactsSubject.next(this._relatedContacts);      // Emit to observable
    }

    public get HasRelatedContacts(): Promise<boolean> {
        return this.relatedContacts.then(relatedContacts => relatedContacts.length > 0);
    }


    /**
     *
     * Gets the OfficeContacts for this Contact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contact.OfficeContacts.then(officeContacts => { ... })
     *   or
     *   await this.contact.OfficeContacts
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
            ContactService.Instance.GetOfficeContactsForContact(this.id)
        )
        .then(officeContacts => {
            this._officeContacts = officeContacts ?? [];
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
     * Gets the ClientContacts for this Contact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contact.ClientContacts.then(clientContacts => { ... })
     *   or
     *   await this.contact.ClientContacts
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
            ContactService.Instance.GetClientContactsForContact(this.id)
        )
        .then(clientContacts => {
            this._clientContacts = clientContacts ?? [];
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
     * Gets the SchedulingTargetContacts for this Contact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contact.SchedulingTargetContacts.then(schedulingTargetContacts => { ... })
     *   or
     *   await this.contact.SchedulingTargetContacts
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
            ContactService.Instance.GetSchedulingTargetContactsForContact(this.id)
        )
        .then(schedulingTargetContacts => {
            this._schedulingTargetContacts = schedulingTargetContacts ?? [];
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
     * Gets the ResourceContacts for this Contact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contact.ResourceContacts.then(resourceContacts => { ... })
     *   or
     *   await this.contact.ResourceContacts
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
            ContactService.Instance.GetResourceContactsForContact(this.id)
        )
        .then(resourceContacts => {
            this._resourceContacts = resourceContacts ?? [];
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
     *
     * Gets the ContactInteractions for this Contact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contact.ContactInteractions.then(contactInteractions => { ... })
     *   or
     *   await this.contact.ContactInteractions
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
            ContactService.Instance.GetContactInteractionsForContact(this.id)
        )
        .then(contactInteractions => {
            this._contactInteractions = contactInteractions ?? [];
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
     *
     * Gets the initiatingContacts for this Contact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contact.initiatingContacts.then(initiatingContacts => { ... })
     *   or
     *   await this.contact.initiatingContacts
     *
    */
    public get initiatingContacts(): Promise<ContactInteractionData[]> {
        if (this._initiatingContacts !== null) {
            return Promise.resolve(this._initiatingContacts);
        }

        if (this._initiatingContactsPromise !== null) {
            return this._initiatingContactsPromise;
        }

        // Start the load
        this.loadInitiatingContacts();

        return this._initiatingContactsPromise!;
    }



    private loadInitiatingContacts(): void {

        this._initiatingContactsPromise = lastValueFrom(
            ContactService.Instance.GetInitiatingContactsForContact(this.id)
        )
        .then(initiatingContacts => {
            this._initiatingContacts = initiatingContacts ?? [];
            this._initiatingContactsSubject.next(this._initiatingContacts);
            return this._initiatingContacts;
         })
        .catch(err => {
            this._initiatingContacts = [];
            this._initiatingContactsSubject.next(this._initiatingContacts);
            throw err;
        })
        .finally(() => {
            this._initiatingContactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached initiatingContact. Call after mutations to force refresh.
     */
    public ClearInitiatingContactsCache(): void {
        this._initiatingContacts = null;
        this._initiatingContactsPromise = null;
        this._initiatingContactsSubject.next(this._initiatingContacts);      // Emit to observable
    }

    public get HasInitiatingContacts(): Promise<boolean> {
        return this.initiatingContacts.then(initiatingContacts => initiatingContacts.length > 0);
    }


    /**
     *
     * Gets the NotificationSubscriptions for this Contact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contact.NotificationSubscriptions.then(notificationSubscriptions => { ... })
     *   or
     *   await this.contact.NotificationSubscriptions
     *
    */
    public get NotificationSubscriptions(): Promise<NotificationSubscriptionData[]> {
        if (this._notificationSubscriptions !== null) {
            return Promise.resolve(this._notificationSubscriptions);
        }

        if (this._notificationSubscriptionsPromise !== null) {
            return this._notificationSubscriptionsPromise;
        }

        // Start the load
        this.loadNotificationSubscriptions();

        return this._notificationSubscriptionsPromise!;
    }



    private loadNotificationSubscriptions(): void {

        this._notificationSubscriptionsPromise = lastValueFrom(
            ContactService.Instance.GetNotificationSubscriptionsForContact(this.id)
        )
        .then(notificationSubscriptions => {
            this._notificationSubscriptions = notificationSubscriptions ?? [];
            this._notificationSubscriptionsSubject.next(this._notificationSubscriptions);
            return this._notificationSubscriptions;
         })
        .catch(err => {
            this._notificationSubscriptions = [];
            this._notificationSubscriptionsSubject.next(this._notificationSubscriptions);
            throw err;
        })
        .finally(() => {
            this._notificationSubscriptionsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached NotificationSubscription. Call after mutations to force refresh.
     */
    public ClearNotificationSubscriptionsCache(): void {
        this._notificationSubscriptions = null;
        this._notificationSubscriptionsPromise = null;
        this._notificationSubscriptionsSubject.next(this._notificationSubscriptions);      // Emit to observable
    }

    public get HasNotificationSubscriptions(): Promise<boolean> {
        return this.NotificationSubscriptions.then(notificationSubscriptions => notificationSubscriptions.length > 0);
    }


    /**
     *
     * Gets the Constituents for this Contact.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.contact.Constituents.then(constituents => { ... })
     *   or
     *   await this.contact.Constituents
     *
    */
    public get Constituents(): Promise<ConstituentData[]> {
        if (this._constituents !== null) {
            return Promise.resolve(this._constituents);
        }

        if (this._constituentsPromise !== null) {
            return this._constituentsPromise;
        }

        // Start the load
        this.loadConstituents();

        return this._constituentsPromise!;
    }



    private loadConstituents(): void {

        this._constituentsPromise = lastValueFrom(
            ContactService.Instance.GetConstituentsForContact(this.id)
        )
        .then(constituents => {
            this._constituents = constituents ?? [];
            this._constituentsSubject.next(this._constituents);
            return this._constituents;
         })
        .catch(err => {
            this._constituents = [];
            this._constituentsSubject.next(this._constituents);
            throw err;
        })
        .finally(() => {
            this._constituentsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Constituent. Call after mutations to force refresh.
     */
    public ClearConstituentsCache(): void {
        this._constituents = null;
        this._constituentsPromise = null;
        this._constituentsSubject.next(this._constituents);      // Emit to observable
    }

    public get HasConstituents(): Promise<boolean> {
        return this.Constituents.then(constituents => constituents.length > 0);
    }




    /**
     * Updates the state of this ContactData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this ContactData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): ContactSubmitData {
        return ContactService.Instance.ConvertToContactSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class ContactService extends SecureEndpointBase {

    private static _instance: ContactService;
    private listCache: Map<string, Observable<Array<ContactData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<ContactBasicListData>>>;
    private recordCache: Map<string, Observable<ContactData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private contactChangeHistoryService: ContactChangeHistoryService,
        private contactTagService: ContactTagService,
        private contactContactService: ContactContactService,
        private officeContactService: OfficeContactService,
        private clientContactService: ClientContactService,
        private schedulingTargetContactService: SchedulingTargetContactService,
        private resourceContactService: ResourceContactService,
        private contactInteractionService: ContactInteractionService,
        private notificationSubscriptionService: NotificationSubscriptionService,
        private constituentService: ConstituentService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<ContactData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<ContactBasicListData>>>();
        this.recordCache = new Map<string, Observable<ContactData>>();

        ContactService._instance = this;
    }

    public static get Instance(): ContactService {
      return ContactService._instance;
    }


    public ClearListCaches(config: ContactQueryParameters | null = null) {

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


    public ConvertToContactSubmitData(data: ContactData): ContactSubmitData {

        let output = new ContactSubmitData();

        output.id = data.id;
        output.contactTypeId = data.contactTypeId;
        output.firstName = data.firstName;
        output.middleName = data.middleName;
        output.lastName = data.lastName;
        output.salutationId = data.salutationId;
        output.title = data.title;
        output.birthDate = data.birthDate;
        output.company = data.company;
        output.email = data.email;
        output.phone = data.phone;
        output.mobile = data.mobile;
        output.position = data.position;
        output.webSite = data.webSite;
        output.contactMethodId = data.contactMethodId;
        output.notes = data.notes;
        output.timeZoneId = data.timeZoneId;
        output.attributes = data.attributes;
        output.iconId = data.iconId;
        output.color = data.color;
        output.avatarFileName = data.avatarFileName;
        output.avatarSize = data.avatarSize;
        output.avatarData = data.avatarData;
        output.avatarMimeType = data.avatarMimeType;
        output.externalId = data.externalId;
        output.versionNumber = data.versionNumber;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetContact(id: bigint | number, includeRelations: boolean = true) : Observable<ContactData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const contact$ = this.requestContact(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Contact", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, contact$);

            return contact$;
        }

        return this.recordCache.get(configHash) as Observable<ContactData>;
    }

    private requestContact(id: bigint | number, includeRelations: boolean = true) : Observable<ContactData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<ContactData>(this.baseUrl + 'api/Contact/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestContact(id, includeRelations));
            }));
    }

    public GetContactList(config: ContactQueryParameters | any = null) : Observable<Array<ContactData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const contactList$ = this.requestContactList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Contact list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, contactList$);

            return contactList$;
        }

        return this.listCache.get(configHash) as Observable<Array<ContactData>>;
    }


    private requestContactList(config: ContactQueryParameters | any) : Observable <Array<ContactData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContactData>>(this.baseUrl + 'api/Contacts', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveContactList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestContactList(config));
            }));
    }

    public GetContactsRowCount(config: ContactQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const contactsRowCount$ = this.requestContactsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Contacts row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, contactsRowCount$);

            return contactsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestContactsRowCount(config: ContactQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Contacts/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContactsRowCount(config));
            }));
    }

    public GetContactsBasicListData(config: ContactQueryParameters | any = null) : Observable<Array<ContactBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const contactsBasicListData$ = this.requestContactsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Contacts basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, contactsBasicListData$);

            return contactsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<ContactBasicListData>>;
    }


    private requestContactsBasicListData(config: ContactQueryParameters | any) : Observable<Array<ContactBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<ContactBasicListData>>(this.baseUrl + 'api/Contacts/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestContactsBasicListData(config));
            }));

    }


    public PutContact(id: bigint | number, contact: ContactSubmitData) : Observable<ContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ContactData>(this.baseUrl + 'api/Contact/' + id.toString(), contact, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutContact(id, contact));
            }));
    }


    public PostContact(contact: ContactSubmitData) : Observable<ContactData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<ContactData>(this.baseUrl + 'api/Contact', contact, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContact(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostContact(contact));
            }));
    }

  
    public DeleteContact(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Contact/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteContact(id));
            }));
    }

    public RollbackContact(id: bigint | number, versionNumber: bigint | number) : Observable<ContactData>{

        let queryParams = new HttpParams();

        queryParams = queryParams.append("id", id.toString());
        queryParams = queryParams.append("versionNumber", versionNumber.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<ContactData>(this.baseUrl + 'api/Contact/Rollback/' + id.toString(), null, { params: queryParams, headers: authenticationHeaders }).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveContact(raw)),
            catchError(error => {
                return this.handleError(error, () => this.RollbackContact(id, versionNumber));
        }));
    }

    private getConfigHash(config: ContactQueryParameters | any): string {

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

    public userIsSchedulerContactReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerContactReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Contacts
        //
        if (userIsSchedulerContactReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerContactReader = user.readPermission >= 1;
            } else {
                userIsSchedulerContactReader = false;
            }
        }

        return userIsSchedulerContactReader;
    }


    public userIsSchedulerContactWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerContactWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Contacts
        //
        if (userIsSchedulerContactWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerContactWriter = user.writePermission >= 1;
          } else {
            userIsSchedulerContactWriter = false;
          }      
        }

        return userIsSchedulerContactWriter;
    }

    public GetContactChangeHistoriesForContact(contactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactChangeHistoryData[]> {
        return this.contactChangeHistoryService.GetContactChangeHistoryList({
            contactId: contactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetContactTagsForContact(contactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactTagData[]> {
        return this.contactTagService.GetContactTagList({
            contactId: contactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetContactContactsForContact(contactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactContactData[]> {
        return this.contactContactService.GetContactContactList({
            contactId: contactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetRelatedContactsForContact(contactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactContactData[]> {
        return this.contactContactService.GetContactContactList({
            relatedContactId: contactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetOfficeContactsForContact(contactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<OfficeContactData[]> {
        return this.officeContactService.GetOfficeContactList({
            contactId: contactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetClientContactsForContact(contactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ClientContactData[]> {
        return this.clientContactService.GetClientContactList({
            contactId: contactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetContactsForContact(contactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetContactData[]> {
        return this.schedulingTargetContactService.GetSchedulingTargetContactList({
            contactId: contactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetResourceContactsForContact(contactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceContactData[]> {
        return this.resourceContactService.GetResourceContactList({
            contactId: contactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetContactInteractionsForContact(contactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactInteractionData[]> {
        return this.contactInteractionService.GetContactInteractionList({
            contactId: contactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetInitiatingContactsForContact(contactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactInteractionData[]> {
        return this.contactInteractionService.GetContactInteractionList({
            initiatingContactId: contactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetNotificationSubscriptionsForContact(contactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<NotificationSubscriptionData[]> {
        return this.notificationSubscriptionService.GetNotificationSubscriptionList({
            contactId: contactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConstituentsForContact(contactId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConstituentData[]> {
        return this.constituentService.GetConstituentList({
            contactId: contactId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full ContactData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the ContactData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when ContactTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveContact(raw: any): ContactData {
    if (!raw) return raw;

    //
    // Create a ContactData object instance with correct prototype
    //
    const revived = Object.create(ContactData.prototype) as ContactData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._contactChangeHistories = null;
    (revived as any)._contactChangeHistoriesPromise = null;
    (revived as any)._contactChangeHistoriesSubject = new BehaviorSubject<ContactChangeHistoryData[] | null>(null);

    (revived as any)._contactTags = null;
    (revived as any)._contactTagsPromise = null;
    (revived as any)._contactTagsSubject = new BehaviorSubject<ContactTagData[] | null>(null);

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

    (revived as any)._contactInteractions = null;
    (revived as any)._contactInteractionsPromise = null;
    (revived as any)._contactInteractionsSubject = new BehaviorSubject<ContactInteractionData[] | null>(null);

    (revived as any)._notificationSubscriptions = null;
    (revived as any)._notificationSubscriptionsPromise = null;
    (revived as any)._notificationSubscriptionsSubject = new BehaviorSubject<NotificationSubscriptionData[] | null>(null);

    (revived as any)._constituents = null;
    (revived as any)._constituentsPromise = null;
    (revived as any)._constituentsSubject = new BehaviorSubject<ConstituentData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadContactXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ContactChangeHistories$ = (revived as any)._contactChangeHistoriesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactChangeHistories === null && (revived as any)._contactChangeHistoriesPromise === null) {
                (revived as any).loadContactChangeHistories();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ContactChangeHistoriesCount$ = ContactChangeHistoryService.Instance.GetContactChangeHistoriesRowCount({contactId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ContactTags$ = (revived as any)._contactTagsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactTags === null && (revived as any)._contactTagsPromise === null) {
                (revived as any).loadContactTags();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ContactTagsCount$ = ContactTagService.Instance.GetContactTagsRowCount({contactId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ContactContacts$ = (revived as any)._contactContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactContacts === null && (revived as any)._contactContactsPromise === null) {
                (revived as any).loadContactContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ContactContactsCount$ = ContactContactService.Instance.GetContactContactsRowCount({contactId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).OfficeContacts$ = (revived as any)._officeContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._officeContacts === null && (revived as any)._officeContactsPromise === null) {
                (revived as any).loadOfficeContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).OfficeContactsCount$ = OfficeContactService.Instance.GetOfficeContactsRowCount({contactId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ClientContacts$ = (revived as any)._clientContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._clientContacts === null && (revived as any)._clientContactsPromise === null) {
                (revived as any).loadClientContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ClientContactsCount$ = ClientContactService.Instance.GetClientContactsRowCount({contactId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SchedulingTargetContacts$ = (revived as any)._schedulingTargetContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetContacts === null && (revived as any)._schedulingTargetContactsPromise === null) {
                (revived as any).loadSchedulingTargetContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SchedulingTargetContactsCount$ = SchedulingTargetContactService.Instance.GetSchedulingTargetContactsRowCount({contactId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).ResourceContacts$ = (revived as any)._resourceContactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resourceContacts === null && (revived as any)._resourceContactsPromise === null) {
                (revived as any).loadResourceContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ResourceContactsCount$ = ResourceContactService.Instance.GetResourceContactsRowCount({contactId: (revived as any).id,
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

    (revived as any).ContactInteractionsCount$ = ContactInteractionService.Instance.GetContactInteractionsRowCount({contactId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).NotificationSubscriptions$ = (revived as any)._notificationSubscriptionsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._notificationSubscriptions === null && (revived as any)._notificationSubscriptionsPromise === null) {
                (revived as any).loadNotificationSubscriptions();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).NotificationSubscriptionsCount$ = NotificationSubscriptionService.Instance.GetNotificationSubscriptionsRowCount({contactId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).Constituents$ = (revived as any)._constituentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._constituents === null && (revived as any)._constituentsPromise === null) {
                (revived as any).loadConstituents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).ConstituentsCount$ = ConstituentService.Instance.GetConstituentsRowCount({contactId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveContactList(rawList: any[]): ContactData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveContact(raw));
  }

}
