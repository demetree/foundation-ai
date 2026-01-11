import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { Subject } from 'rxjs'
import { ResourceQualificationCustomAddModalComponent } from '../resource-qualification-custom-add-modal/resource-qualification-custom-add-modal.component';
import { ResourceQualificationAddEditComponent } from '../../../scheduler-data-components/resource-qualification/resource-qualification-add-edit/resource-qualification-add-edit.component'
import { Component, Input, Output, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { ResourceData } from '../../../scheduler-data-services/resource.service';
import { ResourceQualificationService, ResourceQualificationData } from '../../../scheduler-data-services/resource-qualification.service';

/**
 * Qualifications tab for the Resource detail page.
 *
 * Displays a list of all qualifications held by the resource, with visual indicators
 * for expiry status:
 * - Expired: Red badge + warning icon
 * - Expiring within 30 days: Orange badge
 * - Valid: Green badge
 *
 * Data is loaded lazily via the resource's ResourceQualifications promise.
 */
@Component({
  selector: 'app-resource-qualifications-tab',
  templateUrl: './resource-qualifications-tab.component.html',
  styleUrls: ['./resource-qualifications-tab.component.scss']
})
export class ResourceQualificationsTabComponent implements OnInit {

  @ViewChild(ResourceQualificationAddEditComponent) addEditResourceQualificationComponent!: ResourceQualificationAddEditComponent;

  /**
   * The parent resource object — provides access to lazy-loaded qualifications.
   * Required input — this tab is always used with a loaded resource.
   */
  @Input() resource!: ResourceData;

  // Triggers when a resource qualification is changed.  To be implemented by users of this component.
  @Output() resourceQualificationChanged = new Subject<ResourceQualificationData>();

  /**
   * Holds the resolved qualifications array once loaded.
   * Null while loading, empty array if none.
   */
  public qualifications: ResourceQualificationData[] | null = null;

  public canWrite = this.resourceQualificationService.userIsSchedulerResourceQualificationWriter;

  /**
   * Loading state for the qualifications list.
   */
  public isLoading = true;

  /**
   * Error message if qualification load fails.
   */
  public error: string | null = null;

  constructor(private router: Router,
              private resourceQualificationService: ResourceQualificationService,
              private modalService: NgbModal) { }

  ngOnInit(): void {
    
  }

  /**
   * React to changes in @Input() resource.
   * When resource becomes available, load qualifications.
   */
  ngOnChanges(changes: SimpleChanges): void {

    if (changes['resource'] && this.resource) {

      this.resource.ClearResourceQualificationsCache();

      this.loadQualifications();
    }
  }

  /**
   * Loads qualifications from the resource's lazy-loaded promise.
   * Handles success, error, and empty states.
   */
  public loadQualifications(): void {

    // Safety check — should never be null here due to ngOnChanges guard
    if (!this.resource) {
      this.qualifications = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.resource.ResourceQualifications
      .then(qualifications => {
        this.qualifications = qualifications;
        this.isLoading = false;
      })
      .catch(err => {
        console.error('Failed to load resource qualifications', err);
        this.error = 'Unable to load qualifications';
        this.qualifications = [];
        this.isLoading = false;
      });
  }


  navigateToResourceQualification(id: number | bigint | null | undefined): void {
    if (id) {
      this.router.navigate(['/resourcequalifications', id]);
    }
  }

  openResourceQualificationModal(rq: ResourceQualificationData | undefined): void {

    if (rq == null || rq == undefined) {
      return;
    }

    //
    // Opens up a modal to edit the ResourceQualification
    //
    this.addEditResourceQualificationComponent.openModal(rq); // Default edit behavior
  }



  //
  // Force a reload if a contact changes
  // 
  public reload(rq: ResourceQualificationData[]): void {

    this.resource?.ClearResourceQualificationsCache();

    this.loadQualifications();
  }


  /**
   * Determines the badge class and icon based on qualification expiry status.
   * @param qual The qualification to evaluate
   * @returns Object with badgeClass and iconClass
   */
  public getExpiryStatus(qual: ResourceQualificationData): { badgeClass: string; iconClass: string } {
    if (!qual.expiryDate) {
      // No expiry — permanent
      return { badgeClass: 'bg-success', iconClass: 'fa-solid fa-infinity' };
    }

    const expiry = new Date(qual.expiryDate);
    const today = new Date();
    const daysUntilExpiry = Math.floor((expiry.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));

    if (daysUntilExpiry < 0) {
      // Expired
      return { badgeClass: 'bg-danger', iconClass: 'fa-solid fa-exclamation-triangle' };
    } else if (daysUntilExpiry <= 30) {
      // Expiring soon (within 30 days)
      return { badgeClass: 'bg-warning text-dark', iconClass: 'fa-solid fa-clock' };
    } else {
      // Valid
      return { badgeClass: 'bg-success', iconClass: 'fa-solid fa-check' };
    }
  }

  /**
   * Formats a date string for display (medium date style).
   * Returns '—' if null.
   */
  public formatDate(dateStr: string | null): string {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }


  public getDaysUntilExpiry(qual: ResourceQualificationData): number {
    if (!qual.expiryDate) return Infinity;
    const expiry = new Date(qual.expiryDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return Math.floor((expiry.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
  }


  openAddQualificationModal(): void {
    const modalRef = this.modalService.open(ResourceQualificationCustomAddModalComponent, {
      size: 'lg',
      backdrop: 'static'
    });

    modalRef.componentInstance.resourceId = this.resource.id;
    modalRef.componentInstance.resourceName = this.resource.name;

    modalRef.result.then(
      (data) => {
        this.resource.ClearResourceQualificationsCache();
        this.loadQualifications(); // refresh list
        this.resourceQualificationChanged.next(data);
      },
      () => {
        // dismissed — do nothing
      }
    );
  }
}
