import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditSessionService, AuditSessionData, AuditSessionSubmitData } from '../../../auditor-data-services/audit-session.service';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-audit-session-detail',
  templateUrl: './audit-session-detail.component.html',
  styleUrls: ['./audit-session-detail.component.scss']
})

export class AuditSessionDetailComponent implements OnInit, CanComponentDeactivate {

  auditSessionForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });


  public auditSessionId: string | null = null;
  public auditSessionData: AuditSessionData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditSessions$ = this.auditSessionService.GetAuditSessionList();
  auditEvents$ = this.auditEventService.GetAuditEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditSessionService: AuditSessionService,
    public auditEventService: AuditEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditSessionId from the route parameters
    this.auditSessionId = this.route.snapshot.paramMap.get('auditSessionId');

    if (this.auditSessionId === 'new' ||
        this.auditSessionId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditSessionData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit Session';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit Session';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditSessionForm.dirty) {
      return confirm('You have unsaved Audit Session changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditSessionId != null && this.auditSessionId !== 'new') {

      const id = parseInt(this.auditSessionId, 10);

      if (!isNaN(id)) {
        return { auditSessionId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditSession data for the current auditSessionId.
  *
  * Fully respects the AuditSessionService caching strategy and error handling strategy.
  *
  * @param forceLoadAndDisplaySuccessAlert
  *   - true  will bypass cache entirely and show success alert message
  *   - false/null will use cache if available, no alert message
  */
  public loadData(forceLoadAndDisplaySuccessAlert: boolean | null = null): void {

    //
    // Start loading indicator immediately
    //
    this.isLoadingSubject.next(true);


    //
    // Permission Check
    //
    if (!this.auditSessionService.userIsAuditorAuditSessionReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditSessions.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditSessionId
    //
    if (!this.auditSessionId) {

      this.alertService.showMessage('No AuditSession ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditSessionId = Number(this.auditSessionId);

    if (isNaN(auditSessionId) || auditSessionId <= 0) {

      this.alertService.showMessage(`Invalid Audit Session ID: "${this.auditSessionId}"`,
                                    'Invalid ID',
                                    MessageSeverity.error
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Force refresh: clear specific record cache only
    //
    if (forceLoadAndDisplaySuccessAlert === true) {
      // This is the most targeted way: clear only this AuditSession + relations

      this.auditSessionService.ClearRecordCache(auditSessionId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditSessionService.GetAuditSession(auditSessionId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditSessionData) => {

        //
        // Success path — auditSessionData can legitimately be null if 404'd but request succeeded
        //
        if (!auditSessionData) {

          this.handleAuditSessionNotFound(auditSessionId);

        } else {

          this.auditSessionData = auditSessionData;
          this.buildFormValues(this.auditSessionData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditSession loaded successfully',
              '',
              MessageSeverity.success
            );
          }
        }

        this.isLoadingSubject.next(false);
      },

      error: (error: any) => {
        //
        // All HTTP/network/parsing errors flow here
        // The service already stripped sensitive info and re-threw cleanly
        //
        this.handleAuditSessionLoadError(error, auditSessionId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditSessionNotFound(auditSessionId: number): void {

    this.auditSessionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditSession #${auditSessionId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditSessionLoadError(error: any, auditSessionId: number): void {

    let message = 'Failed to load Audit Session.';
    let title = 'Load Error';
    let severity = MessageSeverity.error;

    //
    // Leverage HTTP status if available
    //
    if (error?.status) {
      switch (error.status) {
        case 401:
          message = 'Your session has expired. Please log in again.';
          title = 'Unauthorized';
          break;
        case 403:
          message = 'You do not have permission to view this Audit Session.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit Session #${auditSessionId} was not found.`;
          title = 'Not Found';
          severity = MessageSeverity.warn;
          break;
        case 500:
          message = 'Server error. Please try again or contact support.';
          title = 'Server Error';
          break;
        case 0:
          message = 'Cannot reach server. Check your internet connection.';
          title = 'Offline';
          break;
        default:
          message = `Server error ${error.status || 'unknown'}: ${error.statusText || 'Request failed'}`;
      }
    } else {
      message = error?.message || message;
    }

    console.error(`Audit Session load failed (ID: ${auditSessionId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditSessionData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditSessionData: AuditSessionData | null) {

    if (auditSessionData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditSessionForm.reset({
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditSessionForm.reset({
        name: auditSessionData.name ?? '',
        comments: auditSessionData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditSessionData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditSessionForm.markAsPristine();
    this.auditSessionForm.markAsUntouched();
  }

  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.auditSessionService.userIsAuditorAuditSessionWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Sessions", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditSessionForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditSessionForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditSessionForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditSessionSubmitData: AuditSessionSubmitData = {
        id: this.auditSessionData?.id || 0,
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditSessionService.PutAuditSession(auditSessionSubmitData.id, auditSessionSubmitData)
      : this.auditSessionService.PostAuditSession(auditSessionSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditSessionData) => {

        this.auditSessionService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit Session's detail page
          //
          this.auditSessionForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditSessionForm.markAsUntouched();

          this.router.navigate(['/auditsessions', savedAuditSessionData.id]);
          this.alertService.showMessage('Audit Session added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditSessionData = savedAuditSessionData;
          this.buildFormValues(this.auditSessionData);

          this.alertService.showMessage("Audit Session saved successfully", '', MessageSeverity.success);
        }
      },
      error: (err) => {

            let errorMessage: string;

            // Check if err is an Error object (e.g., new Error('message'))
            if (err instanceof Error) {
                errorMessage = err.message || 'An unexpected error occurred.';
            }
            // Check if err is a ServerError object with status and error properties
            else if (err.status && err.error)
            {
                if (err.status === 403)
                {
                    errorMessage = err.error?.message ||
                                   'You do not have permission to save this Audit Session.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Session.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Session could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditSessionReader(): boolean {
    return this.auditSessionService.userIsAuditorAuditSessionReader();
  }

  public userIsAuditorAuditSessionWriter(): boolean {
    return this.auditSessionService.userIsAuditorAuditSessionWriter();
  }
}
