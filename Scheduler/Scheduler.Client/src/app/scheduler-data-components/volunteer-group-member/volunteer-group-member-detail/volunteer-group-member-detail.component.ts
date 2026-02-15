/*
   GENERATED FORM FOR THE VOLUNTEERGROUPMEMBER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from VolunteerGroupMember table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to volunteer-group-member-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { VolunteerGroupMemberService, VolunteerGroupMemberData, VolunteerGroupMemberSubmitData } from '../../../scheduler-data-services/volunteer-group-member.service';
import { VolunteerGroupService } from '../../../scheduler-data-services/volunteer-group.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { AssignmentRoleService } from '../../../scheduler-data-services/assignment-role.service';
import { VolunteerGroupMemberChangeHistoryService } from '../../../scheduler-data-services/volunteer-group-member-change-history.service';
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
interface VolunteerGroupMemberFormValues {
  volunteerGroupId: number | bigint,       // For FK link number
  resourceId: number | bigint,       // For FK link number
  assignmentRoleId: number | bigint | null,       // For FK link number
  sequence: string,     // Stored as string for form input, converted to number on submit.
  joinedDate: string | null,
  leftDate: string | null,
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-volunteer-group-member-detail',
  templateUrl: './volunteer-group-member-detail.component.html',
  styleUrls: ['./volunteer-group-member-detail.component.scss']
})

export class VolunteerGroupMemberDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<VolunteerGroupMemberFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public volunteerGroupMemberForm: FormGroup = this.fb.group({
        volunteerGroupId: [null, Validators.required],
        resourceId: [null, Validators.required],
        assignmentRoleId: [null],
        sequence: ['', Validators.required],
        joinedDate: [''],
        leftDate: [''],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public volunteerGroupMemberId: string | null = null;
  public volunteerGroupMemberData: VolunteerGroupMemberData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  volunteerGroupMembers$ = this.volunteerGroupMemberService.GetVolunteerGroupMemberList();
  public volunteerGroups$ = this.volunteerGroupService.GetVolunteerGroupList();
  public resources$ = this.resourceService.GetResourceList();
  public assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  public volunteerGroupMemberChangeHistories$ = this.volunteerGroupMemberChangeHistoryService.GetVolunteerGroupMemberChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public volunteerGroupMemberService: VolunteerGroupMemberService,
    public volunteerGroupService: VolunteerGroupService,
    public resourceService: ResourceService,
    public assignmentRoleService: AssignmentRoleService,
    public volunteerGroupMemberChangeHistoryService: VolunteerGroupMemberChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the volunteerGroupMemberId from the route parameters
    this.volunteerGroupMemberId = this.route.snapshot.paramMap.get('volunteerGroupMemberId');

    if (this.volunteerGroupMemberId === 'new' ||
        this.volunteerGroupMemberId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.volunteerGroupMemberData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.volunteerGroupMemberForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.volunteerGroupMemberForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Volunteer Group Member';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Volunteer Group Member';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.volunteerGroupMemberForm.dirty) {
      return confirm('You have unsaved Volunteer Group Member changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.volunteerGroupMemberId != null && this.volunteerGroupMemberId !== 'new') {

      const id = parseInt(this.volunteerGroupMemberId, 10);

      if (!isNaN(id)) {
        return { volunteerGroupMemberId: id };
      }
    }

    return null;
  }


/*
  * Loads the VolunteerGroupMember data for the current volunteerGroupMemberId.
  *
  * Fully respects the VolunteerGroupMemberService caching strategy and error handling strategy.
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
    if (!this.volunteerGroupMemberService.userIsSchedulerVolunteerGroupMemberReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read VolunteerGroupMembers.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate volunteerGroupMemberId
    //
    if (!this.volunteerGroupMemberId) {

      this.alertService.showMessage('No VolunteerGroupMember ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const volunteerGroupMemberId = Number(this.volunteerGroupMemberId);

    if (isNaN(volunteerGroupMemberId) || volunteerGroupMemberId <= 0) {

      this.alertService.showMessage(`Invalid Volunteer Group Member ID: "${this.volunteerGroupMemberId}"`,
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
      // This is the most targeted way: clear only this VolunteerGroupMember + relations

      this.volunteerGroupMemberService.ClearRecordCache(volunteerGroupMemberId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.volunteerGroupMemberService.GetVolunteerGroupMember(volunteerGroupMemberId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (volunteerGroupMemberData) => {

        //
        // Success path — volunteerGroupMemberData can legitimately be null if 404'd but request succeeded
        //
        if (!volunteerGroupMemberData) {

          this.handleVolunteerGroupMemberNotFound(volunteerGroupMemberId);

        } else {

          this.volunteerGroupMemberData = volunteerGroupMemberData;
          this.buildFormValues(this.volunteerGroupMemberData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'VolunteerGroupMember loaded successfully',
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
        this.handleVolunteerGroupMemberLoadError(error, volunteerGroupMemberId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleVolunteerGroupMemberNotFound(volunteerGroupMemberId: number): void {

    this.volunteerGroupMemberData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `VolunteerGroupMember #${volunteerGroupMemberId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleVolunteerGroupMemberLoadError(error: any, volunteerGroupMemberId: number): void {

    let message = 'Failed to load Volunteer Group Member.';
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
          message = 'You do not have permission to view this Volunteer Group Member.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Volunteer Group Member #${volunteerGroupMemberId} was not found.`;
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

    console.error(`Volunteer Group Member load failed (ID: ${volunteerGroupMemberId})`, error);

    //
    // Reset UI to safe state
    //
    this.volunteerGroupMemberData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(volunteerGroupMemberData: VolunteerGroupMemberData | null) {

    if (volunteerGroupMemberData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.volunteerGroupMemberForm.reset({
        volunteerGroupId: null,
        resourceId: null,
        assignmentRoleId: null,
        sequence: '',
        joinedDate: '',
        leftDate: '',
        notes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.volunteerGroupMemberForm.reset({
        volunteerGroupId: volunteerGroupMemberData.volunteerGroupId,
        resourceId: volunteerGroupMemberData.resourceId,
        assignmentRoleId: volunteerGroupMemberData.assignmentRoleId,
        sequence: volunteerGroupMemberData.sequence?.toString() ?? '',
        joinedDate: volunteerGroupMemberData.joinedDate ?? '',
        leftDate: volunteerGroupMemberData.leftDate ?? '',
        notes: volunteerGroupMemberData.notes ?? '',
        versionNumber: volunteerGroupMemberData.versionNumber?.toString() ?? '',
        active: volunteerGroupMemberData.active ?? true,
        deleted: volunteerGroupMemberData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.volunteerGroupMemberForm.markAsPristine();
    this.volunteerGroupMemberForm.markAsUntouched();
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

    if (this.volunteerGroupMemberService.userIsSchedulerVolunteerGroupMemberWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Volunteer Group Members", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.volunteerGroupMemberForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.volunteerGroupMemberForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.volunteerGroupMemberForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const volunteerGroupMemberSubmitData: VolunteerGroupMemberSubmitData = {
        id: this.volunteerGroupMemberData?.id || 0,
        volunteerGroupId: Number(formValue.volunteerGroupId),
        resourceId: Number(formValue.resourceId),
        assignmentRoleId: formValue.assignmentRoleId ? Number(formValue.assignmentRoleId) : null,
        sequence: Number(formValue.sequence),
        joinedDate: formValue.joinedDate ? formValue.joinedDate.trim() : null,
        leftDate: formValue.leftDate ? formValue.leftDate.trim() : null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.volunteerGroupMemberData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.volunteerGroupMemberService.PutVolunteerGroupMember(volunteerGroupMemberSubmitData.id, volunteerGroupMemberSubmitData)
      : this.volunteerGroupMemberService.PostVolunteerGroupMember(volunteerGroupMemberSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedVolunteerGroupMemberData) => {

        this.volunteerGroupMemberService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Volunteer Group Member's detail page
          //
          this.volunteerGroupMemberForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.volunteerGroupMemberForm.markAsUntouched();

          this.router.navigate(['/volunteergroupmembers', savedVolunteerGroupMemberData.id]);
          this.alertService.showMessage('Volunteer Group Member added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.volunteerGroupMemberData = savedVolunteerGroupMemberData;
          this.buildFormValues(this.volunteerGroupMemberData);

          this.alertService.showMessage("Volunteer Group Member saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Volunteer Group Member.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Volunteer Group Member.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Volunteer Group Member could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerVolunteerGroupMemberReader(): boolean {
    return this.volunteerGroupMemberService.userIsSchedulerVolunteerGroupMemberReader();
  }

  public userIsSchedulerVolunteerGroupMemberWriter(): boolean {
    return this.volunteerGroupMemberService.userIsSchedulerVolunteerGroupMemberWriter();
  }
}
