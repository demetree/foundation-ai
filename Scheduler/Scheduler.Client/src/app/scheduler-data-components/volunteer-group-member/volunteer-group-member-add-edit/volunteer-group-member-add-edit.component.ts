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
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { VolunteerGroupMemberService, VolunteerGroupMemberData, VolunteerGroupMemberSubmitData } from '../../../scheduler-data-services/volunteer-group-member.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { VolunteerGroupService } from '../../../scheduler-data-services/volunteer-group.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { AssignmentRoleService } from '../../../scheduler-data-services/assignment-role.service';
import { AuthService } from '../../../services/auth.service';

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
  selector: 'app-volunteer-group-member-add-edit',
  templateUrl: './volunteer-group-member-add-edit.component.html',
  styleUrls: ['./volunteer-group-member-add-edit.component.scss']
})
export class VolunteerGroupMemberAddEditComponent {
  @ViewChild('volunteerGroupMemberModal') volunteerGroupMemberModal!: TemplateRef<any>;
  @Output() volunteerGroupMemberChanged = new Subject<VolunteerGroupMemberData[]>();
  @Input() volunteerGroupMemberSubmitData: VolunteerGroupMemberSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


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

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  volunteerGroupMembers$ = this.volunteerGroupMemberService.GetVolunteerGroupMemberList();
  volunteerGroups$ = this.volunteerGroupService.GetVolunteerGroupList();
  resources$ = this.resourceService.GetResourceList();
  assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();

  constructor(
    private modalService: NgbModal,
    private volunteerGroupMemberService: VolunteerGroupMemberService,
    private volunteerGroupService: VolunteerGroupService,
    private resourceService: ResourceService,
    private assignmentRoleService: AssignmentRoleService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(volunteerGroupMemberData?: VolunteerGroupMemberData) {

    if (volunteerGroupMemberData != null) {

      if (!this.volunteerGroupMemberService.userIsSchedulerVolunteerGroupMemberReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Volunteer Group Members`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.volunteerGroupMemberSubmitData = this.volunteerGroupMemberService.ConvertToVolunteerGroupMemberSubmitData(volunteerGroupMemberData);
      this.isEditMode = true;
      this.objectGuid = volunteerGroupMemberData.objectGuid;

      this.buildFormValues(volunteerGroupMemberData);

    } else {

      if (!this.volunteerGroupMemberService.userIsSchedulerVolunteerGroupMemberWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Volunteer Group Members`,
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
        this.volunteerGroupMemberForm.patchValue(this.preSeededData);
      }

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

    this.modalRef = this.modalService.open(this.volunteerGroupMemberModal, {
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

    if (this.volunteerGroupMemberService.userIsSchedulerVolunteerGroupMemberWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Volunteer Group Members`,
        '',
        MessageSeverity.info
      );
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
        id: this.volunteerGroupMemberSubmitData?.id || 0,
        volunteerGroupId: Number(formValue.volunteerGroupId),
        resourceId: Number(formValue.resourceId),
        assignmentRoleId: formValue.assignmentRoleId ? Number(formValue.assignmentRoleId) : null,
        sequence: Number(formValue.sequence),
        joinedDate: formValue.joinedDate?.trim() || null,
        leftDate: formValue.leftDate?.trim() || null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.volunteerGroupMemberSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateVolunteerGroupMember(volunteerGroupMemberSubmitData);
      } else {
        this.addVolunteerGroupMember(volunteerGroupMemberSubmitData);
      }
  }

  private addVolunteerGroupMember(volunteerGroupMemberData: VolunteerGroupMemberSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    volunteerGroupMemberData.versionNumber = 0;
    volunteerGroupMemberData.active = true;
    volunteerGroupMemberData.deleted = false;
    this.volunteerGroupMemberService.PostVolunteerGroupMember(volunteerGroupMemberData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newVolunteerGroupMember) => {

        this.volunteerGroupMemberService.ClearAllCaches();

        this.volunteerGroupMemberChanged.next([newVolunteerGroupMember]);

        this.alertService.showMessage("Volunteer Group Member added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/volunteergroupmember', newVolunteerGroupMember.id]);
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


  private updateVolunteerGroupMember(volunteerGroupMemberData: VolunteerGroupMemberSubmitData) {
    this.volunteerGroupMemberService.PutVolunteerGroupMember(volunteerGroupMemberData.id, volunteerGroupMemberData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedVolunteerGroupMember) => {

        this.volunteerGroupMemberService.ClearAllCaches();

        this.volunteerGroupMemberChanged.next([updatedVolunteerGroupMember]);

        this.alertService.showMessage("Volunteer Group Member updated successfully", '', MessageSeverity.success);

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


  public userIsSchedulerVolunteerGroupMemberReader(): boolean {
    return this.volunteerGroupMemberService.userIsSchedulerVolunteerGroupMemberReader();
  }

  public userIsSchedulerVolunteerGroupMemberWriter(): boolean {
    return this.volunteerGroupMemberService.userIsSchedulerVolunteerGroupMemberWriter();
  }
}
