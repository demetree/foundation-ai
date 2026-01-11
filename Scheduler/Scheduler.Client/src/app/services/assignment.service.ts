import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { EventResourceAssignmentSubmitData } from '../scheduler-data-services/event-resource-assignment.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';

const SHARE_REPLAY_CACHE_SIZE = 1;



/**
 * Represents a conflict where a resource is already assigned to another event
 * during an overlapping time period.
 */
export class AssignmentOverlapConflict {
  type: 'AssignmentOverlap' = 'AssignmentOverlap';
  resourceId!: number;
  conflictingAssignmentId!: number;
  conflictingEventName!: string;
  proposedIndex!: number;
  message!: string;
}

/**
 * Represents a conflict where a resource is unavailable due to a blackout
 * (vacation, maintenance, etc.).
 */
export class BlackoutConflict {
  type: 'BlackoutConflict' = 'BlackoutConflict';
  resourceId!: number;
  blackoutStart!: string;     // ISO 8601 UTC
  blackoutEnd!: string | null; // ISO 8601 UTC or null if ongoing
  reason!: string;
  proposedIndex!: number;
  message!: string;
}

/**
 * Union type for all possible conflict responses from the server.
 */
export type AssignmentConflict = AssignmentOverlapConflict | BlackoutConflict;

@Injectable({
  providedIn: 'root'
})
export class AssignmentService extends SecureEndpointBase {

  constructor(http: HttpClient,
    authService: AuthService,
    alertService: AlertService,
    private utilityService: UtilityService,
    @Inject('BASE_URL') private baseUrl: string) {
    super(http, alertService, authService);

  }


  /**
  * Checks a collection of proposed assignments for conflicts.
  *
  * This calls the dedicated conflict-checking endpoint on the server, which validates:
  * - Overlaps with existing active assignments
  * - Overlaps with active resource blackouts (ResourceAvailability records)
  *
  * Crew assignments are automatically expanded server-side into individual resource checks.
  *
  * Returns an Observable that emits an array of conflict objects.
  * An empty array means no conflicts were found.
  *
  * This method does NOT cache results — conflict checks should always be fresh.
  *
  * @param proposedAssignments An array of assignment data as it would be submitted for save.
  *                            These can include crewId or resourceId (but not both).
  * @returns Observable<AssignmentConflict[]> — empty array = safe to save
  */
  public CheckConflicts(
    proposedAssignments: EventResourceAssignmentSubmitData[]
  ): Observable<AssignmentConflict[]> {
    // Guard against empty or null input — server expects a non-empty array
    if (!proposedAssignments || proposedAssignments.length === 0) {
      // Return empty array synchronously — no conflicts if nothing to check
      return new Observable<AssignmentConflict[]>(subscriber => {
        subscriber.next([]);
        subscriber.complete();
      });
    }

    const authenticationHeaders = this.authService.GetAuthenticationHeaders();

    // POST to the dedicated conflict check endpoint
    return this.http.post<AssignmentConflict[]>(
      this.baseUrl + 'api/EventResourceAssignments/CheckConflicts',
      proposedAssignments,
      { headers: authenticationHeaders }
    ).pipe(
      catchError(error => {
        // Use the centralized error handler with retry capability
        return this.handleError(
          error,
          () => this.CheckConflicts(proposedAssignments)
        );
      })
    );
  }
}
