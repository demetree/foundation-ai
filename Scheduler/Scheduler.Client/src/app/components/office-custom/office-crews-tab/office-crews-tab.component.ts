import { Component, Input, Output, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Subject } from 'rxjs'
import { Router } from '@angular/router';
import { OfficeService, OfficeData } from '../../../scheduler-data-services/office.service';
import { CrewMemberService, CrewMemberData } from '../../../scheduler-data-services/crew-member.service';
import { CrewCustomAddEditComponent } from '../../crew-custom/crew-custom-add-edit/crew-custom-add-edit.component'
import { CrewMemberAddEditComponent } from '../../../scheduler-data-components/crew-member/crew-member-add-edit/crew-member-add-edit.component'
import { CrewService, CrewData } from '../../../scheduler-data-services/crew.service';

/**
 * Crews tab for the Office detail page.
 *
 * Displays all crews this office belongs to, including:
 * - Crew name
 * - Role within the crew (if specified)
 * - Sequence/order in crew
 * - Crew description
 *
 * Data loaded imperatively when office is available.
 */
@Component({
  selector: 'app-office-crews-tab',
  templateUrl: './office-crews-tab.component.html',
  styleUrls: ['./office-crews-tab.component.scss']
})
export class OfficeCrewsTabComponent implements OnChanges {

  @ViewChild(CrewMemberAddEditComponent) addEditCrewMemberComponent!: CrewMemberAddEditComponent;
  @ViewChild(CrewCustomAddEditComponent) addEditCrewComponent!: CrewCustomAddEditComponent;

  /**
   * The office passed from the parent detail component.
   */
  @Input() office!: OfficeData | null;

  @Output() crewMembersChanged = new Subject<CrewMemberData>();
  @Output() crewChanged = new Subject<CrewData>();


  /**
   * Resolved crew memberships for this office.
   */
  public crews: CrewData[] | null = null;

  /**
   * Loading and error states.
   */
  public isLoading = true;
  public error: string | null = null;


  // Map to track expanded state: crew.id → boolean
  private expandedCrewIds = new Map<number, boolean>();


  constructor(private router: Router,
    private modalService: NgbModal,
    private crewService: CrewService,
    private crewMemberService: CrewMemberService) { }

  ngAfterViewInit(): void {

    //
    // Subscribe to the observables on the add/edit components and emit when they emit
    //
    if (this.addEditCrewComponent) {
      this.addEditCrewComponent.crewChanged.subscribe({
        next: (data: CrewData[] | null) => {

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

          this.office?.ClearCrewsCache();

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
    if (changes['office'] && this.office) {

      this.office.ClearCrewsCache();

      this.loadCrews();
    }
  }



  /**
   * Loads crewsusing the office's lazy promise getter.
   */
  public loadCrews(): void {
    if (!this.office) {
      this.crews = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.office.Crews
      .then(crews => {
        this.crews = crews ?? [];


        this.crews.forEach(crew => {

          ////
          //// Reydrate the crew because this level of nav property isn't fully constructed yet.  We just have a basic data object for nav properties of nav properties.
          ////
          //// We need to convert it to a full data object by reviving it, and then calling reload on it, so we then get it's nav property values like icon.
          ////
          //cm.crew = CrewService.Instance.ReviveCrew(cm.crew);
          //cm.crew?.Reload(true);

          //cm.office = OfficeService.Instance.ReviveOffice(cm.office);
          //cm.office.Reload();


          crew.CrewMembers.then(cm => { console.log("crew member count is" + cm.length); });
          
        });

        this.isLoading = false;
      })
      .catch(err => {
        console.error('Failed to load crews', err);
        this.error = 'Unable to load crews';
        this.crews = [];
        this.isLoading = false;
      });
  }


  public isCrewExpanded(crew: CrewData): boolean {
    return this.expandedCrewIds.get(crew.id as number) || false;
  }

  public toggleCrewExpanded(crew: CrewData): void {
    this.expandedCrewIds.set(crew.id as number, !this.isCrewExpanded(crew));
  }

  navigateToCrew(crewId: number | bigint | null | undefined): void {

    //
    // This routes to the crew details page.  This is fine, but the back button doens't bring back the previous tab current.
    //
    if (crewId) {
      this.router.navigate(['/crew', crewId]);
    }
  }

  navigateToResource(resourceId: number | bigint | null | undefined): void {

    //
    // This routes to the resource details page.  This is fine, but the back button doens't bring back the previous tab current here.
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

  openAddCrewModal(): void {

    if (this.office == null) {
      return;
    }

    //
    // Opens up a modal to add a crew
    //
    this.addEditCrewComponent.officeId = this.office?.id as number;
    this.addEditCrewComponent.navigateToDetailsAfterAdd = false;      // don't redirect to the new crew page.

    this.addEditCrewComponent.openModal(); // Default add behavior
  }



  openEditCrewModal(crew: CrewData | null | undefined): void {

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

    this.office?.ClearCrewsCache();

    this.loadCrews();
  }



  public userIsSchedulerCrewWriter() :boolean {
    return this.crewService.userIsSchedulerCrewWriter();
  }

}
