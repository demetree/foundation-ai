import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, BehaviorSubject, catchError, throwError, lastValueFrom, map  } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';
import { SecurityUserTitleData } from './security-user-title.service';
import { SecurityTenantData } from './security-tenant.service';
import { SecurityOrganizationData } from './security-organization.service';
import { SecurityDepartmentData } from './security-department.service';
import { SecurityTeamData } from './security-team.service';
import { SecurityTenantUserService, SecurityTenantUserData } from './security-tenant-user.service';
import { SecurityOrganizationUserService, SecurityOrganizationUserData } from './security-organization-user.service';
import { SecurityDepartmentUserService, SecurityDepartmentUserData } from './security-department-user.service';
import { SecurityTeamUserService, SecurityTeamUserData } from './security-team-user.service';
import { SecurityUserEventService, SecurityUserEventData } from './security-user-event.service';
import { SecurityUserPasswordResetTokenService, SecurityUserPasswordResetTokenData } from './security-user-password-reset-token.service';
import { SecurityUserSecurityGroupService, SecurityUserSecurityGroupData } from './security-user-security-group.service';
import { SecurityUserSecurityRoleService, SecurityUserSecurityRoleData } from './security-user-security-role.service';
import { EntityDataTokenService, EntityDataTokenData } from './entity-data-token.service';

const SHARE_REPLAY_CACHE_SIZE = 1;           // To cache the last emit
//
// This class defines the query parameters used for GET API endpoints that return arrays
//
// - Use `QueryParameters` for type-safe queries (e.g., `{ name: 'Test', pageSize: 10 }`).
// - Arbitrary objects are supported but should contain simple values(strings, numbers, booleans) to ensure consistent caching.
// - Avoid passing nested objects or arrays, as they may not serialize correctly.
// - Dates are typed as strings because the server requires ISO UTC dates.  The Javascript date object does not naturally construct with that input.  The string format used in 'Date' fields is to be ISO 8601, including millisconds.  For example, 2025-12-09T01:09:27.093Z
//
export class SecurityUserQueryParameters {
    accountName: string | null | undefined = null;
    activeDirectoryAccount: boolean | null | undefined = null;
    canLogin: boolean | null | undefined = null;
    mustChangePassword: boolean | null | undefined = null;
    firstName: string | null | undefined = null;
    middleName: string | null | undefined = null;
    lastName: string | null | undefined = null;
    dateOfBirth: string | null | undefined = null;        // ISO 8601
    emailAddress: string | null | undefined = null;
    cellPhoneNumber: string | null | undefined = null;
    phoneNumber: string | null | undefined = null;
    phoneExtension: string | null | undefined = null;
    description: string | null | undefined = null;
    securityUserTitleId: bigint | number | null | undefined = null;
    reportsToSecurityUserId: bigint | number | null | undefined = null;
    authenticationDomain: string | null | undefined = null;
    failedLoginCount: bigint | number | null | undefined = null;
    lastLoginAttempt: string | null | undefined = null;        // ISO 8601
    mostRecentActivity: string | null | undefined = null;        // ISO 8601
    alternateIdentifier: string | null | undefined = null;
    settings: string | null | undefined = null;
    securityTenantId: bigint | number | null | undefined = null;
    readPermissionLevel: bigint | number | null | undefined = null;
    writePermissionLevel: bigint | number | null | undefined = null;
    securityOrganizationId: bigint | number | null | undefined = null;
    securityDepartmentId: bigint | number | null | undefined = null;
    securityTeamId: bigint | number | null | undefined = null;
    authenticationToken: string | null | undefined = null;
    authenticationTokenExpiry: string | null | undefined = null;        // ISO 8601
    twoFactorToken: string | null | undefined = null;
    twoFactorTokenExpiry: string | null | undefined = null;        // ISO 8601
    twoFactorSendByEmail: boolean | null | undefined = null;
    twoFactorSendBySMS: boolean | null | undefined = null;
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
export class SecurityUserSubmitData {
    id!: bigint | number;
    accountName!: string;
    activeDirectoryAccount!: boolean;
    canLogin!: boolean;
    mustChangePassword!: boolean;
    firstName: string | null = null;
    middleName: string | null = null;
    lastName: string | null = null;
    dateOfBirth: string | null = null;     // ISO 8601
    emailAddress: string | null = null;
    cellPhoneNumber: string | null = null;
    phoneNumber: string | null = null;
    phoneExtension: string | null = null;
    description: string | null = null;
    securityUserTitleId: bigint | number | null = null;
    reportsToSecurityUserId: bigint | number | null = null;
    authenticationDomain: string | null = null;
    failedLoginCount: bigint | number | null = null;
    lastLoginAttempt: string | null = null;     // ISO 8601
    mostRecentActivity: string | null = null;     // ISO 8601
    alternateIdentifier: string | null = null;
    image: string | null = null;
    settings: string | null = null;
    securityTenantId: bigint | number | null = null;
    readPermissionLevel!: bigint | number;
    writePermissionLevel!: bigint | number;
    securityOrganizationId: bigint | number | null = null;
    securityDepartmentId: bigint | number | null = null;
    securityTeamId: bigint | number | null = null;
    authenticationToken: string | null = null;
    authenticationTokenExpiry: string | null = null;     // ISO 8601
    twoFactorToken: string | null = null;
    twoFactorTokenExpiry: string | null = null;     // ISO 8601
    twoFactorSendByEmail: boolean | null = null;
    twoFactorSendBySMS: boolean | null = null;
    active!: boolean;
    deleted!: boolean;
}


export class SecurityUserBasicListData {
  id!: bigint | number;
  name!: string;
}




//
// Core model returned from the server.
//
// Key design notes:
//
// 1. **Lazy loading of related collections**:
//    - Each related collection (e.g. SecurityUserChildren) is loaded on-demand.
//    - Two access patterns are provided:
//        • Observable: `securityUser.SecurityUserChildren$` — use with `| async` in templates
//        • Promise:    `securityUser.SecurityUserChildren`  — use with `await` or `.then()` in code
//
// 2. **How lazy loading works**:
//    - The observable has a `tap()` that checks if data is already loaded.
//    - On first subscription, it triggers the private `loadX()` method.
//    - The promise getter does the same check and starts the load if needed.
//
// 3. **Important usage rule**:
//    - To trigger loading, you must either:
//        • Subscribe to the `$` observable (e.g., via `*ngIf="securityUser.SecurityUserChildren$ | async"`), or
//        • Access the promise getter (`securityUser.SecurityUserChildren` or `await securityUser.SecurityUserChildren`)
//    - Simply reading `securityUser.SecurityUserChildren` without awaiting does **not** trigger load.
//
// 4. **Reload()**:
//    - Call `await securityUser.Reload()` to refresh the entire object and clear all lazy caches.
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
export class SecurityUserData {
    id!: bigint | number;
    accountName!: string;
    activeDirectoryAccount!: boolean;
    password!: string | null;
    canLogin!: boolean;
    mustChangePassword!: boolean;
    firstName!: string | null;
    middleName!: string | null;
    lastName!: string | null;
    dateOfBirth!: string | null;   // ISO 8601
    emailAddress!: string | null;
    cellPhoneNumber!: string | null;
    phoneNumber!: string | null;
    phoneExtension!: string | null;
    description!: string | null;
    securityUserTitleId!: bigint | number;
    reportsToSecurityUserId!: bigint | number;
    authenticationDomain!: string | null;
    failedLoginCount!: bigint | number;
    lastLoginAttempt!: string | null;   // ISO 8601
    mostRecentActivity!: string | null;   // ISO 8601
    alternateIdentifier!: string | null;
    image!: string | null;
    settings!: string | null;
    securityTenantId!: bigint | number;
    readPermissionLevel!: bigint | number;
    writePermissionLevel!: bigint | number;
    securityOrganizationId!: bigint | number;
    securityDepartmentId!: bigint | number;
    securityTeamId!: bigint | number;
    authenticationToken!: string | null;
    authenticationTokenExpiry!: string | null;   // ISO 8601
    twoFactorToken!: string | null;
    twoFactorTokenExpiry!: string | null;   // ISO 8601
    twoFactorSendByEmail!: boolean | null;
    twoFactorSendBySMS!: boolean | null;
    objectGuid!: string;
    active!: boolean;
    deleted!: boolean;
    reportsToSecurityUser: any | null | undefined = null;            // Navigation property (populated when includeRelations=true)
    securityDepartment: SecurityDepartmentData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    securityOrganization: SecurityOrganizationData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    securityTeam: SecurityTeamData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    securityTenant: SecurityTenantData | null | undefined = null;          // Navigation property (populated when includeRelations=true)
    securityUserTitle: SecurityUserTitleData | null | undefined = null;          // Navigation property (populated when includeRelations=true)

    //
    // Private lazy-loading caches for related collections
    //
    private _securityTenantUsers: SecurityTenantUserData[] | null = null;
    private _securityTenantUsersPromise: Promise<SecurityTenantUserData[]> | null  = null;
    private _securityTenantUsersSubject = new BehaviorSubject<SecurityTenantUserData[] | null>(null);

    private _securityOrganizationUsers: SecurityOrganizationUserData[] | null = null;
    private _securityOrganizationUsersPromise: Promise<SecurityOrganizationUserData[]> | null  = null;
    private _securityOrganizationUsersSubject = new BehaviorSubject<SecurityOrganizationUserData[] | null>(null);

    private _securityDepartmentUsers: SecurityDepartmentUserData[] | null = null;
    private _securityDepartmentUsersPromise: Promise<SecurityDepartmentUserData[]> | null  = null;
    private _securityDepartmentUsersSubject = new BehaviorSubject<SecurityDepartmentUserData[] | null>(null);

    private _securityTeamUsers: SecurityTeamUserData[] | null = null;
    private _securityTeamUsersPromise: Promise<SecurityTeamUserData[]> | null  = null;
    private _securityTeamUsersSubject = new BehaviorSubject<SecurityTeamUserData[] | null>(null);

    private _securityUserEvents: SecurityUserEventData[] | null = null;
    private _securityUserEventsPromise: Promise<SecurityUserEventData[]> | null  = null;
    private _securityUserEventsSubject = new BehaviorSubject<SecurityUserEventData[] | null>(null);

    private _securityUserPasswordResetTokens: SecurityUserPasswordResetTokenData[] | null = null;
    private _securityUserPasswordResetTokensPromise: Promise<SecurityUserPasswordResetTokenData[]> | null  = null;
    private _securityUserPasswordResetTokensSubject = new BehaviorSubject<SecurityUserPasswordResetTokenData[] | null>(null);

    private _securityUserSecurityGroups: SecurityUserSecurityGroupData[] | null = null;
    private _securityUserSecurityGroupsPromise: Promise<SecurityUserSecurityGroupData[]> | null  = null;
    private _securityUserSecurityGroupsSubject = new BehaviorSubject<SecurityUserSecurityGroupData[] | null>(null);

    private _securityUserSecurityRoles: SecurityUserSecurityRoleData[] | null = null;
    private _securityUserSecurityRolesPromise: Promise<SecurityUserSecurityRoleData[]> | null  = null;
    private _securityUserSecurityRolesSubject = new BehaviorSubject<SecurityUserSecurityRoleData[] | null>(null);

    private _entityDataTokens: EntityDataTokenData[] | null = null;
    private _entityDataTokensPromise: Promise<EntityDataTokenData[]> | null  = null;
    private _entityDataTokensSubject = new BehaviorSubject<EntityDataTokenData[] | null>(null);


    //
    // Public observables — use with | async in templates
    // Subscription triggers lazy load if not already cached
    //
    // Also includes an observable for each child list to access its row count.
    //
    public SecurityTenantUsers$ = this._securityTenantUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityTenantUsers === null && this._securityTenantUsersPromise === null) {
            this.loadSecurityTenantUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityTenantUsersCount$ = SecurityTenantUserService.Instance.GetSecurityTenantUsersRowCount({securityUserId: this.id,
      active: true,
      deleted: false
    });



    public SecurityOrganizationUsers$ = this._securityOrganizationUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityOrganizationUsers === null && this._securityOrganizationUsersPromise === null) {
            this.loadSecurityOrganizationUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityOrganizationUsersCount$ = SecurityOrganizationUserService.Instance.GetSecurityOrganizationUsersRowCount({securityUserId: this.id,
      active: true,
      deleted: false
    });



    public SecurityDepartmentUsers$ = this._securityDepartmentUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityDepartmentUsers === null && this._securityDepartmentUsersPromise === null) {
            this.loadSecurityDepartmentUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityDepartmentUsersCount$ = SecurityDepartmentUserService.Instance.GetSecurityDepartmentUsersRowCount({securityUserId: this.id,
      active: true,
      deleted: false
    });



    public SecurityTeamUsers$ = this._securityTeamUsersSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityTeamUsers === null && this._securityTeamUsersPromise === null) {
            this.loadSecurityTeamUsers(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityTeamUsersCount$ = SecurityTeamUserService.Instance.GetSecurityTeamUsersRowCount({securityUserId: this.id,
      active: true,
      deleted: false
    });



    public SecurityUserEvents$ = this._securityUserEventsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityUserEvents === null && this._securityUserEventsPromise === null) {
            this.loadSecurityUserEvents(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityUserEventsCount$ = SecurityUserEventService.Instance.GetSecurityUserEventsRowCount({securityUserId: this.id,
      active: true,
      deleted: false
    });



    public SecurityUserPasswordResetTokens$ = this._securityUserPasswordResetTokensSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityUserPasswordResetTokens === null && this._securityUserPasswordResetTokensPromise === null) {
            this.loadSecurityUserPasswordResetTokens(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityUserPasswordResetTokensCount$ = SecurityUserPasswordResetTokenService.Instance.GetSecurityUserPasswordResetTokensRowCount({securityUserId: this.id,
      active: true,
      deleted: false
    });



    public SecurityUserSecurityGroups$ = this._securityUserSecurityGroupsSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityUserSecurityGroups === null && this._securityUserSecurityGroupsPromise === null) {
            this.loadSecurityUserSecurityGroups(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityUserSecurityGroupsCount$ = SecurityUserSecurityGroupService.Instance.GetSecurityUserSecurityGroupsRowCount({securityUserId: this.id,
      active: true,
      deleted: false
    });



    public SecurityUserSecurityRoles$ = this._securityUserSecurityRolesSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._securityUserSecurityRoles === null && this._securityUserSecurityRolesPromise === null) {
            this.loadSecurityUserSecurityRoles(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public SecurityUserSecurityRolesCount$ = SecurityUserSecurityRoleService.Instance.GetSecurityUserSecurityRolesRowCount({securityUserId: this.id,
      active: true,
      deleted: false
    });



    public EntityDataTokens$ = this._entityDataTokensSubject.asObservable().pipe(

        // Trigger load on first subscription if not already loaded
        tap(() => {
          if (this._entityDataTokens === null && this._entityDataTokensPromise === null) {
            this.loadEntityDataTokens(); // Private method to start fetch
          }
        }),
        shareReplay(1) // Cache last emit
    );

  
    public EntityDataTokensCount$ = EntityDataTokenService.Instance.GetEntityDataTokensRowCount({securityUserId: this.id,
      active: true,
      deleted: false
    });




  //
  // Full reload — refreshes the entire object and clears all lazy caches 
  //
  // Promise based reload method to allow rebuilding of any SecurityUserData object with all of it's relations on demand.  Useful for navigating into nav property
  // objects and getting full state after put or post that may not have returned all nav properties.
  //
  // Usage examples:;
  //
  //  Async:
  //   await this.securityUser.Reload();
  //
  //  Non Async:
  //
  //     securityUser[0].Reload().then(x => {
  //        this.securityUser = x;
  //    });
  //
  public async Reload(includeRelations: boolean = true): Promise<this> {

    const fresh = await lastValueFrom(
      SecurityUserService.Instance.GetSecurityUser(this.id, includeRelations)
    );

    // Merge fresh data into this instance (preserves reference)
    this.UpdateFrom(fresh as this);

    // Clear all lazy caches to force re-load on next access
    this.clearAllLazyCaches();

    return this;
  }


  private clearAllLazyCaches(): void {
     // Reset every collection cache and notify subscribers
     this._securityTenantUsers = null;
     this._securityTenantUsersPromise = null;
     this._securityTenantUsersSubject.next(null);

     this._securityOrganizationUsers = null;
     this._securityOrganizationUsersPromise = null;
     this._securityOrganizationUsersSubject.next(null);

     this._securityDepartmentUsers = null;
     this._securityDepartmentUsersPromise = null;
     this._securityDepartmentUsersSubject.next(null);

     this._securityTeamUsers = null;
     this._securityTeamUsersPromise = null;
     this._securityTeamUsersSubject.next(null);

     this._securityUserEvents = null;
     this._securityUserEventsPromise = null;
     this._securityUserEventsSubject.next(null);

     this._securityUserPasswordResetTokens = null;
     this._securityUserPasswordResetTokensPromise = null;
     this._securityUserPasswordResetTokensSubject.next(null);

     this._securityUserSecurityGroups = null;
     this._securityUserSecurityGroupsPromise = null;
     this._securityUserSecurityGroupsSubject.next(null);

     this._securityUserSecurityRoles = null;
     this._securityUserSecurityRolesPromise = null;
     this._securityUserSecurityRolesSubject.next(null);

     this._entityDataTokens = null;
     this._entityDataTokensPromise = null;
     this._entityDataTokensSubject.next(null);

  }

    //
    // Promise-based getters below — same lazy-load logic as observables
    // Use these in component code with await or .then()
    //
    /**
     *
     * Gets the SecurityTenantUsers for this SecurityUser.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityUser.SecurityTenantUsers.then(securityTenantUsers => { ... })
     *   or
     *   await this.securityUser.SecurityTenantUsers
     *
    */
    public get SecurityTenantUsers(): Promise<SecurityTenantUserData[]> {
        if (this._securityTenantUsers !== null) {
            return Promise.resolve(this._securityTenantUsers);
        }

        if (this._securityTenantUsersPromise !== null) {
            return this._securityTenantUsersPromise;
        }

        // Start the load
        this.loadSecurityTenantUsers();

        return this._securityTenantUsersPromise!;
    }



    private loadSecurityTenantUsers(): void {

        this._securityTenantUsersPromise = lastValueFrom(
            SecurityUserService.Instance.GetSecurityTenantUsersForSecurityUser(this.id)
        )
        .then(securityTenantUsers => {
            this._securityTenantUsers = securityTenantUsers ?? [];
            this._securityTenantUsersSubject.next(this._securityTenantUsers);
            return this._securityTenantUsers;
         })
        .catch(err => {
            this._securityTenantUsers = [];
            this._securityTenantUsersSubject.next(this._securityTenantUsers);
            throw err;
        })
        .finally(() => {
            this._securityTenantUsersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearSecurityTenantUsersCache(): void {
        this._securityTenantUsers = null;
        this._securityTenantUsersPromise = null;
        this._securityTenantUsersSubject.next(this._securityTenantUsers);      // Emit to observable
    }

    public get HasSecurityTenantUsers(): Promise<boolean> {
        return this.SecurityTenantUsers.then(securityTenantUsers => securityTenantUsers.length > 0);
    }


    /**
     *
     * Gets the SecurityOrganizationUsers for this SecurityUser.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityUser.SecurityOrganizationUsers.then(securityOrganizationUsers => { ... })
     *   or
     *   await this.securityUser.SecurityOrganizationUsers
     *
    */
    public get SecurityOrganizationUsers(): Promise<SecurityOrganizationUserData[]> {
        if (this._securityOrganizationUsers !== null) {
            return Promise.resolve(this._securityOrganizationUsers);
        }

        if (this._securityOrganizationUsersPromise !== null) {
            return this._securityOrganizationUsersPromise;
        }

        // Start the load
        this.loadSecurityOrganizationUsers();

        return this._securityOrganizationUsersPromise!;
    }



    private loadSecurityOrganizationUsers(): void {

        this._securityOrganizationUsersPromise = lastValueFrom(
            SecurityUserService.Instance.GetSecurityOrganizationUsersForSecurityUser(this.id)
        )
        .then(securityOrganizationUsers => {
            this._securityOrganizationUsers = securityOrganizationUsers ?? [];
            this._securityOrganizationUsersSubject.next(this._securityOrganizationUsers);
            return this._securityOrganizationUsers;
         })
        .catch(err => {
            this._securityOrganizationUsers = [];
            this._securityOrganizationUsersSubject.next(this._securityOrganizationUsers);
            throw err;
        })
        .finally(() => {
            this._securityOrganizationUsersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearSecurityOrganizationUsersCache(): void {
        this._securityOrganizationUsers = null;
        this._securityOrganizationUsersPromise = null;
        this._securityOrganizationUsersSubject.next(this._securityOrganizationUsers);      // Emit to observable
    }

    public get HasSecurityOrganizationUsers(): Promise<boolean> {
        return this.SecurityOrganizationUsers.then(securityOrganizationUsers => securityOrganizationUsers.length > 0);
    }


    /**
     *
     * Gets the SecurityDepartmentUsers for this SecurityUser.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityUser.SecurityDepartmentUsers.then(securityDepartmentUsers => { ... })
     *   or
     *   await this.securityUser.SecurityDepartmentUsers
     *
    */
    public get SecurityDepartmentUsers(): Promise<SecurityDepartmentUserData[]> {
        if (this._securityDepartmentUsers !== null) {
            return Promise.resolve(this._securityDepartmentUsers);
        }

        if (this._securityDepartmentUsersPromise !== null) {
            return this._securityDepartmentUsersPromise;
        }

        // Start the load
        this.loadSecurityDepartmentUsers();

        return this._securityDepartmentUsersPromise!;
    }



    private loadSecurityDepartmentUsers(): void {

        this._securityDepartmentUsersPromise = lastValueFrom(
            SecurityUserService.Instance.GetSecurityDepartmentUsersForSecurityUser(this.id)
        )
        .then(securityDepartmentUsers => {
            this._securityDepartmentUsers = securityDepartmentUsers ?? [];
            this._securityDepartmentUsersSubject.next(this._securityDepartmentUsers);
            return this._securityDepartmentUsers;
         })
        .catch(err => {
            this._securityDepartmentUsers = [];
            this._securityDepartmentUsersSubject.next(this._securityDepartmentUsers);
            throw err;
        })
        .finally(() => {
            this._securityDepartmentUsersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearSecurityDepartmentUsersCache(): void {
        this._securityDepartmentUsers = null;
        this._securityDepartmentUsersPromise = null;
        this._securityDepartmentUsersSubject.next(this._securityDepartmentUsers);      // Emit to observable
    }

    public get HasSecurityDepartmentUsers(): Promise<boolean> {
        return this.SecurityDepartmentUsers.then(securityDepartmentUsers => securityDepartmentUsers.length > 0);
    }


    /**
     *
     * Gets the SecurityTeamUsers for this SecurityUser.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityUser.SecurityTeamUsers.then(securityTeamUsers => { ... })
     *   or
     *   await this.securityUser.SecurityTeamUsers
     *
    */
    public get SecurityTeamUsers(): Promise<SecurityTeamUserData[]> {
        if (this._securityTeamUsers !== null) {
            return Promise.resolve(this._securityTeamUsers);
        }

        if (this._securityTeamUsersPromise !== null) {
            return this._securityTeamUsersPromise;
        }

        // Start the load
        this.loadSecurityTeamUsers();

        return this._securityTeamUsersPromise!;
    }



    private loadSecurityTeamUsers(): void {

        this._securityTeamUsersPromise = lastValueFrom(
            SecurityUserService.Instance.GetSecurityTeamUsersForSecurityUser(this.id)
        )
        .then(securityTeamUsers => {
            this._securityTeamUsers = securityTeamUsers ?? [];
            this._securityTeamUsersSubject.next(this._securityTeamUsers);
            return this._securityTeamUsers;
         })
        .catch(err => {
            this._securityTeamUsers = [];
            this._securityTeamUsersSubject.next(this._securityTeamUsers);
            throw err;
        })
        .finally(() => {
            this._securityTeamUsersPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearSecurityTeamUsersCache(): void {
        this._securityTeamUsers = null;
        this._securityTeamUsersPromise = null;
        this._securityTeamUsersSubject.next(this._securityTeamUsers);      // Emit to observable
    }

    public get HasSecurityTeamUsers(): Promise<boolean> {
        return this.SecurityTeamUsers.then(securityTeamUsers => securityTeamUsers.length > 0);
    }


    /**
     *
     * Gets the SecurityUserEvents for this SecurityUser.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityUser.SecurityUserEvents.then(securityUserEvents => { ... })
     *   or
     *   await this.securityUser.SecurityUserEvents
     *
    */
    public get SecurityUserEvents(): Promise<SecurityUserEventData[]> {
        if (this._securityUserEvents !== null) {
            return Promise.resolve(this._securityUserEvents);
        }

        if (this._securityUserEventsPromise !== null) {
            return this._securityUserEventsPromise;
        }

        // Start the load
        this.loadSecurityUserEvents();

        return this._securityUserEventsPromise!;
    }



    private loadSecurityUserEvents(): void {

        this._securityUserEventsPromise = lastValueFrom(
            SecurityUserService.Instance.GetSecurityUserEventsForSecurityUser(this.id)
        )
        .then(securityUserEvents => {
            this._securityUserEvents = securityUserEvents ?? [];
            this._securityUserEventsSubject.next(this._securityUserEvents);
            return this._securityUserEvents;
         })
        .catch(err => {
            this._securityUserEvents = [];
            this._securityUserEventsSubject.next(this._securityUserEvents);
            throw err;
        })
        .finally(() => {
            this._securityUserEventsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearSecurityUserEventsCache(): void {
        this._securityUserEvents = null;
        this._securityUserEventsPromise = null;
        this._securityUserEventsSubject.next(this._securityUserEvents);      // Emit to observable
    }

    public get HasSecurityUserEvents(): Promise<boolean> {
        return this.SecurityUserEvents.then(securityUserEvents => securityUserEvents.length > 0);
    }


    /**
     *
     * Gets the SecurityUserPasswordResetTokens for this SecurityUser.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityUser.SecurityUserPasswordResetTokens.then(securityUserPasswordResetTokens => { ... })
     *   or
     *   await this.securityUser.SecurityUserPasswordResetTokens
     *
    */
    public get SecurityUserPasswordResetTokens(): Promise<SecurityUserPasswordResetTokenData[]> {
        if (this._securityUserPasswordResetTokens !== null) {
            return Promise.resolve(this._securityUserPasswordResetTokens);
        }

        if (this._securityUserPasswordResetTokensPromise !== null) {
            return this._securityUserPasswordResetTokensPromise;
        }

        // Start the load
        this.loadSecurityUserPasswordResetTokens();

        return this._securityUserPasswordResetTokensPromise!;
    }



    private loadSecurityUserPasswordResetTokens(): void {

        this._securityUserPasswordResetTokensPromise = lastValueFrom(
            SecurityUserService.Instance.GetSecurityUserPasswordResetTokensForSecurityUser(this.id)
        )
        .then(securityUserPasswordResetTokens => {
            this._securityUserPasswordResetTokens = securityUserPasswordResetTokens ?? [];
            this._securityUserPasswordResetTokensSubject.next(this._securityUserPasswordResetTokens);
            return this._securityUserPasswordResetTokens;
         })
        .catch(err => {
            this._securityUserPasswordResetTokens = [];
            this._securityUserPasswordResetTokensSubject.next(this._securityUserPasswordResetTokens);
            throw err;
        })
        .finally(() => {
            this._securityUserPasswordResetTokensPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearSecurityUserPasswordResetTokensCache(): void {
        this._securityUserPasswordResetTokens = null;
        this._securityUserPasswordResetTokensPromise = null;
        this._securityUserPasswordResetTokensSubject.next(this._securityUserPasswordResetTokens);      // Emit to observable
    }

    public get HasSecurityUserPasswordResetTokens(): Promise<boolean> {
        return this.SecurityUserPasswordResetTokens.then(securityUserPasswordResetTokens => securityUserPasswordResetTokens.length > 0);
    }


    /**
     *
     * Gets the SecurityUserSecurityGroups for this SecurityUser.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityUser.SecurityUserSecurityGroups.then(securityUserSecurityGroups => { ... })
     *   or
     *   await this.securityUser.SecurityUserSecurityGroups
     *
    */
    public get SecurityUserSecurityGroups(): Promise<SecurityUserSecurityGroupData[]> {
        if (this._securityUserSecurityGroups !== null) {
            return Promise.resolve(this._securityUserSecurityGroups);
        }

        if (this._securityUserSecurityGroupsPromise !== null) {
            return this._securityUserSecurityGroupsPromise;
        }

        // Start the load
        this.loadSecurityUserSecurityGroups();

        return this._securityUserSecurityGroupsPromise!;
    }



    private loadSecurityUserSecurityGroups(): void {

        this._securityUserSecurityGroupsPromise = lastValueFrom(
            SecurityUserService.Instance.GetSecurityUserSecurityGroupsForSecurityUser(this.id)
        )
        .then(securityUserSecurityGroups => {
            this._securityUserSecurityGroups = securityUserSecurityGroups ?? [];
            this._securityUserSecurityGroupsSubject.next(this._securityUserSecurityGroups);
            return this._securityUserSecurityGroups;
         })
        .catch(err => {
            this._securityUserSecurityGroups = [];
            this._securityUserSecurityGroupsSubject.next(this._securityUserSecurityGroups);
            throw err;
        })
        .finally(() => {
            this._securityUserSecurityGroupsPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearSecurityUserSecurityGroupsCache(): void {
        this._securityUserSecurityGroups = null;
        this._securityUserSecurityGroupsPromise = null;
        this._securityUserSecurityGroupsSubject.next(this._securityUserSecurityGroups);      // Emit to observable
    }

    public get HasSecurityUserSecurityGroups(): Promise<boolean> {
        return this.SecurityUserSecurityGroups.then(securityUserSecurityGroups => securityUserSecurityGroups.length > 0);
    }


    /**
     *
     * Gets the SecurityUserSecurityRoles for this SecurityUser.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityUser.SecurityUserSecurityRoles.then(securityUserSecurityRoles => { ... })
     *   or
     *   await this.securityUser.SecurityUserSecurityRoles
     *
    */
    public get SecurityUserSecurityRoles(): Promise<SecurityUserSecurityRoleData[]> {
        if (this._securityUserSecurityRoles !== null) {
            return Promise.resolve(this._securityUserSecurityRoles);
        }

        if (this._securityUserSecurityRolesPromise !== null) {
            return this._securityUserSecurityRolesPromise;
        }

        // Start the load
        this.loadSecurityUserSecurityRoles();

        return this._securityUserSecurityRolesPromise!;
    }



    private loadSecurityUserSecurityRoles(): void {

        this._securityUserSecurityRolesPromise = lastValueFrom(
            SecurityUserService.Instance.GetSecurityUserSecurityRolesForSecurityUser(this.id)
        )
        .then(securityUserSecurityRoles => {
            this._securityUserSecurityRoles = securityUserSecurityRoles ?? [];
            this._securityUserSecurityRolesSubject.next(this._securityUserSecurityRoles);
            return this._securityUserSecurityRoles;
         })
        .catch(err => {
            this._securityUserSecurityRoles = [];
            this._securityUserSecurityRolesSubject.next(this._securityUserSecurityRoles);
            throw err;
        })
        .finally(() => {
            this._securityUserSecurityRolesPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearSecurityUserSecurityRolesCache(): void {
        this._securityUserSecurityRoles = null;
        this._securityUserSecurityRolesPromise = null;
        this._securityUserSecurityRolesSubject.next(this._securityUserSecurityRoles);      // Emit to observable
    }

    public get HasSecurityUserSecurityRoles(): Promise<boolean> {
        return this.SecurityUserSecurityRoles.then(securityUserSecurityRoles => securityUserSecurityRoles.length > 0);
    }


    /**
     *
     * Gets the EntityDataTokens for this SecurityUser.
     *
     * If already loaded, returns cached array.
     *
     * If not, fetches from server and caches the result.
     * 
     * Usage in components:
     *   this.securityUser.EntityDataTokens.then(entityDataTokens => { ... })
     *   or
     *   await this.securityUser.EntityDataTokens
     *
    */
    public get EntityDataTokens(): Promise<EntityDataTokenData[]> {
        if (this._entityDataTokens !== null) {
            return Promise.resolve(this._entityDataTokens);
        }

        if (this._entityDataTokensPromise !== null) {
            return this._entityDataTokensPromise;
        }

        // Start the load
        this.loadEntityDataTokens();

        return this._entityDataTokensPromise!;
    }



    private loadEntityDataTokens(): void {

        this._entityDataTokensPromise = lastValueFrom(
            SecurityUserService.Instance.GetEntityDataTokensForSecurityUser(this.id)
        )
        .then(entityDataTokens => {
            this._entityDataTokens = entityDataTokens ?? [];
            this._entityDataTokensSubject.next(this._entityDataTokens);
            return this._entityDataTokens;
         })
        .catch(err => {
            this._entityDataTokens = [];
            this._entityDataTokensSubject.next(this._entityDataTokens);
            throw err;
        })
        .finally(() => {
            this._entityDataTokensPromise = null; // Allow retry if needed
        });
    }

    /**
     * Clears the cached crew members. Call after mutations to force refresh.
     */
    public ClearEntityDataTokensCache(): void {
        this._entityDataTokens = null;
        this._entityDataTokensPromise = null;
        this._entityDataTokensSubject.next(this._entityDataTokens);      // Emit to observable
    }

    public get HasEntityDataTokens(): Promise<boolean> {
        return this.EntityDataTokens.then(entityDataTokens => entityDataTokens.length > 0);
    }




    /**
     * Updates the state of this SecurityUserData object using values from another object that has some or all of the fields needed.
     */
    public UpdateFrom(other: Partial<this>): void {
        Object.assign(this, other);
    }


    /**
     * Converts this SecurityUserData object to a submission object for sending to the server.
     */
    public ConvertToSubmitData(): SecurityUserSubmitData {
        return SecurityUserService.Instance.ConvertToSecurityUserSubmitData(this);
    }
}


@Injectable({
  providedIn: 'root'
})
export class SecurityUserService extends SecureEndpointBase {

    private static _instance: SecurityUserService;
    private listCache: Map<string, Observable<Array<SecurityUserData>>>;
    private rowCountCache: Map<string, Observable<bigint | number>>;
    private basicListDataCache: Map<string, Observable<Array<SecurityUserBasicListData>>>;
    private recordCache: Map<string, Observable<SecurityUserData>>;


    constructor(http: HttpClient,
        authService: AuthService,
        alertService: AlertService,
        private utilityService: UtilityService,
        private securityTenantUserService: SecurityTenantUserService,
        private securityOrganizationUserService: SecurityOrganizationUserService,
        private securityDepartmentUserService: SecurityDepartmentUserService,
        private securityTeamUserService: SecurityTeamUserService,
        private securityUserEventService: SecurityUserEventService,
        private securityUserPasswordResetTokenService: SecurityUserPasswordResetTokenService,
        private securityUserSecurityGroupService: SecurityUserSecurityGroupService,
        private securityUserSecurityRoleService: SecurityUserSecurityRoleService,
        private entityDataTokenService: EntityDataTokenService,
        @Inject('BASE_URL') private baseUrl: string) {
        super(http, alertService, authService);

        this.listCache = new Map<string, Observable<Array<SecurityUserData>>>();
        this.rowCountCache = new Map<string, Observable<bigint | number>>();
        this.basicListDataCache = new Map<string, Observable<Array<SecurityUserBasicListData>>>();
        this.recordCache = new Map<string, Observable<SecurityUserData>>();

        SecurityUserService._instance = this;
    }

    public static get Instance(): SecurityUserService {
      return SecurityUserService._instance;
    }


    public ClearListCaches(config: SecurityUserQueryParameters | null = null) {

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


    public ConvertToSecurityUserSubmitData(data: SecurityUserData): SecurityUserSubmitData {

        let output = new SecurityUserSubmitData();

        output.id = data.id;
        output.accountName = data.accountName;
        output.activeDirectoryAccount = data.activeDirectoryAccount;
        output.canLogin = data.canLogin;
        output.mustChangePassword = data.mustChangePassword;
        output.firstName = data.firstName;
        output.middleName = data.middleName;
        output.lastName = data.lastName;
        output.dateOfBirth = data.dateOfBirth;
        output.emailAddress = data.emailAddress;
        output.cellPhoneNumber = data.cellPhoneNumber;
        output.phoneNumber = data.phoneNumber;
        output.phoneExtension = data.phoneExtension;
        output.description = data.description;
        output.securityUserTitleId = data.securityUserTitleId;
        output.reportsToSecurityUserId = data.reportsToSecurityUserId;
        output.authenticationDomain = data.authenticationDomain;
        output.failedLoginCount = data.failedLoginCount;
        output.lastLoginAttempt = data.lastLoginAttempt;
        output.mostRecentActivity = data.mostRecentActivity;
        output.alternateIdentifier = data.alternateIdentifier;
        output.image = data.image;
        output.settings = data.settings;
        output.securityTenantId = data.securityTenantId;
        output.readPermissionLevel = data.readPermissionLevel;
        output.writePermissionLevel = data.writePermissionLevel;
        output.securityOrganizationId = data.securityOrganizationId;
        output.securityDepartmentId = data.securityDepartmentId;
        output.securityTeamId = data.securityTeamId;
        output.authenticationToken = data.authenticationToken;
        output.authenticationTokenExpiry = data.authenticationTokenExpiry;
        output.twoFactorToken = data.twoFactorToken;
        output.twoFactorTokenExpiry = data.twoFactorTokenExpiry;
        output.twoFactorSendByEmail = data.twoFactorSendByEmail;
        output.twoFactorSendBySMS = data.twoFactorSendBySMS;
        output.active = data.active;
        output.deleted = data.deleted;

        return output;
    }

    public GetSecurityUser(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityUserData> {

        const configHash = this.utilityService.hashCode("_" + id.toString() + "_" + includeRelations.toString());

        if (this.recordCache.has(configHash) == false) {

            const securityUser$ = this.requestSecurityUser(id, includeRelations).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.recordCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUser", error);

                    return throwError(() => error);
                })
            );

            this.recordCache.set(configHash, securityUser$);

            return securityUser$;
        }

        return this.recordCache.get(configHash) as Observable<SecurityUserData>;
    }

    private requestSecurityUser(id: bigint | number, includeRelations: boolean = true) : Observable<SecurityUserData> {

        let queryParams = new HttpParams();

        queryParams = queryParams.append("includeRelations", includeRelations.toString());

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<SecurityUserData>(this.baseUrl + 'api/SecurityUser/' + id.toString(), { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(raw => this.ReviveSecurityUser(raw)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUser(id, includeRelations));
            }));
    }

    public GetSecurityUserList(config: SecurityUserQueryParameters | any = null) : Observable<Array<SecurityUserData>> {

        const configHash = this.getConfigHash(config);

        if (!this.listCache.has(configHash)) {
            const securityUserList$ = this.requestSecurityUserList(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.listCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUser list", error);

                    return throwError(() => error);
                })
            );

            this.listCache.set(configHash, securityUserList$);

            return securityUserList$;
        }

        return this.listCache.get(configHash) as Observable<Array<SecurityUserData>>;
    }


    private requestSecurityUserList(config: SecurityUserQueryParameters | any) : Observable <Array<SecurityUserData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityUserData>>(this.baseUrl + 'api/SecurityUsers', { 
            params: queryParams, 
            headers: authenticationHeaders }).pipe(
            map(rawList => this.ReviveSecurityUserList(rawList)),
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUserList(config));
            }));
    }

    public GetSecurityUsersRowCount(config: SecurityUserQueryParameters | any = null) : Observable<bigint | number> {

        const configHash = this.getConfigHash(config);

        if (!this.rowCountCache.has(configHash)) {
            const securityUsersRowCount$ = this.requestSecurityUsersRowCount(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.rowCountCache.delete(configHash);
          
                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUsers row count", error);

                    return throwError(() => error);
                })
            )

            this.rowCountCache.set(configHash, securityUsersRowCount$);

            return securityUsersRowCount$;
        }

        return this.rowCountCache.get(configHash) as Observable<bigint | number>;
    }

    private requestSecurityUsersRowCount(config: SecurityUserQueryParameters | any) : Observable<bigint | number> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<bigint | number>(this.baseUrl + 'api/SecurityUsers/RowCount', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUsersRowCount(config));
            }));
    }

    public GetSecurityUsersBasicListData(config: SecurityUserQueryParameters | any = null) : Observable<Array<SecurityUserBasicListData>> {

        const configHash = this.getConfigHash(config);

        if (!this.basicListDataCache.has(configHash)) {
            const securityUsersBasicListData$ = this.requestSecurityUsersBasicListData(config).pipe(
                shareReplay({ bufferSize: SHARE_REPLAY_CACHE_SIZE, refCount: true }),
                catchError((error) => {
                    this.basicListDataCache.delete(configHash);

                    //this.alertService.showHttpErrorMessage("Unable to get SecurityUsers basic list data", error);

                    return throwError(() => error);
                })
            );
      
            this.basicListDataCache.set(configHash, securityUsersBasicListData$);

            return securityUsersBasicListData$;
        }

        return this.basicListDataCache.get(configHash) as Observable<Array<SecurityUserBasicListData>>;
    }


    private requestSecurityUsersBasicListData(config: SecurityUserQueryParameters | any) : Observable<Array<SecurityUserBasicListData>> {

        let queryParams = new HttpParams();

        if (config != null) {

            for (const property in config) {
                if (config[property] != null) {
                    queryParams = queryParams.append(property, config[property].toString());
                }
            }
        }

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.get<Array<SecurityUserBasicListData>>(this.baseUrl + 'api/SecurityUsers/ListData', { params: queryParams, headers: authenticationHeaders }).pipe(
            catchError(error => {
                return this.handleError(error, () => this.requestSecurityUsersBasicListData(config));
            }));

    }


    public PutSecurityUser(id: bigint | number, securityUser: SecurityUserSubmitData) : Observable<SecurityUserData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.put<SecurityUserData>(this.baseUrl + 'api/SecurityUser/' + id.toString(), securityUser, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityUser(raw)),
            catchError(error => {
                return this.handleError(error, () => this.PutSecurityUser(id, securityUser));
            }));
    }


    public PostSecurityUser(securityUser: SecurityUserSubmitData) : Observable<SecurityUserData> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.post<SecurityUserData>(this.baseUrl + 'api/SecurityUser', securityUser, { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            map(raw => this.ReviveSecurityUser(raw)),
            catchError(error => {
              return this.handleError(error, () => this.PostSecurityUser(securityUser));
            }));
    }

  
    public DeleteSecurityUser(id: bigint | number) : Observable<any> {

        const authenticationHeaders = this.authService.GetAuthenticationHeaders();

        return this.http.delete<void>(this.baseUrl + 'api/SecurityUser/' + id.toString(), { headers: authenticationHeaders } ).pipe(
            tap(() => this.ClearAllCaches()),
            catchError(error => {
                return this.handleError(error, () => this.DeleteSecurityUser(id));
            }));
    }


    private getConfigHash(config: SecurityUserQueryParameters | any): string {

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

    public userIsSecuritySecurityUserReader(): boolean {

        //
        // First get the overall module reading privilege
        //
        let userIsSecuritySecurityUserReader = this.authService.isSecurityReader;

        //
        // Next test to see if the user has a high enough read permission level to read from Security.SecurityUsers
        //
        if (userIsSecuritySecurityUserReader == true) {
            const user = this.authService.currentUser;

            if (user != null) {
                userIsSecuritySecurityUserReader = user.readPermission >= 0;
            } else {
                userIsSecuritySecurityUserReader = false;
            }
        }

        return userIsSecuritySecurityUserReader;
    }


    public userIsSecuritySecurityUserWriter(): boolean {

        //
        // First get the overall module writing privilege
        //
        let userIsSecuritySecurityUserWriter = this.authService.isSecurityReaderWriter;

        //
        // Next test to see if the user has a high enough write permission level to write to Security.SecurityUsers
        //
        if (userIsSecuritySecurityUserWriter == true) {
          let user = this.authService.currentUser;

          if (user != null) {
            userIsSecuritySecurityUserWriter = user.writePermission >= 0;
          } else {
            userIsSecuritySecurityUserWriter = false;
          }      
        }

        return userIsSecuritySecurityUserWriter;
    }

    public GetSecurityTenantUsersForSecurityUser(securityUserId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityTenantUserData[]> {
        return this.securityTenantUserService.GetSecurityTenantUserList({
            securityUserId: securityUserId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityOrganizationUsersForSecurityUser(securityUserId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityOrganizationUserData[]> {
        return this.securityOrganizationUserService.GetSecurityOrganizationUserList({
            securityUserId: securityUserId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityDepartmentUsersForSecurityUser(securityUserId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityDepartmentUserData[]> {
        return this.securityDepartmentUserService.GetSecurityDepartmentUserList({
            securityUserId: securityUserId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityTeamUsersForSecurityUser(securityUserId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityTeamUserData[]> {
        return this.securityTeamUserService.GetSecurityTeamUserList({
            securityUserId: securityUserId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityUserEventsForSecurityUser(securityUserId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityUserEventData[]> {
        return this.securityUserEventService.GetSecurityUserEventList({
            securityUserId: securityUserId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityUserPasswordResetTokensForSecurityUser(securityUserId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityUserPasswordResetTokenData[]> {
        return this.securityUserPasswordResetTokenService.GetSecurityUserPasswordResetTokenList({
            securityUserId: securityUserId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityUserSecurityGroupsForSecurityUser(securityUserId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityUserSecurityGroupData[]> {
        return this.securityUserSecurityGroupService.GetSecurityUserSecurityGroupList({
            securityUserId: securityUserId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetSecurityUserSecurityRolesForSecurityUser(securityUserId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<SecurityUserSecurityRoleData[]> {
        return this.securityUserSecurityRoleService.GetSecurityUserSecurityRoleList({
            securityUserId: securityUserId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


    public GetEntityDataTokensForSecurityUser(securityUserId: number | bigint, active: boolean = true, deleted: boolean = false): Observable<EntityDataTokenData[]> {
        return this.entityDataTokenService.GetEntityDataTokenList({
            securityUserId: securityUserId,
            active: active,
            deleted: deleted,
            includeRelations: true
        });
    }


 /**
   *
   * Revives a plain object from the server into a full SecurityUserData instance.
   *
   * This is critical for the lazy-loading pattern to work correctly.
   *
   * When the server returns JSON, it is a plain object with no prototype methods
   * or observable properties. This method:
   * 1. Re-attaches the SecurityUserData prototype
   * 2. Copies all properties from the raw object
   * 3. Re-initializes all private caches and BehaviorSubjects
   * 4. Re-creates all public observable properties ($ suffixed) with their
   *    original tap() triggers that initiate lazy loading on first subscription
   *
   * Without this, revived objects would not trigger loads when SecurityUserTags$ etc.
   * are subscribed to in templates.
   *
   */
  public ReviveSecurityUser(raw: any): SecurityUserData {
    if (!raw) return raw;

    //
    // Create a SecurityUserData object instance with correct prototype
    //
    const revived = Object.create(SecurityUserData.prototype) as SecurityUserData;

    //
    // Copy all raw properties
    //
    Object.assign(revived, raw);

    //
    // Explicitly initialize all private caches
    // This ensures the getters work correctly on revived objects
    //
    (revived as any)._securityTenantUsers = null;
    (revived as any)._securityTenantUsersPromise = null;
    (revived as any)._securityTenantUsersSubject = new BehaviorSubject<SecurityTenantUserData[] | null>(null);

    (revived as any)._securityOrganizationUsers = null;
    (revived as any)._securityOrganizationUsersPromise = null;
    (revived as any)._securityOrganizationUsersSubject = new BehaviorSubject<SecurityOrganizationUserData[] | null>(null);

    (revived as any)._securityDepartmentUsers = null;
    (revived as any)._securityDepartmentUsersPromise = null;
    (revived as any)._securityDepartmentUsersSubject = new BehaviorSubject<SecurityDepartmentUserData[] | null>(null);

    (revived as any)._securityTeamUsers = null;
    (revived as any)._securityTeamUsersPromise = null;
    (revived as any)._securityTeamUsersSubject = new BehaviorSubject<SecurityTeamUserData[] | null>(null);

    (revived as any)._securityUserEvents = null;
    (revived as any)._securityUserEventsPromise = null;
    (revived as any)._securityUserEventsSubject = new BehaviorSubject<SecurityUserEventData[] | null>(null);

    (revived as any)._securityUserPasswordResetTokens = null;
    (revived as any)._securityUserPasswordResetTokensPromise = null;
    (revived as any)._securityUserPasswordResetTokensSubject = new BehaviorSubject<SecurityUserPasswordResetTokenData[] | null>(null);

    (revived as any)._securityUserSecurityGroups = null;
    (revived as any)._securityUserSecurityGroupsPromise = null;
    (revived as any)._securityUserSecurityGroupsSubject = new BehaviorSubject<SecurityUserSecurityGroupData[] | null>(null);

    (revived as any)._securityUserSecurityRoles = null;
    (revived as any)._securityUserSecurityRolesPromise = null;
    (revived as any)._securityUserSecurityRolesSubject = new BehaviorSubject<SecurityUserSecurityRoleData[] | null>(null);

    (revived as any)._entityDataTokens = null;
    (revived as any)._entityDataTokensPromise = null;
    (revived as any)._entityDataTokensSubject = new BehaviorSubject<EntityDataTokenData[] | null>(null);


    //
    // Re-attach ALL public observables with their lazy-load tap() triggers
    // This mirrors the original class definition exactly
    //
    //
    // Re-create all public observables with their lazy-load triggers
    // We use 'as any' because:
    // 1. The revived object has the correct prototype
    // 2. But private methods (loadSecurityUserXYZ, etc.) are not accessible via the typed variable
    // 3. This is a controlled revival context — safe and necessary
    //
    (revived as any).SecurityTenantUsers$ = (revived as any)._securityTenantUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityTenantUsers === null && (revived as any)._securityTenantUsersPromise === null) {
                (revived as any).loadSecurityTenantUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityTenantUsersCount$ = SecurityTenantUserService.Instance.GetSecurityTenantUsersRowCount({securityUserId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SecurityOrganizationUsers$ = (revived as any)._securityOrganizationUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityOrganizationUsers === null && (revived as any)._securityOrganizationUsersPromise === null) {
                (revived as any).loadSecurityOrganizationUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityOrganizationUsersCount$ = SecurityOrganizationUserService.Instance.GetSecurityOrganizationUsersRowCount({securityUserId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SecurityDepartmentUsers$ = (revived as any)._securityDepartmentUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityDepartmentUsers === null && (revived as any)._securityDepartmentUsersPromise === null) {
                (revived as any).loadSecurityDepartmentUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityDepartmentUsersCount$ = SecurityDepartmentUserService.Instance.GetSecurityDepartmentUsersRowCount({securityUserId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SecurityTeamUsers$ = (revived as any)._securityTeamUsersSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityTeamUsers === null && (revived as any)._securityTeamUsersPromise === null) {
                (revived as any).loadSecurityTeamUsers();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityTeamUsersCount$ = SecurityTeamUserService.Instance.GetSecurityTeamUsersRowCount({securityUserId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SecurityUserEvents$ = (revived as any)._securityUserEventsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityUserEvents === null && (revived as any)._securityUserEventsPromise === null) {
                (revived as any).loadSecurityUserEvents();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityUserEventsCount$ = SecurityUserEventService.Instance.GetSecurityUserEventsRowCount({securityUserId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SecurityUserPasswordResetTokens$ = (revived as any)._securityUserPasswordResetTokensSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityUserPasswordResetTokens === null && (revived as any)._securityUserPasswordResetTokensPromise === null) {
                (revived as any).loadSecurityUserPasswordResetTokens();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityUserPasswordResetTokensCount$ = SecurityUserPasswordResetTokenService.Instance.GetSecurityUserPasswordResetTokensRowCount({securityUserId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SecurityUserSecurityGroups$ = (revived as any)._securityUserSecurityGroupsSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityUserSecurityGroups === null && (revived as any)._securityUserSecurityGroupsPromise === null) {
                (revived as any).loadSecurityUserSecurityGroups();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityUserSecurityGroupsCount$ = SecurityUserSecurityGroupService.Instance.GetSecurityUserSecurityGroupsRowCount({securityUserId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).SecurityUserSecurityRoles$ = (revived as any)._securityUserSecurityRolesSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._securityUserSecurityRoles === null && (revived as any)._securityUserSecurityRolesPromise === null) {
                (revived as any).loadSecurityUserSecurityRoles();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).SecurityUserSecurityRolesCount$ = SecurityUserSecurityRoleService.Instance.GetSecurityUserSecurityRolesRowCount({securityUserId: (revived as any).id,
      active: true,
      deleted: false
    });



    (revived as any).EntityDataTokens$ = (revived as any)._entityDataTokensSubject.asObservable().pipe(
        tap(() => {
              if ((revived as any)._entityDataTokens === null && (revived as any)._entityDataTokensPromise === null) {
                (revived as any).loadEntityDataTokens();        // Need to cast to any to invoke private load method
              }
        }),
        shareReplay(1)
      );

    (revived as any).EntityDataTokensCount$ = EntityDataTokenService.Instance.GetEntityDataTokensRowCount({securityUserId: (revived as any).id,
      active: true,
      deleted: false
    });




    return revived;
  }

  private ReviveSecurityUserList(rawList: any[]): SecurityUserData[] {

    if (!rawList) {
        return [];
    }

    return rawList.map(raw => this.ReviveSecurityUser(raw));
  }

}
