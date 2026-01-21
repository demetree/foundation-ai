/*
   GENERATED FORM FOR THE AUDITEVENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from AuditEvent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to audit-event-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuditEventService, AuditEventData, AuditEventSubmitData } from '../../../auditor-data-services/audit-event.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuditUserService } from '../../../auditor-data-services/audit-user.service';
import { AuditSessionService } from '../../../auditor-data-services/audit-session.service';
import { AuditTypeService } from '../../../auditor-data-services/audit-type.service';
import { AuditAccessTypeService } from '../../../auditor-data-services/audit-access-type.service';
import { AuditSourceService } from '../../../auditor-data-services/audit-source.service';
import { AuditUserAgentService } from '../../../auditor-data-services/audit-user-agent.service';
import { AuditModuleService } from '../../../auditor-data-services/audit-module.service';
import { AuditModuleEntityService } from '../../../auditor-data-services/audit-module-entity.service';
import { AuditResourceService } from '../../../auditor-data-services/audit-resource.service';
import { AuditHostSystemService } from '../../../auditor-data-services/audit-host-system.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface AuditEventFormValues {
  startTime: string,
  stopTime: string,
  completedSuccessfully: boolean,
  auditUserId: number | bigint,       // For FK link number
  auditSessionId: number | bigint,       // For FK link number
  auditTypeId: number | bigint,       // For FK link number
  auditAccessTypeId: number | bigint,       // For FK link number
  auditSourceId: number | bigint,       // For FK link number
  auditUserAgentId: number | bigint,       // For FK link number
  auditModuleId: number | bigint,       // For FK link number
  auditModuleEntityId: number | bigint,       // For FK link number
  auditResourceId: number | bigint,       // For FK link number
  auditHostSystemId: number | bigint,       // For FK link number
  primaryKey: string | null,
  threadId: string | null,     // Stored as string for form input, converted to number on submit.
  message: string,
};

@Component({
  selector: 'app-audit-event-add-edit',
  templateUrl: './audit-event-add-edit.component.html',
  styleUrls: ['./audit-event-add-edit.component.scss']
})
export class AuditEventAddEditComponent {
  @ViewChild('auditEventModal') auditEventModal!: TemplateRef<any>;
  @Output() auditEventChanged = new Subject<AuditEventData[]>();
  @Input() auditEventSubmitData: AuditEventSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<AuditEventFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public auditEventForm: FormGroup = this.fb.group({
        startTime: ['', Validators.required],
        stopTime: ['', Validators.required],
        completedSuccessfully: [false],
        auditUserId: [null, Validators.required],
        auditSessionId: [null, Validators.required],
        auditTypeId: [null, Validators.required],
        auditAccessTypeId: [null, Validators.required],
        auditSourceId: [null, Validators.required],
        auditUserAgentId: [null, Validators.required],
        auditModuleId: [null, Validators.required],
        auditModuleEntityId: [null, Validators.required],
        auditResourceId: [null, Validators.required],
        auditHostSystemId: [null, Validators.required],
        primaryKey: [''],
        threadId: [''],
        message: ['', Validators.required],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  auditEvents$ = this.auditEventService.GetAuditEventList();
  auditUsers$ = this.auditUserService.GetAuditUserList();
  auditSessions$ = this.auditSessionService.GetAuditSessionList();
  auditTypes$ = this.auditTypeService.GetAuditTypeList();
  auditAccessTypes$ = this.auditAccessTypeService.GetAuditAccessTypeList();
  auditSources$ = this.auditSourceService.GetAuditSourceList();
  auditUserAgents$ = this.auditUserAgentService.GetAuditUserAgentList();
  auditModules$ = this.auditModuleService.GetAuditModuleList();
  auditModuleEntities$ = this.auditModuleEntityService.GetAuditModuleEntityList();
  auditResources$ = this.auditResourceService.GetAuditResourceList();
  auditHostSystems$ = this.auditHostSystemService.GetAuditHostSystemList();

  constructor(
    private modalService: NgbModal,
    private auditEventService: AuditEventService,
    private auditUserService: AuditUserService,
    private auditSessionService: AuditSessionService,
    private auditTypeService: AuditTypeService,
    private auditAccessTypeService: AuditAccessTypeService,
    private auditSourceService: AuditSourceService,
    private auditUserAgentService: AuditUserAgentService,
    private auditModuleService: AuditModuleService,
    private auditModuleEntityService: AuditModuleEntityService,
    private auditResourceService: AuditResourceService,
    private auditHostSystemService: AuditHostSystemService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(auditEventData?: AuditEventData) {

    if (auditEventData != null) {

      if (!this.auditEventService.userIsAuditorAuditEventReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Audit Events`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.auditEventSubmitData = this.auditEventService.ConvertToAuditEventSubmitData(auditEventData);
      this.isEditMode = true;

      this.buildFormValues(auditEventData);

    } else {

      if (!this.auditEventService.userIsAuditorAuditEventWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Audit Events`,
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
        this.auditEventForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.auditEventForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.auditEventModal, {
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

    if (this.auditEventService.userIsAuditorAuditEventWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Audit Events`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.auditEventForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.auditEventForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.auditEventForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const auditEventSubmitData: AuditEventSubmitData = {
        id: this.auditEventSubmitData?.id || 0,
        startTime: dateTimeLocalToIsoUtc(formValue.startTime!.trim())!,
        stopTime: dateTimeLocalToIsoUtc(formValue.stopTime!.trim())!,
        completedSuccessfully: !!formValue.completedSuccessfully,
        auditUserId: Number(formValue.auditUserId),
        auditSessionId: Number(formValue.auditSessionId),
        auditTypeId: Number(formValue.auditTypeId),
        auditAccessTypeId: Number(formValue.auditAccessTypeId),
        auditSourceId: Number(formValue.auditSourceId),
        auditUserAgentId: Number(formValue.auditUserAgentId),
        auditModuleId: Number(formValue.auditModuleId),
        auditModuleEntityId: Number(formValue.auditModuleEntityId),
        auditResourceId: Number(formValue.auditResourceId),
        auditHostSystemId: Number(formValue.auditHostSystemId),
        primaryKey: formValue.primaryKey?.trim() || null,
        threadId: formValue.threadId ? Number(formValue.threadId) : null,
        message: formValue.message!.trim(),
   };

      if (this.isEditMode) {
        this.updateAuditEvent(auditEventSubmitData);
      } else {
        this.addAuditEvent(auditEventSubmitData);
      }
  }

  private addAuditEvent(auditEventData: AuditEventSubmitData) {
    this.auditEventService.PostAuditEvent(auditEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newAuditEvent) => {

        this.auditEventService.ClearAllCaches();

        this.auditEventChanged.next([newAuditEvent]);

        this.alertService.showMessage("Audit Event added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/auditevent', newAuditEvent.id]);
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
                                   'You do not have permission to save this Audit Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateAuditEvent(auditEventData: AuditEventSubmitData) {
    this.auditEventService.PutAuditEvent(auditEventData.id, auditEventData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedAuditEvent) => {

        this.auditEventService.ClearAllCaches();

        this.auditEventChanged.next([updatedAuditEvent]);

        this.alertService.showMessage("Audit Event updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Audit Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Audit Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Audit Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(auditEventData: AuditEventData | null) {

    if (auditEventData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.auditEventForm.reset({
        startTime: '',
        stopTime: '',
        completedSuccessfully: false,
        auditUserId: null,
        auditSessionId: null,
        auditTypeId: null,
        auditAccessTypeId: null,
        auditSourceId: null,
        auditUserAgentId: null,
        auditModuleId: null,
        auditModuleEntityId: null,
        auditResourceId: null,
        auditHostSystemId: null,
        primaryKey: '',
        threadId: '',
        message: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.auditEventForm.reset({
        startTime: isoUtcStringToDateTimeLocal(auditEventData.startTime) ?? '',
        stopTime: isoUtcStringToDateTimeLocal(auditEventData.stopTime) ?? '',
        completedSuccessfully: auditEventData.completedSuccessfully ?? false,
        auditUserId: auditEventData.auditUserId,
        auditSessionId: auditEventData.auditSessionId,
        auditTypeId: auditEventData.auditTypeId,
        auditAccessTypeId: auditEventData.auditAccessTypeId,
        auditSourceId: auditEventData.auditSourceId,
        auditUserAgentId: auditEventData.auditUserAgentId,
        auditModuleId: auditEventData.auditModuleId,
        auditModuleEntityId: auditEventData.auditModuleEntityId,
        auditResourceId: auditEventData.auditResourceId,
        auditHostSystemId: auditEventData.auditHostSystemId,
        primaryKey: auditEventData.primaryKey ?? '',
        threadId: auditEventData.threadId?.toString() ?? '',
        message: auditEventData.message ?? '',
      }, { emitEvent: false});
    }

    this.auditEventForm.markAsPristine();
    this.auditEventForm.markAsUntouched();
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


  public userIsAuditorAuditEventReader(): boolean {
    return this.auditEventService.userIsAuditorAuditEventReader();
  }

  public userIsAuditorAuditEventWriter(): boolean {
    return this.auditEventService.userIsAuditorAuditEventWriter();
  }
}
