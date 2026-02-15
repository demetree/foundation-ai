import { Component, Input, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ResourceService, ResourceData } from '../../../scheduler-data-services/resource.service';
import { AssignmentRoleService, AssignmentRoleData } from '../../../scheduler-data-services/assignment-role.service';
import { VolunteerGroupMemberService, VolunteerGroupMemberSubmitData } from '../../../scheduler-data-services/volunteer-group-member.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
    selector: 'app-volunteer-group-add-member-modal',
    templateUrl: './volunteer-group-add-member-modal.component.html',
    styleUrls: ['./volunteer-group-add-member-modal.component.scss']
})
export class VolunteerGroupAddMemberModalComponent implements OnInit {
    @Input() volunteerGroupId!: number;
    @Input() groupName!: string;
    @Input() officeId: number | null = null;

    public addForm: FormGroup;
    public isSaving: boolean = false;

    // Data streams
    public resources$!: Observable<ResourceData[]>;
    public assignmentRoles$: Observable<AssignmentRoleData[]>;

    constructor(
        public activeModal: NgbActiveModal,
        private fb: FormBuilder,
        private resourceService: ResourceService,
        private assignmentRoleService: AssignmentRoleService,
        private volunteerGroupMemberService: VolunteerGroupMemberService,
        private alertService: AlertService
    ) {
        this.addForm = this.fb.group({
            resourceId: [null, Validators.required],
            assignmentRoleId: [null],
            sequence: [1, [Validators.required, Validators.min(1)]],
            joinedDate: [new Date().toISOString().split('T')[0]]
        });

        // Load all active assignment roles
        this.assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
    }

    ngOnInit(): void {
        // Load resources, optionally filtered by office
        if (this.officeId != null) {
            this.resources$ = this.resourceService.GetResourceList({ officeId: this.officeId, active: true, deleted: false });
        } else {
            this.resources$ = this.resourceService.GetResourceList({ active: true, deleted: false });
        }
    }

    public submit(): void {
        if (this.isSaving || !this.addForm.valid) {
            return;
        }

        this.isSaving = true;

        const formValue = this.addForm.value;

        const submitData: VolunteerGroupMemberSubmitData = {
            id: 0,
            volunteerGroupId: this.volunteerGroupId,
            resourceId: Number(formValue.resourceId),
            assignmentRoleId: formValue.assignmentRoleId ? Number(formValue.assignmentRoleId) : null,
            sequence: Number(formValue.sequence),
            joinedDate: formValue.joinedDate || null,
            leftDate: null,
            notes: null,
            versionNumber: 1,
            active: true,
            deleted: false
        };

        this.volunteerGroupMemberService.PostVolunteerGroupMember(submitData).subscribe({
            next: (newMember) => {
                this.volunteerGroupMemberService.ClearAllCaches();
                this.alertService.showMessage(
                    'Member added successfully',
                    '',
                    MessageSeverity.success
                );
                this.activeModal.close(newMember);
            },
            error: (err) => {
                this.alertService.showMessage(
                    'Failed to add member',
                    err.message || 'Unknown error',
                    MessageSeverity.error
                );
                this.isSaving = false;
            }
        });
    }

    public cancel(): void {
        this.activeModal.dismiss('cancel');
    }
}
