import { Component, Input } from '@angular/core';
import { VolunteerGroupData } from '../../../scheduler-data-services/volunteer-group.service';

@Component({
    selector: 'app-volunteer-group-overview-tab',
    templateUrl: './volunteer-group-overview-tab.component.html',
    styleUrls: ['./volunteer-group-overview-tab.component.scss']
})
export class VolunteerGroupOverviewTabComponent {
    @Input() group: VolunteerGroupData | null = null;
}
