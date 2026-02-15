import { Component } from '@angular/core';

@Component({
    selector: 'app-hub-schedule',
    template: `
    <div class="hub-page">
      <h1 class="hub-page-title animate-in">
        <i class="fas fa-calendar"></i> My Schedule
      </h1>
      <div class="hub-empty-state hub-card animate-in animate-delay-2">
        <i class="fas fa-calendar-days"></i>
        <h3>Calendar Coming Soon</h3>
        <p>Your personal schedule will appear here with all your upcoming volunteer events.</p>
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
export class HubScheduleComponent { }
