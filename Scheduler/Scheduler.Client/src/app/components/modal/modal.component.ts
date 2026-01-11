import { Component } from '@angular/core';
import { MdbModalRef } from 'mdb-angular-ui-kit/modal';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss'],
})
export class ModalComponent {
  constructor(public modalRef: MdbModalRef<ModalComponent>) { }

  submitForm() {
    console.log("Form submitted!");

    // Show an alert only if all mandatory fields are completed
    alert("Project added successfully!");

    // Close the modal after successful submission
    this.modalRef.close();
  }

  cancelForm() {
    // Handle form cancellation
    if (confirm("Are you sure you want to discard changes?")) {
      this.modalRef.close();
    }
  }
}
