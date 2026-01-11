/*
   GENERATED FORM FOR THE CONSTITUENT TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Constituent table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to constituent-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConstituentService, ConstituentData, ConstituentSubmitData } from '../../../scheduler-data-services/constituent.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { ContactService } from '../../../scheduler-data-services/contact.service';
import { ClientService } from '../../../scheduler-data-services/client.service';
import { HouseholdService } from '../../../scheduler-data-services/household.service';
import { ConstituentJourneyStageService } from '../../../scheduler-data-services/constituent-journey-stage.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface ConstituentFormValues {
  contactId: number | bigint | null,       // For FK link number
  clientId: number | bigint | null,       // For FK link number
  householdId: number | bigint | null,       // For FK link number
  constituentNumber: string,
  doNotSolicit: boolean,
  doNotEmail: boolean,
  doNotMail: boolean,
  totalLifetimeGiving: string,     // Stored as string for form input, converted to number on submit.
  totalYTDGiving: string,     // Stored as string for form input, converted to number on submit.
  lastGiftDate: string | null,
  lastGiftAmount: string | null,     // Stored as string for form input, converted to number on submit.
  largestGiftAmount: string | null,     // Stored as string for form input, converted to number on submit.
  totalGiftCount: string | null,     // Stored as string for form input, converted to number on submit.
  externalId: string | null,
  notes: string | null,
  constituentJourneyStageId: number | bigint | null,       // For FK link number
  dateEnteredCurrentStage: string | null,
  attributes: string | null,
  iconId: number | bigint | null,       // For FK link number
  color: string | null,
  avatarFileName: string | null,
  avatarSize: string | null,     // Stored as string for form input, converted to number on submit.
  avatarData: string | null,
  avatarMimeType: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-constituent-add-edit',
  templateUrl: './constituent-add-edit.component.html',
  styleUrls: ['./constituent-add-edit.component.scss']
})
export class ConstituentAddEditComponent {
  @ViewChild('constituentModal') constituentModal!: TemplateRef<any>;
  @Output() constituentChanged = new Subject<ConstituentData[]>();
  @Input() constituentSubmitData: ConstituentSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<ConstituentFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public constituentForm: FormGroup = this.fb.group({
        contactId: [null],
        clientId: [null],
        householdId: [null],
        constituentNumber: ['', Validators.required],
        doNotSolicit: [false],
        doNotEmail: [false],
        doNotMail: [false],
        totalLifetimeGiving: ['', Validators.required],
        totalYTDGiving: ['', Validators.required],
        lastGiftDate: [''],
        lastGiftAmount: [''],
        largestGiftAmount: [''],
        totalGiftCount: [''],
        externalId: [''],
        notes: [''],
        constituentJourneyStageId: [null],
        dateEnteredCurrentStage: [''],
        attributes: [''],
        iconId: [null],
        color: [''],
        avatarFileName: [''],
        avatarSize: [''],
        avatarData: [''],
        avatarMimeType: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  constituents$ = this.constituentService.GetConstituentList();
  contacts$ = this.contactService.GetContactList();
  clients$ = this.clientService.GetClientList();
  households$ = this.householdService.GetHouseholdList();
  constituentJourneyStages$ = this.constituentJourneyStageService.GetConstituentJourneyStageList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private constituentService: ConstituentService,
    private contactService: ContactService,
    private clientService: ClientService,
    private householdService: HouseholdService,
    private constituentJourneyStageService: ConstituentJourneyStageService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(constituentData?: ConstituentData) {

    if (constituentData != null) {

      if (!this.constituentService.userIsSchedulerConstituentReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Constituents`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.constituentSubmitData = this.constituentService.ConvertToConstituentSubmitData(constituentData);
      this.isEditMode = true;
      this.objectGuid = constituentData.objectGuid;

      this.buildFormValues(constituentData);

    } else {

      if (!this.constituentService.userIsSchedulerConstituentWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Constituents`,
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
        this.constituentForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.constituentForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.constituentModal, {
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

    if (this.constituentService.userIsSchedulerConstituentWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Constituents`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.constituentForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.constituentForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.constituentForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const constituentSubmitData: ConstituentSubmitData = {
        id: this.constituentSubmitData?.id || 0,
        contactId: formValue.contactId ? Number(formValue.contactId) : null,
        clientId: formValue.clientId ? Number(formValue.clientId) : null,
        householdId: formValue.householdId ? Number(formValue.householdId) : null,
        constituentNumber: formValue.constituentNumber!.trim(),
        doNotSolicit: !!formValue.doNotSolicit,
        doNotEmail: !!formValue.doNotEmail,
        doNotMail: !!formValue.doNotMail,
        totalLifetimeGiving: Number(formValue.totalLifetimeGiving),
        totalYTDGiving: Number(formValue.totalYTDGiving),
        lastGiftDate: formValue.lastGiftDate?.trim() || null,
        lastGiftAmount: formValue.lastGiftAmount ? Number(formValue.lastGiftAmount) : null,
        largestGiftAmount: formValue.largestGiftAmount ? Number(formValue.largestGiftAmount) : null,
        totalGiftCount: formValue.totalGiftCount ? Number(formValue.totalGiftCount) : null,
        externalId: formValue.externalId?.trim() || null,
        notes: formValue.notes?.trim() || null,
        constituentJourneyStageId: formValue.constituentJourneyStageId ? Number(formValue.constituentJourneyStageId) : null,
        dateEnteredCurrentStage: formValue.dateEnteredCurrentStage ? dateTimeLocalToIsoUtc(formValue.dateEnteredCurrentStage.trim()) : null,
        attributes: formValue.attributes?.trim() || null,
        iconId: formValue.iconId ? Number(formValue.iconId) : null,
        color: formValue.color?.trim() || null,
        avatarFileName: formValue.avatarFileName?.trim() || null,
        avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
        avatarData: formValue.avatarData?.trim() || null,
        avatarMimeType: formValue.avatarMimeType?.trim() || null,
        versionNumber: this.constituentSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateConstituent(constituentSubmitData);
      } else {
        this.addConstituent(constituentSubmitData);
      }
  }

  private addConstituent(constituentData: ConstituentSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    constituentData.versionNumber = 0;
    constituentData.active = true;
    constituentData.deleted = false;
    this.constituentService.PostConstituent(constituentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newConstituent) => {

        this.constituentService.ClearAllCaches();

        this.constituentChanged.next([newConstituent]);

        this.alertService.showMessage("Constituent added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/constituent', newConstituent.id]);
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
                                   'You do not have permission to save this Constituent.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Constituent.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Constituent could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateConstituent(constituentData: ConstituentSubmitData) {
    this.constituentService.PutConstituent(constituentData.id, constituentData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedConstituent) => {

        this.constituentService.ClearAllCaches();

        this.constituentChanged.next([updatedConstituent]);

        this.alertService.showMessage("Constituent updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Constituent.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Constituent.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Constituent could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(constituentData: ConstituentData | null) {

    if (constituentData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.constituentForm.reset({
        contactId: null,
        clientId: null,
        householdId: null,
        constituentNumber: '',
        doNotSolicit: false,
        doNotEmail: false,
        doNotMail: false,
        totalLifetimeGiving: '',
        totalYTDGiving: '',
        lastGiftDate: '',
        lastGiftAmount: '',
        largestGiftAmount: '',
        totalGiftCount: '',
        externalId: '',
        notes: '',
        constituentJourneyStageId: null,
        dateEnteredCurrentStage: '',
        attributes: '',
        iconId: null,
        color: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.constituentForm.reset({
        contactId: constituentData.contactId,
        clientId: constituentData.clientId,
        householdId: constituentData.householdId,
        constituentNumber: constituentData.constituentNumber ?? '',
        doNotSolicit: constituentData.doNotSolicit ?? false,
        doNotEmail: constituentData.doNotEmail ?? false,
        doNotMail: constituentData.doNotMail ?? false,
        totalLifetimeGiving: constituentData.totalLifetimeGiving?.toString() ?? '',
        totalYTDGiving: constituentData.totalYTDGiving?.toString() ?? '',
        lastGiftDate: constituentData.lastGiftDate ?? '',
        lastGiftAmount: constituentData.lastGiftAmount?.toString() ?? '',
        largestGiftAmount: constituentData.largestGiftAmount?.toString() ?? '',
        totalGiftCount: constituentData.totalGiftCount?.toString() ?? '',
        externalId: constituentData.externalId ?? '',
        notes: constituentData.notes ?? '',
        constituentJourneyStageId: constituentData.constituentJourneyStageId,
        dateEnteredCurrentStage: isoUtcStringToDateTimeLocal(constituentData.dateEnteredCurrentStage) ?? '',
        attributes: constituentData.attributes ?? '',
        iconId: constituentData.iconId,
        color: constituentData.color ?? '',
        avatarFileName: constituentData.avatarFileName ?? '',
        avatarSize: constituentData.avatarSize?.toString() ?? '',
        avatarData: constituentData.avatarData ?? '',
        avatarMimeType: constituentData.avatarMimeType ?? '',
        versionNumber: constituentData.versionNumber?.toString() ?? '',
        active: constituentData.active ?? true,
        deleted: constituentData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.constituentForm.markAsPristine();
    this.constituentForm.markAsUntouched();
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


  public userIsSchedulerConstituentReader(): boolean {
    return this.constituentService.userIsSchedulerConstituentReader();
  }

  public userIsSchedulerConstituentWriter(): boolean {
    return this.constituentService.userIsSchedulerConstituentWriter();
  }
}
