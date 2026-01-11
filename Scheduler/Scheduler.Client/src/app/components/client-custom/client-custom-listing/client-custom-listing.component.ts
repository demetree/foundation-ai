import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ClientService, ClientData } from '../../../scheduler-data-services/client.service';
import { ClientCustomAddEditComponent } from '../client-custom-add-edit/client-custom-add-edit.component';
import { ClientCustomTableComponent } from '../client-custom-table/client-custom-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-client-custom-listing',
  templateUrl: './client-custom-listing.component.html',
  styleUrls: ['./client-custom-listing.component.scss']
})
export class ClientCustomListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ClientCustomAddEditComponent) addEditClientComponent!: ClientCustomAddEditComponent;
  @ViewChild(ClientCustomTableComponent) clientTableComponent!: ClientCustomTableComponent;

  public Clients: ClientData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalClientCount$ : Observable<number> | null = null;
  public filteredClientCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private clientService: ClientService,
              private alertService: AlertService,
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
    // Subscribe to the clientChanged observable on the add/edit component so that when a Client changes we can reload the list.
    //
    this.addEditClientComponent.clientChanged.subscribe({
      next: (result: ClientData[] | null) => {
        this.clientTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Client changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditClientComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalClientCount$ = this.clientService.GetClientsRowCount({
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

      this.filteredClientCount$ = this.clientService.GetClientsRowCount({
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

      this.filteredClientCount$ = this.totalClientCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalClientCount$.subscribe();

    if (this.filteredClientCount$ != this.totalClientCount$) {
      this.filteredClientCount$.subscribe();
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
  public onFilterChange(): void {

    clearTimeout(this.debounceTimeout);

    this.debounceTimeout = setTimeout(() => {
      this.clientTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSchedulerClientReader(): boolean {
    return this.clientService.userIsSchedulerClientReader();
  }

  public userIsSchedulerClientWriter(): boolean {
    return this.clientService.userIsSchedulerClientWriter();
  }
}
