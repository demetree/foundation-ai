//
// Input Dialog Service
//
// Service for displaying input dialogs (text or password) in a styled modal.
// Follows the ConfirmationService pattern.
//

import { Injectable } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { InputDialogComponent, InputDialogOptions } from './input-dialog/input-dialog.component';

@Injectable({
    providedIn: 'root',
})
export class InputDialogService {

    constructor(private modalService: NgbModal) { }

    /**
     * Opens an input dialog and returns the entered value.
     * Returns null if the user cancels.
     */
    public prompt(options: InputDialogOptions): Promise<string | null> {
        const modalRef = this.modalService.open(InputDialogComponent, {
            centered: true,
            backdrop: 'static',
            keyboard: true
        });

        modalRef.componentInstance.options = options;

        return modalRef.result.catch(() => null);
    }

    /**
     * Convenience method for password input with standard complexity requirements.
     */
    public promptPassword(title: string, message?: string): Promise<string | null> {
        return this.prompt({
            title,
            message,
            inputLabel: 'Password',
            inputType: 'password',
            inputPlaceholder: 'Enter password',
            confirmButtonText: 'Set Password',
            minLength: 8,
            showPasswordToggle: true,
            pattern: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;':",.<>\/?\\`~]).{8,}$/,
            patternError: 'Password must contain uppercase, lowercase, number, and special character'
        });
    }

    /**
     * Convenience method for simple text input.
     */
    public promptText(title: string, options?: Partial<InputDialogOptions>): Promise<string | null> {
        return this.prompt({
            title,
            inputType: 'text',
            ...options
        });
    }
}
