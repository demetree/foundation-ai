//
// User Overview Tab Component
//
// Displays at-a-glance user information: contact info, account settings, 2FA, and assignments.
//

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { SecurityUserData } from '../../../security-data-services/security-user.service';

@Component({
    selector: 'app-user-overview-tab',
    templateUrl: './user-overview-tab.component.html',
    styleUrls: ['./user-overview-tab.component.scss']
})
export class UserOverviewTabComponent implements OnChanges {

    @Input() user: SecurityUserData | null = null;


    ngOnChanges(changes: SimpleChanges): void {
        // React to user changes if needed
    }


    //
    // Display helpers
    //
    public formatRelativeTime(dateString: string | null): string {
        if (dateString == null) return 'Never';

        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now.getTime() - date.getTime();
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins} minutes ago`;
        if (diffHours < 24) return `${diffHours} hours ago`;
        if (diffDays < 7) return `${diffDays} days ago`;

        return date.toLocaleDateString();
    }


    public formatDate(dateString: string | null): string {
        if (dateString == null) return '—';
        return new Date(dateString).toLocaleString();
    }
}
