/*
   GENERATED FORM FOR THE QUALIFICATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Qualification table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to qualification-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { QualificationService, QualificationData, QualificationSubmitData } from '../../../scheduler-data-services/qualification.service';
import { AssignmentRoleQualificationRequirementService } from '../../../scheduler-data-services/assignment-role-qualification-requirement.service';
import { SchedulingTargetQualificationRequirementService } from '../../../scheduler-data-services/scheduling-target-qualification-requirement.service';
import { ResourceQualificationService } from '../../../scheduler-data-services/resource-qualification.service';
import { ScheduledEventTemplateQualificationRequirementService } from '../../../scheduler-data-services/scheduled-event-template-qualification-requirement.service';
import { ScheduledEventQualificationRequirementService } from '../../../scheduler-data-services/scheduled-event-qualification-requirement.service';
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
interface QualificationFormValues {
  name: string,
  description: string,
  isLicense: boolean | null,
  color: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-qualification-detail',
  templateUrl: './qualification-detail.component.html',
  styleUrls: ['./qualification-detail.component.scss']
})

export class QualificationDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<QualificationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public qualificationForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        isLicense: [false],
        color: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public qualificationId: string | null = null;
  public qualificationData: QualificationData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  qualifications$ = this.qualificationService.GetQualificationList();
  public assignmentRoleQualificationRequirements$ = this.assignmentRoleQualificationRequirementService.GetAssignmentRoleQualificationRequirementList();
  public schedulingTargetQualificationRequirements$ = this.schedulingTargetQualificationRequirementService.GetSchedulingTargetQualificationRequirementList();
  public resourceQualifications$ = this.resourceQualificationService.GetResourceQualificationList();
  public scheduledEventTemplateQualificationRequirements$ = this.scheduledEventTemplateQualificationRequirementService.GetScheduledEventTemplateQualificationRequirementList();
  public scheduledEventQualificationRequirements$ = this.scheduledEventQualificationRequirementService.GetScheduledEventQualificationRequirementList();

  private destroy$ = new Subject<void>();

  constructor(
    public qualificationService: QualificationService,
    public assignmentRoleQualificationRequirementService: AssignmentRoleQualificationRequirementService,
    public schedulingTargetQualificationRequirementService: SchedulingTargetQualificationRequirementService,
    public resourceQualificationService: ResourceQualificationService,
    public scheduledEventTemplateQualificationRequirementService: ScheduledEventTemplateQualificationRequirementService,
    public scheduledEventQualificationRequirementService: ScheduledEventQualificationRequirementService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the qualificationId from the route parameters
    this.qualificationId = this.route.snapshot.paramMap.get('qualificationId');

    if (this.qualificationId === 'new' ||
        this.qualificationId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.qualificationData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.qualificationForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.qualificationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Qualification';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Qualification';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.qualificationForm.dirty) {
      return confirm('You have unsaved Qualification changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.qualificationId != null && this.qualificationId !== 'new') {

      const id = parseInt(this.qualificationId, 10);

      if (!isNaN(id)) {
        return { qualificationId: id };
      }
    }

    return null;
  }


/*
  * Loads the Qualification data for the current qualificationId.
  *
  * Fully respects the QualificationService caching strategy and error handling strategy.
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
    if (!this.qualificationService.userIsSchedulerQualificationReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Qualifications.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate qualificationId
    //
    if (!this.qualificationId) {

      this.alertService.showMessage('No Qualification ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const qualificationId = Number(this.qualificationId);

    if (isNaN(qualificationId) || qualificationId <= 0) {

      this.alertService.showMessage(`Invalid Qualification ID: "${this.qualificationId}"`,
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
      // This is the most targeted way: clear only this Qualification + relations

      this.qualificationService.ClearRecordCache(qualificationId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.qualificationService.GetQualification(qualificationId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (qualificationData) => {

        //
        // Success path — qualificationData can legitimately be null if 404'd but request succeeded
        //
        if (!qualificationData) {

          this.handleQualificationNotFound(qualificationId);

        } else {

          this.qualificationData = qualificationData;
          this.buildFormValues(this.qualificationData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Qualification loaded successfully',
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
        this.handleQualificationLoadError(error, qualificationId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleQualificationNotFound(qualificationId: number): void {

    this.qualificationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `Qualification #${qualificationId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleQualificationLoadError(error: any, qualificationId: number): void {

    let message = 'Failed to load Qualification.';
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
          message = 'You do not have permission to view this Qualification.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Qualification #${qualificationId} was not found.`;
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

    console.error(`Qualification load failed (ID: ${qualificationId})`, error);

    //
    // Reset UI to safe state
    //
    this.qualificationData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(qualificationData: QualificationData | null) {

    if (qualificationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.qualificationForm.reset({
        name: '',
        description: '',
        isLicense: false,
        color: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.qualificationForm.reset({
        name: qualificationData.name ?? '',
        description: qualificationData.description ?? '',
        isLicense: qualificationData.isLicense ?? false,
        color: qualificationData.color ?? '',
        sequence: qualificationData.sequence?.toString() ?? '',
        active: qualificationData.active ?? true,
        deleted: qualificationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.qualificationForm.markAsPristine();
    this.qualificationForm.markAsUntouched();
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

    if (this.qualificationService.userIsSchedulerQualificationWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Qualifications", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.qualificationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.qualificationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.qualificationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const qualificationSubmitData: QualificationSubmitData = {
        id: this.qualificationData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        isLicense: formValue.isLicense == true ? true : formValue.isLicense == false ? false : null,
        color: formValue.color?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.qualificationService.PutQualification(qualificationSubmitData.id, qualificationSubmitData)
      : this.qualificationService.PostQualification(qualificationSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedQualificationData) => {

        this.qualificationService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Qualification's detail page
          //
          this.qualificationForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.qualificationForm.markAsUntouched();

          this.router.navigate(['/qualifications', savedQualificationData.id]);
          this.alertService.showMessage('Qualification added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.qualificationData = savedQualificationData;
          this.buildFormValues(this.qualificationData);

          this.alertService.showMessage("Qualification saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Qualification.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Qualification.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Qualification could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerQualificationReader(): boolean {
    return this.qualificationService.userIsSchedulerQualificationReader();
  }

  public userIsSchedulerQualificationWriter(): boolean {
    return this.qualificationService.userIsSchedulerQualificationWriter();
  }
}
