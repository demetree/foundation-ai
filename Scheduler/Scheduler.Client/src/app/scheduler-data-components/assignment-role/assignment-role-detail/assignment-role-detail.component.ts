/*
   GENERATED FORM FOR THE ASSIGNMENTROLE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AssignmentRole table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to assignment-role-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AssignmentRoleService, AssignmentRoleData, AssignmentRoleSubmitData } from '../../../scheduler-data-services/assignment-role.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AssignmentRoleQualificationRequirementService } from '../../../scheduler-data-services/assignment-role-qualification-requirement.service';
import { RateSheetService } from '../../../scheduler-data-services/rate-sheet.service';
import { CrewMemberService } from '../../../scheduler-data-services/crew-member.service';
import { VolunteerGroupMemberService } from '../../../scheduler-data-services/volunteer-group-member.service';
import { EventResourceAssignmentService } from '../../../scheduler-data-services/event-resource-assignment.service';
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
interface AssignmentRoleFormValues {
  name: string,
  description: string | null,
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-assignment-role-detail',
  templateUrl: './assignment-role-detail.component.html',
  styleUrls: ['./assignment-role-detail.component.scss']
})

export class AssignmentRoleDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AssignmentRoleFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public assignmentRoleForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        iconId: [null],
        color: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });


  public assignmentRoleId: string | null = null;
  public assignmentRoleData: AssignmentRoleData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  public icons$ = this.iconService.GetIconList();
  public assignmentRoleQualificationRequirements$ = this.assignmentRoleQualificationRequirementService.GetAssignmentRoleQualificationRequirementList();
  public rateSheets$ = this.rateSheetService.GetRateSheetList();
  public crewMembers$ = this.crewMemberService.GetCrewMemberList();
  public volunteerGroupMembers$ = this.volunteerGroupMemberService.GetVolunteerGroupMemberList();
  public eventResourceAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentList();

  private destroy$ = new Subject<void>();

  constructor(
    public assignmentRoleService: AssignmentRoleService,
    public iconService: IconService,
    public assignmentRoleQualificationRequirementService: AssignmentRoleQualificationRequirementService,
    public rateSheetService: RateSheetService,
    public crewMemberService: CrewMemberService,
    public volunteerGroupMemberService: VolunteerGroupMemberService,
    public eventResourceAssignmentService: EventResourceAssignmentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the assignmentRoleId from the route parameters
    this.assignmentRoleId = this.route.snapshot.paramMap.get('assignmentRoleId');

    if (this.assignmentRoleId === 'new' ||
        this.assignmentRoleId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.assignmentRoleData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.assignmentRoleForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.assignmentRoleForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Assignment Role';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Assignment Role';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.assignmentRoleForm.dirty) {
      return confirm('You have unsaved Assignment Role changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.assignmentRoleId != null && this.assignmentRoleId !== 'new') {

      const id = parseInt(this.assignmentRoleId, 10);

      if (!isNaN(id)) {
        return { assignmentRoleId: id };
      }
    }

    return null;
  }


/*
  * Loads the AssignmentRole data for the current assignmentRoleId.
  *
  * Fully respects the AssignmentRoleService caching strategy and error handling strategy.
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
    if (!this.assignmentRoleService.userIsSchedulerAssignmentRoleReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read AssignmentRoles.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate assignmentRoleId
    //
    if (!this.assignmentRoleId) {

      this.alertService.showMessage('No AssignmentRole ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const assignmentRoleId = Number(this.assignmentRoleId);

    if (isNaN(assignmentRoleId) || assignmentRoleId <= 0) {

      this.alertService.showMessage(`Invalid Assignment Role ID: "${this.assignmentRoleId}"`,
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
      // This is the most targeted way: clear only this AssignmentRole + relations

      this.assignmentRoleService.ClearRecordCache(assignmentRoleId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.assignmentRoleService.GetAssignmentRole(assignmentRoleId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (assignmentRoleData) => {

        //
        // Success path — assignmentRoleData can legitimately be null if 404'd but request succeeded
        //
        if (!assignmentRoleData) {

          this.handleAssignmentRoleNotFound(assignmentRoleId);

        } else {

          this.assignmentRoleData = assignmentRoleData;
          this.buildFormValues(this.assignmentRoleData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'AssignmentRole loaded successfully',
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
        this.handleAssignmentRoleLoadError(error, assignmentRoleId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleAssignmentRoleNotFound(assignmentRoleId: number): void {

    this.assignmentRoleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `AssignmentRole #${assignmentRoleId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleAssignmentRoleLoadError(error: any, assignmentRoleId: number): void {

    let message = 'Failed to load Assignment Role.';
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
          message = 'You do not have permission to view this Assignment Role.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Assignment Role #${assignmentRoleId} was not found.`;
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

    console.error(`Assignment Role load failed (ID: ${assignmentRoleId})`, error);

    //
    // Reset UI to safe state
    //
    this.assignmentRoleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(assignmentRoleData: AssignmentRoleData | null) {

    if (assignmentRoleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.assignmentRoleForm.reset({
        name: '',
        description: '',
        iconId: null,
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
        this.assignmentRoleForm.reset({
        name: assignmentRoleData.name ?? '',
        description: assignmentRoleData.description ?? '',
        iconId: assignmentRoleData.iconId,
        color: assignmentRoleData.color ?? '',
        sequence: assignmentRoleData.sequence?.toString() ?? '',
        active: assignmentRoleData.active ?? true,
        deleted: assignmentRoleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.assignmentRoleForm.markAsPristine();
    this.assignmentRoleForm.markAsUntouched();
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

    if (this.assignmentRoleService.userIsSchedulerAssignmentRoleWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Assignment Roles", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.assignmentRoleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.assignmentRoleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.assignmentRoleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const assignmentRoleSubmitData: AssignmentRoleSubmitData = {
        id: this.assignmentRoleData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.assignmentRoleService.PutAssignmentRole(assignmentRoleSubmitData.id, assignmentRoleSubmitData)
      : this.assignmentRoleService.PostAssignmentRole(assignmentRoleSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedAssignmentRoleData) => {

        this.assignmentRoleService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Assignment Role's detail page
          //
          this.assignmentRoleForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.assignmentRoleForm.markAsUntouched();

          this.router.navigate(['/assignmentroles', savedAssignmentRoleData.id]);
          this.alertService.showMessage('Assignment Role added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.assignmentRoleData = savedAssignmentRoleData;
          this.buildFormValues(this.assignmentRoleData);

          this.alertService.showMessage("Assignment Role saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Assignment Role.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Assignment Role.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Assignment Role could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerAssignmentRoleReader(): boolean {
    return this.assignmentRoleService.userIsSchedulerAssignmentRoleReader();
  }

  public userIsSchedulerAssignmentRoleWriter(): boolean {
    return this.assignmentRoleService.userIsSchedulerAssignmentRoleWriter();
  }
}
