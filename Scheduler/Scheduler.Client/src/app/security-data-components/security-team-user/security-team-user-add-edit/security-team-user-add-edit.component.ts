import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityTeamUserService, SecurityTeamUserData, SecurityTeamUserSubmitData } from '../../../security-data-services/security-team-user.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityTeamService } from '../../../security-data-services/security-team.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-security-team-user-add-edit',
  templateUrl: './security-team-user-add-edit.component.html',
  styleUrls: ['./security-team-user-add-edit.component.scss']
})
export class SecurityTeamUserAddEditComponent {
  @ViewChild('securityTeamUserModal') securityTeamUserModal!: TemplateRef<any>;
  @Output() securityTeamUserChanged = new Subject<SecurityTeamUserData[]>();
  @Input() securityTeamUserSubmitData: SecurityTeamUserSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  securityTeamUserForm: FormGroup = this.fb.group({
        securityTeamId: [null, Validators.required],
        securityUserId: [null, Validators.required],
        canRead: [false],
        canWrite: [false],
        canChangeHierarchy: [false],
        canChangeOwner: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  securityTeamUsers$ = this.securityTeamUserService.GetSecurityTeamUserList();
  securityTeams$ = this.securityTeamService.GetSecurityTeamList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();

  constructor(
    private modalService: NgbModal,
    private securityTeamUserService: SecurityTeamUserService,
    private securityTeamService: SecurityTeamService,
    private securityUserService: SecurityUserService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityTeamUserData?: SecurityTeamUserData) {

    if (securityTeamUserData != null) {

      if (!this.securityTeamUserService.userIsSecuritySecurityTeamUserReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security Team Users`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityTeamUserSubmitData = this.securityTeamUserService.ConvertToSecurityTeamUserSubmitData(securityTeamUserData);
      this.isEditMode = true;
      this.objectGuid = securityTeamUserData.objectGuid;

      this.buildFormValues(securityTeamUserData);

    } else {

      if (!this.securityTeamUserService.userIsSecuritySecurityTeamUserWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security Team Users`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.securityTeamUserModal, {
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

    if (this.securityTeamUserService.userIsSecuritySecurityTeamUserWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security Team Users`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityTeamUserForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityTeamUserForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityTeamUserForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityTeamUserSubmitData: SecurityTeamUserSubmitData = {
        id: this.securityTeamUserSubmitData?.id || 0,
        securityTeamId: Number(formValue.securityTeamId),
        securityUserId: Number(formValue.securityUserId),
        canRead: !!formValue.canRead,
        canWrite: !!formValue.canWrite,
        canChangeHierarchy: !!formValue.canChangeHierarchy,
        canChangeOwner: !!formValue.canChangeOwner,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityTeamUser(securityTeamUserSubmitData);
      } else {
        this.addSecurityTeamUser(securityTeamUserSubmitData);
      }
  }

  private addSecurityTeamUser(securityTeamUserData: SecurityTeamUserSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityTeamUserData.active = true;
    securityTeamUserData.deleted = false;
    this.securityTeamUserService.PostSecurityTeamUser(securityTeamUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityTeamUser) => {

        this.securityTeamUserService.ClearAllCaches();

        this.securityTeamUserChanged.next([newSecurityTeamUser]);

        this.alertService.showMessage("Security Team User added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securityteamuser', newSecurityTeamUser.id]);
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
                                   'You do not have permission to save this Security Team User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Team User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Team User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityTeamUser(securityTeamUserData: SecurityTeamUserSubmitData) {
    this.securityTeamUserService.PutSecurityTeamUser(securityTeamUserData.id, securityTeamUserData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityTeamUser) => {

        this.securityTeamUserService.ClearAllCaches();

        this.securityTeamUserChanged.next([updatedSecurityTeamUser]);

        this.alertService.showMessage("Security Team User updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security Team User.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Team User.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Team User could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityTeamUserData: SecurityTeamUserData | null) {

    if (securityTeamUserData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityTeamUserForm.reset({
        securityTeamId: null,
        securityUserId: null,
        canRead: false,
        canWrite: false,
        canChangeHierarchy: false,
        canChangeOwner: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityTeamUserForm.reset({
        securityTeamId: securityTeamUserData.securityTeamId,
        securityUserId: securityTeamUserData.securityUserId,
        canRead: securityTeamUserData.canRead ?? false,
        canWrite: securityTeamUserData.canWrite ?? false,
        canChangeHierarchy: securityTeamUserData.canChangeHierarchy ?? false,
        canChangeOwner: securityTeamUserData.canChangeOwner ?? false,
        active: securityTeamUserData.active ?? true,
        deleted: securityTeamUserData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityTeamUserForm.markAsPristine();
    this.securityTeamUserForm.markAsUntouched();
  }

  public userIsSecuritySecurityTeamUserReader(): boolean {
    return this.securityTeamUserService.userIsSecuritySecurityTeamUserReader();
  }

  public userIsSecuritySecurityTeamUserWriter(): boolean {
    return this.securityTeamUserService.userIsSecuritySecurityTeamUserWriter();
  }
}
