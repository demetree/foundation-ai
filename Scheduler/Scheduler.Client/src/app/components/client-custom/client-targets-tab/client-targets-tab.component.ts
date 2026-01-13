import { Component, Input, Output, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { Subject } from 'rxjs'
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
//import { SchedulingTargetCustomAddEditComponent } from '../../scheduling-target-custom/scheduling-target-custom-add-edit/scheduling-target-custom-add-edit.component';
import { SchedulingTargetAddEditComponent } from '../../../scheduler-data-components/scheduling-target/scheduling-target-add-edit/scheduling-target-add-edit.component';
import { Router } from '@angular/router';
import { ClientData } from '../../../scheduler-data-services/client.service';
import { SchedulingTargetService, SchedulingTargetData } from '../../../scheduler-data-services/scheduling-target.service';

/**
 * SchedulingTargets tab for the Client detail page.
 *
 * Displays all schedulingTargets for this client
 * 
 * Data loaded imperatively when client is available.
 */
@Component({
  selector: 'app-client-targets-tab',
  templateUrl: './client-targets-tab.component.html',
  styleUrls: ['./client-targets-tab.component.scss']
})
export class ClientTargetsTabComponent implements OnChanges {

  @ViewChild(SchedulingTargetAddEditComponent) addEditSchedulingTargetComponent!: SchedulingTargetAddEditComponent;

  /**
   * The client passed from the parent detail component.
   */
  @Input() client!: ClientData | null;

  @Output() schedulingTargetChanged = new Subject<SchedulingTargetData>();


  /**
   * Resolved schedulingTargets for this client
   */
  public schedulingTargets: SchedulingTargetData[] | null = null;

  /**
   * Loading and error states.
   */
  public isLoading = true;
  public error: string | null = null;

  constructor(private router: Router,
    private modalService: NgbModal,
    private schedulingTargetService: SchedulingTargetService) { }

  ngAfterViewInit(): void {

    //
    // Subscribe to the observables on the add/edit components and emit when they emit
    //
    if (this.addEditSchedulingTargetComponent) {
      this.addEditSchedulingTargetComponent.schedulingTargetChanged.subscribe({
        next: (data: SchedulingTargetData[] | null) => {

          this.client?.ClearSchedulingTargetsCache();

          if (data != null && data.length > 0) {
            this.schedulingTargetChanged.next(data[0]);
          }
        },
        error: (err: any) => {
        }
      });
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['client'] && this.client) {

      this.client?.ClearSchedulingTargetsCache();

      this.loadSchedulingTargets();
    }
  }

  /**
   * Loads schedulingTargets using the client's lazy promise getter.
   */
  public loadSchedulingTargets(): void {
    if (!this.client) {
      this.schedulingTargets = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.client.SchedulingTargets
      .then(schedulingTarget => {
        this.schedulingTargets = schedulingTarget;

        this.isLoading = false;
      })
      .catch(err => {
        console.error('Failed to load schedulingTargets', err);
        this.error = 'Unable to load schedulingTargets';
        this.schedulingTargets = [];
        this.isLoading = false;
      });
  }


  navigateToSchedulingTarget(schedulingTargetId: number | bigint | null | undefined): void {

    //
    // This routes to the schedulingTarget details page.  This is fine, but the back button doens't bring back the previous tab current.
    //
    if (schedulingTargetId) {
      this.router.navigate(['/schedulingtarget', schedulingTargetId]);
    }
  }


  openSchedulingTargetModal(schedulingTarget: SchedulingTargetData | null | undefined): void {

    if (schedulingTarget == null || schedulingTarget == undefined) {
      return;
    }

    //
    // Opens up a modal to edit the crew
    //
    this.addEditSchedulingTargetComponent.openModal(schedulingTarget); // Default edit behavior
  }


  //
  // Force a reload if there are changes
  // 
  public reload(data: SchedulingTargetData[] ): void {

    this.client?.ClearSchedulingTargetsCache();

    this.loadSchedulingTargets();
  }


  public userIsSchedulerTargetWriter(): boolean {
    return this.schedulingTargetService.userIsSchedulerSchedulingTargetWriter();
  }


  public openAddTargetModal(): void {

    if (this.client == null) {
      return;
    }

    this.addEditSchedulingTargetComponent.navigateToDetailsAfterAdd = false;
    //this.addEditSchedulingTargetComponent.clientId = this.client.id as number;
    this.addEditSchedulingTargetComponent.openModal();
  }
}
