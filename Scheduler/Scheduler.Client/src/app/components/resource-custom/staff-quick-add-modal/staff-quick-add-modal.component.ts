//
// Staff Quick Add Modal — quickly create a new Resource (staff member).
//
// AI-Developed — This file was significantly developed with AI assistance.
//
import { Component, Output, ViewChild, TemplateRef, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Subject, finalize, take } from 'rxjs';
import { ResourceService, ResourceData, ResourceSubmitData } from '../../../scheduler-data-services/resource.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { CurrentUserService } from '../../../services/current-user.service';
import { ResourceTypeService } from '../../../scheduler-data-services/resource-type.service';
import { TerminologyService } from '../../../services/terminology.service';

@Component({
  selector: 'app-staff-quick-add-modal',
  templateUrl: './staff-quick-add-modal.component.html',
  styleUrls: ['./staff-quick-add-modal.component.scss']
})
export class StaffQuickAddModalComponent implements OnInit {
  @ViewChild('quickAddModal') quickAddModal!: TemplateRef<any>;
  @Output() resourceChanged = new Subject<ResourceData[]>();

  public isSaving = false;
  public modalIsDisplayed = false;
  private modalRef?: NgbModalRef;
  private defaultResourceTypeId: number = 0;

  quickAddForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: ['']
  });

  constructor(
    private modalService: NgbModal,
    private resourceService: ResourceService,
    private resourceTypeService: ResourceTypeService,
    private alertService: AlertService,
    public authService: AuthService,
    private currentUserService: CurrentUserService,
    private fb: FormBuilder,
    public terminology: TerminologyService
  ) {}

  ngOnInit() {
    this.resourceTypeService.GetResourceTypeList().pipe(take(1)).subscribe({
      next: (types: any) => {
        if (types && types.length > 0) {
          this.defaultResourceTypeId = Number(types[0].id);
        } else {
          console.warn('No Resource Types found — quick-add may fail on submit.');
        }
      },
      error: () => {
        console.warn('Could not load Resource Types for quick-add defaults.');
      }
    });
  }

  public openModal() {
    // Permission check — matches established pattern
    if (!this.authService.isSchedulerReaderWriter) {
      this.alertService.showMessage(
        'Permission Denied',
        `You do not have permission to create ${this.terminology.getTerm('Resource', true)}.`,
        MessageSeverity.info
      );
      return;
    }

    this.quickAddForm.reset();
    this.modalIsDisplayed = true;
    this.modalRef = this.modalService.open(this.quickAddModal, { backdrop: 'static', centered: true });
  }

  public closeModal() {
    this.modalIsDisplayed = false;
    this.modalRef?.dismiss();
  }

  public submitForm() {
    if (this.quickAddForm.invalid) {
      this.quickAddForm.markAllAsTouched();
      return;
    }

    if (this.defaultResourceTypeId === 0) {
      this.alertService.showMessage(
        'Configuration Error',
        'No Resource Types are configured. Please contact an administrator.',
        MessageSeverity.warn
      );
      return;
    }

    this.isSaving = true;
    const formValue = this.quickAddForm.value;

    const newResource: ResourceSubmitData = {
      id: 0,
      name: formValue.name?.trim(),
      description: formValue.description?.trim() || null,
      resourceTypeId: this.defaultResourceTypeId,
      timeZoneId: this.currentUserService.defaultTimeZoneId || 0,
      officeId: this.currentUserService.defaultOfficeId as any,
      versionNumber: 0,
      active: true,
      deleted: false,
      // Nullable fields — let server apply defaults
      shiftPatternId: null,
      targetWeeklyWorkHours: null,
      notes: null,
      externalId: null,
      color: null,
      attributes: null,
      avatarFileName: null,
      avatarSize: null,
      avatarData: null,
      avatarMimeType: null
    };

    this.resourceService.PostResource(newResource).pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (res: any) => {
        this.resourceService.ClearAllCaches();
        this.resourceChanged.next([res]);
        this.alertService.showMessage(
          `${this.terminology.getTerm('Resource')} added successfully`,
          '',
          MessageSeverity.success
        );
        this.closeModal();
      },
      error: (err: any) => {
        const errorMessage = this.extractErrorMessage(err,
          `Could not save ${this.terminology.getTerm('Resource')}.`);
        this.alertService.showMessage(
          `Could not save ${this.terminology.getTerm('Resource')}`,
          errorMessage,
          MessageSeverity.error
        );
      }
    });
  }

  /** Structured error extraction matching established app patterns. */
  private extractErrorMessage(err: any, fallback: string): string {
    if (err instanceof Error) {
      return err.message || fallback;
    }
    if (err?.status && err?.error) {
      if (err.status === 403) {
        return err.error?.message || 'You do not have permission to perform this action.';
      }
      return err.error?.message || err.error?.error_description || err.error?.detail || fallback;
    }
    return fallback;
  }
}
