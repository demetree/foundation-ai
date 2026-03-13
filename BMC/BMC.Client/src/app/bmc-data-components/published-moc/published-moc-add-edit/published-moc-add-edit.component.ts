/*
   GENERATED FORM FOR THE PUBLISHEDMOC TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PublishedMoc table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to published-moc-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PublishedMocService, PublishedMocData, PublishedMocSubmitData } from '../../../bmc-data-services/published-moc.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ProjectService } from '../../../bmc-data-services/project.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PublishedMocFormValues {
  projectId: number | bigint,       // For FK link number
  name: string,
  description: string | null,
  thumbnailImagePath: string | null,
  tags: string | null,
  isPublished: boolean,
  isFeatured: boolean,
  publishedDate: string | null,
  viewCount: string,     // Stored as string for form input, converted to number on submit.
  likeCount: string,     // Stored as string for form input, converted to number on submit.
  commentCount: string,     // Stored as string for form input, converted to number on submit.
  favouriteCount: string,     // Stored as string for form input, converted to number on submit.
  partCount: string | null,     // Stored as string for form input, converted to number on submit.
  allowForking: boolean,
  visibility: string,
  forkCount: string,     // Stored as string for form input, converted to number on submit.
  forkedFromMocId: number | bigint | null,       // For FK link number
  licenseName: string | null,
  readmeMarkdown: string | null,
  slug: string | null,
  defaultBranchName: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-published-moc-add-edit',
  templateUrl: './published-moc-add-edit.component.html',
  styleUrls: ['./published-moc-add-edit.component.scss']
})
export class PublishedMocAddEditComponent {
  @ViewChild('publishedMocModal') publishedMocModal!: TemplateRef<any>;
  @Output() publishedMocChanged = new Subject<PublishedMocData[]>();
  @Input() publishedMocSubmitData: PublishedMocSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PublishedMocFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public publishedMocForm: FormGroup = this.fb.group({
        projectId: [null, Validators.required],
        name: ['', Validators.required],
        description: [''],
        thumbnailImagePath: [''],
        tags: [''],
        isPublished: [false],
        isFeatured: [false],
        publishedDate: [''],
        viewCount: ['', Validators.required],
        likeCount: ['', Validators.required],
        commentCount: ['', Validators.required],
        favouriteCount: ['', Validators.required],
        partCount: [''],
        allowForking: [false],
        visibility: ['', Validators.required],
        forkCount: ['', Validators.required],
        forkedFromMocId: [null],
        licenseName: [''],
        readmeMarkdown: [''],
        slug: [''],
        defaultBranchName: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  publishedMocs$ = this.publishedMocService.GetPublishedMocList();
  projects$ = this.projectService.GetProjectList();

  constructor(
    private modalService: NgbModal,
    private publishedMocService: PublishedMocService,
    private projectService: ProjectService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(publishedMocData?: PublishedMocData) {

    if (publishedMocData != null) {

      if (!this.publishedMocService.userIsBMCPublishedMocReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Published Mocs`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.publishedMocSubmitData = this.publishedMocService.ConvertToPublishedMocSubmitData(publishedMocData);
      this.isEditMode = true;
      this.objectGuid = publishedMocData.objectGuid;

      this.buildFormValues(publishedMocData);

    } else {

      if (!this.publishedMocService.userIsBMCPublishedMocWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Published Mocs`,
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
        this.publishedMocForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.publishedMocForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.publishedMocModal, {
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

    if (this.publishedMocService.userIsBMCPublishedMocWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Published Mocs`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.publishedMocForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.publishedMocForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.publishedMocForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const publishedMocSubmitData: PublishedMocSubmitData = {
        id: this.publishedMocSubmitData?.id || 0,
        projectId: Number(formValue.projectId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        thumbnailImagePath: formValue.thumbnailImagePath?.trim() || null,
        tags: formValue.tags?.trim() || null,
        isPublished: !!formValue.isPublished,
        isFeatured: !!formValue.isFeatured,
        publishedDate: formValue.publishedDate ? dateTimeLocalToIsoUtc(formValue.publishedDate.trim()) : null,
        viewCount: Number(formValue.viewCount),
        likeCount: Number(formValue.likeCount),
        commentCount: Number(formValue.commentCount),
        favouriteCount: Number(formValue.favouriteCount),
        partCount: formValue.partCount ? Number(formValue.partCount) : null,
        allowForking: !!formValue.allowForking,
        visibility: formValue.visibility!.trim(),
        forkCount: Number(formValue.forkCount),
        forkedFromMocId: formValue.forkedFromMocId ? Number(formValue.forkedFromMocId) : null,
        licenseName: formValue.licenseName?.trim() || null,
        readmeMarkdown: formValue.readmeMarkdown?.trim() || null,
        slug: formValue.slug?.trim() || null,
        defaultBranchName: formValue.defaultBranchName?.trim() || null,
        versionNumber: this.publishedMocSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePublishedMoc(publishedMocSubmitData);
      } else {
        this.addPublishedMoc(publishedMocSubmitData);
      }
  }

  private addPublishedMoc(publishedMocData: PublishedMocSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    publishedMocData.versionNumber = 0;
    publishedMocData.active = true;
    publishedMocData.deleted = false;
    this.publishedMocService.PostPublishedMoc(publishedMocData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPublishedMoc) => {

        this.publishedMocService.ClearAllCaches();

        this.publishedMocChanged.next([newPublishedMoc]);

        this.alertService.showMessage("Published Moc added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/publishedmoc', newPublishedMoc.id]);
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
                                   'You do not have permission to save this Published Moc.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Published Moc.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Published Moc could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePublishedMoc(publishedMocData: PublishedMocSubmitData) {
    this.publishedMocService.PutPublishedMoc(publishedMocData.id, publishedMocData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPublishedMoc) => {

        this.publishedMocService.ClearAllCaches();

        this.publishedMocChanged.next([updatedPublishedMoc]);

        this.alertService.showMessage("Published Moc updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Published Moc.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Published Moc.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Published Moc could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(publishedMocData: PublishedMocData | null) {

    if (publishedMocData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.publishedMocForm.reset({
        projectId: null,
        name: '',
        description: '',
        thumbnailImagePath: '',
        tags: '',
        isPublished: false,
        isFeatured: false,
        publishedDate: '',
        viewCount: '',
        likeCount: '',
        commentCount: '',
        favouriteCount: '',
        partCount: '',
        allowForking: false,
        visibility: '',
        forkCount: '',
        forkedFromMocId: null,
        licenseName: '',
        readmeMarkdown: '',
        slug: '',
        defaultBranchName: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.publishedMocForm.reset({
        projectId: publishedMocData.projectId,
        name: publishedMocData.name ?? '',
        description: publishedMocData.description ?? '',
        thumbnailImagePath: publishedMocData.thumbnailImagePath ?? '',
        tags: publishedMocData.tags ?? '',
        isPublished: publishedMocData.isPublished ?? false,
        isFeatured: publishedMocData.isFeatured ?? false,
        publishedDate: isoUtcStringToDateTimeLocal(publishedMocData.publishedDate) ?? '',
        viewCount: publishedMocData.viewCount?.toString() ?? '',
        likeCount: publishedMocData.likeCount?.toString() ?? '',
        commentCount: publishedMocData.commentCount?.toString() ?? '',
        favouriteCount: publishedMocData.favouriteCount?.toString() ?? '',
        partCount: publishedMocData.partCount?.toString() ?? '',
        allowForking: publishedMocData.allowForking ?? false,
        visibility: publishedMocData.visibility ?? '',
        forkCount: publishedMocData.forkCount?.toString() ?? '',
        forkedFromMocId: publishedMocData.forkedFromMocId,
        licenseName: publishedMocData.licenseName ?? '',
        readmeMarkdown: publishedMocData.readmeMarkdown ?? '',
        slug: publishedMocData.slug ?? '',
        defaultBranchName: publishedMocData.defaultBranchName ?? '',
        versionNumber: publishedMocData.versionNumber?.toString() ?? '',
        active: publishedMocData.active ?? true,
        deleted: publishedMocData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.publishedMocForm.markAsPristine();
    this.publishedMocForm.markAsUntouched();
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


  public userIsBMCPublishedMocReader(): boolean {
    return this.publishedMocService.userIsBMCPublishedMocReader();
  }

  public userIsBMCPublishedMocWriter(): boolean {
    return this.publishedMocService.userIsBMCPublishedMocWriter();
  }
}
