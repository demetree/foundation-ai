/*
   GENERATED FORM FOR THE MEDIACONTENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MediaContent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to media-content-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MediaContentService, MediaContentData, MediaContentSubmitData } from '../../../community-data-services/media-content.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { MediaAssetService } from '../../../community-data-services/media-asset.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface MediaContentFormValues {
  mediaAssetId: number | bigint,       // For FK link number
  fileData: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-media-content-add-edit',
  templateUrl: './media-content-add-edit.component.html',
  styleUrls: ['./media-content-add-edit.component.scss']
})
export class MediaContentAddEditComponent {
  @ViewChild('mediaContentModal') mediaContentModal!: TemplateRef<any>;
  @Output() mediaContentChanged = new Subject<MediaContentData[]>();
  @Input() mediaContentSubmitData: MediaContentSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MediaContentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public mediaContentForm: FormGroup = this.fb.group({
        mediaAssetId: [null, Validators.required],
        fileData: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  mediaContents$ = this.mediaContentService.GetMediaContentList();
  mediaAssets$ = this.mediaAssetService.GetMediaAssetList();

  constructor(
    private modalService: NgbModal,
    private mediaContentService: MediaContentService,
    private mediaAssetService: MediaAssetService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(mediaContentData?: MediaContentData) {

    if (mediaContentData != null) {

      if (!this.mediaContentService.userIsCommunityMediaContentReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Media Contents`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.mediaContentSubmitData = this.mediaContentService.ConvertToMediaContentSubmitData(mediaContentData);
      this.isEditMode = true;
      this.objectGuid = mediaContentData.objectGuid;

      this.buildFormValues(mediaContentData);

    } else {

      if (!this.mediaContentService.userIsCommunityMediaContentWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Media Contents`,
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
        this.mediaContentForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.mediaContentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.mediaContentModal, {
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

    if (this.mediaContentService.userIsCommunityMediaContentWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Media Contents`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.mediaContentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.mediaContentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.mediaContentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const mediaContentSubmitData: MediaContentSubmitData = {
        id: this.mediaContentSubmitData?.id || 0,
        mediaAssetId: Number(formValue.mediaAssetId),
        fileData: formValue.fileData!.trim(),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateMediaContent(mediaContentSubmitData);
      } else {
        this.addMediaContent(mediaContentSubmitData);
      }
  }

  private addMediaContent(mediaContentData: MediaContentSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    mediaContentData.active = true;
    mediaContentData.deleted = false;
    this.mediaContentService.PostMediaContent(mediaContentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newMediaContent) => {

        this.mediaContentService.ClearAllCaches();

        this.mediaContentChanged.next([newMediaContent]);

        this.alertService.showMessage("Media Content added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/mediacontent', newMediaContent.id]);
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
                                   'You do not have permission to save this Media Content.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Media Content.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Media Content could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateMediaContent(mediaContentData: MediaContentSubmitData) {
    this.mediaContentService.PutMediaContent(mediaContentData.id, mediaContentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedMediaContent) => {

        this.mediaContentService.ClearAllCaches();

        this.mediaContentChanged.next([updatedMediaContent]);

        this.alertService.showMessage("Media Content updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Media Content.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Media Content.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Media Content could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(mediaContentData: MediaContentData | null) {

    if (mediaContentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.mediaContentForm.reset({
        mediaAssetId: null,
        fileData: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.mediaContentForm.reset({
        mediaAssetId: mediaContentData.mediaAssetId,
        fileData: mediaContentData.fileData ?? '',
        active: mediaContentData.active ?? true,
        deleted: mediaContentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.mediaContentForm.markAsPristine();
    this.mediaContentForm.markAsUntouched();
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


  public userIsCommunityMediaContentReader(): boolean {
    return this.mediaContentService.userIsCommunityMediaContentReader();
  }

  public userIsCommunityMediaContentWriter(): boolean {
    return this.mediaContentService.userIsCommunityMediaContentWriter();
  }
}
