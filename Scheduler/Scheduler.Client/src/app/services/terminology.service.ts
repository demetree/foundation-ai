import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TerminologyService {
  // For this remediation, we default to 'generic' terminology (Staff/Team), 
  // but this can be dynamically set by the tenant profile during app initialization.
  private currentIndustry: string = 'generic'; 

  constructor() {}

  /**
   * Sets the active industry for terminology mapping.
   * @param industry The industry identifier (e.g., 'healthcare', 'construction')
   */
  public setIndustry(industry: string): void {
    if (industry) {
      this.currentIndustry = industry.toLowerCase();
    }
  }

  /**
   * Gets the industry-specific term for a given system string.
   * @param baseTerm The system term (e.g., 'Resource', 'Crew')
   * @param plural Whether to return the plural form
   * @returns The localized/industry-specific term
   */
  public getTerm(baseTerm: string, plural: boolean = false): string {
    const termKey = baseTerm.toLowerCase();
    
    const dictionary: Record<string, Record<string, { singular: string, plural: string }>> = {
      'generic': {
        'resource': { singular: 'Staff', plural: 'Staff' },
        'crew': { singular: 'Crew', plural: 'Crews' }
      },
      'healthcare': {
        'resource': { singular: 'Clinical Staff', plural: 'Clinical Staff' },
        'crew': { singular: 'Care Team', plural: 'Care Teams' }
      },
      'construction': {
        'resource': { singular: 'Technician', plural: 'Technicians' },
        'crew': { singular: 'Crew', plural: 'Crews' }
      },
      'salon': {
        'resource': { singular: 'Stylist', plural: 'Stylists' },
        'crew': { singular: 'Team', plural: 'Teams' }
      }
    };

    const industryDict = dictionary[this.currentIndustry] || dictionary['generic'];
    const termMap = industryDict[termKey] || dictionary['generic'][termKey];

    if (!termMap) {
        // Fallback if not defined in dictionary
        return plural ? baseTerm + 's' : baseTerm;
    }

    return plural ? termMap.plural : termMap.singular;
  }
}
