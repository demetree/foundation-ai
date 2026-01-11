import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { OAUTHTokenService, OAUTHTokenData, OAUTHTokenSubmitData } from '../../../security-data-services/o-a-u-t-h-token.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-o-a-u-t-h-token-detail',
  templateUrl: './o-a-u-t-h-token-detail.component.html',
  styleUrls: ['./o-a-u-t-h-token-detail.component.scss']
})

export class OAUTHTokenDetailComponent implements OnInit, CanComponentDeactivate {

  oAUTHTokenForm: FormGroup = this.fb.group({
        token: ['', Validators.required],
        expiryDateTime: ['', Validators.required],
        userData: [''],
        active: [true],
        deleted: [false],
      });


  public oAUTHTokenId: string | null = null;
  public oAUTHTokenData: OAUTHTokenData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  oAUTHTokens$ = this.oAUTHTokenService.GetOAUTHTokenList();

  private destroy$ = new Subject<void>();

  constructor(
    public oAUTHTokenService: OAUTHTokenService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the oAUTHTokenId from the route parameters
    this.oAUTHTokenId = this.route.snapshot.paramMap.get('oAUTHTokenId');

    if (this.oAUTHTokenId === 'new' ||
        this.oAUTHTokenId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.oAUTHTokenData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New O A U T H Token';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit O A U T H Token';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.oAUTHTokenForm.dirty) {
      return confirm('You have unsaved O A U T H Token changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.oAUTHTokenId != null && this.oAUTHTokenId !== 'new') {

      const id = parseInt(this.oAUTHTokenId, 10);

      if (!isNaN(id)) {
        return { oAUTHTokenId: id };
      }
    }

    return null;
  }


/*
  * Loads the OAUTHToken data for the current oAUTHTokenId.
  *
  * Fully respects the OAUTHTokenService caching strategy and error handling strategy.
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
    if (!this.oAUTHTokenService.userIsSecurityOAUTHTokenReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read OAUTHTokens.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate oAUTHTokenId
    //
    if (!this.oAUTHTokenId) {

      this.alertService.showMessage('No OAUTHToken ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const oAUTHTokenId = Number(this.oAUTHTokenId);

    if (isNaN(oAUTHTokenId) || oAUTHTokenId <= 0) {

      this.alertService.showMessage(`Invalid O A U T H Token ID: "${this.oAUTHTokenId}"`,
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
      // This is the most targeted way: clear only this OAUTHToken + relations

      this.oAUTHTokenService.ClearRecordCache(oAUTHTokenId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.oAUTHTokenService.GetOAUTHToken(oAUTHTokenId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (oAUTHTokenData) => {

        //
        // Success path — oAUTHTokenData can legitimately be null if 404'd but request succeeded
        //
        if (!oAUTHTokenData) {

          this.handleOAUTHTokenNotFound(oAUTHTokenId);

        } else {

          this.oAUTHTokenData = oAUTHTokenData;
          this.buildFormValues(this.oAUTHTokenData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'OAUTHToken loaded successfully',
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
        this.handleOAUTHTokenLoadError(error, oAUTHTokenId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleOAUTHTokenNotFound(oAUTHTokenId: number): void {

    this.oAUTHTokenData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `OAUTHToken #${oAUTHTokenId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleOAUTHTokenLoadError(error: any, oAUTHTokenId: number): void {

    let message = 'Failed to load O A U T H Token.';
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
          message = 'You do not have permission to view this O A U T H Token.';
          title = 'Forbidden';
          break;
        case 404:
          message = `O A U T H Token #${oAUTHTokenId} was not found.`;
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

    console.error(`O A U T H Token load failed (ID: ${oAUTHTokenId})`, error);

    //
    // Reset UI to safe state
    //
    this.oAUTHTokenData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(oAUTHTokenData: OAUTHTokenData | null) {

    if (oAUTHTokenData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.oAUTHTokenForm.reset({
        token: '',
        expiryDateTime: '',
        userData: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.oAUTHTokenForm.reset({
        token: oAUTHTokenData.token ?? '',
        expiryDateTime: isoUtcStringToDateTimeLocal(oAUTHTokenData.expiryDateTime) ?? '',
        userData: oAUTHTokenData.userData ?? '',
        active: oAUTHTokenData.active ?? true,
        deleted: oAUTHTokenData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.oAUTHTokenForm.markAsPristine();
    this.oAUTHTokenForm.markAsUntouched();
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

    if (this.oAUTHTokenService.userIsSecurityOAUTHTokenWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to O A U T H Tokens", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.oAUTHTokenForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.oAUTHTokenForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.oAUTHTokenForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const oAUTHTokenSubmitData: OAUTHTokenSubmitData = {
        id: this.oAUTHTokenData?.id || 0,
        token: formValue.token!.trim(),
        expiryDateTime: dateTimeLocalToIsoUtc(formValue.expiryDateTime!.trim())!,
        userData: formValue.userData?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.oAUTHTokenService.PutOAUTHToken(oAUTHTokenSubmitData.id, oAUTHTokenSubmitData)
      : this.oAUTHTokenService.PostOAUTHToken(oAUTHTokenSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedOAUTHTokenData) => {

        this.oAUTHTokenService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created O A U T H Token's detail page
          //
          this.oAUTHTokenForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.oAUTHTokenForm.markAsUntouched();

          this.router.navigate(['/oauthtokens', savedOAUTHTokenData.id]);
          this.alertService.showMessage('O A U T H Token added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.oAUTHTokenData = savedOAUTHTokenData;
          this.buildFormValues(this.oAUTHTokenData);

          this.alertService.showMessage("O A U T H Token saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this O A U T H Token.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the O A U T H Token.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('O A U T H Token could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecurityOAUTHTokenReader(): boolean {
    return this.oAUTHTokenService.userIsSecurityOAUTHTokenReader();
  }

  public userIsSecurityOAUTHTokenWriter(): boolean {
    return this.oAUTHTokenService.userIsSecurityOAUTHTokenWriter();
  }
}
