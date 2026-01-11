import { Component, Input, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CrewService, CrewData } from '../../../scheduler-data-services/crew.service';
import { AssignmentRoleService, AssignmentRoleData } from '../../../scheduler-data-services/assignment-role.service';
import { CrewMemberService, CrewMemberSubmitData } from '../../../scheduler-data-services/crew-member.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-resource-add-to-crew-modal',
  templateUrl: './resource-add-to-crew-modal.component.html',
  styleUrls: ['./resource-add-to-crew-modal.component.scss']
})
export class ResourceAddToCrewModalComponent implements OnInit {
  @Input() resourceId!: number;             // The resource being added to a crew
  @Input() officeId!: number | null;        // The optional office constraint that the resource belongs to.
  @Input() resourceName!: string;

  public addForm: FormGroup;
  public isSaving = false;

  // Data streams
  public crews$!: Observable<CrewData[]>;
  public assignmentRoles$: Observable<AssignmentRoleData[]>;

  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private crewService: CrewService,
    private assignmentRoleService: AssignmentRoleService,
    private crewMemberService: CrewMemberService,
    private alertService: AlertService
  ) {
    this.addForm = this.fb.group({
      crewId: [null, Validators.required],
      assignmentRoleId: [null],
      sequence: [1, [Validators.required, Validators.min(1)]]
    });

    // Load all active assignment roles - We can do this on construction because we don't care about the inputs at this point yet.
    this.assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  }

  ngOnInit(): void {

    // Load all active crews - filter by office if provided.
    this.crews$ = this.crewService.GetCrewList({ officeId: this.officeId, active: true, deleted: false });

    // Default issue date to today
    const today = new Date().toISOString().split('T')[0];

  }

  public submit(): void {
    if (this.isSaving || !this.addForm.valid) return;

    this.isSaving = true;

    const formValue = this.addForm.value;

    const submitData: CrewMemberSubmitData = {
      id: 0,
      crewId: Number(formValue.crewId),
      resourceId: this.resourceId,
      assignmentRoleId: formValue.assignmentRoleId ? Number(formValue.assignmentRoleId) : null,
      sequence: Number(formValue.sequence),
      iconId: null,  // Could allow selection later
      color: null,
      versionNumber: 1,
      active: true,
      deleted: false
    };

    this.crewMemberService.PostCrewMember(submitData).subscribe({
      next: (newMember) => {

        this.crewMemberService.ClearAllCaches();

        this.alertService.showMessage(
          `${this.resourceName} successfully added to crew`,
          '',
          MessageSeverity.success
        );
        this.activeModal.close(newMember);
      },
      error: (err) => {
        this.alertService.showMessage(
          'Failed to add to crew',
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
