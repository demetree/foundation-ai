import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HubApiService } from '../../services/hub-api.service';

@Component({
    selector: 'app-hub-register',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './hub-register.component.html',
    styleUrls: ['./hub-register.component.scss']
})
export class HubRegisterComponent {
    firstName = '';
    lastName = '';
    email = '';
    phone = '';
    availability = '';
    skills = '';
    emergencyContact = '';

    submitting = false;
    submitted = false;
    errorMessage = '';

    constructor(
        private api: HubApiService,
        private router: Router
    ) { }

    submit(): void {
        if (!this.firstName.trim() || !this.lastName.trim() || !this.email.trim()) {
            this.errorMessage = 'Please fill in all required fields.';
            return;
        }

        this.submitting = true;
        this.errorMessage = '';

        this.api.register({
            firstName: this.firstName.trim(),
            lastName: this.lastName.trim(),
            email: this.email.trim(),
            phone: this.phone.trim() || undefined,
            availabilityPreferences: this.availability.trim() || undefined,
            interestsAndSkillsNotes: this.skills.trim() || undefined,
            emergencyContactNotes: this.emergencyContact.trim() || undefined
        }).subscribe({
            next: () => {
                this.submitting = false;
                this.submitted = true;
            },
            error: (err) => {
                this.submitting = false;
                this.errorMessage = err.error?.error || 'Registration failed. Please try again.';
            }
        });
    }

    goToLogin(): void {
        this.router.navigate(['/login']);
    }
}
