import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, Subject, from, of, throwError, timer } from 'rxjs';
import { mergeMap, switchMap, catchError, takeUntil, delay } from 'rxjs/operators';
import { AlertService, MessageSeverity } from './alert.service';
import { AuthService } from './auth.service';
import { User } from '../models/user.model';

interface ServerError {
  status: number;
  error: {
    error: string;
    error_description: string;
  };
  retryAttempt: number;
}


@Injectable()
export abstract class SecureEndpointBase {
  private taskPauser: Subject<boolean> | null = null;
  private isRefreshingLogin = false;
  private refreshRetryCount = 3;

  // For 429 retry handling
  private readonly maxRetryCount = 3;    // Maximum retries for 429 errors
  private readonly defaultRetryDelayMs = 2000;     // Delay in milliseconds (2 seconds)

  constructor(
    protected http: HttpClient,
    protected alertService: AlertService,
    protected authService: AuthService) {
  }


  public refreshLogin(): Observable<User> {
    return this.authService.refreshLogin().pipe(
      catchError((error: ServerError) => {
        return this.handleError(error, () => this.refreshLogin());
      }));
  }


  //
  // This handles 401, 403, 429, 503, and invalid grants gracefully.
  //
  protected handleError<T>(error: ServerError, continuation: () => Observable<T>) {

    if (error.status === 401) {
      if (this.isRefreshingLogin == true) {
        return this.pauseTask(continuation);
      }

      this.isRefreshingLogin = true;

      return from(this.authService.refreshLogin()).pipe(
        mergeMap(() => {
          this.isRefreshingLogin = false;
          this.resumeTasks(true);

          return continuation();
        }),
        catchError(refreshLoginError => {
          this.isRefreshingLogin = false;
          this.resumeTasks(false);

          if (refreshLoginError.status === 401 || (refreshLoginError.error && refreshLoginError.error.error === 'invalid_grant')) {
            if (this.refreshRetryCount > 0) {
              this.refreshRetryCount--;
              return this.refreshLogin().pipe(
                delay(1000), // delay to avoid immediate retries
                mergeMap(() => continuation())
              );
            } else {
              this.authService.reLogin();
              return throwError(() => new Error('session expired'));
            }
          } else {
            return throwError(() => refreshLoginError || new Error('server error'));
          }
        }));

    } else if (error.status === 403) {

      this.alertService.showMessage(
        'Access Denied',
        'Insufficient permissions: You do not have access to this resource.',
        MessageSeverity.error
      );

      // Explicitly handle 403 Forbidden
      return throwError(() => new Error('Insufficient permissions: You do not have access to this resource.'));

    } else if (error.error && error.error.error === 'invalid_grant') {
      this.authService.reLogin();

      return throwError(() => (error.error && error.error.error_description) ? `session expired (${error.error.error_description})` : 'session expired');

    } else if (error.status === 429 || error.status === 503) {

      const retryAttempt = error.retryAttempt || 0;
      const errorType = error.status === 429 ? 'Too Many Requests' : 'Service Unavailable';

      if (retryAttempt < this.maxRetryCount) {

        let retryDelayMs = this.getRetryDelayMs(error);

        console.log(
          `${error.status} ${errorType}. Retrying (${retryAttempt + 1}/${this.maxRetryCount}) after ${retryDelayMs}ms`
        );

        const updatedError = { ...error, retryAttempt: retryAttempt + 1 };

        return of(updatedError).pipe(
          delay(retryDelayMs),
          mergeMap(() => continuation())
        );
      } else {

        const errorMessage = error.status === 429 ? 'The server is rate-limiting requests. Please try again later.'
          : 'The server is temporarily unavailable. Please try again later.';

        this.alertService.showMessage(errorType, errorMessage, MessageSeverity.error);

        return throwError(() => new Error(`Maximum retry attempts reached for ${error.status} error`));
      }
    }
    else {
      return throwError(() => error);
    }
  }

  private getRetryDelayMs(error: any): number {

    // First try to get the value from the HTTP Header.  Value returned is in seconds by Data Controllers but Date types are supported as well.
    const retryAfterHeader = error?.headers?.get?.('Retry-After');

    if (retryAfterHeader) {
      const seconds = parseInt(retryAfterHeader, 10);

      if (!isNaN(seconds) && seconds > 0) {
        return seconds * 1000;
      }

      const retryDate = Date.parse(retryAfterHeader);
      if (!isNaN(retryDate)) {
        const delay = retryDate - Date.now();
        if (delay > 0) {
          return delay;
        }
      }
    }

    // Use default value
    return this.defaultRetryDelayMs;
  }

  private pauseTask<T>(continuation: () => Observable<T>) {
    if (!this.taskPauser) {
      this.taskPauser = new Subject();
    }

    return this.taskPauser.pipe(switchMap(continueOp => {
      return continueOp ? continuation() : throwError(() => new Error('session expired'));
    }));
  }

  private resumeTasks(continueOp: boolean) {
    setTimeout(() => {
      if (this.taskPauser) {
        this.taskPauser.next(continueOp);
        this.taskPauser.complete();
        this.taskPauser = null;
      }
    });
  }
}
