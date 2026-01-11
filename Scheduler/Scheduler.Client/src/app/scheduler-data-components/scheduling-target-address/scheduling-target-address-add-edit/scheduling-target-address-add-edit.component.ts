/*
   GENERATED FORM FOR THE SCHEDULINGTARGETADDRESS TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SchedulingTargetAddress table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to scheduling-target-address-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SchedulingTargetAddressService, SchedulingTargetAddressData, SchedulingTargetAddressSubmitData } from '../../../scheduler-data-services/scheduling-target-address.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { StateProvinceService } from '../../../scheduler-data-services/state-province.service';
import { CountryService } from '../../../scheduler-data-services/country.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SchedulingTargetAddressFormValues {
  schedulingTargetId: number | bigint,       // For FK link number
  clientId: number | bigint | null,       // For FK link number
  addressLine1: string,
  addressLine2: string | null,
  city: string,
  postalCode: string | null,
  stateProvinceId: number | bigint,       // For FK link number
  countryId: number | bigint,       // For FK link number
  latitude: string | null,     // Stored as string for form input, converted to number on submit.
  longitude: string | null,     // Stored as string for form input, converted to number on submit.
  label: string | null,
  isPrimary: boolean,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-scheduling-target-address-add-edit',
  templateUrl: './scheduling-target-address-add-edit.component.html',
  styleUrls: ['./scheduling-target-address-add-edit.component.scss']
})
export class SchedulingTargetAddressAddEditComponent {
  @ViewChild('schedulingTargetAddressModal') schedulingTargetAddressModal!: TemplateRef<any>;
  @Output() schedulingTargetAddressChanged = new Subject<SchedulingTargetAddressData[]>();
  @Input() schedulingTargetAddressSubmitData: SchedulingTargetAddressSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SchedulingTargetAddressFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public schedulingTargetAddressForm: FormGroup = this.fb.group({
        schedulingTargetId: [null, Validators.required],
        clientId: [null],
        addressLine1: ['', Validators.required],
        addressLine2: [''],
        city: ['', Validators.required],
        postalCode: [''],
        stateProvinceId: [null, Validators.required],
        countryId: [null, Validators.required],
        latitude: [''],
        longitude: [''],
        label: [''],
        isPrimary: [false],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  schedulingTargetAddresses$ = this.schedulingTargetAddressService.GetSchedulingTargetAddressList();
  schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  clients$ = this.clientService.GetClientList();
  stateProvinces$ = this.stateProvinceService.GetStateProvinceList();
  countries$ = this.countryService.GetCountryList();

  constructor(
    private modalService: NgbModal,
    private schedulingTargetAddressService: SchedulingTargetAddressService,
    private schedulingTargetService: SchedulingTargetService,
    private clientService: ClientService,
    private stateProvinceService: StateProvinceService,
    private countryService: CountryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(schedulingTargetAddressData?: SchedulingTargetAddressData) {

    if (schedulingTargetAddressData != null) {

      if (!this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Scheduling Target Addresses`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.schedulingTargetAddressSubmitData = this.schedulingTargetAddressService.ConvertToSchedulingTargetAddressSubmitData(schedulingTargetAddressData);
      this.isEditMode = true;
      this.objectGuid = schedulingTargetAddressData.objectGuid;

      this.buildFormValues(schedulingTargetAddressData);

    } else {

      if (!this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Scheduling Target Addresses`,
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
        this.schedulingTargetAddressForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.schedulingTargetAddressForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.schedulingTargetAddressModal, {
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

    if (this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Scheduling Target Addresses`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.schedulingTargetAddressForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.schedulingTargetAddressForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.schedulingTargetAddressForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const schedulingTargetAddressSubmitData: SchedulingTargetAddressSubmitData = {
        id: this.schedulingTargetAddressSubmitData?.id || 0,
        schedulingTargetId: Number(formValue.schedulingTargetId),
        clientId: formValue.clientId ? Number(formValue.clientId) : null,
        addressLine1: formValue.addressLine1!.trim(),
        addressLine2: formValue.addressLine2?.trim() || null,
        city: formValue.city!.trim(),
        postalCode: formValue.postalCode?.trim() || null,
        stateProvinceId: Number(formValue.stateProvinceId),
        countryId: Number(formValue.countryId),
        latitude: formValue.latitude ? Number(formValue.latitude) : null,
        longitude: formValue.longitude ? Number(formValue.longitude) : null,
        label: formValue.label?.trim() || null,
        isPrimary: !!formValue.isPrimary,
        versionNumber: this.schedulingTargetAddressSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSchedulingTargetAddress(schedulingTargetAddressSubmitData);
      } else {
        this.addSchedulingTargetAddress(schedulingTargetAddressSubmitData);
      }
  }

  private addSchedulingTargetAddress(schedulingTargetAddressData: SchedulingTargetAddressSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    schedulingTargetAddressData.versionNumber = 0;
    schedulingTargetAddressData.active = true;
    schedulingTargetAddressData.deleted = false;
    this.schedulingTargetAddressService.PostSchedulingTargetAddress(schedulingTargetAddressData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSchedulingTargetAddress) => {

        this.schedulingTargetAddressService.ClearAllCaches();

        this.schedulingTargetAddressChanged.next([newSchedulingTargetAddress]);

        this.alertService.showMessage("Scheduling Target Address added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/schedulingtargetaddress', newSchedulingTargetAddress.id]);
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
                                   'You do not have permission to save this Scheduling Target Address.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Address.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Address could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSchedulingTargetAddress(schedulingTargetAddressData: SchedulingTargetAddressSubmitData) {
    this.schedulingTargetAddressService.PutSchedulingTargetAddress(schedulingTargetAddressData.id, schedulingTargetAddressData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSchedulingTargetAddress) => {

        this.schedulingTargetAddressService.ClearAllCaches();

        this.schedulingTargetAddressChanged.next([updatedSchedulingTargetAddress]);

        this.alertService.showMessage("Scheduling Target Address updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Scheduling Target Address.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Scheduling Target Address.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Scheduling Target Address could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(schedulingTargetAddressData: SchedulingTargetAddressData | null) {

    if (schedulingTargetAddressData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.schedulingTargetAddressForm.reset({
        schedulingTargetId: null,
        clientId: null,
        addressLine1: '',
        addressLine2: '',
        city: '',
        postalCode: '',
        stateProvinceId: null,
        countryId: null,
        latitude: '',
        longitude: '',
        label: '',
        isPrimary: false,
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.schedulingTargetAddressForm.reset({
        schedulingTargetId: schedulingTargetAddressData.schedulingTargetId,
        clientId: schedulingTargetAddressData.clientId,
        addressLine1: schedulingTargetAddressData.addressLine1 ?? '',
        addressLine2: schedulingTargetAddressData.addressLine2 ?? '',
        city: schedulingTargetAddressData.city ?? '',
        postalCode: schedulingTargetAddressData.postalCode ?? '',
        stateProvinceId: schedulingTargetAddressData.stateProvinceId,
        countryId: schedulingTargetAddressData.countryId,
        latitude: schedulingTargetAddressData.latitude?.toString() ?? '',
        longitude: schedulingTargetAddressData.longitude?.toString() ?? '',
        label: schedulingTargetAddressData.label ?? '',
        isPrimary: schedulingTargetAddressData.isPrimary ?? false,
        versionNumber: schedulingTargetAddressData.versionNumber?.toString() ?? '',
        active: schedulingTargetAddressData.active ?? true,
        deleted: schedulingTargetAddressData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.schedulingTargetAddressForm.markAsPristine();
    this.schedulingTargetAddressForm.markAsUntouched();
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


  public userIsSchedulerSchedulingTargetAddressReader(): boolean {
    return this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressReader();
  }

  public userIsSchedulerSchedulingTargetAddressWriter(): boolean {
    return this.schedulingTargetAddressService.userIsSchedulerSchedulingTargetAddressWriter();
  }
}
