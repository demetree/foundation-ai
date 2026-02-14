import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OfficeService, OfficeData, OfficeSubmitData } from '../../../scheduler-data-services/office.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeTypeService } from '../../../scheduler-data-services/office-type.service';
import { TimeZoneService } from '../../../scheduler-data-services/time-zone.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { StateProvinceService } from '../../../scheduler-data-services/state-province.service';
import { CountryService } from '../../../scheduler-data-services/country.service';
import { AuthService } from '../../../services/auth.service';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';
import { CurrentUserService } from '../../../services/current-user.service';

@Component({
  selector: 'app-office-custom-add-edit',
  templateUrl: './office-custom-add-edit.component.html',
  styleUrls: ['./office-custom-add-edit.component.scss'],
  animations: [
    trigger('collapse', [
      state('false', style({ height: '0', overflow: 'hidden', opacity: 0 })),
      state('true', style({ height: '*', opacity: 1 })),
      transition('false <=> true', animate('300ms ease-in-out'))
    ])
  ]
})
export class OfficeCustomAddEditComponent {
  @ViewChild('officeModal') officeModal!: TemplateRef<any>;
  @Output() officeChanged = new Subject<OfficeData[]>();
  @Input() officeSubmitData: OfficeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  public officeData: OfficeData | null = null;

  public currentAvatarUrl: string | null = null;
  public isAvatarPanelOpen = false;
  public isDragOver = false;

  public attributesParsed: any = {};

  /** Tracked latitude from the map component. */
  public mapLatitude: number | null = null;

  /** Tracked longitude from the map component. */
  public mapLongitude: number | null = null;

  /** Handler for the map component's coordinatesChanged event. */
  onCoordinatesChanged(coords: { latitude: number; longitude: number }) {
    this.mapLatitude = coords.latitude;
    this.mapLongitude = coords.longitude;
    this.officeForm.markAsDirty();
  }

  onDynamicAttributeChange(data: any) {
    this.attributesParsed = data;
    this.officeForm.markAsDirty();
  }

  officeForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    officeTypeId: [null, Validators.required],
    timeZoneId: [this.currentUserService.defaultTimeZoneId, Validators.required],
    currencyId: [this.currentUserService.defaultCurrencyId, Validators.required],
    addressLine1: ['', Validators.required],
    addressLine2: [''],
    city: ['', Validators.required],
    postalCode: [''],
    stateProvinceId: [this.currentUserService.defaultStateProvinceId, Validators.required],
    countryId: [this.currentUserService.defaultCountryId, Validators.required],
    phone: [''],
    email: [''],
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

  offices$ = this.officeService.GetOfficeList();
  officeTypes$ = this.officeTypeService.GetOfficeTypeList();
  timeZones$ = this.timeZoneService.GetTimeZoneList();
  currencies$ = this.currencyService.GetCurrencyList();
  stateProvinces$ = this.stateProvinceService.GetStateProvinceList();
  countries$ = this.countryService.GetCountryList();

  constructor(
    private modalService: NgbModal,
    private officeService: OfficeService,
    private officeTypeService: OfficeTypeService,
    private timeZoneService: TimeZoneService,
    private currencyService: CurrencyService,
    private stateProvinceService: StateProvinceService,
    private countryService: CountryService,
    private authService: AuthService,
    private alertService: AlertService,
    private schedulerHelperService: SchedulerHelperService,
    private currentUserService: CurrentUserService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(officeData?: OfficeData) {

    if (officeData != null) {

      if (!this.officeService.userIsSchedulerOfficeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Offices`,
          '',
          MessageSeverity.info
        );
        return;
      }

      this.officeData = officeData;
      this.officeSubmitData = this.officeService.ConvertToOfficeSubmitData(officeData);
      this.isEditMode = true;
      this.objectGuid = officeData.objectGuid;

      this.buildFormValues(officeData);

    } else {

      if (!this.officeService.userIsSchedulerOfficeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Offices`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.officeData = null;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.officeModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;
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
      this.officeForm.patchValue({
        avatarFileName: file.name,
        avatarSize: file.size,
        avatarData: base64Data,         // ← Only the raw base64 string
        avatarMimeType: file.type
      });

      this.officeForm.markAsDirty();
    };

    reader.onerror = () => {
      this.alertService.showMessage('Failed to read file', '', MessageSeverity.error);
    };

    reader.readAsDataURL(file);
  }

  clearAvatar(): void {
    this.currentAvatarUrl = null;
    this.officeForm.patchValue({
      avatarFileName: null,
      avatarSize: null,
      avatarData: null,
      avatarMimeType: null
    });
    this.officeForm.markAsDirty();
  }


  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.isAvatarPanelOpen = false;
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.officeService.userIsSchedulerOfficeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Offices`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.officeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.officeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.officeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    // Note not falling back to user default values on null here becase we want them to be able to create records with null linksif they set the form that way
    //
    const officeSubmitData: OfficeSubmitData = {
      id: this.officeSubmitData?.id || 0,
      name: formValue.name!.trim(),
      description: formValue.description?.trim() || null,
      officeTypeId: Number(formValue.officeTypeId),
      timeZoneId: Number(formValue.timeZoneId),
      currencyId: Number(formValue.currencyId),
      addressLine1: formValue.addressLine1!.trim(),
      addressLine2: formValue.addressLine2?.trim() || null,
      city: formValue.city!.trim(),
      postalCode: formValue.postalCode?.trim() || null,
      stateProvinceId: Number(formValue.stateProvinceId),
      countryId: Number(formValue.countryId),
      phone: formValue.phone?.trim() || null,
      email: formValue.email?.trim() || null,
      notes: formValue.notes?.trim() || null,
      externalId: formValue.externalId?.trim() || null,
      color: formValue.color?.trim() || null,
      latitude: this.mapLatitude,
      longitude: this.mapLongitude,
      attributes: Object.keys(this.attributesParsed).length > 0 ? JSON.stringify(this.attributesParsed) : null,
      avatarFileName: formValue.avatarFileName?.trim() || null,
      avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
      avatarData: formValue.avatarData?.trim() || null,
      avatarMimeType: formValue.avatarMimeType?.trim() || null,
      versionNumber: this.officeSubmitData?.versionNumber ?? 0,
      active: !!formValue.active,
      deleted: !!formValue.deleted,
    };

    if (this.isEditMode) {
      this.updateOffice(officeSubmitData);
    } else {
      this.addOffice(officeSubmitData);
    }
  }

  private addOffice(officeData: OfficeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    officeData.versionNumber = 0;
    officeData.active = true;
    officeData.deleted = false;
    this.officeService.PostOffice(officeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newOffice) => {

        // Trigger a reload on the scheduler helper to update the office counts observable value
        this.schedulerHelperService.Reload();

        this.officeService.ClearAllCaches();

        this.officeChanged.next([newOffice]);

        this.alertService.showMessage("Office added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/office', newOffice.id]);
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
              'You do not have permission to save this Office.';
          }
          else {
            errorMessage = err.error?.message ||
              err.error?.error_description ||
              err.error?.detail ||
              'An error occurred while saving the Office.';
          }
        }
        // Fallback for unexpected error formats
        else {
          errorMessage = 'An unexpected error occurred.';
        }

        this.alertService.showMessage('Office could not be saved',
          errorMessage,
          MessageSeverity.error);
      }
    });
  }


  private updateOffice(officeData: OfficeSubmitData) {
    this.officeService.PutOffice(officeData.id, officeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedOffice) => {

        // Trigger a reload on the scheduler helper to update the office counts observable value
        this.schedulerHelperService.Reload();

        this.officeService.ClearAllCaches();

        this.officeChanged.next([updatedOffice]);

        this.alertService.showMessage("Office updated successfully", '', MessageSeverity.success);

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
              'You do not have permission to save this Office.';
          }
          else {
            errorMessage = err.error?.message ||
              err.error?.error_description ||
              err.error?.detail ||
              'An error occurred while saving the Office.';
          }
        }
        // Fallback for unexpected error formats
        else {
          errorMessage = 'An unexpected error occurred.';
        }

        this.alertService.showMessage('Office could not be saved',
          errorMessage,
          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(officeData: OfficeData | null) {

    if (officeData == null) {

      this.attributesParsed = {};
      this.currentAvatarUrl = null;
      this.mapLatitude = null;
      this.mapLongitude = null;

      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.officeForm.reset({
        name: '',
        description: '',
        officeTypeId: null,
        timeZoneId: this.currentUserService.defaultTimeZoneId,
        currencyId: this.currentUserService.defaultCurrencyId,
        addressLine1: '',
        addressLine2: '',
        city: '',
        postalCode: '',
        stateProvinceId: this.currentUserService.defaultStateProvinceId,
        countryId: this.currentUserService.defaultCountryId,
        phone: '',
        email: '',
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
        this.attributesParsed = officeData.attributes ? JSON.parse(officeData.attributes) : {};
      } catch (e) {
        this.attributesParsed = {};
      }

      // Reconstruct full data URL for preview if we have base64 data
      if (officeData.avatarData && officeData.avatarMimeType) {
        this.currentAvatarUrl = `data:${officeData.avatarMimeType};base64,${officeData.avatarData}`;
      } else {
        this.currentAvatarUrl = null;
      }

      this.mapLatitude = officeData.latitude ?? null;
      this.mapLongitude = officeData.longitude ?? null;
      //
      // Reset the form with properly formatted values that support dates in datetime-local inputs
      //
      this.officeForm.reset({
        name: officeData.name ?? '',
        description: officeData.description ?? '',
        officeTypeId: officeData.officeTypeId,
        timeZoneId: officeData.timeZoneId,
        currencyId: officeData.currencyId,
        addressLine1: officeData.addressLine1 ?? '',
        addressLine2: officeData.addressLine2 ?? '',
        city: officeData.city ?? '',
        postalCode: officeData.postalCode ?? '',
        stateProvinceId: officeData.stateProvinceId,
        countryId: officeData.countryId,
        phone: officeData.phone ?? '',
        email: officeData.email ?? '',
        notes: officeData.notes ?? '',
        externalId: officeData.externalId ?? '',
        color: officeData.color ?? '',
        attributes: officeData.attributes ?? '',
        avatarFileName: officeData.avatarFileName ?? '',
        avatarSize: officeData.avatarSize?.toString() ?? '',
        avatarData: officeData.avatarData ?? '',
        avatarMimeType: officeData.avatarMimeType ?? '',
        versionNumber: officeData.versionNumber?.toString() ?? '',
        active: officeData.active ?? true,
        deleted: officeData.deleted ?? false,
      }, { emitEvent: false });
    }

    this.officeForm.markAsPristine();
    this.officeForm.markAsUntouched();
  }

  public userIsSchedulerOfficeReader(): boolean {
    return this.officeService.userIsSchedulerOfficeReader();
  }

  public userIsSchedulerOfficeWriter(): boolean {
    return this.officeService.userIsSchedulerOfficeWriter();
  }

  public userIsSchedulerAdminsitrator(): boolean {
    return this.authService.isSchedulerAdministrator;
  }

  public userIsFoundationAdministrator(): boolean {
    return this.authService.isFoundationAdmin;
  }
}
