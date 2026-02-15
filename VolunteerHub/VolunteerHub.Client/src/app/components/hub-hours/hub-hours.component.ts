import { Component } from '@angular/core';

@Component({
    selector: 'app-hub-hours',
    template: `
    <div class="hub-page">
      <h1 class="hub-page-title animate-in">
        <i class="fas fa-clock"></i> My Hours
      </h1>
      <div class="hub-empty-state hub-card animate-in animate-delay-2">
        <i class="fas fa-hourglass-half"></i>
        <h3>Hours Logging Coming Soon</h3>
        <p>View your logged hours, submit timesheets, and track your volunteer contributions.</p>
      </div>
    </div>
  `,
    styles: [`
    .hub-page-title i {
      color: var(--hub-primary-light);
      margin-right: 0.5rem;
    }
  `]
})
export class HubHoursComponent { }
