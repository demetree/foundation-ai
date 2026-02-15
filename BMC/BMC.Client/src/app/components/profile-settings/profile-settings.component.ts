import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface ProfileData {
    id: number;
    displayName: string;
    bio: string;
    location: string;
    avatarImagePath: string;
    profileBannerImagePath: string;
    websiteUrl: string;
    isPublic: boolean;
}

interface LinkType {
    id: number;
    name: string;
    description: string;
    iconCssClass: string;
    sequence: number;
}

interface ProfileLink {
    userProfileLinkTypeId: number;
    url: string;
    displayLabel: string;
    sequence: number;
}

@Component({
    selector: 'app-profile-settings',
    templateUrl: './profile-settings.component.html',
    styleUrl: './profile-settings.component.scss'
})
export class ProfileSettingsComponent implements OnInit {
    // Form fields
    displayName = '';
    bio = '';
    location = '';
    avatarImagePath = '';
    profileBannerImagePath = '';
    websiteUrl = '';
    isPublic = true;

    // Links
    links: ProfileLink[] = [];
    linkTypes: LinkType[] = [];

    // State
    isLoading = true;
    isSaving = false;
    error = '';
    successMessage = '';

    constructor(
        private http: HttpClient,
        private authService: AuthService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.loadData();
    }

    loadData(): void {
        this.isLoading = true;
        const headers = this.authService.GetAuthenticationHeaders();

        // Load profile
        this.http.get<any>('/api/profile/mine', { headers }).subscribe({
            next: (profile) => {
                this.displayName = profile.displayName || '';
                this.bio = profile.bio || '';
                this.location = profile.location || '';
                this.avatarImagePath = profile.avatarImagePath || '';
                this.profileBannerImagePath = profile.profileBannerImagePath || '';
                this.websiteUrl = profile.websiteUrl || '';
                this.isPublic = profile.isPublic;

                // Map links
                this.links = (profile.links || []).map((l: any) => ({
                    userProfileLinkTypeId: l.userProfileLinkTypeId,
                    url: l.url,
                    displayLabel: l.displayLabel || '',
                    sequence: l.sequence ?? 0
                }));

                this.isLoading = false;
            },
            error: (err) => {
                this.error = 'Failed to load profile.';
                this.isLoading = false;
                console.error(err);
            }
        });

        // Load link types
        this.http.get<LinkType[]>('/api/profile/link-types', { headers }).subscribe({
            next: (types) => {
                this.linkTypes = types;
            },
            error: (err) => {
                console.error('Failed to load link types:', err);
            }
        });
    }

    addLink(): void {
        this.links.push({
            userProfileLinkTypeId: this.linkTypes.length > 0 ? this.linkTypes[0].id : 0,
            url: '',
            displayLabel: '',
            sequence: this.links.length
        });
    }

    removeLink(index: number): void {
        this.links.splice(index, 1);
        // Resequence
        this.links.forEach((l, i) => l.sequence = i);
    }

    getLinkTypeName(id: number): string {
        const type = this.linkTypes.find(t => t.id === id);
        return type?.name || 'Unknown';
    }

    save(): void {
        this.error = '';
        this.successMessage = '';

        if (!this.displayName.trim()) {
            this.error = 'Display name is required.';
            return;
        }

        if (this.displayName.length > 50) {
            this.error = 'Display name must be 50 characters or fewer.';
            return;
        }

        this.isSaving = true;
        const headers = this.authService.GetAuthenticationHeaders();

        // Save profile
        const profilePayload = {
            displayName: this.displayName.trim(),
            bio: this.bio.trim(),
            location: this.location.trim(),
            avatarImagePath: this.avatarImagePath.trim(),
            profileBannerImagePath: this.profileBannerImagePath.trim(),
            websiteUrl: this.websiteUrl.trim(),
            isPublic: this.isPublic
        };

        this.http.put('/api/profile/mine', profilePayload, { headers }).subscribe({
            next: () => {
                // Also save links
                const validLinks = this.links.filter(l => l.url.trim().length > 0);
                this.http.put('/api/profile/mine/links', validLinks, { headers }).subscribe({
                    next: () => {
                        this.isSaving = false;
                        this.successMessage = 'Profile saved successfully!';
                        setTimeout(() => this.successMessage = '', 3000);
                    },
                    error: (err) => {
                        this.isSaving = false;
                        this.error = 'Saved profile but failed to save links.';
                        console.error(err);
                    }
                });
            },
            error: (err) => {
                this.isSaving = false;
                this.error = 'Failed to save profile.';
                console.error(err);
            }
        });
    }

    cancel(): void {
        this.router.navigate(['/profile']);
    }
}
