/*
   GENERATED FORM FOR THE VOLUNTEERPROFILE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from VolunteerProfile table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to volunteer-profile-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, OnInit, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { VolunteerProfileService, VolunteerProfileData, VolunteerProfileSubmitData } from '../../../scheduler-data-services/volunteer-profile.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { VolunteerStatusService } from '../../../scheduler-data-services/volunteer-status.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { VolunteerProfileChangeHistoryService } from '../../../scheduler-data-services/volunteer-profile-change-history.service';
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
interface VolunteerProfileFormValues {
  resourceId: number | bigint,       // For FK link number
  volunteerStatusId: number | bigint,       // For FK link number
  onboardedDate: string | null,
  inactiveSince: string | null,
  totalHoursServed: string | null,     // Stored as string for form input, converted to number on submit.
  lastActivityDate: string | null,
  backgroundCheckCompleted: boolean,
  backgroundCheckDate: string | null,
  backgroundCheckExpiry: string | null,
  confidentialityAgreementSigned: boolean,
  confidentialityAgreementDate: string | null,
  availabilityPreferences: string | null,
  interestsAndSkillsNotes: string | null,
  emergencyContactNotes: string | null,
  constituentId: number | bigint | null,       // For FK link number
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  attributes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};


@Component({
  selector: 'app-volunteer-profile-detail',
  templateUrl: './volunteer-profile-detail.component.html',
  styleUrls: ['./volunteer-profile-detail.component.scss']
})

export class VolunteerProfileDetailComponent implements OnInit, CanComponentDeactivate {


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<VolunteerProfileFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public volunteerProfileForm: FormGroup = this.fb.group({
        resourceId: [null, Validators.required],
        volunteerStatusId: [null, Validators.required],
        onboardedDate: [''],
        inactiveSince: [''],
        totalHoursServed: [''],
        lastActivityDate: [''],
        backgroundCheckCompleted: [false],
        backgroundCheckDate: [''],
        backgroundCheckExpiry: [''],
        confidentialityAgreementSigned: [false],
        confidentialityAgreementDate: [''],
        availabilityPreferences: [''],
        interestsAndSkillsNotes: [''],
        emergencyContactNotes: [''],
        constituentId: [null],
        iconId: [null],
        color: [''],
        attributes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });


  public volunteerProfileId: string | null = null;
  public volunteerProfileData: VolunteerProfileData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  volunteerProfiles$ = this.volunteerProfileService.GetVolunteerProfileList();
  public resources$ = this.resourceService.GetResourceList();
  public volunteerStatuses$ = this.volunteerStatusService.GetVolunteerStatusList();
  public constituents$ = this.constituentService.GetConstituentList();
  public icons$ = this.iconService.GetIconList();
  public volunteerProfileChangeHistories$ = this.volunteerProfileChangeHistoryService.GetVolunteerProfileChangeHistoryList();

  private destroy$ = new Subject<void>();

  constructor(
    public volunteerProfileService: VolunteerProfileService,
    public resourceService: ResourceService,
    public volunteerStatusService: VolunteerStatusService,
    public constituentService: ConstituentService,
    public iconService: IconService,
    public volunteerProfileChangeHistoryService: VolunteerProfileChangeHistoryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the volunteerProfileId from the route parameters
    this.volunteerProfileId = this.route.snapshot.paramMap.get('volunteerProfileId');

    if (this.volunteerProfileId === 'new' ||
        this.volunteerProfileId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.volunteerProfileData = null;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.volunteerProfileForm.patchValue(this.preSeededData);
      }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.volunteerProfileForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }


      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Volunteer Profile';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Volunteer Profile';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.volunteerProfileForm.dirty) {
      return confirm('You have unsaved Volunteer Profile changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.volunteerProfileId != null && this.volunteerProfileId !== 'new') {

      const id = parseInt(this.volunteerProfileId, 10);

      if (!isNaN(id)) {
        return { volunteerProfileId: id };
      }
    }

    return null;
  }


/*
  * Loads the VolunteerProfile data for the current volunteerProfileId.
  *
  * Fully respects the VolunteerProfileService caching strategy and error handling strategy.
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
    if (!this.volunteerProfileService.userIsSchedulerVolunteerProfileReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read VolunteerProfiles.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate volunteerProfileId
    //
    if (!this.volunteerProfileId) {

      this.alertService.showMessage('No VolunteerProfile ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const volunteerProfileId = Number(this.volunteerProfileId);

    if (isNaN(volunteerProfileId) || volunteerProfileId <= 0) {

      this.alertService.showMessage(`Invalid Volunteer Profile ID: "${this.volunteerProfileId}"`,
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
      // This is the most targeted way: clear only this VolunteerProfile + relations

      this.volunteerProfileService.ClearRecordCache(volunteerProfileId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.volunteerProfileService.GetVolunteerProfile(volunteerProfileId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (volunteerProfileData) => {

        //
        // Success path — volunteerProfileData can legitimately be null if 404'd but request succeeded
        //
        if (!volunteerProfileData) {

          this.handleVolunteerProfileNotFound(volunteerProfileId);

        } else {

          this.volunteerProfileData = volunteerProfileData;
          this.buildFormValues(this.volunteerProfileData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'VolunteerProfile loaded successfully',
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
        this.handleVolunteerProfileLoadError(error, volunteerProfileId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleVolunteerProfileNotFound(volunteerProfileId: number): void {

    this.volunteerProfileData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `VolunteerProfile #${volunteerProfileId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleVolunteerProfileLoadError(error: any, volunteerProfileId: number): void {

    let message = 'Failed to load Volunteer Profile.';
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
          message = 'You do not have permission to view this Volunteer Profile.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Volunteer Profile #${volunteerProfileId} was not found.`;
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

    console.error(`Volunteer Profile load failed (ID: ${volunteerProfileId})`, error);

    //
    // Reset UI to safe state
    //
    this.volunteerProfileData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(volunteerProfileData: VolunteerProfileData | null) {

    if (volunteerProfileData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.volunteerProfileForm.reset({
        resourceId: null,
        volunteerStatusId: null,
        onboardedDate: '',
        inactiveSince: '',
        totalHoursServed: '',
        lastActivityDate: '',
        backgroundCheckCompleted: false,
        backgroundCheckDate: '',
        backgroundCheckExpiry: '',
        confidentialityAgreementSigned: false,
        confidentialityAgreementDate: '',
        availabilityPreferences: '',
        interestsAndSkillsNotes: '',
        emergencyContactNotes: '',
        constituentId: null,
        iconId: null,
        color: '',
        attributes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.volunteerProfileForm.reset({
        resourceId: volunteerProfileData.resourceId,
        volunteerStatusId: volunteerProfileData.volunteerStatusId,
        onboardedDate: volunteerProfileData.onboardedDate ?? '',
        inactiveSince: volunteerProfileData.inactiveSince ?? '',
        totalHoursServed: volunteerProfileData.totalHoursServed?.toString() ?? '',
        lastActivityDate: volunteerProfileData.lastActivityDate ?? '',
        backgroundCheckCompleted: volunteerProfileData.backgroundCheckCompleted ?? false,
        backgroundCheckDate: volunteerProfileData.backgroundCheckDate ?? '',
        backgroundCheckExpiry: volunteerProfileData.backgroundCheckExpiry ?? '',
        confidentialityAgreementSigned: volunteerProfileData.confidentialityAgreementSigned ?? false,
        confidentialityAgreementDate: volunteerProfileData.confidentialityAgreementDate ?? '',
        availabilityPreferences: volunteerProfileData.availabilityPreferences ?? '',
        interestsAndSkillsNotes: volunteerProfileData.interestsAndSkillsNotes ?? '',
        emergencyContactNotes: volunteerProfileData.emergencyContactNotes ?? '',
        constituentId: volunteerProfileData.constituentId,
        iconId: volunteerProfileData.iconId,
        color: volunteerProfileData.color ?? '',
        attributes: volunteerProfileData.attributes ?? '',
        versionNumber: volunteerProfileData.versionNumber?.toString() ?? '',
        active: volunteerProfileData.active ?? true,
        deleted: volunteerProfileData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.volunteerProfileForm.markAsPristine();
    this.volunteerProfileForm.markAsUntouched();
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

    if (this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Volunteer Profiles", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.volunteerProfileForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.volunteerProfileForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.volunteerProfileForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const volunteerProfileSubmitData: VolunteerProfileSubmitData = {
        id: this.volunteerProfileData?.id || 0,
        resourceId: Number(formValue.resourceId),
        volunteerStatusId: Number(formValue.volunteerStatusId),
        onboardedDate: formValue.onboardedDate ? formValue.onboardedDate.trim() : null,
        inactiveSince: formValue.inactiveSince ? formValue.inactiveSince.trim() : null,
        totalHoursServed: formValue.totalHoursServed ? Number(formValue.totalHoursServed) : null,
        lastActivityDate: formValue.lastActivityDate ? formValue.lastActivityDate.trim() : null,
        backgroundCheckCompleted: !!formValue.backgroundCheckCompleted,
        backgroundCheckDate: formValue.backgroundCheckDate ? formValue.backgroundCheckDate.trim() : null,
        backgroundCheckExpiry: formValue.backgroundCheckExpiry ? formValue.backgroundCheckExpiry.trim() : null,
        confidentialityAgreementSigned: !!formValue.confidentialityAgreementSigned,
        confidentialityAgreementDate: formValue.confidentialityAgreementDate ? formValue.confidentialityAgreementDate.trim() : null,
        availabilityPreferences: formValue.availabilityPreferences?.trim() || null,
        interestsAndSkillsNotes: formValue.interestsAndSkillsNotes?.trim() || null,
        emergencyContactNotes: formValue.emergencyContactNotes?.trim() || null,
        constituentId: formValue.constituentId ? Number(formValue.constituentId) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        attributes: formValue.attributes?.trim() || null,
        versionNumber: this.volunteerProfileData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.volunteerProfileService.PutVolunteerProfile(volunteerProfileSubmitData.id, volunteerProfileSubmitData)
      : this.volunteerProfileService.PostVolunteerProfile(volunteerProfileSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedVolunteerProfileData) => {

        this.volunteerProfileService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Volunteer Profile's detail page
          //
          this.volunteerProfileForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.volunteerProfileForm.markAsUntouched();

          this.router.navigate(['/volunteerprofiles', savedVolunteerProfileData.id]);
          this.alertService.showMessage('Volunteer Profile added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.volunteerProfileData = savedVolunteerProfileData;
          this.buildFormValues(this.volunteerProfileData);

          this.alertService.showMessage("Volunteer Profile saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Volunteer Profile.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Volunteer Profile.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Volunteer Profile could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSchedulerVolunteerProfileReader(): boolean {
    return this.volunteerProfileService.userIsSchedulerVolunteerProfileReader();
  }

  public userIsSchedulerVolunteerProfileWriter(): boolean {
    return this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter();
  }
}
