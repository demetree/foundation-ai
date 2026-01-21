/*
   GENERATED FORM FOR THE AUDITTYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AuditType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to audit-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditTypeService, AuditTypeData, AuditTypeSubmitData } from '../../../auditor-data-services/audit-type.service';
import { AuditEventService } from '../../../auditor-data-services/audit-event.service';
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
interface AuditTypeFormValues {
  name: string,
  description: string | null,
};


@Component({
  selector: 'app-audit-type-detail',
  templateUrl: './audit-type-detail.component.html',
  styleUrls: ['./audit-type-detail.component.scss']
})

export class AuditTypeDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AuditTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public auditTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
      });


  public auditTypeId: string | null = null;
  public auditTypeData: AuditTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditTypes$ = this.auditTypeService.GetAuditTypeList();
  public auditEvents$ = this.auditEventService.GetAuditEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditTypeService: AuditTypeService,
    public auditEventService: AuditEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditTypeId from the route parameters
    this.auditTypeId = this.route.snapshot.paramMap.get('auditTypeId');

    if (this.auditTypeId === 'new' ||
        this.auditTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditTypeData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.auditTypeForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.auditTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditTypeForm.dirty) {
      return confirm('You have unsaved Audit Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditTypeId != null && this.auditTypeId !== 'new') {

      const id = parseInt(this.auditTypeId, 10);

      if (!isNaN(id)) {
        return { auditTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditType data for the current auditTypeId.
  *
  * Fully respects the AuditTypeService caching strategy and error handling strategy.
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
    if (!this.auditTypeService.userIsAuditorAuditTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditTypeId
    //
    if (!this.auditTypeId) {

      this.alertService.showMessage('No AuditType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditTypeId = Number(this.auditTypeId);

    if (isNaN(auditTypeId) || auditTypeId <= 0) {

      this.alertService.showMessage(`Invalid Audit Type ID: "${this.auditTypeId}"`,
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
      // This is the most targeted way: clear only this AuditType + relations

      this.auditTypeService.ClearRecordCache(auditTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditTypeService.GetAuditType(auditTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditTypeData) => {

        //
        // Success path — auditTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!auditTypeData) {

          this.handleAuditTypeNotFound(auditTypeId);

        } else {

          this.auditTypeData = auditTypeData;
          this.buildFormValues(this.auditTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditType loaded successfully',
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
        this.handleAuditTypeLoadError(error, auditTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditTypeNotFound(auditTypeId: number): void {

    this.auditTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditType #${auditTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditTypeLoadError(error: any, auditTypeId: number): void {

    let message = 'Failed to load Audit Type.';
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
          message = 'You do not have permission to view this Audit Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit Type #${auditTypeId} was not found.`;
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

    console.error(`Audit Type load failed (ID: ${auditTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditTypeData: AuditTypeData | null) {

    if (auditTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditTypeForm.reset({
        name: '',
        description: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditTypeForm.reset({
        name: auditTypeData.name ?? '',
        description: auditTypeData.description ?? '',
      }, { emitEvent: false});
    }

    this.auditTypeForm.markAsPristine();
    this.auditTypeForm.markAsUntouched();
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

    if (this.auditTypeService.userIsAuditorAuditTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditTypeSubmitData: AuditTypeSubmitData = {
        id: this.auditTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditTypeService.PutAuditType(auditTypeSubmitData.id, auditTypeSubmitData)
      : this.auditTypeService.PostAuditType(auditTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditTypeData) => {

        this.auditTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit Type's detail page
          //
          this.auditTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditTypeForm.markAsUntouched();

          this.router.navigate(['/audittypes', savedAuditTypeData.id]);
          this.alertService.showMessage('Audit Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditTypeData = savedAuditTypeData;
          this.buildFormValues(this.auditTypeData);

          this.alertService.showMessage("Audit Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Audit Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditTypeReader(): boolean {
    return this.auditTypeService.userIsAuditorAuditTypeReader();
  }

  public userIsAuditorAuditTypeWriter(): boolean {
    return this.auditTypeService.userIsAuditorAuditTypeWriter();
  }
}
