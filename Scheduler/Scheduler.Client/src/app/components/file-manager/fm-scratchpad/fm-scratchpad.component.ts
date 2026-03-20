//
// fm-scratchpad.component.ts
//
// AI-Developed — This file was significantly developed with AI assistance.
//
// Embeddable scratchpad (entity notes) component.
// Wraps the fm-text-editor: loads or creates a per-entity markdown note
// stored in the _Notes folder, with archive-and-start-fresh support.
//
import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { FileManagerService, DocumentDTO } from '../../../services/file-manager.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';


@Component({
    selector: 'fm-scratchpad',
    templateUrl: './fm-scratchpad.component.html',
    styleUrls: ['./fm-scratchpad.component.scss']
})
export class FmScratchpadComponent implements OnInit, OnChanges {

    @Input() entityType!: string;
    @Input() entityId!: number;
    @Input() entityName = '';

    /** null = loading, undefined = no scratchpad yet */
    scratchpadDoc: DocumentDTO | null | undefined = null;
    isLoading = true;
    isCreating = false;
    isArchiving = false;

    constructor(
        private fileManagerService: FileManagerService,
        private alertService: AlertService
    ) {}

    ngOnInit(): void {
        this.loadScratchpad();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if ((changes['entityId'] || changes['entityType']) && !changes['entityId']?.firstChange) {
            this.loadScratchpad();
        }
    }

    loadScratchpad(): void {
        if (!this.entityType || !this.entityId) return;
        this.isLoading = true;
        this.fileManagerService.getScratchpad(this.entityType, this.entityId).subscribe({
            next: (doc) => {
                this.scratchpadDoc = doc ?? undefined;
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error loading scratchpad', err);
                this.scratchpadDoc = undefined;
                this.isLoading = false;
            }
        });
    }

    createNote(): void {
        this.isCreating = true;
        this.fileManagerService.createScratchpad(this.entityType, this.entityId, this.entityName).subscribe({
            next: (doc) => {
                this.scratchpadDoc = doc;
                this.isCreating = false;
                this.alertService.showMessage('Notes', 'Scratchpad created.', MessageSeverity.success);
            },
            error: (err) => {
                console.error('Error creating scratchpad', err);
                this.alertService.showMessage('Error', 'Could not create scratchpad.', MessageSeverity.error);
                this.isCreating = false;
            }
        });
    }

    archiveAndStartFresh(): void {
        if (!this.scratchpadDoc) return;
        this.isArchiving = true;
        this.fileManagerService.archiveScratchpad(this.entityType, this.entityId, this.entityName).subscribe({
            next: (newDoc) => {
                this.scratchpadDoc = newDoc;
                this.isArchiving = false;
                this.alertService.showMessage('Notes', 'Previous note archived. Fresh note created.', MessageSeverity.success);
            },
            error: (err) => {
                console.error('Error archiving scratchpad', err);
                this.alertService.showMessage('Error', 'Could not archive note.', MessageSeverity.error);
                this.isArchiving = false;
            }
        });
    }

    onEditorSaved(doc: DocumentDTO): void {
        // Update the local reference with the latest version info
        if (this.scratchpadDoc) {
            this.scratchpadDoc.versionNumber = doc.versionNumber;
        }
    }
}
