import { Component, Input, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ResourceService, ResourceData } from '../../../scheduler-data-services/resource.service';
import { CrewService } from '../../../scheduler-data-services/crew.service';
import { AssignmentRoleService, AssignmentRoleData } from '../../../scheduler-data-services/assignment-role.service';
import { CrewMemberService, CrewMemberSubmitData } from '../../../scheduler-data-services/crew-member.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';

@Component({
  selector: 'app-crew-add-to-crew-modal',
  templateUrl: './crew-add-to-crew-modal.component.html',
  styleUrls: ['./crew-add-to-crew-modal.component.scss']
})
export class CrewAddToCrewModalComponent implements OnInit {
  @Input() crewId!: number;
  @Input() officeId!: number;   // Optional office ID to filter resources by.
  @Input() resourceName!: string;

  public addForm: FormGroup;
  public isSaving = false;

  // Data streams
  public resources$!: Observable<ResourceData[]>;
  public assignmentRoles$: Observable<AssignmentRoleData[]>;

  constructor(
    public activeModal: NgbActiveModal,
    private fb: FormBuilder,
    private resourceService: ResourceService,
    private crewService: CrewService,
    private assignmentRoleService: AssignmentRoleService,
    private crewMemberService: CrewMemberService,
    private alertService: AlertService
  ) {
    this.addForm = this.fb.group({
      resourceId: [null, Validators.required],
      assignmentRoleId: [null],
      sequence: [1, [Validators.required, Validators.min(1)]]
    });

    // Load all active assignment roles - do this during construction because we don't care about the input values for this
    this.assignmentRoles$ = this.assignmentRoleService.GetAssignmentRoleList();
  }

  ngOnInit(): void {

    // Load resources after we have the office ID parameter set
    this.resources$ = this.resourceService.GetResourceList({ officeId: this.officeId, active: true, deleted: false });


    // Default issue date to today
    const today = new Date().toISOString().split('T')[0];

  }

  public submit(): void {
    if (this.isSaving || !this.addForm.valid) return;

    this.isSaving = true;

    const formValue = this.addForm.value;

    const submitData: CrewMemberSubmitData = {
      id: 0,
      crewId: this.crewId,
      resourceId: Number(formValue.resourceId),
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
