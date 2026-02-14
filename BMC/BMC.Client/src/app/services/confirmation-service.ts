import { Injectable } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationDialogComponent } from './confirmation-dialog/confirmation-dialog.component';

@Injectable({
  providedIn: 'root',
})
export class ConfirmationService {
  constructor(private modalService: NgbModal) { }

  confirm(title: string, message: string): Promise<boolean> {
    const modalRef = this.modalService.open(ConfirmationDialogComponent, { centered: true });
    modalRef.componentInstance.title = title;
    modalRef.componentInstance.message = message;

    return modalRef.result; // Returns a promise resolved with `true` or `false`
  }
}
