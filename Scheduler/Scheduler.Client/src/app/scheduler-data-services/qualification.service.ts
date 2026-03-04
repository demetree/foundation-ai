/*

   GENERATED SERVICE FOR THE QUALIFICATION TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Qualification table.

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
import { AssignmentRoleQualificationRequirementService, AssignmentRoleQualificationRequirementData } from './assignment-role-qualification-requirement.service';
import { SchedulingTargetQualificationRequirementService, SchedulingTargetQualificationRequirementData } from './scheduling-target-qualification-requirement.service';
import { ResourceQualificationService, ResourceQualificationData } from './resource-qualification.service';
import { ScheduledEventTemplateQualificationRequirementService, ScheduledEventTemplateQualificationRequirementData } from './scheduled-event-template-qualification-requirement.service';
import { ScheduledEventQualificationRequirementService, ScheduledEventQualificationRequirementData } from './scheduled-event-qualification-requirement.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class QualificationQueryParameters {
    name: string | null | undefined = null;
    description: string | null | undefined = null;
    isLicense: boolean | null | undefined = null;
    color: string | null | undefined = null;
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
export class QualificationSubmitData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isLicense: boolean | null = null;
    color: string | null = null;
    sequence: bigint | number | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class QualificationBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. QualificationChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `qualification.QualificationChildren$` — use with `| async` in templates
//        • Promise:    `qualification.QualificationChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="qualification.QualificationChildren$ | async"`), or
//        • Access the promise getter (`qualification.QualificationChildren` or `await qualification.QualificationChildren`)
//    - Simply reading `qualification.QualificationChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await qualification.Reload()` to refresh the entire object and clear all lazy caches.
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
export class QualificationData {
    id!: bigint | number;
    name!: string;
    description!: string;
    isLicense!: boolean | null;
    color!: string | null;
    sequence!: bigint | number;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;

    //
    // Private lazy-loading caches for related collections
    //
    private _assignmentRoleQualificationRequirements: AssignmentRoleQualificationRequirementData[] | null = null;
    private _assignmentRoleQualificationRequirementsPromise: Promise<AssignmentRoleQualificationRequirementData[]> | null  = null;
    private _assignmentRoleQualificationRequirementsSubject = new BehaviorSubject<AssignmentRoleQualificationRequirementData[] | null>(null);

                
    private _schedulingTargetQualificationRequirements: SchedulingTargetQualificationRequirementData[] | null = null;
    private _schedulingTargetQualificationRequirementsPromise: Promise<SchedulingTargetQualificationRequirementData[]> | null  = null;
    private _schedulingTargetQualificationRequirementsSubject = new BehaviorSubject<SchedulingTargetQualificationRequirementData[] | null>(null);

                
    private _resourceQualifications: ResourceQualificationData[] | null = null;
    private _resourceQualificationsPromise: Promise<ResourceQualificationData[]> | null  = null;
    private _resourceQualificationsSubject = new BehaviorSubject<ResourceQualificationData[] | null>(null);

                
    private _scheduledEventTemplateQualificationRequirements: ScheduledEventTemplateQualificationRequirementData[] | null = null;
    private _scheduledEventTemplateQualificationRequirementsPromise: Promise<ScheduledEventTemplateQualificationRequirementData[]> | null  = null;
    private _scheduledEventTemplateQualificationRequirementsSubject = new BehaviorSubject<ScheduledEventTemplateQualificationRequirementData[] | null>(null);

                
    private _scheduledEventQualificationRequirements: ScheduledEventQualificationRequirementData[] | null = null;
    private _scheduledEventQualificationRequirementsPromise: Promise<ScheduledEventQualificationRequirementData[]> | null  = null;
    private _scheduledEventQualificationRequirementsSubject = new BehaviorSubject<ScheduledEventQualificationRequirementData[] | null>(null);

                

    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public AssignmentRoleQualificationRequirements$ = this._assignmentRoleQualificationRequirementsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._assignmentRoleQualificationRequirements === null && this._assignmentRoleQualificationRequirementsPromise === null) {
            this.loadAssignmentRoleQualificationRequirements(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _assignmentRoleQualificationRequirementsCount$: Observable<bigint | number> | null = null;
    public get AssignmentRoleQualificationRequirementsCount$(): Observable<bigint | number> {
        if (this._assignmentRoleQualificationRequirementsCount$ === null) {
            this._assignmentRoleQualificationRequirementsCount$ = AssignmentRoleQualificationRequirementService.Instance.GetAssignmentRoleQualificationRequirementsRowCount({qualificationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._assignmentRoleQualificationRequirementsCount$;
    }



    public SchedulingTargetQualificationRequirements$ = this._schedulingTargetQualificationRequirementsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._schedulingTargetQualificationRequirements === null && this._schedulingTargetQualificationRequirementsPromise === null) {
            this.loadSchedulingTargetQualificationRequirements(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _schedulingTargetQualificationRequirementsCount$: Observable<bigint | number> | null = null;
    public get SchedulingTargetQualificationRequirementsCount$(): Observable<bigint | number> {
        if (this._schedulingTargetQualificationRequirementsCount$ === null) {
            this._schedulingTargetQualificationRequirementsCount$ = SchedulingTargetQualificationRequirementService.Instance.GetSchedulingTargetQualificationRequirementsRowCount({qualificationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._schedulingTargetQualificationRequirementsCount$;
    }



    public ResourceQualifications$ = this._resourceQualificationsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._resourceQualifications === null && this._resourceQualificationsPromise === null) {
            this.loadResourceQualifications(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _resourceQualificationsCount$: Observable<bigint | number> | null = null;
    public get ResourceQualificationsCount$(): Observable<bigint | number> {
        if (this._resourceQualificationsCount$ === null) {
            this._resourceQualificationsCount$ = ResourceQualificationService.Instance.GetResourceQualificationsRowCount({qualificationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._resourceQualificationsCount$;
    }



    public ScheduledEventTemplateQualificationRequirements$ = this._scheduledEventTemplateQualificationRequirementsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventTemplateQualificationRequirements === null && this._scheduledEventTemplateQualificationRequirementsPromise === null) {
            this.loadScheduledEventTemplateQualificationRequirements(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _scheduledEventTemplateQualificationRequirementsCount$: Observable<bigint | number> | null = null;
    public get ScheduledEventTemplateQualificationRequirementsCount$(): Observable<bigint | number> {
        if (this._scheduledEventTemplateQualificationRequirementsCount$ === null) {
            this._scheduledEventTemplateQualificationRequirementsCount$ = ScheduledEventTemplateQualificationRequirementService.Instance.GetScheduledEventTemplateQualificationRequirementsRowCount({qualificationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._scheduledEventTemplateQualificationRequirementsCount$;
    }



    public ScheduledEventQualificationRequirements$ = this._scheduledEventQualificationRequirementsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._scheduledEventQualificationRequirements === null && this._scheduledEventQualificationRequirementsPromise === null) {
            this.loadScheduledEventQualificationRequirements(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );


    private _scheduledEventQualificationRequirementsCount$: Observable<bigint | number> | null = null;
    public get ScheduledEventQualificationRequirementsCount$(): Observable<bigint | number> {
        if (this._scheduledEventQualificationRequirementsCount$ === null) {
            this._scheduledEventQualificationRequirementsCount$ = ScheduledEventQualificationRequirementService.Instance.GetScheduledEventQualificationRequirementsRowCount({qualificationId: this.id,
              active: true,
              deleted: false
            });
        }
        return this._scheduledEventQualificationRequirementsCount$;
    }




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any QualificationData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.qualification.Reload();
  //
  //  Non Async:
  //
  //     qualification[0].Reload().then(x => {
  //        this.qualification = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      QualificationService.Instance.GetQualification(this.id, includeRelations)
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
     this._assignmentRoleQualificationRequirements = null;
     this._assignmentRoleQualificationRequirementsPromise = null;
     this._assignmentRoleQualificationRequirementsSubject.next(null);
     this._assignmentRoleQualificationRequirementsCount$ = null;

     this._schedulingTargetQualificationRequirements = null;
     this._schedulingTargetQualificationRequirementsPromise = null;
     this._schedulingTargetQualificationRequirementsSubject.next(null);
     this._schedulingTargetQualificationRequirementsCount$ = null;

     this._resourceQualifications = null;
     this._resourceQualificationsPromise = null;
     this._resourceQualificationsSubject.next(null);
     this._resourceQualificationsCount$ = null;

     this._scheduledEventTemplateQualificationRequirements = null;
     this._scheduledEventTemplateQualificationRequirementsPromise = null;
     this._scheduledEventTemplateQualificationRequirementsSubject.next(null);
     this._scheduledEventTemplateQualificationRequirementsCount$ = null;

     this._scheduledEventQualificationRequirements = null;
     this._scheduledEventQualificationRequirementsPromise = null;
     this._scheduledEventQualificationRequirementsSubject.next(null);
     this._scheduledEventQualificationRequirementsCount$ = null;

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the AssignmentRoleQualificationRequirements for this Qualification.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.qualification.AssignmentRoleQualificationRequirements.then(qualifications => { ... })
     *   or
     *   await this.qualification.qualifications
     *
    */
    public get AssignmentRoleQualificationRequirements(): Promise<AssignmentRoleQualificationRequirementData[]> {
        if (this._assignmentRoleQualificationRequirements !== null) {
            return Promise.resolve(this._assignmentRoleQualificationRequirements);
        }

        if (this._assignmentRoleQualificationRequirementsPromise !== null) {
            return this._assignmentRoleQualificationRequirementsPromise;
        }

        // Start the load
        this.loadAssignmentRoleQualificationRequirements();

        return this._assignmentRoleQualificationRequirementsPromise!;
    }



    private loadAssignmentRoleQualificationRequirements(): void {

        this._assignmentRoleQualificationRequirementsPromise = lastValueFrom(
            QualificationService.Instance.GetAssignmentRoleQualificationRequirementsForQualification(this.id)
        )
        .then(AssignmentRoleQualificationRequirements => {
            this._assignmentRoleQualificationRequirements = AssignmentRoleQualificationRequirements ?? [];
            this._assignmentRoleQualificationRequirementsSubject.next(this._assignmentRoleQualificationRequirements);
            return this._assignmentRoleQualificationRequirements;
         })
        .catch(err => {
            this._assignmentRoleQualificationRequirements = [];
            this._assignmentRoleQualificationRequirementsSubject.next(this._assignmentRoleQualificationRequirements);
            throw err;
        })
        .finally(() => {
            this._assignmentRoleQualificationRequirementsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached AssignmentRoleQualificationRequirement. Call after mutations to force refresh.
     */
    public ClearAssignmentRoleQualificationRequirementsCache(): void {
        this._assignmentRoleQualificationRequirements = null;
        this._assignmentRoleQualificationRequirementsPromise = null;
        this._assignmentRoleQualificationRequirementsSubject.next(this._assignmentRoleQualificationRequirements);      // Emit to observable
    }

    public get HasAssignmentRoleQualificationRequirements(): Promise<boolean> {
        return this.AssignmentRoleQualificationRequirements.then(assignmentRoleQualificationRequirements => assignmentRoleQualificationRequirements.length > 0);
    }


    /**
     *
     * Gets the SchedulingTargetQualificationRequirements for this Qualification.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.qualification.SchedulingTargetQualificationRequirements.then(qualifications => { ... })
     *   or
     *   await this.qualification.qualifications
     *
    */
    public get SchedulingTargetQualificationRequirements(): Promise<SchedulingTargetQualificationRequirementData[]> {
        if (this._schedulingTargetQualificationRequirements !== null) {
            return Promise.resolve(this._schedulingTargetQualificationRequirements);
        }

        if (this._schedulingTargetQualificationRequirementsPromise !== null) {
            return this._schedulingTargetQualificationRequirementsPromise;
        }

        // Start the load
        this.loadSchedulingTargetQualificationRequirements();

        return this._schedulingTargetQualificationRequirementsPromise!;
    }



    private loadSchedulingTargetQualificationRequirements(): void {

        this._schedulingTargetQualificationRequirementsPromise = lastValueFrom(
            QualificationService.Instance.GetSchedulingTargetQualificationRequirementsForQualification(this.id)
        )
        .then(SchedulingTargetQualificationRequirements => {
            this._schedulingTargetQualificationRequirements = SchedulingTargetQualificationRequirements ?? [];
            this._schedulingTargetQualificationRequirementsSubject.next(this._schedulingTargetQualificationRequirements);
            return this._schedulingTargetQualificationRequirements;
         })
        .catch(err => {
            this._schedulingTargetQualificationRequirements = [];
            this._schedulingTargetQualificationRequirementsSubject.next(this._schedulingTargetQualificationRequirements);
            throw err;
        })
        .finally(() => {
            this._schedulingTargetQualificationRequirementsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached SchedulingTargetQualificationRequirement. Call after mutations to force refresh.
     */
    public ClearSchedulingTargetQualificationRequirementsCache(): void {
        this._schedulingTargetQualificationRequirements = null;
        this._schedulingTargetQualificationRequirementsPromise = null;
        this._schedulingTargetQualificationRequirementsSubject.next(this._schedulingTargetQualificationRequirements);      // Emit to observable
    }

    public get HasSchedulingTargetQualificationRequirements(): Promise<boolean> {
        return this.SchedulingTargetQualificationRequirements.then(schedulingTargetQualificationRequirements => schedulingTargetQualificationRequirements.length > 0);
    }


    /**
     *
     * Gets the ResourceQualifications for this Qualification.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.qualification.ResourceQualifications.then(qualifications => { ... })
     *   or
     *   await this.qualification.qualifications
     *
    */
    public get ResourceQualifications(): Promise<ResourceQualificationData[]> {
        if (this._resourceQualifications !== null) {
            return Promise.resolve(this._resourceQualifications);
        }

        if (this._resourceQualificationsPromise !== null) {
            return this._resourceQualificationsPromise;
        }

        // Start the load
        this.loadResourceQualifications();

        return this._resourceQualificationsPromise!;
    }



    private loadResourceQualifications(): void {

        this._resourceQualificationsPromise = lastValueFrom(
            QualificationService.Instance.GetResourceQualificationsForQualification(this.id)
        )
        .then(ResourceQualifications => {
            this._resourceQualifications = ResourceQualifications ?? [];
            this._resourceQualificationsSubject.next(this._resourceQualifications);
            return this._resourceQualifications;
         })
        .catch(err => {
            this._resourceQualifications = [];
            this._resourceQualificationsSubject.next(this._resourceQualifications);
            throw err;
        })
        .finally(() => {
            this._resourceQualificationsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ResourceQualification. Call after mutations to force refresh.
     */
    public ClearResourceQualificationsCache(): void {
        this._resourceQualifications = null;
        this._resourceQualificationsPromise = null;
        this._resourceQualificationsSubject.next(this._resourceQualifications);      // Emit to observable
    }

    public get HasResourceQualifications(): Promise<boolean> {
        return this.ResourceQualifications.then(resourceQualifications => resourceQualifications.length > 0);
    }


    /**
     *
     * Gets the ScheduledEventTemplateQualificationRequirements for this Qualification.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.qualification.ScheduledEventTemplateQualificationRequirements.then(qualifications => { ... })
     *   or
     *   await this.qualification.qualifications
     *
    */
    public get ScheduledEventTemplateQualificationRequirements(): Promise<ScheduledEventTemplateQualificationRequirementData[]> {
        if (this._scheduledEventTemplateQualificationRequirements !== null) {
            return Promise.resolve(this._scheduledEventTemplateQualificationRequirements);
        }

        if (this._scheduledEventTemplateQualificationRequirementsPromise !== null) {
            return this._scheduledEventTemplateQualificationRequirementsPromise;
        }

        // Start the load
        this.loadScheduledEventTemplateQualificationRequirements();

        return this._scheduledEventTemplateQualificationRequirementsPromise!;
    }



    private loadScheduledEventTemplateQualificationRequirements(): void {

        this._scheduledEventTemplateQualificationRequirementsPromise = lastValueFrom(
            QualificationService.Instance.GetScheduledEventTemplateQualificationRequirementsForQualification(this.id)
        )
        .then(ScheduledEventTemplateQualificationRequirements => {
            this._scheduledEventTemplateQualificationRequirements = ScheduledEventTemplateQualificationRequirements ?? [];
            this._scheduledEventTemplateQualificationRequirementsSubject.next(this._scheduledEventTemplateQualificationRequirements);
            return this._scheduledEventTemplateQualificationRequirements;
         })
        .catch(err => {
            this._scheduledEventTemplateQualificationRequirements = [];
            this._scheduledEventTemplateQualificationRequirementsSubject.next(this._scheduledEventTemplateQualificationRequirements);
            throw err;
        })
        .finally(() => {
            this._scheduledEventTemplateQualificationRequirementsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventTemplateQualificationRequirement. Call after mutations to force refresh.
     */
    public ClearScheduledEventTemplateQualificationRequirementsCache(): void {
        this._scheduledEventTemplateQualificationRequirements = null;
        this._scheduledEventTemplateQualificationRequirementsPromise = null;
        this._scheduledEventTemplateQualificationRequirementsSubject.next(this._scheduledEventTemplateQualificationRequirements);      // Emit to observable
    }

    public get HasScheduledEventTemplateQualificationRequirements(): Promise<boolean> {
        return this.ScheduledEventTemplateQualificationRequirements.then(scheduledEventTemplateQualificationRequirements => scheduledEventTemplateQualificationRequirements.length > 0);
    }


    /**
     *
     * Gets the ScheduledEventQualificationRequirements for this Qualification.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.qualification.ScheduledEventQualificationRequirements.then(qualifications => { ... })
     *   or
     *   await this.qualification.qualifications
     *
    */
    public get ScheduledEventQualificationRequirements(): Promise<ScheduledEventQualificationRequirementData[]> {
        if (this._scheduledEventQualificationRequirements !== null) {
            return Promise.resolve(this._scheduledEventQualificationRequirements);
        }

        if (this._scheduledEventQualificationRequirementsPromise !== null) {
            return this._scheduledEventQualificationRequirementsPromise;
        }

        // Start the load
        this.loadScheduledEventQualificationRequirements();

        return this._scheduledEventQualificationRequirementsPromise!;
    }



    private loadScheduledEventQualificationRequirements(): void {

        this._scheduledEventQualificationRequirementsPromise = lastValueFrom(
            QualificationService.Instance.GetScheduledEventQualificationRequirementsForQualification(this.id)
        )
        .then(ScheduledEventQualificationRequirements => {
            this._scheduledEventQualificationRequirements = ScheduledEventQualificationRequirements ?? [];
            this._scheduledEventQualificationRequirementsSubject.next(this._scheduledEventQualificationRequirements);
            return this._scheduledEventQualificationRequirements;
         })
        .catch(err => {
            this._scheduledEventQualificationRequirements = [];
            this._scheduledEventQualificationRequirementsSubject.next(this._scheduledEventQualificationRequirements);
            throw err;
        })
        .finally(() => {
            this._scheduledEventQualificationRequirementsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached ScheduledEventQualificationRequirement. Call after mutations to force refresh.
     */
    public ClearScheduledEventQualificationRequirementsCache(): void {
        this._scheduledEventQualificationRequirements = null;
        this._scheduledEventQualificationRequirementsPromise = null;
        this._scheduledEventQualificationRequirementsSubject.next(this._scheduledEventQualificationRequirements);      // Emit to observable
    }

    public get HasScheduledEventQualificationRequirements(): Promise<boolean> {
        return this.ScheduledEventQualificationRequirements.then(scheduledEventQualificationRequirements => scheduledEventQualificationRequirements.length > 0);
    }




    /**
     * Updates the state of this QualificationData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this QualificationData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): QualificationSubmitData {
        return QualificationService.Instance.ConvertToQualificationSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class QualificationService extends SecureEndpointBase {

    private static _instance: QualificationService;
    private listCache: Map<string, Observable<Array<QualificationData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<QualificationBasicListData>>>;
    private recordCache: Map<string, Observable<QualificationData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private assignmentRoleQualificationRequirementService: AssignmentRoleQualificationRequirementService,
        private schedulingTargetQualificationRequirementService: SchedulingTargetQualificationRequirementService,
        private resourceQualificationService: ResourceQualificationService,
        private scheduledEventTemplateQualificationRequirementService: ScheduledEventTemplateQualificationRequirementService,
        private scheduledEventQualificationRequirementService: ScheduledEventQualificationRequirementService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<QualificationData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<QualificationBasicListData>>>();
        this.recordCache = new Map<string, Observable<QualificationData>>();

        QualificationService._instance = this;
    }

    public static get Instance(): QualificationService {
      return QualificationService._instance;
    }


    public ClearListCaches(config: QualificationQueryParameters | null = null) {

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


    public ConvertToQualificationSubmitData(data: QualificationData): QualificationSubmitData {

        let output = new QualificationSubmitData();

        output.id = data.id;
        output.name = data.name;
        output.description = data.description;
        output.isLicense = data.isLicense;
        output.color = data.color;
        output.sequence = data.sequence;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetQualification(id: bigint | number, includeRelations: boolean = true) : Observable<QualificationData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const qualification$ = this.requestQualification(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Qualification", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, qualification$);

            return qualification$;
        }

        return this.recordCache.get(configHash) as Observable<QualificationData>;
    }

    private requestQualification(id: bigint | number, includeRelations: boolean = true) : Observable<QualificationData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<QualificationData>(this.baseUrl + 'api/Qualification/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveQualification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestQualification(id, includeRelations));
            }));
    }

    public GetQualificationList(config: QualificationQueryParameters | any = null) : Observable<Array<QualificationData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const qualificationList$ = this.requestQualificationList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Qualification list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, qualificationList$);

            return qualificationList$;
        }

        return this.listCache.get(configHash) as Observable<Array<QualificationData>>;
    }


    private requestQualificationList(config: QualificationQueryParameters | any) : Observable <Array<QualificationData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<QualificationData>>(this.baseUrl + 'api/Qualifications', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveQualificationList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestQualificationList(config));
            }));
    }

    public GetQualificationsRowCount(config: QualificationQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const qualificationsRowCount$ = this.requestQualificationsRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get Qualifications row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, qualificationsRowCount$);

            return qualificationsRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestQualificationsRowCount(config: QualificationQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/Qualifications/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestQualificationsRowCount(config));
            }));
    }

    public GetQualificationsBasicListData(config: QualificationQueryParameters | any = null) : Observable<Array<QualificationBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const qualificationsBasicListData$ = this.requestQualificationsBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get Qualifications basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, qualificationsBasicListData$);

            return qualificationsBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<QualificationBasicListData>>;
    }


    private requestQualificationsBasicListData(config: QualificationQueryParameters | any) : Observable<Array<QualificationBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<QualificationBasicListData>>(this.baseUrl + 'api/Qualifications/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestQualificationsBasicListData(config));
            }));

    }


    public PutQualification(id: bigint | number, qualification: QualificationSubmitData) : Observable<QualificationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<QualificationData>(this.baseUrl + 'api/Qualification/' + id.toString(), qualification, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveQualification(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutQualification(id, qualification));
            }));
    }


    public PostQualification(qualification: QualificationSubmitData) : Observable<QualificationData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<QualificationData>(this.baseUrl + 'api/Qualification', qualification, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveQualification(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostQualification(qualification));
            }));
    }

  
    public DeleteQualification(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/Qualification/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteQualification(id));
            }));
    }


    private getConfigHash(config: QualificationQueryParameters | any): string {

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

    public userIsSchedulerQualificationReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSchedulerQualificationReader = this.authService.isSchedulerReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Scheduler.Qualifications
        //
        if (userIsSchedulerQualificationReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSchedulerQualificationReader = user.readPermission >= 1;
            } else {
                userIsSchedulerQualificationReader = false;
            }
        }

        return userIsSchedulerQualificationReader;
    }


    public userIsSchedulerQualificationWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSchedulerQualificationWriter = this.authService.isSchedulerReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Scheduler.Qualifications
        //
        if (userIsSchedulerQualificationWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSchedulerQualificationWriter = user.writePermission >= 50;
          } else {
            userIsSchedulerQualificationWriter = false;
          }      
        }

        return userIsSchedulerQualificationWriter;
    }

    public GetAssignmentRoleQualificationRequirementsForQualification(qualificationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<AssignmentRoleQualificationRequirementData[]> {
        return this.assignmentRoleQualificationRequirementService.GetAssignmentRoleQualificationRequirementList({
            qualificationId: qualificationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSchedulingTargetQualificationRequirementsForQualification(qualificationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SchedulingTargetQualificationRequirementData[]> {
        return this.schedulingTargetQualificationRequirementService.GetSchedulingTargetQualificationRequirementList({
            qualificationId: qualificationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetResourceQualificationsForQualification(qualificationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ResourceQualificationData[]> {
        return this.resourceQualificationService.GetResourceQualificationList({
            qualificationId: qualificationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventTemplateQualificationRequirementsForQualification(qualificationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventTemplateQualificationRequirementData[]> {
        return this.scheduledEventTemplateQualificationRequirementService.GetScheduledEventTemplateQualificationRequirementList({
            qualificationId: qualificationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetScheduledEventQualificationRequirementsForQualification(qualificationId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<ScheduledEventQualificationRequirementData[]> {
        return this.scheduledEventQualificationRequirementService.GetScheduledEventQualificationRequirementList({
            qualificationId: qualificationId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full QualificationData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the QualificationData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when QualificationTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveQualification(raw: any): QualificationData {
    if (!raw) return raw;

    //
    // Create a QualificationData object instance with correct prototype
    //
    const revived = Object.create(QualificationData.prototype) as QualificationData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._assignmentRoleQualificationRequirements = null;
    (revived as any)._assignmentRoleQualificationRequirementsPromise = null;
    (revived as any)._assignmentRoleQualificationRequirementsSubject = new BehaviorSubject<AssignmentRoleQualificationRequirementData[] | null>(null);

    (revived as any)._schedulingTargetQualificationRequirements = null;
    (revived as any)._schedulingTargetQualificationRequirementsPromise = null;
    (revived as any)._schedulingTargetQualificationRequirementsSubject = new BehaviorSubject<SchedulingTargetQualificationRequirementData[] | null>(null);

    (revived as any)._resourceQualifications = null;
    (revived as any)._resourceQualificationsPromise = null;
    (revived as any)._resourceQualificationsSubject = new BehaviorSubject<ResourceQualificationData[] | null>(null);

    (revived as any)._scheduledEventTemplateQualificationRequirements = null;
    (revived as any)._scheduledEventTemplateQualificationRequirementsPromise = null;
    (revived as any)._scheduledEventTemplateQualificationRequirementsSubject = new BehaviorSubject<ScheduledEventTemplateQualificationRequirementData[] | null>(null);

    (revived as any)._scheduledEventQualificationRequirements = null;
    (revived as any)._scheduledEventQualificationRequirementsPromise = null;
    (revived as any)._scheduledEventQualificationRequirementsSubject = new BehaviorSubject<ScheduledEventQualificationRequirementData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadQualificationXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).AssignmentRoleQualificationRequirements$ = (revived as any)._assignmentRoleQualificationRequirementsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._assignmentRoleQualificationRequirements === null && (revived as any)._assignmentRoleQualificationRequirementsPromise === null) {
                (revived as any).loadAssignmentRoleQualificationRequirements();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._assignmentRoleQualificationRequirementsCount$ = null;


    (revived as any).SchedulingTargetQualificationRequirements$ = (revived as any)._schedulingTargetQualificationRequirementsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._schedulingTargetQualificationRequirements === null && (revived as any)._schedulingTargetQualificationRequirementsPromise === null) {
                (revived as any).loadSchedulingTargetQualificationRequirements();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._schedulingTargetQualificationRequirementsCount$ = null;


    (revived as any).ResourceQualifications$ = (revived as any)._resourceQualificationsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._resourceQualifications === null && (revived as any)._resourceQualificationsPromise === null) {
                (revived as any).loadResourceQualifications();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._resourceQualificationsCount$ = null;


    (revived as any).ScheduledEventTemplateQualificationRequirements$ = (revived as any)._scheduledEventTemplateQualificationRequirementsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventTemplateQualificationRequirements === null && (revived as any)._scheduledEventTemplateQualificationRequirementsPromise === null) {
                (revived as any).loadScheduledEventTemplateQualificationRequirements();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._scheduledEventTemplateQualificationRequirementsCount$ = null;


    (revived as any).ScheduledEventQualificationRequirements$ = (revived as any)._scheduledEventQualificationRequirementsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._scheduledEventQualificationRequirements === null && (revived as any)._scheduledEventQualificationRequirementsPromise === null) {
                (revived as any).loadScheduledEventQualificationRequirements();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any)._scheduledEventQualificationRequirementsCount$ = null;



    return revived;
  }

  private ReviveQualificationList(rawList: any[]): QualificationData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveQualification(raw));
  }

}
