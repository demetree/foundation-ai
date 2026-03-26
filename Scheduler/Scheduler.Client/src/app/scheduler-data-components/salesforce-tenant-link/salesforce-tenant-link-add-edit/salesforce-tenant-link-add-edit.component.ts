/*
   GENERATED FORM FOR THE SALESFORCETENANTLINK TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from SalesforceTenantLink table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to salesforce-tenant-link-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SalesforceTenantLinkService, SalesforceTenantLinkData, SalesforceTenantLinkSubmitData } from '../../../scheduler-data-services/salesforce-tenant-link.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface SalesforceTenantLinkFormValues {
  syncEnabled: boolean,
  syncDirectionFlags: string | null,
  pullIntervalMinutes: string | null,     // Stored as string for form input, converted to number on submit.
  lastPullDate: string | null,
  loginUrl: string | null,
  sfClientId: string | null,
  sfClientSecret: string | null,
  sfUsername: string | null,
  sfPassword: string | null,
  sfSecurityToken: string | null,
  instanceUrl: string | null,
  apiVersion: string | null,
  versionNumber: string,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-salesforce-tenant-link-add-edit',
  templateUrl: './salesforce-tenant-link-add-edit.component.html',
  styleUrls: ['./salesforce-tenant-link-add-edit.component.scss']
})
export class SalesforceTenantLinkAddEditComponent {
  @ViewChild('salesforceTenantLinkModal') salesforceTenantLinkModal!: TemplateRef<any>;
  @Output() salesforceTenantLinkChanged = new Subject<SalesforceTenantLinkData[]>();
  @Input() salesforceTenantLinkSubmitData: SalesforceTenantLinkSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<SalesforceTenantLinkFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public salesforceTenantLinkForm: FormGroup = this.fb.group({
        syncEnabled: [false],
        syncDirectionFlags: [''],
        pullIntervalMinutes: [''],
        lastPullDate: [''],
        loginUrl: [''],
        sfClientId: [''],
        sfClientSecret: [''],
        sfUsername: [''],
        sfPassword: [''],
        sfSecurityToken: [''],
        instanceUrl: [''],
        apiVersion: [''],
        versionNumber: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  salesforceTenantLinks$ = this.salesforceTenantLinkService.GetSalesforceTenantLinkList();

  constructor(
    private modalService: NgbModal,
    private salesforceTenantLinkService: SalesforceTenantLinkService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(salesforceTenantLinkData?: SalesforceTenantLinkData) {

    if (salesforceTenantLinkData != null) {

      if (!this.salesforceTenantLinkService.userIsSchedulerSalesforceTenantLinkReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Salesforce Tenant Links`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.salesforceTenantLinkSubmitData = this.salesforceTenantLinkService.ConvertToSalesforceTenantLinkSubmitData(salesforceTenantLinkData);
      this.isEditMode = true;
      this.objectGuid = salesforceTenantLinkData.objectGuid;

      this.buildFormValues(salesforceTenantLinkData);

    } else {

      if (!this.salesforceTenantLinkService.userIsSchedulerSalesforceTenantLinkWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Salesforce Tenant Links`,
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
        this.salesforceTenantLinkForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.salesforceTenantLinkForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.salesforceTenantLinkModal, {
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

    if (this.salesforceTenantLinkService.userIsSchedulerSalesforceTenantLinkWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Salesforce Tenant Links`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.salesforceTenantLinkForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.salesforceTenantLinkForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.salesforceTenantLinkForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const salesforceTenantLinkSubmitData: SalesforceTenantLinkSubmitData = {
        id: this.salesforceTenantLinkSubmitData?.id || 0,
        syncEnabled: !!formValue.syncEnabled,
        syncDirectionFlags: formValue.syncDirectionFlags?.trim() || null,
        pullIntervalMinutes: formValue.pullIntervalMinutes ? Number(formValue.pullIntervalMinutes) : null,
        lastPullDate: formValue.lastPullDate ? dateTimeLocalToIsoUtc(formValue.lastPullDate.trim()) : null,
        loginUrl: formValue.loginUrl?.trim() || null,
        sfClientId: formValue.sfClientId?.trim() || null,
        sfClientSecret: formValue.sfClientSecret?.trim() || null,
        sfUsername: formValue.sfUsername?.trim() || null,
        sfPassword: formValue.sfPassword?.trim() || null,
        sfSecurityToken: formValue.sfSecurityToken?.trim() || null,
        instanceUrl: formValue.instanceUrl?.trim() || null,
        apiVersion: formValue.apiVersion?.trim() || null,
        versionNumber: this.salesforceTenantLinkSubmitData?.versionNumber ?? 0,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateSalesforceTenantLink(salesforceTenantLinkSubmitData);
      } else {
        this.addSalesforceTenantLink(salesforceTenantLinkSubmitData);
      }
  }

  private addSalesforceTenantLink(salesforceTenantLinkData: SalesforceTenantLinkSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    salesforceTenantLinkData.versionNumber = 0;
    salesforceTenantLinkData.active = true;
    salesforceTenantLinkData.deleted = false;
    this.salesforceTenantLinkService.PostSalesforceTenantLink(salesforceTenantLinkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newSalesforceTenantLink) => {

        this.salesforceTenantLinkService.ClearAllCaches();

        this.salesforceTenantLinkChanged.next([newSalesforceTenantLink]);

        this.alertService.showMessage("Salesforce Tenant Link added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/salesforcetenantlink', newSalesforceTenantLink.id]);
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
                                   'You do not have permission to save this Salesforce Tenant Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Salesforce Tenant Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Salesforce Tenant Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateSalesforceTenantLink(salesforceTenantLinkData: SalesforceTenantLinkSubmitData) {
    this.salesforceTenantLinkService.PutSalesforceTenantLink(salesforceTenantLinkData.id, salesforceTenantLinkData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedSalesforceTenantLink) => {

        this.salesforceTenantLinkService.ClearAllCaches();

        this.salesforceTenantLinkChanged.next([updatedSalesforceTenantLink]);

        this.alertService.showMessage("Salesforce Tenant Link updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Salesforce Tenant Link.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Salesforce Tenant Link.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Salesforce Tenant Link could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(salesforceTenantLinkData: SalesforceTenantLinkData | null) {

    if (salesforceTenantLinkData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.salesforceTenantLinkForm.reset({
        syncEnabled: false,
        syncDirectionFlags: '',
        pullIntervalMinutes: '',
        lastPullDate: '',
        loginUrl: '',
        sfClientId: '',
        sfClientSecret: '',
        sfUsername: '',
        sfPassword: '',
        sfSecurityToken: '',
        instanceUrl: '',
        apiVersion: '',
        versionNumber: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.salesforceTenantLinkForm.reset({
        syncEnabled: salesforceTenantLinkData.syncEnabled ?? false,
        syncDirectionFlags: salesforceTenantLinkData.syncDirectionFlags ?? '',
        pullIntervalMinutes: salesforceTenantLinkData.pullIntervalMinutes?.toString() ?? '',
        lastPullDate: isoUtcStringToDateTimeLocal(salesforceTenantLinkData.lastPullDate) ?? '',
        loginUrl: salesforceTenantLinkData.loginUrl ?? '',
        sfClientId: salesforceTenantLinkData.sfClientId ?? '',
        sfClientSecret: salesforceTenantLinkData.sfClientSecret ?? '',
        sfUsername: salesforceTenantLinkData.sfUsername ?? '',
        sfPassword: salesforceTenantLinkData.sfPassword ?? '',
        sfSecurityToken: salesforceTenantLinkData.sfSecurityToken ?? '',
        instanceUrl: salesforceTenantLinkData.instanceUrl ?? '',
        apiVersion: salesforceTenantLinkData.apiVersion ?? '',
        versionNumber: salesforceTenantLinkData.versionNumber?.toString() ?? '',
        active: salesforceTenantLinkData.active ?? true,
        deleted: salesforceTenantLinkData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.salesforceTenantLinkForm.markAsPristine();
    this.salesforceTenantLinkForm.markAsUntouched();
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


  public userIsSchedulerSalesforceTenantLinkReader(): boolean {
    return this.salesforceTenantLinkService.userIsSchedulerSalesforceTenantLinkReader();
  }

  public userIsSchedulerSalesforceTenantLinkWriter(): boolean {
    return this.salesforceTenantLinkService.userIsSchedulerSalesforceTenantLinkWriter();
  }
}
