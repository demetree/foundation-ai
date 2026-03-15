/*
   GENERATED FORM FOR THE ANNOUNCEMENTCHANGEHISTORY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AnnouncementChangeHistory table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to announcement-change-history-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AnnouncementChangeHistoryService, AnnouncementChangeHistoryData, AnnouncementChangeHistorySubmitData } from '../../../community-data-services/announcement-change-history.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AnnouncementService } from '../../../community-data-services/announcement.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface AnnouncementChangeHistoryFormValues {
};

@Component({
  selector: 'app-announcement-change-history-add-edit',
  templateUrl: './announcement-change-history-add-edit.component.html',
  styleUrls: ['./announcement-change-history-add-edit.component.scss']
})
export class AnnouncementChangeHistoryAddEditComponent {
  @ViewChild('announcementChangeHistoryModal') announcementChangeHistoryModal!: TemplateRef<any>;
  @Output() announcementChangeHistoryChanged = new Subject<AnnouncementChangeHistoryData[]>();
  @Input() announcementChangeHistorySubmitData: AnnouncementChangeHistorySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AnnouncementChangeHistoryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public announcementChangeHistoryForm: FormGroup = this.fb.group({
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  announcementChangeHistories$ = this.announcementChangeHistoryService.GetAnnouncementChangeHistoryList();
  announcements$ = this.announcementService.GetAnnouncementList();

  constructor(
    private modalService: NgbModal,
    private announcementChangeHistoryService: AnnouncementChangeHistoryService,
    private announcementService: AnnouncementService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(announcementChangeHistoryData?: AnnouncementChangeHistoryData) {

    if (announcementChangeHistoryData != null) {

      if (!this.announcementChangeHistoryService.userIsCommunityAnnouncementChangeHistoryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Announcement Change Histories`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.announcementChangeHistorySubmitData = this.announcementChangeHistoryService.ConvertToAnnouncementChangeHistorySubmitData(announcementChangeHistoryData);
      this.isEditMode = true;

      this.buildFormValues(announcementChangeHistoryData);

    } else {

      if (!this.announcementChangeHistoryService.userIsCommunityAnnouncementChangeHistoryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Announcement Change Histories`,
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
        this.announcementChangeHistoryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.announcementChangeHistoryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.announcementChangeHistoryModal, {
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

    if (this.announcementChangeHistoryService.userIsCommunityAnnouncementChangeHistoryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Announcement Change Histories`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.announcementChangeHistoryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.announcementChangeHistoryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.announcementChangeHistoryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const announcementChangeHistorySubmitData: AnnouncementChangeHistorySubmitData = {
        id: this.announcementChangeHistorySubmitData?.id || 0,
   };

      if (this.isEditMode) {
        this.updateAnnouncementChangeHistory(announcementChangeHistorySubmitData);
      } else {
        this.addAnnouncementChangeHistory(announcementChangeHistorySubmitData);
      }
  }

  private addAnnouncementChangeHistory(announcementChangeHistoryData: AnnouncementChangeHistorySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    announcementChangeHistoryData.versionNumber = 0;
    this.announcementChangeHistoryService.PostAnnouncementChangeHistory(announcementChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAnnouncementChangeHistory) => {

        this.announcementChangeHistoryService.ClearAllCaches();

        this.announcementChangeHistoryChanged.next([newAnnouncementChangeHistory]);

        this.alertService.showMessage("Announcement Change History added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/announcementchangehistory', newAnnouncementChangeHistory.id]);
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
                                   'You do not have permission to save this Announcement Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Announcement Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Announcement Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAnnouncementChangeHistory(announcementChangeHistoryData: AnnouncementChangeHistorySubmitData) {
    this.announcementChangeHistoryService.PutAnnouncementChangeHistory(announcementChangeHistoryData.id, announcementChangeHistoryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAnnouncementChangeHistory) => {

        this.announcementChangeHistoryService.ClearAllCaches();

        this.announcementChangeHistoryChanged.next([updatedAnnouncementChangeHistory]);

        this.alertService.showMessage("Announcement Change History updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Announcement Change History.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Announcement Change History.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Announcement Change History could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(announcementChangeHistoryData: AnnouncementChangeHistoryData | null) {

    if (announcementChangeHistoryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.announcementChangeHistoryForm.reset({
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.announcementChangeHistoryForm.reset({
      }, { emitEvent: false});
    }

    this.announcementChangeHistoryForm.markAsPristine();
    this.announcementChangeHistoryForm.markAsUntouched();
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


  public userIsCommunityAnnouncementChangeHistoryReader(): boolean {
    return this.announcementChangeHistoryService.userIsCommunityAnnouncementChangeHistoryReader();
  }

  public userIsCommunityAnnouncementChangeHistoryWriter(): boolean {
    return this.announcementChangeHistoryService.userIsCommunityAnnouncementChangeHistoryWriter();
  }
}
