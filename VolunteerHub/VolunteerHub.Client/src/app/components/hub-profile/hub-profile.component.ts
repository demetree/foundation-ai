import { Component } from '@angular/core';

@Component({
    selector: 'app-hub-profile',
    template: `
    <div class="hub-page">
      <h1 class="hub-page-title animate-in">
        <i class="fas fa-user"></i> My Profile
      </h1>
      <div class="hub-empty-state hub-card animate-in animate-delay-2">
        <i class="fas fa-id-badge"></i>
        <h3>Profile Management Coming Soon</h3>
        <p>Edit your availability, skills, emergency contacts, and notification preferences.</p>
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
export class HubProfileComponent { }
