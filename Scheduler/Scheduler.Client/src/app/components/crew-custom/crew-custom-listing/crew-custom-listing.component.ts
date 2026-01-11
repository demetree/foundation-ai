import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { CrewService, CrewData } from '../../../scheduler-data-services/crew.service';
import { CrewCustomAddEditComponent } from '../crew-custom-add-edit/crew-custom-add-edit.component';
import { CrewCustomTableComponent } from '../crew-custom-table/crew-custom-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-crew-custom-listing',
  templateUrl: './crew-custom-listing.component.html',
  styleUrls: ['./crew-custom-listing.component.scss']
})
export class CrewCustomListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(CrewCustomAddEditComponent) addEditCrewComponent!: CrewCustomAddEditComponent;
  @ViewChild(CrewCustomTableComponent) crewTableComponent!: CrewCustomTableComponent;

  public Crews: CrewData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalCrewCount$ : Observable<number> | null = null;
  public filteredCrewCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private crewService: CrewService,
    private alertService: AlertService,
    private authService: AuthService,
    private navigationService: NavigationService,
    private breakpointObserver: BreakpointObserver) { }

  ngOnInit(): void {

    this.breakpointObserver
      .observe(['(max-width: 1100px)']) // this size is specified to try and find a balance so tablets and phone see cards, but wider screens get a table.
      .subscribe((result) => {
        this.isSmallScreen = result.matches;
      });

    this.loadCounts();
  }

  ngAfterViewInit(): void {
    //
    // Subscribe to the crewChanged observable on the add/edit component so that when a Crew changes we can reload the list.
    //
    this.addEditCrewComponent.crewChanged.subscribe({
      next: (result: CrewData[] | null) => {
        this.crewTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Crew changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditCrewComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalCrewCount$ = this.crewService.GetCrewsRowCount({
      active: true,
      deleted: false
    }).pipe(
      map(c => Number(c ?? 0)),
      startWith(0),
      finalize(() => {
        this.loadingTotalCount = false;
      }),
      shareReplay(1)
    );

    if (this.filterText) {

      this.filteredCrewCount$ = this.crewService.GetCrewsRowCount({
        active: true,
        deleted: false,
        anyStringContains: this.filterText || undefined
      }).pipe(
        map(c => Number(c ?? 0)),
        startWith(0),
        finalize(() => {
          this.loadingFilteredCount = false;
        }),
        shareReplay(1)
      )
    } else {

      this.filteredCrewCount$ = this.totalCrewCount$; // No filter → same as total
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, fast filtering operations can get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalCrewCount$.subscribe();

    if (this.filteredCrewCount$ != this.totalCrewCount$) {
      this.filteredCrewCount$.subscribe();
    }
  }
  

  public goBack(): void {
    this.navigationService.goBack();
   }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public clearFilter() {
    this.filterText = '';
  }

  //
  // Update the counts when the filter change
  //
  onFilterChange(): void {

    clearTimeout(this.debounceTimeout);

    this.debounceTimeout = setTimeout(() => {
      this.crewTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 100);
  }


  public userIsSchedulerCrewReader(): boolean {
    return this.crewService.userIsSchedulerCrewReader();
  }

  public userIsSchedulerCrewWriter(): boolean {
    return this.crewService.userIsSchedulerCrewWriter();
  }
}
