//
// Manual Editor Service
//
// Wraps the existing Foundation CRUD APIs for BuildManual, BuildManualPage,
// BuildManualStep, BuildStepAnnotation, and BuildStepPart entities.
//
// No new server endpoints are needed — the schema and APIs already exist.
//
// AI-assisted development — reviewed and adapted to project conventions.
//

import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';


// ─── DTOs matching server-side entities ────────────────────────────────

export interface BuildManualDto {
    id: number;
    projectId: number;
    name: string;
    description: string;
    pageWidthMm: number | null;
    pageHeightMm: number | null;
    isPublished: boolean;
    versionNumber: number;
    objectGuid: string;
    active: boolean;
    deleted: boolean;
    project?: any;
}

export interface BuildManualPageDto {
    id: number;
    buildManualId: number;
    pageNum: number | null;
    title: string;
    notes: string;
    backgroundTheme: string | null;
    layoutPreset: string | null;
    backgroundColorHex: string | null;
    objectGuid: string;
    active: boolean;
    deleted: boolean;
}

export interface BuildManualStepDto {
    id: number;
    buildManualPageId: number;
    stepNumber: number | null;
    cameraPositionX: number | null;
    cameraPositionY: number | null;
    cameraPositionZ: number | null;
    cameraTargetX: number | null;
    cameraTargetY: number | null;
    cameraTargetZ: number | null;
    cameraZoom: number | null;
    showExplodedView: boolean;
    explodedDistance: number | null;
    renderImagePath: string | null;
    pliImagePath: string | null;
    fadeStepEnabled: boolean;
    isCallout: boolean;
    calloutModelName: string | null;
    showPartsListImage: boolean;
    objectGuid: string;
    active: boolean;
    deleted: boolean;
}

export interface BuildStepAnnotationDto {
    id: number;
    buildManualStepId: number;
    buildStepAnnotationTypeId: number;
    positionX: number | null;
    positionY: number | null;
    width: number | null;
    height: number | null;
    text: string;
    placedBrickId: number | null;
    objectGuid: string;
    active: boolean;
    deleted: boolean;
}

export interface BuildStepAnnotationTypeDto {
    id: number;
    name: string;
    description: string;
    sequence: number | null;
    objectGuid: string;
    active: boolean;
    deleted: boolean;
}


@Injectable({
    providedIn: 'root'
})
export class ManualEditorService {

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }


    private get headers(): HttpHeaders {
        return new HttpHeaders({
            'Authorization': `Bearer ${this.authService.accessToken}`,
            'Content-Type': 'application/json'
        });
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Build Manuals
    // ═══════════════════════════════════════════════════════════════════

    getManuals(projectId?: number): Observable<BuildManualDto[]> {
        let url = '/api/BuildManuals?deleted=false';
        if (projectId != null) {
            url += `&projectId=${projectId}`;
        }
        return this.http.get<BuildManualDto[]>(url, { headers: this.headers });
    }

    getManual(id: number): Observable<BuildManualDto> {
        return this.http.get<BuildManualDto>(`/api/BuildManual/${id}`, { headers: this.headers });
    }

    createManual(dto: Partial<BuildManualDto>): Observable<BuildManualDto> {
        return this.http.post<BuildManualDto>('/api/BuildManual', dto, { headers: this.headers });
    }

    updateManual(id: number, dto: BuildManualDto): Observable<BuildManualDto> {
        return this.http.put<BuildManualDto>(`/api/BuildManual/${id}`, dto, { headers: this.headers });
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Build Manual Pages
    // ═══════════════════════════════════════════════════════════════════

    getPages(buildManualId: number): Observable<BuildManualPageDto[]> {
        return this.http.get<BuildManualPageDto[]>(
            `/api/BuildManualPages?buildManualId=${buildManualId}&deleted=false`,
            { headers: this.headers }
        );
    }

    getPage(id: number): Observable<BuildManualPageDto> {
        return this.http.get<BuildManualPageDto>(`/api/BuildManualPage/${id}`, { headers: this.headers });
    }

    createPage(dto: Partial<BuildManualPageDto>): Observable<BuildManualPageDto> {
        return this.http.post<BuildManualPageDto>('/api/BuildManualPage', dto, { headers: this.headers });
    }

    updatePage(id: number, dto: BuildManualPageDto): Observable<BuildManualPageDto> {
        return this.http.put<BuildManualPageDto>(`/api/BuildManualPage/${id}`, dto, { headers: this.headers });
    }

    deletePage(id: number): Observable<any> {
        return this.http.delete(`/api/BuildManualPage/${id}`, { headers: this.headers });
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Build Manual Steps
    // ═══════════════════════════════════════════════════════════════════

    getSteps(buildManualPageId: number): Observable<BuildManualStepDto[]> {
        return this.http.get<BuildManualStepDto[]>(
            `/api/BuildManualSteps?buildManualPageId=${buildManualPageId}&deleted=false`,
            { headers: this.headers }
        );
    }

    /**
     * Batch load ALL steps for an entire manual in a single request.
     * Returns steps across all pages, each with buildManualPageId for grouping.
     */
    getAllSteps(manualId: number): Observable<BuildManualStepDto[]> {
        return this.http.get<BuildManualStepDto[]>(
            `/api/manual-generator/manual/${manualId}/all-steps`,
            { headers: this.headers }
        );
    }

    getStep(id: number): Observable<BuildManualStepDto> {
        return this.http.get<BuildManualStepDto>(`/api/BuildManualStep/${id}`, { headers: this.headers });
    }

    createStep(dto: Partial<BuildManualStepDto>): Observable<BuildManualStepDto> {
        return this.http.post<BuildManualStepDto>('/api/BuildManualStep', dto, { headers: this.headers });
    }

    updateStep(id: number, dto: BuildManualStepDto): Observable<BuildManualStepDto> {
        return this.http.put<BuildManualStepDto>(`/api/BuildManualStep/${id}`, dto, { headers: this.headers });
    }

    deleteStep(id: number): Observable<any> {
        return this.http.delete(`/api/BuildManualStep/${id}`, { headers: this.headers });
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Export
    // ═══════════════════════════════════════════════════════════════════

    /**
     * Export a manual as HTML or PDF.
     * Returns a download URL that can be opened in a new window.
     */
    exportManual(manualId: number, format: 'html' | 'pdf'): Observable<{ downloadUrl: string; format: string; totalSteps: number; totalParts: number }> {
        return this.http.post<{ downloadUrl: string; format: string; totalSteps: number; totalParts: number }>(
            `/api/manual-generator/manual/${manualId}/export?format=${format}`,
            {},
            { headers: this.headers }
        );
    }

    /**
     * Download a file with auth headers, returning the blob.
     */
    downloadFile(url: string): Observable<Blob> {
        return this.http.get(url, {
            headers: new HttpHeaders({
                'Authorization': `Bearer ${this.authService.accessToken}`
            }),
            responseType: 'blob'
        });
    }

    /**
     * Reorder steps within a page.
     * Supports optional targetPageId for cross-page moves.
     */
    reorderSteps(pageId: number, stepIds: number[], targetPageId?: number): Observable<void> {
        return this.http.put<void>(
            `/api/manual-generator/page/${pageId}/reorder-steps`,
            { stepIds, targetPageId },
            { headers: this.headers }
        );
    }

    /**
     * Re-render a step with its current camera coordinates.
     * Returns the new base64 image.
     */
    reRenderStep(stepId: number): Observable<{ renderImagePath: string }> {
        return this.http.post<{ renderImagePath: string }>(
            `/api/manual-generator/step/${stepId}/re-render`,
            {},
            { headers: this.headers }
        );
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Build Step Annotations
    // ═══════════════════════════════════════════════════════════════════

    getAnnotations(buildManualStepId: number): Observable<BuildStepAnnotationDto[]> {
        return this.http.get<BuildStepAnnotationDto[]>(
            `/api/BuildStepAnnotations?buildManualStepId=${buildManualStepId}&deleted=false`,
            { headers: this.headers }
        );
    }

    createAnnotation(dto: Partial<BuildStepAnnotationDto>): Observable<BuildStepAnnotationDto> {
        return this.http.post<BuildStepAnnotationDto>('/api/BuildStepAnnotation', dto, { headers: this.headers });
    }

    updateAnnotation(id: number, dto: BuildStepAnnotationDto): Observable<BuildStepAnnotationDto> {
        return this.http.put<BuildStepAnnotationDto>(`/api/BuildStepAnnotation/${id}`, dto, { headers: this.headers });
    }

    deleteAnnotation(id: number): Observable<any> {
        return this.http.delete(`/api/BuildStepAnnotation/${id}`, { headers: this.headers });
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Annotation Types (lookup)
    // ═══════════════════════════════════════════════════════════════════

    getAnnotationTypes(): Observable<BuildStepAnnotationTypeDto[]> {
        return this.http.get<BuildStepAnnotationTypeDto[]>(
            '/api/BuildStepAnnotationTypes?deleted=false',
            { headers: this.headers }
        );
    }
}
