import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ResourceAvailabilityService, ResourceAvailabilitySubmitData } from '../../../scheduler-data-services/resource-availability.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-resource-availability-add-modal',
  templateUrl: './resource-availability-add-modal.component.html',
  styleUrls: ['./resource-availability-add-modal.component.scss']
})
export class ResourceAvailabilityAddModalComponent implements OnInit {
  /**
   * The ID of the resource we're adding a blackout for.
   */
  @Input() resourceId!: number;

  /**
   * Optional: Resource name for display in header
   */
  @Input() resourceName?: string;

  // Time zone ID to use when saving
  @Input() timeZoneId!: number;

  public blackoutForm: FormGroup;
  public isSaving = false;

  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private availabilityService: ResourceAvailabilityService,
    private alertService: AlertService
  ) {
    this.blackoutForm = this.fb.group({
      startDateTime: ['', Validators.required],
      endDateTime: ['', Validators.required],
      reason: ['', Validators.required],
      notes: ['']
    });
  }

  ngOnInit(): void {
    // Pre-fill with reasonable defaults
    const now = new Date();
    const roundedStart = new Date(Math.ceil(now.getTime() / (15 * 60 * 1000)) * (15 * 60 * 1000)); // Round up to next 15 min
    const defaultEnd = new Date(roundedStart.getTime() + 60 * 60 * 1000); // +1 hour

    this.blackoutForm.patchValue({
      startDateTime: this.formatDateTimeForInput(roundedStart),
      endDateTime: this.formatDateTimeForInput(defaultEnd)
    });
  }

  /**
   * Formats a Date for datetime-local input (YYYY-MM-DDTHH:mm)
   */
  private formatDateTimeForInput(date: Date): string {
    return date.toISOString().slice(0, 16);
  }

  public submit(): void {
    if (this.isSaving || !this.blackoutForm.valid) {
      return;
    }

    this.isSaving = true;

    const formValue = this.blackoutForm.value;

    // Convert local datetime strings to UTC ISO for server
    const startUtc = new Date(formValue.startDateTime).toISOString();
    const endUtc = new Date(formValue.endDateTime).toISOString();

    const submitData: ResourceAvailabilitySubmitData = {
      id: 0,
      resourceId: this.resourceId,
      startDateTime: startUtc,
      endDateTime: endUtc,
      reason: formValue.reason.trim(),
      versionNumber: 1,
      timeZoneId: this.timeZoneId,
      notes: formValue.notes,
      active: true,
      deleted: false
    };

    this.availabilityService.PostResourceAvailability(submitData).subscribe({
      next: (newBlackout) => {

        this.availabilityService.ClearAllCaches();

        this.alertService.showMessage(
          'Blackout period added successfully',
          '',
          MessageSeverity.success
        );
        this.activeModal.close(newBlackout);
      },
      error: (err) => {
        this.alertService.showMessage(
          'Failed to add blackout',
          err.message || 'Unknown error',
          MessageSeverity.error
        );
        this.isSaving = false;
      }
    });
  }

  public cancel(): void {
    this.activeModal.dismiss('cancel');
  }
}
