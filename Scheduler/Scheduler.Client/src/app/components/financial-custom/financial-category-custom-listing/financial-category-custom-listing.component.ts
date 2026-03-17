// AI-Developed — This file was significantly developed with AI assistance.
import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationService } from '../../../utility-services/navigation.service';
import { FinancialCategoryService, FinancialCategoryData } from '../../../scheduler-data-services/financial-category.service';
import { FinancialOfficeService, FinancialOfficeData } from '../../../scheduler-data-services/financial-office.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { AuthService } from '../../../services/auth.service';
import { FinancialCategoryCustomAddEditComponent } from '../financial-category-custom-add-edit/financial-category-custom-add-edit.component';
import { Router } from '@angular/router';


//
// Tree view interfaces
//
interface TreeNode {
    category: FinancialCategoryData;
    children: TreeNode[];
    depth: number;
    expanded: boolean;
}

interface AccountTypeGroup {
    typeName: string;
    isRevenue: boolean;
    categories: TreeNode[];
    totalCount: number;
    expanded: boolean;
}

interface CategoryUsageMap {
    [categoryId: number]: number;
}


@Component({
    selector: 'app-financial-category-custom-listing',
    templateUrl: './financial-category-custom-listing.component.html',
    styleUrls: ['./financial-category-custom-listing.component.scss']
})
export class FinancialCategoryCustomListingComponent implements OnInit {

    public categories: FinancialCategoryData[] = [];
    public filteredCategories: FinancialCategoryData[] = [];
    public isLoading = true;
    public isSmallScreen = false;

    // Filter state
    public filterText = '';
    public filterType: 'all' | 'income' | 'expense' = 'all';

    // Sort state
    public sortColumn = 'code';
    public sortDirection: 'asc' | 'desc' = 'asc';

    // Counts
    public totalCount = 0;
    public incomeCount = 0;
    public expenseCount = 0;
    public assetLiabilityEquityCount = 0;

    private debounceTimeout: any;

    // View mode
    public viewMode: 'table' | 'tree' = 'table';

    // Tree view state
    public accountTypeGroups: AccountTypeGroup[] = [];
    public expandedParents: Set<number> = new Set<number>();

    // Transaction usage counts (from CategoryBreakdown)
    public transactionCounts: CategoryUsageMap = {};
    public loadingCounts = false;

    // Account type colors and labels
    public accountTypeConfig: { [key: string]: { bg: string; color: string; icon: string; gradient: string } } = {
        'Income':    { bg: 'rgba(16, 185, 129, 0.1)',  color: '#059669', icon: 'fa-arrow-trend-up',   gradient: 'linear-gradient(135deg, #059669, #10b981)' },
        'Expense':   { bg: 'rgba(239, 68, 68, 0.1)',   color: '#dc2626', icon: 'fa-arrow-trend-down', gradient: 'linear-gradient(135deg, #dc2626, #ef4444)' },
        'COGS':      { bg: 'rgba(245, 158, 11, 0.1)',  color: '#d97706', icon: 'fa-industry',         gradient: 'linear-gradient(135deg, #d97706, #f59e0b)' },
        'Asset':     { bg: 'rgba(59, 130, 246, 0.1)',   color: '#2563eb', icon: 'fa-building-columns', gradient: 'linear-gradient(135deg, #2563eb, #3b82f6)' },
        'Liability': { bg: 'rgba(139, 92, 246, 0.1)',   color: '#7c3aed', icon: 'fa-file-invoice-dollar', gradient: 'linear-gradient(135deg, #7c3aed, #8b5cf6)' },
        'Equity':    { bg: 'rgba(236, 72, 153, 0.1)',   color: '#db2777', icon: 'fa-scale-balanced',  gradient: 'linear-gradient(135deg, #db2777, #ec4899)' }
    };

    // Office filter
    public offices: FinancialOfficeData[] = [];
    public selectedOfficeId: number | null = null;


    constructor(
        private categoryService: FinancialCategoryService,
        private financialOfficeService: FinancialOfficeService,
        private http: HttpClient,
        private alertService: AlertService,
        private authService: AuthService,
        private navigationService: NavigationService,
        private breakpointObserver: BreakpointObserver,
        private router: Router
    ) { }

    @ViewChild('catAddEdit') catAddEdit!: FinancialCategoryCustomAddEditComponent;


    ngOnInit(): void {
        this.breakpointObserver
            .observe(['(max-width: 768px)'])
            .subscribe((result) => {
                this.isSmallScreen = result.matches;
            });

        this.loadData();
        this.loadOffices();
        this.loadTransactionCounts();
    }


    public loadData(): void {
        this.isLoading = true;

        this.categoryService.GetFinancialCategoryList({
            active: true,
            deleted: false,
            includeRelations: true,
            pageSize: 500
        }).subscribe({
            next: (data: FinancialCategoryData[] | null) => {
                this.categories = data ?? [];
                this.computeCounts();
                this.applyFiltersAndSort();
                this.buildTree();
                this.isLoading = false;
            },
            error: () => {
                this.alertService.showMessage('Error', 'Failed to load categories', MessageSeverity.error);
                this.isLoading = false;
            }
        });
    }


    private computeCounts(): void {
        this.totalCount = this.categories.length;
        this.incomeCount = this.categories.filter(c => c.accountType?.isRevenue === true).length;
        this.expenseCount = this.categories.filter(c =>
            c.accountType?.isRevenue === false &&
            c.accountType?.name !== 'Asset' &&
            c.accountType?.name !== 'Liability' &&
            c.accountType?.name !== 'Equity'
        ).length;
        this.assetLiabilityEquityCount = this.categories.filter(c =>
            c.accountType?.name === 'Asset' ||
            c.accountType?.name === 'Liability' ||
            c.accountType?.name === 'Equity'
        ).length;
    }


    private loadOffices(): void {
        this.financialOfficeService.GetFinancialOfficeList({
            active: true,
            deleted: false,
            pageSize: 100
        }).subscribe({
            next: (data: FinancialOfficeData[] | null) => {
                this.offices = data ?? [];
            }
        });
    }


    /**
     * Loads transaction usage counts per category from the server-side CategoryBreakdown endpoint.
     */
    private loadTransactionCounts(): void {
        this.loadingCounts = true;

        const currentYear = new Date().getFullYear();
        const url = `/api/FinancialTransactions/CategoryBreakdown?year=${currentYear}`;
        const headers = new HttpHeaders({
            'Authorization': 'Bearer ' + this.authService.accessToken
        });

        this.http.get<{ year: number; categories: { categoryId: number; count: number }[] }>(url, { headers }).subscribe({
            next: (response) => {
                this.transactionCounts = {};
                for (const cat of (response?.categories ?? [])) {
                    this.transactionCounts[cat.categoryId] = cat.count;
                }
                this.loadingCounts = false;
            },
            error: () => {
                this.loadingCounts = false;
            }
        });
    }


    public getTransactionCount(categoryId: number | bigint): number {
        return this.transactionCounts[Number(categoryId)] || 0;
    }


    //
    // ── View Mode Toggle ──
    //

    public setViewMode(mode: 'table' | 'tree'): void {
        this.viewMode = mode;
    }


    //
    // ── Tree View Logic ──
    //

    private buildTree(): void {
        // Group categories by account type
        const groupMap = new Map<string, { isRevenue: boolean; cats: FinancialCategoryData[] }>();

        // Define group ordering
        const groupOrder = ['Income', 'Expense', 'COGS', 'Asset', 'Liability', 'Equity'];

        for (const cat of this.categories) {
            const typeName = cat.accountType?.name || 'Other';
            if (!groupMap.has(typeName)) {
                groupMap.set(typeName, {
                    isRevenue: cat.accountType?.isRevenue ?? false,
                    cats: []
                });
            }
            groupMap.get(typeName)!.cats.push(cat);
        }

        // Build tree nodes within each group
        this.accountTypeGroups = [];

        // Process in defined order first, then any others
        const processedTypes = new Set<string>();

        for (const typeName of groupOrder) {
            if (groupMap.has(typeName)) {
                const group = groupMap.get(typeName)!;
                this.accountTypeGroups.push({
                    typeName,
                    isRevenue: group.isRevenue,
                    categories: this.buildTreeNodes(group.cats, 0),
                    totalCount: group.cats.length,
                    expanded: true
                });
                processedTypes.add(typeName);
            }
        }

        // Add any remaining types
        for (const [typeName, group] of groupMap) {
            if (!processedTypes.has(typeName)) {
                this.accountTypeGroups.push({
                    typeName,
                    isRevenue: group.isRevenue,
                    categories: this.buildTreeNodes(group.cats, 0),
                    totalCount: group.cats.length,
                    expanded: true
                });
            }
        }
    }


    private buildTreeNodes(categories: FinancialCategoryData[], depth: number): TreeNode[] {
        // Find root categories (no parent, or parent not in this group)
        const catIds = new Set(categories.map(c => Number(c.id)));

        const roots = categories.filter(c =>
            !c.parentFinancialCategoryId || !catIds.has(Number(c.parentFinancialCategoryId))
        );

        const childMap = new Map<number, FinancialCategoryData[]>();
        for (const cat of categories) {
            if (cat.parentFinancialCategoryId && catIds.has(Number(cat.parentFinancialCategoryId))) {
                const parentId = Number(cat.parentFinancialCategoryId);
                if (!childMap.has(parentId)) {
                    childMap.set(parentId, []);
                }
                childMap.get(parentId)!.push(cat);
            }
        }

        const buildNodes = (cats: FinancialCategoryData[], d: number): TreeNode[] => {
            return cats
                .sort((a, b) => (a.code ?? '').localeCompare(b.code ?? ''))
                .map(cat => {
                    const children = childMap.get(Number(cat.id)) || [];
                    return {
                        category: cat,
                        children: buildNodes(children, d + 1),
                        depth: d,
                        expanded: this.expandedParents.has(Number(cat.id))
                    };
                });
        };

        return buildNodes(roots, depth);
    }


    public toggleGroup(group: AccountTypeGroup): void {
        group.expanded = !group.expanded;
    }


    public toggleParent(node: TreeNode): void {
        const id = Number(node.category.id);
        if (this.expandedParents.has(id)) {
            this.expandedParents.delete(id);
            node.expanded = false;
        } else {
            this.expandedParents.add(id);
            node.expanded = true;
        }
    }


    /**
     * Flattens tree nodes for rendering, respecting expand/collapse state.
     */
    public flattenTreeNodes(nodes: TreeNode[]): TreeNode[] {
        const result: TreeNode[] = [];
        for (const node of nodes) {
            result.push(node);
            if (node.expanded && node.children.length > 0) {
                result.push(...this.flattenTreeNodes(node.children));
            }
        }
        return result;
    }


    //
    // ── Existing Filter/Sort Logic (unchanged) ──
    //

    public onOfficeChange(): void {
        this.applyFiltersAndSort();
    }


    public applyFiltersAndSort(): void {
        let result = [...this.categories];

        // Office filter
        if (this.selectedOfficeId !== null) {
            result = result.filter(c => Number(c.financialOfficeId) === this.selectedOfficeId);
        }

        // Type filter
        if (this.filterType === 'income') {
            result = result.filter(c => c.accountType?.isRevenue);
        } else if (this.filterType === 'expense') {
            result = result.filter(c => !c.accountType?.isRevenue);
        }

        // Text filter
        if (this.filterText) {
            const search = this.filterText.toLowerCase();
            result = result.filter(c =>
                (c.name && c.name.toLowerCase().includes(search)) ||
                (c.code && c.code.toLowerCase().includes(search)) ||
                (c.description && c.description.toLowerCase().includes(search))
            );
        }

        // Sort
        result.sort((a, b) => {
            let valA: any, valB: any;

            switch (this.sortColumn) {
                case 'code':
                    valA = (a.code ?? '').toLowerCase();
                    valB = (b.code ?? '').toLowerCase();
                    break;
                case 'name':
                    valA = (a.name ?? '').toLowerCase();
                    valB = (b.name ?? '').toLowerCase();
                    break;
                case 'accountType':
                    valA = (a.accountType?.name ?? '').toLowerCase();
                    valB = (b.accountType?.name ?? '').toLowerCase();
                    break;
                default:
                    valA = 0;
                    valB = 0;
            }

            if (valA < valB) return this.sortDirection === 'asc' ? -1 : 1;
            if (valA > valB) return this.sortDirection === 'asc' ? 1 : -1;
            return 0;
        });

        this.filteredCategories = result;
    }


    public onFilterChange(): void {
        clearTimeout(this.debounceTimeout);
        this.debounceTimeout = setTimeout(() => {
            this.applyFiltersAndSort();
        }, 100);
    }


    public sortBy(column: string): void {
        if (this.sortColumn === column) {
            this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortColumn = column;
            this.sortDirection = 'asc';
        }
        this.applyFiltersAndSort();
    }


    public getSortIcon(column: string): string {
        if (this.sortColumn !== column) return 'fa-sort';
        return this.sortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    }


    public setTypeFilter(type: 'all' | 'income' | 'expense'): void {
        this.filterType = type;
        this.applyFiltersAndSort();
    }


    public getAccountTypeStyle(type: string): { [key: string]: string } {
        const config = this.accountTypeConfig[type] || { bg: 'rgba(107, 114, 128, 0.1)', color: '#6b7280' };
        return {
            'background': config.bg,
            'color': config.color
        };
    }


    public getAccountTypeIcon(type: string): string {
        return this.accountTypeConfig[type]?.icon || 'fa-folder';
    }


    public getAccountTypeGradient(type: string): string {
        return this.accountTypeConfig[type]?.gradient || 'linear-gradient(135deg, #6b7280, #9ca3af)';
    }


    public goBack(): void {
        this.navigationService.goBack();
    }

    public canGoBack(): boolean {
        return this.navigationService.canGoBack();
    }

    public goToDashboard(): void {
        this.router.navigate(['/finances']);
    }

    public trackById(index: number, item: FinancialCategoryData): number {
        return Number(item.id);
    }

    public trackByNodeId(index: number, item: TreeNode): number {
        return Number(item.category.id);
    }


    public addCategory(): void {
        if (this.catAddEdit) {
            this.catAddEdit.preSeededData = {
                financialOfficeId: this.selectedOfficeId
            };
            this.catAddEdit.openModal();
        }
    }


    public editCategory(cat: FinancialCategoryData): void {
        if (this.catAddEdit) {
            this.catAddEdit.openModal(cat);
        }
    }


    public onCategoryChanged(): void {
        this.loadData();
        this.loadTransactionCounts();
    }
}
