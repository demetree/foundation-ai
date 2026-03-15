/*
   GENERATED FORM FOR THE MENU TABLE - DO NOT MODIFY DIRECTLY
   =================================================================================

   This is the default form generated from Menu table metadata.

   It is useful for low usage worksflows such as basic configuration, but is likely not good enough for primary workflow usage
   because it's form layout and validation is too simple.
   
   For building better looking and/or versions with custom logic, create a custom version of this:

   1. Copy this component
   2. Rename to menu-custom (or similar)
   3. Modify layout, grouping, field types, add workflow logic
   
   This generated version is kept simple on purpose so it's easy to use as a reference/scaffold.

*/
import { Component, ViewChild, Output, Input, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { MenuService, MenuData, MenuSubmitData } from '../../../community-data-services/menu.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { AuthService } from '../../../services/auth.service';

//
// Define a type for the form values to improve readability and type safety.
// This mirrors the structure of the FormGroup controls, with considerations for form input types:
// - Numeric fields like latitude are strings in the form (due to input type="number" behavior).
// - Allows null for optional fields.
// - Does not include navigation properties or methods from domain models.
//
interface MenuFormValues {
};

@Component({
  selector: 'app-menu-add-edit',
  templateUrl: './menu-add-edit.component.html',
  styleUrls: ['./menu-add-edit.component.scss']
})
export class MenuAddEditComponent {
  @ViewChild('menuModal') menuModal!: TemplateRef<any>;
  @Output() menuChanged = new Subject<MenuData[]>();
  @Input() menuSubmitData: MenuSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;


  //
  // Input for pre-seeded data in add mode. This allows the parent component to provide
  // initial values for one or more fields. Use Partial to allow selective seeding.
  // Only applied in add mode (not edit mode, where existing data takes precedence).
  //
  @Input() preSeededData: Partial<MenuFormValues> | null = null;

  //
  // Input for fields to hide. This is an array of field names (e.g., ['name', 'description']).
  // Hiding a field will remove its form group from the template and disable its validator.
  //
  @Input() hiddenFields: string[] = [];


  public menuForm: FormGroup = this.fb.group({
      });

  private modalRef: NgbModalRef | undefined;
  public isEditMode = false;
  public objectGuid: string = "";
  public modalIsDisplayed: boolean = false;

  public isSaving: boolean = false;

  menus$ = this.menuService.GetMenuList();

  constructor(
    private modalService: NgbModal,
    private menuService: MenuService,
    private authService: AuthService,
    private alertService: AlertService,
    private router: Router,
    private fb: FormBuilder) {
  }

  openModal(menuData?: MenuData) {

    if (menuData != null) {

      if (!this.menuService.userIsCommunityMenuReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Menus`,
          '',
          MessageSeverity.info
        );
        return;
      }
      this.menuSubmitData = this.menuService.ConvertToMenuSubmitData(menuData);
      this.isEditMode = true;

      this.buildFormValues(menuData);

    } else {

      if (!this.menuService.userIsCommunityMenuWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Menus`,
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
        this.menuForm.patchValue(this.preSeededData);
      }

    }


    //
    // Disable validators for hidden fields to prevent form invalidation.
    // This prevents requiring values for hidden fields.
    //
    let index: number;

    for (index = 0; index < this.hiddenFields.length; index++) {
      const fieldName = this.hiddenFields[index];
      const control = this.menuForm.get(fieldName);
      if (control !== null) {
        control.clearValidators();
        control.updateValueAndValidity(); // Refresh validation state.
      }
    }

    this.modalRef = this.modalService.open(this.menuModal, {
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

    if (this.menuService.userIsCommunityMenuWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Menus`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.menuForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.menuForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.menuForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const menuSubmitData: MenuSubmitData = {
        id: this.menuSubmitData?.id || 0,
   };

      if (this.isEditMode) {
        this.updateMenu(menuSubmitData);
      } else {
        this.addMenu(menuSubmitData);
      }
  }

  private addMenu(menuData: MenuSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    menuData.active = true;
    menuData.deleted = false;
    this.menuService.PostMenu(menuData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newMenu) => {

        this.menuService.ClearAllCaches();

        this.menuChanged.next([newMenu]);

        this.alertService.showMessage("Menu added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/menu', newMenu.id]);
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
                                   'You do not have permission to save this Menu.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Menu.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Menu could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }


  private updateMenu(menuData: MenuSubmitData) {
    this.menuService.PutMenu(menuData.id, menuData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedMenu) => {

        this.menuService.ClearAllCaches();

        this.menuChanged.next([updatedMenu]);

        this.alertService.showMessage("Menu updated successfully", '', MessageSeverity.success);

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
                                   'You do not have permission to save this Menu.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Menu.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Menu could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(menuData: MenuData | null) {

    if (menuData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.menuForm.reset({
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.menuForm.reset({
      }, { emitEvent: false});
    }

    this.menuForm.markAsPristine();
    this.menuForm.markAsUntouched();
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


  public userIsCommunityMenuReader(): boolean {
    return this.menuService.userIsCommunityMenuReader();
  }

  public userIsCommunityMenuWriter(): boolean {
    return this.menuService.userIsCommunityMenuWriter();
  }
}
