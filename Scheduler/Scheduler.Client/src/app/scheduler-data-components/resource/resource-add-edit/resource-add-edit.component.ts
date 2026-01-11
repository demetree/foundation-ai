/*
   GENERATED FORM FOR THE RESOURCE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Resource table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to resource-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceService, ResourceData, ResourceSubmitData } from '../../../scheduler-data-services/resource.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { ResourceTypeService } from '../../../scheduler-data-services/resource-type.service';
import { ShiftPatternService } from '../../../scheduler-data-services/shift-pattern.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ResourceFormValues {
  name: string,
  description: string | null,
  officeId: number | bigint | null,       // For FK link number
  resourceTypeId: number | bigint,       // For FK link number
  shiftPatternId: number | bigint | null,       // For FK link number
  timeZoneId: number | bigint,       // For FK link number
  targetWeeklyWorkHours: string | null,     // Stored as string for form input, converted to number on submit.
  notes: string | null,
  externalId: string | null,
  color: string | null,
  attributes: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-resource-add-edit',
  templateUrl: './resource-add-edit.component.html',
  styleUrls: ['./resource-add-edit.component.scss']
})
export class ResourceAddEditComponent {
  @ViewChild('resourceModal') resourceModal!: TemplateRef<any>;
  @Output() resourceChanged = new Subject<ResourceData[]>();
  @Input() resourceSubmitData: ResourceSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ResourceFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public resourceForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        officeId: [null],
        resourceTypeId: [null, Validators.required],
        shiftPatternId: [null],
        timeZoneId: [null, Validators.required],
        targetWeeklyWorkHours: [''],
        notes: [''],
        externalId: [''],
        color: [''],
        attributes: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  resources$ = this.resourceService.GetResourceList();
  offices$ = this.officeService.GetOfficeList();
  resourceTypes$ = this.resourceTypeService.GetResourceTypeList();
  shiftPatterns$ = this.shiftPatternService.GetShiftPatternList();
  timeZones$ = this.timeZoneService.GetTimeZoneList();

  constructor(
    private modalService: NgbModal,
    private resourceService: ResourceService,
    private officeService: OfficeService,
    private resourceTypeService: ResourceTypeService,
    private shiftPatternService: ShiftPatternService,
    private timeZoneService: TimeZoneService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(resourceData?: ResourceData) {

    if (resourceData != null) {

      if (!this.resourceService.userIsSchedulerResourceReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Resources`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.resourceSubmitData = this.resourceService.ConvertToResourceSubmitData(resourceData);
      this.isEditMode = true;
      this.objectGuid = resourceData.objectGuid;

      this.buildFormValues(resourceData);

    } else {

      if (!this.resourceService.userIsSchedulerResourceWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Resources`,
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
        this.resourceForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.resourceForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.resourceModal, {
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

    if (this.resourceService.userIsSchedulerResourceWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Resources`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.resourceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.resourceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.resourceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const resourceSubmitData: ResourceSubmitData = {
        id: this.resourceSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        resourceTypeId: Number(formValue.resourceTypeId),
        shiftPatternId: formValue.shiftPatternId ? Number(formValue.shiftPatternId) : null,
        timeZoneId: Number(formValue.timeZoneId),
        targetWeeklyWorkHours: formValue.targetWeeklyWorkHours ? Number(formValue.targetWeeklyWorkHours) : null,
        notes: formValue.notes?.trim() || null,
        externalId: formValue.externalId?.trim() || null,
        color: formValue.color?.trim() || null,
        attributes: formValue.attributes?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.resourceSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateResource(resourceSubmitData);
      } else {
        this.addResource(resourceSubmitData);
      }
  }

  private addResource(resourceData: ResourceSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    resourceData.versionNumber = 0;
    resourceData.active = true;
    resourceData.deleted = false;
    this.resourceService.PostResource(resourceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newResource) => {

        this.resourceService.ClearAllCaches();

        this.resourceChanged.next([newResource]);

        this.alertService.showMessage("Resource added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/resource', newResource.id]);
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
                                   'You do not have permission to save this Resource.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateResource(resourceData: ResourceSubmitData) {
    this.resourceService.PutResource(resourceData.id, resourceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedResource) => {

        this.resourceService.ClearAllCaches();

        this.resourceChanged.next([updatedResource]);

        this.alertService.showMessage("Resource updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Resource.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Resource.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Resource could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(resourceData: ResourceData | null) {

    if (resourceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.resourceForm.reset({
        name: '',
        description: '',
        officeId: null,
        resourceTypeId: null,
        shiftPatternId: null,
        timeZoneId: null,
        targetWeeklyWorkHours: '',
        notes: '',
        externalId: '',
        color: '',
        attributes: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.resourceForm.reset({
        name: resourceData.name ?? '',
        description: resourceData.description ?? '',
        officeId: resourceData.officeId,
        resourceTypeId: resourceData.resourceTypeId,
        shiftPatternId: resourceData.shiftPatternId,
        timeZoneId: resourceData.timeZoneId,
        targetWeeklyWorkHours: resourceData.targetWeeklyWorkHours?.toString() ?? '',
        notes: resourceData.notes ?? '',
        externalId: resourceData.externalId ?? '',
        color: resourceData.color ?? '',
        attributes: resourceData.attributes ?? '',
        avatarFileName: resourceData.avatarFileName ?? '',
        avatarSize: resourceData.avatarSize?.toString() ?? '',
        avatarData: resourceData.avatarData ?? '',
        avatarMimeType: resourceData.avatarMimeType ?? '',
        versionNumber: resourceData.versionNumber?.toString() ?? '',
        active: resourceData.active ?? true,
        deleted: resourceData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.resourceForm.markAsPristine();
    this.resourceForm.markAsUntouched();
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


  public userIsSchedulerResourceReader(): boolean {
    return this.resourceService.userIsSchedulerResourceReader();
  }

  public userIsSchedulerResourceWriter(): boolean {
    return this.resourceService.userIsSchedulerResourceWriter();
  }
}
