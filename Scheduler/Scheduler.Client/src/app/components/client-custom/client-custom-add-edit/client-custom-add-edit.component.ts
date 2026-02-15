import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ClientService, ClientData, ClientSubmitData } from '../../../scheduler-data-services/client.service';
import { ClientTypeService } from '../../../scheduler-data-services/client-type.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { CalendarService } from '../../../scheduler-data-services/calendar.service';
import { StateProvinceService } from '../../../scheduler-data-services/state-province.service';
import { CountryService } from '../../../scheduler-data-services/country.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-client-custom-add-edit',
  templateUrl: './client-custom-add-edit.component.html',
  styleUrls: ['./client-custom-add-edit.component.scss'],
  animations: [
    trigger('collapse', [
      state('false', style({ height: '0', overflow: 'hidden', opacity: 0 })),
      state('true', style({ height: '*', opacity: 1 })),
      transition('false <=> true', animate('300ms ease-in-out'))
    ])
  ]
})
export class ClientCustomAddEditComponent {
  @ViewChild('clientModal') clientModal!: TemplateRef<any>;
  @Output() clientChanged = new Subject<ClientData[]>();
  @Input() clientSubmitData: ClientSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  public attributesParsed: any = {};

  public currentAvatarUrl: string | null = null;
  public isAvatarPanelOpen = false;
  public isDragOver = false;

  /** Tracked latitude from the map component. */
  public mapLatitude: number | null = null;

  /** Tracked longitude from the map component. */
  public mapLongitude: number | null = null;

  /** Handler for the map component's coordinatesChanged event. */
  onCoordinatesChanged(coords: { latitude: number; longitude: number }) {
    this.mapLatitude = coords.latitude;
    this.mapLongitude = coords.longitude;
    this.clientForm.markAsDirty();
  }

  onDynamicAttributeChange(data: any) {
    this.attributesParsed = data;
    this.clientForm.markAsDirty();
  }

  clientForm: FormGroup = this.fb.group({
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
    this.isAvatarPanelOpen = false;
    this.modalIsDisplayed = false;
  }


  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (!files || files.length === 0) return;

    const file = files[0];
    if (!file.type.startsWith('image/')) {
      this.alertService.showMessage('Invalid file type', 'Please drop an image file', MessageSeverity.warn);
      return;
    }

    // Reuse existing logic — simulate file input change
    const fakeEvent = { target: { files: [file] } } as any;
    this.onAvatarSelected(fakeEvent);
  }


  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];

    // Enforce 2MB limit
    if (file.size > 2 * 1024 * 1024) {
      this.alertService.showMessage(
        'Image too large',
        'Please select an image under 2MB',
        MessageSeverity.warn
      );
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const result = e.target?.result as string;

      if (!result) return;

      // Extract only the base64 part (remove data:image/png;base64, prefix)
      const base64Data = result.split(',')[1];

      if (!base64Data) {
        this.alertService.showMessage('Invalid image data', '', MessageSeverity.error);
        return;
      }

      this.currentAvatarUrl = result; // Full data URL for preview (includes prefix)

      // Populate form fields
      this.clientForm.patchValue({
        avatarFileName: file.name,
        avatarSize: file.size,
        avatarData: base64Data,         // ← Only the raw base64 string
        avatarMimeType: file.type
      });

      this.clientForm.markAsDirty();
    };

    reader.onerror = () => {
      this.alertService.showMessage('Failed to read file', '', MessageSeverity.error);
    };

    reader.readAsDataURL(file);
  }

  clearAvatar(): void {
    this.currentAvatarUrl = null;
    this.clientForm.patchValue({
      avatarFileName: null,
      avatarSize: null,
      avatarData: null,
      avatarMimeType: null
    });
    this.clientForm.markAsDirty();
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
      latitude: this.mapLatitude,
      longitude: this.mapLongitude,
      notes: formValue.notes?.trim() || null,
      externalId: formValue.externalId?.trim() || null,
      color: formValue.color?.trim() || null,
      attributes: Object.keys(this.attributesParsed).length > 0 ? JSON.stringify(this.attributesParsed) : null,
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
        else if (err.status && err.error) {
          if (err.status === 403) {
            errorMessage = err.error?.message ||
              'You do not have permission to save this Client.';
          }
          else {
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
        else if (err.status && err.error) {
          if (err.status === 403) {
            errorMessage = err.error?.message ||
              'You do not have permission to save this Client.';
          }
          else {
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

      this.attributesParsed = {};
      this.mapLatitude = null;
      this.mapLongitude = null;
      this.currentAvatarUrl = null;

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
      }, { emitEvent: false });

    }
    else {

      try {
        this.attributesParsed = clientData.attributes ? JSON.parse(clientData.attributes) : {};
      } catch (e) {
        this.attributesParsed = {};
      }

      this.mapLatitude = clientData.latitude ?? null;
      this.mapLongitude = clientData.longitude ?? null;

      // Reconstruct full data URL for preview if we have base64 data
      if (clientData.avatarData && clientData.avatarMimeType) {
        this.currentAvatarUrl = `data:${clientData.avatarMimeType};base64,${clientData.avatarData}`;
      } else {
        this.currentAvatarUrl = null;
      }

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
      }, { emitEvent: false });
    }

    this.clientForm.markAsPristine();
    this.clientForm.markAsUntouched();
  }

  public userIsSchedulerClientReader(): boolean {
    return this.clientService.userIsSchedulerClientReader();
  }

  public userIsSchedulerClientWriter(): boolean {
    return this.clientService.userIsSchedulerClientWriter();
  }

  public userIsFoundationAdministrator(): boolean {
    return this.authService.isFoundationAdmin;
  }
}
