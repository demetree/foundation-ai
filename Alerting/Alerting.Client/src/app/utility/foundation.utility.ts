//
// Utility functions and interfaces used by Foundation components
//
//

/**
* This interface is be used to define a column that is used on a Foundation table component.  Each table component
* predefines their default set of these, but users of the components can override that set any way they want to change
* things like content, width, position, etc..
*
*/
export interface TableColumn {

  // Property path in the object, e.g. 'startTime' or 'auditUser.name'.  Supports nested objects
  key: string;

  // Display heeader
  label: string;

  // Optional fixed width (px) or percentage width.  Using undefined makes it auto size.
  width?: string;

  // Cell template data type 
  template?: 'boolean' | 'date' | 'link' | 'color' | 'default';

  // Link target for link template types.  First element is the route, starting with a slash, the second element is the name of the property that contains the index to the route.  eg ['/auditevent', 'auditEventId']
  linkPath?: string[];

  // Show on mobile cards (default is shown if no value.  'prominent' to make it show in the mobile card header, 'hidden' to hide it.)
  mobile?: 'hidden' | 'prominent'
}



/**
* Converts an ISO UTC date-time string (e.g. "1999-06-30T00:00:00.000000Z")
* into the format required by <input type="datetime-local"> ("YYYY-MM-DDTHH:mm").
* If the input is null, undefined, or empty, returns null.
* This handles the fact that the server sends strings, not Date objects.
*/
export function isoUtcStringToDateTimeLocal(value: string | null | undefined): string | null {

  if (!value || typeof value !== 'string') {
    return null;
  }

  // The native Date parser understands ISO strings with Z correctly.
  const date = new Date(value);

  // Guard against invalid dates
  if (isNaN(date.getTime())) {
    return null;
  }

  // Build the exact format the browser input expects.
  // We intentionally use local time components because datetime-local is a local control.
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');

  return `${year}-${month}-${day}T${hours}:${minutes}`;
}


/**
 * Covert the datetime-local string back to a UTC ISO string that the server expects.
 */
export function dateTimeLocalToIsoUtc(value: string | null | undefined): string | null {

  if (!value) {
    return null;
  }

  const date = new Date(value);

  if (isNaN(date.getTime())) {
    return null;
  }

  //
  // toISOString() always returns UTC with Z and milliseconds
  //
  return date.toISOString();
}
