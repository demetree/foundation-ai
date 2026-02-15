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
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { VolunteerProfileService, VolunteerProfileData, VolunteerProfileSubmitData } from '../../../scheduler-data-services/volunteer-profile.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { VolunteerStatusService } from '../../../scheduler-data-services/volunteer-status.service';
import { ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

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
  selector: 'app-volunteer-profile-add-edit',
  templateUrl: './volunteer-profile-add-edit.component.html',
  styleUrls: ['./volunteer-profile-add-edit.component.scss']
})
export class VolunteerProfileAddEditComponent {
  @ViewChild('volunteerProfileModal') volunteerProfileModal!: TemplateRef<any>;
  @Output() volunteerProfileChanged = new Subject<VolunteerProfileData[]>();
  @Input() volunteerProfileSubmitData: VolunteerProfileSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  volunteerProfiles$ = this.volunteerProfileService.GetVolunteerProfileList();
  resources$ = this.resourceService.GetResourceList();
  volunteerStatuses$ = this.volunteerStatusService.GetVolunteerStatusList();
  constituents$ = this.constituentService.GetConstituentList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private volunteerProfileService: VolunteerProfileService,
    private resourceService: ResourceService,
    private volunteerStatusService: VolunteerStatusService,
    private constituentService: ConstituentService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(volunteerProfileData?: VolunteerProfileData) {

    if (volunteerProfileData != null) {

      if (!this.volunteerProfileService.userIsSchedulerVolunteerProfileReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Volunteer Profiles`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.volunteerProfileSubmitData = this.volunteerProfileService.ConvertToVolunteerProfileSubmitData(volunteerProfileData);
      this.isEditMode = true;
      this.objectGuid = volunteerProfileData.objectGuid;

      this.buildFormValues(volunteerProfileData);

    } else {

      if (!this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Volunteer Profiles`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.volunteerProfileForm.patchValue(this.preSeededData);
      }

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

    this.modalRef = this.modalService.open(this.volunteerProfileModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;
  }


  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Volunteer Profiles`,
        '',
        MessageSeverity.info
      );
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
        id: this.volunteerProfileSubmitData?.id || 0,
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
        versionNumber: this.volunteerProfileSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateVolunteerProfile(volunteerProfileSubmitData);
      } else {
        this.addVolunteerProfile(volunteerProfileSubmitData);
      }
  }

  private addVolunteerProfile(volunteerProfileData: VolunteerProfileSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    volunteerProfileData.versionNumber = 0;
    volunteerProfileData.active = true;
    volunteerProfileData.deleted = false;
    this.volunteerProfileService.PostVolunteerProfile(volunteerProfileData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newVolunteerProfile) => {

        this.volunteerProfileService.ClearAllCaches();

        this.volunteerProfileChanged.next([newVolunteerProfile]);

        this.alertService.showMessage("Volunteer Profile added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/volunteerprofile', newVolunteerProfile.id]);
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


  private updateVolunteerProfile(volunteerProfileData: VolunteerProfileSubmitData) {
    this.volunteerProfileService.PutVolunteerProfile(volunteerProfileData.id, volunteerProfileData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedVolunteerProfile) => {

        this.volunteerProfileService.ClearAllCaches();

        this.volunteerProfileChanged.next([updatedVolunteerProfile]);

        this.alertService.showMessage("Volunteer Profile updated successfully", '', MessageSeverity.success);

        this.closeModal();
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


  public userIsSchedulerVolunteerProfileReader(): boolean {
    return this.volunteerProfileService.userIsSchedulerVolunteerProfileReader();
  }

  public userIsSchedulerVolunteerProfileWriter(): boolean {
    return this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter();
  }
}
