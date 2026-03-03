/*
   GENERATED FORM FOR THE BRICKSETUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickSetUserLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-set-user-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickSetUserLinkService, BrickSetUserLinkData, BrickSetUserLinkSubmitData } from '../../../bmc-data-services/brick-set-user-link.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BrickSetUserLinkFormValues {
  brickSetUsername: string,
  encryptedUserHash: string,
  encryptedPassword: string | null,
  syncEnabled: boolean,
  syncDirection: string,
  lastSyncDate: string | null,
  lastPullDate: string | null,
  lastPushDate: string | null,
  lastSyncError: string | null,
  userHashStoredDate: string | null,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-brick-set-user-link-add-edit',
  templateUrl: './brick-set-user-link-add-edit.component.html',
  styleUrls: ['./brick-set-user-link-add-edit.component.scss']
})
export class BrickSetUserLinkAddEditComponent {
  @ViewChild('brickSetUserLinkModal') brickSetUserLinkModal!: TemplateRef<any>;
  @Output() brickSetUserLinkChanged = new Subject<BrickSetUserLinkData[]>();
  @Input() brickSetUserLinkSubmitData: BrickSetUserLinkSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickSetUserLinkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickSetUserLinkForm: FormGroup = this.fb.group({
        brickSetUsername: ['', Validators.required],
        encryptedUserHash: ['', Validators.required],
        encryptedPassword: [''],
        syncEnabled: [false],
        syncDirection: ['', Validators.required],
        lastSyncDate: [''],
        lastPullDate: [''],
        lastPushDate: [''],
        lastSyncError: [''],
        userHashStoredDate: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  brickSetUserLinks$ = this.brickSetUserLinkService.GetBrickSetUserLinkList();

  constructor(
    private modalService: NgbModal,
    private brickSetUserLinkService: BrickSetUserLinkService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(brickSetUserLinkData?: BrickSetUserLinkData) {

    if (brickSetUserLinkData != null) {

      if (!this.brickSetUserLinkService.userIsBMCBrickSetUserLinkReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Brick Set User Links`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.brickSetUserLinkSubmitData = this.brickSetUserLinkService.ConvertToBrickSetUserLinkSubmitData(brickSetUserLinkData);
      this.isEditMode = true;
      this.objectGuid = brickSetUserLinkData.objectGuid;

      this.buildFormValues(brickSetUserLinkData);

    } else {

      if (!this.brickSetUserLinkService.userIsBMCBrickSetUserLinkWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Brick Set User Links`,
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
        this.brickSetUserLinkForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickSetUserLinkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.brickSetUserLinkModal, {
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

    if (this.brickSetUserLinkService.userIsBMCBrickSetUserLinkWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Brick Set User Links`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.brickSetUserLinkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickSetUserLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickSetUserLinkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickSetUserLinkSubmitData: BrickSetUserLinkSubmitData = {
        id: this.brickSetUserLinkSubmitData?.id || 0,
        brickSetUsername: formValue.brickSetUsername!.trim(),
        encryptedUserHash: formValue.encryptedUserHash!.trim(),
        encryptedPassword: formValue.encryptedPassword?.trim() || null,
        syncEnabled: !!formValue.syncEnabled,
        syncDirection: formValue.syncDirection!.trim(),
        lastSyncDate: formValue.lastSyncDate ? dateTimeLocalToIsoUtc(formValue.lastSyncDate.trim()) : null,
        lastPullDate: formValue.lastPullDate ? dateTimeLocalToIsoUtc(formValue.lastPullDate.trim()) : null,
        lastPushDate: formValue.lastPushDate ? dateTimeLocalToIsoUtc(formValue.lastPushDate.trim()) : null,
        lastSyncError: formValue.lastSyncError?.trim() || null,
        userHashStoredDate: formValue.userHashStoredDate ? dateTimeLocalToIsoUtc(formValue.userHashStoredDate.trim()) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBrickSetUserLink(brickSetUserLinkSubmitData);
      } else {
        this.addBrickSetUserLink(brickSetUserLinkSubmitData);
      }
  }

  private addBrickSetUserLink(brickSetUserLinkData: BrickSetUserLinkSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    brickSetUserLinkData.active = true;
    brickSetUserLinkData.deleted = false;
    this.brickSetUserLinkService.PostBrickSetUserLink(brickSetUserLinkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBrickSetUserLink) => {

        this.brickSetUserLinkService.ClearAllCaches();

        this.brickSetUserLinkChanged.next([newBrickSetUserLink]);

        this.alertService.showMessage("Brick Set User Link added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/bricksetuserlink', newBrickSetUserLink.id]);
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
                                   'You do not have permission to save this Brick Set User Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Set User Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Set User Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBrickSetUserLink(brickSetUserLinkData: BrickSetUserLinkSubmitData) {
    this.brickSetUserLinkService.PutBrickSetUserLink(brickSetUserLinkData.id, brickSetUserLinkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBrickSetUserLink) => {

        this.brickSetUserLinkService.ClearAllCaches();

        this.brickSetUserLinkChanged.next([updatedBrickSetUserLink]);

        this.alertService.showMessage("Brick Set User Link updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Brick Set User Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Set User Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Set User Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(brickSetUserLinkData: BrickSetUserLinkData | null) {

    if (brickSetUserLinkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickSetUserLinkForm.reset({
        brickSetUsername: '',
        encryptedUserHash: '',
        encryptedPassword: '',
        syncEnabled: false,
        syncDirection: '',
        lastSyncDate: '',
        lastPullDate: '',
        lastPushDate: '',
        lastSyncError: '',
        userHashStoredDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickSetUserLinkForm.reset({
        brickSetUsername: brickSetUserLinkData.brickSetUsername ?? '',
        encryptedUserHash: brickSetUserLinkData.encryptedUserHash ?? '',
        encryptedPassword: brickSetUserLinkData.encryptedPassword ?? '',
        syncEnabled: brickSetUserLinkData.syncEnabled ?? false,
        syncDirection: brickSetUserLinkData.syncDirection ?? '',
        lastSyncDate: isoUtcStringToDateTimeLocal(brickSetUserLinkData.lastSyncDate) ?? '',
        lastPullDate: isoUtcStringToDateTimeLocal(brickSetUserLinkData.lastPullDate) ?? '',
        lastPushDate: isoUtcStringToDateTimeLocal(brickSetUserLinkData.lastPushDate) ?? '',
        lastSyncError: brickSetUserLinkData.lastSyncError ?? '',
        userHashStoredDate: isoUtcStringToDateTimeLocal(brickSetUserLinkData.userHashStoredDate) ?? '',
        active: brickSetUserLinkData.active ?? true,
        deleted: brickSetUserLinkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickSetUserLinkForm.markAsPristine();
    this.brickSetUserLinkForm.markAsUntouched();
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


  public userIsBMCBrickSetUserLinkReader(): boolean {
    return this.brickSetUserLinkService.userIsBMCBrickSetUserLinkReader();
  }

  public userIsBMCBrickSetUserLinkWriter(): boolean {
    return this.brickSetUserLinkService.userIsBMCBrickSetUserLinkWriter();
  }
}
