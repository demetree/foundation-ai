//
// Module Overview Tab Component
//
// Displays basic module information in the Overview tab.
//

import { Component, Input } from '@angular/core';
import { ModuleData } from '../../../security-data-services/module.service';

@Component({
    selector: 'app-module-overview-tab',
    templateUrl: './module-overview-tab.component.html',
    styleUrls: ['./module-overview-tab.component.scss']
})
export class ModuleOverviewTabComponent {

    @Input() module: ModuleData | null = null;

}
