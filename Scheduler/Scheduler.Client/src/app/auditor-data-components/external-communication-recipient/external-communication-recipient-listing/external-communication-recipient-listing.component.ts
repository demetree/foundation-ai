import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ExternalCommunicationRecipientService, ExternalCommunicationRecipientData } from '../../../auditor-data-services/external-communication-recipient.service';
import { ExternalCommunicationRecipientAddEditComponent } from '../external-communication-recipient-add-edit/external-communication-recipient-add-edit.component';
import { ExternalCommunicationRecipientTableComponent } from '../external-communication-recipient-table/external-communication-recipient-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-external-communication-recipient-listing',
  templateUrl: './external-communication-recipient-listing.component.html',
  styleUrls: ['./external-communication-recipient-listing.component.scss']
})
export class ExternalCommunicationRecipientListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ExternalCommunicationRecipientAddEditComponent) addEditExternalCommunicationRecipientComponent!: ExternalCommunicationRecipientAddEditComponent;
  @ViewChild(ExternalCommunicationRecipientTableComponent) externalCommunicationRecipientTableComponent!: ExternalCommunicationRecipientTableComponent;

  public ExternalCommunicationRecipients: ExternalCommunicationRecipientData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalExternalCommunicationRecipientCount$ : Observable<number> | null = null;
  public filteredExternalCommunicationRecipientCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;

  constructor(private externalCommunicationRecipientService: ExternalCommunicationRecipientService,
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
    // Subscribe to the externalCommunicationRecipientChanged observable on the add/edit component so that when a ExternalCommunicationRecipient changes we can reload the list.
    //
    this.addEditExternalCommunicationRecipientComponent.externalCommunicationRecipientChanged.subscribe({
      next: (result: ExternalCommunicationRecipientData[] | null) => {
        this.externalCommunicationRecipientTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during External Communication Recipient changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditExternalCommunicationRecipientComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalExternalCommunicationRecipientCount$ = this.externalCommunicationRecipientService.GetExternalCommunicationRecipientsRowCount({
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

      this.filteredExternalCommunicationRecipientCount$ = this.externalCommunicationRecipientService.GetExternalCommunicationRecipientsRowCount({
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

      this.filteredExternalCommunicationRecipientCount$ = this.totalExternalCommunicationRecipientCount$; // No filter, so assign to be same as total observable
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, repeated fast filtering operations can sometimes get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalExternalCommunicationRecipientCount$.subscribe();

    if (this.filteredExternalCommunicationRecipientCount$ != this.totalExternalCommunicationRecipientCount$) {
      this.filteredExternalCommunicationRecipientCount$.subscribe();
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
      this.externalCommunicationRecipientTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 500);           // 500 millisecond debounce
  }



  public userIsAuditorExternalCommunicationRecipientReader(): boolean {
    return this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientReader();
  }

  public userIsAuditorExternalCommunicationRecipientWriter(): boolean {
    return this.externalCommunicationRecipientService.userIsAuditorExternalCommunicationRecipientWriter();
  }
}
