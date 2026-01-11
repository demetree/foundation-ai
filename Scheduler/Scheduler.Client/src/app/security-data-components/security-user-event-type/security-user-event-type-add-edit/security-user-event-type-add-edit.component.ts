import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserEventTypeService, SecurityUserEventTypeData, SecurityUserEventTypeSubmitData } from '../../../security-data-services/security-user-event-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-security-user-event-type-add-edit',
  templateUrl: './security-user-event-type-add-edit.component.html',
  styleUrls: ['./security-user-event-type-add-edit.component.scss']
})
export class SecurityUserEventTypeAddEditComponent {
  @ViewChild('securityUserEventTypeModal') securityUserEventTypeModal!: TemplateRef<any>;
  @Output() securityUserEventTypeChanged = new Subject<SecurityUserEventTypeData[]>();
  @Input() securityUserEventTypeSubmitData: SecurityUserEventTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  securityUserEventTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  securityUserEventTypes$ = this.securityUserEventTypeService.GetSecurityUserEventTypeList();

  constructor(
    private modalService: NgbModal,
    private securityUserEventTypeService: SecurityUserEventTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityUserEventTypeData?: SecurityUserEventTypeData) {

    if (securityUserEventTypeData != null) {

      if (!this.securityUserEventTypeService.userIsSecuritySecurityUserEventTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security User Event Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityUserEventTypeSubmitData = this.securityUserEventTypeService.ConvertToSecurityUserEventTypeSubmitData(securityUserEventTypeData);
      this.isEditMode = true;

      this.buildFormValues(securityUserEventTypeData);

    } else {

      if (!this.securityUserEventTypeService.userIsSecuritySecurityUserEventTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security User Event Types`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.securityUserEventTypeModal, {
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

    if (this.securityUserEventTypeService.userIsSecuritySecurityUserEventTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security User Event Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityUserEventTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityUserEventTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityUserEventTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityUserEventTypeSubmitData: SecurityUserEventTypeSubmitData = {
        id: this.securityUserEventTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
   };

      if (this.isEditMode) {
        this.updateSecurityUserEventType(securityUserEventTypeSubmitData);
      } else {
        this.addSecurityUserEventType(securityUserEventTypeSubmitData);
      }
  }

  private addSecurityUserEventType(securityUserEventTypeData: SecurityUserEventTypeSubmitData) {
    this.securityUserEventTypeService.PostSecurityUserEventType(securityUserEventTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityUserEventType) => {

        this.securityUserEventTypeService.ClearAllCaches();

        this.securityUserEventTypeChanged.next([newSecurityUserEventType]);

        this.alertService.showMessage("Security User Event Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securityusereventtype', newSecurityUserEventType.id]);
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
                                   'You do not have permission to save this Security User Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityUserEventType(securityUserEventTypeData: SecurityUserEventTypeSubmitData) {
    this.securityUserEventTypeService.PutSecurityUserEventType(securityUserEventTypeData.id, securityUserEventTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityUserEventType) => {

        this.securityUserEventTypeService.ClearAllCaches();

        this.securityUserEventTypeChanged.next([updatedSecurityUserEventType]);

        this.alertService.showMessage("Security User Event Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security User Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityUserEventTypeData: SecurityUserEventTypeData | null) {

    if (securityUserEventTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityUserEventTypeForm.reset({
        name: '',
        description: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityUserEventTypeForm.reset({
        name: securityUserEventTypeData.name ?? '',
        description: securityUserEventTypeData.description ?? '',
      }, { emitEvent: false});
    }

    this.securityUserEventTypeForm.markAsPristine();
    this.securityUserEventTypeForm.markAsUntouched();
  }

  public userIsSecuritySecurityUserEventTypeReader(): boolean {
    return this.securityUserEventTypeService.userIsSecuritySecurityUserEventTypeReader();
  }

  public userIsSecuritySecurityUserEventTypeWriter(): boolean {
    return this.securityUserEventTypeService.userIsSecuritySecurityUserEventTypeWriter();
  }
}
