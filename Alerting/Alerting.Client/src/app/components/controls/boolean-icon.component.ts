import { Component, Input } from '@angular/core';

@Component({
  selector: 'boolean-icon',
  template: `
    <ng-container [ngSwitch]="value">
      <i *ngSwitchCase="true" class="fa fa-check-circle text-success" aria-hidden="true" title="True"></i>
      <i *ngSwitchCase="false" class="fa fa-times-circle text-danger" aria-hidden="true" title="False"></i>
      <i *ngSwitchDefault class="fa fa-question-circle text-muted" aria-hidden="true" title="Unknown"></i>
    </ng-container>
  `,
  styles: [`
    .fa {
      font-size: 1.2rem; /* Adjust size as needed */
    }
  `]
})
export class BooleanIconComponent {
  @Input() value: boolean | null = null;
}
