/*
   GENERATED FORM FOR THE AUDITEVENTENTITYSTATE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AuditEventEntityState table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to audit-event-entity-state-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditEventEntityStateService, AuditEventEntityStateData, AuditEventEntityStateSubmitData } from '../../../auditor-data-services/audit-event-entity-state.service';
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
interface AuditEventEntityStateFormValues {
  auditEventId: number | bigint,       // For FK link number
  beforeState: string | null,
  afterState: string | null,
};


@Component({
  selector: 'app-audit-event-entity-state-detail',
  templateUrl: './audit-event-entity-state-detail.component.html',
  styleUrls: ['./audit-event-entity-state-detail.component.scss']
})

export class AuditEventEntityStateDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AuditEventEntityStateFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public auditEventEntityStateForm: FormGroup = this.fb.group({
        auditEventId: [null, Validators.required],
        beforeState: [''],
        afterState: [''],
      });


  public auditEventEntityStateId: string | null = null;
  public auditEventEntityStateData: AuditEventEntityStateData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  auditEventEntityStates$ = this.auditEventEntityStateService.GetAuditEventEntityStateList();
  public auditEvents$ = this.auditEventService.GetAuditEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public auditEventEntityStateService: AuditEventEntityStateService,
    public auditEventService: AuditEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the auditEventEntityStateId from the route parameters
    this.auditEventEntityStateId = this.route.snapshot.paramMap.get('auditEventEntityStateId');

    if (this.auditEventEntityStateId === 'new' ||
        this.auditEventEntityStateId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.auditEventEntityStateData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.auditEventEntityStateForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.auditEventEntityStateForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Audit Event Entity State';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Audit Event Entity State';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.auditEventEntityStateForm.dirty) {
      return confirm('You have unsaved Audit Event Entity State changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.auditEventEntityStateId != null && this.auditEventEntityStateId !== 'new') {

      const id = parseInt(this.auditEventEntityStateId, 10);

      if (!isNaN(id)) {
        return { auditEventEntityStateId: id };
      }
    }

    return null;
  }


/*
  * Loads the AuditEventEntityState data for the current auditEventEntityStateId.
  *
  * Fully respects the AuditEventEntityStateService caching strategy and error handling strategy.
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
    if (!this.auditEventEntityStateService.userIsAuditorAuditEventEntityStateReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AuditEventEntityStates.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate auditEventEntityStateId
    //
    if (!this.auditEventEntityStateId) {

      this.alertService.showMessage('No AuditEventEntityState ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const auditEventEntityStateId = Number(this.auditEventEntityStateId);

    if (isNaN(auditEventEntityStateId) || auditEventEntityStateId <= 0) {

      this.alertService.showMessage(`Invalid Audit Event Entity State ID: "${this.auditEventEntityStateId}"`,
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
      // This is the most targeted way: clear only this AuditEventEntityState + relations

      this.auditEventEntityStateService.ClearRecordCache(auditEventEntityStateId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.auditEventEntityStateService.GetAuditEventEntityState(auditEventEntityStateId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (auditEventEntityStateData) => {

        //
        // Success path — auditEventEntityStateData can legitimately be null if 404'd but request succeeded
        //
        if (!auditEventEntityStateData) {

          this.handleAuditEventEntityStateNotFound(auditEventEntityStateId);

        } else {

          this.auditEventEntityStateData = auditEventEntityStateData;
          this.buildFormValues(this.auditEventEntityStateData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AuditEventEntityState loaded successfully',
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
        this.handleAuditEventEntityStateLoadError(error, auditEventEntityStateId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAuditEventEntityStateNotFound(auditEventEntityStateId: number): void {

    this.auditEventEntityStateData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AuditEventEntityState #${auditEventEntityStateId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAuditEventEntityStateLoadError(error: any, auditEventEntityStateId: number): void {

    let message = 'Failed to load Audit Event Entity State.';
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
          message = 'You do not have permission to view this Audit Event Entity State.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Audit Event Entity State #${auditEventEntityStateId} was not found.`;
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

    console.error(`Audit Event Entity State load failed (ID: ${auditEventEntityStateId})`, error);

    //
    // Reset UI to safe state
    //
    this.auditEventEntityStateData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(auditEventEntityStateData: AuditEventEntityStateData | null) {

    if (auditEventEntityStateData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditEventEntityStateForm.reset({
        auditEventId: null,
        beforeState: '',
        afterState: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditEventEntityStateForm.reset({
        auditEventId: auditEventEntityStateData.auditEventId,
        beforeState: auditEventEntityStateData.beforeState ?? '',
        afterState: auditEventEntityStateData.afterState ?? '',
      }, { emitEvent: false});
    }

    this.auditEventEntityStateForm.markAsPristine();
    this.auditEventEntityStateForm.markAsUntouched();
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

    if (this.auditEventEntityStateService.userIsAuditorAuditEventEntityStateWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Audit Event Entity States", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.auditEventEntityStateForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditEventEntityStateForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditEventEntityStateForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditEventEntityStateSubmitData: AuditEventEntityStateSubmitData = {
        id: this.auditEventEntityStateData?.id || 0,
        auditEventId: Number(formValue.auditEventId),
        beforeState: formValue.beforeState?.trim() || null,
        afterState: formValue.afterState?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.auditEventEntityStateService.PutAuditEventEntityState(auditEventEntityStateSubmitData.id, auditEventEntityStateSubmitData)
      : this.auditEventEntityStateService.PostAuditEventEntityState(auditEventEntityStateSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAuditEventEntityStateData) => {

        this.auditEventEntityStateService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Audit Event Entity State's detail page
          //
          this.auditEventEntityStateForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.auditEventEntityStateForm.markAsUntouched();

          this.router.navigate(['/auditevententitystates', savedAuditEventEntityStateData.id]);
          this.alertService.showMessage('Audit Event Entity State added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.auditEventEntityStateData = savedAuditEventEntityStateData;
          this.buildFormValues(this.auditEventEntityStateData);

          this.alertService.showMessage("Audit Event Entity State saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Audit Event Entity State.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Event Entity State.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Event Entity State could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAuditorAuditEventEntityStateReader(): boolean {
    return this.auditEventEntityStateService.userIsAuditorAuditEventEntityStateReader();
  }

  public userIsAuditorAuditEventEntityStateWriter(): boolean {
    return this.auditEventEntityStateService.userIsAuditorAuditEventEntityStateWriter();
  }
}
