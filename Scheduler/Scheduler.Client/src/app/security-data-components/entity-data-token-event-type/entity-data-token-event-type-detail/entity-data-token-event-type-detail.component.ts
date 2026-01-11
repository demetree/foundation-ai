import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { CanComponentDeactivate } from '../../../guards/unsaved-changes.guard';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { EntityDataTokenEventTypeService, EntityDataTokenEventTypeData, EntityDataTokenEventTypeSubmitData } from '../../../security-data-services/entity-data-token-event-type.service';
import { EntityDataTokenEventService } from '../../../security-data-services/entity-data-token-event.service';
import { AuthService } from '../../../services/auth.service';
import { BehaviorSubject, Subject, takeUntil, finalize } from 'rxjs';
import { isoUtcStringToDateTimeLocal, dateTimeLocalToIsoUtc } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-entity-data-token-event-type-detail',
  templateUrl: './entity-data-token-event-type-detail.component.html',
  styleUrls: ['./entity-data-token-event-type-detail.component.scss']
})

export class EntityDataTokenEventTypeDetailComponent implements OnInit, CanComponentDeactivate {

  entityDataTokenEventTypeForm: FormGroup = this.fb.group({
        name: ['', Validators.required],
        description: [''],
      });


  public entityDataTokenEventTypeId: string | null = null;
  public entityDataTokenEventTypeData: EntityDataTokenEventTypeData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  entityDataTokenEventTypes$ = this.entityDataTokenEventTypeService.GetEntityDataTokenEventTypeList();
  entityDataTokenEvents$ = this.entityDataTokenEventService.GetEntityDataTokenEventList();

  private destroy$ = new Subject<void>();

  constructor(
    public entityDataTokenEventTypeService: EntityDataTokenEventTypeService,
    public entityDataTokenEventService: EntityDataTokenEventService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private alertService: AlertService,
    private navigationService: NavigationService) { 

    }

  ngOnInit(): void {

    // Get the entityDataTokenEventTypeId from the route parameters
    this.entityDataTokenEventTypeId = this.route.snapshot.paramMap.get('entityDataTokenEventTypeId');

    if (this.entityDataTokenEventTypeId === 'new' ||
        this.entityDataTokenEventTypeId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.entityDataTokenEventTypeData = null;

      this.buildFormValues(null);

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Entity Data Token Event Type';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Entity Data Token Event Type';

      // Load the data from the server
      this.loadData(false);
    }

  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public canDeactivate(): boolean {
    if (this.entityDataTokenEventTypeForm.dirty) {
      return confirm('You have unsaved Entity Data Token Event Type changes. Are you sure you want to leave this page?');
    }
    return true;
  }


 public GetQueryParameters(): any {

    if (this.entityDataTokenEventTypeId != null && this.entityDataTokenEventTypeId !== 'new') {

      const id = parseInt(this.entityDataTokenEventTypeId, 10);

      if (!isNaN(id)) {
        return { entityDataTokenEventTypeId: id };
      }
    }

    return null;
  }


/*
  * Loads the EntityDataTokenEventType data for the current entityDataTokenEventTypeId.
  *
  * Fully respects the EntityDataTokenEventTypeService caching strategy and error handling strategy.
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
    if (!this.entityDataTokenEventTypeService.userIsSecurityEntityDataTokenEventTypeReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read EntityDataTokenEventTypes.`,
                                    'Access Denied',
                                     MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate entityDataTokenEventTypeId
    //
    if (!this.entityDataTokenEventTypeId) {

      this.alertService.showMessage('No EntityDataTokenEventType ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const entityDataTokenEventTypeId = Number(this.entityDataTokenEventTypeId);

    if (isNaN(entityDataTokenEventTypeId) || entityDataTokenEventTypeId <= 0) {

      this.alertService.showMessage(`Invalid Entity Data Token Event Type ID: "${this.entityDataTokenEventTypeId}"`,
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
      // This is the most targeted way: clear only this EntityDataTokenEventType + relations

      this.entityDataTokenEventTypeService.ClearRecordCache(entityDataTokenEventTypeId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.entityDataTokenEventTypeService.GetEntityDataTokenEventType(entityDataTokenEventTypeId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (entityDataTokenEventTypeData) => {

        //
        // Success path — entityDataTokenEventTypeData can legitimately be null if 404'd but request succeeded
        //
        if (!entityDataTokenEventTypeData) {

          this.handleEntityDataTokenEventTypeNotFound(entityDataTokenEventTypeId);

        } else {

          this.entityDataTokenEventTypeData = entityDataTokenEventTypeData;
          this.buildFormValues(this.entityDataTokenEventTypeData);

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'EntityDataTokenEventType loaded successfully',
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
        this.handleEntityDataTokenEventTypeLoadError(error, entityDataTokenEventTypeId);
        this.isLoadingSubject.next(false);
      }
    });
  }


  private handleEntityDataTokenEventTypeNotFound(entityDataTokenEventTypeId: number): void {

    this.entityDataTokenEventTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(
      `EntityDataTokenEventType #${entityDataTokenEventTypeId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleEntityDataTokenEventTypeLoadError(error: any, entityDataTokenEventTypeId: number): void {

    let message = 'Failed to load Entity Data Token Event Type.';
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
          message = 'You do not have permission to view this Entity Data Token Event Type.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Entity Data Token Event Type #${entityDataTokenEventTypeId} was not found.`;
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

    console.error(`Entity Data Token Event Type load failed (ID: ${entityDataTokenEventTypeId})`, error);

    //
    // Reset UI to safe state
    //
    this.entityDataTokenEventTypeData = null;
    this.buildFormValues(null);

    this.alertService.showMessage(message, title, severity);
  }


  private buildFormValues(entityDataTokenEventTypeData: EntityDataTokenEventTypeData | null) {

    if (entityDataTokenEventTypeData == null) {
      
      //
      // Reset the form group to null state, but don't change the form instance.
      //
      this.entityDataTokenEventTypeForm.reset({
        name: '',
        description: '',
   }, { emitEvent: false});

    }
    else {

        //
        // Reset the form with properly formatted values that support dates in datetime-local inputs
        //
        this.entityDataTokenEventTypeForm.reset({
        name: entityDataTokenEventTypeData.name ?? '',
        description: entityDataTokenEventTypeData.description ?? '',
      }, { emitEvent: false});
    }

    this.entityDataTokenEventTypeForm.markAsPristine();
    this.entityDataTokenEventTypeForm.markAsUntouched();
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

    if (this.entityDataTokenEventTypeService.userIsSecurityEntityDataTokenEventTypeWriter() == false) {
      this.alertService.showMessage(this.authService.currentUser?.userName + " does not have the permission to write to Entity Data Token Event Types", 'Access Denied', MessageSeverity.info);
      return;
    }

    if (!this.entityDataTokenEventTypeForm.valid) {
      this.alertService.showMessage('Please fix form errors before saving.', 'Invalid Data', MessageSeverity.warn);
      this.entityDataTokenEventTypeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;

    const formValue = this.entityDataTokenEventTypeForm.getRawValue();



    //
    // Build clean submit object from form + fallback to current data if needed
    //
    const entityDataTokenEventTypeSubmitData: EntityDataTokenEventTypeSubmitData = {
        id: this.entityDataTokenEventTypeData?.id || 0,
        name: formValue.name!.trim(),
        description: formValue.description?.trim() || null,
   };


    //
    // Choose the save method we want
    //
    const saveObservable = this.isEditMode
      ? this.entityDataTokenEventTypeService.PutEntityDataTokenEventType(entityDataTokenEventTypeSubmitData.id, entityDataTokenEventTypeSubmitData)
      : this.entityDataTokenEventTypeService.PostEntityDataTokenEventType(entityDataTokenEventTypeSubmitData);


    saveObservable.pipe(
      finalize(() => this.isSaving = false)
    ).subscribe({
      next: (savedEntityDataTokenEventTypeData) => {

        this.entityDataTokenEventTypeService.ClearAllCaches();       // Clear the data service cache because we know we have changed the data.

        if (!this.isEditMode) {
          //
          // Navigate to the newly created Entity Data Token Event Type's detail page
          //
          this.entityDataTokenEventTypeForm.markAsPristine();     // Set the form to new state so the deactivate guard won't complain during routing
          this.entityDataTokenEventTypeForm.markAsUntouched();

          this.router.navigate(['/entitydatatokeneventtypes', savedEntityDataTokenEventTypeData.id]);
          this.alertService.showMessage('Entity Data Token Event Type added successfully', '', MessageSeverity.success);
        } else {

          //
          // Rebuild the form with the new data
          //
          this.entityDataTokenEventTypeData = savedEntityDataTokenEventTypeData;
          this.buildFormValues(this.entityDataTokenEventTypeData);

          this.alertService.showMessage("Entity Data Token Event Type saved successfully", '', MessageSeverity.success);
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
                                   'You do not have permission to save this Entity Data Token Event Type.';
                }
                else
                {
                    errorMessage = err.error?.message ||
                                   err.error?.error_description ||
                                   err.error?.detail ||
                                   'An error occurred while saving the Entity Data Token Event Type.';
                }
            }
            // Fallback for unexpected error formats
            else {
                errorMessage = 'An unexpected error occurred.';
            }

            this.alertService.showMessage('Entity Data Token Event Type could not be saved',
                                          errorMessage,
                                          MessageSeverity.error);
      }
    });
  }

  public userIsSecurityEntityDataTokenEventTypeReader(): boolean {
    return this.entityDataTokenEventTypeService.userIsSecurityEntityDataTokenEventTypeReader();
  }

  public userIsSecurityEntityDataTokenEventTypeWriter(): boolean {
    return this.entityDataTokenEventTypeService.userIsSecurityEntityDataTokenEventTypeWriter();
  }
}
