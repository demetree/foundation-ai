import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditResourceService, AuditResourceData, AuditResourceSubmitData } from '../../../auditor-data-services/audit-resource.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-audit-resource-add-edit',
  templateUrl: './audit-resource-add-edit.component.html',
  styleUrls: ['./audit-resource-add-edit.component.scss']
})
export class AuditResourceAddEditComponent {
  @ViewChild('auditResourceModal') auditResourceModal!: TemplateRef<any>;
  @Output() auditResourceChanged = new Subject<AuditResourceData[]>();
  @Input() auditResourceSubmitData: AuditResourceSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  auditResourceForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        comments: [''],
        firstAccess: [''],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  auditResources$ = this.auditResourceService.GetAuditResourceList();

  constructor(
    private modalService: NgbModal,
    private auditResourceService: AuditResourceService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(auditResourceData?: AuditResourceData) {

    if (auditResourceData != null) {

      if (!this.auditResourceService.userIsAuditorAuditResourceReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Audit Resources`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.auditResourceSubmitData = this.auditResourceService.ConvertToAuditResourceSubmitData(auditResourceData);
      this.isEditMode = true;

      this.buildFormValues(auditResourceData);

    } else {

      if (!this.auditResourceService.userIsAuditorAuditResourceWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Audit Resources`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.auditResourceModal, {
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

    if (this.auditResourceService.userIsAuditorAuditResourceWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Audit Resources`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.auditResourceForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditResourceForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditResourceForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditResourceSubmitData: AuditResourceSubmitData = {
        id: this.auditResourceSubmitData?.id || 0,
        name: formValue.name!.trim(),
        comments: formValue.comments?.trim() || null,
        firstAccess: formValue.firstAccess ? dateTimeLocalToIsoUtc(formValue.firstAccess.trim()) : null,
   };

      if (this.isEditMode) {
        this.updateAuditResource(auditResourceSubmitData);
      } else {
        this.addAuditResource(auditResourceSubmitData);
      }
  }

  private addAuditResource(auditResourceData: AuditResourceSubmitData) {
    this.auditResourceService.PostAuditResource(auditResourceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAuditResource) => {

        this.auditResourceService.ClearAllCaches();

        this.auditResourceChanged.next([newAuditResource]);

        this.alertService.showMessage("Audit Resource added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/auditresource', newAuditResource.id]);
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
                                   'You do not have permission to save this Audit Resource.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Resource.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Resource could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAuditResource(auditResourceData: AuditResourceSubmitData) {
    this.auditResourceService.PutAuditResource(auditResourceData.id, auditResourceData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAuditResource) => {

        this.auditResourceService.ClearAllCaches();

        this.auditResourceChanged.next([updatedAuditResource]);

        this.alertService.showMessage("Audit Resource updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Audit Resource.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Resource.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Resource could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(auditResourceData: AuditResourceData | null) {

    if (auditResourceData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditResourceForm.reset({
        name: '',
        comments: '',
        firstAccess: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditResourceForm.reset({
        name: auditResourceData.name ?? '',
        comments: auditResourceData.comments ?? '',
        firstAccess: isoUtcStringToDateTimeLocal(auditResourceData.firstAccess) ?? '',
      }, { emitEvent: false});
    }

    this.auditResourceForm.markAsPristine();
    this.auditResourceForm.markAsUntouched();
  }

  public userIsAuditorAuditResourceReader(): boolean {
    return this.auditResourceService.userIsAuditorAuditResourceReader();
  }

  public userIsAuditorAuditResourceWriter(): boolean {
    return this.auditResourceService.userIsAuditorAuditResourceWriter();
  }
}
