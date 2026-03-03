/*
   GENERATED FORM FOR THE BRICKSETSETREVIEW TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BrickSetSetReview table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to brick-set-set-review-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BrickSetSetReviewService, BrickSetSetReviewData, BrickSetSetReviewSubmitData } from '../../../bmc-data-services/brick-set-set-review.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { LegoSetService } from '../../../bmc-data-services/lego-set.service';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BrickSetSetReviewFormValues {
  legoSetId: number | bigint,       // For FK link number
  reviewAuthor: string,
  reviewDate: string | null,
  reviewTitle: string | null,
  reviewBody: string | null,
  overallRating: string | null,     // Stored as string for form input, converted to number on submit.
  buildingExperienceRating: string | null,     // Stored as string for form input, converted to number on submit.
  valueForMoneyRating: string | null,     // Stored as string for form input, converted to number on submit.
  partsRating: string | null,     // Stored as string for form input, converted to number on submit.
  playabilityRating: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-brick-set-set-review-add-edit',
  templateUrl: './brick-set-set-review-add-edit.component.html',
  styleUrls: ['./brick-set-set-review-add-edit.component.scss']
})
export class BrickSetSetReviewAddEditComponent {
  @ViewChild('brickSetSetReviewModal') brickSetSetReviewModal!: TemplateRef<any>;
  @Output() brickSetSetReviewChanged = new Subject<BrickSetSetReviewData[]>();
  @Input() brickSetSetReviewSubmitData: BrickSetSetReviewSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BrickSetSetReviewFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public brickSetSetReviewForm: FormGroup = this.fb.group({
        legoSetId: [null, Validators.required],
        reviewAuthor: ['', Validators.required],
        reviewDate: [''],
        reviewTitle: [''],
        reviewBody: [''],
        overallRating: [''],
        buildingExperienceRating: [''],
        valueForMoneyRating: [''],
        partsRating: [''],
        playabilityRating: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  brickSetSetReviews$ = this.brickSetSetReviewService.GetBrickSetSetReviewList();
  legoSets$ = this.legoSetService.GetLegoSetList();

  constructor(
    private modalService: NgbModal,
    private brickSetSetReviewService: BrickSetSetReviewService,
    private legoSetService: LegoSetService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(brickSetSetReviewData?: BrickSetSetReviewData) {

    if (brickSetSetReviewData != null) {

      if (!this.brickSetSetReviewService.userIsBMCBrickSetSetReviewReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Brick Set Set Reviews`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.brickSetSetReviewSubmitData = this.brickSetSetReviewService.ConvertToBrickSetSetReviewSubmitData(brickSetSetReviewData);
      this.isEditMode = true;
      this.objectGuid = brickSetSetReviewData.objectGuid;

      this.buildFormValues(brickSetSetReviewData);

    } else {

      if (!this.brickSetSetReviewService.userIsBMCBrickSetSetReviewWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Brick Set Set Reviews`,
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
        this.brickSetSetReviewForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.brickSetSetReviewForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.brickSetSetReviewModal, {
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

    if (this.brickSetSetReviewService.userIsBMCBrickSetSetReviewWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Brick Set Set Reviews`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.brickSetSetReviewForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.brickSetSetReviewForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.brickSetSetReviewForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const brickSetSetReviewSubmitData: BrickSetSetReviewSubmitData = {
        id: this.brickSetSetReviewSubmitData?.id || 0,
        legoSetId: Number(formValue.legoSetId),
        reviewAuthor: formValue.reviewAuthor!.trim(),
        reviewDate: formValue.reviewDate ? dateTimeLocalToIsoUtc(formValue.reviewDate.trim()) : null,
        reviewTitle: formValue.reviewTitle?.trim() || null,
        reviewBody: formValue.reviewBody?.trim() || null,
        overallRating: formValue.overallRating ? Number(formValue.overallRating) : null,
        buildingExperienceRating: formValue.buildingExperienceRating ? Number(formValue.buildingExperienceRating) : null,
        valueForMoneyRating: formValue.valueForMoneyRating ? Number(formValue.valueForMoneyRating) : null,
        partsRating: formValue.partsRating ? Number(formValue.partsRating) : null,
        playabilityRating: formValue.playabilityRating ? Number(formValue.playabilityRating) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBrickSetSetReview(brickSetSetReviewSubmitData);
      } else {
        this.addBrickSetSetReview(brickSetSetReviewSubmitData);
      }
  }

  private addBrickSetSetReview(brickSetSetReviewData: BrickSetSetReviewSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    brickSetSetReviewData.active = true;
    brickSetSetReviewData.deleted = false;
    this.brickSetSetReviewService.PostBrickSetSetReview(brickSetSetReviewData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBrickSetSetReview) => {

        this.brickSetSetReviewService.ClearAllCaches();

        this.brickSetSetReviewChanged.next([newBrickSetSetReview]);

        this.alertService.showMessage("Brick Set Set Review added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/bricksetsetreview', newBrickSetSetReview.id]);
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
                                   'You do not have permission to save this Brick Set Set Review.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Set Set Review.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Set Set Review could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBrickSetSetReview(brickSetSetReviewData: BrickSetSetReviewSubmitData) {
    this.brickSetSetReviewService.PutBrickSetSetReview(brickSetSetReviewData.id, brickSetSetReviewData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBrickSetSetReview) => {

        this.brickSetSetReviewService.ClearAllCaches();

        this.brickSetSetReviewChanged.next([updatedBrickSetSetReview]);

        this.alertService.showMessage("Brick Set Set Review updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Brick Set Set Review.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Brick Set Set Review.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Brick Set Set Review could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(brickSetSetReviewData: BrickSetSetReviewData | null) {

    if (brickSetSetReviewData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.brickSetSetReviewForm.reset({
        legoSetId: null,
        reviewAuthor: '',
        reviewDate: '',
        reviewTitle: '',
        reviewBody: '',
        overallRating: '',
        buildingExperienceRating: '',
        valueForMoneyRating: '',
        partsRating: '',
        playabilityRating: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.brickSetSetReviewForm.reset({
        legoSetId: brickSetSetReviewData.legoSetId,
        reviewAuthor: brickSetSetReviewData.reviewAuthor ?? '',
        reviewDate: isoUtcStringToDateTimeLocal(brickSetSetReviewData.reviewDate) ?? '',
        reviewTitle: brickSetSetReviewData.reviewTitle ?? '',
        reviewBody: brickSetSetReviewData.reviewBody ?? '',
        overallRating: brickSetSetReviewData.overallRating?.toString() ?? '',
        buildingExperienceRating: brickSetSetReviewData.buildingExperienceRating?.toString() ?? '',
        valueForMoneyRating: brickSetSetReviewData.valueForMoneyRating?.toString() ?? '',
        partsRating: brickSetSetReviewData.partsRating?.toString() ?? '',
        playabilityRating: brickSetSetReviewData.playabilityRating?.toString() ?? '',
        active: brickSetSetReviewData.active ?? true,
        deleted: brickSetSetReviewData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.brickSetSetReviewForm.markAsPristine();
    this.brickSetSetReviewForm.markAsUntouched();
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


  public userIsBMCBrickSetSetReviewReader(): boolean {
    return this.brickSetSetReviewService.userIsBMCBrickSetSetReviewReader();
  }

  public userIsBMCBrickSetSetReviewWriter(): boolean {
    return this.brickSetSetReviewService.userIsBMCBrickSetSetReviewWriter();
  }
}
