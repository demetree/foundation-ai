import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable, OnDestroy } from '@angular/core';
import { Observable, Subject, takeUntil, tap, throwError } from 'rxjs';
import { catchError, shareReplay } from 'rxjs/operators'
import { UtilityService } from '../utility-services/utility.service';
import { AlertService } from './alert.service';
import { AuthService } from './auth.service';
import { SecureEndpointBase } from './secure-endpoint-base.service';


@Injectable({
  providedIn: 'root'
})
export class CurrentUserService extends SecureEndpointBase implements OnDestroy {



  /**
   * Subject used for cleanup of internal subscriptions.
   */
  private destroy$ = new Subject<void>();

  constructor(http: HttpClient,
    authService: AuthService,
    alertService: AlertService,
    private utilityService: UtilityService,
    @Inject('BASE_URL') private baseUrl: string)
  {
    super(http, alertService, authService);


    this.loadAllUserData();

  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public loadAllUserData() {

  }


  public ClearAllCaches() {

  }
}
