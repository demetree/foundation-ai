
import { Injectable } from '@angular/core';
import { IntelligenceContext, IRagProvider, IntelligenceDossier, IntelligenceField } from '../intelligence.service';
import { GeminiGroundingProvider } from '../providers/gemini-grounding.provider';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';

@Injectable({
    providedIn: 'root'
})
export class RagProviderResolver {

    constructor(
        private geminiProvider: GeminiGroundingProvider
    ) { }

    public resolve(context: IntelligenceContext): IRagProvider {

        //
        // Strategic Routing Logic
        //
        if (context.entityType === 'Account') {
            // Enterprise Data -> Vertex AI (Simulated/Mock for now)
            return new MockVertexAIProvider();
        }

        if (context.entityType === 'Contact' || context.entityType === 'Lead') {
            // Persons -> Web Search / Public Data -> Gemini
            return this.geminiProvider;
        }

        // Default / Fallback
        return new MockRagProvider();
    }
}

//
// Mock Provider Implementation (Preserved/Moved)
//
export class MockRagProvider implements IRagProvider {

    public getIntelligence(context: IntelligenceContext): Observable<IntelligenceDossier> {
        const latency = Math.floor(Math.random() * 1500) + 1500;

        const dossier: IntelligenceDossier = {
            status: 'complete',
            generatedAt: new Date(),
            provider: 'Mock RAG Engine v1.0',
            professionalSummary: {
                value: 'Senior Project Manager with 15+ years in commercial construction. Distinct focus on sustainable building practices.',
                confidence: 0.85,
                citations: [{ sourceName: 'LinkedIn', sourceUrl: 'https://linkedin.com/in/mock', quotedSnippet: '15+ years experience' }],
                isVerified: true
            },
            digitalFootprint: {
                value: {
                    linkedInUrl: 'https://linkedin.com/in/example',
                    twitterHandle: '@construction_guru',
                    publicMentions: 12,
                    websiteUrl: 'https://example.com'
                },
                confidence: 0.9,
                citations: [],
                isVerified: true
            },
            engagementHooks: {
                value: [
                    'Mention the recent "Downtown Metro" article.',
                    'Ask about LEED v4.1 standards.'
                ],
                confidence: 0.7,
                citations: [],
                isVerified: false
            }
        };

        return of(dossier).pipe(delay(latency));
    }
}

//
// Mock Vertex (Internal Docs)
//
export class MockVertexAIProvider implements IRagProvider {
    public getIntelligence(context: IntelligenceContext): Observable<IntelligenceDossier> {
        const dossier: IntelligenceDossier = {
            status: 'complete',
            generatedAt: new Date(),
            provider: 'Google Vertex AI (Internal)',
            professionalSummary: {
                value: 'Strategic Account: High value renewal target. 3 active support tickets in last quarter.',
                confidence: 0.99,
                citations: [{ sourceName: 'Salesforce', sourceUrl: 'internal://crm/123', quotedSnippet: 'Renewal Probability: High' }],
                isVerified: true
            }
        };
        return of(dossier).pipe(delay(1000));
    }
}
