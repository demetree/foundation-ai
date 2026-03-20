//
// fm-activity-panel.component.ts
//
// AI-Developed — This file was developed with AI assistance.
//
// Child component: Recent activity timeline panel (slide-over).
//
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ActivityItem } from '../../../services/file-manager.service';


@Component({
    selector: 'fm-activity-panel',
    templateUrl: './fm-activity-panel.component.html',
    styleUrls: ['./fm-activity-panel.component.scss']
})
export class FmActivityPanelComponent {

    @Input() activityItems: ActivityItem[] = [];

    @Output() close = new EventEmitter<void>();
}
