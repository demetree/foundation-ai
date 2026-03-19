//
// file-manager.service.ts
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Angular service for the Document Manager / File Manager feature.
// Provides typed methods for all FileManagerController endpoints.
//
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AlertService } from './alert.service';
import { AuthService } from './auth.service';
import { SecureEndpointBase } from './secure-endpoint-base.service';


// ─── Interfaces ──────────────────────────────────────────────────────

export interface FolderDTO {
    id: number;
    name: string;
    description?: string;
    parentDocumentFolderId?: number | null;
    iconId?: number | null;
    color?: string;
    sequence: number;
    notes?: string;
    versionNumber: number;
    objectGuid: string;
    active?: boolean;
    deleted?: boolean;

    // Output DTO nav properties
    icon?: any;
    parentDocumentFolder?: any;

    // Client-side hierarchy helpers (computed, not from server)
    children?: FolderDTO[];
    level?: number;
    expanded?: boolean;
}

export interface DocumentDTO {
    id: number;
    documentTypeId?: number | null;
    documentFolderId?: number | null;
    name: string;
    description?: string;
    fileName: string;
    mimeType: string;
    fileSizeBytes: number;
    fileDataFileName?: string;
    fileDataSize?: number;
    fileDataData?: any;
    fileDataMimeType?: string;
    invoiceId?: number | null;
    receiptId?: number | null;
    scheduledEventId?: number | null;
    financialTransactionId?: number | null;
    contactId?: number | null;
    resourceId?: number | null;
    clientId?: number | null;
    officeId?: number | null;
    crewId?: number | null;
    schedulingTargetId?: number | null;
    paymentTransactionId?: number | null;
    financialOfficeId?: number | null;
    tenantProfileId?: number | null;
    campaignId?: number | null;
    householdId?: number | null;
    constituentId?: number | null;
    tributeId?: number | null;
    volunteerProfileId?: number | null;
    status?: string;
    statusDate?: string;
    statusChangedBy?: string;
    uploadedDate: string;
    uploadedBy?: string;
    notes?: string;
    versionNumber: number;
    objectGuid: string;
    active?: boolean;
    deleted?: boolean;

    // Output DTO nav properties
    documentType?: any;
    documentFolder?: any;
}

export interface DocumentTagDTO {
    id: number;
    name: string;
    description?: string;
    color?: string;
    sequence: number;
    notes?: string;
    versionNumber: number;
    objectGuid: string;
    active?: boolean;
    deleted?: boolean;
}

export interface UploadOptions {
    folderId?: number | null;
    documentTypeId?: number | null;
    scheduledEventId?: number | null;
    contactId?: number | null;
    clientId?: number | null;
    resourceId?: number | null;
    crewId?: number | null;
    schedulingTargetId?: number | null;
    financialTransactionId?: number | null;
    officeId?: number | null;
    campaignId?: number | null;
    householdId?: number | null;
    constituentId?: number | null;
    tributeId?: number | null;
    volunteerProfileId?: number | null;
}


@Injectable({ providedIn: 'root' })
export class FileManagerService extends SecureEndpointBase {

    private readonly base = '/api/FileManager';

    constructor(
        http: HttpClient,
        alertService: AlertService,
        authService: AuthService
    ) {
        super(http, alertService, authService);
    }

    private authHeaders(): HttpHeaders {
        return new HttpHeaders({
            'Authorization': 'Bearer ' + this.authService.accessToken
        });
    }

    private jsonHeaders(): HttpHeaders {
        return new HttpHeaders({
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + this.authService.accessToken
        });
    }


    // ─── Folders ──────────────────────────────────────────────────────

    getFolders(): Observable<FolderDTO[]> {
        return this.http.get<FolderDTO[]>(`${this.base}/Folders`, { headers: this.authHeaders() }).pipe(
            catchError((error: any) => this.handleError(error, () => this.getFolders()))
        );
    }

    getFolder(folderId: number): Observable<FolderDTO> {
        return this.http.get<FolderDTO>(`${this.base}/Folders/${folderId}`, { headers: this.authHeaders() }).pipe(
            catchError((error: any) => this.handleError(error, () => this.getFolder(folderId)))
        );
    }

    createFolder(folder: Partial<FolderDTO>): Observable<FolderDTO> {
        return this.http.post<FolderDTO>(`${this.base}/Folders`, folder, { headers: this.jsonHeaders() }).pipe(
            catchError((error: any) => this.handleError(error, () => this.createFolder(folder)))
        );
    }

    updateFolder(folder: Partial<FolderDTO>): Observable<FolderDTO> {
        return this.http.put<FolderDTO>(`${this.base}/Folders`, folder, { headers: this.jsonHeaders() }).pipe(
            catchError((error: any) => this.handleError(error, () => this.updateFolder(folder)))
        );
    }

    deleteFolder(folderId: number, cascade = false): Observable<any> {
        let params = new HttpParams();
        if (cascade) {
            params = params.set('cascade', 'true');
        }
        return this.http.delete(`${this.base}/Folders/${folderId}`, { headers: this.authHeaders(), params }).pipe(
            catchError((error: any) => this.handleError(error, () => this.deleteFolder(folderId, cascade)))
        );
    }


    // ─── Documents ───────────────────────────────────────────────────

    getDocumentsInFolder(folderId: number | null): Observable<DocumentDTO[]> {
        let params = new HttpParams();
        if (folderId != null) {
            params = params.set('folderId', folderId.toString());
        }
        return this.http.get<DocumentDTO[]>(`${this.base}/Documents`, { headers: this.authHeaders(), params }).pipe(
            catchError((error: any) => this.handleError(error, () => this.getDocumentsInFolder(folderId)))
        );
    }

    getDocument(documentId: number): Observable<DocumentDTO> {
        return this.http.get<DocumentDTO>(`${this.base}/Documents/${documentId}`, { headers: this.authHeaders() }).pipe(
            catchError((error: any) => this.handleError(error, () => this.getDocument(documentId)))
        );
    }

    uploadDocuments(files: File[], options: UploadOptions = {}): Observable<DocumentDTO[]> {
        const formData = new FormData();
        for (const file of files) {
            formData.append('files', file, file.name);
        }

        let params = new HttpParams();
        if (options.folderId != null) params = params.set('folderId', options.folderId.toString());
        if (options.documentTypeId != null) params = params.set('documentTypeId', options.documentTypeId.toString());
        if (options.scheduledEventId != null) params = params.set('scheduledEventId', options.scheduledEventId.toString());
        if (options.contactId != null) params = params.set('contactId', options.contactId.toString());
        if (options.clientId != null) params = params.set('clientId', options.clientId.toString());
        if (options.resourceId != null) params = params.set('resourceId', options.resourceId.toString());
        if (options.crewId != null) params = params.set('crewId', options.crewId.toString());
        if (options.schedulingTargetId != null) params = params.set('schedulingTargetId', options.schedulingTargetId.toString());
        if (options.financialTransactionId != null) params = params.set('financialTransactionId', options.financialTransactionId.toString());
        if (options.officeId != null) params = params.set('officeId', options.officeId.toString());
        if (options.campaignId != null) params = params.set('campaignId', options.campaignId.toString());
        if (options.householdId != null) params = params.set('householdId', options.householdId.toString());
        if (options.constituentId != null) params = params.set('constituentId', options.constituentId.toString());
        if (options.tributeId != null) params = params.set('tributeId', options.tributeId.toString());
        if (options.volunteerProfileId != null) params = params.set('volunteerProfileId', options.volunteerProfileId.toString());

        const headers = new HttpHeaders({
            'Authorization': 'Bearer ' + this.authService.accessToken
            // Note: Do NOT set Content-Type for multipart — browser sets boundary automatically
        });

        return this.http.post<DocumentDTO[]>(`${this.base}/Documents/Upload`, formData, { headers, params }).pipe(
            catchError((error: any) => this.handleError(error, () => this.uploadDocuments(files, options)))
        );
    }

    downloadDocumentUrl(documentId: number): string {
        return `${this.base}/Documents/${documentId}/Download`;
    }

    downloadDocument(documentId: number): Observable<Blob> {
        return this.http.get(`${this.base}/Documents/${documentId}/Download`, {
            headers: this.authHeaders(),
            responseType: 'blob'
        }).pipe(
            catchError((error: any) => this.handleError(error, () => this.downloadDocument(documentId)))
        );
    }

    updateDocumentMetadata(document: Partial<DocumentDTO>): Observable<DocumentDTO> {
        return this.http.put<DocumentDTO>(`${this.base}/Documents`, document, { headers: this.jsonHeaders() }).pipe(
            catchError((error: any) => this.handleError(error, () => this.updateDocumentMetadata(document)))
        );
    }

    moveDocument(documentId: number, targetFolderId: number | null): Observable<any> {
        let params = new HttpParams();
        if (targetFolderId != null) {
            params = params.set('targetFolderId', targetFolderId.toString());
        }
        return this.http.put(`${this.base}/Documents/${documentId}/Move`, null, { headers: this.authHeaders(), params }).pipe(
            catchError((error: any) => this.handleError(error, () => this.moveDocument(documentId, targetFolderId)))
        );
    }

    deleteDocument(documentId: number): Observable<any> {
        return this.http.delete(`${this.base}/Documents/${documentId}`, { headers: this.authHeaders() }).pipe(
            catchError((error: any) => this.handleError(error, () => this.deleteDocument(documentId)))
        );
    }

    searchDocuments(query: string): Observable<DocumentDTO[]> {
        const params = new HttpParams().set('q', query);
        return this.http.get<DocumentDTO[]>(`${this.base}/Documents/Search`, { headers: this.authHeaders(), params }).pipe(
            catchError((error: any) => this.handleError(error, () => this.searchDocuments(query)))
        );
    }


    // ─── Tags ────────────────────────────────────────────────────────

    getTags(): Observable<DocumentTagDTO[]> {
        return this.http.get<DocumentTagDTO[]>(`${this.base}/Tags`, { headers: this.authHeaders() }).pipe(
            catchError((error: any) => this.handleError(error, () => this.getTags()))
        );
    }

    createTag(tag: Partial<DocumentTagDTO>): Observable<DocumentTagDTO> {
        return this.http.post<DocumentTagDTO>(`${this.base}/Tags`, tag, { headers: this.jsonHeaders() }).pipe(
            catchError((error: any) => this.handleError(error, () => this.createTag(tag)))
        );
    }

    addTagToDocument(documentId: number, tagId: number): Observable<any> {
        return this.http.post(`${this.base}/Documents/${documentId}/Tags/${tagId}`, null, { headers: this.authHeaders() }).pipe(
            catchError((error: any) => this.handleError(error, () => this.addTagToDocument(documentId, tagId)))
        );
    }

    removeTagFromDocument(documentId: number, tagId: number): Observable<any> {
        return this.http.delete(`${this.base}/Documents/${documentId}/Tags/${tagId}`, { headers: this.authHeaders() }).pipe(
            catchError((error: any) => this.handleError(error, () => this.removeTagFromDocument(documentId, tagId)))
        );
    }

    getTagsForDocument(documentId: number): Observable<DocumentTagDTO[]> {
        return this.http.get<DocumentTagDTO[]>(`${this.base}/Documents/${documentId}/Tags`, { headers: this.authHeaders() }).pipe(
            catchError((error: any) => this.handleError(error, () => this.getTagsForDocument(documentId)))
        );
    }


    // ─── Utility ─────────────────────────────────────────────────────

    /**
     * Builds a hierarchical folder tree from the flat folder list.
     */
    buildFolderTree(folders: FolderDTO[]): FolderDTO[] {
        const map = new Map<number, FolderDTO>();
        const roots: FolderDTO[] = [];

        // Initialize
        for (const f of folders) {
            f.children = [];
            f.expanded = false;
            map.set(f.id, f);
        }

        // Build tree
        for (const f of folders) {
            if (f.parentDocumentFolderId && map.has(f.parentDocumentFolderId)) {
                map.get(f.parentDocumentFolderId)!.children!.push(f);
            } else {
                roots.push(f);
            }
        }

        // Assign levels
        const assignLevel = (nodes: FolderDTO[], level: number) => {
            for (const n of nodes) {
                n.level = level;
                if (n.children && n.children.length > 0) {
                    assignLevel(n.children, level + 1);
                }
            }
        };
        assignLevel(roots, 0);

        return roots;
    }

    /**
     * Returns a human-readable file size string.
     */
    formatFileSize(bytes: number): string {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
    }

    /**
     * Returns a Font Awesome icon class for a mime type.
     */
    getFileIcon(mimeType: string): string {
        if (!mimeType) return 'fa-file';
        if (mimeType.startsWith('image/')) return 'fa-file-image';
        if (mimeType === 'application/pdf') return 'fa-file-pdf';
        if (mimeType.includes('spreadsheet') || mimeType.includes('excel') || mimeType.includes('csv')) return 'fa-file-excel';
        if (mimeType.includes('word') || mimeType.includes('document')) return 'fa-file-word';
        if (mimeType.includes('presentation') || mimeType.includes('powerpoint')) return 'fa-file-powerpoint';
        if (mimeType.startsWith('text/')) return 'fa-file-lines';
        if (mimeType.startsWith('video/')) return 'fa-file-video';
        if (mimeType.startsWith('audio/')) return 'fa-file-audio';
        if (mimeType.includes('zip') || mimeType.includes('rar') || mimeType.includes('tar') || mimeType.includes('gzip')) return 'fa-file-zipper';
        return 'fa-file';
    }

    /**
     * Returns a color for a mime type category.
     */
    getFileColor(mimeType: string): string {
        if (!mimeType) return '#6c757d';
        if (mimeType.startsWith('image/')) return '#e74c3c';
        if (mimeType === 'application/pdf') return '#c0392b';
        if (mimeType.includes('spreadsheet') || mimeType.includes('excel')) return '#27ae60';
        if (mimeType.includes('word') || mimeType.includes('document')) return '#2980b9';
        if (mimeType.includes('presentation') || mimeType.includes('powerpoint')) return '#e67e22';
        if (mimeType.startsWith('text/')) return '#8e44ad';
        if (mimeType.startsWith('video/')) return '#2c3e50';
        if (mimeType.startsWith('audio/')) return '#16a085';
        return '#6c757d';
    }
}
