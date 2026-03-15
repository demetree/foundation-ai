/*
   GENERATED FORM FOR THE POSTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PostChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to post-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PostChangeHistoryService, PostChangeHistoryData, PostChangeHistorySubmitData } from '../../../community-data-services/post-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PostService } from '../../../community-data-services/post.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PostChangeHistoryFormValues {
};

@Component({
  selector: 'app-post-change-history-add-edit',
  templateUrl: './post-change-history-add-edit.component.html',
  styleUrls: ['./post-change-history-add-edit.component.scss']
})
export class PostChangeHistoryAddEditComponent {
  @ViewChild('postChangeHistoryModal') postChangeHistoryModal!: TemplateRef<any>;
  @Output() postChangeHistoryChanged = new Subject<PostChangeHistoryData[]>();
  @Input() postChangeHistorySubmitData: PostChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PostChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public postChangeHistoryForm: FormGroup = this.fb.group({
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  postChangeHistories$ = this.postChangeHistoryService.GetPostChangeHistoryList();
  posts$ = this.postService.GetPostList();

  constructor(
    private modalService: NgbModal,
    private postChangeHistoryService: PostChangeHistoryService,
    private postService: PostService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(postChangeHistoryData?: PostChangeHistoryData) {

    if (postChangeHistoryData != null) {

      if (!this.postChangeHistoryService.userIsCommunityPostChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Post Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.postChangeHistorySubmitData = this.postChangeHistoryService.ConvertToPostChangeHistorySubmitData(postChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(postChangeHistoryData);

    } else {

      if (!this.postChangeHistoryService.userIsCommunityPostChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Post Change Histories`,
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
        this.postChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.postChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.postChangeHistoryModal, {
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

    if (this.postChangeHistoryService.userIsCommunityPostChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Post Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.postChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.postChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.postChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const postChangeHistorySubmitData: PostChangeHistorySubmitData = {
        id: this.postChangeHistorySubmitData?.id || 0,
   };

      if (this.isEditMode) {
        this.updatePostChangeHistory(postChangeHistorySubmitData);
      } else {
        this.addPostChangeHistory(postChangeHistorySubmitData);
      }
  }

  private addPostChangeHistory(postChangeHistoryData: PostChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    postChangeHistoryData.versionNumber = 0;
    this.postChangeHistoryService.PostPostChangeHistory(postChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPostChangeHistory) => {

        this.postChangeHistoryService.ClearAllCaches();

        this.postChangeHistoryChanged.next([newPostChangeHistory]);

        this.alertService.showMessage("Post Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/postchangehistory', newPostChangeHistory.id]);
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
                                   'You do not have permission to save this Post Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Post Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Post Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePostChangeHistory(postChangeHistoryData: PostChangeHistorySubmitData) {
    this.postChangeHistoryService.PutPostChangeHistory(postChangeHistoryData.id, postChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPostChangeHistory) => {

        this.postChangeHistoryService.ClearAllCaches();

        this.postChangeHistoryChanged.next([updatedPostChangeHistory]);

        this.alertService.showMessage("Post Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Post Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Post Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Post Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(postChangeHistoryData: PostChangeHistoryData | null) {

    if (postChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.postChangeHistoryForm.reset({
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.postChangeHistoryForm.reset({
      }, { emitEvent: false});
    }

    this.postChangeHistoryForm.markAsPristine();
    this.postChangeHistoryForm.markAsUntouched();
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


  public userIsCommunityPostChangeHistoryReader(): boolean {
    return this.postChangeHistoryService.userIsCommunityPostChangeHistoryReader();
  }

  public userIsCommunityPostChangeHistoryWriter(): boolean {
    return this.postChangeHistoryService.userIsCommunityPostChangeHistoryWriter();
  }
}
