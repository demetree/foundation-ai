import { Component, Input } from '@angular/core';
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

  constructor(private authService: AuthService,
    private contactService: ContactService,
    private contactInteractionService: ContactInteractionService) { }

  // Reuse permission checks from parent or service
  userIsSchedulerContactWriter(): boolean {

    return this.contactService.userIsSchedulerContactWriter();
  }

  canLogInteraction(): boolean {
    return this.contactInteractionService.userIsSchedulerContactInteractionWriter();
  }

  // Placeholder action methods
  openEditModal(): void { /* Emit or open modal */ }
  openLogInteractionModal(): void { /* Open interaction modal */ }
  sendEmail(): void { window.location.href = 'mailto:' + this.contact.email; }
  callPhone(): void {
    const phone = this.contact.mobile || this.contact.phone;
    if (phone) window.location.href = 'tel:' + phone;
  }
  addRelationship(): void { /* Open relationship modal */ }
  manageTags(): void { /* Open tag management */ }


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
