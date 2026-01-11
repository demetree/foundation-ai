import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, Subject, timer, Subscription } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { SecureEndpointBase } from './secure-endpoint-base.service';
import { AuthService } from './auth.service';
import { AlertService } from '../services/alert.service';
import { CurrentUserService } from './current-user.service';
import { AuditorDataServiceManagerService } from '../auditor-data-services/auditor-data-service-manager.service';
import { SecurityDataServiceManagerService } from '../security-data-services/security-data-service-manager.service';
import { SchedulerDataServiceManagerService } from '../scheduler-data-services/scheduler-data-service-manager.service';

//interface SetupDataChangeTime {
//  latestSetupDataChangeTime: string;
//}


@Injectable({
  providedIn: 'root'
})
export class CacheManagerService extends SecureEndpointBase {

  // bring this back when it will add some value
  //private lastTimestamp: Date = new Date(0); // Initialize to a very old date
  //private setupDataUpdatedSubject = new Subject<void>();
  //private pollingSubscription: Subscription | null = null;

  //private pollingInterval = 15000;

  constructor(
    http: HttpClient,
    authService: AuthService,
    alertService: AlertService,
    @Inject('BASE_URL') private baseUrl: string,
    private currentUserService: CurrentUserService,
    private auditorDataServiceManagerService: AuditorDataServiceManagerService,
    private securityDataServiceManagerService: SecurityDataServiceManagerService,
    private schedulerDataServiceManagerService: SchedulerDataServiceManagerService,
  ) {
    super(http, alertService, authService);

  }

  /// <summary>
  /// Clear all service caches in the application
  /// </summary>
  public ClearAllCaches() {
    this.auditorDataServiceManagerService.ClearAllCaches();
    this.currentUserService.ClearAllCaches();
    this.securityDataServiceManagerService.ClearAllCaches();
    this.schedulerDataServiceManagerService.ClearAllCaches();
  }


  
  /**
   * Observable that notifies subscribers when new setup data is available (i.e., when the server timestamp is newer).
   * Components can subscribe to this to reload their data as needed.
   */

  /* bring this back once we have a more solid UI and workflows
  public get setupDataUpdated$(): Observable<void> {
    return this.setupDataUpdatedSubject.asObservable();
  }


  public setPollingInterval(ms: number): void {
    this.stopPolling();
    this.pollingInterval = ms;
    this.startPolling();
  }

  
  // This starts the polling for new setup data
  public startPolling(): void {
    if (this.pollingSubscription) {
      return; // Already polling, do nothing
    }

    this.pollingSubscription = timer(0, 15000) // Start immediately, then every 15 seconds
      .pipe(
        switchMap(() => this.fetchLatestTimestamp()),
        tap((response: { latestSetupDataChangeTime: string }) => {
          const newTimestamp = new Date(response.latestSetupDataChangeTime);
          if (newTimestamp > this.lastTimestamp) {
            this.lastTimestamp = newTimestamp;
            this.ClearAllCaches();
            this.setupDataUpdatedSubject.next();
          }
        }),
        catchError((error) => {
          console.error('Error fetching latest setup data timestamp:', error);
          return []; // Continue polling without throwing
        })
      )
      .subscribe();
  }


  public stopPolling(): void {
    if (this.pollingSubscription) {
      this.pollingSubscription.unsubscribe();
      this.pollingSubscription = null;
    }
  }


  private fetchLatestTimestamp(): Observable<SetupDataChangeTime> {

    const headers = this.authService.GetAuthenticationHeaders();
    const url = `${this.baseUrl}api/ClientCache/LatestSetupDataChangeTime`;

    return this.http.get<SetupDataChangeTime>(url, { headers }).pipe(
      catchError(error => {
        return this.handleError<SetupDataChangeTime>(error, () => this.fetchLatestTimestamp());
      }));
  }
  */
}
