//
// fm-trash-view.component.ts
//
// AI-Developed — This file was developed with AI assistance.
//
// Child component: Trash / Recycle Bin view with restore and permanent delete.
//
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FileManagerService } from '../../../services/file-manager.service';


@Component({
    selector: 'fm-trash-view',
    templateUrl: './fm-trash-view.component.html',
    styleUrls: ['./fm-trash-view.component.scss']
})
export class FmTrashViewComponent {

    @Input() trashDocuments: any[] = [];

    @Output() close = new EventEmitter<void>();
    @Output() restore = new EventEmitter<any>();
    @Output() permanentDelete = new EventEmitter<any>();
    @Output() emptyTrash = new EventEmitter<void>();


    constructor(public fileManagerService: FileManagerService) {}
}
