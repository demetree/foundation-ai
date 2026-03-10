import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HubApiService } from '../../services/hub-api.service';
import { HubAuthService } from '../../services/hub-auth.service';
import { environment } from '../../../environments/environment';

@Component({
    selector: 'app-hub-landing',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './hub-landing.component.html',
    styleUrls: ['./hub-landing.component.scss']
})
export class HubLandingComponent implements OnInit {

    organizationName = 'Volunteer Hub';
    logoUrl: string | null = null;
    primaryColor: string | null = null;
    secondaryColor: string | null = null;
    contactEmail: string | null = null;
    isLoading = true;

    constructor(
        private api: HubApiService,
        private auth: HubAuthService,
        private router: Router
    ) { }


    ngOnInit(): void {
        // If already logged in, send to dashboard
        if (this.auth.hasValidSession()) {
            this.router.navigate(['/dashboard']);
            return;
        }

        this.loadBranding();
    }


    private loadBranding(): void {
        this.api.getBranding().subscribe({
            next: (branding) => {
                this.organizationName = branding.organizationName || 'Volunteer Hub';
                this.contactEmail = branding.email || null;
                this.primaryColor = branding.primaryColor || null;
                this.secondaryColor = branding.secondaryColor || null;

                if (branding.hasLogo && branding.logoUrl) {
                    this.logoUrl = `${environment.apiBaseUrl}${branding.logoUrl}`;
                }

                this.isLoading = false;
            },
            error: () => {
                this.isLoading = false;
            }
        });
    }


    goToRegister(): void {
        this.router.navigate(['/register']);
    }


    goToLogin(): void {
        this.router.navigate(['/login']);
    }
}
