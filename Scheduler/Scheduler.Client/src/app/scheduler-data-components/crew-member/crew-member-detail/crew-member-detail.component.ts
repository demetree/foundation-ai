/*
   GENERATED FORM FOR THE CREWMEMBER TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from CrewMember table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to crew-member-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CrewMemberService, CrewMemberData, CrewMemberSubmitData } from '../../../scheduler-data-services/crew-member.service';
import { CrewService } from '../../../scheduler-data-services/crew.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { AssignmentRoleService } from '../../../scheduler-data-services/assignment-role.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { CrewMemberChangeHistoryService } from '../../../scheduler-data-services/crew-member-change-history.service';
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
interface CrewMemberFormValues {
  crewId: number | bigint,       // For FK link number
  resourceId: number | bigint,       // For FK link number
  assignmentRoleId: number | bigint | null,       // For FK link number
  sequence: string,     // Stored as string for form input, converted to number on submit.
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-crew-member-detail',
  templateUrl: './crew-member-detail.component.html',
  styleUrls: ['./crew-member-detail.component.scss']
})

export class CrewMemberDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CrewMemberFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public crewMemberForm: FormGroup = this.fb.group({
        crewId: [null, Validators.required],
        resourceId: [null, Validators.required],
        assignmentRoleId: [null],
        sequence: ['', Validators.required],
        iconId: [null],
        color: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public crewMemberId: string | null = null;
  public crewMemberData: CrewMemberData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  crewMembers$ = this.crewMemberService.GetCrewMemberList();
  public crews$ = this.crewService.GetCrewList();
  public resources$ = this.resourceService.GetResourceList();
  public assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  public icons$ = this.iconService.GetIconList();
  public crewMemberChangeHistories$ = this.crewMemberChangeHistoryService.GetCrewMemberChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public crewMemberService: CrewMemberService,
    public crewService: CrewService,
    public resourceService: ResourceService,
    public assignmentRoleService: AssignmentRoleService,
    public iconService: IconService,
    public crewMemberChangeHistoryService: CrewMemberChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the crewMemberId from the route parameters
    this.crewMemberId = this.route.snapshot.paramMap.get('crewMemberId');

    if (this.crewMemberId === 'new' ||
        this.crewMemberId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.crewMemberData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.crewMemberForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.crewMemberForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Crew Member';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Crew Member';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.crewMemberForm.dirty) {
      return confirm('You have unsaved Crew Member changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.crewMemberId != null && this.crewMemberId !== 'new') {

      const id = parseInt(this.crewMemberId, 10);

      if (!isNaN(id)) {
        return { crewMemberId: id };
      }
    }

    return null;
  }


/*
  * Loads the CrewMember data for the current crewMemberId.
  *
  * Fully respects the CrewMemberService caching strategy and error handling strategy.
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
    if (!this.crewMemberService.userIsSchedulerCrewMemberReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read CrewMembers.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate crewMemberId
    //
    if (!this.crewMemberId) {

      this.alertService.showMessage('No CrewMember ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const crewMemberId = Number(this.crewMemberId);

    if (isNaN(crewMemberId) || crewMemberId <= 0) {

      this.alertService.showMessage(`Invalid Crew Member ID: "${this.crewMemberId}"`,
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
      // This is the most targeted way: clear only this CrewMember + relations

      this.crewMemberService.ClearRecordCache(crewMemberId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.crewMemberService.GetCrewMember(crewMemberId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (crewMemberData) => {

        //
        // Success path — crewMemberData can legitimately be null if 404'd but request succeeded
        //
        if (!crewMemberData) {

          this.handleCrewMemberNotFound(crewMemberId);

        } else {

          this.crewMemberData = crewMemberData;
          this.buildFormValues(this.crewMemberData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'CrewMember loaded successfully',
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
        this.handleCrewMemberLoadError(error, crewMemberId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleCrewMemberNotFound(crewMemberId: number): void {

    this.crewMemberData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `CrewMember #${crewMemberId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleCrewMemberLoadError(error: any, crewMemberId: number): void {

    let message = 'Failed to load Crew Member.';
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
          message = 'You do not have permission to view this Crew Member.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Crew Member #${crewMemberId} was not found.`;
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

    console.error(`Crew Member load failed (ID: ${crewMemberId})`, error);

    //
    // Reset UI to safe state
    //
    this.crewMemberData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(crewMemberData: CrewMemberData | null) {

    if (crewMemberData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.crewMemberForm.reset({
        crewId: null,
        resourceId: null,
        assignmentRoleId: null,
        sequence: '',
        iconId: null,
        color: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.crewMemberForm.reset({
        crewId: crewMemberData.crewId,
        resourceId: crewMemberData.resourceId,
        assignmentRoleId: crewMemberData.assignmentRoleId,
        sequence: crewMemberData.sequence?.toString() ?? '',
        iconId: crewMemberData.iconId,
        color: crewMemberData.color ?? '',
        versionNumber: crewMemberData.versionNumber?.toString() ?? '',
        active: crewMemberData.active ?? true,
        deleted: crewMemberData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.crewMemberForm.markAsPristine();
    this.crewMemberForm.markAsUntouched();
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

    if (this.crewMemberService.userIsSchedulerCrewMemberWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Crew Members", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.crewMemberForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.crewMemberForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.crewMemberForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const crewMemberSubmitData: CrewMemberSubmitData = {
        id: this.crewMemberData?.id || 0,
        crewId: Number(formValue.crewId),
        resourceId: Number(formValue.resourceId),
        assignmentRoleId: formValue.assignmentRoleId ? Number(formValue.assignmentRoleId) : null,
        sequence: Number(formValue.sequence),
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        versionNumber: this.crewMemberData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.crewMemberService.PutCrewMember(crewMemberSubmitData.id, crewMemberSubmitData)
      : this.crewMemberService.PostCrewMember(crewMemberSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedCrewMemberData) => {

        this.crewMemberService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Crew Member's detail page
          //
          this.crewMemberForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.crewMemberForm.markAsUntouched();

          this.router.navigate(['/crewmembers', savedCrewMemberData.id]);
          this.alertService.showMessage('Crew Member added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.crewMemberData = savedCrewMemberData;
          this.buildFormValues(this.crewMemberData);

          this.alertService.showMessage("Crew Member saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Crew Member.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Crew Member.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Crew Member could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerCrewMemberReader(): boolean {
    return this.crewMemberService.userIsSchedulerCrewMemberReader();
  }

  public userIsSchedulerCrewMemberWriter(): boolean {
    return this.crewMemberService.userIsSchedulerCrewMemberWriter();
  }
}
