import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserTitleService, SecurityUserTitleData, SecurityUserTitleSubmitData } from '../../../security-data-services/security-user-title.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-security-user-title-add-edit',
  templateUrl: './security-user-title-add-edit.component.html',
  styleUrls: ['./security-user-title-add-edit.component.scss']
})
export class SecurityUserTitleAddEditComponent {
  @ViewChild('securityUserTitleModal') securityUserTitleModal!: TemplateRef<any>;
  @Output() securityUserTitleChanged = new Subject<SecurityUserTitleData[]>();
  @Input() securityUserTitleSubmitData: SecurityUserTitleSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  securityUserTitleForm: FormGroup = this.fb.group({
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

  securityUserTitles$ = this.securityUserTitleService.GetSecurityUserTitleList();

  constructor(
    private modalService: NgbModal,
    private securityUserTitleService: SecurityUserTitleService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(securityUserTitleData?: SecurityUserTitleData) {

    if (securityUserTitleData != null) {

      if (!this.securityUserTitleService.userIsSecuritySecurityUserTitleReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Security User Titles`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.securityUserTitleSubmitData = this.securityUserTitleService.ConvertToSecurityUserTitleSubmitData(securityUserTitleData);
      this.isEditMode = true;
      this.objectGuid = securityUserTitleData.objectGuid;

      this.buildFormValues(securityUserTitleData);

    } else {

      if (!this.securityUserTitleService.userIsSecuritySecurityUserTitleWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Security User Titles`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.securityUserTitleModal, {
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

    if (this.securityUserTitleService.userIsSecuritySecurityUserTitleWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Security User Titles`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.securityUserTitleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityUserTitleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityUserTitleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityUserTitleSubmitData: SecurityUserTitleSubmitData = {
        id: this.securityUserTitleSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSecurityUserTitle(securityUserTitleSubmitData);
      } else {
        this.addSecurityUserTitle(securityUserTitleSubmitData);
      }
  }

  private addSecurityUserTitle(securityUserTitleData: SecurityUserTitleSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    securityUserTitleData.active = true;
    securityUserTitleData.deleted = false;
    this.securityUserTitleService.PostSecurityUserTitle(securityUserTitleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSecurityUserTitle) => {

        this.securityUserTitleService.ClearAllCaches();

        this.securityUserTitleChanged.next([newSecurityUserTitle]);

        this.alertService.showMessage("Security User Title added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/securityusertitle', newSecurityUserTitle.id]);
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
                                   'You do not have permission to save this Security User Title.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Title.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Title could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSecurityUserTitle(securityUserTitleData: SecurityUserTitleSubmitData) {
    this.securityUserTitleService.PutSecurityUserTitle(securityUserTitleData.id, securityUserTitleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSecurityUserTitle) => {

        this.securityUserTitleService.ClearAllCaches();

        this.securityUserTitleChanged.next([updatedSecurityUserTitle]);

        this.alertService.showMessage("Security User Title updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Security User Title.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Title.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Title could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(securityUserTitleData: SecurityUserTitleData | null) {

    if (securityUserTitleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityUserTitleForm.reset({
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
        this.securityUserTitleForm.reset({
        name: securityUserTitleData.name ?? '',
        description: securityUserTitleData.description ?? '',
        active: securityUserTitleData.active ?? true,
        deleted: securityUserTitleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityUserTitleForm.markAsPristine();
    this.securityUserTitleForm.markAsUntouched();
  }

  public userIsSecuritySecurityUserTitleReader(): boolean {
    return this.securityUserTitleService.userIsSecuritySecurityUserTitleReader();
  }

  public userIsSecuritySecurityUserTitleWriter(): boolean {
    return this.securityUserTitleService.userIsSecuritySecurityUserTitleWriter();
  }
}
