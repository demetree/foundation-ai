import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserSecurityGroupService, SecurityUserSecurityGroupData, SecurityUserSecurityGroupSubmitData } from '../../../security-data-services/security-user-security-group.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { SecurityGroupService } from '../../../security-data-services/security-group.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-security-user-security-group-add-edit',
  templateUrl: './security-user-security-group-add-edit.component.html',
  styleUrls: ['./security-user-security-group-add-edit.component.scss']
})
export class SecurityUserSecurityGroupAddEditComponent {
  @ViewChild('securityUserSecurityGroupModal') securityUserSecurityGroupModal!: TemplateRef<any>;
  @Output() securityUserSecurityGroupChanged = new Subject<SecurityUserSecurityGroupData[]>();
  @Input() securityUserSecurityGroupSubmitData: SecurityUserSecurityGroupSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  securityUserSecurityGroupForm: FormGroup = this.fb.group({
        securityUserId: [null, Validators.required],
        securityGroupId: [null, Validators.required],
        comments: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  securityUserSecurityGroups$ = this.securityUserSecurityGroupService.GetSecurityUserSecurityGroupList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();
  securityGroups$ = this.securityGroupService.GetSecurityGroupList();

  constructor(
    private modalService: NgbModal,
    private securityUserSecurityGroupService: SecurityUserSecurityGroupService,
    private securityUserService: SecurityUserService,
    private securityGroupService: SecurityGroupService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityUserSecurityGroupData?: SecurityUserSecurityGroupData) {

    if (securityUserSecurityGroupData != null) {

      if (!this.securityUserSecurityGroupService.userIsSecuritySecurityUserSecurityGroupReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security User Security Groups`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityUserSecurityGroupSubmitData = this.securityUserSecurityGroupService.ConvertToSecurityUserSecurityGroupSubmitData(securityUserSecurityGroupData);
      this.isEditMode = true;

      this.buildFormValues(securityUserSecurityGroupData);

    } else {

      if (!this.securityUserSecurityGroupService.userIsSecuritySecurityUserSecurityGroupWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security User Security Groups`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.securityUserSecurityGroupModal, {
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

    if (this.securityUserSecurityGroupService.userIsSecuritySecurityUserSecurityGroupWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security User Security Groups`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityUserSecurityGroupForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityUserSecurityGroupForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityUserSecurityGroupForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityUserSecurityGroupSubmitData: SecurityUserSecurityGroupSubmitData = {
        id: this.securityUserSecurityGroupSubmitData?.id || 0,
        securityUserId: Number(formValue.securityUserId),
        securityGroupId: Number(formValue.securityGroupId),
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityUserSecurityGroup(securityUserSecurityGroupSubmitData);
      } else {
        this.addSecurityUserSecurityGroup(securityUserSecurityGroupSubmitData);
      }
  }

  private addSecurityUserSecurityGroup(securityUserSecurityGroupData: SecurityUserSecurityGroupSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityUserSecurityGroupData.active = true;
    securityUserSecurityGroupData.deleted = false;
    this.securityUserSecurityGroupService.PostSecurityUserSecurityGroup(securityUserSecurityGroupData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityUserSecurityGroup) => {

        this.securityUserSecurityGroupService.ClearAllCaches();

        this.securityUserSecurityGroupChanged.next([newSecurityUserSecurityGroup]);

        this.alertService.showMessage("Security User Security Group added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securityusersecuritygroup', newSecurityUserSecurityGroup.id]);
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
                                   'You do not have permission to save this Security User Security Group.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Security Group.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Security Group could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityUserSecurityGroup(securityUserSecurityGroupData: SecurityUserSecurityGroupSubmitData) {
    this.securityUserSecurityGroupService.PutSecurityUserSecurityGroup(securityUserSecurityGroupData.id, securityUserSecurityGroupData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityUserSecurityGroup) => {

        this.securityUserSecurityGroupService.ClearAllCaches();

        this.securityUserSecurityGroupChanged.next([updatedSecurityUserSecurityGroup]);

        this.alertService.showMessage("Security User Security Group updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security User Security Group.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Security Group.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Security Group could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityUserSecurityGroupData: SecurityUserSecurityGroupData | null) {

    if (securityUserSecurityGroupData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityUserSecurityGroupForm.reset({
        securityUserId: null,
        securityGroupId: null,
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityUserSecurityGroupForm.reset({
        securityUserId: securityUserSecurityGroupData.securityUserId,
        securityGroupId: securityUserSecurityGroupData.securityGroupId,
        comments: securityUserSecurityGroupData.comments ?? '',
        active: securityUserSecurityGroupData.active ?? true,
        deleted: securityUserSecurityGroupData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityUserSecurityGroupForm.markAsPristine();
    this.securityUserSecurityGroupForm.markAsUntouched();
  }

  public userIsSecuritySecurityUserSecurityGroupReader(): boolean {
    return this.securityUserSecurityGroupService.userIsSecuritySecurityUserSecurityGroupReader();
  }

  public userIsSecuritySecurityUserSecurityGroupWriter(): boolean {
    return this.securityUserSecurityGroupService.userIsSecuritySecurityUserSecurityGroupWriter();
  }
}
