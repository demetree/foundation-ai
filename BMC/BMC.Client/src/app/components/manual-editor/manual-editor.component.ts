//
// Manual Editor Component
//
// Multi-panel visual editor for build instruction manuals.
// Loads a BuildManual by ID and displays its pages and steps for editing.
//
// Layout:
//   Left sidebar: page thumbnails for navigation
//   Center: page canvas showing step layout
//   Right panel: step properties (camera, annotations)
//   Top toolbar: manual settings, export, save
//
// This is Session 1 — foundational shell with schema wiring.
// Later sessions add: FadeStep rendering, drag-and-drop, annotations, export.
//
// AI-assisted development — reviewed and adapted to project conventions.
//

import { Component, OnInit, OnDestroy, Input, SimpleChanges, OnChanges } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { Subscription } from 'rxjs';
import {
    ManualEditorService,
    BuildManualDto,
    BuildManualPageDto,
    BuildManualStepDto,
    BuildStepAnnotationDto
} from '../../services/manual-editor.service';


@Component({
    selector: 'app-manual-editor',
    templateUrl: './manual-editor.component.html',
    styleUrls: ['./manual-editor.component.scss']
})
export class ManualEditorComponent implements OnInit, OnDestroy, OnChanges {

    // ─── Input — allows embedding inside MOC Viewer ─────────────────────
    //
    // When provided as an @Input (embedded mode), the component loads the
    // manual for this project.  When null, falls back to route params
    // for standalone /manual-editor/:id usage.
    //
    @Input() projectId: number | null = null;

    // ─── Manual Document ────────────────────────────────────────────────
    manual: BuildManualDto | null = null;
    pages: BuildManualPageDto[] = [];
    stepsMap: Map<number, BuildManualStepDto[]> = new Map();      // pageId → steps
    annotationsMap: Map<number, BuildStepAnnotationDto[]> = new Map(); // stepId → annotations

    // ─── Selection State ────────────────────────────────────────────────
    selectedPageIndex = 0;
    selectedStep: BuildManualStepDto | null = null;

    // ─── UI State ───────────────────────────────────────────────────────
    isLoading = true;
    isSaving = false;
    loadError: string | null = null;
    showPropertiesPanel = true;

    // ─── Page Size Presets ──────────────────────────────────────────────
    pageSizePresets = [
        { name: 'A4', widthMm: 210, heightMm: 297 },
        { name: 'A5', widthMm: 148, heightMm: 210 },
        { name: 'Letter', widthMm: 216, heightMm: 279 },
        { name: 'Square', widthMm: 210, heightMm: 210 }
    ];

    private subs: Subscription[] = [];

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private location: Location,
        private editorService: ManualEditorService
    ) { }


    ngOnInit(): void {
        document.title = 'Manual Editor';

        //
        // Embedded mode: projectId is provided via @Input from the MOC Viewer.
        // In this case, skip route param resolution entirely.
        //
        if (this.projectId != null && this.projectId > 0) {
            this.initDraftManual(this.projectId);
            return;
        }

        //
        // Standalone mode: read manual ID from route params, or projectId
        // from query params, to determine what to load.
        //
        this.subs.push(
            this.route.params.subscribe(params => {
                const id = +params['id'];
                if (id) {
                    this.loadManual(id);
                } else {
                    const projectId = +this.route.snapshot.queryParams['projectId'];
                    if (projectId) {
                        this.initDraftManual(projectId);
                    } else {
                        this.isLoading = false;
                        this.loadError = 'No manual ID or project ID provided.';
                    }
                }
            })
        );
    }


    ngOnChanges(changes: SimpleChanges): void {
        //
        // Handle projectId changing while embedded (e.g. navigating between projects)
        //
        if (changes['projectId'] && !changes['projectId'].firstChange) {
            const newId = changes['projectId'].currentValue;
            if (newId != null && newId > 0) {
                this.resetState();
                this.initDraftManual(newId);
            }
        }
    }


    private resetState(): void {
        this.manual = null;
        this.pages = [];
        this.stepsMap.clear();
        this.annotationsMap.clear();
        this.selectedPageIndex = 0;
        this.selectedStep = null;
        this.isLoading = true;
        this.loadError = null;
    }


    ngOnDestroy(): void {
        this.subs.forEach(s => s.unsubscribe());
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Data Loading
    // ═══════════════════════════════════════════════════════════════════

    private initDraftManual(projectId: number): void {
        this.isLoading = true;
        this.loadError = null;

        this.editorService.getManuals(projectId).subscribe({
            next: (manuals) => {
                if (manuals && manuals.length > 0) {
                    // Manual exists for this project, load the first one
                    this.manual = manuals[0];
                    this.loadPages(this.manual.id);
                    this.location.replaceState(`/my-projects/${this.manual.projectId}/viewer?tab=manual`);
                } else {
                    // Start a new in-memory draft
                    this.manual = {
                        id: 0,
                        projectId: projectId,
                        name: 'Untitled Instructions',
                        description: '',
                        pageWidthMm: 210,
                        pageHeightMm: 297,
                        isPublished: false,
                        versionNumber: 1,
                        objectGuid: '00000000-0000-0000-0000-000000000000',
                        active: true,
                        deleted: false
                    };
                    this.pages = [];
                    this.stepsMap.clear();
                    this.annotationsMap.clear();
                    this.isLoading = false;
                }
            },
            error: (err) => {
                this.isLoading = false;
                this.loadError = 'Failed to check existing manuals. ' + (err?.error || err?.message || '');
            }
        });
    }

    private loadManual(id: number): void {
        this.isLoading = true;
        this.loadError = null;

        this.editorService.getManual(id).subscribe({
            next: (manual) => {
                this.manual = manual;
                this.loadPages(manual.id);
            },
            error: (err) => {
                this.isLoading = false;
                this.loadError = 'Failed to load manual. ' + (err?.error || err?.message || '');
            }
        });
    }


    private loadPages(manualId: number): void {
        this.editorService.getPages(manualId).subscribe({
            next: (pages) => {
                this.pages = pages.sort((a, b) => (a.pageNum ?? 0) - (b.pageNum ?? 0));

                // Load steps for each page
                let loadedCount = 0;
                if (this.pages.length === 0) {
                    this.isLoading = false;
                    return;
                }

                this.pages.forEach(page => {
                    this.editorService.getSteps(page.id).subscribe({
                        next: (steps) => {
                            this.stepsMap.set(page.id, steps.sort((a, b) =>
                                (a.stepNumber ?? 0) - (b.stepNumber ?? 0)
                            ));
                            loadedCount++;
                            if (loadedCount === this.pages.length) {
                                this.isLoading = false;
                                // Auto-select first step on first page
                                if (this.pages.length > 0) {
                                    const firstPageSteps = this.stepsMap.get(this.pages[0].id);
                                    if (firstPageSteps && firstPageSteps.length > 0) {
                                        this.selectedStep = firstPageSteps[0];
                                    }
                                }
                            }
                        },
                        error: () => {
                            loadedCount++;
                            if (loadedCount === this.pages.length) {
                                this.isLoading = false;
                            }
                        }
                    });
                });
            },
            error: (err) => {
                this.isLoading = false;
                this.loadError = 'Failed to load pages. ' + (err?.error || err?.message || '');
            }
        });
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Page Management
    // ═══════════════════════════════════════════════════════════════════

    get selectedPage(): BuildManualPageDto | null {
        return this.pages[this.selectedPageIndex] ?? null;
    }

    get selectedPageSteps(): BuildManualStepDto[] {
        if (!this.selectedPage) return [];
        return this.stepsMap.get(this.selectedPage.id) ?? [];
    }

    selectPage(index: number): void {
        this.selectedPageIndex = index;
        this.selectedStep = null;

        // Auto-select first step on new page
        const steps = this.selectedPageSteps;
        if (steps.length > 0) {
            this.selectedStep = steps[0];
        }
    }

    addPage(): void {
        if (!this.manual) return;

        if (this.manual.id === 0) {
            // Must auto-save manual first to get an ID
            this.isSaving = true;
            this.editorService.createManual(this.manual).subscribe({
                next: (created) => {
                    this.manual = created;
                    this.isSaving = false;
                    this.location.replaceState(`/my-projects/${this.manual!.projectId}/viewer?tab=manual`);
                    this.executeAddPage();
                },
                error: (err) => {
                    this.isSaving = false;
                    alert('Failed to save manual before adding page. ' + (err?.error || ''));
                }
            });
        } else {
            this.executeAddPage();
        }
    }

    private executeAddPage(): void {
        if (!this.manual) return;
        const newPageNum = this.pages.length + 1;
        this.editorService.createPage({
            buildManualId: this.manual.id,
            pageNum: newPageNum,
            title: `Page ${newPageNum}`,
            notes: '',
            objectGuid: '00000000-0000-0000-0000-000000000000',
            active: true,
            deleted: false
        }).subscribe({
            next: (page) => {
                this.pages.push(page);
                this.stepsMap.set(page.id, []);
                this.selectPage(this.pages.length - 1);
            }
        });
    }

    deletePage(index: number): void {
        const page = this.pages[index];
        if (!page) return;

        this.editorService.deletePage(page.id).subscribe({
            next: () => {
                this.pages.splice(index, 1);
                this.stepsMap.delete(page.id);
                if (this.selectedPageIndex >= this.pages.length) {
                    this.selectedPageIndex = Math.max(0, this.pages.length - 1);
                }
                this.selectedStep = null;
            }
        });
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Step Management
    // ═══════════════════════════════════════════════════════════════════

    selectStep(step: BuildManualStepDto): void {
        this.selectedStep = step;
    }

    addStep(): void {
        const page = this.selectedPage;
        if (!page) return;

        const existingSteps = this.selectedPageSteps;
        const newStepNum = existingSteps.length > 0
            ? Math.max(...existingSteps.map(s => s.stepNumber ?? 0)) + 1
            : 1;

        this.editorService.createStep({
            buildManualPageId: page.id,
            stepNumber: newStepNum,
            cameraPositionX: 150,
            cameraPositionY: 150,
            cameraPositionZ: 150,
            cameraTargetX: 0,
            cameraTargetY: 0,
            cameraTargetZ: 0,
            cameraZoom: 1,
            showExplodedView: false,
            explodedDistance: 0,
            objectGuid: '00000000-0000-0000-0000-000000000000',
            active: true,
            deleted: false
        }).subscribe({
            next: (step) => {
                const steps = this.stepsMap.get(page.id) ?? [];
                steps.push(step);
                this.stepsMap.set(page.id, steps);
                this.selectedStep = step;
            }
        });
    }

    deleteStep(step: BuildManualStepDto): void {
        const page = this.selectedPage;
        if (!page) return;

        this.editorService.deleteStep(step.id).subscribe({
            next: () => {
                const steps = this.stepsMap.get(page.id) ?? [];
                const idx = steps.findIndex(s => s.id === step.id);
                if (idx >= 0) steps.splice(idx, 1);
                this.stepsMap.set(page.id, steps);
                if (this.selectedStep?.id === step.id) {
                    this.selectedStep = steps.length > 0 ? steps[0] : null;
                }
            }
        });
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Save
    // ═══════════════════════════════════════════════════════════════════

    saveManual(): void {
        if (!this.manual) return;

        this.isSaving = true;
        
        if (this.manual.id === 0) {
            // First time save for draft
            this.editorService.createManual(this.manual).subscribe({
                next: (created) => {
                    this.manual = created;
                    this.isSaving = false;
                    this.location.replaceState(`/my-projects/${this.manual!.projectId}/viewer?tab=manual`);
                },
                error: (err) => {
                    this.isSaving = false;
                    alert('Failed to create manual. ' + (err?.error || ''));
                }
            });
        } else {
            // Update existing
            this.editorService.updateManual(this.manual.id, this.manual).subscribe({
                next: (updated) => {
                    this.manual = updated;
                    this.isSaving = false;
                },
                error: (err) => {
                    this.isSaving = false;
                    alert('Failed to update manual. ' + (err?.error || ''));
                }
            });
        }
    }

    saveStep(): void {
        if (!this.selectedStep) return;

        this.isSaving = true;
        this.editorService.updateStep(this.selectedStep.id, this.selectedStep).subscribe({
            next: (updated) => {
                // Update in stepsMap
                const page = this.selectedPage;
                if (page) {
                    const steps = this.stepsMap.get(page.id) ?? [];
                    const idx = steps.findIndex(s => s.id === updated.id);
                    if (idx >= 0) steps[idx] = updated;
                }
                this.selectedStep = updated;
                this.isSaving = false;
            },
            error: () => {
                this.isSaving = false;
            }
        });
    }


    // ═══════════════════════════════════════════════════════════════════
    //  Manual Settings
    // ═══════════════════════════════════════════════════════════════════

    setPageSize(preset: { name: string; widthMm: number; heightMm: number }): void {
        if (!this.manual) return;
        this.manual.pageWidthMm = preset.widthMm;
        this.manual.pageHeightMm = preset.heightMm;
    }

    get pageAspectRatio(): number {
        if (!this.manual?.pageWidthMm || !this.manual?.pageHeightMm) return 210 / 297;
        return this.manual.pageWidthMm / this.manual.pageHeightMm;
    }

    togglePropertiesPanel(): void {
        this.showPropertiesPanel = !this.showPropertiesPanel;
    }

    goBack(): void {
        if (this.manual?.projectId) {
            this.router.navigate(['/my-projects', this.manual.projectId, 'viewer']);
        } else {
            this.router.navigate(['/my-projects']);
        }
    }
}
