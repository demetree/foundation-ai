/*
   GENERATED FORM FOR THE TAG TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Tag table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to tag-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { TagService, TagData, TagSubmitData } from '../../../scheduler-data-services/tag.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PriorityService } from '../../../scheduler-data-services/priority.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface TagFormValues {
  name: string,
  description: string,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  isSystem: boolean | null,
  priorityId: number | bigint | null,       // For FK link number
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-tag-add-edit',
  templateUrl: './tag-add-edit.component.html',
  styleUrls: ['./tag-add-edit.component.scss']
})
export class TagAddEditComponent {
  @ViewChild('tagModal') tagModal!: TemplateRef<any>;
  @Output() tagChanged = new Subject<TagData[]>();
  @Input() tagSubmitData: TagSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<TagFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public tagForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        sequence: [''],
        isSystem: [false],
        priorityId: [null],
        iconId: [null],
        color: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  tags$ = this.tagService.GetTagList();
  priorities$ = this.priorityService.GetPriorityList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private tagService: TagService,
    private priorityService: PriorityService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(tagData?: TagData) {

    if (tagData != null) {

      if (!this.tagService.userIsSchedulerTagReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Tags`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.tagSubmitData = this.tagService.ConvertToTagSubmitData(tagData);
      this.isEditMode = true;
      this.objectGuid = tagData.objectGuid;

      this.buildFormValues(tagData);

    } else {

      if (!this.tagService.userIsSchedulerTagWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Tags`,
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
        this.tagForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.tagForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.tagModal, {
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

    if (this.tagService.userIsSchedulerTagWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Tags`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.tagForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.tagForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.tagForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const tagSubmitData: TagSubmitData = {
        id: this.tagSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        isSystem: formValue.isSystem == true ? true : formValue.isSystem == false ? false : null,
        priorityId: formValue.priorityId ? Number(formValue.priorityId) : null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateTag(tagSubmitData);
      } else {
        this.addTag(tagSubmitData);
      }
  }

  private addTag(tagData: TagSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    tagData.active = true;
    tagData.deleted = false;
    this.tagService.PostTag(tagData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newTag) => {

        this.tagService.ClearAllCaches();

        this.tagChanged.next([newTag]);

        this.alertService.showMessage("Tag added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/tag', newTag.id]);
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
                                   'You do not have permission to save this Tag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateTag(tagData: TagSubmitData) {
    this.tagService.PutTag(tagData.id, tagData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedTag) => {

        this.tagService.ClearAllCaches();

        this.tagChanged.next([updatedTag]);

        this.alertService.showMessage("Tag updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Tag.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Tag.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Tag could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(tagData: TagData | null) {

    if (tagData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.tagForm.reset({
        name: '',
        description: '',
        sequence: '',
        isSystem: false,
        priorityId: null,
        iconId: null,
        color: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.tagForm.reset({
        name: tagData.name ?? '',
        description: tagData.description ?? '',
        sequence: tagData.sequence?.toString() ?? '',
        isSystem: tagData.isSystem ?? false,
        priorityId: tagData.priorityId,
        iconId: tagData.iconId,
        color: tagData.color ?? '',
        active: tagData.active ?? true,
        deleted: tagData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.tagForm.markAsPristine();
    this.tagForm.markAsUntouched();
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


  public userIsSchedulerTagReader(): boolean {
    return this.tagService.userIsSchedulerTagReader();
  }

  public userIsSchedulerTagWriter(): boolean {
    return this.tagService.userIsSchedulerTagWriter();
  }
}
