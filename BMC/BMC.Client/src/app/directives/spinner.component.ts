import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-spinner',
  template: `
    <div class="spinner-container">
      <div class="d-flex justify-content-center align-items-center" style="height: 100%">
        <div class="spinner-border text-secondary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
/* 
     * The outer container should cover the entire area of the host element
     * and must have a fully transparent background.
     */
    .spinner-container {
      position: absolute;     /* Overlay on top of the original content */
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background-color: transparent;  /* Explicitly transparent – critical */
      display: flex;
      align-items: center;
      justify-content: center;
      /* Ensure it sits above the original content but below modals etc. */
      z-index: 10;
    }

    /*
     * The inner flex container uses Bootstrap's h-100 to fill the parent height.
     * No background needed here either – inherit transparency.
     */
    .spinner-container > .d-flex {
      width: 100%;
      height: 100%;
      background-color: transparent;
    }
  `]
})
export class SpinnerComponent {
  @Input() loading: boolean | null | undefined;
}
