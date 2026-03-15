/*
   GENERATED FORM FOR THE PAGE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Page table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to page-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PageService, PageData, PageSubmitData } from '../../../community-data-services/page.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PageFormValues {
};

@Component({
  selector: 'app-page-add-edit',
  templateUrl: './page-add-edit.component.html',
  styleUrls: ['./page-add-edit.component.scss']
})
export class PageAddEditComponent {
  @ViewChild('pageModal') pageModal!: TemplateRef<any>;
  @Output() pageChanged = new Subject<PageData[]>();
  @Input() pageSubmitData: PageSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PageFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public pageForm: FormGroup = this.fb.group({
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  pages$ = this.pageService.GetPageList();

  constructor(
    private modalService: NgbModal,
    private pageService: PageService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(pageData?: PageData) {

    if (pageData != null) {

      if (!this.pageService.userIsCommunityPageReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Pages`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.pageSubmitData = this.pageService.ConvertToPageSubmitData(pageData);
      this.isEditMode = true;

      this.buildFormValues(pageData);

    } else {

      if (!this.pageService.userIsCommunityPageWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Pages`,
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
        this.pageForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.pageForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.pageModal, {
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

    if (this.pageService.userIsCommunityPageWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Pages`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.pageForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.pageForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.pageForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const pageSubmitData: PageSubmitData = {
        id: this.pageSubmitData?.id || 0,
   };

      if (this.isEditMode) {
        this.updatePage(pageSubmitData);
      } else {
        this.addPage(pageSubmitData);
      }
  }

  private addPage(pageData: PageSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    pageData.versionNumber = 0;
    pageData.active = true;
    pageData.deleted = false;
    this.pageService.PostPage(pageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPage) => {

        this.pageService.ClearAllCaches();

        this.pageChanged.next([newPage]);

        this.alertService.showMessage("Page added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/page', newPage.id]);
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
                                   'You do not have permission to save this Page.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Page.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Page could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePage(pageData: PageSubmitData) {
    this.pageService.PutPage(pageData.id, pageData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPage) => {

        this.pageService.ClearAllCaches();

        this.pageChanged.next([updatedPage]);

        this.alertService.showMessage("Page updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Page.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Page.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Page could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(pageData: PageData | null) {

    if (pageData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.pageForm.reset({
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.pageForm.reset({
      }, { emitEvent: false});
    }

    this.pageForm.markAsPristine();
    this.pageForm.markAsUntouched();
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


  public userIsCommunityPageReader(): boolean {
    return this.pageService.userIsCommunityPageReader();
  }

  public userIsCommunityPageWriter(): boolean {
    return this.pageService.userIsCommunityPageWriter();
  }
}
