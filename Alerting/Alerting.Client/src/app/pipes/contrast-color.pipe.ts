import { Pipe, PipeTransform } from '@angular/core';

/**
 * ContrastColorPipe
 *
 * Purpose:
 * Determines whether dark (#000000) or light (#ffffff) text should be used
 * on a given background hex color for optimal readability.
 *
 * Algorithm:
 * Calculates relative luminance using the standard formula:
 *   luminance = (0.299 * R + 0.587 * G + 0.114 * B) / 255
 * If luminance > 0.5 → use black text
 * Otherwise → use white text
 *
 * Handles edge cases:
 * - null/undefined/empty input → returns white (#ffffff) as safe default
 * - Accepts colors with or without leading '#'
 * - Assumes 6-character hex (e.g., #FF0000 or FF0000)
 *
 * Usage in templates:
 *   <span [style.background-color]="bgColor"
 *         [style.color]="bgColor | contrastColor">
 *     Text
 *   </span>
 *
 * Why a pipe?
 * - Declarative: keeps template logic clean
 * - Reusable across all components displaying colored badges/elements
 * - Pure pipe → excellent performance (recalculates only on input change)
 */
@Pipe({
  name: 'contrastColor',
  pure: true  // Default, but explicitly stated for clarity
})
export class ContrastColorPipe implements PipeTransform {

  /**
   * Transforms a hex background color into the appropriate contrasting text color.
   *
   * @param hexColor The background color as a hex string (with or without #)
   *                 Can be null, undefined, or empty.
   * @returns '#000000' (black) for light backgrounds or '#ffffff' (white) for dark backgrounds
   */
  transform(hexColor: string | null | undefined): string {
    // Guard clause: no valid color provided → default to white text (safe on dark backgrounds)
    if (!hexColor || hexColor.trim() === '') {
      return '#ffffff';
    }

    // Normalize: remove leading # if present
    let color = hexColor.trim();
    if (color.charAt(0) === '#') {
      color = color.substring(1);
    }

    // Defensive: ensure we have at least 6 characters (basic hex validation)
    if (color.length < 6) {
      return '#ffffff'; // fallback to white on invalid input
    }

    // Extract RGB components (first 6 characters)
    const r = parseInt(color.substr(0, 2), 16);
    const g = parseInt(color.substr(2, 2), 16);
    const b = parseInt(color.substr(4, 2), 16);

    // Calculate relative luminance using WCAG formula weights
    const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;

    // Return black text on light backgrounds, white on dark
    return luminance > 0.5 ? '#000000' : '#ffffff';
  }
}
