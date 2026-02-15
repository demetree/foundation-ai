import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeStyle } from '@angular/platform-browser';

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
    selector: 'app-public-profile',
    templateUrl: './public-profile.component.html',
    styleUrl: './public-profile.component.scss'
})
export class PublicProfileComponent implements OnInit {
    profile: ProfileData | null = null;
    profileId: number = 0;
    isLoading = true;
    notFound = false;

    bannerSafeStyle: SafeStyle | null = null;
    stats: { icon: string; label: string; value: number; color: string }[] = [];

    constructor(
        private http: HttpClient,
        private route: ActivatedRoute,
        private router: Router,
        private sanitizer: DomSanitizer
    ) { }

    ngOnInit(): void {
        this.route.params.subscribe(params => {
            this.profileId = +params['id'];
            if (this.profileId) {
                this.loadProfile();
            } else {
                this.notFound = true;
                this.isLoading = false;
            }
        });
    }

    loadProfile(): void {
        this.isLoading = true;
        this.notFound = false;

        // No auth headers — public endpoint
        this.http.get<ProfileData>(`/api/profile/${this.profileId}`).subscribe({
            next: (data) => {
                this.profile = data;
                this.buildStats(data);
                this.isLoading = false;

                if (data.hasBanner && data.bannerUrl) {
                    this.bannerSafeStyle = this.sanitizer.bypassSecurityTrustStyle(`url(${data.bannerUrl})`);
                }
            },
            error: (err) => {
                if (err.status === 404) {
                    this.notFound = true;
                }
                this.isLoading = false;
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
