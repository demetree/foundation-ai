import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Location } from '@angular/common';
import { VolunteerGroupService, VolunteerGroupData } from '../../../scheduler-data-services/volunteer-group.service';
import { VolunteerGroupCustomAddEditComponent } from '../volunteer-group-custom-add-edit/volunteer-group-custom-add-edit.component';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'app-volunteer-group-custom-detail',
    templateUrl: './volunteer-group-custom-detail.component.html',
    styleUrls: ['./volunteer-group-custom-detail.component.scss']
})
export class VolunteerGroupCustomDetailComponent implements OnInit, OnDestroy {
    @ViewChild(VolunteerGroupCustomAddEditComponent) addEditComponent!: VolunteerGroupCustomAddEditComponent;

    public group: VolunteerGroupData | null = null;
    public isLoading: boolean = true;
    public activeTab: string = 'overview';

    // Change history
    public auditHistory: any[] | null = null;
    public isLoadingHistory: boolean = false;

    private destroy$ = new Subject<void>();

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private location: Location,
        private volunteerGroupService: VolunteerGroupService,
        private alertService: AlertService,
        private authService: AuthService
    ) { }

    ngOnInit(): void {
        this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
            const id = params['volunteerGroupId'];
            if (id) {
                this.loadGroup(Number(id));
            }
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadGroup(id: number): void {
        this.isLoading = true;
        this.volunteerGroupService.ClearRecordCache(id);
        this.volunteerGroupService.GetVolunteerGroup(id, true).pipe(
            takeUntil(this.destroy$)
        ).subscribe({
            next: (data) => {
                this.group = data;
                this.isLoading = false;
            },
            error: (err) => {
                this.isLoading = false;
                this.alertService.showMessage('Error loading Group', JSON.stringify(err), MessageSeverity.error);
            }
        });
    }

    onTabChange(tabId: string): void {
        this.activeTab = tabId;
        if (this.activeTab === 'history') {
            this.loadHistory();
        }
    }

    public loadHistory(): void {
        if (this.auditHistory != null || !this.group) {
            return;
        }
        this.isLoadingHistory = true;
        this.volunteerGroupService.GetVolunteerGroupAuditHistory(this.group.id as number, true).subscribe({
            next: (data) => { this.auditHistory = data || []; this.isLoadingHistory = false; },
            error: () => { this.auditHistory = []; this.isLoadingHistory = false; }
        });
    }

    openEditModal(): void {
        if (this.group && this.addEditComponent) {
            this.addEditComponent.openModal(this.group);
        }
    }

    onGroupChanged(): void {
        this.auditHistory = null;
        if (this.group) {
            this.loadGroup(this.group.id as number);
        }
    }

    goBack(): void {
        this.location.back();
    }

    public userIsGroupWriter(): boolean {
        return this.volunteerGroupService.userIsSchedulerVolunteerGroupWriter();
    }
}
