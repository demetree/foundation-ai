import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { RateSheetService, RateSheetData } from '../../../scheduler-data-services/rate-sheet.service';
import { RateSheetCustomAddEditComponent } from '../rate-sheet-custom-add-edit/rate-sheet-custom-add-edit.component'
import { RateSheetCustomTableComponent } from '../rate-sheet-custom-table/rate-sheet-custom-table.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AssignmentRoleService } from '../../../scheduler-data-services/assignment-role.service';
import { ResourceService } from '../../../scheduler-data-services/resource.service';
import { SchedulingTargetService } from '../../../scheduler-data-services/scheduling-target.service';
import { RateTypeService } from '../../../scheduler-data-services/rate-type.service';
import { CurrencyService } from '../../../scheduler-data-services/currency.service';
import { OfficeService } from '../../../scheduler-data-services/office.service';
import { SchedulerHelperService } from '../../../services/scheduler-helper.service';

@Component({
  selector: 'app-rate-sheet-custom-listing',
  templateUrl: './rate-sheet-custom-listing.component.html',
  styleUrls: ['./rate-sheet-custom-listing.component.scss']
})
export class RateSheetCustomListingComponent implements OnInit, AfterViewInit, CanComponentDeactivate {
  @ViewChild(RateSheetCustomAddEditComponent) addEditRateSheetComponent!: RateSheetCustomAddEditComponent;
  @ViewChild(RateSheetCustomTableComponent) rateSheetTableComponent!: RateSheetCustomTableComponent;

  public RateSheets: RateSheetData[] | null = null;
  public isSmallScreen: boolean = false;

  public filterText: string | null = null;

  public showPreview: boolean = false;

  public offices$ = this.officeService.GetOfficeList();
  public rateSheets$ = this.rateSheetService.GetRateSheetList();
  public assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  public resources$ = this.resourceService.GetResourceList();
  public schedulingTargets$ = this.schedulingTargetService.GetSchedulingTargetList();
  public rateTypes$ = this.rateTypeService.GetRateTypeList();
  public currencies$ = this.currencyService.GetCurrencyList();


  // To get the count of offices to allow the offices button to be invisible if there are no offices (It can always be found under Administration)
  public officeCount$ = this.schedulerHelperService.ActiveOfficeCount$;

  // Rate Preview State
  public preview = {
    officeId: null as number | null,
    resourceId: null as number | null,
    assignmentRoleId: null as number | null,
    schedulingTargetId: null as number | null,
    rateTypeId: null as number | null,
    date: new Date().toISOString().split('T')[0], // Today
    result: null as any,
    error: null as string | null,
    isLoading: false
  };

  private previewDebounce: any;

  constructor(private rateSheetService: RateSheetService,
              private officeService: OfficeService,
              private assignmentRoleService: AssignmentRoleService,
              private resourceService: ResourceService,
              private schedulingTargetService: SchedulingTargetService,
              private rateTypeService: RateTypeService,
              private currencyService: CurrencyService,
              private alertService: AlertService,
              private navigationService: NavigationService,
              private schedulerHelperService: SchedulerHelperService,
              private breakpointObserver: BreakpointObserver) { }

  ngOnInit(): void {

    this.breakpointObserver
      .observe(['(max-width: 1100px)']) // this size is specified to try and find a balance so tablets and phone see cards, but wider screens get a table.
      .subscribe((result) => {
        this.isSmallScreen = result.matches;
      });
  }

  ngAfterViewInit(): void {
    //
    // Subscribe to the rateSheetChanged observable on the add/edit component so that when a RateSheet changes we can reload the list.
    //
    this.addEditRateSheetComponent.rateSheetChanged.subscribe({
      next: (result: RateSheetData[] | null) => {
        this.rateSheetTableComponent.loadData();
      },
      error: (err: any) => {
         this.alertService.showMessage("Error during Rate Sheet changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  canDeactivate(): boolean {
    //
    // Do not allow route changes when the modal is up.
    //
    if (this.addEditRateSheetComponent.modalIsDisplayed == true) {
      return false;
    } else {
      return true;
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


  onFilterChange(): void {
  }



  public userIsSchedulerRateSheetReader(): boolean {
    return this.rateSheetService.userIsSchedulerRateSheetReader();
  }

  public userIsSchedulerRateSheetWriter(): boolean {
    return this.rateSheetService.userIsSchedulerRateSheetWriter();
  }

  /**
* Calls the backend Rate Resolve endpoint with current preview values
* Debounced to avoid hammering server on every keystroke
*/
  public previewRate(): void {
    clearTimeout(this.previewDebounce);


    this.preview.isLoading = true;
    this.preview.error = null;
    this.preview.result = null;

    this.previewDebounce = setTimeout(() => {
      this.schedulerHelperService.resolveRate({
        officeId: this.preview.officeId,
        resourceId: this.preview.resourceId,
        assignmentRoleId: this.preview.assignmentRoleId,
        schedulingTargetId: this.preview.schedulingTargetId,
        rateTypeId: this.preview.rateTypeId!,
        date: this.preview.date
      }).subscribe({
        next: (result) => {
          this.preview.result = result;
          this.preview.isLoading = false;
        },
        error: (err) => {
          this.preview.error = err.error?.message || 'Unable to resolve rate';
          this.preview.isLoading = false;
        }
      });
    }, 500); // 500ms debounce
  }

  /**
 * Opens the Add/Edit modal with the RateSheet that was matched in the preview
 * @param rateSheetId The ID from the resolver result
 */
  public editMatchedRateSheet(rateSheetId: number): void {
    if (!rateSheetId) {
      this.alertService.showMessage('No Rate Sheet to edit', 'Invalid ID', MessageSeverity.warn);
      return;
    }

    // Fetch the full RateSheet data (or use cached if available)
    this.rateSheetService.GetRateSheet(rateSheetId).subscribe({
      next: (rateSheet) => {
        if (rateSheet) {
          // Open the modal in edit mode with this data
          this.addEditRateSheetComponent.openModal(rateSheet);
        } else {
          this.alertService.showMessage('Rate Sheet not found', '', MessageSeverity.error);
        }
      },
      error: (err) => {
        this.alertService.showMessage('Error loading Rate Sheet for edit', JSON.stringify(err), MessageSeverity.error);
      }
    });
  }
}
