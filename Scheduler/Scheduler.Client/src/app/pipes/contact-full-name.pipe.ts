import { Pipe, PipeTransform } from '@angular/core';
import { ContactData } from '../scheduler-data-services/contact.service'; // Adjust path as needed

/**
 * FullNamePipe
 *
 * Purpose:
 * Formats a ContactData object into a readable full name string:
 *   "First Middle Last"
 *
 * Rules:
 * - Always includes firstName if present
 * - Includes middleName only if it exists and is non-empty
 * - Includes lastName only if it exists and is non-empty
 * - Trims excess whitespace between parts
 * - Returns null if the input ContactData is null
 * - Returns empty string if no name parts are present (though this shouldn't happen with valid data)
 *
 * Usage in templates:
 *   {{ contact | fullName }}
 *   {{ someContact | fullName || 'Unknown Contact' }}
 *
 * Why a pipe?
 * - Keeps templates clean and declarative
 * - Reusable across all contact displays (headers, lists, tooltips, etc.)
 * - Pure pipe → performant (only recalculates when input reference changes)
 */
@Pipe({
  name: 'contactFullName',
  pure: true // Default and recommended — recalculates only on input change
})
export class ContactFullNamePipe implements PipeTransform {

  /**
   * Transforms a ContactData object into a formatted full name.
   *
   * @param contactData The contact to format — can be null
   * @returns Formatted name string or null if input is null
   */
  transform(contactData: ContactData | null): string | null {
    //
    // Guard clause: null input → null output
    // This allows safe usage like *ngIf="contact | fullName"
    //
    if (contactData == null) {
      return null;
    }

    // Start with first name (required field, but defensive)
    let fullName: string = contactData.firstName?.trim() || '';

    // Add middle name if present and non-empty
    if (contactData.middleName != null && contactData.middleName.trim() !== '') {
      fullName = (fullName + ' ' + contactData.middleName.trim()).trim();
    }

    // Add last name if present and non-empty
    if (contactData.lastName != null && contactData.lastName.trim() !== '') {
      fullName = (fullName + ' ' + contactData.lastName.trim()).trim();
    }

    // Return the built name
    // If no parts were present, this will be empty string (rare but safe)
    return fullName;
  }
}
