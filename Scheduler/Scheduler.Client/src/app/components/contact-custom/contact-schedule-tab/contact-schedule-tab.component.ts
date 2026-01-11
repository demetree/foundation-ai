import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Observable, combineLatest, map, of } from 'rxjs';
import { ContactData } from '../../../scheduler-data-services/contact.service';
import { ContactInteractionData } from '../../../scheduler-data-services/contact-interaction.service';

@Component({
    selector: 'app-contact-schedule-tab',
    templateUrl: './contact-schedule-tab.component.html',
    styleUrls: ['./contact-schedule-tab.component.scss']
})
export class ContactScheduleTabComponent implements OnInit, OnDestroy {

    @Input() contact!: ContactData;

    public upcomingEvents$: Observable<ContactInteractionData[]>;
    public pastEvents$: Observable<ContactInteractionData[]>;
    public isLoading$: Observable<boolean>;
    public hasEvents$: Observable<boolean>;

    constructor() {
        this.upcomingEvents$ = of([]);
        this.pastEvents$ = of([]);
        this.isLoading$ = of(true);
        this.hasEvents$ = of(false);
    }

    ngOnInit(): void {
        if (!this.contact) {
            this.isLoading$ = of(false);
            return;
        }

        const interactions$ = this.contact.ContactInteractions$;

        this.upcomingEvents$ = interactions$.pipe(
            map(interactions => {
                if (!interactions) return [];
                const now = new Date().toISOString();
                return interactions
                    .filter(i => i.scheduledEventId != null && i.startTime > now)
                    .sort((a, b) => a.startTime.localeCompare(b.startTime));
            })
        );

        this.pastEvents$ = interactions$.pipe(
            map(interactions => {
                if (!interactions) return [];
                const now = new Date().toISOString();
                return interactions
                    .filter(i => i.scheduledEventId != null && i.startTime <= now)
                    .sort((a, b) => b.startTime.localeCompare(a.startTime)); // Newest first
            })
        );

        this.isLoading$ = interactions$.pipe(map(x => x === null));

        this.hasEvents$ = combineLatest([this.upcomingEvents$, this.pastEvents$]).pipe(
            map(([upcoming, past]) => upcoming.length > 0 || past.length > 0)
        );
    }

    ngOnDestroy(): void {
    }
}
