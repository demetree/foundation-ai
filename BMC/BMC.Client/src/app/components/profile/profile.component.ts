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
export class ProfileComponent implements OnInit {
    profile: ProfileData | null = null;
    isLoading = true;
    error = '';

    stats: { icon: string; label: string; value: number; color: string }[] = [];

    constructor(
        private http: HttpClient,
        private authService: AuthService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.loadProfile();
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
