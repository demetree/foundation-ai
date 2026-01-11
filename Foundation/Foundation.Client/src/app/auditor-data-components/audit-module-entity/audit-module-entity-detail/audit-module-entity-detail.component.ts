import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditModuleEntityService, AuditModuleEntityData, AuditModuleEntitySubmitData } from '../../../auditor-data-services/audit-module-entity.service';
import { AuditModuleService } from '../../../auditor-data-services/audit-module.service';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-audit-module-entity-detail',
  templateUrl: './audit-module-entity-detail.component.html',
  styleUrls: ['./audit-module-entity-detail.component.scss']
})

export class AuditModuleEntityDetailComponent implements OnInit, CanComponentDeactivate {

  auditModuleEntityForm: FormGroup = this.fb.group({
        auditModuleId: [null, Validators.required],
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });


  public auditModuleEntityId: string | null = null;
  public auditModuleEntityData: AuditModuleEntityData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditModuleEntities$ = this.auditModuleEntityService.GetAuditModuleEntityList();
  auditModules$ = this.auditModuleService.GetAuditModuleList();
  auditEvents$ = this.auditEventService.GetAuditEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditModuleEntityService: AuditModuleEntityService,
    public auditModuleService: AuditModuleService,
    public auditEventService: AuditEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditModuleEntityId from the route parameters
    this.auditModuleEntityId = this.route.snapshot.paramMap.get('auditModuleEntityId');

    if (this.auditModuleEntityId === 'new' ||
        this.auditModuleEntityId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditModuleEntityData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit Module Entity';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit Module Entity';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditModuleEntityForm.dirty) {
      return confirm('You have unsaved Audit Module Entity changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditModuleEntityId != null && this.auditModuleEntityId !== 'new') {

      const id = parseInt(this.auditModuleEntityId, 10);

      if (!isNaN(id)) {
        return { auditModuleEntityId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditModuleEntity data for the current auditModuleEntityId.
  *
  * Fully respects the AuditModuleEntityService caching strategy and error handling strategy.
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
    if (!this.auditModuleEntityService.userIsAuditorAuditModuleEntityReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditModuleEntities.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditModuleEntityId
    //
    if (!this.auditModuleEntityId) {

      this.alertService.showMessage('No AuditModuleEntity ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditModuleEntityId = Number(this.auditModuleEntityId);

    if (isNaN(auditModuleEntityId) || auditModuleEntityId <= 0) {

      this.alertService.showMessage(`Invalid Audit Module Entity ID: "${this.auditModuleEntityId}"`,
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
      // This is the most targeted way: clear only this AuditModuleEntity + relations

      this.auditModuleEntityService.ClearRecordCache(auditModuleEntityId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditModuleEntityService.GetAuditModuleEntity(auditModuleEntityId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditModuleEntityData) => {

        //
        // Success path — auditModuleEntityData can legitimately be null if 404'd but request succeeded
        //
        if (!auditModuleEntityData) {

          this.handleAuditModuleEntityNotFound(auditModuleEntityId);

        } else {

          this.auditModuleEntityData = auditModuleEntityData;
          this.buildFormValues(this.auditModuleEntityData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditModuleEntity loaded successfully',
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
        this.handleAuditModuleEntityLoadError(error, auditModuleEntityId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditModuleEntityNotFound(auditModuleEntityId: number): void {

    this.auditModuleEntityData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditModuleEntity #${auditModuleEntityId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditModuleEntityLoadError(error: any, auditModuleEntityId: number): void {

    let message = 'Failed to load Audit Module Entity.';
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
          message = 'You do not have permission to view this Audit Module Entity.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit Module Entity #${auditModuleEntityId} was not found.`;
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

    console.error(`Audit Module Entity load failed (ID: ${auditModuleEntityId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditModuleEntityData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditModuleEntityData: AuditModuleEntityData | null) {

    if (auditModuleEntityData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditModuleEntityForm.reset({
        auditModuleId: null,
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditModuleEntityForm.reset({
        auditModuleId: auditModuleEntityData.auditModuleId,
        name: auditModuleEntityData.name ?? '',
        comments: auditModuleEntityData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditModuleEntityData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditModuleEntityForm.markAsPristine();
    this.auditModuleEntityForm.markAsUntouched();
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

    if (this.auditModuleEntityService.userIsAuditorAuditModuleEntityWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Module Entities", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditModuleEntityForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditModuleEntityForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditModuleEntityForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditModuleEntitySubmitData: AuditModuleEntitySubmitData = {
        id: this.auditModuleEntityData?.id || 0,
        auditModuleId: Number(formValue.auditModuleId),
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditModuleEntityService.PutAuditModuleEntity(auditModuleEntitySubmitData.id, auditModuleEntitySubmitData)
      : this.auditModuleEntityService.PostAuditModuleEntity(auditModuleEntitySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditModuleEntityData) => {

        this.auditModuleEntityService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit Module Entity's detail page
          //
          this.auditModuleEntityForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditModuleEntityForm.markAsUntouched();

          this.router.navigate(['/auditmoduleentities', savedAuditModuleEntityData.id]);
          this.alertService.showMessage('Audit Module Entity added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditModuleEntityData = savedAuditModuleEntityData;
          this.buildFormValues(this.auditModuleEntityData);

          this.alertService.showMessage("Audit Module Entity saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Audit Module Entity.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Module Entity.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Module Entity could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditModuleEntityReader(): boolean {
    return this.auditModuleEntityService.userIsAuditorAuditModuleEntityReader();
  }

  public userIsAuditorAuditModuleEntityWriter(): boolean {
    return this.auditModuleEntityService.userIsAuditorAuditModuleEntityWriter();
  }
}
