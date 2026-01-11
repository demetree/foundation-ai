import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityGroupService, SecurityGroupData, SecurityGroupSubmitData } from '../../../security-data-services/security-group.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-security-group-add-edit',
  templateUrl: './security-group-add-edit.component.html',
  styleUrls: ['./security-group-add-edit.component.scss']
})
export class SecurityGroupAddEditComponent {
  @ViewChild('securityGroupModal') securityGroupModal!: TemplateRef<any>;
  @Output() securityGroupChanged = new Subject<SecurityGroupData[]>();
  @Input() securityGroupSubmitData: SecurityGroupSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  securityGroupForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  securityGroups$ = this.securityGroupService.GetSecurityGroupList();

  constructor(
    private modalService: NgbModal,
    private securityGroupService: SecurityGroupService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityGroupData?: SecurityGroupData) {

    if (securityGroupData != null) {

      if (!this.securityGroupService.userIsSecuritySecurityGroupReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security Groups`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityGroupSubmitData = this.securityGroupService.ConvertToSecurityGroupSubmitData(securityGroupData);
      this.isEditMode = true;

      this.buildFormValues(securityGroupData);

    } else {

      if (!this.securityGroupService.userIsSecuritySecurityGroupWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security Groups`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.securityGroupModal, {
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

    if (this.securityGroupService.userIsSecuritySecurityGroupWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security Groups`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityGroupForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityGroupForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityGroupForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityGroupSubmitData: SecurityGroupSubmitData = {
        id: this.securityGroupSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityGroup(securityGroupSubmitData);
      } else {
        this.addSecurityGroup(securityGroupSubmitData);
      }
  }

  private addSecurityGroup(securityGroupData: SecurityGroupSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityGroupData.active = true;
    securityGroupData.deleted = false;
    this.securityGroupService.PostSecurityGroup(securityGroupData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityGroup) => {

        this.securityGroupService.ClearAllCaches();

        this.securityGroupChanged.next([newSecurityGroup]);

        this.alertService.showMessage("Security Group added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securitygroup', newSecurityGroup.id]);
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
                                   'You do not have permission to save this Security Group.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Group.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Group could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityGroup(securityGroupData: SecurityGroupSubmitData) {
    this.securityGroupService.PutSecurityGroup(securityGroupData.id, securityGroupData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityGroup) => {

        this.securityGroupService.ClearAllCaches();

        this.securityGroupChanged.next([updatedSecurityGroup]);

        this.alertService.showMessage("Security Group updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security Group.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Group.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Group could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityGroupData: SecurityGroupData | null) {

    if (securityGroupData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityGroupForm.reset({
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityGroupForm.reset({
        name: securityGroupData.name ?? '',
        description: securityGroupData.description ?? '',
        active: securityGroupData.active ?? true,
        deleted: securityGroupData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityGroupForm.markAsPristine();
    this.securityGroupForm.markAsUntouched();
  }

  public userIsSecuritySecurityGroupReader(): boolean {
    return this.securityGroupService.userIsSecuritySecurityGroupReader();
  }

  public userIsSecuritySecurityGroupWriter(): boolean {
    return this.securityGroupService.userIsSecuritySecurityGroupWriter();
  }
}
