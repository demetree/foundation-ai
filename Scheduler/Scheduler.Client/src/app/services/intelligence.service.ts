
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';

//
// Intelligence Context - Defines what we are analyzing
//
export interface IntelligenceContext {
    entityType: string;
    entityId: number | string;
    correlationId?: string;
}

//
// Dossier Data Structures
//
export interface IntelligenceDossier {
    digitalFootprint?: DigitalFootprint;
    professionalSummary?: string;
    engagementHooks?: string[];
    generatedAt: Date;
    provider: string;
}

export interface DigitalFootprint {
    linkedInUrl?: string;
    twitterHandle?: string;
    websiteUrl?: string;
    publicMentions: number;
}

//
// RAG Provider Interface - Strategy Pattern
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

    private _activeProvider: IRagProvider;

    constructor() {
        // Default to Mock provider for now
        this._activeProvider = new MockRagProvider();
    }

    public getIntelligence(context: IntelligenceContext): Observable<IntelligenceDossier> {
        return this._activeProvider.getIntelligence(context);
    }
}

//
// Mock Provider Implementation
//
export class MockRagProvider implements IRagProvider {

    public getIntelligence(context: IntelligenceContext): Observable<IntelligenceDossier> {

        //
        // Simulate network latency (between 1.5s and 3s)
        //
        const latency = Math.floor(Math.random() * 1500) + 1500;

        const dossier: IntelligenceDossier = {
            generatedAt: new Date(),
            provider: 'Mock RAG Engine v1.0',
            professionalSummary: 'Senior Project Manager with 15+ years in commercial construction. distinct focus on sustainable building practices and LEAN methodologies. Recently mentioned in "Construction Weekly" regarding the downtown metro expansion.',
            digitalFootprint: {
                linkedInUrl: 'https://linkedin.com/in/example',
                twitterHandle: '@construction_guru',
                publicMentions: 12
            },
            engagementHooks: [
                'Mention the recent "Downtown Metro" article.',
                'Ask about their experience with the new LEED v4.1 standards.',
                'Connect on their shared interest in modular pre-fabrication.'
            ]
        };

        return of(dossier).pipe(delay(latency));
    }
}
