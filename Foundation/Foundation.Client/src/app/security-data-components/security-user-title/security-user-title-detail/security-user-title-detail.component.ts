import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { SecurityUserTitleService, SecurityUserTitleData, SecurityUserTitleSubmitData } from '../../../security-data-services/security-user-title.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-security-user-title-detail',
  templateUrl: './security-user-title-detail.component.html',
  styleUrls: ['./security-user-title-detail.component.scss']
})

export class SecurityUserTitleDetailComponent implements OnInit, CanComponentDeactivate {

  securityUserTitleForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        active: [true],
        deleted: [false],
      });


  public securityUserTitleId: string | null = null;
  public securityUserTitleData: SecurityUserTitleData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  securityUserTitles$ = this.securityUserTitleService.GetSecurityUserTitleList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();

  private destroy$ = new Subject<void>();

  constructor(
    public securityUserTitleService: SecurityUserTitleService,
    public securityUserService: SecurityUserService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the securityUserTitleId from the route parameters
    this.securityUserTitleId = this.route.snapshot.paramMap.get('securityUserTitleId');

    if (this.securityUserTitleId === 'new' ||
        this.securityUserTitleId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.securityUserTitleData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Security User Title';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Security User Title';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.securityUserTitleForm.dirty) {
      return confirm('You have unsaved Security User Title changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.securityUserTitleId != null && this.securityUserTitleId !== 'new') {

      const id = parseInt(this.securityUserTitleId, 10);

      if (!isNaN(id)) {
        return { securityUserTitleId: id };
      }
    }

    return null;
  }


/*
  * Loads the SecurityUserTitle data for the current securityUserTitleId.
  *
  * Fully respects the SecurityUserTitleService caching strategy and error handling strategy.
  *
  * @param forceLoadAndDisplaySuccessAlert
  *   - true  will bypass cache entirely and show success alert message
  *   - false/null will use cache if available, no alert message
  */
  public loadData(forceLoadAndDisplaySuccessAlert: boolean | null = null): void {

    //
    // Start loading indicator immediately
    //
    this.isLoadingSubject.next(true);


    //
    // Permission Check
    //
    if (!this.securityUserTitleService.userIsSecuritySecurityUserTitleReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read SecurityUserTitles.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate securityUserTitleId
    //
    if (!this.securityUserTitleId) {

      this.alertService.showMessage('No SecurityUserTitle ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const securityUserTitleId = Number(this.securityUserTitleId);

    if (isNaN(securityUserTitleId) || securityUserTitleId <= 0) {

      this.alertService.showMessage(`Invalid Security User Title ID: "${this.securityUserTitleId}"`,
                                    'Invalid ID',
                                    MessageSeverity.error
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Force refresh: clear specific record cache only
    //
    if (forceLoadAndDisplaySuccessAlert === true) {
      // This is the most targeted way: clear only this SecurityUserTitle + relations

      this.securityUserTitleService.ClearRecordCache(securityUserTitleId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.securityUserTitleService.GetSecurityUserTitle(securityUserTitleId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (securityUserTitleData) => {

        //
        // Success path — securityUserTitleData can legitimately be null if 404'd but request succeeded
        //
        if (!securityUserTitleData) {

          this.handleSecurityUserTitleNotFound(securityUserTitleId);

        } else {

          this.securityUserTitleData = securityUserTitleData;
          this.buildFormValues(this.securityUserTitleData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'SecurityUserTitle loaded successfully',
              '',
              MessageSeverity.success
            );
          }
        }

        this.isLoadingSubject.next(false);
      },

      error: (error: any) => {
        //
        // All HTTP/network/parsing errors flow here
        // The service already stripped sensitive info and re-threw cleanly
        //
        this.handleSecurityUserTitleLoadError(error, securityUserTitleId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleSecurityUserTitleNotFound(securityUserTitleId: number): void {

    this.securityUserTitleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `SecurityUserTitle #${securityUserTitleId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleSecurityUserTitleLoadError(error: any, securityUserTitleId: number): void {

    let message = 'Failed to load Security User Title.';
    let title = 'Load Error';
    let severity = MessageSeverity.error;

    //
    // Leverage HTTP status if available
    //
    if (error?.status) {
      switch (error.status) {
        case 401:
          message = 'Your session has expired. Please log in again.';
          title = 'Unauthorized';
          break;
        case 403:
          message = 'You do not have permission to view this Security User Title.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Security User Title #${securityUserTitleId} was not found.`;
          title = 'Not Found';
          severity = MessageSeverity.warn;
          break;
        case 500:
          message = 'Server error. Please try again or contact support.';
          title = 'Server Error';
          break;
        case 0:
          message = 'Cannot reach server. Check your internet connection.';
          title = 'Offline';
          break;
        default:
          message = `Server error ${error.status || 'unknown'}: ${error.statusText || 'Request failed'}`;
      }
    } else {
      message = error?.message || message;
    }

    console.error(`Security User Title load failed (ID: ${securityUserTitleId})`, error);

    //
    // Reset UI to safe state
    //
    this.securityUserTitleData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(securityUserTitleData: SecurityUserTitleData | null) {

    if (securityUserTitleData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.securityUserTitleForm.reset({
        name: '',
        description: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.securityUserTitleForm.reset({
        name: securityUserTitleData.name ?? '',
        description: securityUserTitleData.description ?? '',
        active: securityUserTitleData.active ?? true,
        deleted: securityUserTitleData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.securityUserTitleForm.markAsPristine();
    this.securityUserTitleForm.markAsUntouched();
  }

  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public submitForm() {

    if (this.isSaving == true) {
      return;
    }

    if (this.securityUserTitleService.userIsSecuritySecurityUserTitleWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Security User Titles", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.securityUserTitleForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.securityUserTitleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.securityUserTitleForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const securityUserTitleSubmitData: SecurityUserTitleSubmitData = {
        id: this.securityUserTitleData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.securityUserTitleService.PutSecurityUserTitle(securityUserTitleSubmitData.id, securityUserTitleSubmitData)
      : this.securityUserTitleService.PostSecurityUserTitle(securityUserTitleSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedSecurityUserTitleData) => {

        this.securityUserTitleService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Security User Title's detail page
          //
          this.securityUserTitleForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.securityUserTitleForm.markAsUntouched();

          this.router.navigate(['/securityusertitles', savedSecurityUserTitleData.id]);
          this.alertService.showMessage('Security User Title added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.securityUserTitleData = savedSecurityUserTitleData;
          this.buildFormValues(this.securityUserTitleData);

          this.alertService.showMessage("Security User Title saved successfully", '', MessageSeverity.success);
        }
      },
      error: (err) => {

            let errorMessage: string;

            // Check if err is an Error object (e.g., new Error('message'))
            if (err instanceof Error) {
                errorMessage = err.message || 'An unexpected error occurred.';
            }
            // Check if err is a ServerError object with status and error properties
            else if (err.status && err.error)
            {
                if (err.status === 403)
                {
                    errorMessage = err.error?.message ||
                                   'You do not have permission to save this Security User Title.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Security User Title.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Security User Title could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecuritySecurityUserTitleReader(): boolean {
    return this.securityUserTitleService.userIsSecuritySecurityUserTitleReader();
  }

  public userIsSecuritySecurityUserTitleWriter(): boolean {
    return this.securityUserTitleService.userIsSecuritySecurityUserTitleWriter();
  }
}
