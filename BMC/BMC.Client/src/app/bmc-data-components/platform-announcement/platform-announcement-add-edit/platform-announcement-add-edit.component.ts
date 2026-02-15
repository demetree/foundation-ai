/*
   GENERATED FORM FOR THE PLATFORMANNOUNCEMENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from PlatformAnnouncement table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to platform-announcement-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PlatformAnnouncementService, PlatformAnnouncementData, PlatformAnnouncementSubmitData } from '../../../bmc-data-services/platform-announcement.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface PlatformAnnouncementFormValues {
  name: string,
  body: string | null,
  announcementType: string | null,
  startDate: string,
  endDate: string | null,
  isActive: boolean,
  priority: string,     // Stored as string for form input, converted to number on submit.
  showOnLandingPage: boolean,
  showOnDashboard: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-platform-announcement-add-edit',
  templateUrl: './platform-announcement-add-edit.component.html',
  styleUrls: ['./platform-announcement-add-edit.component.scss']
})
export class PlatformAnnouncementAddEditComponent {
  @ViewChild('platformAnnouncementModal') platformAnnouncementModal!: TemplateRef<any>;
  @Output() platformAnnouncementChanged = new Subject<PlatformAnnouncementData[]>();
  @Input() platformAnnouncementSubmitData: PlatformAnnouncementSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<PlatformAnnouncementFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public platformAnnouncementForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        body: [''],
        announcementType: [''],
        startDate: ['', Validators.required],
        endDate: [''],
        isActive: [false],
        priority: ['', Validators.required],
        showOnLandingPage: [false],
        showOnDashboard: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  platformAnnouncements$ = this.platformAnnouncementService.GetPlatformAnnouncementList();

  constructor(
    private modalService: NgbModal,
    private platformAnnouncementService: PlatformAnnouncementService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(platformAnnouncementData?: PlatformAnnouncementData) {

    if (platformAnnouncementData != null) {

      if (!this.platformAnnouncementService.userIsBMCPlatformAnnouncementReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Platform Announcements`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.platformAnnouncementSubmitData = this.platformAnnouncementService.ConvertToPlatformAnnouncementSubmitData(platformAnnouncementData);
      this.isEditMode = true;
      this.objectGuid = platformAnnouncementData.objectGuid;

      this.buildFormValues(platformAnnouncementData);

    } else {

      if (!this.platformAnnouncementService.userIsBMCPlatformAnnouncementWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Platform Announcements`,
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
        this.platformAnnouncementForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.platformAnnouncementForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.platformAnnouncementModal, {
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

    if (this.platformAnnouncementService.userIsBMCPlatformAnnouncementWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Platform Announcements`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.platformAnnouncementForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.platformAnnouncementForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.platformAnnouncementForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const platformAnnouncementSubmitData: PlatformAnnouncementSubmitData = {
        id: this.platformAnnouncementSubmitData?.id || 0,
        name: formValue.name!.trim(),
        body: formValue.body?.trim() || null,
        announcementType: formValue.announcementType?.trim() || null,
        startDate: dateTimeLocalToIsoUtc(formValue.startDate!.trim())!,
        endDate: formValue.endDate ? dateTimeLocalToIsoUtc(formValue.endDate.trim()) : null,
        isActive: !!formValue.isActive,
        priority: Number(formValue.priority),
        showOnLandingPage: !!formValue.showOnLandingPage,
        showOnDashboard: !!formValue.showOnDashboard,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updatePlatformAnnouncement(platformAnnouncementSubmitData);
      } else {
        this.addPlatformAnnouncement(platformAnnouncementSubmitData);
      }
  }

  private addPlatformAnnouncement(platformAnnouncementData: PlatformAnnouncementSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    platformAnnouncementData.active = true;
    platformAnnouncementData.deleted = false;
    this.platformAnnouncementService.PostPlatformAnnouncement(platformAnnouncementData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPlatformAnnouncement) => {

        this.platformAnnouncementService.ClearAllCaches();

        this.platformAnnouncementChanged.next([newPlatformAnnouncement]);

        this.alertService.showMessage("Platform Announcement added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/platformannouncement', newPlatformAnnouncement.id]);
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
                                   'You do not have permission to save this Platform Announcement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Platform Announcement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Platform Announcement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePlatformAnnouncement(platformAnnouncementData: PlatformAnnouncementSubmitData) {
    this.platformAnnouncementService.PutPlatformAnnouncement(platformAnnouncementData.id, platformAnnouncementData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPlatformAnnouncement) => {

        this.platformAnnouncementService.ClearAllCaches();

        this.platformAnnouncementChanged.next([updatedPlatformAnnouncement]);

        this.alertService.showMessage("Platform Announcement updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Platform Announcement.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Platform Announcement.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Platform Announcement could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(platformAnnouncementData: PlatformAnnouncementData | null) {

    if (platformAnnouncementData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.platformAnnouncementForm.reset({
        name: '',
        body: '',
        announcementType: '',
        startDate: '',
        endDate: '',
        isActive: false,
        priority: '',
        showOnLandingPage: false,
        showOnDashboard: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.platformAnnouncementForm.reset({
        name: platformAnnouncementData.name ?? '',
        body: platformAnnouncementData.body ?? '',
        announcementType: platformAnnouncementData.announcementType ?? '',
        startDate: isoUtcStringToDateTimeLocal(platformAnnouncementData.startDate) ?? '',
        endDate: isoUtcStringToDateTimeLocal(platformAnnouncementData.endDate) ?? '',
        isActive: platformAnnouncementData.isActive ?? false,
        priority: platformAnnouncementData.priority?.toString() ?? '',
        showOnLandingPage: platformAnnouncementData.showOnLandingPage ?? false,
        showOnDashboard: platformAnnouncementData.showOnDashboard ?? false,
        active: platformAnnouncementData.active ?? true,
        deleted: platformAnnouncementData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.platformAnnouncementForm.markAsPristine();
    this.platformAnnouncementForm.markAsUntouched();
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


  public userIsBMCPlatformAnnouncementReader(): boolean {
    return this.platformAnnouncementService.userIsBMCPlatformAnnouncementReader();
  }

  public userIsBMCPlatformAnnouncementWriter(): boolean {
    return this.platformAnnouncementService.userIsBMCPlatformAnnouncementWriter();
  }
}
