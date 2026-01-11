import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable, OnDestroy } from '@angular/core';
import { Observable, Subject, takeUntil, tap } from 'rxjs';
import { catchError, shareReplay } from 'rxjs/operators'
import { UtilityService } from '../utility-services/utility.service';
import { AlertService } from './alert.service';
import { AuthService } from './auth.service';
import { SecureEndpointBase } from './secure-endpoint-base.service';
import { TimeZoneService, TimeZoneData } from '../scheduler-data-services/time-zone.service'
import { CurrencyService, CurrencyData } from '../scheduler-data-services/currency.service'
import { CountryService, CountryData } from '../scheduler-data-services/country.service'
import { StateProvinceService, StateProvinceData } from '../scheduler-data-services/state-province.service'
import { OfficeService, OfficeData } from '../scheduler-data-services/office.service'


@Injectable({
  providedIn: 'root'
})
export class CurrentUserService extends SecureEndpointBase implements OnDestroy {

  //
  // Just a basic implementation for now.  Can get fancier later this is in use in places that span time zones and currencies etc..
  //
  public defaultTimeZoneId: number | null = null;
  public defaultCurrencyId: number | null = null;
  public defaultCountryId: number | null = null;
  public defaultStateProvinceId: number | null = null;
  public defaultOfficeId: number | null = null;


  /**
   * Subject used for cleanup of internal subscriptions.
   */
  private destroy$ = new Subject<void>();

  constructor(http: HttpClient,
    authService: AuthService,
    alertService: AlertService,
    private utilityService: UtilityService,
    private timeZoneService: TimeZoneService,
    private currencyService: CurrencyService,
    private countryService: CountryService,
    private stateProvinceService: StateProvinceService,
    private officeService: OfficeService,
    @Inject('BASE_URL') private baseUrl: string)
  {
    super(http, alertService, authService);

    this.loadDefaultTimeZone();
    this.loadDefaultCurrency();
    this.loadDefaultCountry();    // this calls get default state province 
    this.loadDefaultOffice();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  /**
   * Loads the list of active time zones and assigns the ID of the first entry
   * to defaultTimeZoneId.
   *
   * The TimeZoneService.GetTimeZoneList() method returns an Observable<TimeZoneData[]>.
   */
  private loadDefaultTimeZone(): void {
    this.timeZoneService
      .GetTimeZoneList({ active: true, deleted: false, pageSize: 1 })   // Take only one record that is active and not deleted.
      .pipe(
        // Use tap to perform a side-effect (assigning the default) without altering the stream
        tap((timeZoneData: TimeZoneData[]) => {
          if (timeZoneData && timeZoneData.length > 0) {
            // Assign the ID of the first time zone in the array
            this.defaultTimeZoneId = Number(timeZoneData[0].id);
          } else {
            
            this.defaultTimeZoneId = null;
            console.warn('No active time zones found.');
          }
        }),

        // Automatically unsubscribe when the destroy$ subject emits (service cleanup)
        takeUntil(this.destroy$),
      )
      .subscribe(); // We must subscribe to trigger the observable chain
  }


  private loadDefaultCurrency(): void {
    this.currencyService
      .GetCurrencyList({ active: true, deleted: false, pageSize: 1 })   // Take only one record that is active and not deleted.
      .pipe(
        // Use tap to perform a side-effect (assigning the default) without altering the stream
        tap((currencyData: CurrencyData[]) => {
          if (currencyData && currencyData.length > 0) {
            // Assign the ID of the first time zone in the array
            this.defaultCurrencyId = Number(currencyData[0].id);
          } else {
            
            this.defaultCurrencyId = null;
            console.warn('No active currencies found.');
          }
        }),

        // Automatically unsubscribe when the destroy$ subject emits (service cleanup)
        takeUntil(this.destroy$),
      )
      .subscribe(); // We must subscribe to trigger the observable chain
  }


  private loadDefaultCountry(): void {
    this.countryService
      .GetCountryList({ active: true, deleted: false, pageSize: 1 })   // Take only one record that is active and not deleted.
      .pipe(
        // Use tap to perform a side-effect (assigning the default) without altering the stream
        tap((countryData: CountryData[]) => {
          if (countryData && countryData.length > 0) {
            // Assign the ID of the first item in the array
            this.defaultCountryId = Number(countryData[0].id);

            this.loadDefaultStateProvince(this.defaultCountryId);

          } else {

            this.defaultCountryId = null;
            this.defaultStateProvinceId = null;
            console.warn('No active countries found.');
          }
        }),

        // Automatically unsubscribe when the destroy$ subject emits (service cleanup)
        takeUntil(this.destroy$),
      )
      .subscribe(); // We must subscribe to trigger the observable chain
  }


  private loadDefaultStateProvince(countryId: number): void {
    this.stateProvinceService
      .GetStateProvinceList({ active: true, deleted: false, countryId: countryId, pageSize: 1 })   // Take only one record that is active and not deleted.
      .pipe(
        // Use tap to perform a side-effect (assigning the default) without altering the stream
        tap((stateProvinceData: StateProvinceData[]) => {
          if (stateProvinceData && stateProvinceData.length > 0) {
            // Assign the ID of the first item in the array
            this.defaultStateProvinceId = Number(stateProvinceData[0].id);
          } else {

            this.defaultStateProvinceId = null;
            console.warn('No active states/provinces found.');
          }
        }),

        // Automatically unsubscribe when the destroy$ subject emits (service cleanup)
        takeUntil(this.destroy$),
      )
      .subscribe(); // We must subscribe to trigger the observable chain
  }


  private loadDefaultOffice(): void {
    this.officeService
      .GetOfficeList({ active: true, deleted: false, pageSize: 1 })   // Take only one record that is active and not deleted.
      .pipe(
        // Use tap to perform a side-effect (assigning the default) without altering the stream
        tap((OfficeData: OfficeData[]) => {
          if (OfficeData && OfficeData.length > 0) {
            // Assign the ID of the first item in the array
            this.defaultOfficeId = Number(OfficeData[0].id);
          } else {

            this.defaultOfficeId = null;
            console.warn('No active offices found.');
          }
        }),

        // Automatically unsubscribe when the destroy$ subject emits (service cleanup)
        takeUntil(this.destroy$),
      )
      .subscribe(); // We must subscribe to trigger the observable chain
  }

  public ClearAllCaches() {

    this.loadDefaultTimeZone();
    this.loadDefaultCurrency();
    this.loadDefaultCountry();    // this calls get default state province 
    this.loadDefaultOffice();
  }
}
