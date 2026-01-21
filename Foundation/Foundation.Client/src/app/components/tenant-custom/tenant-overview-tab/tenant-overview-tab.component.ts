//
// Tenant Overview Tab Component
//
// Displays basic tenant information in the Overview tab.
//

import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { SecurityTenantData } from '../../../security-data-services/security-tenant.service';

@Component({
    selector: 'app-tenant-overview-tab',
    templateUrl: './tenant-overview-tab.component.html',
    styleUrls: ['./tenant-overview-tab.component.scss']
})
export class TenantOverviewTabComponent implements OnInit, OnChanges {

    @Input() tenant: SecurityTenantData | null = null;


    constructor() { }


    ngOnInit(): void { }


    ngOnChanges(changes: SimpleChanges): void {
        // Handle tenant changes if needed
    }
}
