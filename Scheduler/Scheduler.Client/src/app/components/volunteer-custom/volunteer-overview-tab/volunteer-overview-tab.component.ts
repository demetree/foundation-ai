import { Component, Input } from '@angular/core';
import { VolunteerProfileData } from '../../../scheduler-data-services/volunteer-profile.service';

@Component({
    selector: 'app-volunteer-overview-tab',
    templateUrl: './volunteer-overview-tab.component.html',
    styleUrls: ['./volunteer-overview-tab.component.scss']
})
export class VolunteerOverviewTabComponent {
    @Input() volunteer: VolunteerProfileData | null = null;
}
