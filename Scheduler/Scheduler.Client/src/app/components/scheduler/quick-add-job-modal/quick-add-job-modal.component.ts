import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ClientService, ClientSubmitData } from '../../../scheduler-data-services/client.service';
import { SchedulingTargetService, SchedulingTargetSubmitData } from '../../../scheduler-data-services/scheduling-target.service';
import { ScheduledEventService, ScheduledEventSubmitData } from '../../../scheduler-data-services/scheduled-event.service';
import { TerminologyService } from '../../../services/terminology.service';

@Component({
  selector: 'app-quick-add-job-modal',
  templateUrl: './quick-add-job-modal.component.html',
  styleUrl: './quick-add-job-modal.component.scss'
})
export class QuickAddJobModalComponent implements OnInit {

  public jobForm!: FormGroup;
  public isSaving = false;
  public errorMessage: string | null = null;

  constructor(
    public activeModal: NgbActiveModal,
    private formBuilder: FormBuilder,
    private clientService: ClientService,
    private targetService: SchedulingTargetService,
    private eventService: ScheduledEventService,
    public terminology: TerminologyService
  ) {}

  ngOnInit(): void {
    // Initialize form with defaults suitable for a quick entry
    this.jobForm = this.formBuilder.group({
      customerName: ['', Validators.required],
      email: [''],
      phone: [''],
      addressLine1: ['', Validators.required],
      city: ['', Validators.required],
      jobName: ['', Validators.required],
      date: ['', Validators.required],
      time: ['', Validators.required],
      durationHours: [1, [Validators.required, Validators.min(0.5)]]
    });
  }

  public onSubmit(): void {
    if (this.jobForm.invalid) {
      this.jobForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = null;

    const vals = this.jobForm.value;

    // 1. Create the Client (Customer)
    const newClient = {
      id: Number(0),
      name: vals.customerName,
      email: vals.email || null,
      phone: vals.phone || null,
      addressLine1: vals.addressLine1,
      city: vals.city,
      // Using common defaults for a quick add to bypass enterprise requirements
      clientTypeId: Number(1), 
      currencyId: Number(1),
      timeZoneId: Number(1),
      stateProvinceId: Number(1),
      countryId: Number(1),
      versionNumber: Number(0),
      active: true,
      deleted: false
    } as ClientSubmitData;

    this.clientService.PostClient(newClient).subscribe({
      next: (createdClient) => {
        if (!createdClient) {
          this.errorMessage = "Failed to create customer record.";
          this.isSaving = false;
          return;
        }

        // 2. Create the Scheduling Target (Location) using the new Client ID
        const newTarget = {
          id: Number(0),
          name: vals.addressLine1, // Often the target name is just the address for small orgs
          clientId: Number(createdClient.id),
          schedulingTargetTypeId: Number(1),
          timeZoneId: Number(1),
          versionNumber: Number(0),
          active: true,
          deleted: false
        } as SchedulingTargetSubmitData;

        this.targetService.PostSchedulingTarget(newTarget).subscribe({
          next: (createdTarget) => {
            if (!createdTarget) {
              this.errorMessage = "Failed to create location record.";
              this.isSaving = false;
              return;
            }

            // 3. Create the Scheduled Event (Job) using the new Target ID
            const [year, month, day] = vals.date.split('-');
            const [hours, minutes] = vals.time.split(':');
            const startDateTime = new Date(year, month - 1, day, hours, minutes);
            
            const endDateTime = new Date(startDateTime.getTime());
            endDateTime.setHours(endDateTime.getHours() + Math.floor(vals.durationHours));
            endDateTime.setMinutes(endDateTime.getMinutes() + ((vals.durationHours % 1) * 60));

            const newEvent = {
              id: Number(0),
              name: vals.jobName,
              schedulingTargetId: Number(createdTarget.id),
              startDateTime: startDateTime.toISOString(),
              endDateTime: endDateTime.toISOString(),
              eventStatusId: Number(1), // Default "Scheduled" or similar
              isOpenForVolunteers: false,
              versionNumber: Number(0),
              active: true,
              deleted: false
            } as ScheduledEventSubmitData;

            this.eventService.PostScheduledEvent(newEvent).subscribe({
              next: (createdEvent) => {
                if (!createdEvent) {
                  this.errorMessage = "Failed to create job record.";
                  this.isSaving = false;
                  return;
                }
                
                // Success - close modal and return the new event
                this.isSaving = false;
                this.activeModal.close(createdEvent);
              },
              error: (err) => {
                this.errorMessage = err.message || 'An unexpected error occurred while saving the job.';
                this.isSaving = false;
              }
            });

          },
          error: (err) => {
            this.errorMessage = err.message || 'An unexpected error occurred while saving the location.';
            this.isSaving = false;
          }
        });

      },
      error: (err) => {
        this.errorMessage = err.message || 'An unexpected error occurred while saving the customer.';
        this.isSaving = false;
      }
    });
  }
}
