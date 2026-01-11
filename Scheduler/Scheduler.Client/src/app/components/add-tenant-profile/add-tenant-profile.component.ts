import { Component, ViewChild, ElementRef, Output, Input, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { NgForm } from '@angular/forms';
import { Subject } from 'rxjs';
import { Modal } from 'bootstrap';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { TenantProfileService, TenantProfileData, TenantProfileSubmitData } from '../../scheduler-data-services/tenant-profile.service';
import { StateProvinceService } from '../../scheduler-data-services/state-province.service';
import { CountryService } from '../../scheduler-data-services/country.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-add-tenant-profile',
  templateUrl: './add-tenant-profile.component.html',
  styleUrl: './add-tenant-profile.component.scss'
})

export class AddTenantProfileComponent {

  @ViewChild('tenantProfileForm') tenantProfileForm!: NgForm;           // Access the form instance
  @ViewChild('tenantProfileModal') tenantProfileModal!: ElementRef;     // Access the modal element
  @Output() tenantProfileChanged = new Subject<TenantProfileData[]>();
  @Input() tenantProfileSubmitData: TenantProfileSubmitData = new TenantProfileSubmitData();
  @Input() navigateToDetailsAfterAdd: boolean = true;

  private modalInstance: Modal | undefined;
  public isEditMode = false; // Flag to indicate whether in "Edit" mode or "Add" mode

  public modalIsDisplayed: boolean = false;

  tenantProfiles$ = this.tenantProfileService.GetTenantProfileList();
  states$ = this.stateService.GetStateProvinceList();
  countries$ = this.countryService.GetCountryList();
  constructor(
    private tenantProfileService: TenantProfileService,
    private stateService: StateProvinceService,
    private countryService: CountryService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router
  ) { }


  openModal(tenantProfileData?: TenantProfileData) {

    if (tenantProfileData) {
      // Edit Mode
      if (this.tenantProfileService.userIsSchedulerTenantProfileReader() == false) {
        this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Tenant Profiles", '', MessageSeverity.info);
        return;
      }

      this.tenantProfileSubmitData = this.tenantProfileService.ConvertToTenantProfileSubmitData(tenantProfileData); // Convert TenantProfile data to post object for editing
      this.isEditMode = true; // Set to "Edit" mode
    } else {
      // Add Mode
      if (this.tenantProfileService.userIsSchedulerTenantProfileWriter() == false) {
        this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Tenant Profiles", '', MessageSeverity.info);
        return;
      }

      this.tenantProfileSubmitData = new TenantProfileSubmitData();
      this.isEditMode = false; // Set to "Add" mode
    }

    this.tenantProfileForm.resetForm(this.tenantProfileSubmitData); // Resets form values to their original state and sets the form state to pristine.

    this.modalInstance = new Modal(this.tenantProfileModal.nativeElement, {
      backdrop: 'static',
      keyboard: false
    });

    this.modalInstance.show();
    this.modalIsDisplayed = true;
  }


  closeModal() {
    if (this.modalInstance) {
      this.modalInstance.hide();
    }

    this.modalIsDisplayed = false;
  }




  onFileDrop(event: DragEvent): void {
    event.preventDefault();
    const file = event.dataTransfer?.files?.[0];
    if (file) this.processLogoFile(file);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault(); // Necessary to allow drop
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input?.files?.length) {
      this.processLogoFile(input.files[0]);
    }
  }

  processLogoFile(file: File): void {
    const allowedTypes = ['image/png', 'image/jpeg', 'image/jpg', 'image/svg+xml'];

    if (!allowedTypes.includes(file.type)) {
      this.alertService.showStickyMessage(
        "Invalid file type",
        "Only PNG, JPG, JPEG, and SVG formats are allowed.",
        MessageSeverity.error
      );
      return;
    }

    const reader = new FileReader();

    reader.onload = () => {
      const base64String = (reader.result as string).split(',')[1];
      this.tenantProfileSubmitData.companyLogoData = base64String;
      this.tenantProfileSubmitData.companyLogoFileName = file.name;
      this.tenantProfileSubmitData.companyLogoSize = Math.round(file.size / 1024); // KB
      this.tenantProfileSubmitData.companyLogoMimeType = file.type;
    };

    reader.onerror = () => {
      console.error("Error reading file.");
      this.alertService.showStickyMessage("File Error", "There was an issue reading the file.", MessageSeverity.error);
    };

    reader.readAsDataURL(file);
  }


  clearLogo(): void {
    this.tenantProfileSubmitData.companyLogoData = '';
    this.tenantProfileSubmitData.companyLogoFileName = '';
    this.tenantProfileSubmitData.companyLogoSize = 0;
    this.tenantProfileSubmitData.companyLogoMimeType = '';
  }


  onHexInputChange(value: string | null, field: 'primary' | 'secondary'): void {
    if (!value) return;

    value = value.trim().toLowerCase();

    if (value.startsWith('#')) value = value.slice(1);
    if (!/^[0-9a-f]{0,6}$/.test(value)) return;

    const finalValue = '#' + value;

    if (field === 'primary') {
      this.tenantProfileSubmitData.primaryColor = finalValue;
    } else {
      this.tenantProfileSubmitData.secondaryColor = finalValue;
    }
  }

  onColorPickerChange(value: string, field: 'primary' | 'secondary'): void {
    const hex = value.toLowerCase();
    if (field === 'primary') {
      this.tenantProfileSubmitData.primaryColor = hex;
    } else {
      this.tenantProfileSubmitData.secondaryColor = hex;
    }
  }

  restrictHex(event: KeyboardEvent): void {
    const allowed = /[0-9a-fA-F]/;
    if (!allowed.test(event.key) && event.key !== '#') {
      event.preventDefault();
    }
  }


  submitForm() {
    if (this.tenantProfileService.userIsSchedulerTenantProfileWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Tenant Profiles", '', MessageSeverity.info);
      return;
    }

    // Explicit check for required logo
    if (!this.tenantProfileSubmitData.companyLogoData) {
      this.alertService.showStickyMessage("Validation Error", "Company logo is required.", MessageSeverity.error);
      return;
    }

    const tenantProfileSubmitData: TenantProfileSubmitData = this.tenantProfileSubmitData;

    if (this.isEditMode) {
      this.updateTenantProfile(tenantProfileSubmitData);
    } else {
      this.addTenantProfile(tenantProfileSubmitData);
    }
  }



  private addTenantProfile(tenantProfileData: TenantProfileSubmitData) {

    // Assign initial values to non-nullable control fields suitable for adding new data.
    tenantProfileData.versionNumber = 0;
    tenantProfileData.active = true;
    tenantProfileData.deleted = false;

    this.tenantProfileService.PostTenantProfile(tenantProfileData).subscribe({
      next: (newTenantProfile) => {

        this.tenantProfileService.ClearAllCaches();   // Clear the service data caches because we know that we have changed data.

        this.tenantProfileChanged.next([newTenantProfile]);     // Trigger an event on the changed subject so subscribers know something happened.

        this.alertService.showMessage("Tenant Profile added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd == true) {
          // Navigate to the details page for the new TenantProfile
          this.router.navigate(['/tenantprofile', newTenantProfile.id]);
        }
      },
      error: (err) => {
        this.alertService.showMessage("Tenant Profile could not be added", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  private updateTenantProfile(tenantProfileData: TenantProfileSubmitData) {
    this.tenantProfileService.PutTenantProfile(tenantProfileData.id, tenantProfileData).subscribe({
      next: (updatedTenantProfile) => {

        this.tenantProfileService.ClearAllCaches();      // Clear the service data caches because we know that we have changed data.

        this.tenantProfileChanged.next([updatedTenantProfile]);     // Trigger an event on the changed subject so subscribers know something happened.

        this.alertService.showMessage("Tenant Profile updated successfully", '', MessageSeverity.success);

        this.closeModal();
      },
      error: (err) => {
        this.alertService.showMessage("Tenant Profile could not be updated", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public userIsTenantProfileReader(): boolean {
    return this.tenantProfileService.userIsSchedulerTenantProfileReader();
  }

  public userIsTenantProfileWriter(): boolean {
    return this.tenantProfileService.userIsSchedulerTenantProfileWriter();
  }
}
