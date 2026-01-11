import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SystemSettingService, SystemSettingData, SystemSettingSubmitData } from '../../../security-data-services/system-setting.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-system-setting-add-edit',
  templateUrl: './system-setting-add-edit.component.html',
  styleUrls: ['./system-setting-add-edit.component.scss']
})
export class SystemSettingAddEditComponent {
  @ViewChild('systemSettingModal') systemSettingModal!: TemplateRef<any>;
  @Output() systemSettingChanged = new Subject<SystemSettingData[]>();
  @Input() systemSettingSubmitData: SystemSettingSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  systemSettingForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        value: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  systemSettings$ = this.systemSettingService.GetSystemSettingList();

  constructor(
    private modalService: NgbModal,
    private systemSettingService: SystemSettingService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(systemSettingData?: SystemSettingData) {

    if (systemSettingData != null) {

      if (!this.systemSettingService.userIsSecuritySystemSettingReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read System Settings`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.systemSettingSubmitData = this.systemSettingService.ConvertToSystemSettingSubmitData(systemSettingData);
      this.isEditMode = true;

      this.buildFormValues(systemSettingData);

    } else {

      if (!this.systemSettingService.userIsSecuritySystemSettingWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write System Settings`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.systemSettingModal, {
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

    if (this.systemSettingService.userIsSecuritySystemSettingWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write System Settings`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.systemSettingForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.systemSettingForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.systemSettingForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const systemSettingSubmitData: SystemSettingSubmitData = {
        id: this.systemSettingSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        value: formValue.value?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSystemSetting(systemSettingSubmitData);
      } else {
        this.addSystemSetting(systemSettingSubmitData);
      }
  }

  private addSystemSetting(systemSettingData: SystemSettingSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    systemSettingData.active = true;
    systemSettingData.deleted = false;
    this.systemSettingService.PostSystemSetting(systemSettingData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSystemSetting) => {

        this.systemSettingService.ClearAllCaches();

        this.systemSettingChanged.next([newSystemSetting]);

        this.alertService.showMessage("System Setting added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/systemsetting', newSystemSetting.id]);
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
                                   'You do not have permission to save this System Setting.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the System Setting.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('System Setting could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSystemSetting(systemSettingData: SystemSettingSubmitData) {
    this.systemSettingService.PutSystemSetting(systemSettingData.id, systemSettingData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSystemSetting) => {

        this.systemSettingService.ClearAllCaches();

        this.systemSettingChanged.next([updatedSystemSetting]);

        this.alertService.showMessage("System Setting updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this System Setting.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the System Setting.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('System Setting could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(systemSettingData: SystemSettingData | null) {

    if (systemSettingData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.systemSettingForm.reset({
        name: '',
        description: '',
        value: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.systemSettingForm.reset({
        name: systemSettingData.name ?? '',
        description: systemSettingData.description ?? '',
        value: systemSettingData.value ?? '',
        active: systemSettingData.active ?? true,
        deleted: systemSettingData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.systemSettingForm.markAsPristine();
    this.systemSettingForm.markAsUntouched();
  }

  public userIsSecuritySystemSettingReader(): boolean {
    return this.systemSettingService.userIsSecuritySystemSettingReader();
  }

  public userIsSecuritySystemSettingWriter(): boolean {
    return this.systemSettingService.userIsSecuritySystemSettingWriter();
  }
}
