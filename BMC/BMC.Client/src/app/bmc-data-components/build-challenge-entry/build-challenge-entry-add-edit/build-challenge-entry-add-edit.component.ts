/*
   GENERATED FORM FOR THE BUILDCHALLENGEENTRY TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BuildChallengeEntry table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to build-challenge-entry-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BuildChallengeEntryService, BuildChallengeEntryData, BuildChallengeEntrySubmitData } from '../../../bmc-data-services/build-challenge-entry.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { BuildChallengeService } from '../../../bmc-data-services/build-challenge.service';
import { PublishedMocService } from '../../../bmc-data-services/published-moc.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BuildChallengeEntryFormValues {
  buildChallengeId: number | bigint,       // For FK link number
  publishedMocId: number | bigint,       // For FK link number
  submittedDate: string,
  entryNotes: string | null,
  voteCount: string,     // Stored as string for form input, converted to number on submit.
  isWinner: boolean,
  isDisqualified: boolean,
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-build-challenge-entry-add-edit',
  templateUrl: './build-challenge-entry-add-edit.component.html',
  styleUrls: ['./build-challenge-entry-add-edit.component.scss']
})
export class BuildChallengeEntryAddEditComponent {
  @ViewChild('buildChallengeEntryModal') buildChallengeEntryModal!: TemplateRef<any>;
  @Output() buildChallengeEntryChanged = new Subject<BuildChallengeEntryData[]>();
  @Input() buildChallengeEntrySubmitData: BuildChallengeEntrySubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BuildChallengeEntryFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public buildChallengeEntryForm: FormGroup = this.fb.group({
        buildChallengeId: [null, Validators.required],
        publishedMocId: [null, Validators.required],
        submittedDate: ['', Validators.required],
        entryNotes: [''],
        voteCount: ['', Validators.required],
        isWinner: [false],
        isDisqualified: [false],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  buildChallengeEntries$ = this.buildChallengeEntryService.GetBuildChallengeEntryList();
  buildChallenges$ = this.buildChallengeService.GetBuildChallengeList();
  publishedMocs$ = this.publishedMocService.GetPublishedMocList();

  constructor(
    private modalService: NgbModal,
    private buildChallengeEntryService: BuildChallengeEntryService,
    private buildChallengeService: BuildChallengeService,
    private publishedMocService: PublishedMocService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(buildChallengeEntryData?: BuildChallengeEntryData) {

    if (buildChallengeEntryData != null) {

      if (!this.buildChallengeEntryService.userIsBMCBuildChallengeEntryReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Build Challenge Entries`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.buildChallengeEntrySubmitData = this.buildChallengeEntryService.ConvertToBuildChallengeEntrySubmitData(buildChallengeEntryData);
      this.isEditMode = true;
      this.objectGuid = buildChallengeEntryData.objectGuid;

      this.buildFormValues(buildChallengeEntryData);

    } else {

      if (!this.buildChallengeEntryService.userIsBMCBuildChallengeEntryWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Build Challenge Entries`,
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
        this.buildChallengeEntryForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.buildChallengeEntryForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.buildChallengeEntryModal, {
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

    if (this.buildChallengeEntryService.userIsBMCBuildChallengeEntryWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Build Challenge Entries`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.buildChallengeEntryForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.buildChallengeEntryForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.buildChallengeEntryForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const buildChallengeEntrySubmitData: BuildChallengeEntrySubmitData = {
        id: this.buildChallengeEntrySubmitData?.id || 0,
        buildChallengeId: Number(formValue.buildChallengeId),
        publishedMocId: Number(formValue.publishedMocId),
        submittedDate: dateTimeLocalToIsoUtc(formValue.submittedDate!.trim())!,
        entryNotes: formValue.entryNotes?.trim() || null,
        voteCount: Number(formValue.voteCount),
        isWinner: !!formValue.isWinner,
        isDisqualified: !!formValue.isDisqualified,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBuildChallengeEntry(buildChallengeEntrySubmitData);
      } else {
        this.addBuildChallengeEntry(buildChallengeEntrySubmitData);
      }
  }

  private addBuildChallengeEntry(buildChallengeEntryData: BuildChallengeEntrySubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    buildChallengeEntryData.active = true;
    buildChallengeEntryData.deleted = false;
    this.buildChallengeEntryService.PostBuildChallengeEntry(buildChallengeEntryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBuildChallengeEntry) => {

        this.buildChallengeEntryService.ClearAllCaches();

        this.buildChallengeEntryChanged.next([newBuildChallengeEntry]);

        this.alertService.showMessage("Build Challenge Entry added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/buildchallengeentry', newBuildChallengeEntry.id]);
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
                                   'You do not have permission to save this Build Challenge Entry.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Challenge Entry.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Challenge Entry could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBuildChallengeEntry(buildChallengeEntryData: BuildChallengeEntrySubmitData) {
    this.buildChallengeEntryService.PutBuildChallengeEntry(buildChallengeEntryData.id, buildChallengeEntryData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBuildChallengeEntry) => {

        this.buildChallengeEntryService.ClearAllCaches();

        this.buildChallengeEntryChanged.next([updatedBuildChallengeEntry]);

        this.alertService.showMessage("Build Challenge Entry updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Build Challenge Entry.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Build Challenge Entry.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Build Challenge Entry could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(buildChallengeEntryData: BuildChallengeEntryData | null) {

    if (buildChallengeEntryData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.buildChallengeEntryForm.reset({
        buildChallengeId: null,
        publishedMocId: null,
        submittedDate: '',
        entryNotes: '',
        voteCount: '',
        isWinner: false,
        isDisqualified: false,
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.buildChallengeEntryForm.reset({
        buildChallengeId: buildChallengeEntryData.buildChallengeId,
        publishedMocId: buildChallengeEntryData.publishedMocId,
        submittedDate: isoUtcStringToDateTimeLocal(buildChallengeEntryData.submittedDate) ?? '',
        entryNotes: buildChallengeEntryData.entryNotes ?? '',
        voteCount: buildChallengeEntryData.voteCount?.toString() ?? '',
        isWinner: buildChallengeEntryData.isWinner ?? false,
        isDisqualified: buildChallengeEntryData.isDisqualified ?? false,
        active: buildChallengeEntryData.active ?? true,
        deleted: buildChallengeEntryData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.buildChallengeEntryForm.markAsPristine();
    this.buildChallengeEntryForm.markAsUntouched();
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


  public userIsBMCBuildChallengeEntryReader(): boolean {
    return this.buildChallengeEntryService.userIsBMCBuildChallengeEntryReader();
  }

  public userIsBMCBuildChallengeEntryWriter(): boolean {
    return this.buildChallengeEntryService.userIsBMCBuildChallengeEntryWriter();
  }
}
