/*
   GENERATED FORM FOR THE VOLUNTEERGROUP TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from VolunteerGroup table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to volunteer-group-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { VolunteerGroupService, VolunteerGroupData, VolunteerGroupSubmitData } from '../../../scheduler-data-services/volunteer-group.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { VolunteerStatusService } from '../../../scheduler-data-services/volunteer-status.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { VolunteerGroupChangeHistoryService } from '../../../scheduler-data-services/volunteer-group-change-history.service';
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
interface VolunteerGroupFormValues {
  name: string,
  description: string | null,
  purpose: string | null,
  officeId: number | bigint | null,       // For FK link number
  volunteerStatusId: number | bigint | null,       // For FK link number
  maxMembers: string | null,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  notes: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-volunteer-group-detail',
  templateUrl: './volunteer-group-detail.component.html',
  styleUrls: ['./volunteer-group-detail.component.scss']
})

export class VolunteerGroupDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<VolunteerGroupFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public volunteerGroupForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        purpose: [''],
        officeId: [null],
        volunteerStatusId: [null],
        maxMembers: [''],
        iconId: [null],
        color: [''],
        notes: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public volunteerGroupId: string | null = null;
  public volunteerGroupData: VolunteerGroupData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  volunteerGroups$ = this.volunteerGroupService.GetVolunteerGroupList();
  public offices$ = this.officeService.GetOfficeList();
  public volunteerStatuses$ = this.volunteerStatusService.GetVolunteerStatusList();
  public icons$ = this.iconService.GetIconList();
  public volunteerGroupChangeHistories$ = this.volunteerGroupChangeHistoryService.GetVolunteerGroupChangeHistoryList();
  public volunteerGroupMembers$ = this.volunteerGroupMemberService.GetVolunteerGroupMemberList();
  public eventResourceAssignments$ = this.eventResourceAssignmentService.GetEventResourceAssignmentList();

  private destroy$ = new Subject<void>();

  constructor(
    public volunteerGroupService: VolunteerGroupService,
    public officeService: OfficeService,
    public volunteerStatusService: VolunteerStatusService,
    public iconService: IconService,
    public volunteerGroupChangeHistoryService: VolunteerGroupChangeHistoryService,
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

    // Get the volunteerGroupId from the route parameters
    this.volunteerGroupId = this.route.snapshot.paramMap.get('volunteerGroupId');

    if (this.volunteerGroupId === 'new' ||
        this.volunteerGroupId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.volunteerGroupData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.volunteerGroupForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.volunteerGroupForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Volunteer Group';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Volunteer Group';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.volunteerGroupForm.dirty) {
      return confirm('You have unsaved Volunteer Group changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.volunteerGroupId != null && this.volunteerGroupId !== 'new') {

      const id = parseInt(this.volunteerGroupId, 10);

      if (!isNaN(id)) {
        return { volunteerGroupId: id };
      }
    }

    return null;
  }


/*
  * Loads the VolunteerGroup data for the current volunteerGroupId.
  *
  * Fully respects the VolunteerGroupService caching strategy and error handling strategy.
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
    if (!this.volunteerGroupService.userIsSchedulerVolunteerGroupReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read VolunteerGroups.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate volunteerGroupId
    //
    if (!this.volunteerGroupId) {

      this.alertService.showMessage('No VolunteerGroup ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const volunteerGroupId = Number(this.volunteerGroupId);

    if (isNaN(volunteerGroupId) || volunteerGroupId <= 0) {

      this.alertService.showMessage(`Invalid Volunteer Group ID: "${this.volunteerGroupId}"`,
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
      // This is the most targeted way: clear only this VolunteerGroup + relations

      this.volunteerGroupService.ClearRecordCache(volunteerGroupId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.volunteerGroupService.GetVolunteerGroup(volunteerGroupId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (volunteerGroupData) => {

        //
        // Success path — volunteerGroupData can legitimately be null if 404'd but request succeeded
        //
        if (!volunteerGroupData) {

          this.handleVolunteerGroupNotFound(volunteerGroupId);

        } else {

          this.volunteerGroupData = volunteerGroupData;
          this.buildFormValues(this.volunteerGroupData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'VolunteerGroup loaded successfully',
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
        this.handleVolunteerGroupLoadError(error, volunteerGroupId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleVolunteerGroupNotFound(volunteerGroupId: number): void {

    this.volunteerGroupData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `VolunteerGroup #${volunteerGroupId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleVolunteerGroupLoadError(error: any, volunteerGroupId: number): void {

    let message = 'Failed to load Volunteer Group.';
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
          message = 'You do not have permission to view this Volunteer Group.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Volunteer Group #${volunteerGroupId} was not found.`;
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

    console.error(`Volunteer Group load failed (ID: ${volunteerGroupId})`, error);

    //
    // Reset UI to safe state
    //
    this.volunteerGroupData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(volunteerGroupData: VolunteerGroupData | null) {

    if (volunteerGroupData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.volunteerGroupForm.reset({
        name: '',
        description: '',
        purpose: '',
        officeId: null,
        volunteerStatusId: null,
        maxMembers: '',
        iconId: null,
        color: '',
        notes: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.volunteerGroupForm.reset({
        name: volunteerGroupData.name ?? '',
        description: volunteerGroupData.description ?? '',
        purpose: volunteerGroupData.purpose ?? '',
        officeId: volunteerGroupData.officeId,
        volunteerStatusId: volunteerGroupData.volunteerStatusId,
        maxMembers: volunteerGroupData.maxMembers?.toString() ?? '',
        iconId: volunteerGroupData.iconId,
        color: volunteerGroupData.color ?? '',
        notes: volunteerGroupData.notes ?? '',
        avatarFileName: volunteerGroupData.avatarFileName ?? '',
        avatarSize: volunteerGroupData.avatarSize?.toString() ?? '',
        avatarData: volunteerGroupData.avatarData ?? '',
        avatarMimeType: volunteerGroupData.avatarMimeType ?? '',
        versionNumber: volunteerGroupData.versionNumber?.toString() ?? '',
        active: volunteerGroupData.active ?? true,
        deleted: volunteerGroupData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.volunteerGroupForm.markAsPristine();
    this.volunteerGroupForm.markAsUntouched();
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

    if (this.volunteerGroupService.userIsSchedulerVolunteerGroupWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Volunteer Groups", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.volunteerGroupForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.volunteerGroupForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.volunteerGroupForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const volunteerGroupSubmitData: VolunteerGroupSubmitData = {
        id: this.volunteerGroupData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        purpose: formValue.purpose?.trim() || null,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        volunteerStatusId: formValue.volunteerStatusId ? Number(formValue.volunteerStatusId) : null,
        maxMembers: formValue.maxMembers ? Number(formValue.maxMembers) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        notes: formValue.notes?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.volunteerGroupData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.volunteerGroupService.PutVolunteerGroup(volunteerGroupSubmitData.id, volunteerGroupSubmitData)
      : this.volunteerGroupService.PostVolunteerGroup(volunteerGroupSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedVolunteerGroupData) => {

        this.volunteerGroupService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Volunteer Group's detail page
          //
          this.volunteerGroupForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.volunteerGroupForm.markAsUntouched();

          this.router.navigate(['/volunteergroups', savedVolunteerGroupData.id]);
          this.alertService.showMessage('Volunteer Group added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.volunteerGroupData = savedVolunteerGroupData;
          this.buildFormValues(this.volunteerGroupData);

          this.alertService.showMessage("Volunteer Group saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Volunteer Group.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Volunteer Group.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Volunteer Group could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerVolunteerGroupReader(): boolean {
    return this.volunteerGroupService.userIsSchedulerVolunteerGroupReader();
  }

  public userIsSchedulerVolunteerGroupWriter(): boolean {
    return this.volunteerGroupService.userIsSchedulerVolunteerGroupWriter();
  }
}
