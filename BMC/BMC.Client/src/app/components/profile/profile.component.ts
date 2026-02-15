import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { DomSanitizer, SafeStyle } from '@angular/platform-browser';
import { AuthService } from '../../services/auth.service';

interface ProfileData {
    id: number;
    displayName: string;
    bio: string;
    location: string;
    hasAvatar: boolean;
    avatarUrl: string | null;
    hasBanner: boolean;
    bannerUrl: string | null;
    websiteUrl: string;
    isPublic: boolean;
    memberSinceDate: string;
    totalPartsOwned: number;
    totalUniquePartsOwned: number;
    totalSetsOwned: number;
    totalMocsPublished: number;
    totalFollowers: number;
    totalFollowing: number;
    totalLikesReceived: number;
    totalAchievementPoints: number;
    links: ProfileLink[];
}

interface ProfileLink {
    id: number;
    userProfileLinkTypeId: number;
    linkTypeName: string;
    iconCssClass: string;
    url: string;
    displayLabel: string;
    sequence: number;
}

@Component({
    selector: 'app-profile',
    templateUrl: './profile.component.html',
    styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit, OnDestroy {
    profile: ProfileData | null = null;
    isLoading = true;
    error = '';

    // Blob URLs for authenticated image loading
    avatarBlobUrl: string | null = null;
    bannerBlobUrl: string | null = null;
    bannerSafeStyle: SafeStyle | null = null;

    stats: { icon: string; label: string; value: number; color: string }[] = [];

    constructor(
        private http: HttpClient,
        private authService: AuthService,
        private router: Router,
        private sanitizer: DomSanitizer
    ) { }

    ngOnInit(): void {
        this.loadProfile();
    }

    ngOnDestroy(): void {
        if (this.avatarBlobUrl) { URL.revokeObjectURL(this.avatarBlobUrl); }
        if (this.bannerBlobUrl) { URL.revokeObjectURL(this.bannerBlobUrl); }
    }

    loadProfile(): void {
        this.isLoading = true;
        this.error = '';
        const headers = this.authService.GetAuthenticationHeaders();

        this.http.get<ProfileData>('/api/profile/mine', { headers }).subscribe({
            next: (data) => {
                this.profile = data;
                this.buildStats(data);
                this.isLoading = false;

                // Fetch images as blobs (browser <img src> can't carry Bearer tokens)
                if (data.hasAvatar) {
                    this.fetchImageBlob('/api/profile/mine/avatar', 'avatar');
                }
                if (data.hasBanner) {
                    this.fetchImageBlob('/api/profile/mine/banner', 'banner');
                }
            },
            error: (err) => {
                this.error = 'Failed to load profile.';
                this.isLoading = false;
                console.error('Profile load error:', err);
            }
        });
    }

    buildStats(p: ProfileData): void {
        this.stats = [
            { icon: 'fas fa-puzzle-piece', label: 'Parts Owned', value: p.totalPartsOwned, color: '#ffa726' },
            { icon: 'fas fa-layer-group', label: 'Sets Owned', value: p.totalSetsOwned, color: '#42a5f5' },
            { icon: 'fas fa-drafting-compass', label: 'MOCs Published', value: p.totalMocsPublished, color: '#ab47bc' },
            { icon: 'fas fa-users', label: 'Followers', value: p.totalFollowers, color: '#26a69a' },
            { icon: 'fas fa-heart', label: 'Likes Received', value: p.totalLikesReceived, color: '#ef5350' },
            { icon: 'fas fa-trophy', label: 'Achievement Pts', value: p.totalAchievementPoints, color: '#ffca28' },
        ];
    }

    private fetchImageBlob(url: string, type: 'avatar' | 'banner'): void {
        const headers = this.authService.GetAuthenticationHeaders().delete('Content-Type');
        this.http.get(url, { headers, responseType: 'blob' }).subscribe({
            next: (blob) => {
                const objectUrl = URL.createObjectURL(blob);
                if (type === 'avatar') {
                    if (this.avatarBlobUrl) { URL.revokeObjectURL(this.avatarBlobUrl); }
                    this.avatarBlobUrl = objectUrl;
                } else {
                    if (this.bannerBlobUrl) { URL.revokeObjectURL(this.bannerBlobUrl); }
                    this.bannerBlobUrl = objectUrl;
                    this.bannerSafeStyle = this.sanitizer.bypassSecurityTrustStyle('url(' + objectUrl + ')');
                }
            },
            error: (err) => console.warn(`Failed to load ${type}:`, err)
        });
    }

    goToSettings(): void {
        this.router.navigate(['/profile/settings']);
    }

    getMemberSinceFormatted(): string {
        if (!this.profile?.memberSinceDate) return '';
        const d = new Date(this.profile.memberSinceDate);
        return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long' });
    }

    getAvatarInitials(): string {
        if (!this.profile?.displayName) return '?';
        return this.profile.displayName.charAt(0).toUpperCase();
    }
}
