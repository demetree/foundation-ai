import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { VolunteerGroupData } from '../../../scheduler-data-services/volunteer-group.service';
import { VolunteerGroupMemberService, VolunteerGroupMemberData } from '../../../scheduler-data-services/volunteer-group-member.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-volunteer-group-members-tab',
    templateUrl: './volunteer-group-members-tab.component.html',
    styleUrls: ['./volunteer-group-members-tab.component.scss']
})
export class VolunteerGroupMembersTabComponent implements OnInit, OnDestroy {
    @Input() group: VolunteerGroupData | null = null;

    public members: VolunteerGroupMemberData[] = [];
    public isLoading: boolean = true;

    private destroy$ = new Subject<void>();

    constructor(
        private volunteerGroupMemberService: VolunteerGroupMemberService,
        private alertService: AlertService
    ) { }

    ngOnInit(): void {
        if (this.group) {
            this.loadMembers();
        }
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadMembers(): void {
        this.isLoading = true;
        this.volunteerGroupMemberService.GetVolunteerGroupMemberList({
            volunteerGroupId: this.group!.id,
            includeRelations: true
        }).pipe(takeUntil(this.destroy$)).subscribe({
            next: (list) => {
                this.members = list || [];
                this.isLoading = false;
            },
            error: (err) => {
                this.isLoading = false;
                this.alertService.showMessage('Error loading members', JSON.stringify(err), MessageSeverity.error);
            }
        });
    }
}
