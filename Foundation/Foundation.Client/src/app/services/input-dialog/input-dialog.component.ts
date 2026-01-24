//
// Input Dialog Component
//
// Reusable modal dialog for collecting text/password input from the user.
// Follows the ConfirmationDialogComponent pattern.
//

import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

export interface InputDialogOptions {
    title: string;
    message?: string;
    inputLabel?: string;
    inputPlaceholder?: string;
    inputType?: 'text' | 'password';
    confirmButtonText?: string;
    cancelButtonText?: string;
    minLength?: number;
    maxLength?: number;
    pattern?: RegExp;
    patternError?: string;
    showPasswordToggle?: boolean;
}

@Component({
    selector: 'app-input-dialog',
    templateUrl: './input-dialog.component.html',
    styleUrls: ['./input-dialog.component.scss']
})
export class InputDialogComponent implements OnInit {

    @Input() options: InputDialogOptions = {
        title: 'Enter Value',
        inputType: 'text'
    };

    public inputValue: string = '';
    public showPassword: boolean = false;
    public errorMessage: string = '';

    //
    // Computed properties for template
    //
    public get actualInputType(): string {
        if (this.options.inputType === 'password' && this.showPassword) {
            return 'text';
        }
        return this.options.inputType || 'text';
    }

    constructor(public activeModal: NgbActiveModal) { }

    ngOnInit(): void {
        // Initialize
    }

    //
    // Validation
    //
    public validate(): boolean {
        this.errorMessage = '';

        const value = this.inputValue?.trim() || '';

        if (value.length === 0) {
            this.errorMessage = 'This field is required';
            return false;
        }

        if (this.options.minLength && value.length < this.options.minLength) {
            this.errorMessage = `Minimum ${this.options.minLength} characters required`;
            return false;
        }

        if (this.options.maxLength && value.length > this.options.maxLength) {
            this.errorMessage = `Maximum ${this.options.maxLength} characters allowed`;
            return false;
        }

        if (this.options.pattern && !this.options.pattern.test(value)) {
            this.errorMessage = this.options.patternError || 'Invalid format';
            return false;
        }

        return true;
    }

    //
    // Actions
    //
    public confirm(): void {
        if (this.validate()) {
            this.activeModal.close(this.inputValue.trim());
        }
    }

    public cancel(): void {
        this.activeModal.dismiss(null);
    }

    public togglePasswordVisibility(): void {
        this.showPassword = !this.showPassword;
    }

    public onKeyDown(event: KeyboardEvent): void {
        if (event.key === 'Enter') {
            this.confirm();
        }
    }
}
