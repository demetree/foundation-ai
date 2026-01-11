/*
   GENERATED FORM FOR THE BOOKINGSOURCETYPE TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from BookingSourceType table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to booking-source-type-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { BookingSourceTypeService, BookingSourceTypeData, BookingSourceTypeSubmitData } from '../../../scheduler-data-services/booking-source-type.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface BookingSourceTypeFormValues {
  name: string,
  description: string,
  color: string | null,
  sequence: string | null,     // Stored as string for form input, converted to number on submit.
  active: boolean,
  deleted: boolean,
};

@Component({
  selector: 'app-booking-source-type-add-edit',
  templateUrl: './booking-source-type-add-edit.component.html',
  styleUrls: ['./booking-source-type-add-edit.component.scss']
})
export class BookingSourceTypeAddEditComponent {
  @ViewChild('bookingSourceTypeModal') bookingSourceTypeModal!: TemplateRef<any>;
  @Output() bookingSourceTypeChanged = new Subject<BookingSourceTypeData[]>();
  @Input() bookingSourceTypeSubmitData: BookingSourceTypeSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<BookingSourceTypeFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public bookingSourceTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        color: [''],
        sequence: [''],
        active: [true],
        deleted: [false],
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  bookingSourceTypes$ = this.bookingSourceTypeService.GetBookingSourceTypeList();

  constructor(
    private modalService: NgbModal,
    private bookingSourceTypeService: BookingSourceTypeService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(bookingSourceTypeData?: BookingSourceTypeData) {

    if (bookingSourceTypeData != null) {

      if (!this.bookingSourceTypeService.userIsSchedulerBookingSourceTypeReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Booking Source Types`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.bookingSourceTypeSubmitData = this.bookingSourceTypeService.ConvertToBookingSourceTypeSubmitData(bookingSourceTypeData);
      this.isEditMode = true;
      this.objectGuid = bookingSourceTypeData.objectGuid;

      this.buildFormValues(bookingSourceTypeData);

    } else {

      if (!this.bookingSourceTypeService.userIsSchedulerBookingSourceTypeWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Booking Source Types`,
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
        this.bookingSourceTypeForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.bookingSourceTypeForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.bookingSourceTypeModal, {
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

    if (this.bookingSourceTypeService.userIsSchedulerBookingSourceTypeWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Booking Source Types`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.bookingSourceTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.bookingSourceTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.bookingSourceTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const bookingSourceTypeSubmitData: BookingSourceTypeSubmitData = {
        id: this.bookingSourceTypeSubmitData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description!.trim(),
        color: formValue.color?.trim() || null,
        sequence: formValue.sequence ? Number(formValue.sequence) : null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };

      if (this.isEditMode) {
        this.updateBookingSourceType(bookingSourceTypeSubmitData);
      } else {
        this.addBookingSourceType(bookingSourceTypeSubmitData);
      }
  }

  private addBookingSourceType(bookingSourceTypeData: BookingSourceTypeSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    bookingSourceTypeData.active = true;
    bookingSourceTypeData.deleted = false;
    this.bookingSourceTypeService.PostBookingSourceType(bookingSourceTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newBookingSourceType) => {

        this.bookingSourceTypeService.ClearAllCaches();

        this.bookingSourceTypeChanged.next([newBookingSourceType]);

        this.alertService.showMessage("Booking Source Type added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/bookingsourcetype', newBookingSourceType.id]);
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
                                   'You do not have permission to save this Booking Source Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Booking Source Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Booking Source Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateBookingSourceType(bookingSourceTypeData: BookingSourceTypeSubmitData) {
    this.bookingSourceTypeService.PutBookingSourceType(bookingSourceTypeData.id, bookingSourceTypeData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedBookingSourceType) => {

        this.bookingSourceTypeService.ClearAllCaches();

        this.bookingSourceTypeChanged.next([updatedBookingSourceType]);

        this.alertService.showMessage("Booking Source Type updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Booking Source Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Booking Source Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Booking Source Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(bookingSourceTypeData: BookingSourceTypeData | null) {

    if (bookingSourceTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.bookingSourceTypeForm.reset({
        name: '',
        description: '',
        color: '',
        sequence: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.bookingSourceTypeForm.reset({
        name: bookingSourceTypeData.name ?? '',
        description: bookingSourceTypeData.description ?? '',
        color: bookingSourceTypeData.color ?? '',
        sequence: bookingSourceTypeData.sequence?.toString() ?? '',
        active: bookingSourceTypeData.active ?? true,
        deleted: bookingSourceTypeData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.bookingSourceTypeForm.markAsPristine();
    this.bookingSourceTypeForm.markAsUntouched();
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


  public userIsSchedulerBookingSourceTypeReader(): boolean {
    return this.bookingSourceTypeService.userIsSchedulerBookingSourceTypeReader();
  }

  public userIsSchedulerBookingSourceTypeWriter(): boolean {
    return this.bookingSourceTypeService.userIsSchedulerBookingSourceTypeWriter();
  }
}
