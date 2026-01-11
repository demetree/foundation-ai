import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ModuleService, ModuleData, ModuleSubmitData } from '../../../security-data-services/module.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-module-add-edit',
  templateUrl: './module-add-edit.component.html',
  styleUrls: ['./module-add-edit.component.scss']
})
export class ModuleAddEditComponent {
  @ViewChild('moduleModal') moduleModal!: TemplateRef<any>;
  @Output() moduleChanged = new Subject<ModuleData[]>();
  @Input() moduleSubmitData: ModuleSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;

  moduleForm: FormGroup = this.fb.group({
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

  modules$ = this.moduleService.GetModuleList();

  constructor(
    private modalService: NgbModal,
    private moduleService: ModuleService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(moduleData?: ModuleData) {

    if (moduleData != null) {

      if (!this.moduleService.userIsSecurityModuleReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Modules`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.moduleSubmitData = this.moduleService.ConvertToModuleSubmitData(moduleData);
      this.isEditMode = true;

      this.buildFormValues(moduleData);

    } else {

      if (!this.moduleService.userIsSecurityModuleWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Modules`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.moduleModal, {
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

    if (this.moduleService.userIsSecurityModuleWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Modules`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.moduleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.moduleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.moduleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const moduleSubmitData: ModuleSubmitData = {
        id: this.moduleSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateModule(moduleSubmitData);
      } else {
        this.addModule(moduleSubmitData);
      }
  }

  private addModule(moduleData: ModuleSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    moduleData.active = true;
    moduleData.deleted = false;
    this.moduleService.PostModule(moduleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newModule) => {

        this.moduleService.ClearAllCaches();

        this.moduleChanged.next([newModule]);

        this.alertService.showMessage("Module added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/module', newModule.id]);
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
                                   'You do not have permission to save this Module.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Module.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Module could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateModule(moduleData: ModuleSubmitData) {
    this.moduleService.PutModule(moduleData.id, moduleData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedModule) => {

        this.moduleService.ClearAllCaches();

        this.moduleChanged.next([updatedModule]);

        this.alertService.showMessage("Module updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Module.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Module.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Module could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(moduleData: ModuleData | null) {

    if (moduleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.moduleForm.reset({
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
        this.moduleForm.reset({
        name: moduleData.name ?? '',
        description: moduleData.description ?? '',
        active: moduleData.active ?? true,
        deleted: moduleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.moduleForm.markAsPristine();
    this.moduleForm.markAsUntouched();
  }

  public userIsSecurityModuleReader(): boolean {
    return this.moduleService.userIsSecurityModuleReader();
  }

  public userIsSecurityModuleWriter(): boolean {
    return this.moduleService.userIsSecurityModuleWriter();
  }
}
