import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { RateSheetService, RateSheetData, RateSheetSubmitData } from '../../../scheduler-data-services/rate-sheet.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AssignmentRoleService } from '../../../scheduler-data-services/assignment-role.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { RateTypeService } from '../../../scheduler-data-services/rate-type.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-rate-sheet-custom-add-edit',
  templateUrl: './rate-sheet-custom-add-edit.component.html',
  styleUrls: ['./rate-sheet-custom-add-edit.component.scss']
})
export class RateSheetCustomAddEditComponent {
  @ViewChild('rateSheetModal') rateSheetModal!: TemplateRef<any>;
  @Output() rateSheetChanged = new Subject<RateSheetData[]>();
  @Input() rateSheetSubmitData: RateSheetSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Note that we are purposely NOT using any user default settings for the relations like office ID here because this entity
  // is designed to use nulls for fields for global, so we don't want to replace those with a default value, and force those to be used when the user means null.
  //
  rateSheetForm: FormGroup = this.fb.group({
        scopeMode: [null, Validators.required],
        officeId: [null],
        assignmentRoleId: [null],
        resourceId: [null],
        schedulingTargetId: [null],
        rateTypeId: [null, Validators.required],
        effectiveDate: ['', Validators.required],
        currencyId: [null, Validators.required],
        costRate: ['', Validators.required],
        billingRate: ['', Validators.required],
        notes: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  // To get the count of offices to allow the offices button to be invisible if there are no offices (It can always be found under Administration)
  public officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;

  public rateSheets$ = this.rateSheetService.GetRateSheetList();
  public offices$ = this.officeService.GetOfficeList();
  public assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  public resources$ = this.resourceService.GetResourceList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public rateTypes$ = this.rateTypeService.GetRateTypeList();
  public currencies$ = this.currencyService.GetCurrencyList();

  constructor(
    private modalService: NgbModal,
    private rateSheetService: RateSheetService,
    private officeService: OfficeService,
    private assignmentRoleService: AssignmentRoleService,
    private resourceService: ResourceService,
    private schedulingTargetService: SchedulingTargetService,
    private rateTypeService: RateTypeService,
    private currencyService: CurrencyService,
    private schedulerHelperService: SchedulerHelperService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(rateSheetData?: RateSheetData) {

    if (rateSheetData != null) {

      let scopeMode = 'globalRole';

      if (rateSheetData) {

        if (rateSheetData.schedulingTargetId && rateSheetData.resourceId) {

          scopeMode = 'targetResource';

        } else if (rateSheetData.schedulingTargetId && rateSheetData.assignmentRoleId) {

          scopeMode = 'targetRole';
        }
        else if (rateSheetData.resourceId) {

          scopeMode = 'globalResource';

        } else if (rateSheetData.assignmentRoleId) {

          scopeMode = 'globalRole';
        }
        this.rateSheetForm.patchValue({ scopeMode });
      }

      if (!this.rateSheetService.userIsSchedulerRateSheetReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Rate Sheets`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.rateSheetSubmitData = this.rateSheetService.ConvertToRateSheetSubmitData(rateSheetData);
      this.isEditMode = true;
      this.objectGuid = rateSheetData.objectGuid;

      this.buildFormValues(rateSheetData, scopeMode);

    } else {

      if (!this.rateSheetService.userIsSchedulerRateSheetWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Rate Sheets`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.rateSheetModal, {
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

    if (this.rateSheetService.userIsSchedulerRateSheetWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Rate Sheets`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.rateSheetForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.rateSheetForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.rateSheetForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const rateSheetSubmitData: RateSheetSubmitData = {
        id: this.rateSheetSubmitData?.id || 0,
        officeId: formValue.officeId ? Number(formValue.officeId) : null,
        assignmentRoleId: formValue.assignmentRoleId ? Number(formValue.assignmentRoleId) : null,
        resourceId: formValue.resourceId ? Number(formValue.resourceId) : null,
        schedulingTargetId: formValue.schedulingTargetId ? Number(formValue.schedulingTargetId) : null,
        rateTypeId: Number(formValue.rateTypeId),
        effectiveDate: dateTimeLocalToIsoUtc(formValue.effectiveDate!.trim())!,
        currencyId: Number(formValue.currencyId),
        costRate: Number(formValue.costRate),
        billingRate: Number(formValue.billingRate),
        notes: formValue.notes,
        versionNumber: this.rateSheetSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateRateSheet(rateSheetSubmitData);
      } else {
        this.addRateSheet(rateSheetSubmitData);
      }
  }

  private addRateSheet(rateSheetData: RateSheetSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    rateSheetData.versionNumber = 0;
    rateSheetData.active = true;
    rateSheetData.deleted = false;
    this.rateSheetService.PostRateSheet(rateSheetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newRateSheet) => {

        this.rateSheetService.ClearAllCaches();

        this.rateSheetChanged.next([newRateSheet]);

        this.alertService.showMessage("Rate Sheet added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/ratesheet', newRateSheet.id]);
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
                                   'You do not have permission to save this Rate Sheet.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rate Sheet.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rate Sheet could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateRateSheet(rateSheetData: RateSheetSubmitData) {
    this.rateSheetService.PutRateSheet(rateSheetData.id, rateSheetData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedRateSheet) => {

        this.rateSheetService.ClearAllCaches();

        this.rateSheetChanged.next([updatedRateSheet]);

        this.alertService.showMessage("Rate Sheet updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Rate Sheet.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Rate Sheet.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Rate Sheet could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(rateSheetData: RateSheetData | null, scopeMode: string | null = null) {

    if (rateSheetData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.rateSheetForm.reset({
        scopeMode: scopeMode,
        assignmentRoleId: null,
        resourceId: null,
        schedulingTargetId: null,
        rateTypeId: null,
        effectiveDate: '',
        currencyId: null,
        costRate: '',
        billingRate: '',
        notes: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {
      
        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
      this.rateSheetForm.reset({
        scopeMode: scopeMode,
        officeId: rateSheetData?.officeId ?? null,
        assignmentRoleId: rateSheetData.assignmentRoleId,
        resourceId: rateSheetData.resourceId,
        schedulingTargetId: rateSheetData.schedulingTargetId,
        rateTypeId: rateSheetData.rateTypeId,
        effectiveDate: isoUtcStringToDateTimeLocal(rateSheetData.effectiveDate) ?? '',
        currencyId: rateSheetData.currencyId,
        costRate: rateSheetData.costRate?.toString() ?? '',
        billingRate: rateSheetData.billingRate?.toString() ?? '',
        notes: rateSheetData.notes ?? '',
        versionNumber: rateSheetData.versionNumber?.toString() ?? '',
        active: rateSheetData.active ?? true,
        deleted: rateSheetData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.rateSheetForm.markAsPristine();
    this.rateSheetForm.markAsUntouched();
  }


  // Helper methods

  public isRoleScope(): boolean {
    const mode = this.rateSheetForm.get('scopeMode')?.value;
    return mode === 'globalRole' || mode === 'targetRole';
  }

  public isResourceScope(): boolean {
    const mode = this.rateSheetForm.get('scopeMode')?.value;
    return mode === 'globalResource' || mode === 'targetResource';
  }

  public isTargetScope(): boolean {
    const mode = this.rateSheetForm.get('scopeMode')?.value;
    return mode === 'targetRole' || mode === 'targetResource';
  }

  public onScopeModeChange(): void {
    const mode = this.rateSheetForm.get('scopeMode')?.value;

    // Reset dependent fields  - Note we DO NOT clear the officeId here
    this.rateSheetForm.patchValue({
      assignmentRoleId: null,
      resourceId: null,
      schedulingTargetId: null
    });

    // Add required validators dynamically
    if (this.isRoleScope()) {
      this.rateSheetForm.get('assignmentRoleId')?.setValidators(Validators.required);
    } else {
      this.rateSheetForm.get('assignmentRoleId')?.clearValidators();
    }

    if (this.isResourceScope()) {
      this.rateSheetForm.get('resourceId')?.setValidators(Validators.required);
    } else {
      this.rateSheetForm.get('resourceId')?.clearValidators();
    }

    if (this.isTargetScope()) {
      this.rateSheetForm.get('schedulingTargetId')?.setValidators(Validators.required);
    } else {
      this.rateSheetForm.get('schedulingTargetId')?.clearValidators();
    }

    // Update validity
    ['assignmentRoleId', 'resourceId', 'schedulingTargetId'].forEach(field => {
      this.rateSheetForm.get(field)?.updateValueAndValidity();
    });
  }

  public userIsSchedulerRateSheetReader(): boolean {
    return this.rateSheetService.userIsSchedulerRateSheetReader();
  }

  public userIsSchedulerRateSheetWriter(): boolean {
    return this.rateSheetService.userIsSchedulerRateSheetWriter();
  }
}
