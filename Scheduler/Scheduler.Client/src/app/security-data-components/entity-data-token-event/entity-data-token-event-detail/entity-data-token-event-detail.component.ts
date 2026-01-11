import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EntityDataTokenEventService, EntityDataTokenEventData, EntityDataTokenEventSubmitData } from '../../../security-data-services/entity-data-token-event.service';
import { EntityDataTokenService } from '../../../security-data-services/entity-data-token.service';
import { EntityDataTokenEventTypeService } from '../../../security-data-services/entity-data-token-event-type.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-entity-data-token-event-detail',
  templateUrl: './entity-data-token-event-detail.component.html',
  styleUrls: ['./entity-data-token-event-detail.component.scss']
})

export class EntityDataTokenEventDetailComponent implements OnInit, CanComponentDeactivate {

  entityDataTokenEventForm: FormGroup = this.fb.group({
        entityDataTokenId: [null, Validators.required],
        entityDataTokenEventTypeId: [null, Validators.required],
        timeStamp: ['', Validators.required],
        comments: [''],
        active: [true],
        deleted: [false],
      });


  public entityDataTokenEventId: string | null = null;
  public entityDataTokenEventData: EntityDataTokenEventData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  entityDataTokenEvents$ = this.entityDataTokenEventService.GetEntityDataTokenEventList();
  entityDataTokens$ = this.entityDataTokenService.GetEntityDataTokenList();
  entityDataTokenEventTypes$ = this.entityDataTokenEventTypeService.GetEntityDataTokenEventTypeList();

  private destroy$ = new Subject<void>();

  constructor(
    public entityDataTokenEventService: EntityDataTokenEventService,
    public entityDataTokenService: EntityDataTokenService,
    public entityDataTokenEventTypeService: EntityDataTokenEventTypeService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the entityDataTokenEventId from the route parameters
    this.entityDataTokenEventId = this.route.snapshot.paramMap.get('entityDataTokenEventId');

    if (this.entityDataTokenEventId === 'new' ||
        this.entityDataTokenEventId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.entityDataTokenEventData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Entity Data Token Event';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Entity Data Token Event';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.entityDataTokenEventForm.dirty) {
      return confirm('You have unsaved Entity Data Token Event changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.entityDataTokenEventId != null && this.entityDataTokenEventId !== 'new') {

      const id = parseInt(this.entityDataTokenEventId, 10);

      if (!isNaN(id)) {
        return { entityDataTokenEventId: id };
      }
    }

    return null;
  }


/*
  * Loads the EntityDataTokenEvent data for the current entityDataTokenEventId.
  *
  * Fully respects the EntityDataTokenEventService caching strategy and error handling strategy.
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
    if (!this.entityDataTokenEventService.userIsSecurityEntityDataTokenEventReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read EntityDataTokenEvents.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate entityDataTokenEventId
    //
    if (!this.entityDataTokenEventId) {

      this.alertService.showMessage('No EntityDataTokenEvent ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const entityDataTokenEventId = Number(this.entityDataTokenEventId);

    if (isNaN(entityDataTokenEventId) || entityDataTokenEventId <= 0) {

      this.alertService.showMessage(`Invalid Entity Data Token Event ID: "${this.entityDataTokenEventId}"`,
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
      // This is the most targeted way: clear only this EntityDataTokenEvent + relations

      this.entityDataTokenEventService.ClearRecordCache(entityDataTokenEventId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.entityDataTokenEventService.GetEntityDataTokenEvent(entityDataTokenEventId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (entityDataTokenEventData) => {

        //
        // Success path — entityDataTokenEventData can legitimately be null if 404'd but request succeeded
        //
        if (!entityDataTokenEventData) {

          this.handleEntityDataTokenEventNotFound(entityDataTokenEventId);

        } else {

          this.entityDataTokenEventData = entityDataTokenEventData;
          this.buildFormValues(this.entityDataTokenEventData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'EntityDataTokenEvent loaded successfully',
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
        this.handleEntityDataTokenEventLoadError(error, entityDataTokenEventId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleEntityDataTokenEventNotFound(entityDataTokenEventId: number): void {

    this.entityDataTokenEventData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `EntityDataTokenEvent #${entityDataTokenEventId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleEntityDataTokenEventLoadError(error: any, entityDataTokenEventId: number): void {

    let message = 'Failed to load Entity Data Token Event.';
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
          message = 'You do not have permission to view this Entity Data Token Event.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Entity Data Token Event #${entityDataTokenEventId} was not found.`;
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

    console.error(`Entity Data Token Event load failed (ID: ${entityDataTokenEventId})`, error);

    //
    // Reset UI to safe state
    //
    this.entityDataTokenEventData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(entityDataTokenEventData: EntityDataTokenEventData | null) {

    if (entityDataTokenEventData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.entityDataTokenEventForm.reset({
        entityDataTokenId: null,
        entityDataTokenEventTypeId: null,
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
        this.entityDataTokenEventForm.reset({
        entityDataTokenId: entityDataTokenEventData.entityDataTokenId,
        entityDataTokenEventTypeId: entityDataTokenEventData.entityDataTokenEventTypeId,
        timeStamp: isoUtcStringToDateTimeLocal(entityDataTokenEventData.timeStamp) ?? '',
        comments: entityDataTokenEventData.comments ?? '',
        active: entityDataTokenEventData.active ?? true,
        deleted: entityDataTokenEventData.deleted ?? false,
      }, { emitEvent: false});
    }

    this.entityDataTokenEventForm.markAsPristine();
    this.entityDataTokenEventForm.markAsUntouched();
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

    if (this.entityDataTokenEventService.userIsSecurityEntityDataTokenEventWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Entity Data Token Events", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.entityDataTokenEventForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.entityDataTokenEventForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.entityDataTokenEventForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const entityDataTokenEventSubmitData: EntityDataTokenEventSubmitData = {
        id: this.entityDataTokenEventData?.id || 0,
        entityDataTokenId: Number(formValue.entityDataTokenId),
        entityDataTokenEventTypeId: Number(formValue.entityDataTokenEventTypeId),
        timeStamp: dateTimeLocalToIsoUtc(formValue.timeStamp!.trim())!,
        comments: formValue.comments?.trim() || null,
        active: !!formValue.active,
        deleted: !!formValue.deleted,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.entityDataTokenEventService.PutEntityDataTokenEvent(entityDataTokenEventSubmitData.id, entityDataTokenEventSubmitData)
      : this.entityDataTokenEventService.PostEntityDataTokenEvent(entityDataTokenEventSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedEntityDataTokenEventData) => {

        this.entityDataTokenEventService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Entity Data Token Event's detail page
          //
          this.entityDataTokenEventForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.entityDataTokenEventForm.markAsUntouched();

          this.router.navigate(['/entitydatatokenevents', savedEntityDataTokenEventData.id]);
          this.alertService.showMessage('Entity Data Token Event added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.entityDataTokenEventData = savedEntityDataTokenEventData;
          this.buildFormValues(this.entityDataTokenEventData);

          this.alertService.showMessage("Entity Data Token Event saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Entity Data Token Event.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Entity Data Token Event.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Entity Data Token Event could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecurityEntityDataTokenEventReader(): boolean {
    return this.entityDataTokenEventService.userIsSecurityEntityDataTokenEventReader();
  }

  public userIsSecurityEntityDataTokenEventWriter(): boolean {
    return this.entityDataTokenEventService.userIsSecurityEntityDataTokenEventWriter();
  }
}
