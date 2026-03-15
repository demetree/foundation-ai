/*
   GENERATED FORM FOR THE POST TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Post table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to post-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PostService, PostData, PostSubmitData } from '../../../community-data-services/post.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PostCategoryService } from '../../../community-data-services/post-category.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PostFormValues {
};

@Component({
  selector: 'app-post-add-edit',
  templateUrl: './post-add-edit.component.html',
  styleUrls: ['./post-add-edit.component.scss']
})
export class PostAddEditComponent {
  @ViewChild('postModal') postModal!: TemplateRef<any>;
  @Output() postChanged = new Subject<PostData[]>();
  @Input() postSubmitData: PostSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PostFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public postForm: FormGroup = this.fb.group({
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  posts$ = this.postService.GetPostList();
  postCategories$ = this.postCategoryService.GetPostCategoryList();

  constructor(
    private modalService: NgbModal,
    private postService: PostService,
    private postCategoryService: PostCategoryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(postData?: PostData) {

    if (postData != null) {

      if (!this.postService.userIsCommunityPostReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Posts`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.postSubmitData = this.postService.ConvertToPostSubmitData(postData);
      this.isEditMode = true;

      this.buildFormValues(postData);

    } else {

      if (!this.postService.userIsCommunityPostWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Posts`,
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
        this.postForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.postForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.postModal, {
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

    if (this.postService.userIsCommunityPostWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Posts`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.postForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.postForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.postForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const postSubmitData: PostSubmitData = {
        id: this.postSubmitData?.id || 0,
   };

      if (this.isEditMode) {
        this.updatePost(postSubmitData);
      } else {
        this.addPost(postSubmitData);
      }
  }

  private addPost(postData: PostSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    postData.versionNumber = 0;
    postData.active = true;
    postData.deleted = false;
    this.postService.PostPost(postData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPost) => {

        this.postService.ClearAllCaches();

        this.postChanged.next([newPost]);

        this.alertService.showMessage("Post added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/post', newPost.id]);
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
                                   'You do not have permission to save this Post.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Post.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Post could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePost(postData: PostSubmitData) {
    this.postService.PutPost(postData.id, postData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPost) => {

        this.postService.ClearAllCaches();

        this.postChanged.next([updatedPost]);

        this.alertService.showMessage("Post updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Post.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Post.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Post could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(postData: PostData | null) {

    if (postData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.postForm.reset({
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.postForm.reset({
      }, { emitEvent: false});
    }

    this.postForm.markAsPristine();
    this.postForm.markAsUntouched();
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


  public userIsCommunityPostReader(): boolean {
    return this.postService.userIsCommunityPostReader();
  }

  public userIsCommunityPostWriter(): boolean {
    return this.postService.userIsCommunityPostWriter();
  }
}
