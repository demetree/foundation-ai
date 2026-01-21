//
// Module Tokens Tab Component
//
// Displays EntityDataToken entries for the module.
//

import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';

import { ModuleData } from '../../../security-data-services/module.service';
import { EntityDataTokenData } from '../../../security-data-services/entity-data-token.service';

@Component({
    selector: 'app-module-tokens-tab',
    templateUrl: './module-tokens-tab.component.html',
    styleUrls: ['./module-tokens-tab.component.scss']
})
export class ModuleTokensTabComponent implements OnInit, OnDestroy {

    @Input() module: ModuleData | null = null;

    private destroy$ = new Subject<void>();

    public tokens: EntityDataTokenData[] = [];
    public isLoading: boolean = true;
    public errorMessage: string | null = null;


    ngOnInit(): void {
        this.loadTokens();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    private loadTokens(): void {
        if (!this.module) {
            this.isLoading = false;
            return;
        }

        this.isLoading = true;
        this.errorMessage = null;

        this.module.EntityDataTokens
            .then(tokens => {
                this.tokens = tokens;
                this.isLoading = false;
            })
            .catch(err => {
                console.error('Error loading entity data tokens:', err);
                this.errorMessage = 'Failed to load entity tokens.';
                this.isLoading = false;
            });
    }


    getStatusBadgeClass(token: EntityDataTokenData): string {
        if (token.deleted) return 'badge-deleted';
        return token.active ? 'badge-active' : 'badge-inactive';
    }


    getStatusText(token: EntityDataTokenData): string {
        if (token.deleted) return 'Deleted';
        return token.active ? 'Active' : 'Inactive';
    }


    trackByTokenId(index: number, token: EntityDataTokenData): number | bigint {
        return token.id;
    }
}
