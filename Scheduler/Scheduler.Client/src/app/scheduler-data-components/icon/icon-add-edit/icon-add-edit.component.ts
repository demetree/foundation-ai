/*
   GENERATED FORM FOR THE ICON TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Icon table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to icon-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IconService, IconData, IconSubmitData } from '../../../scheduler-data-services/icon.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface IconFormValues {
  name: string,
  fontAwesomeCode: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-icon-add-edit',
  templateUrl: './icon-add-edit.component.html',
  styleUrls: ['./icon-add-edit.component.scss']
})
export class IconAddEditComponent {
  @ViewChild('iconModal') iconModal!: TemplateRef<any>;
  @Output() iconChanged = new Subject<IconData[]>();
  @Input() iconSubmitData: IconSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IconFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public iconForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        fontAwesomeCode: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(iconData?: IconData) {

    if (iconData != null) {

      if (!this.iconService.userIsSchedulerIconReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Icons`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.iconSubmitData = this.iconService.ConvertToIconSubmitData(iconData);
      this.isEditMode = true;
      this.objectGuid = iconData.objectGuid;

      this.buildFormValues(iconData);

    } else {

      if (!this.iconService.userIsSchedulerIconWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Icons`,
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
        this.iconForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.iconForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.iconModal, {
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

    if (this.iconService.userIsSchedulerIconWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Icons`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.iconForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.iconForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.iconForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const iconSubmitData: IconSubmitData = {
        id: this.iconSubmitData?.id || 0,
        name: formValue.name!.trim(),
        fontAwesomeCode: formValue.fontAwesomeCode?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateIcon(iconSubmitData);
      } else {
        this.addIcon(iconSubmitData);
      }
  }

  private addIcon(iconData: IconSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    iconData.active = true;
    iconData.deleted = false;
    this.iconService.PostIcon(iconData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newIcon) => {

        this.iconService.ClearAllCaches();

        this.iconChanged.next([newIcon]);

        this.alertService.showMessage("Icon added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/icon', newIcon.id]);
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
                                   'You do not have permission to save this Icon.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Icon.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Icon could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateIcon(iconData: IconSubmitData) {
    this.iconService.PutIcon(iconData.id, iconData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedIcon) => {

        this.iconService.ClearAllCaches();

        this.iconChanged.next([updatedIcon]);

        this.alertService.showMessage("Icon updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Icon.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Icon.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Icon could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(iconData: IconData | null) {

    if (iconData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.iconForm.reset({
        name: '',
        fontAwesomeCode: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.iconForm.reset({
        name: iconData.name ?? '',
        fontAwesomeCode: iconData.fontAwesomeCode ?? '',
        sequence: iconData.sequence?.toString() ?? '',
        active: iconData.active ?? true,
        deleted: iconData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.iconForm.markAsPristine();
    this.iconForm.markAsUntouched();
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


  public userIsSchedulerIconReader(): boolean {
    return this.iconService.userIsSchedulerIconReader();
  }

  public userIsSchedulerIconWriter(): boolean {
    return this.iconService.userIsSchedulerIconWriter();
  }
}
