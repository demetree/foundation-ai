/*

   GENERATED SERVICE FOR THE ICON TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Icon table.

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
import { ResourceTypeService, ResourceTypeData } from './resource-type.service';
import { PriorityService, PriorityData } from './priority.service';
import { ContactMethodService, ContactMethodData } from './contact-method.service';
import { InteractionTypeService, InteractionTypeData } from './interaction-type.service';
import { TagService, TagData } from './tag.service';
import { VolunteerStatusService, VolunteerStatusData } from './volunteer-status.service';
import { ContactTypeService, ContactTypeData } from './contact-type.service';
import { ContactService, ContactData } from './contact.service';
import { RelationshipTypeService, RelationshipTypeData } from './relationship-type.service';
import { OfficeTypeService, OfficeTypeData } from './office-type.service';
import { CalendarService, CalendarData } from './calendar.service';
import { ClientTypeService, ClientTypeData } from './client-type.service';
import { AssignmentRoleService, AssignmentRoleData } from './assignment-role.service';
import { SchedulingTargetTypeService, SchedulingTargetTypeData } from './scheduling-target-type.service';
import { CrewService, CrewData } from './crew.service';
import { CrewMemberService, CrewMemberData } from './crew-member.service';
import { FundService, FundData } from './fund.service';
import { CampaignService, CampaignData } from './campaign.service';
import { AppealService, AppealData } from './appeal.service';
import { HouseholdService, HouseholdData } from './household.service';
import { ConstituentJourneyStageService, ConstituentJourneyStageData } from './constituent-journey-stage.service';
import { ConstituentService, ConstituentData } from './constituent.service';
import { TributeService, TributeData } from './tribute.service';
import { VolunteerProfileService, VolunteerProfileData } from './volunteer-profile.service';
import { VolunteerGroupService, VolunteerGroupData } from './volunteer-group.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class IconQueryParameters {
    name: string | null | undefined = null;
    fontAwesomeCode: string | null | undefined = null;
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
export class IconSubmitData {
    id!: bigint | number;
    name!: string;
    fontAwesomeCode: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class IconBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. IconChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `icon.IconChildren$` — use with `| async` in templates
//        • Promise:    `icon.IconChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="icon.IconChildren$ | async"`), or
//        • Access the promise getter (`icon.IconChildren` or `await icon.IconChildren`)
//    - Simply reading `icon.IconChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await icon.Reload()` to refresh the entire object and clear all lazy caches.
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
export class IconData {
    id!: bigint | number;
    name!: string;
    fontAwesomeCode!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _resourceTypes: ResourceTypeData[] | null = null;
    private _resourceTypesPromise: Promise<ResourceTypeData[]> | null  = null;
    private _resourceTypesSubject = new BehaviorSubject<ResourceTypeData[] | null>(null);

                
    private _priorities: PriorityData[] | null = null;
    private _prioritiesPromise: Promise<PriorityData[]> | null  = null;
    private _prioritiesSubject = new BehaviorSubject<PriorityData[] | null>(null);

                
    private _contactMethods: ContactMethodData[] | null = null;
    private _contactMethodsPromise: Promise<ContactMethodData[]> | null  = null;
    private _contactMethodsSubject = new BehaviorSubject<ContactMethodData[] | null>(null);

                
    private _interactionTypes: InteractionTypeData[] | null = null;
    private _interactionTypesPromise: Promise<InteractionTypeData[]> | null  = null;
    private _interactionTypesSubject = new BehaviorSubject<InteractionTypeData[] | null>(null);

                
    private _tags: TagData[] | null = null;
    private _tagsPromise: Promise<TagData[]> | null  = null;
    private _tagsSubject = new BehaviorSubject<TagData[] | null>(null);

                
    private _volunteerStatuses: VolunteerStatusData[] | null = null;
    private _volunteerStatusesPromise: Promise<VolunteerStatusData[]> | null  = null;
    private _volunteerStatusesSubject = new BehaviorSubject<VolunteerStatusData[] | null>(null);

                
    private _contactTypes: ContactTypeData[] | null = null;
    private _contactTypesPromise: Promise<ContactTypeData[]> | null  = null;
    private _contactTypesSubject = new BehaviorSubject<ContactTypeData[] | null>(null);

                
    private _contacts: ContactData[] | null = null;
    private _contactsPromise: Promise<ContactData[]> | null  = null;
    private _contactsSubject = new BehaviorSubject<ContactData[] | null>(null);

                
    private _relationshipTypes: RelationshipTypeData[] | null = null;
    private _relationshipTypesPromise: Promise<RelationshipTypeData[]> | null  = null;
    private _relationshipTypesSubject = new BehaviorSubject<RelationshipTypeData[] | null>(null);

                
    private _officeTypes: OfficeTypeData[] | null = null;
    private _officeTypesPromise: Promise<OfficeTypeData[]> | null  = null;
    private _officeTypesSubject = new BehaviorSubject<OfficeTypeData[] | null>(null);

                
    private _calendars: CalendarData[] | null = null;
    private _calendarsPromise: Promise<CalendarData[]> | null  = null;
    private _calendarsSubject = new BehaviorSubject<CalendarData[] | null>(null);

                
    private _clientTypes: ClientTypeData[] | null = null;
    private _clientTypesPromise: Promise<ClientTypeData[]> | null  = null;
    private _clientTypesSubject = new BehaviorSubject<ClientTypeData[] | null>(null);

                
    private _assignmentRoles: AssignmentRoleData[] | null = null;
    private _assignmentRolesPromise: Promise<AssignmentRoleData[]> | null  = null;
    private _assignmentRolesSubject = new BehaviorSubject<AssignmentRoleData[] | null>(null);

                
    private _schedulingTargetTypes: SchedulingTargetTypeData[] | null = null;
    private _schedulingTargetTypesPromise: Promise<SchedulingTargetTypeData[]> | null  = null;
    private _schedulingTargetTypesSubject = new BehaviorSubject<SchedulingTargetTypeData[] | null>(null);

                
    private _crews: CrewData[] | null = null;
    private _crewsPromise: Promise<CrewData[]> | null  = null;
    private _crewsSubject = new BehaviorSubject<CrewData[] | null>(null);

                
    private _crewMembers: CrewMemberData[] | null = null;
    private _crewMembersPromise: Promise<CrewMemberData[]> | null  = null;
    private _crewMembersSubject = new BehaviorSubject<CrewMemberData[] | null>(null);

                
    private _funds: FundData[] | null = null;
    private _fundsPromise: Promise<FundData[]> | null  = null;
    private _fundsSubject = new BehaviorSubject<FundData[] | null>(null);

                
    private _campaigns: CampaignData[] | null = null;
    private _campaignsPromise: Promise<CampaignData[]> | null  = null;
    private _campaignsSubject = new BehaviorSubject<CampaignData[] | null>(null);

                
    private _appeals: AppealData[] | null = null;
    private _appealsPromise: Promise<AppealData[]> | null  = null;
    private _appealsSubject = new BehaviorSubject<AppealData[] | null>(null);

                
    private _households: HouseholdData[] | null = null;
    private _householdsPromise: Promise<HouseholdData[]> | null  = null;
    private _householdsSubject = new BehaviorSubject<HouseholdData[] | null>(null);

                
    private _constituentJourneyStages: ConstituentJourneyStageData[] | null = null;
    private _constituentJourneyStagesPromise: Promise<ConstituentJourneyStageData[]> | null  = null;
    private _constituentJourneyStagesSubject = new BehaviorSubject<ConstituentJourneyStageData[] | null>(null);

                
    private _constituents: ConstituentData[] | null = null;
    private _constituentsPromise: Promise<ConstituentData[]> | null  = null;
    private _constituentsSubject = new BehaviorSubject<ConstituentData[] | null>(null);

                
    private _tributes: TributeData[] | null = null;
    private _tributesPromise: Promise<TributeData[]> | null  = null;
    private _tributesSubject = new BehaviorSubject<TributeData[] | null>(null);

                
    private _volunteerProfiles: VolunteerProfileData[] | null = null;
    private _volunteerProfilesPromise: Promise<VolunteerProfileData[]> | null  = null;
    private _volunteerProfilesSubject = new BehaviorSubject<VolunteerProfileData[] | null>(null);

                
    private _volunteerGroups: VolunteerGroupData[] | null = null;
    private _volunteerGroupsPromise: Promise<VolunteerGroupData[]> | null  = null;
    private _volunteerGroupsSubject = new BehaviorSubject<VolunteerGroupData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public ResourceTypes$ = this._resourceTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resourceTypes === null && this._resourceTypesPromise === null) {
            this.loadResourceTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _resourceTypesCount$: Observable<bigint | number> | null = null;
    public get ResourceTypesCount$(): Observable<bigint | number> {
        if (this._resourceTypesCount$ === null) {
            this._resourceTypesCount$ = ResourceTypeService.Instance.GetResourceTypesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._resourceTypesCount$;
    }



    public Priorities$ = this._prioritiesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._priorities === null && this._prioritiesPromise === null) {
            this.loadPriorities(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _prioritiesCount$: Observable<bigint | number> | null = null;
    public get PrioritiesCount$(): Observable<bigint | number> {
        if (this._prioritiesCount$ === null) {
            this._prioritiesCount$ = PriorityService.Instance.GetPrioritiesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._prioritiesCount$;
    }



    public ContactMethods$ = this._contactMethodsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactMethods === null && this._contactMethodsPromise === null) {
            this.loadContactMethods(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _contactMethodsCount$: Observable<bigint | number> | null = null;
    public get ContactMethodsCount$(): Observable<bigint | number> {
        if (this._contactMethodsCount$ === null) {
            this._contactMethodsCount$ = ContactMethodService.Instance.GetContactMethodsRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._contactMethodsCount$;
    }



    public InteractionTypes$ = this._interactionTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._interactionTypes === null && this._interactionTypesPromise === null) {
            this.loadInteractionTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _interactionTypesCount$: Observable<bigint | number> | null = null;
    public get InteractionTypesCount$(): Observable<bigint | number> {
        if (this._interactionTypesCount$ === null) {
            this._interactionTypesCount$ = InteractionTypeService.Instance.GetInteractionTypesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._interactionTypesCount$;
    }



    public Tags$ = this._tagsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._tags === null && this._tagsPromise === null) {
            this.loadTags(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _tagsCount$: Observable<bigint | number> | null = null;
    public get TagsCount$(): Observable<bigint | number> {
        if (this._tagsCount$ === null) {
            this._tagsCount$ = TagService.Instance.GetTagsRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._tagsCount$;
    }



    public VolunteerStatuses$ = this._volunteerStatusesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._volunteerStatuses === null && this._volunteerStatusesPromise === null) {
            this.loadVolunteerStatuses(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _volunteerStatusesCount$: Observable<bigint | number> | null = null;
    public get VolunteerStatusesCount$(): Observable<bigint | number> {
        if (this._volunteerStatusesCount$ === null) {
            this._volunteerStatusesCount$ = VolunteerStatusService.Instance.GetVolunteerStatusesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._volunteerStatusesCount$;
    }



    public ContactTypes$ = this._contactTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contactTypes === null && this._contactTypesPromise === null) {
            this.loadContactTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _contactTypesCount$: Observable<bigint | number> | null = null;
    public get ContactTypesCount$(): Observable<bigint | number> {
        if (this._contactTypesCount$ === null) {
            this._contactTypesCount$ = ContactTypeService.Instance.GetContactTypesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._contactTypesCount$;
    }



    public Contacts$ = this._contactsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._contacts === null && this._contactsPromise === null) {
            this.loadContacts(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _contactsCount$: Observable<bigint | number> | null = null;
    public get ContactsCount$(): Observable<bigint | number> {
        if (this._contactsCount$ === null) {
            this._contactsCount$ = ContactService.Instance.GetContactsRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._contactsCount$;
    }



    public RelationshipTypes$ = this._relationshipTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._relationshipTypes === null && this._relationshipTypesPromise === null) {
            this.loadRelationshipTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _relationshipTypesCount$: Observable<bigint | number> | null = null;
    public get RelationshipTypesCount$(): Observable<bigint | number> {
        if (this._relationshipTypesCount$ === null) {
            this._relationshipTypesCount$ = RelationshipTypeService.Instance.GetRelationshipTypesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._relationshipTypesCount$;
    }



    public OfficeTypes$ = this._officeTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._officeTypes === null && this._officeTypesPromise === null) {
            this.loadOfficeTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _officeTypesCount$: Observable<bigint | number> | null = null;
    public get OfficeTypesCount$(): Observable<bigint | number> {
        if (this._officeTypesCount$ === null) {
            this._officeTypesCount$ = OfficeTypeService.Instance.GetOfficeTypesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._officeTypesCount$;
    }



    public Calendars$ = this._calendarsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._calendars === null && this._calendarsPromise === null) {
            this.loadCalendars(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _calendarsCount$: Observable<bigint | number> | null = null;
    public get CalendarsCount$(): Observable<bigint | number> {
        if (this._calendarsCount$ === null) {
            this._calendarsCount$ = CalendarService.Instance.GetCalendarsRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._calendarsCount$;
    }



    public ClientTypes$ = this._clientTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._clientTypes === null && this._clientTypesPromise === null) {
            this.loadClientTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _clientTypesCount$: Observable<bigint | number> | null = null;
    public get ClientTypesCount$(): Observable<bigint | number> {
        if (this._clientTypesCount$ === null) {
            this._clientTypesCount$ = ClientTypeService.Instance.GetClientTypesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._clientTypesCount$;
    }



    public AssignmentRoles$ = this._assignmentRolesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._assignmentRoles === null && this._assignmentRolesPromise === null) {
            this.loadAssignmentRoles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _assignmentRolesCount$: Observable<bigint | number> | null = null;
    public get AssignmentRolesCount$(): Observable<bigint | number> {
        if (this._assignmentRolesCount$ === null) {
            this._assignmentRolesCount$ = AssignmentRoleService.Instance.GetAssignmentRolesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._assignmentRolesCount$;
    }



    public SchedulingTargetTypes$ = this._schedulingTargetTypesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetTypes === null && this._schedulingTargetTypesPromise === null) {
            this.loadSchedulingTargetTypes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _schedulingTargetTypesCount$: Observable<bigint | number> | null = null;
    public get SchedulingTargetTypesCount$(): Observable<bigint | number> {
        if (this._schedulingTargetTypesCount$ === null) {
            this._schedulingTargetTypesCount$ = SchedulingTargetTypeService.Instance.GetSchedulingTargetTypesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._schedulingTargetTypesCount$;
    }



    public Crews$ = this._crewsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._crews === null && this._crewsPromise === null) {
            this.loadCrews(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _crewsCount$: Observable<bigint | number> | null = null;
    public get CrewsCount$(): Observable<bigint | number> {
        if (this._crewsCount$ === null) {
            this._crewsCount$ = CrewService.Instance.GetCrewsRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._crewsCount$;
    }



    public CrewMembers$ = this._crewMembersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._crewMembers === null && this._crewMembersPromise === null) {
            this.loadCrewMembers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _crewMembersCount$: Observable<bigint | number> | null = null;
    public get CrewMembersCount$(): Observable<bigint | number> {
        if (this._crewMembersCount$ === null) {
            this._crewMembersCount$ = CrewMemberService.Instance.GetCrewMembersRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._crewMembersCount$;
    }



    public Funds$ = this._fundsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._funds === null && this._fundsPromise === null) {
            this.loadFunds(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _fundsCount$: Observable<bigint | number> | null = null;
    public get FundsCount$(): Observable<bigint | number> {
        if (this._fundsCount$ === null) {
            this._fundsCount$ = FundService.Instance.GetFundsRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._fundsCount$;
    }



    public Campaigns$ = this._campaignsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._campaigns === null && this._campaignsPromise === null) {
            this.loadCampaigns(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _campaignsCount$: Observable<bigint | number> | null = null;
    public get CampaignsCount$(): Observable<bigint | number> {
        if (this._campaignsCount$ === null) {
            this._campaignsCount$ = CampaignService.Instance.GetCampaignsRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._campaignsCount$;
    }



    public Appeals$ = this._appealsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._appeals === null && this._appealsPromise === null) {
            this.loadAppeals(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _appealsCount$: Observable<bigint | number> | null = null;
    public get AppealsCount$(): Observable<bigint | number> {
        if (this._appealsCount$ === null) {
            this._appealsCount$ = AppealService.Instance.GetAppealsRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._appealsCount$;
    }



    public Households$ = this._householdsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._households === null && this._householdsPromise === null) {
            this.loadHouseholds(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _householdsCount$: Observable<bigint | number> | null = null;
    public get HouseholdsCount$(): Observable<bigint | number> {
        if (this._householdsCount$ === null) {
            this._householdsCount$ = HouseholdService.Instance.GetHouseholdsRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._householdsCount$;
    }



    public ConstituentJourneyStages$ = this._constituentJourneyStagesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._constituentJourneyStages === null && this._constituentJourneyStagesPromise === null) {
            this.loadConstituentJourneyStages(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _constituentJourneyStagesCount$: Observable<bigint | number> | null = null;
    public get ConstituentJourneyStagesCount$(): Observable<bigint | number> {
        if (this._constituentJourneyStagesCount$ === null) {
            this._constituentJourneyStagesCount$ = ConstituentJourneyStageService.Instance.GetConstituentJourneyStagesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._constituentJourneyStagesCount$;
    }



    public Constituents$ = this._constituentsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._constituents === null && this._constituentsPromise === null) {
            this.loadConstituents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _constituentsCount$: Observable<bigint | number> | null = null;
    public get ConstituentsCount$(): Observable<bigint | number> {
        if (this._constituentsCount$ === null) {
            this._constituentsCount$ = ConstituentService.Instance.GetConstituentsRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._constituentsCount$;
    }



    public Tributes$ = this._tributesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._tributes === null && this._tributesPromise === null) {
            this.loadTributes(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _tributesCount$: Observable<bigint | number> | null = null;
    public get TributesCount$(): Observable<bigint | number> {
        if (this._tributesCount$ === null) {
            this._tributesCount$ = TributeService.Instance.GetTributesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._tributesCount$;
    }



    public VolunteerProfiles$ = this._volunteerProfilesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._volunteerProfiles === null && this._volunteerProfilesPromise === null) {
            this.loadVolunteerProfiles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _volunteerProfilesCount$: Observable<bigint | number> | null = null;
    public get VolunteerProfilesCount$(): Observable<bigint | number> {
        if (this._volunteerProfilesCount$ === null) {
            this._volunteerProfilesCount$ = VolunteerProfileService.Instance.GetVolunteerProfilesRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._volunteerProfilesCount$;
    }



    public VolunteerGroups$ = this._volunteerGroupsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._volunteerGroups === null && this._volunteerGroupsPromise === null) {
            this.loadVolunteerGroups(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _volunteerGroupsCount$: Observable<bigint | number> | null = null;
    public get VolunteerGroupsCount$(): Observable<bigint | number> {
        if (this._volunteerGroupsCount$ === null) {
            this._volunteerGroupsCount$ = VolunteerGroupService.Instance.GetVolunteerGroupsRowCount({iconId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._volunteerGroupsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any IconData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.icon.Reload();
  //
  //  Non Async:
  //
  //     icon[0].Reload().then(x => {
  //        this.icon = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      IconService.Instance.GetIcon(this.id, includeRelations)
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
     this._resourceTypes = null;
     this._resourceTypesPromise = null;
     this._resourceTypesSubject.next(null);
     this._resourceTypesCount$ = null;

     this._priorities = null;
     this._prioritiesPromise = null;
     this._prioritiesSubject.next(null);
     this._prioritiesCount$ = null;

     this._contactMethods = null;
     this._contactMethodsPromise = null;
     this._contactMethodsSubject.next(null);
     this._contactMethodsCount$ = null;

     this._interactionTypes = null;
     this._interactionTypesPromise = null;
     this._interactionTypesSubject.next(null);
     this._interactionTypesCount$ = null;

     this._tags = null;
     this._tagsPromise = null;
     this._tagsSubject.next(null);
     this._tagsCount$ = null;

     this._volunteerStatuses = null;
     this._volunteerStatusesPromise = null;
     this._volunteerStatusesSubject.next(null);
     this._volunteerStatusesCount$ = null;

     this._contactTypes = null;
     this._contactTypesPromise = null;
     this._contactTypesSubject.next(null);
     this._contactTypesCount$ = null;

     this._contacts = null;
     this._contactsPromise = null;
     this._contactsSubject.next(null);
     this._contactsCount$ = null;

     this._relationshipTypes = null;
     this._relationshipTypesPromise = null;
     this._relationshipTypesSubject.next(null);
     this._relationshipTypesCount$ = null;

     this._officeTypes = null;
     this._officeTypesPromise = null;
     this._officeTypesSubject.next(null);
     this._officeTypesCount$ = null;

     this._calendars = null;
     this._calendarsPromise = null;
     this._calendarsSubject.next(null);
     this._calendarsCount$ = null;

     this._clientTypes = null;
     this._clientTypesPromise = null;
     this._clientTypesSubject.next(null);
     this._clientTypesCount$ = null;

     this._assignmentRoles = null;
     this._assignmentRolesPromise = null;
     this._assignmentRolesSubject.next(null);
     this._assignmentRolesCount$ = null;

     this._schedulingTargetTypes = null;
     this._schedulingTargetTypesPromise = null;
     this._schedulingTargetTypesSubject.next(null);
     this._schedulingTargetTypesCount$ = null;

     this._crews = null;
     this._crewsPromise = null;
     this._crewsSubject.next(null);
     this._crewsCount$ = null;

     this._crewMembers = null;
     this._crewMembersPromise = null;
     this._crewMembersSubject.next(null);
     this._crewMembersCount$ = null;

     this._funds = null;
     this._fundsPromise = null;
     this._fundsSubject.next(null);
     this._fundsCount$ = null;

     this._campaigns = null;
     this._campaignsPromise = null;
     this._campaignsSubject.next(null);
     this._campaignsCount$ = null;

     this._appeals = null;
     this._appealsPromise = null;
     this._appealsSubject.next(null);
     this._appealsCount$ = null;

     this._households = null;
     this._householdsPromise = null;
     this._householdsSubject.next(null);
     this._householdsCount$ = null;

     this._constituentJourneyStages = null;
     this._constituentJourneyStagesPromise = null;
     this._constituentJourneyStagesSubject.next(null);
     this._constituentJourneyStagesCount$ = null;

     this._constituents = null;
     this._constituentsPromise = null;
     this._constituentsSubject.next(null);
     this._constituentsCount$ = null;

     this._tributes = null;
     this._tributesPromise = null;
     this._tributesSubject.next(null);
     this._tributesCount$ = null;

     this._volunteerProfiles = null;
     this._volunteerProfilesPromise = null;
     this._volunteerProfilesSubject.next(null);
     this._volunteerProfilesCount$ = null;

     this._volunteerGroups = null;
     this._volunteerGroupsPromise = null;
     this._volunteerGroupsSubject.next(null);
     this._volunteerGroupsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the ResourceTypes for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.ResourceTypes.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get ResourceTypes(): Promise<ResourceTypeData[]> {
        if (this._resourceTypes !== null) {
            return Promise.resolve(this._resourceTypes);
        }

        if (this._resourceTypesPromise !== null) {
            return this._resourceTypesPromise;
        }

        // Start the load
        this.loadResourceTypes();

        return this._resourceTypesPromise!;
    }



    private loadResourceTypes(): void {

        this._resourceTypesPromise = lastValueFrom(
            IconService.Instance.GetResourceTypesForIcon(this.id)
        )
        .then(ResourceTypes => {
            this._resourceTypes = ResourceTypes ?? [];
            this._resourceTypesSubject.next(this._resourceTypes);
            return this._resourceTypes;
         })
        .catch(err => {
            this._resourceTypes = [];
            this._resourceTypesSubject.next(this._resourceTypes);
            throw err;
        })
        .finally(() => {
            this._resourceTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ResourceType. Call after mutations to force refresh.
     */
    public ClearResourceTypesCache(): void {
        this._resourceTypes = null;
        this._resourceTypesPromise = null;
        this._resourceTypesSubject.next(this._resourceTypes);      // Emit to observable
    }

    public get HasResourceTypes(): Promise<boolean> {
        return this.ResourceTypes.then(resourceTypes => resourceTypes.length > 0);
    }


    /**
     *
     * Gets the Priorities for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.Priorities.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get Priorities(): Promise<PriorityData[]> {
        if (this._priorities !== null) {
            return Promise.resolve(this._priorities);
        }

        if (this._prioritiesPromise !== null) {
            return this._prioritiesPromise;
        }

        // Start the load
        this.loadPriorities();

        return this._prioritiesPromise!;
    }



    private loadPriorities(): void {

        this._prioritiesPromise = lastValueFrom(
            IconService.Instance.GetPrioritiesForIcon(this.id)
        )
        .then(Priorities => {
            this._priorities = Priorities ?? [];
            this._prioritiesSubject.next(this._priorities);
            return this._priorities;
         })
        .catch(err => {
            this._priorities = [];
            this._prioritiesSubject.next(this._priorities);
            throw err;
        })
        .finally(() => {
            this._prioritiesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Priority. Call after mutations to force refresh.
     */
    public ClearPrioritiesCache(): void {
        this._priorities = null;
        this._prioritiesPromise = null;
        this._prioritiesSubject.next(this._priorities);      // Emit to observable
    }

    public get HasPriorities(): Promise<boolean> {
        return this.Priorities.then(priorities => priorities.length > 0);
    }


    /**
     *
     * Gets the ContactMethods for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.ContactMethods.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get ContactMethods(): Promise<ContactMethodData[]> {
        if (this._contactMethods !== null) {
            return Promise.resolve(this._contactMethods);
        }

        if (this._contactMethodsPromise !== null) {
            return this._contactMethodsPromise;
        }

        // Start the load
        this.loadContactMethods();

        return this._contactMethodsPromise!;
    }



    private loadContactMethods(): void {

        this._contactMethodsPromise = lastValueFrom(
            IconService.Instance.GetContactMethodsForIcon(this.id)
        )
        .then(ContactMethods => {
            this._contactMethods = ContactMethods ?? [];
            this._contactMethodsSubject.next(this._contactMethods);
            return this._contactMethods;
         })
        .catch(err => {
            this._contactMethods = [];
            this._contactMethodsSubject.next(this._contactMethods);
            throw err;
        })
        .finally(() => {
            this._contactMethodsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContactMethod. Call after mutations to force refresh.
     */
    public ClearContactMethodsCache(): void {
        this._contactMethods = null;
        this._contactMethodsPromise = null;
        this._contactMethodsSubject.next(this._contactMethods);      // Emit to observable
    }

    public get HasContactMethods(): Promise<boolean> {
        return this.ContactMethods.then(contactMethods => contactMethods.length > 0);
    }


    /**
     *
     * Gets the InteractionTypes for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.InteractionTypes.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get InteractionTypes(): Promise<InteractionTypeData[]> {
        if (this._interactionTypes !== null) {
            return Promise.resolve(this._interactionTypes);
        }

        if (this._interactionTypesPromise !== null) {
            return this._interactionTypesPromise;
        }

        // Start the load
        this.loadInteractionTypes();

        return this._interactionTypesPromise!;
    }



    private loadInteractionTypes(): void {

        this._interactionTypesPromise = lastValueFrom(
            IconService.Instance.GetInteractionTypesForIcon(this.id)
        )
        .then(InteractionTypes => {
            this._interactionTypes = InteractionTypes ?? [];
            this._interactionTypesSubject.next(this._interactionTypes);
            return this._interactionTypes;
         })
        .catch(err => {
            this._interactionTypes = [];
            this._interactionTypesSubject.next(this._interactionTypes);
            throw err;
        })
        .finally(() => {
            this._interactionTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached InteractionType. Call after mutations to force refresh.
     */
    public ClearInteractionTypesCache(): void {
        this._interactionTypes = null;
        this._interactionTypesPromise = null;
        this._interactionTypesSubject.next(this._interactionTypes);      // Emit to observable
    }

    public get HasInteractionTypes(): Promise<boolean> {
        return this.InteractionTypes.then(interactionTypes => interactionTypes.length > 0);
    }


    /**
     *
     * Gets the Tags for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.Tags.then(icons => { ... })
     *   or
     *   await this.icon.icons
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
            IconService.Instance.GetTagsForIcon(this.id)
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
     * Gets the VolunteerStatuses for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.VolunteerStatuses.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get VolunteerStatuses(): Promise<VolunteerStatusData[]> {
        if (this._volunteerStatuses !== null) {
            return Promise.resolve(this._volunteerStatuses);
        }

        if (this._volunteerStatusesPromise !== null) {
            return this._volunteerStatusesPromise;
        }

        // Start the load
        this.loadVolunteerStatuses();

        return this._volunteerStatusesPromise!;
    }



    private loadVolunteerStatuses(): void {

        this._volunteerStatusesPromise = lastValueFrom(
            IconService.Instance.GetVolunteerStatusesForIcon(this.id)
        )
        .then(VolunteerStatuses => {
            this._volunteerStatuses = VolunteerStatuses ?? [];
            this._volunteerStatusesSubject.next(this._volunteerStatuses);
            return this._volunteerStatuses;
         })
        .catch(err => {
            this._volunteerStatuses = [];
            this._volunteerStatusesSubject.next(this._volunteerStatuses);
            throw err;
        })
        .finally(() => {
            this._volunteerStatusesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached VolunteerStatus. Call after mutations to force refresh.
     */
    public ClearVolunteerStatusesCache(): void {
        this._volunteerStatuses = null;
        this._volunteerStatusesPromise = null;
        this._volunteerStatusesSubject.next(this._volunteerStatuses);      // Emit to observable
    }

    public get HasVolunteerStatuses(): Promise<boolean> {
        return this.VolunteerStatuses.then(volunteerStatuses => volunteerStatuses.length > 0);
    }


    /**
     *
     * Gets the ContactTypes for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.ContactTypes.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get ContactTypes(): Promise<ContactTypeData[]> {
        if (this._contactTypes !== null) {
            return Promise.resolve(this._contactTypes);
        }

        if (this._contactTypesPromise !== null) {
            return this._contactTypesPromise;
        }

        // Start the load
        this.loadContactTypes();

        return this._contactTypesPromise!;
    }



    private loadContactTypes(): void {

        this._contactTypesPromise = lastValueFrom(
            IconService.Instance.GetContactTypesForIcon(this.id)
        )
        .then(ContactTypes => {
            this._contactTypes = ContactTypes ?? [];
            this._contactTypesSubject.next(this._contactTypes);
            return this._contactTypes;
         })
        .catch(err => {
            this._contactTypes = [];
            this._contactTypesSubject.next(this._contactTypes);
            throw err;
        })
        .finally(() => {
            this._contactTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ContactType. Call after mutations to force refresh.
     */
    public ClearContactTypesCache(): void {
        this._contactTypes = null;
        this._contactTypesPromise = null;
        this._contactTypesSubject.next(this._contactTypes);      // Emit to observable
    }

    public get HasContactTypes(): Promise<boolean> {
        return this.ContactTypes.then(contactTypes => contactTypes.length > 0);
    }


    /**
     *
     * Gets the Contacts for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.Contacts.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get Contacts(): Promise<ContactData[]> {
        if (this._contacts !== null) {
            return Promise.resolve(this._contacts);
        }

        if (this._contactsPromise !== null) {
            return this._contactsPromise;
        }

        // Start the load
        this.loadContacts();

        return this._contactsPromise!;
    }



    private loadContacts(): void {

        this._contactsPromise = lastValueFrom(
            IconService.Instance.GetContactsForIcon(this.id)
        )
        .then(Contacts => {
            this._contacts = Contacts ?? [];
            this._contactsSubject.next(this._contacts);
            return this._contacts;
         })
        .catch(err => {
            this._contacts = [];
            this._contactsSubject.next(this._contacts);
            throw err;
        })
        .finally(() => {
            this._contactsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Contact. Call after mutations to force refresh.
     */
    public ClearContactsCache(): void {
        this._contacts = null;
        this._contactsPromise = null;
        this._contactsSubject.next(this._contacts);      // Emit to observable
    }

    public get HasContacts(): Promise<boolean> {
        return this.Contacts.then(contacts => contacts.length > 0);
    }


    /**
     *
     * Gets the RelationshipTypes for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.RelationshipTypes.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get RelationshipTypes(): Promise<RelationshipTypeData[]> {
        if (this._relationshipTypes !== null) {
            return Promise.resolve(this._relationshipTypes);
        }

        if (this._relationshipTypesPromise !== null) {
            return this._relationshipTypesPromise;
        }

        // Start the load
        this.loadRelationshipTypes();

        return this._relationshipTypesPromise!;
    }



    private loadRelationshipTypes(): void {

        this._relationshipTypesPromise = lastValueFrom(
            IconService.Instance.GetRelationshipTypesForIcon(this.id)
        )
        .then(RelationshipTypes => {
            this._relationshipTypes = RelationshipTypes ?? [];
            this._relationshipTypesSubject.next(this._relationshipTypes);
            return this._relationshipTypes;
         })
        .catch(err => {
            this._relationshipTypes = [];
            this._relationshipTypesSubject.next(this._relationshipTypes);
            throw err;
        })
        .finally(() => {
            this._relationshipTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached RelationshipType. Call after mutations to force refresh.
     */
    public ClearRelationshipTypesCache(): void {
        this._relationshipTypes = null;
        this._relationshipTypesPromise = null;
        this._relationshipTypesSubject.next(this._relationshipTypes);      // Emit to observable
    }

    public get HasRelationshipTypes(): Promise<boolean> {
        return this.RelationshipTypes.then(relationshipTypes => relationshipTypes.length > 0);
    }


    /**
     *
     * Gets the OfficeTypes for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.OfficeTypes.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get OfficeTypes(): Promise<OfficeTypeData[]> {
        if (this._officeTypes !== null) {
            return Promise.resolve(this._officeTypes);
        }

        if (this._officeTypesPromise !== null) {
            return this._officeTypesPromise;
        }

        // Start the load
        this.loadOfficeTypes();

        return this._officeTypesPromise!;
    }



    private loadOfficeTypes(): void {

        this._officeTypesPromise = lastValueFrom(
            IconService.Instance.GetOfficeTypesForIcon(this.id)
        )
        .then(OfficeTypes => {
            this._officeTypes = OfficeTypes ?? [];
            this._officeTypesSubject.next(this._officeTypes);
            return this._officeTypes;
         })
        .catch(err => {
            this._officeTypes = [];
            this._officeTypesSubject.next(this._officeTypes);
            throw err;
        })
        .finally(() => {
            this._officeTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached OfficeType. Call after mutations to force refresh.
     */
    public ClearOfficeTypesCache(): void {
        this._officeTypes = null;
        this._officeTypesPromise = null;
        this._officeTypesSubject.next(this._officeTypes);      // Emit to observable
    }

    public get HasOfficeTypes(): Promise<boolean> {
        return this.OfficeTypes.then(officeTypes => officeTypes.length > 0);
    }


    /**
     *
     * Gets the Calendars for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.Calendars.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get Calendars(): Promise<CalendarData[]> {
        if (this._calendars !== null) {
            return Promise.resolve(this._calendars);
        }

        if (this._calendarsPromise !== null) {
            return this._calendarsPromise;
        }

        // Start the load
        this.loadCalendars();

        return this._calendarsPromise!;
    }



    private loadCalendars(): void {

        this._calendarsPromise = lastValueFrom(
            IconService.Instance.GetCalendarsForIcon(this.id)
        )
        .then(Calendars => {
            this._calendars = Calendars ?? [];
            this._calendarsSubject.next(this._calendars);
            return this._calendars;
         })
        .catch(err => {
            this._calendars = [];
            this._calendarsSubject.next(this._calendars);
            throw err;
        })
        .finally(() => {
            this._calendarsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Calendar. Call after mutations to force refresh.
     */
    public ClearCalendarsCache(): void {
        this._calendars = null;
        this._calendarsPromise = null;
        this._calendarsSubject.next(this._calendars);      // Emit to observable
    }

    public get HasCalendars(): Promise<boolean> {
        return this.Calendars.then(calendars => calendars.length > 0);
    }


    /**
     *
     * Gets the ClientTypes for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.ClientTypes.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get ClientTypes(): Promise<ClientTypeData[]> {
        if (this._clientTypes !== null) {
            return Promise.resolve(this._clientTypes);
        }

        if (this._clientTypesPromise !== null) {
            return this._clientTypesPromise;
        }

        // Start the load
        this.loadClientTypes();

        return this._clientTypesPromise!;
    }



    private loadClientTypes(): void {

        this._clientTypesPromise = lastValueFrom(
            IconService.Instance.GetClientTypesForIcon(this.id)
        )
        .then(ClientTypes => {
            this._clientTypes = ClientTypes ?? [];
            this._clientTypesSubject.next(this._clientTypes);
            return this._clientTypes;
         })
        .catch(err => {
            this._clientTypes = [];
            this._clientTypesSubject.next(this._clientTypes);
            throw err;
        })
        .finally(() => {
            this._clientTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ClientType. Call after mutations to force refresh.
     */
    public ClearClientTypesCache(): void {
        this._clientTypes = null;
        this._clientTypesPromise = null;
        this._clientTypesSubject.next(this._clientTypes);      // Emit to observable
    }

    public get HasClientTypes(): Promise<boolean> {
        return this.ClientTypes.then(clientTypes => clientTypes.length > 0);
    }


    /**
     *
     * Gets the AssignmentRoles for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.AssignmentRoles.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get AssignmentRoles(): Promise<AssignmentRoleData[]> {
        if (this._assignmentRoles !== null) {
            return Promise.resolve(this._assignmentRoles);
        }

        if (this._assignmentRolesPromise !== null) {
            return this._assignmentRolesPromise;
        }

        // Start the load
        this.loadAssignmentRoles();

        return this._assignmentRolesPromise!;
    }



    private loadAssignmentRoles(): void {

        this._assignmentRolesPromise = lastValueFrom(
            IconService.Instance.GetAssignmentRolesForIcon(this.id)
        )
        .then(AssignmentRoles => {
            this._assignmentRoles = AssignmentRoles ?? [];
            this._assignmentRolesSubject.next(this._assignmentRoles);
            return this._assignmentRoles;
         })
        .catch(err => {
            this._assignmentRoles = [];
            this._assignmentRolesSubject.next(this._assignmentRoles);
            throw err;
        })
        .finally(() => {
            this._assignmentRolesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached AssignmentRole. Call after mutations to force refresh.
     */
    public ClearAssignmentRolesCache(): void {
        this._assignmentRoles = null;
        this._assignmentRolesPromise = null;
        this._assignmentRolesSubject.next(this._assignmentRoles);      // Emit to observable
    }

    public get HasAssignmentRoles(): Promise<boolean> {
        return this.AssignmentRoles.then(assignmentRoles => assignmentRoles.length > 0);
    }


    /**
     *
     * Gets the SchedulingTargetTypes for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.SchedulingTargetTypes.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get SchedulingTargetTypes(): Promise<SchedulingTargetTypeData[]> {
        if (this._schedulingTargetTypes !== null) {
            return Promise.resolve(this._schedulingTargetTypes);
        }

        if (this._schedulingTargetTypesPromise !== null) {
            return this._schedulingTargetTypesPromise;
        }

        // Start the load
        this.loadSchedulingTargetTypes();

        return this._schedulingTargetTypesPromise!;
    }



    private loadSchedulingTargetTypes(): void {

        this._schedulingTargetTypesPromise = lastValueFrom(
            IconService.Instance.GetSchedulingTargetTypesForIcon(this.id)
        )
        .then(SchedulingTargetTypes => {
            this._schedulingTargetTypes = SchedulingTargetTypes ?? [];
            this._schedulingTargetTypesSubject.next(this._schedulingTargetTypes);
            return this._schedulingTargetTypes;
         })
        .catch(err => {
            this._schedulingTargetTypes = [];
            this._schedulingTargetTypesSubject.next(this._schedulingTargetTypes);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetTypesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTargetType. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetTypesCache(): void {
        this._schedulingTargetTypes = null;
        this._schedulingTargetTypesPromise = null;
        this._schedulingTargetTypesSubject.next(this._schedulingTargetTypes);      // Emit to observable
    }

    public get HasSchedulingTargetTypes(): Promise<boolean> {
        return this.SchedulingTargetTypes.then(schedulingTargetTypes => schedulingTargetTypes.length > 0);
    }


    /**
     *
     * Gets the Crews for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.Crews.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get Crews(): Promise<CrewData[]> {
        if (this._crews !== null) {
            return Promise.resolve(this._crews);
        }

        if (this._crewsPromise !== null) {
            return this._crewsPromise;
        }

        // Start the load
        this.loadCrews();

        return this._crewsPromise!;
    }



    private loadCrews(): void {

        this._crewsPromise = lastValueFrom(
            IconService.Instance.GetCrewsForIcon(this.id)
        )
        .then(Crews => {
            this._crews = Crews ?? [];
            this._crewsSubject.next(this._crews);
            return this._crews;
         })
        .catch(err => {
            this._crews = [];
            this._crewsSubject.next(this._crews);
            throw err;
        })
        .finally(() => {
            this._crewsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Crew. Call after mutations to force refresh.
     */
    public ClearCrewsCache(): void {
        this._crews = null;
        this._crewsPromise = null;
        this._crewsSubject.next(this._crews);      // Emit to observable
    }

    public get HasCrews(): Promise<boolean> {
        return this.Crews.then(crews => crews.length > 0);
    }


    /**
     *
     * Gets the CrewMembers for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.CrewMembers.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get CrewMembers(): Promise<CrewMemberData[]> {
        if (this._crewMembers !== null) {
            return Promise.resolve(this._crewMembers);
        }

        if (this._crewMembersPromise !== null) {
            return this._crewMembersPromise;
        }

        // Start the load
        this.loadCrewMembers();

        return this._crewMembersPromise!;
    }



    private loadCrewMembers(): void {

        this._crewMembersPromise = lastValueFrom(
            IconService.Instance.GetCrewMembersForIcon(this.id)
        )
        .then(CrewMembers => {
            this._crewMembers = CrewMembers ?? [];
            this._crewMembersSubject.next(this._crewMembers);
            return this._crewMembers;
         })
        .catch(err => {
            this._crewMembers = [];
            this._crewMembersSubject.next(this._crewMembers);
            throw err;
        })
        .finally(() => {
            this._crewMembersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached CrewMember. Call after mutations to force refresh.
     */
    public ClearCrewMembersCache(): void {
        this._crewMembers = null;
        this._crewMembersPromise = null;
        this._crewMembersSubject.next(this._crewMembers);      // Emit to observable
    }

    public get HasCrewMembers(): Promise<boolean> {
        return this.CrewMembers.then(crewMembers => crewMembers.length > 0);
    }


    /**
     *
     * Gets the Funds for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.Funds.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get Funds(): Promise<FundData[]> {
        if (this._funds !== null) {
            return Promise.resolve(this._funds);
        }

        if (this._fundsPromise !== null) {
            return this._fundsPromise;
        }

        // Start the load
        this.loadFunds();

        return this._fundsPromise!;
    }



    private loadFunds(): void {

        this._fundsPromise = lastValueFrom(
            IconService.Instance.GetFundsForIcon(this.id)
        )
        .then(Funds => {
            this._funds = Funds ?? [];
            this._fundsSubject.next(this._funds);
            return this._funds;
         })
        .catch(err => {
            this._funds = [];
            this._fundsSubject.next(this._funds);
            throw err;
        })
        .finally(() => {
            this._fundsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Fund. Call after mutations to force refresh.
     */
    public ClearFundsCache(): void {
        this._funds = null;
        this._fundsPromise = null;
        this._fundsSubject.next(this._funds);      // Emit to observable
    }

    public get HasFunds(): Promise<boolean> {
        return this.Funds.then(funds => funds.length > 0);
    }


    /**
     *
     * Gets the Campaigns for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.Campaigns.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get Campaigns(): Promise<CampaignData[]> {
        if (this._campaigns !== null) {
            return Promise.resolve(this._campaigns);
        }

        if (this._campaignsPromise !== null) {
            return this._campaignsPromise;
        }

        // Start the load
        this.loadCampaigns();

        return this._campaignsPromise!;
    }



    private loadCampaigns(): void {

        this._campaignsPromise = lastValueFrom(
            IconService.Instance.GetCampaignsForIcon(this.id)
        )
        .then(Campaigns => {
            this._campaigns = Campaigns ?? [];
            this._campaignsSubject.next(this._campaigns);
            return this._campaigns;
         })
        .catch(err => {
            this._campaigns = [];
            this._campaignsSubject.next(this._campaigns);
            throw err;
        })
        .finally(() => {
            this._campaignsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Campaign. Call after mutations to force refresh.
     */
    public ClearCampaignsCache(): void {
        this._campaigns = null;
        this._campaignsPromise = null;
        this._campaignsSubject.next(this._campaigns);      // Emit to observable
    }

    public get HasCampaigns(): Promise<boolean> {
        return this.Campaigns.then(campaigns => campaigns.length > 0);
    }


    /**
     *
     * Gets the Appeals for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.Appeals.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get Appeals(): Promise<AppealData[]> {
        if (this._appeals !== null) {
            return Promise.resolve(this._appeals);
        }

        if (this._appealsPromise !== null) {
            return this._appealsPromise;
        }

        // Start the load
        this.loadAppeals();

        return this._appealsPromise!;
    }



    private loadAppeals(): void {

        this._appealsPromise = lastValueFrom(
            IconService.Instance.GetAppealsForIcon(this.id)
        )
        .then(Appeals => {
            this._appeals = Appeals ?? [];
            this._appealsSubject.next(this._appeals);
            return this._appeals;
         })
        .catch(err => {
            this._appeals = [];
            this._appealsSubject.next(this._appeals);
            throw err;
        })
        .finally(() => {
            this._appealsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Appeal. Call after mutations to force refresh.
     */
    public ClearAppealsCache(): void {
        this._appeals = null;
        this._appealsPromise = null;
        this._appealsSubject.next(this._appeals);      // Emit to observable
    }

    public get HasAppeals(): Promise<boolean> {
        return this.Appeals.then(appeals => appeals.length > 0);
    }


    /**
     *
     * Gets the Households for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.Households.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get Households(): Promise<HouseholdData[]> {
        if (this._households !== null) {
            return Promise.resolve(this._households);
        }

        if (this._householdsPromise !== null) {
            return this._householdsPromise;
        }

        // Start the load
        this.loadHouseholds();

        return this._householdsPromise!;
    }



    private loadHouseholds(): void {

        this._householdsPromise = lastValueFrom(
            IconService.Instance.GetHouseholdsForIcon(this.id)
        )
        .then(Households => {
            this._households = Households ?? [];
            this._householdsSubject.next(this._households);
            return this._households;
         })
        .catch(err => {
            this._households = [];
            this._householdsSubject.next(this._households);
            throw err;
        })
        .finally(() => {
            this._householdsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Household. Call after mutations to force refresh.
     */
    public ClearHouseholdsCache(): void {
        this._households = null;
        this._householdsPromise = null;
        this._householdsSubject.next(this._households);      // Emit to observable
    }

    public get HasHouseholds(): Promise<boolean> {
        return this.Households.then(households => households.length > 0);
    }


    /**
     *
     * Gets the ConstituentJourneyStages for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.ConstituentJourneyStages.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get ConstituentJourneyStages(): Promise<ConstituentJourneyStageData[]> {
        if (this._constituentJourneyStages !== null) {
            return Promise.resolve(this._constituentJourneyStages);
        }

        if (this._constituentJourneyStagesPromise !== null) {
            return this._constituentJourneyStagesPromise;
        }

        // Start the load
        this.loadConstituentJourneyStages();

        return this._constituentJourneyStagesPromise!;
    }



    private loadConstituentJourneyStages(): void {

        this._constituentJourneyStagesPromise = lastValueFrom(
            IconService.Instance.GetConstituentJourneyStagesForIcon(this.id)
        )
        .then(ConstituentJourneyStages => {
            this._constituentJourneyStages = ConstituentJourneyStages ?? [];
            this._constituentJourneyStagesSubject.next(this._constituentJourneyStages);
            return this._constituentJourneyStages;
         })
        .catch(err => {
            this._constituentJourneyStages = [];
            this._constituentJourneyStagesSubject.next(this._constituentJourneyStages);
            throw err;
        })
        .finally(() => {
            this._constituentJourneyStagesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ConstituentJourneyStage. Call after mutations to force refresh.
     */
    public ClearConstituentJourneyStagesCache(): void {
        this._constituentJourneyStages = null;
        this._constituentJourneyStagesPromise = null;
        this._constituentJourneyStagesSubject.next(this._constituentJourneyStages);      // Emit to observable
    }

    public get HasConstituentJourneyStages(): Promise<boolean> {
        return this.ConstituentJourneyStages.then(constituentJourneyStages => constituentJourneyStages.length > 0);
    }


    /**
     *
     * Gets the Constituents for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.Constituents.then(icons => { ... })
     *   or
     *   await this.icon.icons
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
            IconService.Instance.GetConstituentsForIcon(this.id)
        )
        .then(Constituents => {
            this._constituents = Constituents ?? [];
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
     *
     * Gets the Tributes for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.Tributes.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get Tributes(): Promise<TributeData[]> {
        if (this._tributes !== null) {
            return Promise.resolve(this._tributes);
        }

        if (this._tributesPromise !== null) {
            return this._tributesPromise;
        }

        // Start the load
        this.loadTributes();

        return this._tributesPromise!;
    }



    private loadTributes(): void {

        this._tributesPromise = lastValueFrom(
            IconService.Instance.GetTributesForIcon(this.id)
        )
        .then(Tributes => {
            this._tributes = Tributes ?? [];
            this._tributesSubject.next(this._tributes);
            return this._tributes;
         })
        .catch(err => {
            this._tributes = [];
            this._tributesSubject.next(this._tributes);
            throw err;
        })
        .finally(() => {
            this._tributesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached Tribute. Call after mutations to force refresh.
     */
    public ClearTributesCache(): void {
        this._tributes = null;
        this._tributesPromise = null;
        this._tributesSubject.next(this._tributes);      // Emit to observable
    }

    public get HasTributes(): Promise<boolean> {
        return this.Tributes.then(tributes => tributes.length > 0);
    }


    /**
     *
     * Gets the VolunteerProfiles for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.VolunteerProfiles.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get VolunteerProfiles(): Promise<VolunteerProfileData[]> {
        if (this._volunteerProfiles !== null) {
            return Promise.resolve(this._volunteerProfiles);
        }

        if (this._volunteerProfilesPromise !== null) {
            return this._volunteerProfilesPromise;
        }

        // Start the load
        this.loadVolunteerProfiles();

        return this._volunteerProfilesPromise!;
    }



    private loadVolunteerProfiles(): void {

        this._volunteerProfilesPromise = lastValueFrom(
            IconService.Instance.GetVolunteerProfilesForIcon(this.id)
        )
        .then(VolunteerProfiles => {
            this._volunteerProfiles = VolunteerProfiles ?? [];
            this._volunteerProfilesSubject.next(this._volunteerProfiles);
            return this._volunteerProfiles;
         })
        .catch(err => {
            this._volunteerProfiles = [];
            this._volunteerProfilesSubject.next(this._volunteerProfiles);
            throw err;
        })
        .finally(() => {
            this._volunteerProfilesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached VolunteerProfile. Call after mutations to force refresh.
     */
    public ClearVolunteerProfilesCache(): void {
        this._volunteerProfiles = null;
        this._volunteerProfilesPromise = null;
        this._volunteerProfilesSubject.next(this._volunteerProfiles);      // Emit to observable
    }

    public get HasVolunteerProfiles(): Promise<boolean> {
        return this.VolunteerProfiles.then(volunteerProfiles => volunteerProfiles.length > 0);
    }


    /**
     *
     * Gets the VolunteerGroups for this Icon.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.icon.VolunteerGroups.then(icons => { ... })
     *   or
     *   await this.icon.icons
     *
    */
    public get VolunteerGroups(): Promise<VolunteerGroupData[]> {
        if (this._volunteerGroups !== null) {
            return Promise.resolve(this._volunteerGroups);
        }

        if (this._volunteerGroupsPromise !== null) {
            return this._volunteerGroupsPromise;
        }

        // Start the load
        this.loadVolunteerGroups();

        return this._volunteerGroupsPromise!;
    }



    private loadVolunteerGroups(): void {

        this._volunteerGroupsPromise = lastValueFrom(
            IconService.Instance.GetVolunteerGroupsForIcon(this.id)
        )
        .then(VolunteerGroups => {
            this._volunteerGroups = VolunteerGroups ?? [];
            this._volunteerGroupsSubject.next(this._volunteerGroups);
            return this._volunteerGroups;
         })
        .catch(err => {
            this._volunteerGroups = [];
            this._volunteerGroupsSubject.next(this._volunteerGroups);
            throw err;
        })
        .finally(() => {
            this._volunteerGroupsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached VolunteerGroup. Call after mutations to force refresh.
     */
    public ClearVolunteerGroupsCache(): void {
        this._volunteerGroups = null;
        this._volunteerGroupsPromise = null;
        this._volunteerGroupsSubject.next(this._volunteerGroups);      // Emit to observable
    }

    public get HasVolunteerGroups(): Promise<boolean> {
        return this.VolunteerGroups.then(volunteerGroups => volunteerGroups.length > 0);
    }




    /**
     * Updates the state of this IconData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this IconData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): IconSubmitData {
        return IconService.Instance.ConvertToIconSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class IconService extends SecureEndpointBase {

    private static _instance: IconService;
    private listCache: Map<string, Observable<Array<IconData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<IconBasicListData>>>;
    private recordCache: Map<string, Observable<IconData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private resourceTypeService: ResourceTypeService,
        private priorityService: PriorityService,
        private contactMethodService: ContactMethodService,
        private interactionTypeService: InteractionTypeService,
        private tagService: TagService,
        private volunteerStatusService: VolunteerStatusService,
        private contactTypeService: ContactTypeService,
        private contactService: ContactService,
        private relationshipTypeService: RelationshipTypeService,
        private officeTypeService: OfficeTypeService,
        private calendarService: CalendarService,
        private clientTypeService: ClientTypeService,
        private assignmentRoleService: AssignmentRoleService,
        private schedulingTargetTypeService: SchedulingTargetTypeService,
        private crewService: CrewService,
        private crewMemberService: CrewMemberService,
        private fundService: FundService,
        private campaignService: CampaignService,
        private appealService: AppealService,
        private householdService: HouseholdService,
        private constituentJourneyStageService: ConstituentJourneyStageService,
        private constituentService: ConstituentService,
        private tributeService: TributeService,
        private volunteerProfileService: VolunteerProfileService,
        private volunteerGroupService: VolunteerGroupService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<IconData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<IconBasicListData>>>();
        this.recordCache = new Map<string, Observable<IconData>>();

        IconService._instance = this;
    }

    public static get Instance(): IconService {
      return IconService._instance;
    }


    public ClearListCaches(config: IconQueryParameters | null = null) {

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


    public ConvertToIconSubmitData(data: IconData): IconSubmitData {

        let output = new IconSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.fontAwesomeCode = data.fontAwesomeCode;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetIcon(id: bigint | number, includeRelations: boolean = true) : Observable<IconData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const icon$ = this.requestIcon(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Icon", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, icon$);

            return icon$;
        }

        return this.recordCache.get(configHash) as Observable<IconData>;
    }

    private requestIcon(id: bigint | number, includeRelations: boolean = true) : Observable<IconData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<IconData>(this.baseUrl + 'api/Icon/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveIcon(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestIcon(id, includeRelations));
            }));
    }

    public GetIconList(config: IconQueryParameters | any = null) : Observable<Array<IconData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const iconList$ = this.requestIconList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Icon list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, iconList$);

            return iconList$;
        }

        return this.listCache.get(configHash) as Observable<Array<IconData>>;
    }


    private requestIconList(config: IconQueryParameters | any) : Observable <Array<IconData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IconData>>(this.baseUrl + 'api/Icons', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveIconList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestIconList(config));
            }));
    }

    public GetIconsRowCount(config: IconQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const iconsRowCount$ = this.requestIconsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Icons row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, iconsRowCount$);

            return iconsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestIconsRowCount(config: IconQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Icons/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIconsRowCount(config));
            }));
    }

    public GetIconsBasicListData(config: IconQueryParameters | any = null) : Observable<Array<IconBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const iconsBasicListData$ = this.requestIconsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Icons basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, iconsBasicListData$);

            return iconsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<IconBasicListData>>;
    }


    private requestIconsBasicListData(config: IconQueryParameters | any) : Observable<Array<IconBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<IconBasicListData>>(this.baseUrl + 'api/Icons/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestIconsBasicListData(config));
            }));

    }


    public PutIcon(id: bigint | number, icon: IconSubmitData) : Observable<IconData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<IconData>(this.baseUrl + 'api/Icon/' + id.toString(), icon, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIcon(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutIcon(id, icon));
            }));
    }


    public PostIcon(icon: IconSubmitData) : Observable<IconData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<IconData>(this.baseUrl + 'api/Icon', icon, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveIcon(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostIcon(icon));
            }));
    }

  
    public DeleteIcon(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Icon/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteIcon(id));
            }));
    }


    private getConfigHash(config: IconQueryParameters | any): string {

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

    public userIsSchedulerIconReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerIconReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Icons
        //
        if (userIsSchedulerIconReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerIconReader = user.readPermission >= 1;
            } else {
                userIsSchedulerIconReader = false;
            }
        }

        return userIsSchedulerIconReader;
    }


    public userIsSchedulerIconWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerIconWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Icons
        //
        if (userIsSchedulerIconWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerIconWriter = user.writePermission >= 255;
          } else {
            userIsSchedulerIconWriter = false;
          }      
        }

        return userIsSchedulerIconWriter;
    }

    public GetResourceTypesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceTypeData[]> {
        return this.resourceTypeService.GetResourceTypeList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetPrioritiesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<PriorityData[]> {
        return this.priorityService.GetPriorityList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetContactMethodsForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactMethodData[]> {
        return this.contactMethodService.GetContactMethodList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetInteractionTypesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<InteractionTypeData[]> {
        return this.interactionTypeService.GetInteractionTypeList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetTagsForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TagData[]> {
        return this.tagService.GetTagList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetVolunteerStatusesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<VolunteerStatusData[]> {
        return this.volunteerStatusService.GetVolunteerStatusList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetContactTypesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactTypeData[]> {
        return this.contactTypeService.GetContactTypeList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetContactsForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ContactData[]> {
        return this.contactService.GetContactList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetRelationshipTypesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<RelationshipTypeData[]> {
        return this.relationshipTypeService.GetRelationshipTypeList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetOfficeTypesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<OfficeTypeData[]> {
        return this.officeTypeService.GetOfficeTypeList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetCalendarsForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CalendarData[]> {
        return this.calendarService.GetCalendarList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetClientTypesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ClientTypeData[]> {
        return this.clientTypeService.GetClientTypeList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetAssignmentRolesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AssignmentRoleData[]> {
        return this.assignmentRoleService.GetAssignmentRoleList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetTypesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetTypeData[]> {
        return this.schedulingTargetTypeService.GetSchedulingTargetTypeList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetCrewsForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CrewData[]> {
        return this.crewService.GetCrewList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetCrewMembersForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CrewMemberData[]> {
        return this.crewMemberService.GetCrewMemberList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetFundsForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<FundData[]> {
        return this.fundService.GetFundList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetCampaignsForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<CampaignData[]> {
        return this.campaignService.GetCampaignList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetAppealsForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AppealData[]> {
        return this.appealService.GetAppealList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetHouseholdsForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<HouseholdData[]> {
        return this.householdService.GetHouseholdList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConstituentJourneyStagesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConstituentJourneyStageData[]> {
        return this.constituentJourneyStageService.GetConstituentJourneyStageList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetConstituentsForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ConstituentData[]> {
        return this.constituentService.GetConstituentList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetTributesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<TributeData[]> {
        return this.tributeService.GetTributeList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetVolunteerProfilesForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<VolunteerProfileData[]> {
        return this.volunteerProfileService.GetVolunteerProfileList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetVolunteerGroupsForIcon(iconId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<VolunteerGroupData[]> {
        return this.volunteerGroupService.GetVolunteerGroupList({
            iconId: iconId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full IconData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the IconData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when IconTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveIcon(raw: any): IconData {
    if (!raw) return raw;

    //
    // Create a IconData object instance with correct prototype
    //
    const revived = Object.create(IconData.prototype) as IconData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._resourceTypes = null;
    (revived as any)._resourceTypesPromise = null;
    (revived as any)._resourceTypesSubject = new BehaviorSubject<ResourceTypeData[] | null>(null);

    (revived as any)._priorities = null;
    (revived as any)._prioritiesPromise = null;
    (revived as any)._prioritiesSubject = new BehaviorSubject<PriorityData[] | null>(null);

    (revived as any)._contactMethods = null;
    (revived as any)._contactMethodsPromise = null;
    (revived as any)._contactMethodsSubject = new BehaviorSubject<ContactMethodData[] | null>(null);

    (revived as any)._interactionTypes = null;
    (revived as any)._interactionTypesPromise = null;
    (revived as any)._interactionTypesSubject = new BehaviorSubject<InteractionTypeData[] | null>(null);

    (revived as any)._tags = null;
    (revived as any)._tagsPromise = null;
    (revived as any)._tagsSubject = new BehaviorSubject<TagData[] | null>(null);

    (revived as any)._volunteerStatuses = null;
    (revived as any)._volunteerStatusesPromise = null;
    (revived as any)._volunteerStatusesSubject = new BehaviorSubject<VolunteerStatusData[] | null>(null);

    (revived as any)._contactTypes = null;
    (revived as any)._contactTypesPromise = null;
    (revived as any)._contactTypesSubject = new BehaviorSubject<ContactTypeData[] | null>(null);

    (revived as any)._contacts = null;
    (revived as any)._contactsPromise = null;
    (revived as any)._contactsSubject = new BehaviorSubject<ContactData[] | null>(null);

    (revived as any)._relationshipTypes = null;
    (revived as any)._relationshipTypesPromise = null;
    (revived as any)._relationshipTypesSubject = new BehaviorSubject<RelationshipTypeData[] | null>(null);

    (revived as any)._officeTypes = null;
    (revived as any)._officeTypesPromise = null;
    (revived as any)._officeTypesSubject = new BehaviorSubject<OfficeTypeData[] | null>(null);

    (revived as any)._calendars = null;
    (revived as any)._calendarsPromise = null;
    (revived as any)._calendarsSubject = new BehaviorSubject<CalendarData[] | null>(null);

    (revived as any)._clientTypes = null;
    (revived as any)._clientTypesPromise = null;
    (revived as any)._clientTypesSubject = new BehaviorSubject<ClientTypeData[] | null>(null);

    (revived as any)._assignmentRoles = null;
    (revived as any)._assignmentRolesPromise = null;
    (revived as any)._assignmentRolesSubject = new BehaviorSubject<AssignmentRoleData[] | null>(null);

    (revived as any)._schedulingTargetTypes = null;
    (revived as any)._schedulingTargetTypesPromise = null;
    (revived as any)._schedulingTargetTypesSubject = new BehaviorSubject<SchedulingTargetTypeData[] | null>(null);

    (revived as any)._crews = null;
    (revived as any)._crewsPromise = null;
    (revived as any)._crewsSubject = new BehaviorSubject<CrewData[] | null>(null);

    (revived as any)._crewMembers = null;
    (revived as any)._crewMembersPromise = null;
    (revived as any)._crewMembersSubject = new BehaviorSubject<CrewMemberData[] | null>(null);

    (revived as any)._funds = null;
    (revived as any)._fundsPromise = null;
    (revived as any)._fundsSubject = new BehaviorSubject<FundData[] | null>(null);

    (revived as any)._campaigns = null;
    (revived as any)._campaignsPromise = null;
    (revived as any)._campaignsSubject = new BehaviorSubject<CampaignData[] | null>(null);

    (revived as any)._appeals = null;
    (revived as any)._appealsPromise = null;
    (revived as any)._appealsSubject = new BehaviorSubject<AppealData[] | null>(null);

    (revived as any)._households = null;
    (revived as any)._householdsPromise = null;
    (revived as any)._householdsSubject = new BehaviorSubject<HouseholdData[] | null>(null);

    (revived as any)._constituentJourneyStages = null;
    (revived as any)._constituentJourneyStagesPromise = null;
    (revived as any)._constituentJourneyStagesSubject = new BehaviorSubject<ConstituentJourneyStageData[] | null>(null);

    (revived as any)._constituents = null;
    (revived as any)._constituentsPromise = null;
    (revived as any)._constituentsSubject = new BehaviorSubject<ConstituentData[] | null>(null);

    (revived as any)._tributes = null;
    (revived as any)._tributesPromise = null;
    (revived as any)._tributesSubject = new BehaviorSubject<TributeData[] | null>(null);

    (revived as any)._volunteerProfiles = null;
    (revived as any)._volunteerProfilesPromise = null;
    (revived as any)._volunteerProfilesSubject = new BehaviorSubject<VolunteerProfileData[] | null>(null);

    (revived as any)._volunteerGroups = null;
    (revived as any)._volunteerGroupsPromise = null;
    (revived as any)._volunteerGroupsSubject = new BehaviorSubject<VolunteerGroupData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadIconXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).ResourceTypes$ = (revived as any)._resourceTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resourceTypes === null && (revived as any)._resourceTypesPromise === null) {
                (revived as any).loadResourceTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._resourceTypesCount$ = null;


    (revived as any).Priorities$ = (revived as any)._prioritiesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._priorities === null && (revived as any)._prioritiesPromise === null) {
                (revived as any).loadPriorities();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._prioritiesCount$ = null;


    (revived as any).ContactMethods$ = (revived as any)._contactMethodsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactMethods === null && (revived as any)._contactMethodsPromise === null) {
                (revived as any).loadContactMethods();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._contactMethodsCount$ = null;


    (revived as any).InteractionTypes$ = (revived as any)._interactionTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._interactionTypes === null && (revived as any)._interactionTypesPromise === null) {
                (revived as any).loadInteractionTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._interactionTypesCount$ = null;


    (revived as any).Tags$ = (revived as any)._tagsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._tags === null && (revived as any)._tagsPromise === null) {
                (revived as any).loadTags();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._tagsCount$ = null;


    (revived as any).VolunteerStatuses$ = (revived as any)._volunteerStatusesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._volunteerStatuses === null && (revived as any)._volunteerStatusesPromise === null) {
                (revived as any).loadVolunteerStatuses();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._volunteerStatusesCount$ = null;


    (revived as any).ContactTypes$ = (revived as any)._contactTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contactTypes === null && (revived as any)._contactTypesPromise === null) {
                (revived as any).loadContactTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._contactTypesCount$ = null;


    (revived as any).Contacts$ = (revived as any)._contactsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._contacts === null && (revived as any)._contactsPromise === null) {
                (revived as any).loadContacts();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._contactsCount$ = null;


    (revived as any).RelationshipTypes$ = (revived as any)._relationshipTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._relationshipTypes === null && (revived as any)._relationshipTypesPromise === null) {
                (revived as any).loadRelationshipTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._relationshipTypesCount$ = null;


    (revived as any).OfficeTypes$ = (revived as any)._officeTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._officeTypes === null && (revived as any)._officeTypesPromise === null) {
                (revived as any).loadOfficeTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._officeTypesCount$ = null;


    (revived as any).Calendars$ = (revived as any)._calendarsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._calendars === null && (revived as any)._calendarsPromise === null) {
                (revived as any).loadCalendars();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._calendarsCount$ = null;


    (revived as any).ClientTypes$ = (revived as any)._clientTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._clientTypes === null && (revived as any)._clientTypesPromise === null) {
                (revived as any).loadClientTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._clientTypesCount$ = null;


    (revived as any).AssignmentRoles$ = (revived as any)._assignmentRolesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._assignmentRoles === null && (revived as any)._assignmentRolesPromise === null) {
                (revived as any).loadAssignmentRoles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._assignmentRolesCount$ = null;


    (revived as any).SchedulingTargetTypes$ = (revived as any)._schedulingTargetTypesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetTypes === null && (revived as any)._schedulingTargetTypesPromise === null) {
                (revived as any).loadSchedulingTargetTypes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._schedulingTargetTypesCount$ = null;


    (revived as any).Crews$ = (revived as any)._crewsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._crews === null && (revived as any)._crewsPromise === null) {
                (revived as any).loadCrews();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._crewsCount$ = null;


    (revived as any).CrewMembers$ = (revived as any)._crewMembersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._crewMembers === null && (revived as any)._crewMembersPromise === null) {
                (revived as any).loadCrewMembers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._crewMembersCount$ = null;


    (revived as any).Funds$ = (revived as any)._fundsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._funds === null && (revived as any)._fundsPromise === null) {
                (revived as any).loadFunds();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._fundsCount$ = null;


    (revived as any).Campaigns$ = (revived as any)._campaignsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._campaigns === null && (revived as any)._campaignsPromise === null) {
                (revived as any).loadCampaigns();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._campaignsCount$ = null;


    (revived as any).Appeals$ = (revived as any)._appealsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._appeals === null && (revived as any)._appealsPromise === null) {
                (revived as any).loadAppeals();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._appealsCount$ = null;


    (revived as any).Households$ = (revived as any)._householdsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._households === null && (revived as any)._householdsPromise === null) {
                (revived as any).loadHouseholds();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._householdsCount$ = null;


    (revived as any).ConstituentJourneyStages$ = (revived as any)._constituentJourneyStagesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._constituentJourneyStages === null && (revived as any)._constituentJourneyStagesPromise === null) {
                (revived as any).loadConstituentJourneyStages();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._constituentJourneyStagesCount$ = null;


    (revived as any).Constituents$ = (revived as any)._constituentsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._constituents === null && (revived as any)._constituentsPromise === null) {
                (revived as any).loadConstituents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._constituentsCount$ = null;


    (revived as any).Tributes$ = (revived as any)._tributesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._tributes === null && (revived as any)._tributesPromise === null) {
                (revived as any).loadTributes();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._tributesCount$ = null;


    (revived as any).VolunteerProfiles$ = (revived as any)._volunteerProfilesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._volunteerProfiles === null && (revived as any)._volunteerProfilesPromise === null) {
                (revived as any).loadVolunteerProfiles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._volunteerProfilesCount$ = null;


    (revived as any).VolunteerGroups$ = (revived as any)._volunteerGroupsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._volunteerGroups === null && (revived as any)._volunteerGroupsPromise === null) {
                (revived as any).loadVolunteerGroups();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._volunteerGroupsCount$ = null;



    return revived;
  }

  private ReviveIconList(rawList: any[]): IconData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveIcon(raw));
  }

}
