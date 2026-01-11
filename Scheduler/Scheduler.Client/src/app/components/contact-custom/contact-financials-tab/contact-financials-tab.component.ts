import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { ContactData } from '../../../scheduler-data-services/contact.service';
import { ConstituentData } from '../../../scheduler-data-services/constituent.service';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'app-contact-financials-tab',
    templateUrl: './contact-financials-tab.component.html',
    styleUrls: ['./contact-financials-tab.component.scss']
})
export class ContactFinancialsTabComponent implements OnInit, OnDestroy {

    @Input() contact!: ContactData;

    public constituents$: Observable<ConstituentData[] | null>;
    public isLoading$: Observable<boolean>;
    public activeConstituent: ConstituentData | null = null;

    constructor(private authService: AuthService) {
        this.constituents$ = of(null);
        this.isLoading$ = of(true);
    }

    ngOnInit(): void {
        if (!this.contact) {
            this.constituents$ = of([]);
            this.isLoading$ = of(false);
            return;
        }

        this.constituents$ = this.contact.Constituents$;

        this.isLoading$ = this.constituents$.pipe(
            map(data => data === null)
        );

        // Auto-select the first constituent for display simplicity
        this.constituents$.subscribe(list => {
            if (list && list.length > 0) {
                this.activeConstituent = list[0];
            }
        });
    }

    ngOnDestroy(): void {
    }
}
