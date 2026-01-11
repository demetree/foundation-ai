import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityDepartmentService, SecurityDepartmentData, SecurityDepartmentSubmitData } from '../../../security-data-services/security-department.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityOrganizationService } from '../../../security-data-services/security-organization.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-security-department-add-edit',
  templateUrl: './security-department-add-edit.component.html',
  styleUrls: ['./security-department-add-edit.component.scss']
})
export class SecurityDepartmentAddEditComponent {
  @ViewChild('securityDepartmentModal') securityDepartmentModal!: TemplateRef<any>;
  @Output() securityDepartmentChanged = new Subject<SecurityDepartmentData[]>();
  @Input() securityDepartmentSubmitData: SecurityDepartmentSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  securityDepartmentForm: FormGroup = this.fb.group({
        securityOrganizationId: [null, Validators.required],
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

  securityDepartments$ = this.securityDepartmentService.GetSecurityDepartmentList();
  securityOrganizations$ = this.securityOrganizationService.GetSecurityOrganizationList();

  constructor(
    private modalService: NgbModal,
    private securityDepartmentService: SecurityDepartmentService,
    private securityOrganizationService: SecurityOrganizationService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityDepartmentData?: SecurityDepartmentData) {

    if (securityDepartmentData != null) {

      if (!this.securityDepartmentService.userIsSecuritySecurityDepartmentReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security Departments`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityDepartmentSubmitData = this.securityDepartmentService.ConvertToSecurityDepartmentSubmitData(securityDepartmentData);
      this.isEditMode = true;
      this.objectGuid = securityDepartmentData.objectGuid;

      this.buildFormValues(securityDepartmentData);

    } else {

      if (!this.securityDepartmentService.userIsSecuritySecurityDepartmentWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security Departments`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.securityDepartmentModal, {
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

    if (this.securityDepartmentService.userIsSecuritySecurityDepartmentWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security Departments`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityDepartmentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityDepartmentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityDepartmentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityDepartmentSubmitData: SecurityDepartmentSubmitData = {
        id: this.securityDepartmentSubmitData?.id || 0,
        securityOrganizationId: Number(formValue.securityOrganizationId),
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityDepartment(securityDepartmentSubmitData);
      } else {
        this.addSecurityDepartment(securityDepartmentSubmitData);
      }
  }

  private addSecurityDepartment(securityDepartmentData: SecurityDepartmentSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityDepartmentData.active = true;
    securityDepartmentData.deleted = false;
    this.securityDepartmentService.PostSecurityDepartment(securityDepartmentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityDepartment) => {

        this.securityDepartmentService.ClearAllCaches();

        this.securityDepartmentChanged.next([newSecurityDepartment]);

        this.alertService.showMessage("Security Department added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securitydepartment', newSecurityDepartment.id]);
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
                                   'You do not have permission to save this Security Department.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Department.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Department could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityDepartment(securityDepartmentData: SecurityDepartmentSubmitData) {
    this.securityDepartmentService.PutSecurityDepartment(securityDepartmentData.id, securityDepartmentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityDepartment) => {

        this.securityDepartmentService.ClearAllCaches();

        this.securityDepartmentChanged.next([updatedSecurityDepartment]);

        this.alertService.showMessage("Security Department updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security Department.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security Department.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security Department could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityDepartmentData: SecurityDepartmentData | null) {

    if (securityDepartmentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityDepartmentForm.reset({
        securityOrganizationId: null,
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
        this.securityDepartmentForm.reset({
        securityOrganizationId: securityDepartmentData.securityOrganizationId,
        name: securityDepartmentData.name ?? '',
        description: securityDepartmentData.description ?? '',
        active: securityDepartmentData.active ?? true,
        deleted: securityDepartmentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityDepartmentForm.markAsPristine();
    this.securityDepartmentForm.markAsUntouched();
  }

  public userIsSecuritySecurityDepartmentReader(): boolean {
    return this.securityDepartmentService.userIsSecuritySecurityDepartmentReader();
  }

  public userIsSecuritySecurityDepartmentWriter(): boolean {
    return this.securityDepartmentService.userIsSecuritySecurityDepartmentWriter();
  }
}
