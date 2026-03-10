import { Component, OnInit } from '@angular/core';
import { HubApiService } from '../../services/hub-api.service';
import { VolunteerProfile } from '../../models/hub-models';

@Component({
    selector: 'app-hub-profile',
    templateUrl: './hub-profile.component.html',
    styleUrls: ['./hub-profile.component.scss']
})
export class HubProfileComponent implements OnInit {

    profile: VolunteerProfile | null = null;
    isLoading = true;
    isSaving = false;
    saveMessage = '';
    saveError = '';

    // Editable fields
    availabilityPreferences = '';
    interestsAndSkillsNotes = '';
    emergencyContactNotes = '';

    constructor(private api: HubApiService) { }

    ngOnInit(): void {
        this.loadProfile();
    }

    private loadProfile(): void {
        this.isLoading = true;
        this.api.getMyProfile().subscribe({
            next: (profile) => {
                this.profile = profile;
                this.availabilityPreferences = profile.availabilityPreferences || '';
                this.interestsAndSkillsNotes = profile.interestsAndSkillsNotes || '';
                this.emergencyContactNotes = profile.emergencyContactNotes || '';
                this.isLoading = false;
            },
            error: () => this.isLoading = false
        });
    }

    saveProfile(): void {
        this.isSaving = true;
        this.saveMessage = '';
        this.saveError = '';

        this.api.updateMyProfile({
            availabilityPreferences: this.availabilityPreferences,
            interestsAndSkillsNotes: this.interestsAndSkillsNotes,
            emergencyContactNotes: this.emergencyContactNotes
        }).subscribe({
            next: () => {
                this.saveMessage = 'Profile saved!';
                this.isSaving = false;
                setTimeout(() => this.saveMessage = '', 3000);
            },
            error: () => {
                this.saveError = 'Failed to save. Please try again.';
                this.isSaving = false;
            }
        });
    }
}
