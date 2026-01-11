import { Component, Input, Output, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { Subject } from 'rxjs'
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ResourceCustomAddEditComponent } from '../../resource-custom/resource-custom-add-edit/resource-custom-add-edit.component';
import { Router } from '@angular/router';
import { OfficeData } from '../../../scheduler-data-services/office.service';
import { ResourceService, ResourceData } from '../../../scheduler-data-services/resource.service';

/**
 * Resources tab for the Office detail page.
 *
 * Displays all resources for this office
 * 
 * Data loaded imperatively when office is available.
 */
@Component({
  selector: 'app-office-resources-tab',
  templateUrl: './office-resources-tab.component.html',
  styleUrls: ['./office-resources-tab.component.scss']
})
export class OfficeResourcesTabComponent implements OnChanges {

  @ViewChild(ResourceCustomAddEditComponent) addEditResourceComponent!: ResourceCustomAddEditComponent;

  /**
   * The office passed from the parent detail component.
   */
  @Input() office!: OfficeData | null;

  @Output() resourceChanged = new Subject<ResourceData>();


  /**
   * Resolved resources for this office
   */
  public resources: ResourceData[] | null = null;

  /**
   * Loading and error states.
   */
  public isLoading = true;
  public error: string | null = null;

  constructor(private router: Router,
    private modalService: NgbModal,
    private resourceService: ResourceService) { }

  ngAfterViewInit(): void {

    //
    // Subscribe to the observables on the add/edit components and emit when they emit
    //
    if (this.addEditResourceComponent) {
      this.addEditResourceComponent.resourceChanged.subscribe({
        next: (data: ResourceData[] | null) => {

          this.office?.ClearResourcesCache();

          if (data != null && data.length > 0) {
            this.resourceChanged.next(data[0]);
          }
        },
        error: (err: any) => {
        }
      });
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['office'] && this.office) {

      this.office?.ClearResourcesCache();

      this.loadResources();
    }
  }

  /**
   * Loads resources using the office's lazy promise getter.
   */
  public loadResources(): void {
    if (!this.office) {
      this.resources = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.office.Resources
      .then(resource => {
        this.resources = resource;

        this.isLoading = false;
      })
      .catch(err => {
        console.error('Failed to load resources', err);
        this.error = 'Unable to load resources';
        this.resources = [];
        this.isLoading = false;
      });
  }


  navigateToResource(resourceId: number | bigint | null | undefined): void {

    //
    // This routes to the resource details page.  This is fine, but the back button doens't bring back the previous tab current.
    //
    if (resourceId) {
      this.router.navigate(['/resource', resourceId]);
    }
  }


  openResourceModal(resource: ResourceData | null | undefined): void {

    if (resource == null || resource == undefined) {
      return;
    }

    //
    // Opens up a modal to edit the crew
    //
    this.addEditResourceComponent.openModal(resource); // Default edit behavior
  }


  //
  // Force a reload if there are changes
  // 
  public reload(data: ResourceData[] ): void {

    this.office?.ClearResourcesCache();

    this.loadResources();
  }


  public userIsSchedulerResourceWriter(): boolean {
    return this.resourceService.userIsSchedulerResourceWriter();
  }


  public openAddResourceModal(): void {

    if (this.office == null) {
      return;
    }

    this.addEditResourceComponent.navigateToDetailsAfterAdd = false;
    this.addEditResourceComponent.officeId = this.office.id as number;
    this.addEditResourceComponent.openModal();
  }
}
