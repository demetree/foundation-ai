/*
   GENERATED FORM FOR THE PUBLISHEDMOCCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PublishedMocChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to published-moc-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PublishedMocChangeHistoryService, PublishedMocChangeHistoryData, PublishedMocChangeHistorySubmitData } from '../../../bmc-data-services/published-moc-change-history.service';
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
interface PublishedMocChangeHistoryFormValues {
  publishedMocId: number | bigint,       // For FK link number
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  timeStamp: string,
  userId: string,     // Stored as string for form input, converted to number on submit.
  data: string,
};

@Component({
  selector: 'app-published-moc-change-history-add-edit',
  templateUrl: './published-moc-change-history-add-edit.component.html',
  styleUrls: ['./published-moc-change-history-add-edit.component.scss']
})
export class PublishedMocChangeHistoryAddEditComponent {
  @ViewChild('publishedMocChangeHistoryModal') publishedMocChangeHistoryModal!: TemplateRef<any>;
  @Output() publishedMocChangeHistoryChanged = new Subject<PublishedMocChangeHistoryData[]>();
  @Input() publishedMocChangeHistorySubmitData: PublishedMocChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PublishedMocChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public publishedMocChangeHistoryForm: FormGroup = this.fb.group({
        publishedMocId: [null, Validators.required],
        versionNumber: [''],
        timeStamp: ['', Validators.required],
        userId: ['', Validators.required],
        data: ['', Validators.required],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  publishedMocChangeHistories$ = this.publishedMocChangeHistoryService.GetPublishedMocChangeHistoryList();
  publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  constructor(
    private modalService: NgbModal,
    private publishedMocChangeHistoryService: PublishedMocChangeHistoryService,
    private publishedMocService: PublishedMocService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(publishedMocChangeHistoryData?: PublishedMocChangeHistoryData) {

    if (publishedMocChangeHistoryData != null) {

      if (!this.publishedMocChangeHistoryService.userIsBMCPublishedMocChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Published Moc Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.publishedMocChangeHistorySubmitData = this.publishedMocChangeHistoryService.ConvertToPublishedMocChangeHistorySubmitData(publishedMocChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(publishedMocChangeHistoryData);

    } else {

      if (!this.publishedMocChangeHistoryService.userIsBMCPublishedMocChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Published Moc Change Histories`,
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
        this.publishedMocChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.publishedMocChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.publishedMocChangeHistoryModal, {
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

    if (this.publishedMocChangeHistoryService.userIsBMCPublishedMocChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Published Moc Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.publishedMocChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.publishedMocChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.publishedMocChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const publishedMocChangeHistorySubmitData: PublishedMocChangeHistorySubmitData = {
        id: this.publishedMocChangeHistorySubmitData?.id || 0,
        publishedMocId: Number(formValue.publishedMocId),
        versionNumber: this.publishedMocChangeHistorySubmitData?.versionNumber ?? 0,
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        userId: Number(formValue.userId),
        data: formValue.data!.trim(),
   };

      if (this.isEditMode) {
        this.updatePublishedMocChangeHistory(publishedMocChangeHistorySubmitData);
      } else {
        this.addPublishedMocChangeHistory(publishedMocChangeHistorySubmitData);
      }
  }

  private addPublishedMocChangeHistory(publishedMocChangeHistoryData: PublishedMocChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    publishedMocChangeHistoryData.versionNumber = 0;
    this.publishedMocChangeHistoryService.PostPublishedMocChangeHistory(publishedMocChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPublishedMocChangeHistory) => {

        this.publishedMocChangeHistoryService.ClearAllCaches();

        this.publishedMocChangeHistoryChanged.next([newPublishedMocChangeHistory]);

        this.alertService.showMessage("Published Moc Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/publishedmocchangehistory', newPublishedMocChangeHistory.id]);
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
                                   'You do not have permission to save this Published Moc Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Published Moc Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Published Moc Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePublishedMocChangeHistory(publishedMocChangeHistoryData: PublishedMocChangeHistorySubmitData) {
    this.publishedMocChangeHistoryService.PutPublishedMocChangeHistory(publishedMocChangeHistoryData.id, publishedMocChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPublishedMocChangeHistory) => {

        this.publishedMocChangeHistoryService.ClearAllCaches();

        this.publishedMocChangeHistoryChanged.next([updatedPublishedMocChangeHistory]);

        this.alertService.showMessage("Published Moc Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Published Moc Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Published Moc Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Published Moc Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(publishedMocChangeHistoryData: PublishedMocChangeHistoryData | null) {

    if (publishedMocChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.publishedMocChangeHistoryForm.reset({
        publishedMocId: null,
        versionNumber: '',
        timeStamp: '',
        userId: '',
        data: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.publishedMocChangeHistoryForm.reset({
        publishedMocId: publishedMocChangeHistoryData.publishedMocId,
        versionNumber: publishedMocChangeHistoryData.versionNumber?.toString() ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(publishedMocChangeHistoryData.timeStamp) ?? '',
        userId: publishedMocChangeHistoryData.userId?.toString() ?? '',
        data: publishedMocChangeHistoryData.data ?? '',
      }, { emitEvent: false});
    }

    this.publishedMocChangeHistoryForm.markAsPristine();
    this.publishedMocChangeHistoryForm.markAsUntouched();
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


  public userIsBMCPublishedMocChangeHistoryReader(): boolean {
    return this.publishedMocChangeHistoryService.userIsBMCPublishedMocChangeHistoryReader();
  }

  public userIsBMCPublishedMocChangeHistoryWriter(): boolean {
    return this.publishedMocChangeHistoryService.userIsBMCPublishedMocChangeHistoryWriter();
  }
}
