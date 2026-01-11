import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ResourceQualificationService, ResourceQualificationSubmitData } from '../../../scheduler-data-services/resource-qualification.service';
import { QualificationService } from '../../../scheduler-data-services/qualification.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-resource-qualification-custom-add-modal',
  templateUrl: './resource-qualification-custom-add-modal.component.html',
  styleUrls: ['./resource-qualification-custom-add-modal.component.scss']
})
export class ResourceQualificationCustomAddModalComponent implements OnInit {
  /**
   * The ID of the resource we're adding a qualification for.
   * Passed from the parent (detail tab).
   */
  @Input() resourceId!: number;

  /**
   * Optional: full resource name for display (nice UX)
   */
  @Input() resourceName?: string;

  public qualificationForm: FormGroup;
  public isSaving = false;

  // Dropdown data
  public qualifications$ = this.qualificationService.GetQualificationList();

  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private qualificationService: QualificationService,
    private resourceQualificationService: ResourceQualificationService,
    private alertService: AlertService
  ) {
    this.qualificationForm = this.fb.group({
      qualificationId: [null, Validators.required],
      issueDate: [''],
      expiryDate: [''],
      issuer: [''],
      notes: ['']
    });
  }

  ngOnInit(): void {
    // Pre-fill today as issue date for convenience
    const today = new Date().toISOString().split('T')[0];
    this.qualificationForm.patchValue({ issueDate: today });
  }

  public submit(): void {
    if (this.isSaving || !this.qualificationForm.valid) {
      return;
    }

    this.isSaving = true;

    const formValue = this.qualificationForm.value;

    const submitData: ResourceQualificationSubmitData = {
      id: 0, // new
      resourceId: this.resourceId,
      qualificationId: Number(formValue.qualificationId),
      issueDate: formValue.issueDate || null,
      expiryDate: formValue.expiryDate || null,
      issuer: formValue.issuer?.trim() || null,
      notes: formValue.notes?.trim() || null,
      versionNumber: 1,
      active: true,
      deleted: false
    };

    this.resourceQualificationService.PostResourceQualification(submitData).subscribe({
      next: (newQual) => {
        this.resourceQualificationService.ClearAllCaches();
        this.alertService.showMessage('Qualification added successfully', '', MessageSeverity.success);
        this.activeModal.close(newQual); // return the new object to parent
      },
      error: (err) => {
        this.resourceQualificationService.ClearAllCaches();
        this.alertService.showMessage('Failed to add qualification', err.message || 'Unknown error', MessageSeverity.error);
        this.isSaving = false;
      },
      complete: () => {
        this.isSaving = false;
      }
    });
  }

  public cancel(): void {
    this.activeModal.dismiss('cancel');
  }
}
