import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EntityDataTokenService, EntityDataTokenData, EntityDataTokenSubmitData } from '../../../security-data-services/entity-data-token.service';
import { SecurityUserService } from '../../../security-data-services/security-user.service';
import { ModuleService } from '../../../security-data-services/module.service';
import { EntityDataTokenEventService } from '../../../security-data-services/entity-data-token-event.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-entity-data-token-detail',
  templateUrl: './entity-data-token-detail.component.html',
  styleUrls: ['./entity-data-token-detail.component.scss']
})

export class EntityDataTokenDetailComponent implements OnInit, CanComponentDeactivate {

  entityDataTokenForm: FormGroup = this.fb.group({
        securityUserId: [null, Validators.required],
        moduleId: [null, Validators.required],
        entity: ['', Validators.required],
        sessionId: ['', Validators.required],
        authenticationToken: ['', Validators.required],
        token: ['', Validators.required],
        timeStamp: ['', Validators.required],
        comments: [''],
        active: [true],
        deleted: [false],
      });


  public entityDataTokenId: string | null = null;
  public entityDataTokenData: EntityDataTokenData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  entityDataTokens$ = this.entityDataTokenService.GetEntityDataTokenList();
  securityUsers$ = this.securityUserService.GetSecurityUserList();
  modules$ = this.moduleService.GetModuleList();
  entityDataTokenEvents$ = this.entityDataTokenEventService.GetEntityDataTokenEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public entityDataTokenService: EntityDataTokenService,
    public securityUserService: SecurityUserService,
    public moduleService: ModuleService,
    public entityDataTokenEventService: EntityDataTokenEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the entityDataTokenId from the route parameters
    this.entityDataTokenId = this.route.snapshot.paramMap.get('entityDataTokenId');

    if (this.entityDataTokenId === 'new' ||
        this.entityDataTokenId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.entityDataTokenData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Entity Data Token';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Entity Data Token';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.entityDataTokenForm.dirty) {
      return confirm('You have unsaved Entity Data Token changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.entityDataTokenId != null && this.entityDataTokenId !== 'new') {

      const id = parseInt(this.entityDataTokenId, 10);

      if (!isNaN(id)) {
        return { entityDataTokenId: id };
      }
    }

    return null;
  }


/*
  * Loads the EntityDataToken data for the current entityDataTokenId.
  *
  * Fully respects the EntityDataTokenService caching strategy and error handling strategy.
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
    if (!this.entityDataTokenService.userIsSecurityEntityDataTokenReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read EntityDataTokens.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate entityDataTokenId
    //
    if (!this.entityDataTokenId) {

      this.alertService.showMessage('No EntityDataToken ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const entityDataTokenId = Number(this.entityDataTokenId);

    if (isNaN(entityDataTokenId) || entityDataTokenId <= 0) {

      this.alertService.showMessage(`Invalid Entity Data Token ID: "${this.entityDataTokenId}"`,
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
      // This is the most targeted way: clear only this EntityDataToken + relations

      this.entityDataTokenService.ClearRecordCache(entityDataTokenId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.entityDataTokenService.GetEntityDataToken(entityDataTokenId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (entityDataTokenData) => {

        //
        // Success path — entityDataTokenData can legitimately be null if 404'd but request succeeded
        //
        if (!entityDataTokenData) {

          this.handleEntityDataTokenNotFound(entityDataTokenId);

        } else {

          this.entityDataTokenData = entityDataTokenData;
          this.buildFormValues(this.entityDataTokenData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'EntityDataToken loaded successfully',
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
        this.handleEntityDataTokenLoadError(error, entityDataTokenId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleEntityDataTokenNotFound(entityDataTokenId: number): void {

    this.entityDataTokenData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `EntityDataToken #${entityDataTokenId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleEntityDataTokenLoadError(error: any, entityDataTokenId: number): void {

    let message = 'Failed to load Entity Data Token.';
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
          message = 'You do not have permission to view this Entity Data Token.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Entity Data Token #${entityDataTokenId} was not found.`;
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

    console.error(`Entity Data Token load failed (ID: ${entityDataTokenId})`, error);

    //
    // Reset UI to safe state
    //
    this.entityDataTokenData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(entityDataTokenData: EntityDataTokenData | null) {

    if (entityDataTokenData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.entityDataTokenForm.reset({
        securityUserId: null,
        moduleId: null,
        entity: '',
        sessionId: '',
        authenticationToken: '',
        token: '',
        timeStamp: '',
        comments: '',
        active: true,
        deleted: false,
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.entityDataTokenForm.reset({
        securityUserId: entityDataTokenData.securityUserId,
        moduleId: entityDataTokenData.moduleId,
        entity: entityDataTokenData.entity ?? '',
        sessionId: entityDataTokenData.sessionId ?? '',
        authenticationToken: entityDataTokenData.authenticationToken ?? '',
        token: entityDataTokenData.token ?? '',
        timeStamp: isoUtcStringToDateTimeLocal(entityDataTokenData.timeStamp) ?? '',
        comments: entityDataTokenData.comments ?? '',
        active: entityDataTokenData.active ?? true,
        deleted: entityDataTokenData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.entityDataTokenForm.markAsPristine();
    this.entityDataTokenForm.markAsUntouched();
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

    if (this.entityDataTokenService.userIsSecurityEntityDataTokenWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Entity Data Tokens", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.entityDataTokenForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.entityDataTokenForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.entityDataTokenForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const entityDataTokenSubmitData: EntityDataTokenSubmitData = {
        id: this.entityDataTokenData?.id || 0,
        securityUserId: Number(formValue.securityUserId),
        moduleId: Number(formValue.moduleId),
        entity: formValue.entity!.trim(),
        sessionId: formValue.sessionId!.trim(),
        authenticationToken: formValue.authenticationToken!.trim(),
        token: formValue.token!.trim(),
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.entityDataTokenService.PutEntityDataToken(entityDataTokenSubmitData.id, entityDataTokenSubmitData)
      : this.entityDataTokenService.PostEntityDataToken(entityDataTokenSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedEntityDataTokenData) => {

        this.entityDataTokenService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Entity Data Token's detail page
          //
          this.entityDataTokenForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.entityDataTokenForm.markAsUntouched();

          this.router.navigate(['/entitydatatokens', savedEntityDataTokenData.id]);
          this.alertService.showMessage('Entity Data Token added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.entityDataTokenData = savedEntityDataTokenData;
          this.buildFormValues(this.entityDataTokenData);

          this.alertService.showMessage("Entity Data Token saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Entity Data Token.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Entity Data Token.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Entity Data Token could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecurityEntityDataTokenReader(): boolean {
    return this.entityDataTokenService.userIsSecurityEntityDataTokenReader();
  }

  public userIsSecurityEntityDataTokenWriter(): boolean {
    return this.entityDataTokenService.userIsSecurityEntityDataTokenWriter();
  }
}
