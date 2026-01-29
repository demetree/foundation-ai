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
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { VolunteerGroupService, VolunteerGroupData, VolunteerGroupSubmitData } from '../../../scheduler-data-services/volunteer-group.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { VolunteerStatusService } from '../../../scheduler-data-services/volunteer-status.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

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
  selector: 'app-volunteer-group-add-edit',
  templateUrl: './volunteer-group-add-edit.component.html',
  styleUrls: ['./volunteer-group-add-edit.component.scss']
})
export class VolunteerGroupAddEditComponent {
  @ViewChild('volunteerGroupModal') volunteerGroupModal!: TemplateRef<any>;
  @Output() volunteerGroupChanged = new Subject<VolunteerGroupData[]>();
  @Input() volunteerGroupSubmitData: VolunteerGroupSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  volunteerGroups$ = this.volunteerGroupService.GetVolunteerGroupList();
  offices$ = this.officeService.GetOfficeList();
  volunteerStatuses$ = this.volunteerStatusService.GetVolunteerStatusList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private volunteerGroupService: VolunteerGroupService,
    private officeService: OfficeService,
    private volunteerStatusService: VolunteerStatusService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(volunteerGroupData?: VolunteerGroupData) {

    if (volunteerGroupData != null) {

      if (!this.volunteerGroupService.userIsSchedulerVolunteerGroupReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Volunteer Groups`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.volunteerGroupSubmitData = this.volunteerGroupService.ConvertToVolunteerGroupSubmitData(volunteerGroupData);
      this.isEditMode = true;
      this.objectGuid = volunteerGroupData.objectGuid;

      this.buildFormValues(volunteerGroupData);

    } else {

      if (!this.volunteerGroupService.userIsSchedulerVolunteerGroupWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Volunteer Groups`,
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
        this.volunteerGroupForm.patchValue(this.preSeededData);
      }

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

    this.modalRef = this.modalService.open(this.volunteerGroupModal, {
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

    if (this.volunteerGroupService.userIsSchedulerVolunteerGroupWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Volunteer Groups`,
        '',
        MessageSeverity.info
      );
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
        id: this.volunteerGroupSubmitData?.id || 0,
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
        versionNumber: this.volunteerGroupSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateVolunteerGroup(volunteerGroupSubmitData);
      } else {
        this.addVolunteerGroup(volunteerGroupSubmitData);
      }
  }

  private addVolunteerGroup(volunteerGroupData: VolunteerGroupSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    volunteerGroupData.versionNumber = 0;
    volunteerGroupData.active = true;
    volunteerGroupData.deleted = false;
    this.volunteerGroupService.PostVolunteerGroup(volunteerGroupData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newVolunteerGroup) => {

        this.volunteerGroupService.ClearAllCaches();

        this.volunteerGroupChanged.next([newVolunteerGroup]);

        this.alertService.showMessage("Volunteer Group added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/volunteergroup', newVolunteerGroup.id]);
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


  private updateVolunteerGroup(volunteerGroupData: VolunteerGroupSubmitData) {
    this.volunteerGroupService.PutVolunteerGroup(volunteerGroupData.id, volunteerGroupData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedVolunteerGroup) => {

        this.volunteerGroupService.ClearAllCaches();

        this.volunteerGroupChanged.next([updatedVolunteerGroup]);

        this.alertService.showMessage("Volunteer Group updated successfully", '', MessageSeverity.success);

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


  public userIsSchedulerVolunteerGroupReader(): boolean {
    return this.volunteerGroupService.userIsSchedulerVolunteerGroupReader();
  }

  public userIsSchedulerVolunteerGroupWriter(): boolean {
    return this.volunteerGroupService.userIsSchedulerVolunteerGroupWriter();
  }
}
