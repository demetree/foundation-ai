import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ResourceAddToCrewModalComponent } from '../resource-add-to-crew-modal/resource-add-to-crew-modal.component';
import { Router } from '@angular/router';
import { Subject } from 'rxjs'
import { Component, Input, Output, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { ResourceService, ResourceData } from '../../../scheduler-data-services/resource.service';
import { CrewMemberService, CrewMemberData } from '../../../scheduler-data-services/crew-member.service';
import { CrewCustomAddEditComponent } from '../../crew-custom/crew-custom-add-edit/crew-custom-add-edit.component'
import { CrewMemberAddEditComponent } from '../../../scheduler-data-components/crew-member/crew-member-add-edit/crew-member-add-edit.component'
import { CrewService, CrewData } from '../../../scheduler-data-services/crew.service';

/**
 * Crews tab for the Resource detail page.
 *
 * Displays all crews this resource belongs to, including:
 * - Crew name
 * - Role within the crew (if specified)
 * - Sequence/order in crew
 * - Crew description
 *
 * Data loaded imperatively when resource is available.
 */
@Component({
  selector: 'app-resource-crews-tab',
  templateUrl: './resource-crews-tab.component.html',
  styleUrls: ['./resource-crews-tab.component.scss']
})
export class ResourceCrewsTabComponent implements OnChanges {

  @ViewChild(CrewMemberAddEditComponent) addEditCrewMemberComponent!: CrewMemberAddEditComponent;
  @ViewChild(CrewCustomAddEditComponent) addEditCrewComponent!: CrewCustomAddEditComponent;

  /**
   * The resource passed from the parent detail component.
   */
  @Input() resource!: ResourceData | null;

  @Output() crewMembersChanged = new Subject<CrewMemberData>();
  @Output() crewChanged = new Subject<CrewData>();


  /**
   * Resolved crew memberships for this resource.
   */
  public crewMembers: CrewMemberData[] | null = null;

  /**
   * Loading and error states.
   */
  public isLoading = true;
  public error: string | null = null;

  constructor(private router: Router,
    private modalService: NgbModal,
    private crewMemberService: CrewMemberService) { }


  ngAfterViewInit(): void {

    //
    // Subscribe to the observables on the add/edit components and emit when they emit
    //
    if (this.addEditCrewComponent) {
      this.addEditCrewComponent.crewChanged.subscribe({
        next: (data: CrewData[] | null) => {

          this.resource?.ClearCrewMembersCache();

          if (data != null && data.length > 0) {
            this.crewChanged.next(data[0]);
          }
        },
        error: (err: any) => {
        }
      });
    }

    if (this.addEditCrewMemberComponent) {
      this.addEditCrewMemberComponent.crewMemberChanged.subscribe({
        next: (data: CrewMemberData[] | null) => {

          this.resource?.ClearCrewMembersCache();

          if (data != null && data.length > 0) {
            this.crewMembersChanged.next(data[0]);
          }
        },
        error: (err: any) => {
        }
      });
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['resource'] && this.resource) {

      this.resource?.ClearCrewMembersCache();

      this.loadCrewMembers();
    }
  }

  /**
   * Loads crew memberships using the resource's lazy promise getter.
   */
  public loadCrewMembers(): void {
    if (!this.resource) {
      this.crewMembers = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.resource.CrewMembers
      .then(members => {
        this.crewMembers = members;

        this.crewMembers.forEach(cm => {

          //
          // Reydrate the crew because this level of nav property isn't fully constructed yet.  We just have a basic data object for nav properties of nav properties.
          //
          // We need to convert it to a full data object by reviving it, and then calling reload on it, so we then get it's nav property values like icon.
          //
          cm.crew = CrewService.Instance.ReviveCrew(cm.crew);
          cm.crew?.Reload(true);
        });

        this.isLoading = false;
      })
      .catch(err => {
        console.error('Failed to load crew memberships', err);
        this.error = 'Unable to load crew memberships';
        this.crewMembers = [];
        this.isLoading = false;
      });
  }


  navigateToCrew(crewId: number | bigint | null | undefined): void {

    //
    // This routes to the crew memebers details page.  This is fine, but the back button doens't bring back the previous tab current.
    //
    if (crewId) {
      this.router.navigate(['/crew', crewId]);
    }
  }

  navigateToCrewMember(crewMemberId: number | bigint | null | undefined): void {

    //
    // This routes to the crew memebers details page.  This is fine, but the back button doens't bring back the previous tab current.
    //
    if (crewMemberId) {
      this.router.navigate(['/crewmember', crewMemberId]);
    }
  }


  openCrewModal(crew: CrewData | null | undefined): void {

    if (crew == null || crew == undefined) {
      return;
    }

    //
    // Opens up a modal to edit the crew
    //
    this.addEditCrewComponent.openModal(crew); // Default edit behavior
  }


  openCrewMemberModal(crewMember: CrewMemberData): void {

    //
    // Opens up a modal to edit the crew member
    //
    this.addEditCrewMemberComponent.openModal(crewMember); // Default edit behavior
  }


  //
  // Force a reload if there are changes
  // 
  public reload(data: CrewData[] | CrewMemberData[]): void {

    this.resource?.ClearCrewMembersCache();

    this.loadCrewMembers();
  }



  public userIsSchedulerCrewMemberWriter() :boolean {
    return this.crewMemberService.userIsSchedulerCrewMemberWriter();
  }

  public openAddToCrewModal(): void {

    if (this.resource == null) {
      return;
    }

    const modalRef = this.modalService.open(ResourceAddToCrewModalComponent, {
      size: 'md',
      backdrop: 'static'
    });

    modalRef.componentInstance.resourceId = this.resource?.id;
    modalRef.componentInstance.officeId = this.resource?.officeId;
    modalRef.componentInstance.resourceName = this.resource?.name;

    modalRef.result.then(
      (data) => {
        this.resource?.ClearCrewMembersCache();
        this.loadCrewMembers();
        this.crewMembersChanged.next(data);
      },
      () => {
        // dismissed — do nothing
      }
    );
  }
}
