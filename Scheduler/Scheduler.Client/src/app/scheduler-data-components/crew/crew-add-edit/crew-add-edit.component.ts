/*
   GENERATED FORM FOR THE CREW TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Crew table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to crew-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CrewService, CrewData, CrewSubmitData } from '../../../scheduler-data-services/crew.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface CrewFormValues {
  name: string,
  description: string | null,
  notes: string | null,
  officeId: number | bigint | null,       // For FK link number
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-crew-add-edit',
  templateUrl: './crew-add-edit.component.html',
  styleUrls: ['./crew-add-edit.component.scss']
})
export class CrewAddEditComponent {
  @ViewChild('crewModal') crewModal!: TemplateRef<any>;
  @Output() crewChanged = new Subject<CrewData[]>();
  @Input() crewSubmitData: CrewSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<CrewFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public crewForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        notes: [''],
        officeId: [null],
        iconId: [null],
        color: [''],
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

  crews$ = this.crewService.GetCrewList();
  offices$ = this.officeService.GetOfficeList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private crewService: CrewService,
    private officeService: OfficeService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(crewData?: CrewData) {

    if (crewData != null) {

      if (!this.crewService.userIsSchedulerCrewReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Crews`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.crewSubmitData = this.crewService.ConvertToCrewSubmitData(crewData);
      this.isEditMode = true;
      this.objectGuid = crewData.objectGuid;

      this.buildFormValues(crewData);

    } else {

      if (!this.crewService.userIsSchedulerCrewWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Crews`,
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
        this.crewForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.crewForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.crewModal, {
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

    if (this.crewService.userIsSchedulerCrewWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Crews`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.crewForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.crewForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.crewForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const crewSubmitData: CrewSubmitData = {
        id: this.crewSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        notes: formValue.notes?.trim() || null,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.crewSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateCrew(crewSubmitData);
      } else {
        this.addCrew(crewSubmitData);
      }
  }

  private addCrew(crewData: CrewSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    crewData.versionNumber = 0;
    crewData.active = true;
    crewData.deleted = false;
    this.crewService.PostCrew(crewData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCrew) => {

        this.crewService.ClearAllCaches();

        this.crewChanged.next([newCrew]);

        this.alertService.showMessage("Crew added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/crew', newCrew.id]);
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
                                   'You do not have permission to save this Crew.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Crew.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Crew could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateCrew(crewData: CrewSubmitData) {
    this.crewService.PutCrew(crewData.id, crewData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCrew) => {

        this.crewService.ClearAllCaches();

        this.crewChanged.next([updatedCrew]);

        this.alertService.showMessage("Crew updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Crew.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Crew.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Crew could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(crewData: CrewData | null) {

    if (crewData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.crewForm.reset({
        name: '',
        description: '',
        notes: '',
        officeId: null,
        iconId: null,
        color: '',
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
        this.crewForm.reset({
        name: crewData.name ?? '',
        description: crewData.description ?? '',
        notes: crewData.notes ?? '',
        officeId: crewData.officeId,
        iconId: crewData.iconId,
        color: crewData.color ?? '',
        avatarFileName: crewData.avatarFileName ?? '',
        avatarSize: crewData.avatarSize?.toString() ?? '',
        avatarData: crewData.avatarData ?? '',
        avatarMimeType: crewData.avatarMimeType ?? '',
        versionNumber: crewData.versionNumber?.toString() ?? '',
        active: crewData.active ?? true,
        deleted: crewData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.crewForm.markAsPristine();
    this.crewForm.markAsUntouched();
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


  public userIsSchedulerCrewReader(): boolean {
    return this.crewService.userIsSchedulerCrewReader();
  }

  public userIsSchedulerCrewWriter(): boolean {
    return this.crewService.userIsSchedulerCrewWriter();
  }
}
