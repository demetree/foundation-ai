import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditHostSystemService, AuditHostSystemData, AuditHostSystemSubmitData } from '../../../auditor-data-services/audit-host-system.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-audit-host-system-add-edit',
  templateUrl: './audit-host-system-add-edit.component.html',
  styleUrls: ['./audit-host-system-add-edit.component.scss']
})
export class AuditHostSystemAddEditComponent {
  @ViewChild('auditHostSystemModal') auditHostSystemModal!: TemplateRef<any>;
  @Output() auditHostSystemChanged = new Subject<AuditHostSystemData[]>();
  @Input() auditHostSystemSubmitData: AuditHostSystemSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  auditHostSystemForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  auditHostSystems$ = this.auditHostSystemService.GetAuditHostSystemList();

  constructor(
    private modalService: NgbModal,
    private auditHostSystemService: AuditHostSystemService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(auditHostSystemData?: AuditHostSystemData) {

    if (auditHostSystemData != null) {

      if (!this.auditHostSystemService.userIsAuditorAuditHostSystemReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Audit Host Systems`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.auditHostSystemSubmitData = this.auditHostSystemService.ConvertToAuditHostSystemSubmitData(auditHostSystemData);
      this.isEditMode = true;

      this.buildFormValues(auditHostSystemData);

    } else {

      if (!this.auditHostSystemService.userIsAuditorAuditHostSystemWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Audit Host Systems`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.auditHostSystemModal, {
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

    if (this.auditHostSystemService.userIsAuditorAuditHostSystemWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Audit Host Systems`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.auditHostSystemForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditHostSystemForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditHostSystemForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditHostSystemSubmitData: AuditHostSystemSubmitData = {
        id: this.auditHostSystemSubmitData?.id || 0,
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };

      if (this.isEditMode) {
        this.updateAuditHostSystem(auditHostSystemSubmitData);
      } else {
        this.addAuditHostSystem(auditHostSystemSubmitData);
      }
  }

  private addAuditHostSystem(auditHostSystemData: AuditHostSystemSubmitData) {
    this.auditHostSystemService.PostAuditHostSystem(auditHostSystemData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAuditHostSystem) => {

        this.auditHostSystemService.ClearAllCaches();

        this.auditHostSystemChanged.next([newAuditHostSystem]);

        this.alertService.showMessage("Audit Host System added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/audithostsystem', newAuditHostSystem.id]);
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
                                   'You do not have permission to save this Audit Host System.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Host System.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Host System could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAuditHostSystem(auditHostSystemData: AuditHostSystemSubmitData) {
    this.auditHostSystemService.PutAuditHostSystem(auditHostSystemData.id, auditHostSystemData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAuditHostSystem) => {

        this.auditHostSystemService.ClearAllCaches();

        this.auditHostSystemChanged.next([updatedAuditHostSystem]);

        this.alertService.showMessage("Audit Host System updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Audit Host System.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Host System.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Host System could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(auditHostSystemData: AuditHostSystemData | null) {

    if (auditHostSystemData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditHostSystemForm.reset({
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditHostSystemForm.reset({
        name: auditHostSystemData.name ?? '',
        comments: auditHostSystemData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditHostSystemData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditHostSystemForm.markAsPristine();
    this.auditHostSystemForm.markAsUntouched();
  }

  public userIsAuditorAuditHostSystemReader(): boolean {
    return this.auditHostSystemService.userIsAuditorAuditHostSystemReader();
  }

  public userIsAuditorAuditHostSystemWriter(): boolean {
    return this.auditHostSystemService.userIsAuditorAuditHostSystemWriter();
  }
}
