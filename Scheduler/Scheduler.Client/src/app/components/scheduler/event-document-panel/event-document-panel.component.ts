/**
 * EventDocumentPanelComponent
 *
 * AI-Developed — Reusable document management panel for any entity that has
 * a foreign key on the Document table (ScheduledEvent, Contact, Resource, etc.).
 *
 * Features:
 *   - Drag-and-drop file upload with file picker fallback
 *   - Inline preview for PDFs and images
 *   - Document type categorization via DocumentType lookup
 *   - Status workflow: Uploaded → Reviewed → Filed
 *   - Download, delete, notes
 *   - Generic owner binding via ownerField / ownerId inputs
 */

import { Component, OnInit, Input, ViewChild, ElementRef } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { DocumentService, DocumentData } from '../../../scheduler-data-services/document.service';
import { DocumentTypeService, DocumentTypeData } from '../../../scheduler-data-services/document-type.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { FileManagerService } from '../../../services/file-manager.service';

interface PendingUpload {
  file: File;
  documentTypeId: number | bigint | null;
  notes: string;
}

@Component({
  selector: 'app-event-document-panel',
  templateUrl: './event-document-panel.component.html',
  styleUrls: ['./event-document-panel.component.scss']
})
export class EventDocumentPanelComponent implements OnInit {

  // -----------------------------------------------------------------------
  // Inputs — generic owner binding
  //
  // ownerField: the FK column name on the Document table
  //             (e.g. 'scheduledEventId', 'contactId', 'resourceId')
  // ownerId:    the id value of the owning entity
  // -----------------------------------------------------------------------
  @Input() ownerField: string = 'scheduledEventId';
  @Input() ownerId!: number | bigint;

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  // -----------------------------------------------------------------------
  // State
  // -----------------------------------------------------------------------
  documents: DocumentData[] = [];
  documentTypes: DocumentTypeData[] = [];
  loading = true;
  uploading = false;
  dragOver = false;

  // Upload form state
  showUploadForm = false;
  pendingUpload: PendingUpload | null = null;

  // Preview state
  previewDocument: DocumentData | null = null;
  previewUrl: SafeResourceUrl | null = null;

  // Thumbnail state
  thumbnailUrls = new Map<number | bigint, string>();

  // Text editor state
  editingDocumentId: number | null = null;
  editingDocumentName = '';

  // Status options
  readonly statusOptions = ['Uploaded', 'Reviewed', 'Filed'];


  constructor(
    private documentService: DocumentService,
    private documentTypeService: DocumentTypeService,
    private alertService: AlertService,
    private authService: AuthService,
    private sanitizer: DomSanitizer,
    private fileManagerService: FileManagerService,
    private router: Router
  ) {}


  ngOnInit(): void {
    this.loadData();
  }


  // -----------------------------------------------------------------------
  // Data Loading
  // -----------------------------------------------------------------------

  private async loadData(): Promise<void> {
    this.loading = true;

    try {
      // Load document types
      this.documentTypes = await lastValueFrom(
        this.documentTypeService.GetDocumentTypeList({ active: true, deleted: false })
      );
    } catch (err) {
      console.error('Failed to load document types', err);
    }

    try {
      //
      // Build a dynamic query using the ownerField to filter documents
      // belonging to the specific entity (event, contact, resource, etc.)
      //
      const query: any = {
        active: true,
        deleted: false,
        includeRelations: true
      };

      query[this.ownerField] = this.ownerId;

      this.documents = await lastValueFrom(
        this.documentService.GetDocumentList(query)
      );
    } catch (err) {
      console.error('Failed to load documents', err);
    }

    this.loading = false;
    this.loadThumbnails();
  }


  private loadThumbnails(): void {
    for (const doc of this.documents) {
      const mime = (doc.mimeType || '').toLowerCase();
      if (mime.startsWith('image/') || mime === 'application/pdf') {
        this.fileManagerService.fetchThumbnailBlob(doc.id as number).subscribe(url => {
          if (url) {
            this.thumbnailUrls.set(doc.id as number, url);
          }
        });
      }
    }
  }


  openInDocuments(doc: DocumentData): void {
    this.router.navigate(['/filemanager'], { queryParams: { docId: doc.id } });
  }


  isTextFile(doc: DocumentData): boolean {
    const m = (doc.mimeType || '').toLowerCase();
    return m.startsWith('text/') ||
      m === 'application/json' ||
      m === 'application/xml' ||
      m === 'application/javascript' ||
      m === 'application/x-yaml';
  }

  openTextEditor(doc: DocumentData): void {
    this.editingDocumentId = doc.id as number;
    this.editingDocumentName = doc.name || doc.fileName || 'Untitled';
  }

  closeTextEditor(): void {
    this.editingDocumentId = null;
    this.editingDocumentName = '';
  }

  async onEditorSaved(_event: any): Promise<void> {
    this.closeTextEditor();
    this.documentService.ClearAllCaches();
    await this.loadData();
  }


  // -----------------------------------------------------------------------
  // Drag & Drop
  // -----------------------------------------------------------------------

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.prepareUpload(files[0]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.prepareUpload(input.files[0]);
    }
  }

  triggerFileInput(): void {
    this.fileInput.nativeElement.click();
  }


  // -----------------------------------------------------------------------
  // Upload
  // -----------------------------------------------------------------------

  private async prepareUpload(file: File): Promise<void> {
    // Validate file size (10MB max)
    const maxSize = 10 * 1024 * 1024;
    if (file.size > maxSize) {
      this.alertService.showMessage('File Too Large',
        'Maximum file size is 10MB.',
        MessageSeverity.warn);
      return;
    }

    // Auto-detect document type from filename
    const detectedTypeId = this.detectDocumentType(file.name);

    this.pendingUpload = {
      file,
      documentTypeId: detectedTypeId,
      notes: ''
    };

    this.showUploadForm = true;
  }




  private detectDocumentType(filename: string): number | bigint | null {
    const lower = filename.toLowerCase();
    const rentalType = this.documentTypes.find(dt => dt.name.toLowerCase().includes('rental'));
    const insuranceType = this.documentTypes.find(dt => dt.name.toLowerCase().includes('insurance'));
    const permitType = this.documentTypes.find(dt => dt.name.toLowerCase().includes('permit'));
    const receiptType = this.documentTypes.find(dt => dt.name.toLowerCase().includes('receipt'));

    if (lower.includes('rental') || lower.includes('agreement') || lower.includes('contract')) {
      return rentalType?.id ?? null;
    }
    if (lower.includes('insurance') || lower.includes('liability')) {
      return insuranceType?.id ?? null;
    }
    if (lower.includes('permit') || lower.includes('license')) {
      return permitType?.id ?? null;
    }
    if (lower.includes('receipt') || lower.includes('payment')) {
      return receiptType?.id ?? null;
    }

    return this.documentTypes.length > 0 ? this.documentTypes[this.documentTypes.length - 1].id : null;
  }


  cancelUpload(): void {
    this.pendingUpload = null;
    this.showUploadForm = false;
    // Reset file input
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
  }


  async uploadDocument(): Promise<void> {
    if (!this.pendingUpload || !this.pendingUpload.documentTypeId) {
      this.alertService.showMessage('Missing Type', 'Please select a document type.', MessageSeverity.warn);
      return;
    }

    this.uploading = true;

    try {
      const options: any = {
        documentTypeId: this.pendingUpload.documentTypeId as number
      };
      options[this.ownerField] = this.ownerId as number;

      await lastValueFrom(
        this.fileManagerService.uploadDocuments([this.pendingUpload.file], options)
      );

      this.alertService.showMessage('Document Uploaded',
        `${this.pendingUpload.file.name} has been uploaded successfully.`,
        MessageSeverity.success);

      this.cancelUpload();

      // Clear caches and reload
      this.documentService.ClearAllCaches();
      await this.loadData();

    } catch (err) {
      console.error('Failed to upload document', err);
      this.alertService.showMessage('Upload Failed', 'Failed to upload document. Please try again.', MessageSeverity.error);
    } finally {
      this.uploading = false;
    }
  }


  // -----------------------------------------------------------------------
  // Preview
  // -----------------------------------------------------------------------

  openPreview(doc: DocumentData): void {
    this.previewDocument = doc;
    this.previewUrl = null;

    // Stream preview from server instead of inline base64
    this.fileManagerService.downloadDocument(doc.id as number).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        this.previewUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
      },
      error: () => {
        this.previewUrl = null;
      }
    });
  }

  closePreview(): void {
    this.previewDocument = null;
    this.previewUrl = null;
  }

  isPreviewable(doc: DocumentData): boolean {
    const mime = (doc.mimeType || '').toLowerCase();
    return mime.startsWith('image/') || mime === 'application/pdf';
  }

  isImage(doc: DocumentData): boolean {
    return (doc.mimeType || '').toLowerCase().startsWith('image/');
  }

  isPdf(doc: DocumentData): boolean {
    return (doc.mimeType || '').toLowerCase() === 'application/pdf';
  }


  // -----------------------------------------------------------------------
  // Download
  // -----------------------------------------------------------------------

  downloadDocument(doc: DocumentData): void {
    this.fileManagerService.downloadDocument(doc.id as number).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = doc.fileName || 'document';
        link.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.alertService.showMessage('Error', 'Download failed.', MessageSeverity.error);
      }
    });
  }


  // -----------------------------------------------------------------------
  // Status Update
  // -----------------------------------------------------------------------

  async updateStatus(doc: DocumentData, newStatus: string): Promise<void> {
    try {
      const submit = doc.ConvertToSubmitData();
      submit.status = newStatus;
      submit.statusDate = new Date().toISOString();
      submit.statusChangedBy = this.authService.currentUser?.userName || 'Unknown';

      await lastValueFrom(
        this.documentService.PutDocument(doc.id as number, submit)
      );

      doc.status = newStatus;
      doc.statusDate = new Date().toISOString();
      doc.statusChangedBy = submit.statusChangedBy;

      this.alertService.showMessage('Status Updated',
        `Document status changed to "${newStatus}".`,
        MessageSeverity.success);

    } catch (err) {
      console.error('Failed to update document status', err);
      this.alertService.showMessage('Error', 'Failed to update document status.', MessageSeverity.error);
    }
  }


  // -----------------------------------------------------------------------
  // Delete
  // -----------------------------------------------------------------------

  async deleteDocument(doc: DocumentData): Promise<void> {
    if (!confirm(`Are you sure you want to delete "${doc.name}"?`)) {
      return;
    }

    try {
      await lastValueFrom(
        this.documentService.DeleteDocument(doc.id as number)
      );

      this.alertService.showMessage('Document Deleted',
        `${doc.name} has been deleted.`,
        MessageSeverity.success);

      // Reload
      this.documentService.ClearAllCaches();
      await this.loadData();

    } catch (err) {
      console.error('Failed to delete document', err);
      this.alertService.showMessage('Error', 'Failed to delete document.', MessageSeverity.error);
    }
  }


  // -----------------------------------------------------------------------
  // Helpers
  // -----------------------------------------------------------------------

  getDocumentTypeName(doc: DocumentData): string {
    if (doc.documentType) {
      return doc.documentType.name;
    }
    const dt = this.documentTypes.find(t => t.id === doc.documentTypeId);
    return dt?.name || 'Unknown';
  }

  getDocumentTypeColor(doc: DocumentData): string {
    if (doc.documentType?.color) {
      return doc.documentType.color;
    }
    const dt = this.documentTypes.find(t => t.id === doc.documentTypeId);
    return dt?.color || '#6B7280';
  }

  formatFileSize(bytes: number | bigint): string {
    const b = Number(bytes);
    if (b < 1024) return `${b} B`;
    if (b < 1024 * 1024) return `${(b / 1024).toFixed(1)} KB`;
    return `${(b / (1024 * 1024)).toFixed(1)} MB`;
  }

  getStatusIcon(status: string | null): string {
    switch (status?.toLowerCase()) {
      case 'filed': return 'fa-circle-check';
      case 'reviewed': return 'fa-eye';
      case 'uploaded':
      default: return 'fa-cloud-arrow-up';
    }
  }

  getStatusClass(status: string | null): string {
    switch (status?.toLowerCase()) {
      case 'filed': return 'status-filed';
      case 'reviewed': return 'status-reviewed';
      case 'uploaded':
      default: return 'status-uploaded';
    }
  }

  getFileIcon(mimeType: string | null): string {
    const mime = (mimeType || '').toLowerCase();
    if (mime.startsWith('image/')) return 'fa-file-image';
    if (mime === 'application/pdf') return 'fa-file-pdf';
    if (mime.includes('word') || mime.includes('document')) return 'fa-file-word';
    if (mime.includes('spreadsheet') || mime.includes('excel')) return 'fa-file-excel';
    return 'fa-file';
  }
}
