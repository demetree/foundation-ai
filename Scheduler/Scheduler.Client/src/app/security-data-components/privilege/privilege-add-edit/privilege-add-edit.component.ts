import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { PrivilegeService, PrivilegeData, PrivilegeSubmitData } from '../../../security-data-services/privilege.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-privilege-add-edit',
  templateUrl: './privilege-add-edit.component.html',
  styleUrls: ['./privilege-add-edit.component.scss']
})
export class PrivilegeAddEditComponent {
  @ViewChild('privilegeModal') privilegeModal!: TemplateRef<any>;
  @Output() privilegeChanged = new Subject<PrivilegeData[]>();
  @Input() privilegeSubmitData: PrivilegeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  privilegeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  privileges$ = this.privilegeService.GetPrivilegeList();

  constructor(
    private modalService: NgbModal,
    private privilegeService: PrivilegeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(privilegeData?: PrivilegeData) {

    if (privilegeData != null) {

      if (!this.privilegeService.userIsSecurityPrivilegeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Privileges`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.privilegeSubmitData = this.privilegeService.ConvertToPrivilegeSubmitData(privilegeData);
      this.isEditMode = true;

      this.buildFormValues(privilegeData);

    } else {

      if (!this.privilegeService.userIsSecurityPrivilegeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Privileges`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.privilegeModal, {
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

    if (this.privilegeService.userIsSecurityPrivilegeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Privileges`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.privilegeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.privilegeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.privilegeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const privilegeSubmitData: PrivilegeSubmitData = {
        id: this.privilegeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
   };

      if (this.isEditMode) {
        this.updatePrivilege(privilegeSubmitData);
      } else {
        this.addPrivilege(privilegeSubmitData);
      }
  }

  private addPrivilege(privilegeData: PrivilegeSubmitData) {
    this.privilegeService.PostPrivilege(privilegeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newPrivilege) => {

        this.privilegeService.ClearAllCaches();

        this.privilegeChanged.next([newPrivilege]);

        this.alertService.showMessage("Privilege added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/privilege', newPrivilege.id]);
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
                                   'You do not have permission to save this Privilege.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Privilege.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Privilege could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updatePrivilege(privilegeData: PrivilegeSubmitData) {
    this.privilegeService.PutPrivilege(privilegeData.id, privilegeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedPrivilege) => {

        this.privilegeService.ClearAllCaches();

        this.privilegeChanged.next([updatedPrivilege]);

        this.alertService.showMessage("Privilege updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Privilege.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Privilege.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Privilege could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(privilegeData: PrivilegeData | null) {

    if (privilegeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.privilegeForm.reset({
        name: '',
        description: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.privilegeForm.reset({
        name: privilegeData.name ?? '',
        description: privilegeData.description ?? '',
      }, { emitEvent: false});
    }

    this.privilegeForm.markAsPristine();
    this.privilegeForm.markAsUntouched();
  }

  public userIsSecurityPrivilegeReader(): boolean {
    return this.privilegeService.userIsSecurityPrivilegeReader();
  }

  public userIsSecurityPrivilegeWriter(): boolean {
    return this.privilegeService.userIsSecurityPrivilegeWriter();
  }
}
