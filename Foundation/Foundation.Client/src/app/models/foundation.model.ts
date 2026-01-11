//
// This interface is be used to define a column that is used on a Foundation table component.  Each table component
// predefines their default set of these, but users of the components can override that set any way they want to change
// things like content, width, position, etc..
//
export interface TableColumn {

  // Property path in the object, e.g. 'startTime' or 'auditUser.name'.  Supports nested objects
  key: string;

  // Display heeader
  label: string;

  // Optional fixed width (px) or percentage width.  Using undefined makes it auto size.
  width?: string;

  // Cell template data type 
  template?: 'boolean' | 'date' | 'link' | 'default';

  // Link target for link template types.  First element is the route, starting with a slash, the second element is the name of the property that contains the index to the route.  eg ['/auditevent', 'auditEventId']
  linkPath?: string[];

  // Show on mobile cards (default is shown if no value.  'prominent' to make it show in the mobile card header, 'hidden' to hide it.)
  mobile?: 'hidden' | 'prominent'
}
