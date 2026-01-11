import { Component, Input, Output, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { Subject } from 'rxjs'
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CalendarCustomAddEditComponent } from '../../calendar-custom/calendar-custom-add-edit/calendar-custom-add-edit.component'
import { Router } from '@angular/router';
import { OfficeData } from '../../../scheduler-data-services/office.service';
import { CalendarService, CalendarData } from '../../../scheduler-data-services/calendar.service';

/**
 * Resources tab for the Office detail page.
 *
 * Displays all calendars for this office
 * 
 * Data loaded imperatively when office is available.
 */
@Component({
  selector: 'app-office-calendars-tab',
  templateUrl: './office-calendars-tab.component.html',
  styleUrls: ['./office-calendars-tab.component.scss']
})
export class OfficeCalendarsTabComponent implements OnChanges {

  @ViewChild(CalendarCustomAddEditComponent) addEditCalendarComponent!: CalendarCustomAddEditComponent;

  /**
   * The office passed from the parent detail component.
   */
  @Input() office!: OfficeData | null;

  @Output() calendarChanged = new Subject<CalendarData>();


  /**
   * Resolved calendar for this office
   */
  public calendars: CalendarData[] | null = null;

  /**
   * Loading and error states.
   */
  public isLoading = true;
  public error: string | null = null;

  constructor(private router: Router,
    private modalService: NgbModal,
    private calendarService: CalendarService) { }

  ngAfterViewInit(): void {

    //
    // Subscribe to the observables on the add/edit components and emit when they emit
    //
    if (this.addEditCalendarComponent) {
      this.addEditCalendarComponent.calendarChanged.subscribe({
        next: (data: CalendarData[] | null) => {

          this.office?.ClearCalendarsCache();

          if (data != null && data.length > 0) {
            this.calendarChanged.next(data[0]);
          }
        },
        error: (err: any) => {
        }
      });
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['office'] && this.office) {

      this.office?.ClearCalendarsCache();

      this.loadCalendars();
    }
  }

  /**
   * Loads calendars using the office's lazy promise getter.
   */
  public loadCalendars(): void {

    if (!this.office) {
      this.calendars = [];
      this.isLoading = false;
      return;
    }

    this.isLoading = true;
    this.error = null;

    this.office.Calendars
      .then(resource => {
        this.calendars = resource;

        this.isLoading = false;
      })
      .catch(err => {
        console.error('Failed to load calendars', err);
        this.error = 'Unable to load calendars';
        this.calendars = [];
        this.isLoading = false;
      });
  }


  navigateToCalendar(calendarId: number | bigint | null | undefined): void {

    //
    // This routes to the resource details page.  This is fine, but the back button doens't bring back the previous tab current.
    //
    if (calendarId) {
      this.router.navigate(['/calendar', calendarId]);
    }
  }


  openCalendarModal(calendar: CalendarData | null | undefined): void {

    if (calendar == null || calendar == undefined || this.office == null) {
      return;
    }

    //
    // Opens up a modal to edit the crew
    //
    this.addEditCalendarComponent.officeId = this.office.id as number;

    this.addEditCalendarComponent.openModal(calendar); // Default edit behavior
  }


  //
  // Force a reload if there are changes
  // 
  public reload(data: CalendarData[]): void {

    this.office?.ClearCalendarsCache();

    this.loadCalendars();
  }


  public userIsSchedulerCalendarWriter(): boolean {
    return this.calendarService.userIsSchedulerCalendarWriter();
  }


  public openAddCalendarModal(): void {

    if (this.office == null) {
      return;
    }

    this.addEditCalendarComponent.navigateToDetailsAfterAdd = false;
    this.addEditCalendarComponent.officeId = this.office.id as number;
    this.addEditCalendarComponent.openModal();
  }
}
