/*
   GENERATED FORM FOR THE POSTTAGASSIGNMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PostTagAssignment table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to post-tag-assignment-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PostTagAssignmentService, PostTagAssignmentData, PostTagAssignmentSubmitData } from '../../../community-data-services/post-tag-assignment.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PostService } from '../../../community-data-services/post.service';
import { PostTagService } from '../../../community-data-services/post-tag.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PostTagAssignmentFormValues {
  postId: number | bigint,       // For FK link number
  postTagId: number | bigint,       // For FK link number
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-post-tag-assignment-add-edit',
  templateUrl: './post-tag-assignment-add-edit.component.html',
  styleUrls: ['./post-tag-assignment-add-edit.component.scss']
})
export class PostTagAssignmentAddEditComponent {
  @ViewChild('postTagAssignmentModal') postTagAssignmentModal!: TemplateRef<any>;
  @Output() postTagAssignmentChanged = new Subject<PostTagAssignmentData[]>();
  @Input() postTagAssignmentSubmitData: PostTagAssignmentSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PostTagAssignmentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public postTagAssignmentForm: FormGroup = this.fb.group({
        postId: [null, Validators.required],
        postTagId: [null, Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  postTagAssignments$ = this.postTagAssignmentService.GetPostTagAssignmentList();
  posts$ = this.postService.GetPostList();
  postTags$ = this.postTagService.GetPostTagList();

  constructor(
    private modalService: NgbModal,
    private postTagAssignmentService: PostTagAssignmentService,
    private postService: PostService,
    private postTagService: PostTagService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(postTagAssignmentData?: PostTagAssignmentData) {

    if (postTagAssignmentData != null) {

      if (!this.postTagAssignmentService.userIsCommunityPostTagAssignmentReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Post Tag Assignments`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.postTagAssignmentSubmitData = this.postTagAssignmentService.ConvertToPostTagAssignmentSubmitData(postTagAssignmentData);
      this.isEditMode = true;
      this.objectGuid = postTagAssignmentData.objectGuid;

      this.buildFormValues(postTagAssignmentData);

    } else {

      if (!this.postTagAssignmentService.userIsCommunityPostTagAssignmentWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Post Tag Assignments`,
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
        this.postTagAssignmentForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.postTagAssignmentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.postTagAssignmentModal, {
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

    if (this.postTagAssignmentService.userIsCommunityPostTagAssignmentWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Post Tag Assignments`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.postTagAssignmentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.postTagAssignmentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.postTagAssignmentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const postTagAssignmentSubmitData: PostTagAssignmentSubmitData = {
        id: this.postTagAssignmentSubmitData?.id || 0,
        postId: Number(formValue.postId),
        postTagId: Number(formValue.postTagId),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePostTagAssignment(postTagAssignmentSubmitData);
      } else {
        this.addPostTagAssignment(postTagAssignmentSubmitData);
      }
  }

  private addPostTagAssignment(postTagAssignmentData: PostTagAssignmentSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    postTagAssignmentData.active = true;
    postTagAssignmentData.deleted = false;
    this.postTagAssignmentService.PostPostTagAssignment(postTagAssignmentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPostTagAssignment) => {

        this.postTagAssignmentService.ClearAllCaches();

        this.postTagAssignmentChanged.next([newPostTagAssignment]);

        this.alertService.showMessage("Post Tag Assignment added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/posttagassignment', newPostTagAssignment.id]);
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
                                   'You do not have permission to save this Post Tag Assignment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Post Tag Assignment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Post Tag Assignment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePostTagAssignment(postTagAssignmentData: PostTagAssignmentSubmitData) {
    this.postTagAssignmentService.PutPostTagAssignment(postTagAssignmentData.id, postTagAssignmentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPostTagAssignment) => {

        this.postTagAssignmentService.ClearAllCaches();

        this.postTagAssignmentChanged.next([updatedPostTagAssignment]);

        this.alertService.showMessage("Post Tag Assignment updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Post Tag Assignment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Post Tag Assignment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Post Tag Assignment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(postTagAssignmentData: PostTagAssignmentData | null) {

    if (postTagAssignmentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.postTagAssignmentForm.reset({
        postId: null,
        postTagId: null,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.postTagAssignmentForm.reset({
        postId: postTagAssignmentData.postId,
        postTagId: postTagAssignmentData.postTagId,
        active: postTagAssignmentData.active ?? true,
        deleted: postTagAssignmentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.postTagAssignmentForm.markAsPristine();
    this.postTagAssignmentForm.markAsUntouched();
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


  public userIsCommunityPostTagAssignmentReader(): boolean {
    return this.postTagAssignmentService.userIsCommunityPostTagAssignmentReader();
  }

  public userIsCommunityPostTagAssignmentWriter(): boolean {
    return this.postTagAssignmentService.userIsCommunityPostTagAssignmentWriter();
  }
}
