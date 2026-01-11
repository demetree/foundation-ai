import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Subject } from 'rxjs'
import { CrewAddToCrewModalComponent } from '../crew-add-to-crew-modal/crew-add-to-crew-modal.component';
import { Router } from '@angular/router';
import { Component, Input, Output, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { CrewMemberService, CrewMemberData } from '../../../scheduler-data-services/crew-member.service';
import { ResourceCustomAddEditComponent } from '../../resource-custom/resource-custom-add-edit/resource-custom-add-edit.component'
import { CrewMemberAddEditComponent } from '../../../scheduler-data-components/crew-member/crew-member-add-edit/crew-member-add-edit.component'
import { CrewService, CrewData } from '../../../scheduler-data-services/crew.service';
import { ResourceService, ResourceData } from '../../../scheduler-data-services/resource.service';

/**
 * Crews tab for the Crew detail page.
 *
 * Displays all crews this crew belongs to, including:
 * - Resource name
 * - Role within the crew (if specified)
 * - Sequence/order in crew
 * - Resource description
 *
 * Data loaded imperatively when crew is available.
 */
@Component({
  selector: 'app-crew-members-tab',
  templateUrl: './crew-members-tab.component.html',
  styleUrls: ['./crew-members-tab.component.scss']
})
export class CrewMembersTabComponent implements OnChanges {

  @ViewChild(CrewMemberAddEditComponent) addEditCrewMemberComponent!: CrewMemberAddEditComponent;
  @ViewChild(ResourceCustomAddEditComponent) addEditResourceComponent!: ResourceCustomAddEditComponent;

  /**
   * The crew passed from the parent detail component.
   */
  @Input() crew!: CrewData | null;

  // Triggers when a crew member is changed.  To be implemented by users of this component.
  @Output() crewMemberChanged = new Subject<CrewMemberData>();
  @Output() resourceChanged = new Subject<ResourceData>();

  /**
   * Resolved crew memberships for this crew.
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
    if (this.addEditCrewMemberComponent) {
      this.addEditCrewMemberComponent.crewMemberChanged.subscribe({
        next: (data: CrewMemberData[] | null) => {

          if (data != null && data.length > 0) {
            this.crewMemberChanged.next(data[0]);
          }
        },
        error: (err: any) => {
        }
      });
    }

    if (this.addEditResourceComponent) {
      this.addEditResourceComponent.resourceChanged.subscribe({
        next: (data: ResourceData[] | null) => {

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
    if (changes['crew'] && this.crew) {
      this.loadCrewMembers();
    }
  }

  /**
   * Loads crew memberships using the crew's lazy promise getter.
   */
  public loadCrewMembers(): void {

    if (!this.crew) {
      this.crewMembers = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.crew.CrewMembers
      .then(members => {
        this.crewMembers = members;

        this.crewMembers.forEach(cm => {

          //
          // Reydrate the crew because this level of nav property isn't fully constructed yet.  We just have a basic data object for nav properties of nav properties.
          //
          // We need to convert it to a full data object by reviving it, and then calling reload on it, so we then get it's nav property values like icon.
          //
          cm.resource = ResourceService.Instance.ReviveResource(cm.resource);
          cm.resource?.Reload(true);

          cm.crew = CrewService.Instance.ReviveCrew(cm.crew);
          cm.crew?.Reload();
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


  navigateToResource(resourceId: number | bigint | null | undefined): void {

    //
    // This routes to the resource details page.  This is fine, but the back button doens't bring back the previous tab current.
    //
    if (resourceId) {
      this.router.navigate(['/resource', resourceId]);
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


  openResourceModal(resource: ResourceData | null | undefined): void {

    if (resource == null || resource == undefined) {
      return;
    }

    //
    // Opens up a modal to edit the crew
    //
    this.addEditResourceComponent.openModal(resource); // Default edit behavior
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
  public reload(data: ResourceData[] | CrewMemberData[]): void {

    this.crew?.ClearCrewMembersCache();

    this.loadCrewMembers();
  }



  public userIsSchedulerCrewMemberWriter() :boolean {
    return this.crewMemberService.userIsSchedulerCrewMemberWriter();
  }

  public openAddToCrewModal(): void {

    if (this.crew == null) {
      return;
    }

    const modalRef = this.modalService.open(CrewAddToCrewModalComponent, {
      size: 'md',
      backdrop: 'static'
    });

    modalRef.componentInstance.crewId = this.crew?.id;
    modalRef.componentInstance.crewName = this.crew?.name;
    modalRef.componentInstance.officeId = this.crew?.officeId;

    modalRef.result.then(
      (data) => {
        this.crew?.ClearCrewMembersCache();
        this.crewMemberChanged.next(data);
        this.loadCrewMembers();
      },
      () => {
        // dismissed — do nothing
      }
    );
  }
}
