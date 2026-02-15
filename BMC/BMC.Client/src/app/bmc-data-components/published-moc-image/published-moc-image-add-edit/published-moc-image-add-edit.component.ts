/*
   GENERATED FORM FOR THE PUBLISHEDMOCIMAGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PublishedMocImage table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to published-moc-image-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PublishedMocImageService, PublishedMocImageData, PublishedMocImageSubmitData } from '../../../bmc-data-services/published-moc-image.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PublishedMocImageFormValues {
  publishedMocId: number | bigint,       // For FK link number
  imagePath: string,
  caption: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-published-moc-image-add-edit',
  templateUrl: './published-moc-image-add-edit.component.html',
  styleUrls: ['./published-moc-image-add-edit.component.scss']
})
export class PublishedMocImageAddEditComponent {
  @ViewChild('publishedMocImageModal') publishedMocImageModal!: TemplateRef<any>;
  @Output() publishedMocImageChanged = new Subject<PublishedMocImageData[]>();
  @Input() publishedMocImageSubmitData: PublishedMocImageSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PublishedMocImageFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public publishedMocImageForm: FormGroup = this.fb.group({
        publishedMocId: [null, Validators.required],
        imagePath: ['', Validators.required],
        caption: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  publishedMocImages$ = this.publishedMocImageService.GetPublishedMocImageList();
  publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  constructor(
    private modalService: NgbModal,
    private publishedMocImageService: PublishedMocImageService,
    private publishedMocService: PublishedMocService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(publishedMocImageData?: PublishedMocImageData) {

    if (publishedMocImageData != null) {

      if (!this.publishedMocImageService.userIsBMCPublishedMocImageReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Published Moc Images`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.publishedMocImageSubmitData = this.publishedMocImageService.ConvertToPublishedMocImageSubmitData(publishedMocImageData);
      this.isEditMode = true;
      this.objectGuid = publishedMocImageData.objectGuid;

      this.buildFormValues(publishedMocImageData);

    } else {

      if (!this.publishedMocImageService.userIsBMCPublishedMocImageWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Published Moc Images`,
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
        this.publishedMocImageForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.publishedMocImageForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.publishedMocImageModal, {
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

    if (this.publishedMocImageService.userIsBMCPublishedMocImageWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Published Moc Images`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.publishedMocImageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.publishedMocImageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.publishedMocImageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const publishedMocImageSubmitData: PublishedMocImageSubmitData = {
        id: this.publishedMocImageSubmitData?.id || 0,
        publishedMocId: Number(formValue.publishedMocId),
        imagePath: formValue.imagePath!.trim(),
        caption: formValue.caption?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePublishedMocImage(publishedMocImageSubmitData);
      } else {
        this.addPublishedMocImage(publishedMocImageSubmitData);
      }
  }

  private addPublishedMocImage(publishedMocImageData: PublishedMocImageSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    publishedMocImageData.active = true;
    publishedMocImageData.deleted = false;
    this.publishedMocImageService.PostPublishedMocImage(publishedMocImageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPublishedMocImage) => {

        this.publishedMocImageService.ClearAllCaches();

        this.publishedMocImageChanged.next([newPublishedMocImage]);

        this.alertService.showMessage("Published Moc Image added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/publishedmocimage', newPublishedMocImage.id]);
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
                                   'You do not have permission to save this Published Moc Image.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Published Moc Image.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Published Moc Image could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePublishedMocImage(publishedMocImageData: PublishedMocImageSubmitData) {
    this.publishedMocImageService.PutPublishedMocImage(publishedMocImageData.id, publishedMocImageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPublishedMocImage) => {

        this.publishedMocImageService.ClearAllCaches();

        this.publishedMocImageChanged.next([updatedPublishedMocImage]);

        this.alertService.showMessage("Published Moc Image updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Published Moc Image.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Published Moc Image.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Published Moc Image could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(publishedMocImageData: PublishedMocImageData | null) {

    if (publishedMocImageData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.publishedMocImageForm.reset({
        publishedMocId: null,
        imagePath: '',
        caption: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.publishedMocImageForm.reset({
        publishedMocId: publishedMocImageData.publishedMocId,
        imagePath: publishedMocImageData.imagePath ?? '',
        caption: publishedMocImageData.caption ?? '',
        sequence: publishedMocImageData.sequence?.toString() ?? '',
        active: publishedMocImageData.active ?? true,
        deleted: publishedMocImageData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.publishedMocImageForm.markAsPristine();
    this.publishedMocImageForm.markAsUntouched();
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


  public userIsBMCPublishedMocImageReader(): boolean {
    return this.publishedMocImageService.userIsBMCPublishedMocImageReader();
  }

  public userIsBMCPublishedMocImageWriter(): boolean {
    return this.publishedMocImageService.userIsBMCPublishedMocImageWriter();
  }
}
