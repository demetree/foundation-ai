import { Component, OnInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs'
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { DocumentService } from '../../../scheduler-data-services/document.service';
import { DocumentCustomTableComponent } from '../document-custom-table/document-custom-table.component';
import { TableColumn } from '../../../utility/foundation.utility';


@Component({
  selector: 'app-document-custom-listing',
  templateUrl: './document-custom-listing.component.html',
  styleUrls: ['./document-custom-listing.component.scss']
})
export class DocumentCustomListingComponent implements OnInit {
  @ViewChild(DocumentCustomTableComponent) documentTableComponent!: DocumentCustomTableComponent;

  public isSmallScreen: boolean = false;
  public filterText: string | null = null;

  public totalDocumentCount$: Observable<number> | null = null;
  public filteredDocumentCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;


  constructor(private documentService: DocumentService,
    private navigationService: NavigationService,
    private breakpointObserver: BreakpointObserver) { }

  ngOnInit(): void {

    this.breakpointObserver
      .observe(['(max-width: 1100px)'])
      .subscribe((result) => {
        this.isSmallScreen = result.matches;
      });

    this.loadCounts();
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalDocumentCount$ = this.documentService.GetDocumentsRowCount({
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

      this.filteredDocumentCount$ = this.documentService.GetDocumentsRowCount({
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

      this.filteredDocumentCount$ = this.totalDocumentCount$; // No filter → same as total
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off defensively.
    //
    this.totalDocumentCount$.subscribe();

    if (this.filteredDocumentCount$ != this.totalDocumentCount$) {
      this.filteredDocumentCount$.subscribe();
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
  // Update the counts when the filter changes
  //
  onFilterChange(): void {

    clearTimeout(this.debounceTimeout);

    this.debounceTimeout = setTimeout(() => {
      this.documentTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 100);
  }


  public buildCustomColumns(): TableColumn[] {

    const customColumns: TableColumn[] = [

      { key: 'name', label: 'Name', width: undefined, mobile: 'prominent' },
      { key: 'documentType.name', label: 'Type', width: undefined },
      { key: 'status', label: 'Status', width: '120px' },
      { key: 'fileName', label: 'File Name', width: undefined },
      { key: 'uploadedDate', label: 'Uploaded', width: undefined, template: 'date' },
      { key: 'uploadedBy', label: 'Uploaded By', width: undefined },

      // Owner entity links
      { key: 'contact.firstName', label: 'Contact', width: undefined, template: 'link', linkPath: ['/contact', 'contactId'] },
      { key: 'resource.name', label: 'Resource', width: undefined, template: 'link', linkPath: ['/resource', 'resourceId'] },
      { key: 'scheduledEvent.name', label: 'Event', width: undefined, template: 'link', linkPath: ['/schedule', 'scheduledEventId'] },

    ];

    return customColumns;
  }
}
