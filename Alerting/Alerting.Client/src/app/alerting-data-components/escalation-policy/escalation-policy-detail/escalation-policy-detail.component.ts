/*
   GENERATED FORM FOR THE ESCALATIONPOLICY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from EscalationPolicy table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to escalation-policy-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EscalationPolicyService, EscalationPolicyData, EscalationPolicySubmitData } from '../../../alerting-data-services/escalation-policy.service';
import { EscalationPolicyChangeHistoryService } from '../../../alerting-data-services/escalation-policy-change-history.service';
import { ServiceService } from '../../../alerting-data-services/service.service';
import { EscalationRuleService } from '../../../alerting-data-services/escalation-rule.service';
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
interface EscalationPolicyFormValues {
  name: string,
  description: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-escalation-policy-detail',
  templateUrl: './escalation-policy-detail.component.html',
  styleUrls: ['./escalation-policy-detail.component.scss']
})

export class EscalationPolicyDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<EscalationPolicyFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public escalationPolicyForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public escalationPolicyId: string | null = null;
  public escalationPolicyData: EscalationPolicyData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  escalationPolicies$ = this.escalationPolicyService.GetEscalationPolicyList();
  public escalationPolicyChangeHistories$ = this.escalationPolicyChangeHistoryService.GetEscalationPolicyChangeHistoryList();
  public services$ = this.serviceService.GetServiceList();
  public escalationRules$ = this.escalationRuleService.GetEscalationRuleList();

  private destroy$ = new Subject<void>();

  constructor(
    public escalationPolicyService: EscalationPolicyService,
    public escalationPolicyChangeHistoryService: EscalationPolicyChangeHistoryService,
    public serviceService: ServiceService,
    public escalationRuleService: EscalationRuleService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the escalationPolicyId from the route parameters
    this.escalationPolicyId = this.route.snapshot.paramMap.get('escalationPolicyId');

    if (this.escalationPolicyId === 'new' ||
        this.escalationPolicyId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.escalationPolicyData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.escalationPolicyForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.escalationPolicyForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Escalation Policy';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Escalation Policy';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.escalationPolicyForm.dirty) {
      return confirm('You have unsaved Escalation Policy changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.escalationPolicyId != null && this.escalationPolicyId !== 'new') {

      const id = parseInt(this.escalationPolicyId, 10);

      if (!isNaN(id)) {
        return { escalationPolicyId: id };
      }
    }

    return null;
  }


/*
  * Loads the EscalationPolicy data for the current escalationPolicyId.
  *
  * Fully respects the EscalationPolicyService caching strategy and error handling strategy.
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
    if (!this.escalationPolicyService.userIsAlertingEscalationPolicyReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read EscalationPolicies.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate escalationPolicyId
    //
    if (!this.escalationPolicyId) {

      this.alertService.showMessage('No EscalationPolicy ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const escalationPolicyId = Number(this.escalationPolicyId);

    if (isNaN(escalationPolicyId) || escalationPolicyId <= 0) {

      this.alertService.showMessage(`Invalid Escalation Policy ID: "${this.escalationPolicyId}"`,
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
      // This is the most targeted way: clear only this EscalationPolicy + relations

      this.escalationPolicyService.ClearRecordCache(escalationPolicyId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.escalationPolicyService.GetEscalationPolicy(escalationPolicyId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (escalationPolicyData) => {

        //
        // Success path — escalationPolicyData can legitimately be null if 404'd but request succeeded
        //
        if (!escalationPolicyData) {

          this.handleEscalationPolicyNotFound(escalationPolicyId);

        } else {

          this.escalationPolicyData = escalationPolicyData;
          this.buildFormValues(this.escalationPolicyData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'EscalationPolicy loaded successfully',
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
        this.handleEscalationPolicyLoadError(error, escalationPolicyId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleEscalationPolicyNotFound(escalationPolicyId: number): void {

    this.escalationPolicyData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `EscalationPolicy #${escalationPolicyId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleEscalationPolicyLoadError(error: any, escalationPolicyId: number): void {

    let message = 'Failed to load Escalation Policy.';
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
          message = 'You do not have permission to view this Escalation Policy.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Escalation Policy #${escalationPolicyId} was not found.`;
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

    console.error(`Escalation Policy load failed (ID: ${escalationPolicyId})`, error);

    //
    // Reset UI to safe state
    //
    this.escalationPolicyData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(escalationPolicyData: EscalationPolicyData | null) {

    if (escalationPolicyData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.escalationPolicyForm.reset({
        name: '',
        description: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.escalationPolicyForm.reset({
        name: escalationPolicyData.name ?? '',
        description: escalationPolicyData.description ?? '',
        versionNumber: escalationPolicyData.versionNumber?.toString() ?? '',
        active: escalationPolicyData.active ?? true,
        deleted: escalationPolicyData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.escalationPolicyForm.markAsPristine();
    this.escalationPolicyForm.markAsUntouched();
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

    if (this.escalationPolicyService.userIsAlertingEscalationPolicyWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Escalation Policies", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.escalationPolicyForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.escalationPolicyForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.escalationPolicyForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const escalationPolicySubmitData: EscalationPolicySubmitData = {
        id: this.escalationPolicyData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        versionNumber: this.escalationPolicyData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.escalationPolicyService.PutEscalationPolicy(escalationPolicySubmitData.id, escalationPolicySubmitData)
      : this.escalationPolicyService.PostEscalationPolicy(escalationPolicySubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedEscalationPolicyData) => {

        this.escalationPolicyService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Escalation Policy's detail page
          //
          this.escalationPolicyForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.escalationPolicyForm.markAsUntouched();

          this.router.navigate(['/escalationpolicies', savedEscalationPolicyData.id]);
          this.alertService.showMessage('Escalation Policy added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.escalationPolicyData = savedEscalationPolicyData;
          this.buildFormValues(this.escalationPolicyData);

          this.alertService.showMessage("Escalation Policy saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Escalation Policy.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Escalation Policy.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Escalation Policy could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsAlertingEscalationPolicyReader(): boolean {
    return this.escalationPolicyService.userIsAlertingEscalationPolicyReader();
  }

  public userIsAlertingEscalationPolicyWriter(): boolean {
    return this.escalationPolicyService.userIsAlertingEscalationPolicyWriter();
  }
}
