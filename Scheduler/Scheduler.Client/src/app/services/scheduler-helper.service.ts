import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable, OnDestroy } from '@angular/core';
import { Observable, Subject, BehaviorSubject, of, shareReplay, catchError, throwError } from 'rxjs';
import { map, startWith, takeUntil } from 'rxjs/operators'
import { UtilityService } from '../utility-services/utility.service'
import { AlertService } from '../services/alert.service';
import { AuthService } from '../services/auth.service';
import { SecureEndpointBase } from '../services/secure-endpoint-base.service';
import { OfficeService } from '../scheduler-data-services/office.service';


@Injectable({
  providedIn: 'root'
})
export class SchedulerHelperService extends SecureEndpointBase implements OnDestroy {

  private static _instance: SchedulerHelperService;

  /**
     * Shared observable that emits the current count of active, non-deleted offices.
     *
     * Key characteristics:
     * - Backed by a BehaviorSubject → always has a current value (starts at 0)
     * - Uses shareReplay(1) → multiple subscribers get the same cached value and don't trigger duplicate HTTP calls
     * - Automatically refreshed on construction and can be manually refreshed via Reload()
     * - Safe default of 0 on error
     */
  private activeOfficeCountSubject = new BehaviorSubject<number>(0);
  public readonly ActiveOfficeCount$: Observable<number> = this.activeOfficeCountSubject.asObservable().pipe(
    shareReplay(1) // Ensures single HTTP request + cached value for all subscribers
  );

  /**
   * Subject used for cleanup of internal subscriptions.
   */
  private destroy$ = new Subject<void>();

  constructor(http: HttpClient,
    authService: AuthService,
    alertService: AlertService,
    private utilityService: UtilityService,
    private officeService: OfficeService,
    @Inject('BASE_URL') private baseUrl: string) {
    super(http, alertService, authService);



    SchedulerHelperService._instance = this;

    // Load initial count on service construction
    this.reloadActiveOfficeCount();
  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public static get Instance(): SchedulerHelperService {
    return SchedulerHelperService._instance;
  }


  /**
   * Forces a refresh of the active office count.
   *
   * Call this after any mutation that could change office count
   * (e.g., creating/deleting/activating an office).
   */
  public Reload(): void {
    this.reloadActiveOfficeCount();
  }

  /**
   * Internal method that performs the actual count load and updates the shared subject.
   *
   * Uses the OfficeService row count endpoint for performance.
   * Errors are logged but do not break the stream — count remains at last known value.
   */
  private reloadActiveOfficeCount(): void {

    this.officeService
      .GetOfficesRowCount({ active: true, deleted: false })
      .pipe(
        map((count: bigint | number) => Number(count ?? 0)),
        catchError((err) => {
          console.warn('SchedulerHelperService: Failed to load active office count', err);
          // Return last known value (or 0) on error — prevents UI from breaking
          return of(this.activeOfficeCountSubject.value);
        }),
        takeUntil(this.destroy$)
      )
      .subscribe((count: number) => {
        this.activeOfficeCountSubject.next(count);
      });
  }



  /**
   * Resolves the applicable RateSheet for a hypothetical assignment using the server's hierarchy rules.
   * 
   * This is used for live preview in the RateSheet Add/Edit modal.
   * 
   * @param params The preview criteria (resource, role, target, rate type, date)
   * @returns Observable of the resolved rate details (or error)
   */
  public resolveRate(params: {
    officeId?: number | null,
    resourceId?: number | null,
    assignmentRoleId?: number | null,
    schedulingTargetId?: number | null,
    rateTypeId: number,
    date: string
  }): Observable<any> {

    // Add validation before call
    if (!params.rateTypeId || !params.date) {
      return throwError(() => new Error('rateTypeId and date are required for rate resolution'));
    }

    // Build authentication headers (standard pattern from SecureEndpointBase)
    const authenticationHeaders = this.authService.GetAuthenticationHeaders();

    // Construct query parameters from the input object
    let httpParams = new HttpParams()
      .set('rateTypeId', params.rateTypeId.toString())
      .set('date', params.date);

    if (params.officeId !== null && params.officeId !== undefined) {
      httpParams = httpParams.set('officeId', params.officeId.toString());
    }

    if (params.resourceId !== null && params.resourceId !== undefined) {
      httpParams = httpParams.set('resourceId', params.resourceId.toString());
    }
    if (params.assignmentRoleId !== null && params.assignmentRoleId !== undefined) {
      httpParams = httpParams.set('assignmentRoleId', params.assignmentRoleId.toString());
    }
    if (params.schedulingTargetId !== null && params.schedulingTargetId !== undefined) {
      httpParams = httpParams.set('schedulingTargetId', params.schedulingTargetId.toString());
    }

    // Perform the GET request with auth headers and query params
    return this.http.get<any>(`${this.baseUrl}api/RateSheets/Resolve`, {
      headers: authenticationHeaders,
      params: httpParams
    }).pipe(
      // On error, delegate to base class handler and retry with the SAME parameters
      catchError(error => {
        // Arrow function preserves 'this' context and allows passing original params
        return this.handleError(error, () => this.resolveRate(params));
      })
    );
  }
}
