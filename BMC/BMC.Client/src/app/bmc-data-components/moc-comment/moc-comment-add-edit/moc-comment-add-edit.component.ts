/*
   GENERATED FORM FOR THE MOCCOMMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MocComment table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to moc-comment-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MocCommentService, MocCommentData, MocCommentSubmitData } from '../../../bmc-data-services/moc-comment.service';
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
interface MocCommentFormValues {
  publishedMocId: number | bigint,       // For FK link number
  commenterTenantGuid: string,
  commentText: string,
  postedDate: string,
  mocCommentId: number | bigint | null,       // For FK link number
  isEdited: boolean,
  isHidden: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-moc-comment-add-edit',
  templateUrl: './moc-comment-add-edit.component.html',
  styleUrls: ['./moc-comment-add-edit.component.scss']
})
export class MocCommentAddEditComponent {
  @ViewChild('mocCommentModal') mocCommentModal!: TemplateRef<any>;
  @Output() mocCommentChanged = new Subject<MocCommentData[]>();
  @Input() mocCommentSubmitData: MocCommentSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MocCommentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public mocCommentForm: FormGroup = this.fb.group({
        publishedMocId: [null, Validators.required],
        commenterTenantGuid: ['', Validators.required],
        commentText: ['', Validators.required],
        postedDate: ['', Validators.required],
        mocCommentId: [null],
        isEdited: [false],
        isHidden: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  mocComments$ = this.mocCommentService.GetMocCommentList();
  publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  constructor(
    private modalService: NgbModal,
    private mocCommentService: MocCommentService,
    private publishedMocService: PublishedMocService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(mocCommentData?: MocCommentData) {

    if (mocCommentData != null) {

      if (!this.mocCommentService.userIsBMCMocCommentReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Moc Comments`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.mocCommentSubmitData = this.mocCommentService.ConvertToMocCommentSubmitData(mocCommentData);
      this.isEditMode = true;
      this.objectGuid = mocCommentData.objectGuid;

      this.buildFormValues(mocCommentData);

    } else {

      if (!this.mocCommentService.userIsBMCMocCommentWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Moc Comments`,
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
        this.mocCommentForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.mocCommentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.mocCommentModal, {
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

    if (this.mocCommentService.userIsBMCMocCommentWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Moc Comments`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.mocCommentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.mocCommentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.mocCommentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const mocCommentSubmitData: MocCommentSubmitData = {
        id: this.mocCommentSubmitData?.id || 0,
        publishedMocId: Number(formValue.publishedMocId),
        commenterTenantGuid: formValue.commenterTenantGuid!.trim(),
        commentText: formValue.commentText!.trim(),
        postedDate: dateTimeLocalToIsoUtc(formValue.postedDate!.trim())!,
        mocCommentId: formValue.mocCommentId ? Number(formValue.mocCommentId) : null,
        isEdited: !!formValue.isEdited,
        isHidden: !!formValue.isHidden,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateMocComment(mocCommentSubmitData);
      } else {
        this.addMocComment(mocCommentSubmitData);
      }
  }

  private addMocComment(mocCommentData: MocCommentSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    mocCommentData.active = true;
    mocCommentData.deleted = false;
    this.mocCommentService.PostMocComment(mocCommentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newMocComment) => {

        this.mocCommentService.ClearAllCaches();

        this.mocCommentChanged.next([newMocComment]);

        this.alertService.showMessage("Moc Comment added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/moccomment', newMocComment.id]);
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
                                   'You do not have permission to save this Moc Comment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Comment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Comment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateMocComment(mocCommentData: MocCommentSubmitData) {
    this.mocCommentService.PutMocComment(mocCommentData.id, mocCommentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedMocComment) => {

        this.mocCommentService.ClearAllCaches();

        this.mocCommentChanged.next([updatedMocComment]);

        this.alertService.showMessage("Moc Comment updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Moc Comment.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Comment.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Comment could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(mocCommentData: MocCommentData | null) {

    if (mocCommentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.mocCommentForm.reset({
        publishedMocId: null,
        commenterTenantGuid: '',
        commentText: '',
        postedDate: '',
        mocCommentId: null,
        isEdited: false,
        isHidden: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.mocCommentForm.reset({
        publishedMocId: mocCommentData.publishedMocId,
        commenterTenantGuid: mocCommentData.commenterTenantGuid ?? '',
        commentText: mocCommentData.commentText ?? '',
        postedDate: isoUtcStringToDateTimeLocal(mocCommentData.postedDate) ?? '',
        mocCommentId: mocCommentData.mocCommentId,
        isEdited: mocCommentData.isEdited ?? false,
        isHidden: mocCommentData.isHidden ?? false,
        active: mocCommentData.active ?? true,
        deleted: mocCommentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.mocCommentForm.markAsPristine();
    this.mocCommentForm.markAsUntouched();
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


  public userIsBMCMocCommentReader(): boolean {
    return this.mocCommentService.userIsBMCMocCommentReader();
  }

  public userIsBMCMocCommentWriter(): boolean {
    return this.mocCommentService.userIsBMCMocCommentWriter();
  }
}
