import { Injectable } from '@angular/core';
import { HttpResponseBase } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';

import { Utilities } from './utilities';

// Define an interface for Problem Details (RFC 9110)
interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: { [key: string]: string[] } | string; // For validation errors or string-based errors
  [key: string]: any; // Allow additional properties
}


@Injectable()
export class AlertService {
  private messages = new Subject<AlertCommand>();
  private dialogs = new Subject<AlertDialog>();

  private loadingMessageTimeoutId: ReturnType<typeof setTimeout> | undefined;

  showDialog(message: string): void;
  showDialog(message: string, type: DialogType, okCallback: (val?: string) => void): void;
  showDialog(message: string, type: DialogType, okCallback?: { (val?: string): void } | null, cancelCallback?: { (): void } | null, okLabel?: string | null, cancelLabel?: string | null, defaultValue?: string | null): void;
  showDialog(message: string, type?: DialogType, okCallback?: (val?: string) => void, cancelCallback?: () => void, okLabel?: string, cancelLabel?: string, defaultValue?: string) {
    if (!type) {
      type = DialogType.alert;
    }

    this.dialogs.next({ message, type, okCallback, cancelCallback, okLabel, cancelLabel, defaultValue });
  }

  showMessage(summary: string): void;
  showMessage(summary: string, detail: string | null, severity: MessageSeverity): void;
  showMessage(summaryAndDetails: string[], summaryAndDetailsSeparator: string, severity: MessageSeverity): void;
  showMessage(response: HttpResponseBase, ignoreValueUseNull: string, severity: MessageSeverity): void;
  showMessage(data: string | string[] | HttpResponseBase, separatorOrDetail?: string | null, severity?: MessageSeverity) {
    if (!severity) {
      severity = MessageSeverity.default;
    }

    if (data instanceof HttpResponseBase) {
      data = Utilities.getHttpResponseMessages(data);
      separatorOrDetail = Utilities.captionAndMessageSeparator;
    }

    if (Array.isArray(data)) {
      for (const message of data) {
        const msgObject = Utilities.splitInTwo(message, separatorOrDetail ?? '');

        this.showMessageHelper(msgObject.firstPart, msgObject.secondPart, severity, false);
      }
    } else {
      this.showMessageHelper(data, separatorOrDetail, severity, false);
    }
  }


  //
  // Use this for standard success alerting
  //
  showSuccessMessage(summary: string, detail: string | null) {

    this.showMessage(summary, detail, MessageSeverity.success);
  }


  //
  // Use this for non-http error alerting
  //
  showErrorMessage(summary: string, detail: string | null) {

    this.showMessage(summary, detail, MessageSeverity.error);
  }


  //
  // This is a wrapper to easily handle the error messages received by HTTP service calls that fail, for either RFC 9110 message sent by ASP.Net.
  //
  // Problem() results general an RFC 9110 object
  // While more general BadRequest() or NotFound() message with string data.
  //
  // This handles both.
  //
  showHttpErrorMessage(summary: string, error: any) {
    let detail: string | null = null;

    // Check if error.error exists and is an object (Problem() or structured BadRequest)
    if (error?.error) {
      if (typeof error.error === 'string') {
        // Handle string-based BadRequest response
        detail = error.error;
      } else if (typeof error.error === 'object') {
        const problemDetails = error.error as ProblemDetails;

        // Prioritize 'detail' for Problem() responses
        if (problemDetails.detail) {
          detail = problemDetails.detail;
        }
        // Fallback to 'title' if 'detail' is not available
        else if (problemDetails.title) {
          detail = problemDetails.title;
        }
        // Handle validation errors (e.g., ModelState errors in ASP.NET)
        else if (problemDetails.errors) {
          if (typeof problemDetails.errors === 'string') {
            detail = problemDetails.errors;
          } else {
            // Convert validation errors to a readable string
            detail = Object.values(problemDetails.errors)
              .flat()
              .join('; ');
          }
        }
        // Fallback to JSON stringification for unrecognized objects
        else {
          detail = JSON.stringify(problemDetails);
        }
      }
    }

    // Fallback if no specific error details are available
    if (!detail) {
      detail = error.message || 'An unexpected error occurred.';
    }

    // Display the error using the existing showMessage method
    this.showMessage(summary, detail, MessageSeverity.error);
  }


  showStickyMessage(summary: string): void;
  showStickyMessage(summary: string, detail: string | null, severity: MessageSeverity, error?: unknown): void;
  showStickyMessage(summary: string, detail: string | null, severity: MessageSeverity, error?: unknown, onRemove?: () => void): void;
  showStickyMessage(summaryAndDetails: string[], summaryAndDetailsSeparator: string, severity: MessageSeverity): void;
  showStickyMessage(response: HttpResponseBase, ignoreValueUseNull: null, severity: MessageSeverity): void;
  showStickyMessage(data: string | string[] | HttpResponseBase, separatorOrDetail?: string | null, severity?: MessageSeverity, error?: unknown, onRemove?: () => void) {

    if (!severity) {
      severity = MessageSeverity.default;
    }

    if (data instanceof HttpResponseBase) {
      data = Utilities.getHttpResponseMessages(data);
      separatorOrDetail = Utilities.captionAndMessageSeparator;
    }

    if (Array.isArray(data)) {
      for (const message of data) {
        const msgObject = Utilities.splitInTwo(message, separatorOrDetail ?? '');

        this.showMessageHelper(msgObject.firstPart, msgObject.secondPart, severity, true);
      }
    } else {
      if (error) {
        const msg = `Severity: "${MessageSeverity[severity]}", Summary: "${data}", Detail: "${separatorOrDetail}", Error: "${Utilities.stringify(error)}"`;

        switch (severity) {
          case MessageSeverity.default:
            this.logInfo(msg);
            break;
          case MessageSeverity.info:
            this.logInfo(msg);
            break;
          case MessageSeverity.success:
            this.logMessage(msg);
            break;
          case MessageSeverity.error:
            this.logError(msg);
            break;
          case MessageSeverity.warn:
            this.logWarning(msg);
            break;
          case MessageSeverity.wait:
            this.logTrace(msg);
            break;
        }
      }

      this.showMessageHelper(data, separatorOrDetail, severity, true, onRemove);
    }
  }

  private showMessageHelper(summary: string, detail: string | null | undefined, severity: MessageSeverity, isSticky: boolean, onRemove?: () => void) {
    if (detail === null)
      detail = undefined;

    const alertCommand: AlertCommand = {
      operation: isSticky ? 'add_sticky' : 'add',
      message: { severity, summary, detail },
      onRemove
    };

    this.messages.next(alertCommand);
  }

  resetStickyMessage() {
    this.messages.next({ operation: 'clear' });
  }

  startLoadingMessage(message = 'Loading...', caption = '') {
    clearTimeout(this.loadingMessageTimeoutId);

    if (!caption) {
      caption = message;
      message = '';
    }

    this.loadingMessageTimeoutId = setTimeout(() => {
      this.showStickyMessage(caption, message, MessageSeverity.wait);
    }, 1000);
  }

  stopLoadingMessage() {
    clearTimeout(this.loadingMessageTimeoutId);
    this.resetStickyMessage();
  }


  logDebug(msg: unknown) {
    console.debug(msg);
  }

  logError(msg: unknown) {
    console.error(msg);
  }

  logInfo(msg: unknown) {
    console.info(msg);
  }

  logMessage(msg: unknown) {
    console.log(msg);
  }

  logTrace(msg: unknown) {
    console.trace(msg);
  }

  logWarning(msg: unknown) {
    console.warn(msg);
  }

  getDialogEvent(): Observable<AlertDialog> {
    return this.dialogs.asObservable();
  }

  getMessageEvent(): Observable<AlertCommand> {
    return this.messages.asObservable();
  }
}


// ******************** Dialog ********************//
export class AlertDialog {
  constructor(
    public message: string,
    public type: DialogType,
    public okCallback?: (val?: string) => void,
    public cancelCallback?: () => void,
    public defaultValue?: string,
    public okLabel?: string,
    public cancelLabel?: string) {

  }
}

export enum DialogType {
  alert,
  confirm,
  prompt
}
// ******************** End ********************//


// ******************** Growls ********************//
export class AlertCommand {
  constructor(
    public operation: 'clear' | 'add' | 'add_sticky',
    public message?: AlertMessage,
    public onRemove?: () => void) { }
}

export class AlertMessage {
  constructor(
    public severity: MessageSeverity,
    public summary: string,
    public detail?: string | undefined) { }
}

export enum MessageSeverity {
  default,
  info,
  success,
  error,
  warn,
  wait
}
// ******************** End ********************//
