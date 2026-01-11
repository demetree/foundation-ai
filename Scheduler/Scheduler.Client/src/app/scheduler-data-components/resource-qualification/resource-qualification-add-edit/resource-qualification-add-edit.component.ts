/*
   GENERATED FORM FOR THE RESOURCEQUALIFICATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from ResourceQualification table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-qualification-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceQualificationService, ResourceQualificationData, ResourceQualificationSubmitData } from '../../../scheduler-data-services/resource-qualification.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { QualificationService } from '../../../scheduler-data-services/qualification.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ResourceQualificationFormValues {
  resourceId: number | bigint,       // For FK link number
  qualificationId: number | bigint,       // For FK link number
  issueDate: string | null,
  expiryDate: string | null,
  issuer: string | null,
  notes: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-resource-qualification-add-edit',
  templateUrl: './resource-qualification-add-edit.component.html',
  styleUrls: ['./resource-qualification-add-edit.component.scss']
})
export class ResourceQualificationAddEditComponent {
  @ViewChild('resourceQualificationModal') resourceQualificationModal!: TemplateRef<any>;
  @Output() resourceQualificationChanged = new Subject<ResourceQualificationData[]>();
  @Input() resourceQualificationSubmitData: ResourceQualificationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ResourceQualificationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public resourceQualificationForm: FormGroup = this.fb.group({
        resourceId: [null, Validators.required],
        qualificationId: [null, Validators.required],
        issueDate: [''],
        expiryDate: [''],
        issuer: [''],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  resourceQualifications$ = this.resourceQualificationService.GetResourceQualificationList();
  resources$ = this.resourceService.GetResourceList();
  qualifications$ = this.qualificationService.GetQualificationList();

  constructor(
    private modalService: NgbModal,
    private resourceQualificationService: ResourceQualificationService,
    private resourceService: ResourceService,
    private qualificationService: QualificationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(resourceQualificationData?: ResourceQualificationData) {

    if (resourceQualificationData != null) {

      if (!this.resourceQualificationService.userIsSchedulerResourceQualificationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Resource Qualifications`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.resourceQualificationSubmitData = this.resourceQualificationService.ConvertToResourceQualificationSubmitData(resourceQualificationData);
      this.isEditMode = true;
      this.objectGuid = resourceQualificationData.objectGuid;

      this.buildFormValues(resourceQualificationData);

    } else {

      if (!this.resourceQualificationService.userIsSchedulerResourceQualificationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Resource Qualifications`,
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
        this.resourceQualificationForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.resourceQualificationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.resourceQualificationModal, {
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

    if (this.resourceQualificationService.userIsSchedulerResourceQualificationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Resource Qualifications`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.resourceQualificationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.resourceQualificationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.resourceQualificationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const resourceQualificationSubmitData: ResourceQualificationSubmitData = {
        id: this.resourceQualificationSubmitData?.id || 0,
        resourceId: Number(formValue.resourceId),
        qualificationId: Number(formValue.qualificationId),
        issueDate: formValue.issueDate ? dateTimeLocalToIsoUtc(formValue.issueDate.trim()) : null,
        expiryDate: formValue.expiryDate ? dateTimeLocalToIsoUtc(formValue.expiryDate.trim()) : null,
        issuer: formValue.issuer?.trim() || null,
        notes: formValue.notes?.trim() || null,
        versionNumber: this.resourceQualificationSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateResourceQualification(resourceQualificationSubmitData);
      } else {
        this.addResourceQualification(resourceQualificationSubmitData);
      }
  }

  private addResourceQualification(resourceQualificationData: ResourceQualificationSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    resourceQualificationData.versionNumber = 0;
    resourceQualificationData.active = true;
    resourceQualificationData.deleted = false;
    this.resourceQualificationService.PostResourceQualification(resourceQualificationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newResourceQualification) => {

        this.resourceQualificationService.ClearAllCaches();

        this.resourceQualificationChanged.next([newResourceQualification]);

        this.alertService.showMessage("Resource Qualification added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/resourcequalification', newResourceQualification.id]);
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
                                   'You do not have permission to save this Resource Qualification.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource Qualification.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource Qualification could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateResourceQualification(resourceQualificationData: ResourceQualificationSubmitData) {
    this.resourceQualificationService.PutResourceQualification(resourceQualificationData.id, resourceQualificationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedResourceQualification) => {

        this.resourceQualificationService.ClearAllCaches();

        this.resourceQualificationChanged.next([updatedResourceQualification]);

        this.alertService.showMessage("Resource Qualification updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Resource Qualification.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource Qualification.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource Qualification could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(resourceQualificationData: ResourceQualificationData | null) {

    if (resourceQualificationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.resourceQualificationForm.reset({
        resourceId: null,
        qualificationId: null,
        issueDate: '',
        expiryDate: '',
        issuer: '',
        notes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.resourceQualificationForm.reset({
        resourceId: resourceQualificationData.resourceId,
        qualificationId: resourceQualificationData.qualificationId,
        issueDate: isoUtcStringToDateTimeLocal(resourceQualificationData.issueDate) ?? '',
        expiryDate: isoUtcStringToDateTimeLocal(resourceQualificationData.expiryDate) ?? '',
        issuer: resourceQualificationData.issuer ?? '',
        notes: resourceQualificationData.notes ?? '',
        versionNumber: resourceQualificationData.versionNumber?.toString() ?? '',
        active: resourceQualificationData.active ?? true,
        deleted: resourceQualificationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.resourceQualificationForm.markAsPristine();
    this.resourceQualificationForm.markAsUntouched();
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


  public userIsSchedulerResourceQualificationReader(): boolean {
    return this.resourceQualificationService.userIsSchedulerResourceQualificationReader();
  }

  public userIsSchedulerResourceQualificationWriter(): boolean {
    return this.resourceQualificationService.userIsSchedulerResourceQualificationWriter();
  }
}
