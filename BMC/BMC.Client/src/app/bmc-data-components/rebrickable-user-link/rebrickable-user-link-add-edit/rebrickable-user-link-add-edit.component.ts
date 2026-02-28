/*
   GENERATED FORM FOR THE REBRICKABLEUSERLINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from RebrickableUserLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to rebrickable-user-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { RebrickableUserLinkService, RebrickableUserLinkData, RebrickableUserLinkSubmitData } from '../../../bmc-data-services/rebrickable-user-link.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface RebrickableUserLinkFormValues {
  rebrickableUsername: string,
  encryptedApiToken: string,
  lastSyncDate: string | null,
  syncEnabled: boolean,
  syncDirectionFlags: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-rebrickable-user-link-add-edit',
  templateUrl: './rebrickable-user-link-add-edit.component.html',
  styleUrls: ['./rebrickable-user-link-add-edit.component.scss']
})
export class RebrickableUserLinkAddEditComponent {
  @ViewChild('rebrickableUserLinkModal') rebrickableUserLinkModal!: TemplateRef<any>;
  @Output() rebrickableUserLinkChanged = new Subject<RebrickableUserLinkData[]>();
  @Input() rebrickableUserLinkSubmitData: RebrickableUserLinkSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<RebrickableUserLinkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public rebrickableUserLinkForm: FormGroup = this.fb.group({
        rebrickableUsername: ['', Validators.required],
        encryptedApiToken: ['', Validators.required],
        lastSyncDate: [''],
        syncEnabled: [false],
        syncDirectionFlags: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  rebrickableUserLinks$ = this.rebrickableUserLinkService.GetRebrickableUserLinkList();

  constructor(
    private modalService: NgbModal,
    private rebrickableUserLinkService: RebrickableUserLinkService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(rebrickableUserLinkData?: RebrickableUserLinkData) {

    if (rebrickableUserLinkData != null) {

      if (!this.rebrickableUserLinkService.userIsBMCRebrickableUserLinkReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Rebrickable User Links`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.rebrickableUserLinkSubmitData = this.rebrickableUserLinkService.ConvertToRebrickableUserLinkSubmitData(rebrickableUserLinkData);
      this.isEditMode = true;
      this.objectGuid = rebrickableUserLinkData.objectGuid;

      this.buildFormValues(rebrickableUserLinkData);

    } else {

      if (!this.rebrickableUserLinkService.userIsBMCRebrickableUserLinkWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Rebrickable User Links`,
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
        this.rebrickableUserLinkForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.rebrickableUserLinkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.rebrickableUserLinkModal, {
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

    if (this.rebrickableUserLinkService.userIsBMCRebrickableUserLinkWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Rebrickable User Links`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.rebrickableUserLinkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.rebrickableUserLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.rebrickableUserLinkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const rebrickableUserLinkSubmitData: RebrickableUserLinkSubmitData = {
        id: this.rebrickableUserLinkSubmitData?.id || 0,
        rebrickableUsername: formValue.rebrickableUsername!.trim(),
        encryptedApiToken: formValue.encryptedApiToken!.trim(),
        lastSyncDate: formValue.lastSyncDate ? dateTimeLocalToIsoUtc(formValue.lastSyncDate.trim()) : null,
        syncEnabled: !!formValue.syncEnabled,
        syncDirectionFlags: formValue.syncDirectionFlags!.trim(),
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateRebrickableUserLink(rebrickableUserLinkSubmitData);
      } else {
        this.addRebrickableUserLink(rebrickableUserLinkSubmitData);
      }
  }

  private addRebrickableUserLink(rebrickableUserLinkData: RebrickableUserLinkSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    rebrickableUserLinkData.active = true;
    rebrickableUserLinkData.deleted = false;
    this.rebrickableUserLinkService.PostRebrickableUserLink(rebrickableUserLinkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newRebrickableUserLink) => {

        this.rebrickableUserLinkService.ClearAllCaches();

        this.rebrickableUserLinkChanged.next([newRebrickableUserLink]);

        this.alertService.showMessage("Rebrickable User Link added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/rebrickableuserlink', newRebrickableUserLink.id]);
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
                                   'You do not have permission to save this Rebrickable User Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rebrickable User Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rebrickable User Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateRebrickableUserLink(rebrickableUserLinkData: RebrickableUserLinkSubmitData) {
    this.rebrickableUserLinkService.PutRebrickableUserLink(rebrickableUserLinkData.id, rebrickableUserLinkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedRebrickableUserLink) => {

        this.rebrickableUserLinkService.ClearAllCaches();

        this.rebrickableUserLinkChanged.next([updatedRebrickableUserLink]);

        this.alertService.showMessage("Rebrickable User Link updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Rebrickable User Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rebrickable User Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rebrickable User Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(rebrickableUserLinkData: RebrickableUserLinkData | null) {

    if (rebrickableUserLinkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.rebrickableUserLinkForm.reset({
        rebrickableUsername: '',
        encryptedApiToken: '',
        lastSyncDate: '',
        syncEnabled: false,
        syncDirectionFlags: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.rebrickableUserLinkForm.reset({
        rebrickableUsername: rebrickableUserLinkData.rebrickableUsername ?? '',
        encryptedApiToken: rebrickableUserLinkData.encryptedApiToken ?? '',
        lastSyncDate: isoUtcStringToDateTimeLocal(rebrickableUserLinkData.lastSyncDate) ?? '',
        syncEnabled: rebrickableUserLinkData.syncEnabled ?? false,
        syncDirectionFlags: rebrickableUserLinkData.syncDirectionFlags ?? '',
        active: rebrickableUserLinkData.active ?? true,
        deleted: rebrickableUserLinkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.rebrickableUserLinkForm.markAsPristine();
    this.rebrickableUserLinkForm.markAsUntouched();
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


  public userIsBMCRebrickableUserLinkReader(): boolean {
    return this.rebrickableUserLinkService.userIsBMCRebrickableUserLinkReader();
  }

  public userIsBMCRebrickableUserLinkWriter(): boolean {
    return this.rebrickableUserLinkService.userIsBMCRebrickableUserLinkWriter();
  }
}
