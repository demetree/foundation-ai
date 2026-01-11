import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Subject, Observable, of } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';

import { ContactData } from '../../../scheduler-data-services/contact.service';
import { ContactInteractionService, ContactInteractionData } from '../../../scheduler-data-services/contact-interaction.service';
import { AuthService } from '../../../services/auth.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ContactInteractionEditModalComponent } from '../contact-interaction-edit-modal/contact-interaction-edit-modal.component';

@Component({
  selector: 'app-contact-interactions-tab',
  templateUrl: './contact-interactions-tab.component.html',
  styleUrls: ['./contact-interactions-tab.component.scss']
})
export class ContactInteractionsTabComponent implements OnInit, OnDestroy {
  /**
   * The parent contact whose interactions we are displaying.
   * This is passed down from the contact detail page.
   */
  @Input() contact!: ContactData;

  /**
   * Observable stream of interactions for this contact.
   * 
   * We expose this directly to the template via | async.
   * The underlying ContactData lazy-loading pattern ensures that the first subscription
   * triggers the server load (via ContactInteractions$).
   */
  public interactions$: Observable<ContactInteractionData[] | null>;

  /**
   * Simple loading indicator derived from the interactions stream.
   * null = still loading (initial state from BehaviorSubject)
   */
  public isLoading$: Observable<boolean>;

  /**
   * Subject used to cleanly unsubscribe from all streams when the component is destroyed.
   * Traditional pattern to prevent memory leaks.
   */
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private contactInteractionServic: ContactInteractionService,
    private modalService: NgbModal
    // If you have a dedicated modal, inject any required services here
  ) {
    // Default initialization — will be overridden in ngOnInit
    this.interactions$ = of(null);
    this.isLoading$ = of(true);
  }

  /**
   * Angular lifecycle hook — called once after @Input properties are set.
   */
  ngOnInit(): void {

    if (!this.contact) {

      this.interactions$ = of([]);
      this.isLoading$ = of(false);
      return;
    }

    //
    // Subscribe to the lazy-loaded observable from ContactData
    //
    // This automatically triggers the load if not already cached
    //
    this.interactions$ = this.contact.ContactInteractions$.pipe(
      map(interactions => {
        if (!interactions) return null;
        return [...interactions].sort((a, b) =>
          new Date(b.startTime).getTime() - new Date(a.startTime).getTime()
        );
      })
    );

    //
    // Derive loading state: true while data is null (initial state), false once loaded
    //
    this.isLoading$ = this.interactions$.pipe(
      map(interactions => interactions === null),  // null = still loading
      takeUntil(this.destroy$)
    );

  }

  /**
   * Angular lifecycle hook — cleanup to prevent memory leaks.
   */
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Checks if the current user has permission to create or edit contact interactions.
   */
  public userCanLogInteraction(): boolean {

    return this.contactInteractionServic.userIsSchedulerContactInteractionWriter();
  }


  public openLogInteractionModal(interactionToEdit?: ContactInteractionData): void {
    const modalRef = this.modalService.open(ContactInteractionEditModalComponent, {
      size: 'lg',
      backdrop: 'static'
    });

    modalRef.componentInstance.contact = this.contact;
    if (interactionToEdit) {
      modalRef.componentInstance.interaction = interactionToEdit;
    }

    modalRef.result.then(
      (savedInteraction) => {
        // Optional: force refresh interactions or rely on cache update
        this.contact.ClearContactInteractionsCache(); // if you want fresh data
      },
      () => {
        // Dismissed
      }
    );
  }
}
