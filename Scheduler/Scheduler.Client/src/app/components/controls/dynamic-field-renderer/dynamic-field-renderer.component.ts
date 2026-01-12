
import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { AttributeDefinitionService, AttributeDefinitionData } from '../../../scheduler-data-services/attribute-definition.service';
import { Observable, map } from 'rxjs';

@Component({
    selector: 'app-dynamic-field-renderer',
    templateUrl: './dynamic-field-renderer.component.html',
    styleUrls: ['./dynamic-field-renderer.component.scss']
})
export class DynamicFieldRendererComponent implements OnInit {

    @Input() entityName: string | null = null;
    @Input() data: any = {};
    @Output() dataChange = new EventEmitter<any>();

    public definitions$: Observable<AttributeDefinitionData[]> | null = null;
    public loadedDefinitions: AttributeDefinitionData[] = [];

    constructor(private attributeDefinitionService: AttributeDefinitionService) { }

    ngOnInit(): void {
        if (this.entityName) {
            this.definitions$ = this.attributeDefinitionService.GetAttributeDefinitionList({
                entityName: this.entityName,
                active: true,
                deleted: false
            }).pipe(
                map(defs => {
                    // Sort by sequence
                    return defs.sort((a, b) => (Number(a.sequence) || 0) - (Number(b.sequence) || 0));
                })
            );

            this.definitions$.subscribe(defs => {
                this.loadedDefinitions = defs;
                this.ensureDataStructure();
            });
        }
    }

    private ensureDataStructure() {
        if (!this.data) {
            this.data = {};
            this.dataChange.emit(this.data);
        }
    }

    public getOptions(def: AttributeDefinitionData): string[] {
        if (!def.options) return [];
        // Assuming options are comma, newline, or pipe separated? 
        // Or maybe just use split by comma for now.
        // If options is a JSON array string, try parsing it.
        try {
            const parsed = JSON.parse(def.options);
            if (Array.isArray(parsed)) return parsed;
        } catch (e) {
            // Not JSON, fall back to split
        }

        if (def.options.includes(',')) return def.options.split(',').map(o => o.trim());
        if (def.options.includes('\n')) return def.options.split('\n').map(o => o.trim());

        return [def.options];
    }

    public onValueChange() {
        this.dataChange.emit(this.data);
    }
}
