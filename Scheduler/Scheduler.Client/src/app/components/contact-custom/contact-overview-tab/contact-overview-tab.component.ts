import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { ContactService, ContactData } from '../../../scheduler-data-services/contact.service';
import { ContactInteractionService, ContactInteractionData } from '../../../scheduler-data-services/contact-interaction.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-contact-overview-tab',
  templateUrl: './contact-overview-tab.component.html',
  styleUrls: ['./contact-overview-tab.component.scss']
})
export class ContactOverviewTabComponent {
  @Input() contact!: ContactData;

  //
  // Output events to communicate actions to parent component
  //
  @Output() editContactRequested = new EventEmitter<void>();
  @Output() addRelationshipRequested = new EventEmitter<void>();
  @Output() manageTagsRequested = new EventEmitter<void>();

  constructor(
    private authService: AuthService,
    private contactService: ContactService,
    private contactInteractionService: ContactInteractionService,
    private router: Router,
    private route: ActivatedRoute
  ) { }


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
