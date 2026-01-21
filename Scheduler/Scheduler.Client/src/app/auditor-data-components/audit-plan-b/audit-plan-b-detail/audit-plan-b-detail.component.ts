/*
   GENERATED FORM FOR THE AUDITPLANB TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AuditPlanB table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to audit-plan-b-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditPlanBService, AuditPlanBData, AuditPlanBSubmitData } from '../../../auditor-data-services/audit-plan-b.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface AuditPlanBFormValues {
  startTime: string,
  stopTime: string,
  completedSuccessfully: boolean,
  user: string | null,
  session: string | null,
  type: string | null,
  accessType: string | null,
  source: string | null,
  userAgent: string | null,
  module: string | null,
  moduleEntity: string | null,
  resource: string | null,
  hostSystem: string | null,
  primaryKey: string | null,
  threadId: string | null,     // Stored as string for form input, converted to number on submit.
  message: string | null,
  beforeState: string | null,
  afterState: string | null,
  errorMessage: string | null,
  exceptionText: string | null,
};


@Component({
  selector: 'app-audit-plan-b-detail',
  templateUrl: './audit-plan-b-detail.component.html',
  styleUrls: ['./audit-plan-b-detail.component.scss']
})

export class AuditPlanBDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AuditPlanBFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public auditPlanBForm: FormGroup = this.fb.group({
        startTime: ['', Validators.required],
        stopTime: ['', Validators.required],
        completedSuccessfully: [false],
        user: [''],
        session: [''],
        type: [''],
        accessType: [''],
        source: [''],
        userAgent: [''],
        module: [''],
        moduleEntity: [''],
        resource: [''],
        hostSystem: [''],
        primaryKey: [''],
        threadId: [''],
        message: [''],
        beforeState: [''],
        afterState: [''],
        errorMessage: [''],
        exceptionText: [''],
      });


  public auditPlanBId: string | null = null;
  public auditPlanBData: AuditPlanBData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditPlanBs$ = this.auditPlanBService.GetAuditPlanBList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditPlanBService: AuditPlanBService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditPlanBId from the route parameters
    this.auditPlanBId = this.route.snapshot.paramMap.get('auditPlanBId');

    if (this.auditPlanBId === 'new' ||
        this.auditPlanBId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditPlanBData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.auditPlanBForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.auditPlanBForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit Plan B';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit Plan B';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditPlanBForm.dirty) {
      return confirm('You have unsaved Audit Plan B changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditPlanBId != null && this.auditPlanBId !== 'new') {

      const id = parseInt(this.auditPlanBId, 10);

      if (!isNaN(id)) {
        return { auditPlanBId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditPlanB data for the current auditPlanBId.
  *
  * Fully respects the AuditPlanBService caching strategy and error handling strategy.
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
    if (!this.auditPlanBService.userIsAuditorAuditPlanBReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditPlanBs.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditPlanBId
    //
    if (!this.auditPlanBId) {

      this.alertService.showMessage('No AuditPlanB ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditPlanBId = Number(this.auditPlanBId);

    if (isNaN(auditPlanBId) || auditPlanBId <= 0) {

      this.alertService.showMessage(`Invalid Audit Plan B ID: "${this.auditPlanBId}"`,
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
      // This is the most targeted way: clear only this AuditPlanB + relations

      this.auditPlanBService.ClearRecordCache(auditPlanBId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditPlanBService.GetAuditPlanB(auditPlanBId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditPlanBData) => {

        //
        // Success path — auditPlanBData can legitimately be null if 404'd but request succeeded
        //
        if (!auditPlanBData) {

          this.handleAuditPlanBNotFound(auditPlanBId);

        } else {

          this.auditPlanBData = auditPlanBData;
          this.buildFormValues(this.auditPlanBData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditPlanB loaded successfully',
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
        this.handleAuditPlanBLoadError(error, auditPlanBId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditPlanBNotFound(auditPlanBId: number): void {

    this.auditPlanBData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditPlanB #${auditPlanBId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditPlanBLoadError(error: any, auditPlanBId: number): void {

    let message = 'Failed to load Audit Plan B.';
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
          message = 'You do not have permission to view this Audit Plan B.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit Plan B #${auditPlanBId} was not found.`;
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

    console.error(`Audit Plan B load failed (ID: ${auditPlanBId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditPlanBData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditPlanBData: AuditPlanBData | null) {

    if (auditPlanBData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditPlanBForm.reset({
        startTime: '',
        stopTime: '',
        completedSuccessfully: false,
        user: '',
        session: '',
        type: '',
        accessType: '',
        source: '',
        userAgent: '',
        module: '',
        moduleEntity: '',
        resource: '',
        hostSystem: '',
        primaryKey: '',
        threadId: '',
        message: '',
        beforeState: '',
        afterState: '',
        errorMessage: '',
        exceptionText: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditPlanBForm.reset({
        startTime: isoUtcStringToDateTimeLocal(auditPlanBData.startTime) ?? '',
        stopTime: isoUtcStringToDateTimeLocal(auditPlanBData.stopTime) ?? '',
        completedSuccessfully: auditPlanBData.completedSuccessfully ?? false,
        user: auditPlanBData.user ?? '',
        session: auditPlanBData.session ?? '',
        type: auditPlanBData.type ?? '',
        accessType: auditPlanBData.accessType ?? '',
        source: auditPlanBData.source ?? '',
        userAgent: auditPlanBData.userAgent ?? '',
        module: auditPlanBData.module ?? '',
        moduleEntity: auditPlanBData.moduleEntity ?? '',
        resource: auditPlanBData.resource ?? '',
        hostSystem: auditPlanBData.hostSystem ?? '',
        primaryKey: auditPlanBData.primaryKey ?? '',
        threadId: auditPlanBData.threadId?.toString() ?? '',
        message: auditPlanBData.message ?? '',
        beforeState: auditPlanBData.beforeState ?? '',
        afterState: auditPlanBData.afterState ?? '',
        errorMessage: auditPlanBData.errorMessage ?? '',
        exceptionText: auditPlanBData.exceptionText ?? '',
      }, { emitEvent: false});
    }

    this.auditPlanBForm.markAsPristine();
    this.auditPlanBForm.markAsUntouched();
  }

  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  //
  // Helper method to determine if a field should be hidden based on the hiddenFields input.
  // Returns true if the field is in the array, false otherwise.
  //
  public isFieldHidden(fieldName: string): boolean {
    // Explicit check for array existence to avoid runtime errors.
    if (this.hiddenFields === null || this.hiddenFields === undefined) {
      return false;
    }
    // Use traditional includes method for clarity.
    return this.hiddenFields.includes(fieldName);
  }


  public submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.auditPlanBService.userIsAuditorAuditPlanBWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Plan Bs", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditPlanBForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditPlanBForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditPlanBForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditPlanBSubmitData: AuditPlanBSubmitData = {
        id: this.auditPlanBData?.id || 0,
        startTime: dateTimeLocalToIsoUtc(formValue.startTime!.trim())!,
        stopTime: dateTimeLocalToIsoUtc(formValue.stopTime!.trim())!,
        completedSuccessfully: !!formValue.completedSuccessfully,
        user: formValue.user?.trim() || null,
        session: formValue.session?.trim() || null,
        type: formValue.type?.trim() || null,
        accessType: formValue.accessType?.trim() || null,
        source: formValue.source?.trim() || null,
        userAgent: formValue.userAgent?.trim() || null,
        module: formValue.module?.trim() || null,
        moduleEntity: formValue.moduleEntity?.trim() || null,
        resource: formValue.resource?.trim() || null,
        hostSystem: formValue.hostSystem?.trim() || null,
        primaryKey: formValue.primaryKey?.trim() || null,
        threadId: formValue.threadId ? Number(formValue.threadId) : null,
        message: formValue.message?.trim() || null,
        beforeState: formValue.beforeState?.trim() || null,
        afterState: formValue.afterState?.trim() || null,
        errorMessage: formValue.errorMessage?.trim() || null,
        exceptionText: formValue.exceptionText?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditPlanBService.PutAuditPlanB(auditPlanBSubmitData.id, auditPlanBSubmitData)
      : this.auditPlanBService.PostAuditPlanB(auditPlanBSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditPlanBData) => {

        this.auditPlanBService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit Plan B's detail page
          //
          this.auditPlanBForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditPlanBForm.markAsUntouched();

          this.router.navigate(['/auditplanbs', savedAuditPlanBData.id]);
          this.alertService.showMessage('Audit Plan B added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditPlanBData = savedAuditPlanBData;
          this.buildFormValues(this.auditPlanBData);

          this.alertService.showMessage("Audit Plan B saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Audit Plan B.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Plan B.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Plan B could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditPlanBReader(): boolean {
    return this.auditPlanBService.userIsAuditorAuditPlanBReader();
  }

  public userIsAuditorAuditPlanBWriter(): boolean {
    return this.auditPlanBService.userIsAuditorAuditPlanBWriter();
  }
}
