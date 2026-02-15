import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { VolunteerProfileData } from '../../../scheduler-data-services/volunteer-profile.service';
import { VolunteerGroupMemberService, VolunteerGroupMemberData } from '../../../scheduler-data-services/volunteer-group-member.service';
import { VolunteerGroupService, VolunteerGroupData } from '../../../scheduler-data-services/volunteer-group.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-volunteer-groups-tab',
    templateUrl: './volunteer-groups-tab.component.html',
    styleUrls: ['./volunteer-groups-tab.component.scss']
})
export class VolunteerGroupsTabComponent implements OnInit, OnDestroy {
    @Input() volunteer: VolunteerProfileData | null = null;

    public groups: VolunteerGroupData[] = [];
    public memberships: VolunteerGroupMemberData[] = [];
    public isLoading: boolean = true;

    private destroy$ = new Subject<void>();

    constructor(
        private volunteerGroupMemberService: VolunteerGroupMemberService,
        private volunteerGroupService: VolunteerGroupService,
        private alertService: AlertService
    ) { }

    ngOnInit(): void {
        if (this.volunteer) {
            this.loadMemberships();
        }
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadMemberships(): void {
        this.isLoading = true;
        this.volunteerGroupMemberService.GetVolunteerGroupMemberList({ resourceId: this.volunteer!.resourceId, includeRelations: true })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (members) => {
                    this.memberships = members || [];
                    this.groups = this.memberships
                        .map(m => m.volunteerGroup)
                        .filter((g): g is VolunteerGroupData => g != null);
                    this.isLoading = false;
                },
                error: (err) => {
                    this.isLoading = false;
                    this.alertService.showMessage('Error loading group memberships', JSON.stringify(err), MessageSeverity.error);
                }
            });
    }
}
