import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Location } from '@angular/common';
import { VolunteerProfileService, VolunteerProfileData } from '../../../scheduler-data-services/volunteer-profile.service';
import { VolunteerCustomAddEditComponent } from '../volunteer-custom-add-edit/volunteer-custom-add-edit.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'app-volunteer-custom-detail',
    templateUrl: './volunteer-custom-detail.component.html',
    styleUrls: ['./volunteer-custom-detail.component.scss']
})
export class VolunteerCustomDetailComponent implements OnInit, OnDestroy {
    @ViewChild(VolunteerCustomAddEditComponent) addEditComponent!: VolunteerCustomAddEditComponent;

    public volunteer: VolunteerProfileData | null = null;
    public isLoading: boolean = true;
    public activeTab: string = 'overview';

    private destroy$ = new Subject<void>();

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private location: Location,
        private volunteerProfileService: VolunteerProfileService,
        private alertService: AlertService,
        private authService: AuthService
    ) { }

    ngOnInit(): void {
        this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
            const id = params['volunteerProfileId'];
            if (id) {
                this.loadVolunteer(Number(id));
            }
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadVolunteer(id: number): void {
        this.isLoading = true;
        this.volunteerProfileService.ClearRecordCache(id);
        this.volunteerProfileService.GetVolunteerProfile(id, true).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (data) => {
                this.volunteer = data;
                this.isLoading = false;
            },
            error: (err) => {
                this.isLoading = false;
                this.alertService.showMessage('Error loading Volunteer', JSON.stringify(err), MessageSeverity.error);
            }
        });
    }

    onTabChange(tabId: string): void {
        this.activeTab = tabId;
    }

    openEditModal(): void {
        if (this.volunteer && this.addEditComponent) {
            this.addEditComponent.openModal(this.volunteer);
        }
    }

    onVolunteerChanged(): void {
        if (this.volunteer) {
            this.loadVolunteer(this.volunteer.id as number);
        }
    }

    goBack(): void {
        this.location.back();
    }

    canGoBack(): boolean {
        return true;
    }

    navigateToResource(): void {
        if (this.volunteer?.resourceId) {
            this.router.navigate(['/resources', this.volunteer.resourceId]);
        }
    }

    public userIsVolunteerWriter(): boolean {
        return this.volunteerProfileService.userIsSchedulerVolunteerProfileWriter();
    }

    public refreshRowCountObservables(): void {
        // Refresh row counts after child tab mutations
        if (this.volunteer) {
            this.volunteer.ClearVolunteerProfileChangeHistoriesCache();
        }
    }
}
