
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { IRagProvider, IntelligenceContext, IntelligenceDossier, IntelligenceField } from '../intelligence.service';

@Injectable({
    providedIn: 'root'
})
export class GeminiGroundingProvider implements IRagProvider {

    public getIntelligence(context: IntelligenceContext): Observable<IntelligenceDossier> {

        //
        // SIMULATION: Gemini API + Google Search Grounding
        //
        const latency = 2000;

        // Simulate a scenario where we might fail grounding if required
        const successfulGrounding = Math.random() > 0.3; // 70% chance of success

        if (context.groundingRequired && !successfulGrounding) {
            // Return a result that will trigger the Service's validation failure
            return of({
                status: 'unverified',
                generatedAt: new Date(),
                provider: 'Gemini Pro + Google Search',
                professionalSummary: {
                    value: 'Based on general knowledge, this contact appears to be involved in the tech sector, but no specific current employment could be verified against trusted sources.',
                    confidence: 0.4,
                    citations: [], // EMPTY CITATIONS -> Will trigger 'unverified' in service
                    isVerified: false
                }
            } as IntelligenceDossier).pipe(delay(latency));
        }

        //
        // Success Case
        //
        const dossier: IntelligenceDossier = {
            status: 'complete',
            generatedAt: new Date(),
            provider: 'Gemini Pro + Google Search',
            professionalSummary: {
                value: 'Current VP of Operations at Kengin. Recently spoke at angular-enterprise-conf regarding aggressive caching strategies.',
                confidence: 0.95,
                citations: [
                    {
                        sourceName: 'Conference Agenda',
                        sourceUrl: 'https://angular-enterprise.com/speakers',
                        quotedSnippet: 'Jacob Kennedy: Caching Strategies at Scale'
                    },
                    {
                        sourceName: 'Abstract',
                        sourceUrl: 'https://kengin.com/blog',
                        quotedSnippet: 'VP of Operations'
                    }
                ],
                isVerified: true
            },
            digitalFootprint: {
                value: {
                    publicMentions: 145,
                    linkedInUrl: 'https://linkedin.com/in/jacobkennedy',
                    twitterHandle: '@jacobkennedy'
                },
                confidence: 0.9,
                citations: [{ sourceName: 'LinkedIn', sourceUrl: 'https://linkedin.com', quotedSnippet: 'Profile found' }],
                isVerified: true
            },
            engagementHooks: {
                value: [
                    'Discuss the nuances of Service Workers in PWA.',
                    'Ask about the Kengin roadmap for Q4.'
                ],
                confidence: 0.8,
                citations: [],
                isVerified: false
            }
        };

        return of(dossier).pipe(delay(latency));
    }

}
