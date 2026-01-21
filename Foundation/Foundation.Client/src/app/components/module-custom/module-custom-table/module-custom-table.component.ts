//
// Module Custom Table Component
//
// Premium table for displaying modules with role/token counts and status badges.
// Modeled after user-custom-table pattern.
//

import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { ModuleService, ModuleData, ModuleQueryParameters } from '../../../security-data-services/module.service';

@Component({
    selector: 'app-module-custom-table',
    templateUrl: './module-custom-table.component.html',
    styleUrls: ['./module-custom-table.component.scss']
})
export class ModuleCustomTableComponent implements OnInit, OnDestroy, OnChanges {

    //
    // Inputs
    //
    @Input() filterText: string = '';

    //
    // Outputs
    //
    @Output() countChange = new EventEmitter<number>();

    //
    // Lifecycle
    //
    private destroy$ = new Subject<void>();
    private filterSubject = new Subject<string>();

    //
    // Data
    //
    public modules: ModuleData[] = [];
    public isLoading: boolean = true;
    public errorMessage: string | null = null;

    //
    // Pagination
    //
    public currentPage: number = 1;
    public pageSize: number = 20;
    public totalCount: number = 0;

    // Math reference for template
    public Math = Math;


    constructor(
        private router: Router,
        private moduleService: ModuleService
    ) { }


    ngOnInit(): void {
        this.setupFilterDebounce();
        this.loadModules();
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    ngOnChanges(changes: SimpleChanges): void {
        if (changes['filterText'] && !changes['filterText'].firstChange) {
            this.filterSubject.next(this.filterText);
        }
    }


    private setupFilterDebounce(): void {
        this.filterSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(() => {
            this.currentPage = 1;
            this.loadModules();
        });
    }


    //
    // Data Loading
    //

    loadModules(): void {
        this.isLoading = true;
        this.errorMessage = null;

        const params: any = {
            active: true,
            deleted: false,
            pageSize: this.pageSize,
            pageNumber: this.currentPage,
            includeRelations: true
        };

        if (this.filterText && this.filterText.trim()) {
            params.anyStringContains = this.filterText.trim();
        }

        this.moduleService.GetModuleList(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (modules) => {
                    this.modules = modules;
                    this.isLoading = false;
                    this.loadTotalCount();
                },
                error: (err) => {
                    console.error('Error loading modules:', err);
                    this.errorMessage = 'Failed to load modules. Please try again.';
                    this.isLoading = false;
                }
            });
    }


    private loadTotalCount(): void {
        const params: any = {
            active: true,
            deleted: false
        };

        if (this.filterText && this.filterText.trim()) {
            params.anyStringContains = this.filterText.trim();
        }

        this.moduleService.GetModulesRowCount(params)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (count) => {
                    this.totalCount = Number(count);
                    this.countChange.emit(this.totalCount);
                },
                error: (err) => {
                    console.error('Error loading module count:', err);
                }
            });
    }


    //
    // Pagination
    //

    onPageChange(page: number): void {
        this.currentPage = page;
        this.loadModules();
    }


    get totalPages(): number {
        return Math.ceil(this.totalCount / this.pageSize);
    }


    //
    // Navigation
    //

    navigateToDetail(module: ModuleData): void {
        this.router.navigate(['/module', module.id]);
    }


    //
    // Display helpers
    //

    getStatusBadgeClass(module: ModuleData): string {
        if (module.deleted) {
            return 'badge-deleted';
        }
        return module.active ? 'badge-active' : 'badge-inactive';
    }


    getStatusText(module: ModuleData): string {
        if (module.deleted) {
            return 'Deleted';
        }
        return module.active ? 'Active' : 'Inactive';
    }


    trackByModuleId(index: number, module: ModuleData): number | bigint {
        return module.id;
    }
}
