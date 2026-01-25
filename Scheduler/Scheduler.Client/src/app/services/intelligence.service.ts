
import { Injectable } from '@angular/core';
import { Observable, Subject, of } from 'rxjs';
import { delay, switchMap, map, catchError, shareReplay, tap } from 'rxjs/operators';
import { RagProviderResolver } from './resolvers/rag-provider.resolver';

//
// 1. Trust-First Interfaces
//

export interface Citation {
    sourceName: string;
    sourceUrl: string;
    quotedSnippet?: string;
}

export interface IntelligenceField<T> {
    value: T;
    confidence: number;
    citations: Citation[];
    isVerified: boolean;
}

export type IntentType = 'prospecting' | 'renewal' | 'risk' | 'upsell';

export interface IntelligenceContext {
    entityType: string;
    entityId: number | string;
    correlationId?: string;
    intent: IntentType;
    groundingRequired: boolean;
}

export type DossierStatus = 'complete' | 'partial' | 'failed' | 'unverified';

export interface IntelligenceDossier {
    status: DossierStatus;
    generatedAt: Date;
    provider: string;

    professionalSummary?: IntelligenceField<string>;
    digitalFootprint?: IntelligenceField<DigitalFootprint>;
    engagementHooks?: IntelligenceField<string[]>;
}

export interface DigitalFootprint {
    linkedInUrl?: string;
    twitterHandle?: string;
    websiteUrl?: string;
    publicMentions: number;
}

//
// RAG Provider Interface
//
export interface IRagProvider {
    getIntelligence(context: IntelligenceContext): Observable<IntelligenceDossier>;
}

//
// Service
//
@Injectable({
    providedIn: 'root'
})
export class IntelligenceService {

    private _requestSubject = new Subject<IntelligenceContext>();

    //
    // Automatic Cancellation & Processing Stream
    //
    public dossier$: Observable<IntelligenceDossier> = this._requestSubject.pipe(
        switchMap(context => {
            const provider = this.providerResolver.resolve(context);

            return provider.getIntelligence(context).pipe(
                map(dossier => this.validateDossier(dossier, context)),
                catchError(err => of(this.createFailedDossier(err, context)))
            );
        }),
        shareReplay(1)
    );

    constructor(
        private providerResolver: RagProviderResolver
    ) { }

    /**
     * Triggers a new intelligence analysis.
     * Previous requests are automatically cancelled via switchMap.
     */
    public getIntelligence(context: IntelligenceContext): void {
        this._requestSubject.next(context);
    }

    //
    // Anti-Hallucination Validation Logic
    //
    private validateDossier(dossier: IntelligenceDossier, context: IntelligenceContext): IntelligenceDossier {

        if (!context.groundingRequired) {
            return dossier;
        }

        let isValid = true;

        // specific validation for Professional Summary
        if (dossier.professionalSummary) {
            if (!dossier.professionalSummary.citations || dossier.professionalSummary.citations.length === 0) {
                dossier.professionalSummary.isVerified = false;
                isValid = false;
            }
        }

        // If grounding was required but failed verification, downgrade status
        if (!isValid) {
            dossier.status = 'partial';
            // In strict mode, we might even set it to 'unverified' or throw
            if (context.groundingRequired && !isValid) {
                dossier.status = 'unverified';
            }
        }

        return dossier;
    }

    private createFailedDossier(error: any, context: IntelligenceContext): IntelligenceDossier {
        console.error('Intelligence gathering failed', error);
        return {
            status: 'failed',
            generatedAt: new Date(),
            provider: 'System Error Handler',
            professionalSummary: {
                value: 'Unable to generate intelligence due to an error.',
                confidence: 0,
                citations: [],
                isVerified: false
            }
        };
    }
}
