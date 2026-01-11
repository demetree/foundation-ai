/*
   GENERATED FORM FOR THE ASSIGNMENTROLEQUALIFICATIONREQUIREMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AssignmentRoleQualificationRequirement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to assignment-role-qualification-requirement-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AssignmentRoleQualificationRequirementService, AssignmentRoleQualificationRequirementData, AssignmentRoleQualificationRequirementSubmitData } from '../../../scheduler-data-services/assignment-role-qualification-requirement.service';
import { AssignmentRoleService } from '../../../scheduler-data-services/assignment-role.service';
import { QualificationService } from '../../../scheduler-data-services/qualification.service';
import { AssignmentRoleQualificationRequirementChangeHistoryService } from '../../../scheduler-data-services/assignment-role-qualification-requirement-change-history.service';
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
interface AssignmentRoleQualificationRequirementFormValues {
  assignmentRoleId: number | bigint,       // For FK link number
  qualificationId: number | bigint,       // For FK link number
  isRequired: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-assignment-role-qualification-requirement-detail',
  templateUrl: './assignment-role-qualification-requirement-detail.component.html',
  styleUrls: ['./assignment-role-qualification-requirement-detail.component.scss']
})

export class AssignmentRoleQualificationRequirementDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AssignmentRoleQualificationRequirementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public assignmentRoleQualificationRequirementForm: FormGroup = this.fb.group({
        assignmentRoleId: [null, Validators.required],
        qualificationId: [null, Validators.required],
        isRequired: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public assignmentRoleQualificationRequirementId: string | null = null;
  public assignmentRoleQualificationRequirementData: AssignmentRoleQualificationRequirementData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  assignmentRoleQualificationRequirements$ = this.assignmentRoleQualificationRequirementService.GetAssignmentRoleQualificationRequirementList();
  public assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  public qualifications$ = this.qualificationService.GetQualificationList();
  public assignmentRoleQualificationRequirementChangeHistories$ = this.assignmentRoleQualificationRequirementChangeHistoryService.GetAssignmentRoleQualificationRequirementChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public assignmentRoleQualificationRequirementService: AssignmentRoleQualificationRequirementService,
    public assignmentRoleService: AssignmentRoleService,
    public qualificationService: QualificationService,
    public assignmentRoleQualificationRequirementChangeHistoryService: AssignmentRoleQualificationRequirementChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the assignmentRoleQualificationRequirementId from the route parameters
    this.assignmentRoleQualificationRequirementId = this.route.snapshot.paramMap.get('assignmentRoleQualificationRequirementId');

    if (this.assignmentRoleQualificationRequirementId === 'new' ||
        this.assignmentRoleQualificationRequirementId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.assignmentRoleQualificationRequirementData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.assignmentRoleQualificationRequirementForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.assignmentRoleQualificationRequirementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Assignment Role Qualification Requirement';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Assignment Role Qualification Requirement';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.assignmentRoleQualificationRequirementForm.dirty) {
      return confirm('You have unsaved Assignment Role Qualification Requirement changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.assignmentRoleQualificationRequirementId != null && this.assignmentRoleQualificationRequirementId !== 'new') {

      const id = parseInt(this.assignmentRoleQualificationRequirementId, 10);

      if (!isNaN(id)) {
        return { assignmentRoleQualificationRequirementId: id };
      }
    }

    return null;
  }


/*
  * Loads the AssignmentRoleQualificationRequirement data for the current assignmentRoleQualificationRequirementId.
  *
  * Fully respects the AssignmentRoleQualificationRequirementService caching strategy and error handling strategy.
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
    if (!this.assignmentRoleQualificationRequirementService.userIsSchedulerAssignmentRoleQualificationRequirementReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AssignmentRoleQualificationRequirements.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate assignmentRoleQualificationRequirementId
    //
    if (!this.assignmentRoleQualificationRequirementId) {

      this.alertService.showMessage('No AssignmentRoleQualificationRequirement ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const assignmentRoleQualificationRequirementId = Number(this.assignmentRoleQualificationRequirementId);

    if (isNaN(assignmentRoleQualificationRequirementId) || assignmentRoleQualificationRequirementId <= 0) {

      this.alertService.showMessage(`Invalid Assignment Role Qualification Requirement ID: "${this.assignmentRoleQualificationRequirementId}"`,
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
      // This is the most targeted way: clear only this AssignmentRoleQualificationRequirement + relations

      this.assignmentRoleQualificationRequirementService.ClearRecordCache(assignmentRoleQualificationRequirementId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.assignmentRoleQualificationRequirementService.GetAssignmentRoleQualificationRequirement(assignmentRoleQualificationRequirementId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (assignmentRoleQualificationRequirementData) => {

        //
        // Success path — assignmentRoleQualificationRequirementData can legitimately be null if 404'd but request succeeded
        //
        if (!assignmentRoleQualificationRequirementData) {

          this.handleAssignmentRoleQualificationRequirementNotFound(assignmentRoleQualificationRequirementId);

        } else {

          this.assignmentRoleQualificationRequirementData = assignmentRoleQualificationRequirementData;
          this.buildFormValues(this.assignmentRoleQualificationRequirementData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AssignmentRoleQualificationRequirement loaded successfully',
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
        this.handleAssignmentRoleQualificationRequirementLoadError(error, assignmentRoleQualificationRequirementId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAssignmentRoleQualificationRequirementNotFound(assignmentRoleQualificationRequirementId: number): void {

    this.assignmentRoleQualificationRequirementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AssignmentRoleQualificationRequirement #${assignmentRoleQualificationRequirementId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAssignmentRoleQualificationRequirementLoadError(error: any, assignmentRoleQualificationRequirementId: number): void {

    let message = 'Failed to load Assignment Role Qualification Requirement.';
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
          message = 'You do not have permission to view this Assignment Role Qualification Requirement.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Assignment Role Qualification Requirement #${assignmentRoleQualificationRequirementId} was not found.`;
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

    console.error(`Assignment Role Qualification Requirement load failed (ID: ${assignmentRoleQualificationRequirementId})`, error);

    //
    // Reset UI to safe state
    //
    this.assignmentRoleQualificationRequirementData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(assignmentRoleQualificationRequirementData: AssignmentRoleQualificationRequirementData | null) {

    if (assignmentRoleQualificationRequirementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.assignmentRoleQualificationRequirementForm.reset({
        assignmentRoleId: null,
        qualificationId: null,
        isRequired: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.assignmentRoleQualificationRequirementForm.reset({
        assignmentRoleId: assignmentRoleQualificationRequirementData.assignmentRoleId,
        qualificationId: assignmentRoleQualificationRequirementData.qualificationId,
        isRequired: assignmentRoleQualificationRequirementData.isRequired ?? false,
        versionNumber: assignmentRoleQualificationRequirementData.versionNumber?.toString() ?? '',
        active: assignmentRoleQualificationRequirementData.active ?? true,
        deleted: assignmentRoleQualificationRequirementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.assignmentRoleQualificationRequirementForm.markAsPristine();
    this.assignmentRoleQualificationRequirementForm.markAsUntouched();
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

    if (this.assignmentRoleQualificationRequirementService.userIsSchedulerAssignmentRoleQualificationRequirementWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Assignment Role Qualification Requirements", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.assignmentRoleQualificationRequirementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.assignmentRoleQualificationRequirementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.assignmentRoleQualificationRequirementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const assignmentRoleQualificationRequirementSubmitData: AssignmentRoleQualificationRequirementSubmitData = {
        id: this.assignmentRoleQualificationRequirementData?.id || 0,
        assignmentRoleId: Number(formValue.assignmentRoleId),
        qualificationId: Number(formValue.qualificationId),
        isRequired: !!formValue.isRequired,
        versionNumber: this.assignmentRoleQualificationRequirementData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.assignmentRoleQualificationRequirementService.PutAssignmentRoleQualificationRequirement(assignmentRoleQualificationRequirementSubmitData.id, assignmentRoleQualificationRequirementSubmitData)
      : this.assignmentRoleQualificationRequirementService.PostAssignmentRoleQualificationRequirement(assignmentRoleQualificationRequirementSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAssignmentRoleQualificationRequirementData) => {

        this.assignmentRoleQualificationRequirementService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Assignment Role Qualification Requirement's detail page
          //
          this.assignmentRoleQualificationRequirementForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.assignmentRoleQualificationRequirementForm.markAsUntouched();

          this.router.navigate(['/assignmentrolequalificationrequirements', savedAssignmentRoleQualificationRequirementData.id]);
          this.alertService.showMessage('Assignment Role Qualification Requirement added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.assignmentRoleQualificationRequirementData = savedAssignmentRoleQualificationRequirementData;
          this.buildFormValues(this.assignmentRoleQualificationRequirementData);

          this.alertService.showMessage("Assignment Role Qualification Requirement saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Assignment Role Qualification Requirement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Assignment Role Qualification Requirement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Assignment Role Qualification Requirement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerAssignmentRoleQualificationRequirementReader(): boolean {
    return this.assignmentRoleQualificationRequirementService.userIsSchedulerAssignmentRoleQualificationRequirementReader();
  }

  public userIsSchedulerAssignmentRoleQualificationRequirementWriter(): boolean {
    return this.assignmentRoleQualificationRequirementService.userIsSchedulerAssignmentRoleQualificationRequirementWriter();
  }
}
