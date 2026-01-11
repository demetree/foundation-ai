import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EntityDataTokenService, EntityDataTokenData, EntityDataTokenSubmitData } from '../../../security-data-services/entity-data-token.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { ModuleService } from '../../../security-data-services/module.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-entity-data-token-add-edit',
  templateUrl: './entity-data-token-add-edit.component.html',
  styleUrls: ['./entity-data-token-add-edit.component.scss']
})
export class EntityDataTokenAddEditComponent {
  @ViewChild('entityDataTokenModal') entityDataTokenModal!: TemplateRef<any>;
  @Output() entityDataTokenChanged = new Subject<EntityDataTokenData[]>();
  @Input() entityDataTokenSubmitData: EntityDataTokenSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  entityDataTokenForm: FormGroup = this.fb.group({
        securityUserId: [null, Validators.required],
        moduleId: [null, Validators.required],
        entity: ['', Validators.required],
        sessionId: ['', Validators.required],
        authenticationToken: ['', Validators.required],
        token: ['', Validators.required],
        timeStamp: ['', Validators.required],
        comments: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  entityDataTokens$ = this.entityDataTokenService.GetEntityDataTokenList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();
  modules$ = this.moduleService.GetModuleList();

  constructor(
    private modalService: NgbModal,
    private entityDataTokenService: EntityDataTokenService,
    private securityUserService: SecurityUserService,
    private moduleService: ModuleService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(entityDataTokenData?: EntityDataTokenData) {

    if (entityDataTokenData != null) {

      if (!this.entityDataTokenService.userIsSecurityEntityDataTokenReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Entity Data Tokens`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.entityDataTokenSubmitData = this.entityDataTokenService.ConvertToEntityDataTokenSubmitData(entityDataTokenData);
      this.isEditMode = true;

      this.buildFormValues(entityDataTokenData);

    } else {

      if (!this.entityDataTokenService.userIsSecurityEntityDataTokenWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Entity Data Tokens`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.entityDataTokenModal, {
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

    if (this.entityDataTokenService.userIsSecurityEntityDataTokenWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Entity Data Tokens`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.entityDataTokenForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.entityDataTokenForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.entityDataTokenForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const entityDataTokenSubmitData: EntityDataTokenSubmitData = {
        id: this.entityDataTokenSubmitData?.id || 0,
        securityUserId: Number(formValue.securityUserId),
        moduleId: Number(formValue.moduleId),
        entity: formValue.entity!.trim(),
        sessionId: formValue.sessionId!.trim(),
        authenticationToken: formValue.authenticationToken!.trim(),
        token: formValue.token!.trim(),
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateEntityDataToken(entityDataTokenSubmitData);
      } else {
        this.addEntityDataToken(entityDataTokenSubmitData);
      }
  }

  private addEntityDataToken(entityDataTokenData: EntityDataTokenSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    entityDataTokenData.active = true;
    entityDataTokenData.deleted = false;
    this.entityDataTokenService.PostEntityDataToken(entityDataTokenData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newEntityDataToken) => {

        this.entityDataTokenService.ClearAllCaches();

        this.entityDataTokenChanged.next([newEntityDataToken]);

        this.alertService.showMessage("Entity Data Token added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/entitydatatoken', newEntityDataToken.id]);
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
                                   'You do not have permission to save this Entity Data Token.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Entity Data Token.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Entity Data Token could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateEntityDataToken(entityDataTokenData: EntityDataTokenSubmitData) {
    this.entityDataTokenService.PutEntityDataToken(entityDataTokenData.id, entityDataTokenData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedEntityDataToken) => {

        this.entityDataTokenService.ClearAllCaches();

        this.entityDataTokenChanged.next([updatedEntityDataToken]);

        this.alertService.showMessage("Entity Data Token updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Entity Data Token.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Entity Data Token.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Entity Data Token could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(entityDataTokenData: EntityDataTokenData | null) {

    if (entityDataTokenData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.entityDataTokenForm.reset({
        securityUserId: null,
        moduleId: null,
        entity: '',
        sessionId: '',
        authenticationToken: '',
        token: '',
        timeStamp: '',
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.entityDataTokenForm.reset({
        securityUserId: entityDataTokenData.securityUserId,
        moduleId: entityDataTokenData.moduleId,
        entity: entityDataTokenData.entity ?? '',
        sessionId: entityDataTokenData.sessionId ?? '',
        authenticationToken: entityDataTokenData.authenticationToken ?? '',
        token: entityDataTokenData.token ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(entityDataTokenData.timeStamp) ?? '',
        comments: entityDataTokenData.comments ?? '',
        active: entityDataTokenData.active ?? true,
        deleted: entityDataTokenData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.entityDataTokenForm.markAsPristine();
    this.entityDataTokenForm.markAsUntouched();
  }

  public userIsSecurityEntityDataTokenReader(): boolean {
    return this.entityDataTokenService.userIsSecurityEntityDataTokenReader();
  }

  public userIsSecurityEntityDataTokenWriter(): boolean {
    return this.entityDataTokenService.userIsSecurityEntityDataTokenWriter();
  }
}
