/*
   GENERATED FORM FOR THE PAGECHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PageChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to page-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PageChangeHistoryService, PageChangeHistoryData, PageChangeHistorySubmitData } from '../../../community-data-services/page-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PageService } from '../../../community-data-services/page.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PageChangeHistoryFormValues {
};

@Component({
  selector: 'app-page-change-history-add-edit',
  templateUrl: './page-change-history-add-edit.component.html',
  styleUrls: ['./page-change-history-add-edit.component.scss']
})
export class PageChangeHistoryAddEditComponent {
  @ViewChild('pageChangeHistoryModal') pageChangeHistoryModal!: TemplateRef<any>;
  @Output() pageChangeHistoryChanged = new Subject<PageChangeHistoryData[]>();
  @Input() pageChangeHistorySubmitData: PageChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PageChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public pageChangeHistoryForm: FormGroup = this.fb.group({
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  pageChangeHistories$ = this.pageChangeHistoryService.GetPageChangeHistoryList();
  pages$ = this.pageService.GetPageList();

  constructor(
    private modalService: NgbModal,
    private pageChangeHistoryService: PageChangeHistoryService,
    private pageService: PageService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(pageChangeHistoryData?: PageChangeHistoryData) {

    if (pageChangeHistoryData != null) {

      if (!this.pageChangeHistoryService.userIsCommunityPageChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Page Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.pageChangeHistorySubmitData = this.pageChangeHistoryService.ConvertToPageChangeHistorySubmitData(pageChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(pageChangeHistoryData);

    } else {

      if (!this.pageChangeHistoryService.userIsCommunityPageChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Page Change Histories`,
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
        this.pageChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.pageChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.pageChangeHistoryModal, {
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

    if (this.pageChangeHistoryService.userIsCommunityPageChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Page Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.pageChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.pageChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.pageChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const pageChangeHistorySubmitData: PageChangeHistorySubmitData = {
        id: this.pageChangeHistorySubmitData?.id || 0,
   };

      if (this.isEditMode) {
        this.updatePageChangeHistory(pageChangeHistorySubmitData);
      } else {
        this.addPageChangeHistory(pageChangeHistorySubmitData);
      }
  }

  private addPageChangeHistory(pageChangeHistoryData: PageChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    pageChangeHistoryData.versionNumber = 0;
    this.pageChangeHistoryService.PostPageChangeHistory(pageChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPageChangeHistory) => {

        this.pageChangeHistoryService.ClearAllCaches();

        this.pageChangeHistoryChanged.next([newPageChangeHistory]);

        this.alertService.showMessage("Page Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/pagechangehistory', newPageChangeHistory.id]);
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
                                   'You do not have permission to save this Page Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Page Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Page Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePageChangeHistory(pageChangeHistoryData: PageChangeHistorySubmitData) {
    this.pageChangeHistoryService.PutPageChangeHistory(pageChangeHistoryData.id, pageChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPageChangeHistory) => {

        this.pageChangeHistoryService.ClearAllCaches();

        this.pageChangeHistoryChanged.next([updatedPageChangeHistory]);

        this.alertService.showMessage("Page Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Page Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Page Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Page Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(pageChangeHistoryData: PageChangeHistoryData | null) {

    if (pageChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.pageChangeHistoryForm.reset({
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.pageChangeHistoryForm.reset({
      }, { emitEvent: false});
    }

    this.pageChangeHistoryForm.markAsPristine();
    this.pageChangeHistoryForm.markAsUntouched();
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


  public userIsCommunityPageChangeHistoryReader(): boolean {
    return this.pageChangeHistoryService.userIsCommunityPageChangeHistoryReader();
  }

  public userIsCommunityPageChangeHistoryWriter(): boolean {
    return this.pageChangeHistoryService.userIsCommunityPageChangeHistoryWriter();
  }
}
