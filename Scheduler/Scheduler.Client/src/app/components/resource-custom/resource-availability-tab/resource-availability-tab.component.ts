import { Component, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { Subject } from 'rxjs'
import { Router } from '@angular/router';
import { ResourceData } from '../../../scheduler-data-services/resource.service';
import { ResourceAvailabilityData } from '../../../scheduler-data-services/resource-availability.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ResourceAvailabilityAddModalComponent } from '../resource-availability-add-modal/resource-availability-add-modal.component';

/**
 * Availability / Blackouts tab for the Resource detail page.
 *
 * Displays all blackout periods for the resource with visual status indicators:
 * - Upcoming: Blue
 * - Active (current): Orange
 * - Past: Gray
 *
 * Data loaded imperatively when tab becomes active.
 */
@Component({
  selector: 'app-resource-availability-tab',
  templateUrl: './resource-availability-tab.component.html',
  styleUrls: ['./resource-availability-tab.component.scss']
})
export class ResourceAvailabilityTabComponent implements OnChanges {

  @Input() resource!: ResourceData | null;

  // Triggers when a resource availability is changed.  To be implemented by users of this component.
  @Output() resourceAvailabilityChanged = new Subject<ResourceAvailabilityData>();


  public availabilities: ResourceAvailabilityData[] | null = null;
  public isLoading = true;
  public error: string | null = null;

  constructor(private modalService: NgbModal, private router: Router) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resource'] && this.resource) {

      this.resource.ClearResourceAvailabilitiesCache();

      this.loadAvailabilities();
    }
  }

  public loadAvailabilities(): void {
    if (!this.resource) {
      this.availabilities = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.resource.ResourceAvailabilities
      .then(avails => {
        // Sort by start date descending (most recent first)
        this.availabilities = avails.sort((a, b) =>
          new Date(b.startDateTime).getTime() - new Date(a.startDateTime).getTime()
        );
        this.isLoading = false;
      })
      .catch(err => {
        console.error('Failed to load resource availabilities', err);
        this.error = 'Unable to load blackout periods';
        this.availabilities = [];
        this.isLoading = false;
      });
  }


  /**
   * Determines status badge class and text for a blackout
   */
  public getBlackoutStatus(avail: ResourceAvailabilityData): { badgeClass: string; text: string } {
    const now = new Date();
    const start = new Date(avail.startDateTime);
    const end = new Date(avail.endDateTime ?? "2999-12-31");

    if (now < start) {
      return { badgeClass: 'bg-info', text: 'Upcoming' };
    } else if (now >= start && now <= end) {
      return { badgeClass: 'bg-warning text-dark', text: 'Active Now' };
    } else {
      return { badgeClass: 'bg-secondary', text: 'Past' };
    }
  }

  public formatDateTime(dateStr: string | null): string {

    if (dateStr == null) {
      return "";
    }

    return new Date(dateStr).toLocaleString(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short'
    });
  }


  navigateToResourceAvailability(resourceAvailabilityId: number | bigint | null | undefined): void {
    if (resourceAvailabilityId) {
      this.router.navigate(['/resourceavailability', resourceAvailabilityId]);
    }
  }


  public openAddBlackoutModal(): void {

    if (!this.resource) return;

    const modalRef = this.modalService.open(ResourceAvailabilityAddModalComponent, {
      size: 'md',
      backdrop: 'static'
    });

    modalRef.componentInstance.resourceId = this.resource!.id;
    modalRef.componentInstance.resourceName = this.resource!.name;
    modalRef.componentInstance.timeZoneId = this.resource!.timeZoneId;

    modalRef.result.then(
      (data) => {
        this.resource?.ClearResourceAvailabilitiesCache();
        this.resourceAvailabilityChanged.next(data);
        this.loadAvailabilities()
      }
      ,
      () => { }
    );
  }
}
