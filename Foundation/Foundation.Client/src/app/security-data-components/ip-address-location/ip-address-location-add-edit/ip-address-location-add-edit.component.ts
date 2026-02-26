/*
   GENERATED FORM FOR THE IPADDRESSLOCATION TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from IpAddressLocation table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to ip-address-location-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IpAddressLocationService, IpAddressLocationData, IpAddressLocationSubmitData } from '../../../security-data-services/ip-address-location.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface IpAddressLocationFormValues {
  ipAddress: string,
  countryCode: string | null,
  countryName: string | null,
  city: string | null,
  latitude: string | null,     // Stored as string for form input, converted to number on submit.
  longitude: string | null,     // Stored as string for form input, converted to number on submit.
  lastLookupDate: string,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-ip-address-location-add-edit',
  templateUrl: './ip-address-location-add-edit.component.html',
  styleUrls: ['./ip-address-location-add-edit.component.scss']
})
export class IpAddressLocationAddEditComponent {
  @ViewChild('ipAddressLocationModal') ipAddressLocationModal!: TemplateRef<any>;
  @Output() ipAddressLocationChanged = new Subject<IpAddressLocationData[]>();
  @Input() ipAddressLocationSubmitData: IpAddressLocationSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IpAddressLocationFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public ipAddressLocationForm: FormGroup = this.fb.group({
        ipAddress: ['', Validators.required],
        countryCode: [''],
        countryName: [''],
        city: [''],
        latitude: [''],
        longitude: [''],
        lastLookupDate: ['', Validators.required],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  ipAddressLocations$ = this.ipAddressLocationService.GetIpAddressLocationList();

  constructor(
    private modalService: NgbModal,
    private ipAddressLocationService: IpAddressLocationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(ipAddressLocationData?: IpAddressLocationData) {

    if (ipAddressLocationData != null) {

      if (!this.ipAddressLocationService.userIsSecurityIpAddressLocationReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Ip Address Locations`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.ipAddressLocationSubmitData = this.ipAddressLocationService.ConvertToIpAddressLocationSubmitData(ipAddressLocationData);
      this.isEditMode = true;

      this.buildFormValues(ipAddressLocationData);

    } else {

      if (!this.ipAddressLocationService.userIsSecurityIpAddressLocationWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Ip Address Locations`,
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
        this.ipAddressLocationForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.ipAddressLocationForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.ipAddressLocationModal, {
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

    if (this.ipAddressLocationService.userIsSecurityIpAddressLocationWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Ip Address Locations`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.ipAddressLocationForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.ipAddressLocationForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.ipAddressLocationForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const ipAddressLocationSubmitData: IpAddressLocationSubmitData = {
        id: this.ipAddressLocationSubmitData?.id || 0,
        ipAddress: formValue.ipAddress!.trim(),
        countryCode: formValue.countryCode?.trim() || null,
        countryName: formValue.countryName?.trim() || null,
        city: formValue.city?.trim() || null,
        latitude: formValue.latitude ? Number(formValue.latitude) : null,
        longitude: formValue.longitude ? Number(formValue.longitude) : null,
        lastLookupDate: dateTimeLocalToIsoUtc(formValue.lastLookupDate!.trim())!,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateIpAddressLocation(ipAddressLocationSubmitData);
      } else {
        this.addIpAddressLocation(ipAddressLocationSubmitData);
      }
  }

  private addIpAddressLocation(ipAddressLocationData: IpAddressLocationSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    ipAddressLocationData.active = true;
    ipAddressLocationData.deleted = false;
    this.ipAddressLocationService.PostIpAddressLocation(ipAddressLocationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newIpAddressLocation) => {

        this.ipAddressLocationService.ClearAllCaches();

        this.ipAddressLocationChanged.next([newIpAddressLocation]);

        this.alertService.showMessage("Ip Address Location added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/ipaddresslocation', newIpAddressLocation.id]);
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
                                   'You do not have permission to save this Ip Address Location.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Ip Address Location.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Ip Address Location could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateIpAddressLocation(ipAddressLocationData: IpAddressLocationSubmitData) {
    this.ipAddressLocationService.PutIpAddressLocation(ipAddressLocationData.id, ipAddressLocationData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedIpAddressLocation) => {

        this.ipAddressLocationService.ClearAllCaches();

        this.ipAddressLocationChanged.next([updatedIpAddressLocation]);

        this.alertService.showMessage("Ip Address Location updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Ip Address Location.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Ip Address Location.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Ip Address Location could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(ipAddressLocationData: IpAddressLocationData | null) {

    if (ipAddressLocationData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.ipAddressLocationForm.reset({
        ipAddress: '',
        countryCode: '',
        countryName: '',
        city: '',
        latitude: '',
        longitude: '',
        lastLookupDate: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.ipAddressLocationForm.reset({
        ipAddress: ipAddressLocationData.ipAddress ?? '',
        countryCode: ipAddressLocationData.countryCode ?? '',
        countryName: ipAddressLocationData.countryName ?? '',
        city: ipAddressLocationData.city ?? '',
        latitude: ipAddressLocationData.latitude?.toString() ?? '',
        longitude: ipAddressLocationData.longitude?.toString() ?? '',
        lastLookupDate: isoUtcStringToDateTimeLocal(ipAddressLocationData.lastLookupDate) ?? '',
        active: ipAddressLocationData.active ?? true,
        deleted: ipAddressLocationData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.ipAddressLocationForm.markAsPristine();
    this.ipAddressLocationForm.markAsUntouched();
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


  public userIsSecurityIpAddressLocationReader(): boolean {
    return this.ipAddressLocationService.userIsSecurityIpAddressLocationReader();
  }

  public userIsSecurityIpAddressLocationWriter(): boolean {
    return this.ipAddressLocationService.userIsSecurityIpAddressLocationWriter();
  }
}
