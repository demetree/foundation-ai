import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

//
// Project Service
//
// Custom Angular service for MOC project management.
// Calls the custom MocImportController endpoints and auto-generated Project CRUD.
//


// ───────────────────────────── DTOs ─────────────────────────────

export interface ProjectSummary {
    id: number;
    name: string;
    description: string;
    notes: string;
    thumbnailImagePath: string | null;
    partCount: number | null;
    lastBuildDate: string | null;
    versionNumber: number;
    userId: number | null;
}

export interface UploadResult {
    projectId: number;
    projectName: string;
    totalPartCount: number;
    submodelCount: number;
    stepCount: number;
    resolvedPartCount: number;
    unresolvedPartCount: number;
    unresolvedParts: string[];
    sourceFormat: string;
}

export interface ImportFormat {
    extension: string;
    name: string;
    description: string;
    mimeType: string;
}

export interface ProjectViewerSummary {
    projectId: number;
    name: string;
    description: string;
    partCount: number | null;
    stepCount: number;
    submodelCount: number;
    sourceFormat: string | null;
    studioVersion: string | null;
    hasThumbnail: boolean;
}


// ───────────────────────────── Service ─────────────────────────────

@Injectable({
    providedIn: 'root'
})
export class ProjectService {
    private readonly projectUrl = '/api/Projects';
    private readonly importUrl = '/api/moc/import';
    private readonly mocUrl = '/api/moc';

    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }

    private get headers() {
        return this.authService.GetAuthenticationHeaders();
    }


    /** GET /api/Project — all projects for the current tenant */
    getMyProjects(): Observable<ProjectSummary[]> {
        return this.http.get<ProjectSummary[]>(
            this.projectUrl,
            { headers: this.headers }
        );
    }


    /** POST /api/moc/import/upload — upload a model file (.ldr, .mpd, .io) */
    uploadModel(file: File): Observable<HttpEvent<UploadResult>> {
        const formData = new FormData();
        formData.append('file', file, file.name);

        //
        // IMPORTANT: For multipart/form-data uploads, the browser must set Content-Type
        // automatically (with the boundary parameter). The default auth headers include
        // 'Content-Type: application/json' which would override this and cause the server
        // to not find the file parameter. So we delete Content-Type here.
        //
        const uploadHeaders = this.headers.delete('Content-Type');

        const req = new HttpRequest('POST', `${this.importUrl}/upload`, formData, {
            headers: uploadHeaders,
            reportProgress: true
        });

        return this.http.request<UploadResult>(req);
    }

    /** Get the upload endpoint URL for direct fetch() calls */
    getUploadUrl(): string {
        return `${this.importUrl}/upload`;
    }

    /** Get auth headers as a plain Record for use with fetch() */
    getAuthHeadersRecord(): Record<string, string> {
        const httpHeaders = this.headers;
        const result: Record<string, string> = {};
        for (const key of httpHeaders.keys()) {
            if (key.toLowerCase() !== 'content-type') {
                result[key] = httpHeaders.get(key)!;
            }
        }
        return result;
    }


    /** GET /api/moc/import/formats — supported import formats */
    getSupportedFormats(): Observable<ImportFormat[]> {
        return this.http.get<ImportFormat[]>(
            `${this.importUrl}/formats`,
            { headers: this.headers }
        );
    }


    /** DELETE /api/Project/{id} — soft-delete a project */
    deleteProject(id: number): Observable<any> {
        return this.http.delete(
            `/api/Project/${id}`,
            { headers: this.headers }
        );
    }


    // ───────────────────── Viewer & Export ─────────────────────

    /** GET /api/moc/project/{id}/summary — project metadata for the viewer */
    getProjectSummary(projectId: number): Observable<ProjectViewerSummary> {
        return this.http.get<ProjectViewerSummary>(
            `${this.mocUrl}/project/${projectId}/summary`,
            { headers: this.headers }
        );
    }


    /** GET /api/moc/project/{id}/viewer-mpd — self-contained MPD text for 3D viewer */
    getViewerMpd(projectId: number): Observable<string> {
        return this.http.get(
            `${this.mocUrl}/project/${projectId}/viewer-mpd`,
            { headers: this.headers, responseType: 'text' }
        );
    }


    /** Get the download URL for exporting a project in a given format */
    getExportUrl(projectId: number, format: 'ldr' | 'mpd' | 'io'): string {
        return `${this.mocUrl}/export/${projectId}/${format}`;
    }


    /** Get the download URL for exporting a project as STL */
    getStlExportUrl(projectId: number, stlFormat: 'binary' | 'ascii' = 'binary'): string {
        return `${this.mocUrl}/export/${projectId}/stl?format=${stlFormat}`;
    }


    /** Get the thumbnail URL for a project */
    getThumbnailUrl(projectId: number): string {
        return `${this.mocUrl}/project/${projectId}/thumbnail`;
    }
}
