/**
 * ShiftCustomAddEditComponent
 *
 * AI-Developed — This file was significantly developed with AI assistance.
 *
 * Thin wrapper that opens the ResourceShiftAddEditModalComponent via NgbModal.
 * Exposes an Add button (controllable via [showAddButton]) and an openModal()
 * method so the table/listing can trigger edits programmatically.
 */
import { Component, Input, Output } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Subject } from 'rxjs';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ResourceShiftService, ResourceShiftData } from '../../../scheduler-data-services/resource-shift.service';
import { ResourceShiftAddEditModalComponent } from '../../resource-custom/resource-shift-add-edit-modal/resource-shift-add-edit-modal.component';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'app-shift-custom-add-edit',
    templateUrl: './shift-custom-add-edit.component.html',
    styleUrls: ['./shift-custom-add-edit.component.scss']
})
export class ShiftCustomAddEditComponent {

    @Input() showAddButton = true;
    @Input() navigateToDetailsAfterAdd = false;

    @Output() resourceShiftChanged = new Subject<ResourceShiftData[]>();

    public modalIsDisplayed = false;

    constructor(
        private modalService: NgbModal,
        private shiftService: ResourceShiftService,
        private authService: AuthService,
        private alertService: AlertService
    ) { }

    /**
     * Opens the shift add/edit modal.
     * When called with no arguments, opens in Add mode.
     * When called with an existing shift, opens in Edit mode.
     */
    openModal(existingShift?: ResourceShiftData): void {

        if (!this.shiftService.userIsSchedulerResourceShiftWriter()) {
            this.alertService.showMessage(
                `${this.authService.currentUser?.userName} does not have permission to manage Shifts`,
                '',
                MessageSeverity.info
            );
            return;
        }

        const modalRef = this.modalService.open(ResourceShiftAddEditModalComponent, {
            size: 'md',
            backdrop: 'static'
        });

        if (existingShift) {
            modalRef.componentInstance.resourceId = existingShift.resourceId;
            modalRef.componentInstance.resourceName = existingShift.resource?.name || '';
            modalRef.componentInstance.timeZoneId = existingShift.timeZoneId;
            modalRef.componentInstance.existingShift = existingShift;
        }

        this.modalIsDisplayed = true;

        modalRef.result.then(
            (savedShift) => {
                this.shiftService.ClearAllCaches();
                this.resourceShiftChanged.next([savedShift]);
                this.modalIsDisplayed = false;
            },
            () => {
                this.modalIsDisplayed = false;
            }
        );
    }

    public userIsSchedulerResourceShiftWriter(): boolean {
        return this.shiftService.userIsSchedulerResourceShiftWriter();
    }

    public userIsSchedulerResourceShiftReader(): boolean {
        return this.shiftService.userIsSchedulerResourceShiftReader();
    }
}
