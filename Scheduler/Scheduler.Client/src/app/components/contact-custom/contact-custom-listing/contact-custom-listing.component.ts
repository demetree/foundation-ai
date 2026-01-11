import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { Observable, map, finalize, startWith, shareReplay } from 'rxjs'
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { ContactService, ContactData } from '../../../scheduler-data-services/contact.service';
import { ContactCustomAddEditComponent } from '../contact-custom-add-edit/contact-custom-add-edit.component';
import { ContactCustomTableComponent } from '../contact-custom-table/contact-custom-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { TableColumn } from '../../../utility/foundation.utility';


@Component({
  selector: 'app-contact-custom-listing',
  templateUrl: './contact-custom-listing.component.html',
  styleUrls: ['./contact-custom-listing.component.scss']
})
export class ContactCustomListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(ContactCustomAddEditComponent) addEditContactComponent!: ContactCustomAddEditComponent;
  @ViewChild(ContactCustomTableComponent) contactTableComponent!: ContactCustomTableComponent;

  public Contacts: ContactData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public totalContactCount$: Observable<number> | null = null;
  public filteredContactCount$: Observable<number> | null = null;
  public loadingTotalCount = false;
  public loadingFilteredCount = false;

  private debounceTimeout: any;


  constructor(private contactService: ContactService,
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
    // Subscribe to the contactChanged observable on the add/edit component so that when a Contact changes we can reload the list.
    //
    this.addEditContactComponent.contactChanged.subscribe({
      next: (result: ContactData[] | null) => {
        this.contactTableComponent.loadData();
        this.loadCounts();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Contact changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditContactComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
    }
  }


  private loadCounts(): void {

    this.loadingTotalCount = true;
    this.loadingFilteredCount = true;

    // Total count (no filter)
    this.totalContactCount$ = this.contactService.GetContactsRowCount({
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

      this.filteredContactCount$ = this.contactService.GetContactsRowCount({
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

      this.filteredContactCount$ = this.totalContactCount$; // No filter → same as total
      this.loadingFilteredCount = false;
    }

    //
    // Subscribe to the observables to kick them off.  We don't want to depend on the template to do this.
    //
    // Although the templates would (and do) subscribe to these, fast filtering operations can get stuck in a loading state, so this is a defensive measure to eliminate that risk.
    //
    this.totalContactCount$.subscribe();

    if (this.filteredContactCount$ != this.totalContactCount$) {
      this.filteredContactCount$.subscribe();
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
      this.contactTableComponent.loadData(); // Refresh table
      this.loadCounts(); // Refresh both counts
    }, 100);
  }


  public buildCustomColumns(): TableColumn[] {

    //
    // Add the fields to the column config.  Refinements to widths and such can be made easily by custom users by tweaking the input as need be.
    //
    // Start with the common columns that everyone sees
    //
    const customColumns: TableColumn[] = [

      { key: 'firstName', label: 'First', width: undefined, mobile: 'prominent', template: 'link', linkPath: ['/contact', 'id'] },
      { key: 'middleName', label: 'Middle', width: undefined },
      { key: 'lastName', label: 'Last', width: undefined },
      { key: 'salutation.name', label: 'Salutation', width: undefined, template: 'link', linkPath: ['/salutation', 'salutationId'] },
      
      //{ key: 'birthDate', label: 'Birth Date', width: undefined, template: 'date' },
      { key: 'contactType.name', label: 'Type', width: undefined, template: 'link', linkPath: ['/contacttype', 'contactTypeId'] },

      { key: 'email', label: 'Email', width: undefined },
      { key: 'phone', label: 'Phone', width: undefined },
      { key: 'mobile', label: 'Mobile', width: undefined },

      { key: 'company', label: 'Company', width: undefined },
      { key: 'position', label: 'Position', width: undefined },
      //{ key: 'title', label: 'Title', width: undefined },


      //{ key: 'webSite', label: 'Web Site', width: undefined },
      //{ key: 'contactMethod.name', label: 'Contact Method', width: undefined, template: 'link', linkPath: ['/contactmethod', 'contactMethodId'] },
      //{ key: 'notes', label: 'Notes', width: undefined },
      //{ key: 'icon.name', label: 'Icon', width: undefined, template: 'link', linkPath: ['/icon', 'iconId'] },
      //{ key: 'color', label: 'Color', width: "50px", template: 'color' },
      // { key: 'avatarFileName', label: 'Avatar File Name', width: undefined },
      // { key: 'avatarSize', label: 'Avatar Size', width: undefined },
      // { key: 'avatarData', label: 'Avatar Data', width: undefined },
      // { key: 'avatarMimeType', label: 'Avatar Mime Type', width: undefined },
      // { key: 'externalId', label: 'External Id', width: undefined },


    ];

    const isWriter = this.contactService.userIsSchedulerContactWriter();
    const isAdmin = this.authService.isSchedulerAdministrator;

    if (isAdmin) {
      customColumns.push({ key: 'versionNumber', label: 'Version Number', width: undefined });
      customColumns.push({ key: 'active', label: 'Active', width: '120px', template: 'boolean' });
      customColumns.push({ key: 'deleted', label: 'Deleted', width: '120px', template: 'boolean' });

    }
    else if (isWriter) {
      customColumns.push({ key: 'active', label: 'Active', width: '120px', template: 'boolean' });
      customColumns.push({ key: 'deleted', label: 'Deleted', width: '120px', template: 'boolean' });
    }


    // Assign the built array as the active columns
    return customColumns;
  }




  public userIsSchedulerContactReader(): boolean {
    return this.contactService.userIsSchedulerContactReader();
  }

  public userIsSchedulerContactWriter(): boolean {
    return this.contactService.userIsSchedulerContactWriter();
  }
}
