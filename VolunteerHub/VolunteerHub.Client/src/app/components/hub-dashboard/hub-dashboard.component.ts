import { Component, OnInit } from '@angular/core';
import { HubAuthService } from '../../services/hub-auth.service';
import { HubApiService } from '../../services/hub-api.service';
import { VolunteerProfile, VolunteerAssignment } from '../../models/hub-models';

@Component({
    selector: 'app-hub-dashboard',
    templateUrl: './hub-dashboard.component.html',
    styleUrls: ['./hub-dashboard.component.scss']
})
export class HubDashboardComponent implements OnInit {

    userName: string = '';
    profile: VolunteerProfile | null = null;
    upcomingAssignments: VolunteerAssignment[] = [];
    isLoading = true;

    constructor(
        private auth: HubAuthService,
        private api: HubApiService
    ) { }

    ngOnInit(): void {
        this.userName = this.auth.userName || 'Volunteer';
        this.loadDashboard();
    }

    private async loadDashboard(): Promise<void> {
        try {
            // Load profile
            this.api.getMyProfile().subscribe({
                next: (profile) => this.profile = profile,
                error: (err) => console.warn('Could not load profile', err)
            });

            // Load upcoming assignments (next 30 days)
            const from = new Date();
            const to = new Date();
            to.setDate(to.getDate() + 30);

            this.api.getMyAssignments(from, to).subscribe({
                next: (assignments) => {
                    this.upcomingAssignments = assignments.slice(0, 5);
                    this.isLoading = false;
                },
                error: (err) => {
                    console.warn('Could not load assignments', err);
                    this.isLoading = false;
                }
            });
        } catch {
            this.isLoading = false;
        }
    }
}
