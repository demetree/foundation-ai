
import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NavigationService } from '../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { TenantProfileService, TenantProfileData, TenantProfileSubmitData } from '../../scheduler-data-services/tenant-profile.service';
import { StateProvinceService } from '../../scheduler-data-services/state-province.service';
import { CountryService } from '../../scheduler-data-services/country.service';
import { AuthService } from '../../services/auth.service';
import { TenantHelperService } from '../../services/tenant-helper.service';
import { BehaviorSubject } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { AddTenantProfileComponent } from '../add-tenant-profile/add-tenant-profile.component';


@Component({
  selector: 'app-administration',
  templateUrl: './administration.component.html',
  styleUrl: './administration.component.scss'
})


export class AdministrationComponent implements OnInit, CanComponentDeactivate {

  @ViewChild('tenantProfileForm') tenantProfileForm!: NgForm;           // Access the form instance
  @ViewChild(AddTenantProfileComponent) addCompanyProfileComponent!: AddTenantProfileComponent; 


  public tenantProfileId: string | null = null;
  public tenantProfileData: TenantProfileData;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  tenantProfiles$ = this.tenantProfileService.GetTenantProfileList();
  states$ = this.stateService.GetStateProvinceList();
  countries$ = this.countryService.GetCountryList();
  public stateName: string = '';
  public countryName: string = '';

  public isDownloadingExcel: boolean = false;

  constructor(
    private tenantProfileService: TenantProfileService,
    private tenantHelperService: TenantHelperService,
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private stateService: StateProvinceService,
    private countryService: CountryService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private alertService: AlertService,
    private navigationService: NavigationService) {

    // Assign a blank Tenant Profile until one is loaded.
    this.tenantProfileData = new TenantProfileData();
  }

  ngOnInit(): void {

    // Get the tenantProfileId from the route parameters
    this.tenantProfileId = this.route.snapshot.paramMap.get('tenantProfileId');
  }

  ngAfterViewInit(): void {

    // Load the data from the server
    this.LoadData();
  }

  public onEdit(companyProfile: TenantProfileData): void {
    if (this.addCompanyProfileComponent) {
      this.addCompanyProfileComponent.openModal(companyProfile); // Open modal with pre-filled client data
    } else {
      console.error('addCompanyProfileComponent is not initialized.');
    }
  }


  canDeactivate(): boolean {
    if (this.tenantProfileForm.dirty) {
      return confirm('You have unsaved changes. Are you sure you want to leave this page?');
    }
    return true;
  }

  getStateNameById(id: number | null): string | null {
    let stateName: string | null = null;
    this.states$.subscribe(states => {
      stateName = states.find(s => s.id === id)?.name ?? null;
    }).unsubscribe();
    return stateName;
  }

  getCountryNameById(id: number | null): string | null {
    let countryName: string | null = null;
    this.countries$.subscribe(countries => {
      countryName = countries.find(c => c.id === id)?.name ?? null;
    }).unsubscribe();
    return countryName;
  }

  GetTenantProfile(): void {
    const endpoint = `${this.baseUrl}api/Tenant/GetProfile`;
    const headers = this.authService.GetAuthenticationHeaders();


    this.http.get<any>(endpoint, { headers }).subscribe({
      next: result => {
        if (result) {
          this.tenantProfileData = result;

          // Lookup names
          this.states$.subscribe(states => {
            this.stateName = states.find(s => s.id === this.tenantProfileData.stateProvinceId)?.name ?? '';
          }).unsubscribe();
          this.countries$.subscribe(countries => {
            this.countryName = countries.find(c => c.id === this.tenantProfileData.countryId)?.name ?? '';
          }).unsubscribe();

          this.cdr.detectChanges();
        }
        

        this.isLoadingSubject.next(false); // Hide spinner
      },
      error: err => {

        this.alertService.showStickyMessage(
          "Load Failed",
          "Unable to load company Profile",
          MessageSeverity.error,
          err
        );
        this.isLoadingSubject.next(false);
      }
    });
  }



  LoadData(): void {

    if (this.tenantProfileService.userIsSchedulerTenantProfileReader() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to read from Tenant Profiles", '', MessageSeverity.info);
      return;
    }

    this.GetTenantProfile();
  }


  public goBack(): void {
    this.navigationService.goBack();
  }


  public submitForm() {

    if (this.tenantProfileService.userIsSchedulerTenantProfileWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Tenant Profiles", '', MessageSeverity.info);
      return;
    }


    const tenantProfileData: TenantProfileSubmitData = this.tenantProfileService.ConvertToTenantProfileSubmitData(this.tenantProfileData!);

    this.tenantProfileService.PutTenantProfile(tenantProfileData.id, tenantProfileData).subscribe({
      next: (updatedTenantProfileData) => {

        this.tenantProfileService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        this.tenantProfileData = updatedTenantProfileData;
        this.tenantProfileForm.resetForm(updatedTenantProfileData); // Resets form values and sets it to pristine
        this.alertService.showMessage("Tenant Profile saved successfully", '', MessageSeverity.success);
      },
      error: (err) => {
        this.alertService.showMessage("Tenant Profile could not be saved", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }

  public exportToExcel(): void {

    this.isDownloadingExcel = true; // Set the flag to indicate that the download is in progress

    this.tenantHelperService.exportToExcel().subscribe({
      next: () => {
        this.isDownloadingExcel = false; // Reset the flag after download is complete
        this.alertService.showSuccessMessage("Data Export", "Data exported successfully");
      },
      error: (error) => {
        this.isDownloadingExcel = false; // Reset the flag if an error occurs
        this.alertService.showHttpErrorMessage("Data could not be exported", error);
      }
    });
  }

  public userIsTenantProfileReader(): boolean {
    return this.tenantProfileService.userIsSchedulerTenantProfileReader();
  }

  public userIsTenantProfileWriter(): boolean {
    return this.tenantProfileService.userIsSchedulerTenantProfileWriter();
  }


  public userIsFoundationAdministrator(): boolean {
    return this.authService.isFoundationAdmin;
  }


  public userIsSchedulerAdministrator(): boolean {
    return this.authService.isSchedulerAdministrator;
  }

}
