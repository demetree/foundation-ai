import { Component, OnInit } from '@angular/core';
import { HubAuthService } from '../../services/hub-auth.service';
import { HubApiService } from '../../services/hub-api.service';
import { environment } from '../../../environments/environment';

@Component({
    selector: 'app-hub-shell',
    templateUrl: './hub-shell.component.html',
    styleUrls: ['./hub-shell.component.scss']
})
export class HubShellComponent implements OnInit {

    organizationName = 'Volunteer Hub';
    logoUrl: string | null = null;

    navItems = [
        { path: '/dashboard', icon: 'fas fa-home', label: 'Home' },
        { path: '/schedule', icon: 'fas fa-calendar', label: 'Schedule' },
        { path: '/hours', icon: 'fas fa-clock', label: 'Hours' },
        { path: '/opportunities', icon: 'fas fa-search', label: 'Opportunities' },
        { path: '/profile', icon: 'fas fa-user', label: 'Profile' }
    ];

    constructor(
        public auth: HubAuthService,
        private api: HubApiService
    ) { }


    ngOnInit(): void {
        this.api.getBranding().subscribe({
            next: (branding) => {
                this.organizationName = branding.organizationName || 'Volunteer Hub';

                if (branding.hasLogo && branding.logoUrl) {
                    this.logoUrl = `${environment.apiBaseUrl}${branding.logoUrl}`;
                }
            }
        });
    }


    logout(): void {
        this.auth.logout();
    }
}
