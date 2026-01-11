import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityTeamService, SecurityTeamData, SecurityTeamSubmitData } from '../../../security-data-services/security-team.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityDepartmentService } from '../../../security-data-services/security-department.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-security-team-add-edit',
  templateUrl: './security-team-add-edit.component.html',
  styleUrls: ['./security-team-add-edit.component.scss']
})
export class SecurityTeamAddEditComponent {
  @ViewChild('securityTeamModal') securityTeamModal!: TemplateRef<any>;
  @Output() securityTeamChanged = new Subject<SecurityTeamData[]>();
  @Input() securityTeamSubmitData: SecurityTeamSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  securityTeamForm: FormGroup = this.fb.group({
        securityDepartmentId: [null, Validators.required],
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

  securityTeams$ = this.securityTeamService.GetSecurityTeamList();
  securityDepartments$ = this.securityDepartmentService.GetSecurityDepartmentList();

  constructor(
    private modalService: NgbModal,
    private securityTeamService: SecurityTeamService,
    private securityDepartmentService: SecurityDepartmentService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityTeamData?: SecurityTeamData) {

    if (securityTeamData != null) {

      if (!this.securityTeamService.userIsSecuritySecurityTeamReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security Teams`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityTeamSubmitData = this.securityTeamService.ConvertToSecurityTeamSubmitData(securityTeamData);
      this.isEditMode = true;
      this.objectGuid = securityTeamData.objectGuid;

      this.buildFormValues(securityTeamData);

    } else {

      if (!this.securityTeamService.userIsSecuritySecurityTeamWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security Teams`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.securityTeamModal, {
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

    if (this.securityTeamService.userIsSecuritySecurityTeamWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security Teams`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityTeamForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityTeamForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityTeamForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityTeamSubmitData: SecurityTeamSubmitData = {
        id: this.securityTeamSubmitData?.id || 0,
        securityDepartmentId: Number(formValue.securityDepartmentId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityTeam(securityTeamSubmitData);
      } else {
        this.addSecurityTeam(securityTeamSubmitData);
      }
  }

  private addSecurityTeam(securityTeamData: SecurityTeamSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityTeamData.active = true;
    securityTeamData.deleted = false;
    this.securityTeamService.PostSecurityTeam(securityTeamData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityTeam) => {

        this.securityTeamService.ClearAllCaches();

        this.securityTeamChanged.next([newSecurityTeam]);

        this.alertService.showMessage("Security Team added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securityteam', newSecurityTeam.id]);
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
                                   'You do not have permission to save this Security Team.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Team.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Team could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityTeam(securityTeamData: SecurityTeamSubmitData) {
    this.securityTeamService.PutSecurityTeam(securityTeamData.id, securityTeamData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityTeam) => {

        this.securityTeamService.ClearAllCaches();

        this.securityTeamChanged.next([updatedSecurityTeam]);

        this.alertService.showMessage("Security Team updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security Team.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Team.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Team could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityTeamData: SecurityTeamData | null) {

    if (securityTeamData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityTeamForm.reset({
        securityDepartmentId: null,
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
        this.securityTeamForm.reset({
        securityDepartmentId: securityTeamData.securityDepartmentId,
        name: securityTeamData.name ?? '',
        description: securityTeamData.description ?? '',
        active: securityTeamData.active ?? true,
        deleted: securityTeamData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityTeamForm.markAsPristine();
    this.securityTeamForm.markAsUntouched();
  }

  public userIsSecuritySecurityTeamReader(): boolean {
    return this.securityTeamService.userIsSecuritySecurityTeamReader();
  }

  public userIsSecuritySecurityTeamWriter(): boolean {
    return this.securityTeamService.userIsSecuritySecurityTeamWriter();
  }
}
