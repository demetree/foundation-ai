/*
   GENERATED FORM FOR THE MOCFORK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from MocFork table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to moc-fork-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MocForkService, MocForkData, MocForkSubmitData } from '../../../bmc-data-services/moc-fork.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
import { MocVersionService } from '../../../bmc-data-services/moc-version.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface MocForkFormValues {
  forkedMocId: number | bigint,       // For FK link number
  sourceMocId: number | bigint,       // For FK link number
  mocVersionId: number | bigint | null,       // For FK link number
  forkerTenantGuid: string,
  forkedDate: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-moc-fork-add-edit',
  templateUrl: './moc-fork-add-edit.component.html',
  styleUrls: ['./moc-fork-add-edit.component.scss']
})
export class MocForkAddEditComponent {
  @ViewChild('mocForkModal') mocForkModal!: TemplateRef<any>;
  @Output() mocForkChanged = new Subject<MocForkData[]>();
  @Input() mocForkSubmitData: MocForkSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MocForkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public mocForkForm: FormGroup = this.fb.group({
        forkedMocId: [null, Validators.required],
        sourceMocId: [null, Validators.required],
        mocVersionId: [null],
        forkerTenantGuid: ['', Validators.required],
        forkedDate: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  mocForks$ = this.mocForkService.GetMocForkList();
  publishedMocs$ = this.publishedMocService.GetPublishedMocList();
  mocVersions$ = this.mocVersionService.GetMocVersionList();

  constructor(
    private modalService: NgbModal,
    private mocForkService: MocForkService,
    private publishedMocService: PublishedMocService,
    private mocVersionService: MocVersionService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(mocForkData?: MocForkData) {

    if (mocForkData != null) {

      if (!this.mocForkService.userIsBMCMocForkReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Moc Forks`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.mocForkSubmitData = this.mocForkService.ConvertToMocForkSubmitData(mocForkData);
      this.isEditMode = true;
      this.objectGuid = mocForkData.objectGuid;

      this.buildFormValues(mocForkData);

    } else {

      if (!this.mocForkService.userIsBMCMocForkWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Moc Forks`,
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
        this.mocForkForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.mocForkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.mocForkModal, {
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

    if (this.mocForkService.userIsBMCMocForkWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Moc Forks`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.mocForkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.mocForkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.mocForkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const mocForkSubmitData: MocForkSubmitData = {
        id: this.mocForkSubmitData?.id || 0,
        forkedMocId: Number(formValue.forkedMocId),
        sourceMocId: Number(formValue.sourceMocId),
        mocVersionId: formValue.mocVersionId ? Number(formValue.mocVersionId) : null,
        forkerTenantGuid: formValue.forkerTenantGuid!.trim(),
        forkedDate: dateTimeLocalToIsoUtc(formValue.forkedDate!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateMocFork(mocForkSubmitData);
      } else {
        this.addMocFork(mocForkSubmitData);
      }
  }

  private addMocFork(mocForkData: MocForkSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    mocForkData.active = true;
    mocForkData.deleted = false;
    this.mocForkService.PostMocFork(mocForkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newMocFork) => {

        this.mocForkService.ClearAllCaches();

        this.mocForkChanged.next([newMocFork]);

        this.alertService.showMessage("Moc Fork added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/mocfork', newMocFork.id]);
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
                                   'You do not have permission to save this Moc Fork.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Fork.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Fork could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateMocFork(mocForkData: MocForkSubmitData) {
    this.mocForkService.PutMocFork(mocForkData.id, mocForkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedMocFork) => {

        this.mocForkService.ClearAllCaches();

        this.mocForkChanged.next([updatedMocFork]);

        this.alertService.showMessage("Moc Fork updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Moc Fork.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Moc Fork.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Moc Fork could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(mocForkData: MocForkData | null) {

    if (mocForkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.mocForkForm.reset({
        forkedMocId: null,
        sourceMocId: null,
        mocVersionId: null,
        forkerTenantGuid: '',
        forkedDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.mocForkForm.reset({
        forkedMocId: mocForkData.forkedMocId,
        sourceMocId: mocForkData.sourceMocId,
        mocVersionId: mocForkData.mocVersionId,
        forkerTenantGuid: mocForkData.forkerTenantGuid ?? '',
        forkedDate: isoUtcStringToDateTimeLocal(mocForkData.forkedDate) ?? '',
        active: mocForkData.active ?? true,
        deleted: mocForkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.mocForkForm.markAsPristine();
    this.mocForkForm.markAsUntouched();
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


  public userIsBMCMocForkReader(): boolean {
    return this.mocForkService.userIsBMCMocForkReader();
  }

  public userIsBMCMocForkWriter(): boolean {
    return this.mocForkService.userIsBMCMocForkWriter();
  }
}
