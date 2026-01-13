import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NavigationService } from '../../../utility-services/navigation.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ContactService, ContactData, ContactSubmitData } from '../../../scheduler-data-services/contact.service';
import { IconService } from '../../../scheduler-data-services/icon.service';
import { ContactTagData } from '../../../scheduler-data-services/contact-tag.service';
import { AuthService } from '../../../services/auth.service';
import { Observable, BehaviorSubject, Subject, takeUntil, finalize, switchMap, forkJoin, shareReplay, map } from 'rxjs';
import { ContactCustomAddEditComponent } from '../contact-custom-add-edit/contact-custom-add-edit.component';
import { TagService } from '../../../scheduler-data-services/tag.service';
import { ConstituentData, ConstituentService } from '../../../scheduler-data-services/constituent.service';
import { ConstituentJourneyStageService } from '../../../scheduler-data-services/constituent-journey-stage.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ConstituentJourneyUpdateModalComponent } from '../constituent-journey-update-modal/constituent-journey-update-modal.component';

@Component({
  selector: 'app-contact-custom-detail',
  templateUrl: './contact-custom-detail.component.html',
  styleUrls: ['./contact-custom-detail.component.scss']
})

export class ContactCustomDetailComponent implements OnInit {

  @ViewChild(ContactCustomAddEditComponent) addEditComponent!: ContactCustomAddEditComponent;


  public contactId: string | null = null;
  public contact: ContactData | null = null;

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  public error: string | null = null;
  public activeTab = 'overview';

  public isSaving = false;

  public isEditMode = true;   // Defaults to true (edit).  Gets set to false in ngOnInit if route is 'new'

  public contactTagsWithIcons$!: Observable<Array<ContactTagData> | null>;
  public constituent$: Observable<ConstituentData | null> = new BehaviorSubject<ConstituentData | null>(null);

  private destroy$ = new Subject<void>();

  constructor(
    public contactService: ContactService,
    public iconService: IconService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private alertService: AlertService,
    private modalService: NgbModal,
    private navigationService: NavigationService) {

  }

  ngOnInit(): void {

    // Get the contactId from the route parameters
    this.contactId = this.route.snapshot.paramMap.get('contactId');

    // Handle tab state from query params
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.activeTab = params['tab'];
      }
    });

    if (this.contactId === 'new' ||
      this.contactId == null) {
      //
      // Add mode
      //
      this.isEditMode = false;
      this.contact = null;

      this.isLoadingSubject.next(false); // No load needed for add mode

      document.title = 'Add New Contact';

    } else {

      // Edit mode
      this.isEditMode = true;

      document.title = 'Edit Contact';

      // Load the data from the server
      this.loadData(false);
    }
  }


  ngAfterViewInit(): void {

    //
    // Open the add/edit modal in add mode after the view is initialized so we have the reference properly
    //
    if (this.isEditMode == false) {
      this.addEditComponent.openModal();      // Open add modal
    }

    //
    // Subscribe to the observable on the add/edit component so that when there are changes we can reload the list.
    //
    this.addEditComponent.contactChanged.subscribe({
      next: (result: ContactData[] | null) => {
        this.loadData();

      },
      error: (err: any) => {
        this.alertService.showMessage("Error during Contact changed notification", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public onTabChange(event: any) {
    this.activeTab = event.nextId;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: this.activeTab },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  public GetQueryParameters(): any {

    if (this.contactId != null && this.contactId !== 'new') {

      const id = parseInt(this.contactId, 10);

      if (!isNaN(id)) {
        return { contactId: id };
      }
    }

    return null;
  }


  /*
    * Loads the Contact data for the current contactId.
    *
    * Fully respects the ContactService caching strategy and error handling strategy.
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
    if (!this.contactService.userIsSchedulerContactReader()) {

      const userName = this.authService.currentUser?.userName || 'Current user';
      this.alertService.showMessage(`${userName} does not have permission to read Contacts.`,
        'Access Denied',
        MessageSeverity.warn
      );

      this.isLoadingSubject.next(false);

      return;
    }

    //
    // Validate contactId
    //
    if (!this.contactId) {

      this.alertService.showMessage('No Contact ID provided.', 'Missing ID', MessageSeverity.error);
      this.isLoadingSubject.next(false);

      return;
    }

    const contactId = Number(this.contactId);

    if (isNaN(contactId) || contactId <= 0) {

      this.alertService.showMessage(`Invalid Contact ID: "${this.contactId}"`,
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
      // This is the most targeted way: clear only this Contact + relations

      this.contactService.ClearRecordCache(contactId, true);
    }

    //
    // Subscribe with full next/error handling
    //
    this.contactService.GetContact(contactId, true).pipe(
      takeUntil(this.destroy$)
    ).subscribe({

      next: (contactData) => {

        //
        // Success path — contactData can legitimately be null if 404'd but request succeeded
        //
        if (!contactData) {

          this.handleContactNotFound(contactId);

        } else {

          this.contact = contactData;


          // Load and expand tags so header badges show icons/colors
          this.loadAndExpandContactTags();

          // Load Primary Constituent for Donor Journey
          this.constituent$ = this.contact.Constituents$.pipe(
            map(list => (list && list.length > 0) ? list[0] : null),
            switchMap(async constituent => {
              if (constituent && constituent.constituentJourneyStageId) {
                // Revive and reload to get full stage details (color, name, icon) if not already loaded
                if (!constituent.constituentJourneyStage) {
                  // We can use the service to get the stage if we don't want to reload the whole constituent
                  // But reloading the constituent is safer to ensure all relations are ok.
                  // Actually, let's just use the ID to fetch the stage if missing, or reload the constituent.
                  // Re-loading the constituent with includeRelations=true is best.
                  const revivified = ConstituentService.Instance.ReviveConstituent(constituent);
                  await revivified.Reload(true);
                  return revivified;
                }
                return constituent;
              }
              return constituent;
            }),
            shareReplay(1)
          );

          if (forceLoadAndDisplaySuccessAlert === true) {
            this.alertService.showMessage(
              'Contact loaded successfully',
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
        this.handleContactLoadError(error, contactId);
        this.isLoadingSubject.next(false);
      }
    });
  }

  //
  // Use the contact tag promise to start the load, then iterate over each one.
  //
  // For each, revive its tag property so that it can be reloaded, then do that.
  //
  // Once reloaded, it'll have the icon property we want loaded.
  //
  private async loadAndExpandContactTags(): Promise<void> {

    if (!this.contact) {
      return;
    }

    try {
      //
      // This triggers the load if not already started
      //
      const contactTags = await this.contact.ContactTags;

      if (contactTags && contactTags.length > 0) {

        //
        // Process all tags in parallel
        //
        await Promise.all(
          contactTags.map(async (ct) => {
            if (ct.tag) {

              //
              // Ensure it's a full object with prototype methods by reviving it.
              //
              ct.tag = TagService.Instance.ReviveTag(ct.tag);

              //
              // Reload to populate icon, color, description
              //
              await ct.tag.Reload(true);
            }
          })
        );
      }
    } catch (err) {
      // Non-fatal — UI will just show name-only badges
      console.warn('Failed to load or expand contact tags', err);
    }
  }


  private handleContactNotFound(contactId: number): void {

    this.contact = null;

    this.alertService.showMessage(
      `Contact #${contactId} was not found or has been deleted.`,
      'Not Found',
      MessageSeverity.warn
    );
  }


  private handleContactLoadError(error: any, contactId: number): void {

    let message = 'Failed to load Contact.';
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
          message = 'You do not have permission to view this Contact.';
          title = 'Forbidden';
          break;
        case 404:
          message = `Contact #${contactId} was not found.`;
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

    console.error(`Contact load failed (ID: ${contactId})`, error);

    //
    // Reset UI to safe state
    //
    this.contact = null;

    this.alertService.showMessage(message, title, severity);
  }


  public goBack(): void {
    this.navigationService.goBack();
  }


  public canGoBack(): boolean {
    return this.navigationService.canGoBack();
  }


  public contactChanged(contactData: ContactData[]) {

    contactData[0].Reload().then(c => {
      this.contact = c;
    });
  }


  public openEditModal(): void {
    if (this.contact) {
      this.addEditComponent.openModal(this.contact);
    }
  }


  public userIsSchedulerContactReader(): boolean {
    return this.contactService.userIsSchedulerContactReader();
  }


  public userIsSchedulerContactWriter(): boolean {
    return this.contactService.userIsSchedulerContactWriter();
  }

  public openJourneyUpdateModal(constituent: ConstituentData): void {
    const modalRef = this.modalService.open(ConstituentJourneyUpdateModalComponent, { size: 'lg', backdrop: 'static' });
    modalRef.componentInstance.constituent = constituent;

    modalRef.result.then((updatedConstituent: ConstituentData) => {
      if (updatedConstituent) {
        // Reload the contact to refresh the view (and thus the constituent$)
        this.loadData();
        // Or force reload constituent$ specifically if we want to be more granular, but loadData works.
      }
    }, () => {
      // Dismissed
    });
  }
}
