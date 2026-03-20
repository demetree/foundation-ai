//
// fm-tag-manager-modal.component.ts
//
// AI-Developed — This file was developed with AI assistance.
//
// Child component: Tag management modal — CRUD for document tags
// with inline editing and color picker.
//
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { DocumentTagDTO } from '../../../services/file-manager.service';


@Component({
    selector: 'fm-tag-manager-modal',
    templateUrl: './fm-tag-manager-modal.component.html',
    styleUrls: ['./fm-tag-manager-modal.component.scss']
})
export class FmTagManagerModalComponent {

    @Input() visible = false;
    @Input() allTags: DocumentTagDTO[] = [];
    @Input() tagCounts: Map<number, number> = new Map();
    @Input() tagColorOptions: string[] = [];

    @Output() close = new EventEmitter<void>();
    @Output() createTag = new EventEmitter<{ name: string; color: string }>();
    @Output() updateTag = new EventEmitter<{ tag: DocumentTagDTO; name: string; color: string }>();
    @Output() deleteTag = new EventEmitter<DocumentTagDTO>();

    // Local state for the modal form
    newTagName = '';
    newTagColor = '#6366f1';
    editingTagId: number | null = null;
    editingTagName = '';
    editingTagColor = '';


    onCreateTag(): void {
        if (!this.newTagName.trim()) return;
        this.createTag.emit({ name: this.newTagName.trim(), color: this.newTagColor });
        this.newTagName = '';
        this.newTagColor = '#6366f1';
    }


    startEditTag(tag: DocumentTagDTO, event: Event): void {
        event.stopPropagation();
        this.editingTagId = tag.id;
        this.editingTagName = tag.name;
        this.editingTagColor = tag.color || '#6366f1';
    }


    saveEditTag(tag: DocumentTagDTO): void {
        this.updateTag.emit({ tag, name: this.editingTagName.trim(), color: this.editingTagColor });
        this.editingTagId = null;
    }


    cancelEditTag(): void {
        this.editingTagId = null;
    }
}
