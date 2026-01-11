import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditModuleService, AuditModuleData, AuditModuleSubmitData } from '../../../auditor-data-services/audit-module.service';
import { AuditModuleEntityService } from '../../../auditor-data-services/audit-module-entity.service';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-audit-module-detail',
  templateUrl: './audit-module-detail.component.html',
  styleUrls: ['./audit-module-detail.component.scss']
})

export class AuditModuleDetailComponent implements OnInit, CanComponentDeactivate {

  auditModuleForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });


  public auditModuleId: string | null = null;
  public auditModuleData: AuditModuleData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditModules$ = this.auditModuleService.GetAuditModuleList();
  auditModuleEntities$ = this.auditModuleEntityService.GetAuditModuleEntityList();
  auditEvents$ = this.auditEventService.GetAuditEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditModuleService: AuditModuleService,
    public auditModuleEntityService: AuditModuleEntityService,
    public auditEventService: AuditEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditModuleId from the route parameters
    this.auditModuleId = this.route.snapshot.paramMap.get('auditModuleId');

    if (this.auditModuleId === 'new' ||
        this.auditModuleId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditModuleData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit Module';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit Module';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditModuleForm.dirty) {
      return confirm('You have unsaved Audit Module changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditModuleId != null && this.auditModuleId !== 'new') {

      const id = parseInt(this.auditModuleId, 10);

      if (!isNaN(id)) {
        return { auditModuleId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditModule data for the current auditModuleId.
  *
  * Fully respects the AuditModuleService caching strategy and error handling strategy.
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
    if (!this.auditModuleService.userIsAuditorAuditModuleReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditModules.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditModuleId
    //
    if (!this.auditModuleId) {

      this.alertService.showMessage('No AuditModule ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditModuleId = Number(this.auditModuleId);

    if (isNaN(auditModuleId) || auditModuleId <= 0) {

      this.alertService.showMessage(`Invalid Audit Module ID: "${this.auditModuleId}"`,
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
      // This is the most targeted way: clear only this AuditModule + relations

      this.auditModuleService.ClearRecordCache(auditModuleId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditModuleService.GetAuditModule(auditModuleId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditModuleData) => {

        //
        // Success path — auditModuleData can legitimately be null if 404'd but request succeeded
        //
        if (!auditModuleData) {

          this.handleAuditModuleNotFound(auditModuleId);

        } else {

          this.auditModuleData = auditModuleData;
          this.buildFormValues(this.auditModuleData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditModule loaded successfully',
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
        this.handleAuditModuleLoadError(error, auditModuleId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditModuleNotFound(auditModuleId: number): void {

    this.auditModuleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditModule #${auditModuleId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditModuleLoadError(error: any, auditModuleId: number): void {

    let message = 'Failed to load Audit Module.';
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
          message = 'You do not have permission to view this Audit Module.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit Module #${auditModuleId} was not found.`;
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

    console.error(`Audit Module load failed (ID: ${auditModuleId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditModuleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditModuleData: AuditModuleData | null) {

    if (auditModuleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditModuleForm.reset({
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditModuleForm.reset({
        name: auditModuleData.name ?? '',
        comments: auditModuleData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditModuleData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditModuleForm.markAsPristine();
    this.auditModuleForm.markAsUntouched();
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

    if (this.auditModuleService.userIsAuditorAuditModuleWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Modules", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditModuleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditModuleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditModuleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditModuleSubmitData: AuditModuleSubmitData = {
        id: this.auditModuleData?.id || 0,
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditModuleService.PutAuditModule(auditModuleSubmitData.id, auditModuleSubmitData)
      : this.auditModuleService.PostAuditModule(auditModuleSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditModuleData) => {

        this.auditModuleService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit Module's detail page
          //
          this.auditModuleForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditModuleForm.markAsUntouched();

          this.router.navigate(['/auditmodules', savedAuditModuleData.id]);
          this.alertService.showMessage('Audit Module added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditModuleData = savedAuditModuleData;
          this.buildFormValues(this.auditModuleData);

          this.alertService.showMessage("Audit Module saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Audit Module.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Module.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Module could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditModuleReader(): boolean {
    return this.auditModuleService.userIsAuditorAuditModuleReader();
  }

  public userIsAuditorAuditModuleWriter(): boolean {
    return this.auditModuleService.userIsAuditorAuditModuleWriter();
  }
}
