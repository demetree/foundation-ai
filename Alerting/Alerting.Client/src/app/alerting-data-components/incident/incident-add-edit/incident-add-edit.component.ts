/*
   GENERATED FORM FOR THE INCIDENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Incident table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to incident-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { IncidentService, IncidentData, IncidentSubmitData } from '../../../alerting-data-services/incident.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ServiceService } from '../../../alerting-data-services/service.service';
import { SeverityTypeService } from '../../../alerting-data-services/severity-type.service';
import { IncidentStatusTypeService } from '../../../alerting-data-services/incident-status-type.service';
import { EscalationRuleService } from '../../../alerting-data-services/escalation-rule.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface IncidentFormValues {
  incidentKey: string,
  serviceId: number | bigint,       // For FK link number
  title: string,
  description: string | null,
  severityTypeId: number | bigint,       // For FK link number
  incidentStatusTypeId: number | bigint,       // For FK link number
  createdAt: string,
  escalationRuleId: number | bigint | null,       // For FK link number
  currentRepeatCount: string | null,     // Stored as string for form input, converted to number on submit.
  nextEscalationAt: string | null,
  acknowledgedAt: string | null,
  resolvedAt: string | null,
  currentAssigneeObjectGuid: string | null,
  sourcePayloadJson: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-incident-add-edit',
  templateUrl: './incident-add-edit.component.html',
  styleUrls: ['./incident-add-edit.component.scss']
})
export class IncidentAddEditComponent {
  @ViewChild('incidentModal') incidentModal!: TemplateRef<any>;
  @Output() incidentChanged = new Subject<IncidentData[]>();
  @Input() incidentSubmitData: IncidentSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<IncidentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public incidentForm: FormGroup = this.fb.group({
        incidentKey: ['', Validators.required],
        serviceId: [null, Validators.required],
        title: ['', Validators.required],
        description: [''],
        severityTypeId: [null, Validators.required],
        incidentStatusTypeId: [null, Validators.required],
        createdAt: ['', Validators.required],
        escalationRuleId: [null],
        currentRepeatCount: [''],
        nextEscalationAt: [''],
        acknowledgedAt: [''],
        resolvedAt: [''],
        currentAssigneeObjectGuid: [''],
        sourcePayloadJson: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  incidents$ = this.incidentService.GetIncidentList();
  services$ = this.serviceService.GetServiceList();
  severityTypes$ = this.severityTypeService.GetSeverityTypeList();
  incidentStatusTypes$ = this.incidentStatusTypeService.GetIncidentStatusTypeList();
  escalationRules$ = this.escalationRuleService.GetEscalationRuleList();

  constructor(
    private modalService: NgbModal,
    private incidentService: IncidentService,
    private serviceService: ServiceService,
    private severityTypeService: SeverityTypeService,
    private incidentStatusTypeService: IncidentStatusTypeService,
    private escalationRuleService: EscalationRuleService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(incidentData?: IncidentData) {

    if (incidentData != null) {

      if (!this.incidentService.userIsAlertingIncidentReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Incidents`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.incidentSubmitData = this.incidentService.ConvertToIncidentSubmitData(incidentData);
      this.isEditMode = true;
      this.objectGuid = incidentData.objectGuid;

      this.buildFormValues(incidentData);

    } else {

      if (!this.incidentService.userIsAlertingIncidentWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Incidents`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.buildFormValues(null);

      //
      // Apply pre-seeded data if provided and we are in add mode.
      // This patches the form with partial values.
      // Check explicitly for null/undefined to avoid errors.
      //
      if (this.preSeededData !== null && this.preSeededData !== undefined) {
        this.incidentForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.incidentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.incidentModal, {
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

    if (this.incidentService.userIsAlertingIncidentWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Incidents`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.incidentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.incidentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.incidentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const incidentSubmitData: IncidentSubmitData = {
        id: this.incidentSubmitData?.id || 0,
        incidentKey: formValue.incidentKey!.trim(),
        serviceId: Number(formValue.serviceId),
        title: formValue.title!.trim(),
        description: formValue.description?.trim() || null,
        severityTypeId: Number(formValue.severityTypeId),
        incidentStatusTypeId: Number(formValue.incidentStatusTypeId),
        createdAt: dateTimeLocalToIsoUtc(formValue.createdAt!.trim())!,
        escalationRuleId: formValue.escalationRuleId ? Number(formValue.escalationRuleId) : null,
        currentRepeatCount: formValue.currentRepeatCount ? Number(formValue.currentRepeatCount) : null,
        nextEscalationAt: formValue.nextEscalationAt ? dateTimeLocalToIsoUtc(formValue.nextEscalationAt.trim()) : null,
        acknowledgedAt: formValue.acknowledgedAt ? dateTimeLocalToIsoUtc(formValue.acknowledgedAt.trim()) : null,
        resolvedAt: formValue.resolvedAt ? dateTimeLocalToIsoUtc(formValue.resolvedAt.trim()) : null,
        currentAssigneeObjectGuid: formValue.currentAssigneeObjectGuid?.trim() || null,
        sourcePayloadJson: formValue.sourcePayloadJson?.trim() || null,
        versionNumber: this.incidentSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateIncident(incidentSubmitData);
      } else {
        this.addIncident(incidentSubmitData);
      }
  }

  private addIncident(incidentData: IncidentSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    incidentData.versionNumber = 0;
    incidentData.active = true;
    incidentData.deleted = false;
    this.incidentService.PostIncident(incidentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newIncident) => {

        this.incidentService.ClearAllCaches();

        this.incidentChanged.next([newIncident]);

        this.alertService.showMessage("Incident added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/incident', newIncident.id]);
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
                                   'You do not have permission to save this Incident.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateIncident(incidentData: IncidentSubmitData) {
    this.incidentService.PutIncident(incidentData.id, incidentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedIncident) => {

        this.incidentService.ClearAllCaches();

        this.incidentChanged.next([updatedIncident]);

        this.alertService.showMessage("Incident updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Incident.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Incident.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Incident could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(incidentData: IncidentData | null) {

    if (incidentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.incidentForm.reset({
        incidentKey: '',
        serviceId: null,
        title: '',
        description: '',
        severityTypeId: null,
        incidentStatusTypeId: null,
        createdAt: '',
        escalationRuleId: null,
        currentRepeatCount: '',
        nextEscalationAt: '',
        acknowledgedAt: '',
        resolvedAt: '',
        currentAssigneeObjectGuid: '',
        sourcePayloadJson: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.incidentForm.reset({
        incidentKey: incidentData.incidentKey ?? '',
        serviceId: incidentData.serviceId,
        title: incidentData.title ?? '',
        description: incidentData.description ?? '',
        severityTypeId: incidentData.severityTypeId,
        incidentStatusTypeId: incidentData.incidentStatusTypeId,
        createdAt: isoUtcStringToDateTimeLocal(incidentData.createdAt) ?? '',
        escalationRuleId: incidentData.escalationRuleId,
        currentRepeatCount: incidentData.currentRepeatCount?.toString() ?? '',
        nextEscalationAt: isoUtcStringToDateTimeLocal(incidentData.nextEscalationAt) ?? '',
        acknowledgedAt: isoUtcStringToDateTimeLocal(incidentData.acknowledgedAt) ?? '',
        resolvedAt: isoUtcStringToDateTimeLocal(incidentData.resolvedAt) ?? '',
        currentAssigneeObjectGuid: incidentData.currentAssigneeObjectGuid ?? '',
        sourcePayloadJson: incidentData.sourcePayloadJson ?? '',
        versionNumber: incidentData.versionNumber?.toString() ?? '',
        active: incidentData.active ?? true,
        deleted: incidentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.incidentForm.markAsPristine();
    this.incidentForm.markAsUntouched();
  }

  //
  // Helper method to determine if a field should be hidden based on the hiddenFields input.
  // Returns true if the field is in the array, false otherwise.
  //
  public isFieldHidden(fieldName: string): boolean {
    // Explicit check for array existence to avoid runtime errors.
    if (this.hiddenFields === null || this.hiddenFields === undefined) {
      return false;
    }
    // Use traditional includes method for clarity.
    return this.hiddenFields.includes(fieldName);
  }


  public userIsAlertingIncidentReader(): boolean {
    return this.incidentService.userIsAlertingIncidentReader();
  }

  public userIsAlertingIncidentWriter(): boolean {
    return this.incidentService.userIsAlertingIncidentWriter();
  }
}
