import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';


export enum MessageSeverity {
    default,
    info,
    success,
    error,
    warn,
    wait
}

export interface AlertMessage {
    severity: MessageSeverity;
    summary: string;
    detail: string;
}

export interface AlertCommand {
    operation: string;
    message?: AlertMessage;
    onRemove?: () => void;
}

export interface AlertDialog {
    message: string;
    type: DialogType;
    okLabel?: string;
    cancelLabel?: string;
    okCallback?: (val?: any) => void;
    cancelCallback?: () => void;
    defaultValue?: string;
}

export enum DialogType {
    alert = 1,
    confirm = 2,
    prompt = 3
}


@Injectable({ providedIn: 'root' })
export class AlertService {

    private messages = new Subject<AlertCommand>();
    private stickyMessages = new Subject<AlertCommand>();
    private dialogs = new Subject<AlertDialog>();

    showMessage(summary: string, detail: string, severity: MessageSeverity) {
        this.messages.next({
            operation: 'add',
            message: { severity, summary, detail }
        });
    }

    showStickyMessage(summary: string, detail: string, severity: MessageSeverity) {
        this.stickyMessages.next({
            operation: 'add_sticky',
            message: { severity, summary, detail }
        });
    }

    resetStickyMessage() {
        this.stickyMessages.next({ operation: 'clear' });
    }

    getMessageEvent(): Observable<AlertCommand> {
        return this.messages.asObservable();
    }

    getStickyMessageEvent(): Observable<AlertCommand> {
        return this.stickyMessages.asObservable();
    }

    getDialogEvent(): Observable<AlertDialog> {
        return this.dialogs.asObservable();
    }
}
