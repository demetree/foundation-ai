import { Component, ViewChild, Output, Input, TemplateRef, SimpleChanges } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject, finalize } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { CrewService, CrewData, CrewSubmitData } from '../../../scheduler-data-services/crew.service';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { AuthService } from '../../../services/auth.service';
import { CurrentUserService } from '../../../services/current-user.service';

@Component({
  selector: 'app-crew-custom-add-edit',
  templateUrl: './crew-custom-add-edit.component.html',
  styleUrls: ['./crew-custom-add-edit.component.scss'],
  animations: [
    trigger('collapse', [
      state('false', style({ height: '0', overflow: 'hidden', opacity: 0 })),
      state('true', style({ height: '*', opacity: 1 })),
      transition('false <=> true', animate('300ms ease-in-out'))
    ])
  ]
})
export class CrewCustomAddEditComponent {
  @ViewChild('crewModal') crewModal!: TemplateRef<any>;
  @Output() crewChanged = new Subject<CrewData[]>();
  @Input() crewSubmitData: CrewSubmitData | null = null;
  @Input() navigateToDetailsAfterAdd: boolean = true;
  @Input() showAddButton: boolean = true;
  @Input() officeId: number | null = null;      // Provide this as input if you want to pre-seed the office - such as when adding a crew from the office crew list.

  public crewData: CrewData | null = null;

  public currentAvatarUrl: string | null = null;
  public isAvatarPanelOpen = false;
  public isDragOver = false;

  // To get the count of offices to allow the offices button to be invisible if there are no offices (It can always be found under Administration)
  public officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;


  crewForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    notes: [''],
    officeId: [this.officeId ?? this.currentUserService.defaultOfficeId],
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

  crews$ = this.crewService.GetCrewList();
  offices$ = this.officeService.GetOfficeList();
  icons$ = this.iconService.GetIconList();

  constructor(
    private modalService: NgbModal,
    private crewService: CrewService,
    private officeService: OfficeService,
    private schedulerHelperService: SchedulerHelperService,
    private iconService: IconService,
    private authService: AuthService,
    private alertService: AlertService,
    private currentUserService: CurrentUserService,
    private router: Router,
    private fb: FormBuilder) {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['officeId'] ) {

      if (this.crewData != null) {
        this.crewData.officeId = this.officeId as number;
      }

      this.crewForm.patchValue({
        officeId: this.officeId
      });
    }
  }

  openModal(crewData?: CrewData) {

    if (crewData != null) {

      if (!this.crewService.userIsSchedulerCrewReader()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to read Crews`,
          '',
          MessageSeverity.info
        );
        return;
      }

      this.crewData = crewData;
      this.crewSubmitData = this.crewService.ConvertToCrewSubmitData(crewData);
      this.isEditMode = true;
      this.objectGuid = crewData.objectGuid;

      this.buildFormValues(crewData);

    } else {

      if (!this.crewService.userIsSchedulerCrewWriter()) {
        this.alertService.showMessage(
          `${this.authService.currentUser?.userName} does not have permission to write Crews`,
          '',
          MessageSeverity.info
        );
        return;

      }

      this.isEditMode = false;

      this.crewData = null;
      this.buildFormValues(null);
    }

    this.modalRef = this.modalService.open(this.crewModal, {
      size: 'xl',
      scrollable: true,
      backdrop: 'static',
      keyboard: true,
      windowClass: 'custom-modal'
    });
    this.modalIsDisplayed = true;
  }



  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (!files || files.length === 0) return;

    const file = files[0];
    if (!file.type.startsWith('image/')) {
      this.alertService.showMessage('Invalid file type', 'Please drop an image file', MessageSeverity.warn);
      return;
    }

    // Reuse existing logic — simulate file input change
    const fakeEvent = { target: { files: [file] } } as any;
    this.onAvatarSelected(fakeEvent);
  }


  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];

    // Enforce 2MB limit
    if (file.size > 2 * 1024 * 1024) {
      this.alertService.showMessage(
        'Image too large',
        'Please select an image under 2MB',
        MessageSeverity.warn
      );
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const result = e.target?.result as string;

      if (!result) return;

      // Extract only the base64 part (remove data:image/png;base64, prefix)
      const base64Data = result.split(',')[1];

      if (!base64Data) {
        this.alertService.showMessage('Invalid image data', '', MessageSeverity.error);
        return;
      }

      this.currentAvatarUrl = result; // Full data URL for preview (includes prefix)

      // Populate form fields
      this.crewForm.patchValue({
        avatarFileName: file.name,
        avatarSize: file.size,
        avatarData: base64Data,         // ← Only the raw base64 string
        avatarMimeType: file.type
      });

      this.crewForm.markAsDirty();
    };

    reader.onerror = () => {
      this.alertService.showMessage('Failed to read file', '', MessageSeverity.error);
    };

    reader.readAsDataURL(file);
  }

  clearAvatar(): void {
    this.currentAvatarUrl = null;
    this.crewForm.patchValue({
      avatarFileName: null,
      avatarSize: null,
      avatarData: null,
      avatarMimeType: null
    });
    this.crewForm.markAsDirty();
  }


  closeModal() {
    if (this.modalRef) {
      this.modalRef.dismiss('cancel');
    }
    this.isAvatarPanelOpen = false;
    this.modalIsDisplayed = false;
  }


  submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.crewService.userIsSchedulerCrewWriter() == false) {
      this.alertService.showMessage(
        `${this.authService.currentUser?.userName} does not have permission to write Crews`,
        '',
        MessageSeverity.info
      );
      return;
    }


    if (!this.crewForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.crewForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.crewForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    // Note not falling back to user default values on null here becase we want them to be able to create records with null linksif they set the form that way
    //
    const crewSubmitData: CrewSubmitData = {
      id: this.crewSubmitData?.id || 0,
      name: formValue.name!.trim(),
      description: formValue.description?.trim() || null,
      notes: formValue.notes?.trim() || null,
      officeId: formValue.officeId ? Number(formValue.officeId) : null, 
      iconId: formValue.iconId ? Number(formValue.iconId) : null,
      color: formValue.color?.trim() || null,
      avatarFileName: formValue.avatarFileName?.trim() || null,
      avatarSize: formValue.avatarSize ? Number(formValue.avatarSize) : null,
      avatarData: formValue.avatarData?.trim() || null,
      avatarMimeType: formValue.avatarMimeType?.trim() || null,
      versionNumber: this.crewSubmitData?.versionNumber ?? 0,
      active: !!formValue.active,
      deleted: !!formValue.deleted,
    };

    if (this.isEditMode) {
      this.updateCrew(crewSubmitData);
    } else {
      this.addCrew(crewSubmitData);
    }
  }

  private addCrew(crewData: CrewSubmitData) {
    // Assign initial values to non-nullable control fields suitable for adding new data.
    crewData.versionNumber = 0;
    crewData.active = true;
    crewData.deleted = false;
    this.crewService.PostCrew(crewData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (newCrew) => {

        this.crewService.ClearAllCaches();

        this.crewChanged.next([newCrew]);

        this.alertService.showMessage("Crew added successfully", '', MessageSeverity.success);

        this.closeModal();

        if (this.navigateToDetailsAfterAdd) {
          this.router.navigate(['/crew', newCrew.id]);
        }
      },
      error: (err) => {
        let errorMessage: string;

        // Check if err is an Error object (e.g., new Error('message'))
        if (err instanceof Error) {
          errorMessage = err.message || 'An unexpected error occurred.';
        }
        // Check if err is a ServerError object with status and error properties
        else if (err.status && err.error) {
          if (err.status === 403) {
            errorMessage = err.error?.message ||
              'You do not have permission to save this Crew.';
          }
          else {
            errorMessage = err.error?.message ||
              err.error?.error_description ||
              err.error?.detail ||
              'An error occurred while saving the Crew.';
          }
        }
        // Fallback for unexpected error formats
        else {
          errorMessage = 'An unexpected error occurred.';
        }

        this.alertService.showMessage('Crew could not be saved',
          errorMessage,
          MessageSeverity.error);
      }
    });
  }


  private updateCrew(crewData: CrewSubmitData) {
    this.crewService.PutCrew(crewData.id, crewData).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (updatedCrew) => {

        this.crewService.ClearAllCaches();

        this.crewChanged.next([updatedCrew]);

        this.alertService.showMessage("Crew updated successfully", '', MessageSeverity.success);

        this.closeModal();
      },
      error: (err) => {
        let errorMessage: string;

        // Check if err is an Error object (e.g., new Error('message'))
        if (err instanceof Error) {
          errorMessage = err.message || 'An unexpected error occurred.';
        }
        // Check if err is a ServerError object with status and error properties
        else if (err.status && err.error) {
          if (err.status === 403) {
            errorMessage = err.error?.message ||
              'You do not have permission to save this Crew.';
          }
          else {
            errorMessage = err.error?.message ||
              err.error?.error_description ||
              err.error?.detail ||
              'An error occurred while saving the Crew.';
          }
        }
        // Fallback for unexpected error formats
        else {
          errorMessage = 'An unexpected error occurred.';
        }

        this.alertService.showMessage('Crew could not be saved',
          errorMessage,
          MessageSeverity.error);
      }
    });
  }



  private buildFormValues(crewData: CrewData | null) {

    if (crewData == null) {

      this.currentAvatarUrl = null;

      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.crewForm.reset({
        name: '',
        description: '',
        notes: '',
        officeId: this.officeId ?? this.currentUserService.defaultOfficeId,
        iconId: null,
        color: '',
        avatarFileName: '',
        avatarSize: '',
        avatarData: '',
        avatarMimeType: '',
        versionNumber: '',
        active: true,
        deleted: false,
      }, { emitEvent: false });

    }
    else {

      // Reconstruct full data URL for preview if we have base64 data
      if (crewData.avatarData && crewData.avatarMimeType) {
        this.currentAvatarUrl = `data:${crewData.avatarMimeType};base64,${crewData.avatarData}`;
      } else {
        this.currentAvatarUrl = null;
      }

      //
      // Reset the form with properly formatted values that support dates in datetime-local inputs
      //
      this.crewForm.reset({
        name: crewData.name ?? '',
        description: crewData.description ?? '',
        notes: crewData.notes ?? '',
        officeId: this.officeId ?? crewData.officeId ?? this.currentUserService.defaultOfficeId,
        iconId: crewData.iconId,
        color: crewData.color ?? '',
        avatarFileName: crewData.avatarFileName ?? '',
        avatarSize: crewData.avatarSize?.toString() ?? '',
        avatarData: crewData.avatarData ?? '',
        avatarMimeType: crewData.avatarMimeType ?? '',
        versionNumber: crewData.versionNumber?.toString() ?? '',
        active: crewData.active ?? true,
        deleted: crewData.deleted ?? false,
      }, { emitEvent: false });
    }

    this.crewForm.markAsPristine();
    this.crewForm.markAsUntouched();
  }

  public userIsSchedulerCrewReader(): boolean {
    return this.crewService.userIsSchedulerCrewReader();
  }

  public userIsSchedulerCrewWriter(): boolean {
    return this.crewService.userIsSchedulerCrewWriter();
  }

  public userIsSchedulerAdminsitrator(): boolean {
    return this.authService.isSchedulerAdministrator;
  }

  public userIsFoundationAdministrator(): boolean {
    return this.authService.isFoundationAdmin;
  }
}
