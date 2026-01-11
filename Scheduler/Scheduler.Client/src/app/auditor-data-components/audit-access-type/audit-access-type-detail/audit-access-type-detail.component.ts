import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditAccessTypeService, AuditAccessTypeData, AuditAccessTypeSubmitData } from '../../../auditor-data-services/audit-access-type.service';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-audit-access-type-detail',
  templateUrl: './audit-access-type-detail.component.html',
  styleUrls: ['./audit-access-type-detail.component.scss']
})

export class AuditAccessTypeDetailComponent implements OnInit, CanComponentDeactivate {

  auditAccessTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
      });


  public auditAccessTypeId: string | null = null;
  public auditAccessTypeData: AuditAccessTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditAccessTypes$ = this.auditAccessTypeService.GetAuditAccessTypeList();
  auditEvents$ = this.auditEventService.GetAuditEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditAccessTypeService: AuditAccessTypeService,
    public auditEventService: AuditEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditAccessTypeId from the route parameters
    this.auditAccessTypeId = this.route.snapshot.paramMap.get('auditAccessTypeId');

    if (this.auditAccessTypeId === 'new' ||
        this.auditAccessTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditAccessTypeData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit Access Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit Access Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditAccessTypeForm.dirty) {
      return confirm('You have unsaved Audit Access Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditAccessTypeId != null && this.auditAccessTypeId !== 'new') {

      const id = parseInt(this.auditAccessTypeId, 10);

      if (!isNaN(id)) {
        return { auditAccessTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditAccessType data for the current auditAccessTypeId.
  *
  * Fully respects the AuditAccessTypeService caching strategy and error handling strategy.
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
    if (!this.auditAccessTypeService.userIsAuditorAuditAccessTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditAccessTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditAccessTypeId
    //
    if (!this.auditAccessTypeId) {

      this.alertService.showMessage('No AuditAccessType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditAccessTypeId = Number(this.auditAccessTypeId);

    if (isNaN(auditAccessTypeId) || auditAccessTypeId <= 0) {

      this.alertService.showMessage(`Invalid Audit Access Type ID: "${this.auditAccessTypeId}"`,
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
      // This is the most targeted way: clear only this AuditAccessType + relations

      this.auditAccessTypeService.ClearRecordCache(auditAccessTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditAccessTypeService.GetAuditAccessType(auditAccessTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditAccessTypeData) => {

        //
        // Success path — auditAccessTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!auditAccessTypeData) {

          this.handleAuditAccessTypeNotFound(auditAccessTypeId);

        } else {

          this.auditAccessTypeData = auditAccessTypeData;
          this.buildFormValues(this.auditAccessTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditAccessType loaded successfully',
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
        this.handleAuditAccessTypeLoadError(error, auditAccessTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditAccessTypeNotFound(auditAccessTypeId: number): void {

    this.auditAccessTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditAccessType #${auditAccessTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditAccessTypeLoadError(error: any, auditAccessTypeId: number): void {

    let message = 'Failed to load Audit Access Type.';
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
          message = 'You do not have permission to view this Audit Access Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit Access Type #${auditAccessTypeId} was not found.`;
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

    console.error(`Audit Access Type load failed (ID: ${auditAccessTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditAccessTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditAccessTypeData: AuditAccessTypeData | null) {

    if (auditAccessTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditAccessTypeForm.reset({
        name: '',
        description: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditAccessTypeForm.reset({
        name: auditAccessTypeData.name ?? '',
        description: auditAccessTypeData.description ?? '',
      }, { emitEvent: false});
    }

    this.auditAccessTypeForm.markAsPristine();
    this.auditAccessTypeForm.markAsUntouched();
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

    if (this.auditAccessTypeService.userIsAuditorAuditAccessTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Access Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditAccessTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditAccessTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditAccessTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditAccessTypeSubmitData: AuditAccessTypeSubmitData = {
        id: this.auditAccessTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditAccessTypeService.PutAuditAccessType(auditAccessTypeSubmitData.id, auditAccessTypeSubmitData)
      : this.auditAccessTypeService.PostAuditAccessType(auditAccessTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditAccessTypeData) => {

        this.auditAccessTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit Access Type's detail page
          //
          this.auditAccessTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditAccessTypeForm.markAsUntouched();

          this.router.navigate(['/auditaccesstypes', savedAuditAccessTypeData.id]);
          this.alertService.showMessage('Audit Access Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditAccessTypeData = savedAuditAccessTypeData;
          this.buildFormValues(this.auditAccessTypeData);

          this.alertService.showMessage("Audit Access Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Audit Access Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Access Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Access Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditAccessTypeReader(): boolean {
    return this.auditAccessTypeService.userIsAuditorAuditAccessTypeReader();
  }

  public userIsAuditorAuditAccessTypeWriter(): boolean {
    return this.auditAccessTypeService.userIsAuditorAuditAccessTypeWriter();
  }
}
