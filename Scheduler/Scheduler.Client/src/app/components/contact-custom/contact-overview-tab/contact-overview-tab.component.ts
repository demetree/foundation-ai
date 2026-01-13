import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Observable, of, Subject, combineLatest } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';

import { ContactService, ContactData } from '../../../scheduler-data-services/contact.service';
import { ContactInteractionService, ContactInteractionData } from '../../../scheduler-data-services/contact-interaction.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-contact-overview-tab',
  templateUrl: './contact-overview-tab.component.html',
  styleUrls: ['./contact-overview-tab.component.scss']
})
export class ContactOverviewTabComponent implements OnInit, OnDestroy {
  @Input() contact!: ContactData;

  //
  // Output events to communicate actions to parent component
  //
  @Output() editContactRequested = new EventEmitter<void>();
  @Output() addRelationshipRequested = new EventEmitter<void>();
  @Output() manageTagsRequested = new EventEmitter<void>();

  //
  // Recent interactions observable (last 5)
  //
  public recentInteractions$: Observable<ContactInteractionData[]>;
  public isLoadingInteractions$: Observable<boolean>;

  //
  // Related entities counts for quick view
  //
  public entityCounts$: Observable<{
    offices: number;
    clients: number;
    schedulingTargets: number;
    resources: number;
    relationships: number;
  }>;
  public isLoadingCounts$: Observable<boolean>;

  //
  // Cleanup subject
  //
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private contactService: ContactService,
    private contactInteractionService: ContactInteractionService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.recentInteractions$ = of([]);
    this.isLoadingInteractions$ = of(true);
    this.entityCounts$ = of({ offices: 0, clients: 0, schedulingTargets: 0, resources: 0, relationships: 0 });
    this.isLoadingCounts$ = of(true);
  }


  ngOnInit(): void {

    if (!this.contact) {
      return;
    }

    //
    // Load recent interactions (last 5, sorted by date descending)
    //
    this.recentInteractions$ = this.contact.ContactInteractions$.pipe(
      map(interactions => {
        if (!interactions) return [];
        return [...interactions]
          .sort((a, b) => new Date(b.startTime).getTime() - new Date(a.startTime).getTime())
          .slice(0, 5);
      }),
      takeUntil(this.destroy$)
    );

    this.isLoadingInteractions$ = this.contact.ContactInteractions$.pipe(
      map(interactions => interactions === null),
      takeUntil(this.destroy$)
    );

    //
    // Load entity counts from lazy-loaded observables
    //
    this.entityCounts$ = combineLatest([
      this.contact.OfficeContacts$.pipe(map(list => list?.length ?? 0)),
      this.contact.ClientContacts$.pipe(map(list => list?.length ?? 0)),
      this.contact.SchedulingTargetContacts$.pipe(map(list => list?.length ?? 0)),
      this.contact.ResourceContacts$.pipe(map(list => list?.length ?? 0)),
      this.contact.ContactContacts$.pipe(map(list => list?.length ?? 0))
    ]).pipe(
      map(([offices, clients, schedulingTargets, resources, relationships]) => ({
        offices,
        clients,
        schedulingTargets,
        resources,
        relationships
      })),
      takeUntil(this.destroy$)
    );

    this.isLoadingCounts$ = this.entityCounts$.pipe(
      map(() => false) // Once combineLatest emits, we're done loading
    );
  }


  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  //
  // Permission checks
  //
  userIsSchedulerContactWriter(): boolean {
    return this.contactService.userIsSchedulerContactWriter();
  }


  canLogInteraction(): boolean {
    return this.contactInteractionService.userIsSchedulerContactInteractionWriter();
  }


  //
  // Quick action methods
  //

  /**
   * Opens edit modal by emitting event to parent
   */
  openEditModal(): void {
    this.editContactRequested.emit();
  }


  /**
   * Navigates to Interactions tab and triggers modal auto-open via query param
   */
  openLogInteractionModal(): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: 'interactions', openModal: 'add' },
      queryParamsHandling: 'merge'
    });
  }


  /**
   * Opens mailto link for sending email
   */
  sendEmail(): void {
    if (this.contact?.email) {
      window.location.href = 'mailto:' + this.contact.email;
    }
  }


  /**
   * Opens tel link for calling phone
   */
  callPhone(): void {
    const phone = this.contact?.mobile || this.contact?.phone;
    if (phone) {
      window.location.href = 'tel:' + phone;
    }
  }


  /**
   * Navigates to Relationships tab to add a relationship
   */
  addRelationship(): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: 'relationships', action: 'add' },
      queryParamsHandling: 'merge'
    });
  }


  /**
   * Emits event to parent to manage tags
   */
  manageTags(): void {
    this.manageTagsRequested.emit();
  }


  /**
   * Navigates to a specific tab
   */
  navigateToTab(tabName: string): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: tabName },
      queryParamsHandling: 'merge'
    });
  }


  /**
   * Gets an icon for the interaction type
   */
  getInteractionIcon(interaction: ContactInteractionData): string {
    const typeName = interaction.interactionType?.name?.toLowerCase() || '';
    if (typeName.includes('email')) return 'fa-envelope';
    if (typeName.includes('call') || typeName.includes('phone')) return 'fa-phone';
    if (typeName.includes('meeting')) return 'fa-users';
    if (typeName.includes('note')) return 'fa-sticky-note';
    return 'fa-comment';
  }


  /**
   * Converts a raw website string from ContactData into a fully qualified URL
   * that can be safely used in an anchor tag (<a href="...">).
   *
   * Rules applied:
   * 1. If the input is null, undefined, or empty after trimming → return null
   * 2. If the string already starts with 'http://' or 'https://' → return as-is
   * 3. Otherwise → prepend 'https://' (modern default; avoids mixed-content warnings)
   * 4. Trim whitespace to prevent issues like ' example.com '
   *
   * Why https by default?
   * - Most modern sites support HTTPS
   * - Browsers will automatically upgrade http → https when possible
   * - Avoids relative URL interpretation (e.g., 'www.example.com' being treated as '/www.example.com' on current origin)
   *
   * @param contactData The ContactData object (or null)
   * @returns A fully qualified URL string (e.g., 'https://www.example.com') or null if no valid website
   */
  public getWebSiteAsAddress(contactData: ContactData | null): string | null {
    // Guard clause: no contact or no website value
    if (
      contactData == null ||
      contactData.webSite == null ||
      contactData.webSite.trim() === ''
    ) {
      return null;
    }

    // Trim whitespace from input
    let rawWebsite: string = contactData.webSite.trim();

    // If it already has a protocol, return it unchanged
    if (/^https?:\/\//i.test(rawWebsite)) {
      return rawWebsite;
    }

    // If it starts with '//' (protocol-relative), prepend 'https:'
    if (rawWebsite.startsWith('//')) {
      return 'https:' + rawWebsite;
    }

    // Default case: no protocol → add https://
    return 'https://' + rawWebsite;
  }
}

