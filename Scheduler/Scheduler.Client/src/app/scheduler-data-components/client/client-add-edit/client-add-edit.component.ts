/*
   GENERATED FORM FOR THE CLIENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Client table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to client-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ClientService, ClientData, ClientSubmitData } from '../../../scheduler-data-services/client.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ClientTypeService } from '../../../scheduler-data-services/client-type.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { CalendarService } from '../../../scheduler-data-services/calendar.service';
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
interface ClientFormValues {
  name: string,
  description: string | null,
  clientTypeId: number | bigint,       // For FK link number
  currencyId: number | bigint,       // For FK link number
  timeZoneId: number | bigint,       // For FK link number
  calendarId: number | bigint | null,       // For FK link number
  addressLine1: string,
  addressLine2: string | null,
  city: string,
  postalCode: string | null,
  stateProvinceId: number | bigint,       // For FK link number
  countryId: number | bigint,       // For FK link number
  phone: string | null,
  email: string | null,
  latitude: string | null,     // Stored as string for form input, converted to number on submit.
  longitude: string | null,     // Stored as string for form input, converted to number on submit.
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
  selector: 'app-client-add-edit',
  templateUrl: './client-add-edit.component.html',
  styleUrls: ['./client-add-edit.component.scss']
})
export class ClientAddEditComponent {
  @ViewChild('clientModal') clientModal!: TemplateRef<any>;
  @Output() clientChanged = new Subject<ClientData[]>();
  @Input() clientSubmitData: ClientSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ClientFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public clientForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        clientTypeId: [null, Validators.required],
        currencyId: [null, Validators.required],
        timeZoneId: [null, Validators.required],
        calendarId: [null],
        addressLine1: ['', Validators.required],
        addressLine2: [''],
        city: ['', Validators.required],
        postalCode: [''],
        stateProvinceId: [null, Validators.required],
        countryId: [null, Validators.required],
        phone: [''],
        email: [''],
        latitude: [''],
        longitude: [''],
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

  clients$ = this.clientService.GetClientList();
  clientTypes$ = this.clientTypeService.GetClientTypeList();
  currencies$ = this.currencyService.GetCurrencyList();
  timeZones$ = this.timeZoneService.GetTimeZoneList();
  calendars$ = this.calendarService.GetCalendarList();
  stateProvinces$ = this.stateProvinceService.GetStateProvinceList();
  countries$ = this.countryService.GetCountryList();

  constructor(
    private modalService: NgbModal,
    private clientService: ClientService,
    private clientTypeService: ClientTypeService,
    private currencyService: CurrencyService,
    private timeZoneService: TimeZoneService,
    private calendarService: CalendarService,
    private stateProvinceService: StateProvinceService,
    private countryService: CountryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(clientData?: ClientData) {

    if (clientData != null) {

      if (!this.clientService.userIsSchedulerClientReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Clients`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.clientSubmitData = this.clientService.ConvertToClientSubmitData(clientData);
      this.isEditMode = true;
      this.objectGuid = clientData.objectGuid;

      this.buildFormValues(clientData);

    } else {

      if (!this.clientService.userIsSchedulerClientWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Clients`,
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
        this.clientForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.clientForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.clientModal, {
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

    if (this.clientService.userIsSchedulerClientWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Clients`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.clientForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.clientForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.clientForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const clientSubmitData: ClientSubmitData = {
        id: this.clientSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        clientTypeId: Number(formValue.clientTypeId),
        currencyId: Number(formValue.currencyId),
        timeZoneId: Number(formValue.timeZoneId),
        calendarId: formValue.calendarId ? Number(formValue.calendarId) : null,
        addressLine1: formValue.addressLine1!.trim(),
        addressLine2: formValue.addressLine2?.trim() || null,
        city: formValue.city!.trim(),
        postalCode: formValue.postalCode?.trim() || null,
        stateProvinceId: Number(formValue.stateProvinceId),
        countryId: Number(formValue.countryId),
        phone: formValue.phone?.trim() || null,
        email: formValue.email?.trim() || null,
        latitude: formValue.latitude ? Number(formValue.latitude) : null,
        longitude: formValue.longitude ? Number(formValue.longitude) : null,
        notes: formValue.notes?.trim() || null,
        externalId: formValue.externalId?.trim() || null,
        color: formValue.color?.trim() || null,
        attributes: formValue.attributes?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.clientSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateClient(clientSubmitData);
      } else {
        this.addClient(clientSubmitData);
      }
  }

  private addClient(clientData: ClientSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    clientData.versionNumber = 0;
    clientData.active = true;
    clientData.deleted = false;
    this.clientService.PostClient(clientData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newClient) => {

        this.clientService.ClearAllCaches();

        this.clientChanged.next([newClient]);

        this.alertService.showMessage("Client added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/client', newClient.id]);
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
                                   'You do not have permission to save this Client.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Client.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Client could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateClient(clientData: ClientSubmitData) {
    this.clientService.PutClient(clientData.id, clientData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedClient) => {

        this.clientService.ClearAllCaches();

        this.clientChanged.next([updatedClient]);

        this.alertService.showMessage("Client updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Client.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Client.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Client could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(clientData: ClientData | null) {

    if (clientData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.clientForm.reset({
        name: '',
        description: '',
        clientTypeId: null,
        currencyId: null,
        timeZoneId: null,
        calendarId: null,
        addressLine1: '',
        addressLine2: '',
        city: '',
        postalCode: '',
        stateProvinceId: null,
        countryId: null,
        phone: '',
        email: '',
        latitude: '',
        longitude: '',
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
        this.clientForm.reset({
        name: clientData.name ?? '',
        description: clientData.description ?? '',
        clientTypeId: clientData.clientTypeId,
        currencyId: clientData.currencyId,
        timeZoneId: clientData.timeZoneId,
        calendarId: clientData.calendarId,
        addressLine1: clientData.addressLine1 ?? '',
        addressLine2: clientData.addressLine2 ?? '',
        city: clientData.city ?? '',
        postalCode: clientData.postalCode ?? '',
        stateProvinceId: clientData.stateProvinceId,
        countryId: clientData.countryId,
        phone: clientData.phone ?? '',
        email: clientData.email ?? '',
        latitude: clientData.latitude?.toString() ?? '',
        longitude: clientData.longitude?.toString() ?? '',
        notes: clientData.notes ?? '',
        externalId: clientData.externalId ?? '',
        color: clientData.color ?? '',
        attributes: clientData.attributes ?? '',
        avatarFileName: clientData.avatarFileName ?? '',
        avatarSize: clientData.avatarSize?.toString() ?? '',
        avatarData: clientData.avatarData ?? '',
        avatarMimeType: clientData.avatarMimeType ?? '',
        versionNumber: clientData.versionNumber?.toString() ?? '',
        active: clientData.active ?? true,
        deleted: clientData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.clientForm.markAsPristine();
    this.clientForm.markAsUntouched();
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


  public userIsSchedulerClientReader(): boolean {
    return this.clientService.userIsSchedulerClientReader();
  }

  public userIsSchedulerClientWriter(): boolean {
    return this.clientService.userIsSchedulerClientWriter();
  }
}
