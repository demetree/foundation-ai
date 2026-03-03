/*
   GENERATED FORM FOR THE BRICKOWLUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickOwlUserLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-owl-user-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickOwlUserLinkService, BrickOwlUserLinkData, BrickOwlUserLinkSubmitData } from '../../../bmc-data-services/brick-owl-user-link.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BrickOwlUserLinkFormValues {
  encryptedApiKey: string | null,
  syncEnabled: boolean,
  syncDirection: string | null,
  lastSyncDate: string | null,
  lastPullDate: string | null,
  lastPushDate: string | null,
  lastSyncError: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-brick-owl-user-link-add-edit',
  templateUrl: './brick-owl-user-link-add-edit.component.html',
  styleUrls: ['./brick-owl-user-link-add-edit.component.scss']
})
export class BrickOwlUserLinkAddEditComponent {
  @ViewChild('brickOwlUserLinkModal') brickOwlUserLinkModal!: TemplateRef<any>;
  @Output() brickOwlUserLinkChanged = new Subject<BrickOwlUserLinkData[]>();
  @Input() brickOwlUserLinkSubmitData: BrickOwlUserLinkSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickOwlUserLinkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickOwlUserLinkForm: FormGroup = this.fb.group({
        encryptedApiKey: [''],
        syncEnabled: [false],
        syncDirection: [''],
        lastSyncDate: [''],
        lastPullDate: [''],
        lastPushDate: [''],
        lastSyncError: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  brickOwlUserLinks$ = this.brickOwlUserLinkService.GetBrickOwlUserLinkList();

  constructor(
    private modalService: NgbModal,
    private brickOwlUserLinkService: BrickOwlUserLinkService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(brickOwlUserLinkData?: BrickOwlUserLinkData) {

    if (brickOwlUserLinkData != null) {

      if (!this.brickOwlUserLinkService.userIsBMCBrickOwlUserLinkReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Brick Owl User Links`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.brickOwlUserLinkSubmitData = this.brickOwlUserLinkService.ConvertToBrickOwlUserLinkSubmitData(brickOwlUserLinkData);
      this.isEditMode = true;
      this.objectGuid = brickOwlUserLinkData.objectGuid;

      this.buildFormValues(brickOwlUserLinkData);

    } else {

      if (!this.brickOwlUserLinkService.userIsBMCBrickOwlUserLinkWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Brick Owl User Links`,
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
        this.brickOwlUserLinkForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickOwlUserLinkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.brickOwlUserLinkModal, {
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

    if (this.brickOwlUserLinkService.userIsBMCBrickOwlUserLinkWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Brick Owl User Links`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.brickOwlUserLinkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickOwlUserLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickOwlUserLinkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickOwlUserLinkSubmitData: BrickOwlUserLinkSubmitData = {
        id: this.brickOwlUserLinkSubmitData?.id || 0,
        encryptedApiKey: formValue.encryptedApiKey?.trim() || null,
        syncEnabled: !!formValue.syncEnabled,
        syncDirection: formValue.syncDirection?.trim() || null,
        lastSyncDate: formValue.lastSyncDate ? dateTimeLocalToIsoUtc(formValue.lastSyncDate.trim()) : null,
        lastPullDate: formValue.lastPullDate ? dateTimeLocalToIsoUtc(formValue.lastPullDate.trim()) : null,
        lastPushDate: formValue.lastPushDate ? dateTimeLocalToIsoUtc(formValue.lastPushDate.trim()) : null,
        lastSyncError: formValue.lastSyncError?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBrickOwlUserLink(brickOwlUserLinkSubmitData);
      } else {
        this.addBrickOwlUserLink(brickOwlUserLinkSubmitData);
      }
  }

  private addBrickOwlUserLink(brickOwlUserLinkData: BrickOwlUserLinkSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    brickOwlUserLinkData.active = true;
    brickOwlUserLinkData.deleted = false;
    this.brickOwlUserLinkService.PostBrickOwlUserLink(brickOwlUserLinkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBrickOwlUserLink) => {

        this.brickOwlUserLinkService.ClearAllCaches();

        this.brickOwlUserLinkChanged.next([newBrickOwlUserLink]);

        this.alertService.showMessage("Brick Owl User Link added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/brickowluserlink', newBrickOwlUserLink.id]);
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
                                   'You do not have permission to save this Brick Owl User Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Owl User Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Owl User Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBrickOwlUserLink(brickOwlUserLinkData: BrickOwlUserLinkSubmitData) {
    this.brickOwlUserLinkService.PutBrickOwlUserLink(brickOwlUserLinkData.id, brickOwlUserLinkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBrickOwlUserLink) => {

        this.brickOwlUserLinkService.ClearAllCaches();

        this.brickOwlUserLinkChanged.next([updatedBrickOwlUserLink]);

        this.alertService.showMessage("Brick Owl User Link updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Brick Owl User Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Owl User Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Owl User Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(brickOwlUserLinkData: BrickOwlUserLinkData | null) {

    if (brickOwlUserLinkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickOwlUserLinkForm.reset({
        encryptedApiKey: '',
        syncEnabled: false,
        syncDirection: '',
        lastSyncDate: '',
        lastPullDate: '',
        lastPushDate: '',
        lastSyncError: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickOwlUserLinkForm.reset({
        encryptedApiKey: brickOwlUserLinkData.encryptedApiKey ?? '',
        syncEnabled: brickOwlUserLinkData.syncEnabled ?? false,
        syncDirection: brickOwlUserLinkData.syncDirection ?? '',
        lastSyncDate: isoUtcStringToDateTimeLocal(brickOwlUserLinkData.lastSyncDate) ?? '',
        lastPullDate: isoUtcStringToDateTimeLocal(brickOwlUserLinkData.lastPullDate) ?? '',
        lastPushDate: isoUtcStringToDateTimeLocal(brickOwlUserLinkData.lastPushDate) ?? '',
        lastSyncError: brickOwlUserLinkData.lastSyncError ?? '',
        active: brickOwlUserLinkData.active ?? true,
        deleted: brickOwlUserLinkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickOwlUserLinkForm.markAsPristine();
    this.brickOwlUserLinkForm.markAsUntouched();
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


  public userIsBMCBrickOwlUserLinkReader(): boolean {
    return this.brickOwlUserLinkService.userIsBMCBrickOwlUserLinkReader();
  }

  public userIsBMCBrickOwlUserLinkWriter(): boolean {
    return this.brickOwlUserLinkService.userIsBMCBrickOwlUserLinkWriter();
  }
}
