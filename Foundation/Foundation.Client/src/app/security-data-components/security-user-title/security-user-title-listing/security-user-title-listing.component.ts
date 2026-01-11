import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { SecurityUserTitleService, SecurityUserTitleData } from '../../../security-data-services/security-user-title.service';
import { SecurityUserTitleAddEditComponent } from '../security-user-title-add-edit/security-user-title-add-edit.component';
import { SecurityUserTitleTableComponent } from '../security-user-title-table/security-user-title-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-security-user-title-listing',
  templateUrl: './security-user-title-listing.component.html',
  styleUrls: ['./security-user-title-listing.component.scss']
})
export class SecurityUserTitleListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(SecurityUserTitleAddEditComponent) addEditSecurityUserTitleComponent!: SecurityUserTitleAddEditComponent;
  @ViewChild(SecurityUserTitleTableComponent) securityUserTitleTableComponent!: SecurityUserTitleTableComponent;

  public SecurityUserTitles: SecurityUserTitleData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalSecurityUserTitleCount$ : Observable<number> | null = null;
  public filteredSecurityUserTitleCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private securityUserTitleService: SecurityUserTitleService,
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
    // Subscribe to the securityUserTitleChanged observable on the add/edit component so that when a SecurityUserTitle changes we can reload the list.
    //
    this.addEditSecurityUserTitleComponent.securityUserTitleChanged.subscribe({
      next: (result: SecurityUserTitleData[] | null) => {
        this.securityUserTitleTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Security User Title changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditSecurityUserTitleComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalSecurityUserTitleCount$ = this.securityUserTitleService.GetSecurityUserTitlesRowCount({
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

      this.filteredSecurityUserTitleCount$ = this.securityUserTitleService.GetSecurityUserTitlesRowCount({
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

      this.filteredSecurityUserTitleCount$ = this.totalSecurityUserTitleCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalSecurityUserTitleCount$.subscribe();

    if (this.filteredSecurityUserTitleCount$ != this.totalSecurityUserTitleCount$) {
      this.filteredSecurityUserTitleCount$.subscribe();
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
      this.securityUserTitleTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsSecuritySecurityUserTitleReader(): boolean {
    return this.securityUserTitleService.userIsSecuritySecurityUserTitleReader();
  }

  public userIsSecuritySecurityUserTitleWriter(): boolean {
    return this.securityUserTitleService.userIsSecuritySecurityUserTitleWriter();
  }
}
