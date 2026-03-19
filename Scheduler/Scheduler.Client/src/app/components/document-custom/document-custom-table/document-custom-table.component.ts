import { Component, OnInit, OnChanges, SimpleChanges, Input, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { DocumentService, DocumentData, DocumentQueryParameters } from '../../../scheduler-data-services/document.service';
import { AlertService, MessageSeverity } from '../../../services/alert.service';
import { ConfirmationService } from '../../../services/confirmation-service';
import { TableColumn } from '../../../utility/foundation.utility';

@Component({
  selector: 'app-document-custom-table',
  templateUrl: './document-custom-table.component.html',
  styleUrls: ['./document-custom-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DocumentCustomTableComponent implements OnInit, OnChanges {

  @Input() Documents: DocumentData[] | null = null; // Optional prefiltered data
  @Input() isSmallScreen: boolean = false;
  @Input() filterText: string | null = null;
  @Input() queryParams: Partial<DocumentQueryParameters> = { }
  @Input() columns: TableColumn[] = [];

  public filteredDocuments: DocumentData[] | null = null;

  // Sorting properties
  public sortColumn: string | null = null;
  public sortDirection: 'asc' | 'desc' = 'asc';

  private isLoadingSubject = new BehaviorSubject<boolean>(true);
  public isLoading$ = this.isLoadingSubject.asObservable();

  private isManagingData: boolean = false;
  private debounceTimeout: any;

  constructor(private documentService: DocumentService,
              private alertService: AlertService,
              private confirmationService: ConfirmationService) { }

  ngOnInit(): void {

    if (this.columns.length === 0) {
      this.buildDefaultColumns();
    }

    if (!this.Documents) {

        this.isManagingData = true;
        this.loadData();

    } else {

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

    }
  }


  ngOnChanges(changes: SimpleChanges): void {

    if (changes['filterText'] && this.isManagingData == true)
    {
       clearTimeout(this.debounceTimeout);
       this.debounceTimeout = setTimeout(() => {

         if (this.isManagingData)
         {
             this.loadData();
         }
         else
         {
             this.applyFiltersAndSort();
         }

       }, 200);
    }

    if (changes['queryParams'])
    {
        this.loadData()
    }
  }


  private buildDefaultColumns(): void {

    const defaultColumns: TableColumn[] = [
      { key: 'name', label: 'Name', width: undefined, mobile: 'prominent' },
      { key: 'documentType.name', label: 'Document Type', width: undefined },
      { key: 'description', label: 'Description', width: undefined },
      { key: 'status', label: 'Status', width: '120px' },
      { key: 'fileName', label: 'File Name', width: undefined },
      { key: 'mimeType', label: 'MIME Type', width: undefined },
      { key: 'uploadedDate', label: 'Uploaded', width: undefined, template: 'date' },
      { key: 'uploadedBy', label: 'Uploaded By', width: undefined },
      { key: 'contact.firstName', label: 'Contact', width: undefined, template: 'link', linkPath: ['/contact', 'contactId'] },
      { key: 'resource.name', label: 'Resource', width: undefined, template: 'link', linkPath: ['/resource', 'resourceId'] },
      { key: 'scheduledEvent.name', label: 'Event', width: undefined, template: 'link', linkPath: ['/schedule', 'scheduledEventId'] },
      { key: 'notes', label: 'Notes', width: undefined },
    ];

    this.columns = defaultColumns;
  }


  public sortBy(column: string) : void {

    if (this.sortColumn === column) {
        this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
        this.sortColumn = column;
        this.sortDirection = 'asc';
    }

    this.applyFiltersAndSort();
  }


  public loadData(): void {

    if (!this.isManagingData) {
      return;
    }

    const documentQueryParams = {
        ...this.queryParams,
        anyStringContains: this.filterText || undefined,
        includeRelations: true
    };

    this.documentService.GetDocumentList(documentQueryParams).subscribe({
      next: (DocumentList) => {
        if (DocumentList) {
          this.Documents = DocumentList;
        } else {
          this.Documents = [];
        }

        this.applyFiltersAndSort();
        this.isLoadingSubject.next(false);

      },
      error: (err) => {
        this.isLoadingSubject.next(false);
        this.alertService.showMessage("Error getting Document data", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


   private applyFiltersAndSort(): void {

    if (!this.Documents) {
      this.filteredDocuments = null;
      return;
    }

    const getNestedValue = (obj: any, path: string): any => {
      return path.split('.').reduce((current, key) => {
        return current && current[key] !== undefined ? current[key] : '';
      }, obj);
    };

    let result = [...this.Documents];

    if (this.filterText) {

      const searchText = this.filterText.toLowerCase().trim();

      if (searchText) {

        const filterFields = [
          'name',
          'description',
          'documentType.name',
          'status',
          'fileName',
          'mimeType',
          'uploadedBy',
          'notes',
          'contact.firstName',
          'contact.lastName',
          'resource.name',
          'scheduledEvent.name',
        ];

        result = result.filter((document) =>

          filterFields.some((field) => {
            const value = getNestedValue(document, field);
            return value && value.toString().toLowerCase().includes(searchText);
          })
        );
      }
    }

    // Apply sorting
    if (this.sortColumn) {
      result.sort((a, b) => {

        const aValue = getNestedValue(a, this.sortColumn!);
        const bValue = getNestedValue(b, this.sortColumn!);

        if (typeof aValue === 'number' && typeof bValue === 'number') {
          return this.sortDirection === 'asc' ? aValue - bValue : bValue - aValue;
        }

        const aStr = aValue ? aValue.toString() : '';
        const bStr = bValue ? bValue.toString() : '';
        const comparison = aStr.localeCompare(bStr, undefined, {sensitivity: 'base' });
        return this.sortDirection === 'asc' ? comparison : -comparison;
      });
    }

    this.filteredDocuments = result;
  }


  public handleDelete(document: DocumentData): void {
    this.confirmationService
      .confirm('Delete Document', 'Are you sure you want to delete this Document?')
      .then((result) => {
          if (result) {
              this.deleteDocument(document);
          }
      })
      .catch(() => { });
  }


  private deleteDocument(documentData: DocumentData): void {
    this.documentService.DeleteDocument(documentData.id).subscribe({
      next: () => {
        this.documentService.ClearAllCaches();
        this.loadData();
      },
      error: (err) => {
         this.alertService.showMessage("Error deleting Document", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public handleUndelete(document: DocumentData): void {
    this.confirmationService
      .confirm('Undelete Document', 'Are you sure you want to undelete this Document?')
      .then((result) => {
          if (result) {
              this.undeleteDocument(document);
          }
      })
      .catch(() => { });
  }


  private undeleteDocument(documentData: DocumentData): void {
    var documentToSubmit = this.documentService.ConvertToDocumentSubmitData(documentData);
    documentToSubmit.deleted = false;

    this.documentService.PutDocument(documentToSubmit.id, documentToSubmit).subscribe({
      next: () => {
        this.documentService.ClearAllCaches();
        this.loadData();
      },
      error: (err) => {
         this.alertService.showMessage("Error undeleting Document", JSON.stringify(err), MessageSeverity.error);
      }
    });
  }


  public getDocumentId(index: number, document: any): number {
    return document.id;
  }


  // Helper to read nested properties safely
  public getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((acc, part) => acc && acc[part], obj);
  }


  // Build routerLink arrays like ['/contact', contactId]
  public buildLink(item: any, path: string[]): any[] {
    return path.map(segment => segment.startsWith('/') ? segment : item[segment]);
  }


  // Returns only columns that should appear on mobile
  get mobileColumns(): TableColumn[] {
    return this.columns.filter(col => col.mobile !== 'hidden');
  }


  // First "prominent" column for mobile view
  get prominentColumn(): TableColumn | null {
    return this.columns.find(col => col.mobile === 'prominent') || null;
  }
}
