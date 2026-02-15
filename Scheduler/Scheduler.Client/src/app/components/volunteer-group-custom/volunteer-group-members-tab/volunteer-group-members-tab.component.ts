import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Subject } from 'rxjs';
import { Router } from '@angular/router';
import { Component, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { VolunteerGroupService, VolunteerGroupData } from '../../../scheduler-data-services/volunteer-group.service';
import { VolunteerGroupMemberService, VolunteerGroupMemberData } from '../../../scheduler-data-services/volunteer-group-member.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { VolunteerGroupAddMemberModalComponent } from '../volunteer-group-add-member-modal/volunteer-group-add-member-modal.component';

/**
 * Members tab for the Volunteer Group detail page.
 *
 * Displays all members of this volunteer group, including:
 * - Resource name and avatar
 * - Role within the group (if specified)
 * - Sequence/order in group
 * - Join/leave dates
 *
 * Supports adding new members via a modal and removing existing members.
 *
 * Data loaded imperatively when group is available.
 */
@Component({
    selector: 'app-volunteer-group-members-tab',
    templateUrl: './volunteer-group-members-tab.component.html',
    styleUrls: ['./volunteer-group-members-tab.component.scss']
})
export class VolunteerGroupMembersTabComponent implements OnChanges {

    /**
     * The volunteer group passed from the parent detail component.
     */
    @Input() group!: VolunteerGroupData | null;

    /**
     * Triggers when a group member is changed. Used by parent to refresh counts.
     */
    @Output() memberChanged = new Subject<VolunteerGroupMemberData>();

    /**
     * Resolved group memberships.
     */
    public groupMembers: VolunteerGroupMemberData[] | null = null;

    /**
     * Loading and error states.
     */
    public isLoading: boolean = true;
    public error: string | null = null;

    constructor(
        private router: Router,
        private modalService: NgbModal,
        private volunteerGroupService: VolunteerGroupService,
        private volunteerGroupMemberService: VolunteerGroupMemberService,
        private alertService: AlertService
    ) { }


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['group'] && this.group) {
            this.loadGroupMembers();
        }
    }


    /**
     * Loads group members from the server.
     * Nav properties (resource, assignmentRole) are populated via includeRelations.
     */
    public loadGroupMembers(): void {
        if (!this.group) {
            this.groupMembers = [];
            this.isLoading = false;
            return;
        }

        this.isLoading = true;
        this.error = null;

        this.volunteerGroupMemberService.GetVolunteerGroupMemberList({
            volunteerGroupId: this.group.id,
            includeRelations: true
        }).subscribe({
            next: (members) => {
                this.groupMembers = members;
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Failed to load group members', err);
                this.error = 'Unable to load group members';
                this.groupMembers = [];
                this.isLoading = false;
            }
        });
    }


    /**
     * Opens the add-member modal.
     */
    public openAddMemberModal(): void {
        if (this.group == null) {
            return;
        }

        const modalRef = this.modalService.open(VolunteerGroupAddMemberModalComponent, {
            size: 'md',
            backdrop: 'static'
        });

        modalRef.componentInstance.volunteerGroupId = this.group.id;
        modalRef.componentInstance.groupName = this.group.name;

        modalRef.result.then(
            (newMember) => {
                this.volunteerGroupMemberService.ClearAllCaches();
                this.memberChanged.next(newMember);
                this.loadGroupMembers();
            },
            () => {
                // Dismissed — do nothing
            }
        );
    }


    /**
     * Removes a member from the group after confirmation.
     */
    public removeMember(member: VolunteerGroupMemberData): void {
        const resourceName = member.resource?.name || 'this member';

        if (!confirm(`Remove ${resourceName} from this group?`)) {
            return;
        }

        this.volunteerGroupMemberService.DeleteVolunteerGroupMember(member.id).subscribe({
            next: () => {
                this.volunteerGroupMemberService.ClearAllCaches();
                this.alertService.showMessage(
                    `${resourceName} removed from group`,
                    '',
                    MessageSeverity.success
                );
                this.memberChanged.next(member);
                this.loadGroupMembers();
            },
            error: (err) => {
                this.alertService.showMessage(
                    'Failed to remove member',
                    err.message || 'Unknown error',
                    MessageSeverity.error
                );
            }
        });
    }


    /**
     * Navigate to volunteer profile detail page.
     */
    public navigateToVolunteer(member: VolunteerGroupMemberData): void {
        if (member.resourceId) {
            this.router.navigate(['/resource', member.resourceId]);
        }
    }


    /**
     * Permission check for writing group members.
     * Uses VolunteerGroup-level permissions since VolunteerGroupMember doesn't have its own custom role.
     */
    public userIsVolunteerGroupMemberWriter(): boolean {
        return this.volunteerGroupService.userIsSchedulerVolunteerGroupWriter();
    }
}
