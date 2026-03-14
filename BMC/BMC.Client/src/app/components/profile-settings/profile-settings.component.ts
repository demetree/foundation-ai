import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { BrickbergPreferenceService } from '../../services/brickberg-preference.service';

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

interface AvailableTheme {
    id: number;
    name: string;
}

interface PreferredTheme {
    legoThemeId: number;
    sequence: number;
}

@Component({
    selector: 'app-profile-settings',
    templateUrl: './profile-settings.component.html',
    styleUrl: './profile-settings.component.scss'
})
export class ProfileSettingsComponent implements OnInit, OnDestroy {
    // Form fields
    displayName = '';
    bio = '';
    location = '';
    websiteUrl = '';
    isPublic = true;
    brickbergEnabled = false;

    // Image state
    hasAvatar = false;
    hasBanner = false;
    avatarPreviewUrl: string | null = null;
    bannerPreviewUrl: string | null = null;
    isUploadingAvatar = false;
    isUploadingBanner = false;
    imageVersion = Date.now();

    // Links
    links: ProfileLink[] = [];
    linkTypes: LinkType[] = [];

    // Preferred Themes
    availableThemes: AvailableTheme[] = [];
    selectedThemeIds: number[] = [];
    themeSearchTerm = '';

    // State
    isLoading = true;
    isSaving = false;
    error = '';
    successMessage = '';

    constructor(
        private http: HttpClient,
        private authService: AuthService,
        private router: Router,
        public brickbergPref: BrickbergPreferenceService
    ) { }

    ngOnInit(): void {
        this.loadData();
        this.brickbergEnabled = this.brickbergPref.isEnabled;
    }

    ngOnDestroy(): void {
        // Revoke any blob URLs to free memory
        if (this.avatarPreviewUrl) { URL.revokeObjectURL(this.avatarPreviewUrl); }
        if (this.bannerPreviewUrl) { URL.revokeObjectURL(this.bannerPreviewUrl); }
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
                this.websiteUrl = profile.websiteUrl || '';
                this.isPublic = profile.isPublic;
                this.hasAvatar = profile.hasAvatar;
                this.hasBanner = profile.hasBanner;

                if (profile.hasAvatar) {
                    this.fetchImageAsBlob('/api/profile/mine/avatar', 'avatar');
                }
                if (profile.hasBanner) {
                    this.fetchImageAsBlob('/api/profile/mine/banner', 'banner');
                }

                // Map links
                this.links = (profile.links || []).map((l: any) => ({
                    userProfileLinkTypeId: l.userProfileLinkTypeId,
                    url: l.url,
                    displayLabel: l.displayLabel || '',
                    sequence: l.sequence ?? 0
                }));

                // Map preferred themes
                this.selectedThemeIds = (profile.preferredThemes || []).map((pt: any) => pt.legoThemeId);

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

        // Load available LEGO themes (top-level themes only)
        this.http.get<any[]>('/api/LegoThemes', { headers, params: { pageSize: '500' } }).subscribe({
            next: (themes) => {
                this.availableThemes = themes
                    .filter((t: any) => !t.legoThemeId)   // Top-level only
                    .map((t: any) => ({ id: t.id, name: t.name }))
                    .sort((a: AvailableTheme, b: AvailableTheme) => a.name.localeCompare(b.name));
            },
            error: (err) => {
                console.error('Failed to load LEGO themes:', err);
            }
        });
    }


    // ------- Image Upload -------

    onAvatarFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (!input.files || input.files.length === 0) return;
        this.uploadImage(input.files[0], 'avatar');
        input.value = ''; // Reset so same file can be re-selected
    }

    onBannerFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (!input.files || input.files.length === 0) return;
        this.uploadImage(input.files[0], 'banner');
        input.value = '';
    }

    private uploadImage(file: File, type: 'avatar' | 'banner'): void {
        const maxSize = type === 'avatar' ? 2 * 1024 * 1024 : 5 * 1024 * 1024;
        if (file.size > maxSize) {
            this.error = `File exceeds the maximum size of ${maxSize / (1024 * 1024)} MB.`;
            return;
        }
        if (!file.type.startsWith('image/')) {
            this.error = 'File must be an image.';
            return;
        }

        this.error = '';
        if (type === 'avatar') { this.isUploadingAvatar = true; }
        else { this.isUploadingBanner = true; }

        const formData = new FormData();
        formData.append('file', file);

        // Use auth-only headers — do NOT set Content-Type so the browser
        // auto-generates the multipart/form-data boundary for FormData.
        const uploadHeaders = new HttpHeaders({
            Authorization: `Bearer ${this.authService.accessToken}`
        });
        this.http.post<any>(`/api/profile/mine/${type}`, formData, { headers: uploadHeaders }).subscribe({
            next: () => {
                if (type === 'avatar') {
                    this.hasAvatar = true;
                    this.isUploadingAvatar = false;
                    this.fetchImageAsBlob('/api/profile/mine/avatar', 'avatar');
                } else {
                    this.hasBanner = true;
                    this.isUploadingBanner = false;
                    this.fetchImageAsBlob('/api/profile/mine/banner', 'banner');
                }
                this.successMessage = `${type === 'avatar' ? 'Avatar' : 'Banner'} uploaded!`;
                setTimeout(() => this.successMessage = '', 3000);
            },
            error: (err) => {
                if (type === 'avatar') { this.isUploadingAvatar = false; }
                else { this.isUploadingBanner = false; }
                this.error = `Failed to upload ${type}.`;
                console.error(err);
            }
        });
    }

    removeAvatar(): void {
        const headers = this.authService.GetAuthenticationHeaders();
        this.http.delete('/api/profile/mine/avatar', { headers }).subscribe({
            next: () => {
                this.hasAvatar = false;
                this.avatarPreviewUrl = null;
            },
            error: (err) => {
                this.error = 'Failed to remove avatar.';
                console.error(err);
            }
        });
    }

    removeBanner(): void {
        const headers = this.authService.GetAuthenticationHeaders();
        this.http.delete('/api/profile/mine/banner', { headers }).subscribe({
            next: () => {
                this.hasBanner = false;
                this.bannerPreviewUrl = null;
            },
            error: (err) => {
                this.error = 'Failed to remove banner.';
                console.error(err);
            }
        });
    }


    /**
     * Fetches an image from an authenticated endpoint as a blob,
     * then creates a local object URL for the <img> binding.
     */
    private fetchImageAsBlob(url: string, type: 'avatar' | 'banner'): void {
        const headers = this.authService.GetAuthenticationHeaders().delete('Content-Type');
        this.http.get(url, { headers, responseType: 'blob' }).subscribe({
            next: (blob) => {
                const objectUrl = URL.createObjectURL(blob);
                if (type === 'avatar') {
                    if (this.avatarPreviewUrl) { URL.revokeObjectURL(this.avatarPreviewUrl); }
                    this.avatarPreviewUrl = objectUrl;
                } else {
                    if (this.bannerPreviewUrl) { URL.revokeObjectURL(this.bannerPreviewUrl); }
                    this.bannerPreviewUrl = objectUrl;
                }
            },
            error: (err) => {
                console.warn(`Failed to load ${type} preview:`, err);
            }
        });
    }


    // ------- Preferred Themes -------

    toggleTheme(themeId: number): void {
        const idx = this.selectedThemeIds.indexOf(themeId);
        if (idx >= 0) {
            this.selectedThemeIds.splice(idx, 1);
        } else {
            this.selectedThemeIds.push(themeId);
        }
    }

    isThemeSelected(themeId: number): boolean {
        return this.selectedThemeIds.includes(themeId);
    }

    get filteredThemes(): AvailableTheme[] {
        if (!this.themeSearchTerm.trim()) {
            return this.availableThemes;
        }
        const term = this.themeSearchTerm.toLowerCase();
        return this.availableThemes.filter(t => t.name.toLowerCase().includes(term));
    }


    // ------- Social Links -------

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
        this.links.forEach((l, i) => l.sequence = i);
    }

    getLinkTypeName(id: number): string {
        const type = this.linkTypes.find(t => t.id === id);
        return type?.name || 'Unknown';
    }


    // ------- Save Profile + Links + Preferred Themes -------

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

        const profilePayload = {
            displayName: this.displayName.trim(),
            bio: this.bio.trim(),
            location: this.location.trim(),
            websiteUrl: this.websiteUrl.trim(),
            isPublic: this.isPublic
        };

        this.http.put('/api/profile/mine', profilePayload, { headers }).subscribe({
            next: () => {
                const validLinks = this.links.filter(l => l.url.trim().length > 0);
                this.http.put('/api/profile/mine/links', validLinks, { headers }).subscribe({
                    next: () => {
                        // Save preferred themes
                        const themesPayload = this.selectedThemeIds.map((id, i) => ({ legoThemeId: id, sequence: i }));
                        this.http.put('/api/profile/mine/preferred-themes', themesPayload, { headers }).subscribe({
                            next: () => {
                                this.isSaving = false;
                                this.successMessage = 'Profile saved successfully!';
                                setTimeout(() => this.successMessage = '', 3000);
                            },
                            error: (err) => {
                                this.isSaving = false;
                                this.error = 'Saved profile and links but failed to save preferred themes.';
                                console.error(err);
                            }
                        });
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
