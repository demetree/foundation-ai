import { Component, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { Subject } from 'rxjs'
import { OfficeData } from '../../../scheduler-data-services/office.service';
import { RateSheetData } from '../../../scheduler-data-services/rate-sheet.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { OfficeRateOverrideAddModalComponent } from '../office-rate-sheet-override-add-modal/office-rate-sheet-override-add-modal.component';

/**
 * Rate Overrides tab for the Office detail page.
 *
 * Displays all rate sheet overrides specific to this office.
 * Shows current active rate prominently.
 *
 * Data loaded imperatively when tab is active.
 */
@Component({
  selector: 'app-office-rates-tab',
  templateUrl: './office-rates-tab.component.html',
  styleUrls: ['./office-rates-tab.component.scss']
})
export class OfficeRatesTabComponent implements OnChanges {

  @Input() office!: OfficeData | null;

  // Triggers when a rate sheet is changed.  To be implemented by users of this component.
  @Output() rateSheetChanged = new Subject<RateSheetData>();


  public rateSheets: RateSheetData[] | null = null;
  public isLoading = true;
  public error: string | null = null;

  constructor(private modalService: NgbModal) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['office'] && this.office) {

      this.office.ClearRateSheetsCache();

      this.loadRateSheets();
    }
  }

  /**
   * Loads rate sheet overrides using the office's lazy promise getter.
   */
  public loadRateSheets(): void {
    if (!this.office) {
      this.rateSheets = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.office.RateSheets
      .then(sheets => {
        // Sort by effective date descending (most recent first)
        this.rateSheets = sheets.sort((a, b) =>
          new Date(b.effectiveDate).getTime() - new Date(a.effectiveDate).getTime()
        );
        this.isLoading = false;
      })
      .catch(err => {
        console.error('Failed to load rate overrides', err);
        this.error = 'Unable to load rate overrides';
        this.rateSheets = [];
        this.isLoading = false;
      });
  }

  public openAddRateOverrideModal(): void {
    const modalRef = this.modalService.open(OfficeRateOverrideAddModalComponent, {
      size: 'lg',
      backdrop: 'static'
    });

    modalRef.componentInstance.officeId = this.office!.id;
    modalRef.componentInstance.officeName = this.office!.name;

    modalRef.result.then(
      (data) => {
        this.office?.ClearRateSheetsCache();
        this.rateSheetChanged.next(data);
        this.loadRateSheets()
      },
      () => { }
    );
  }

  /**
   * Determines status badge class and text for a rate sheet
   */
  public getRateStatus(sheet: RateSheetData): { badgeClass: string; text: string } {
    const now = new Date();
    const effective = new Date(sheet.effectiveDate);

    if (effective > now) {
      return { badgeClass: 'bg-info', text: 'Future' };
    } else {
      return { badgeClass: 'bg-success', text: 'Active' };
    }
  }

  /**
   * Formats currency amount with symbol
   */
  public formatCurrency(amount: number | null, currencyCode: string | null): string {
    if (amount == null || !currencyCode) return '—';
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: currencyCode
    }).format(amount);
  }
}
