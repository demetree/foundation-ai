import { Injectable } from '@angular/core';

export interface CsvColumn<T> {
    header: string;
    accessor: (row: T) => any;
    formatter?: (value: any) => string;
}

@Injectable({ providedIn: 'root' })
export class CsvExportService {

    /**
     * Export data to a CSV file and trigger browser download.
     *
     * @param filename - Name of file (without extension)
     * @param rows     - Array of data objects
     * @param columns  - Column definitions with header, accessor, and optional formatter
     */
    exportToCsv<T>(filename: string, rows: T[], columns: CsvColumn<T>[]): void {
        if (!rows || rows.length === 0) return;

        const headerLine = columns.map(c => this.escapeCsvValue(c.header)).join(',');
        const dataLines = rows.map(row =>
            columns.map(col => {
                const raw = col.accessor(row);
                const formatted = col.formatter ? col.formatter(raw) : this.defaultFormat(raw);
                return this.escapeCsvValue(formatted);
            }).join(',')
        );

        const csvContent = [headerLine, ...dataLines].join('\r\n');
        const blob = new Blob(['\ufeff' + csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = url;
        link.download = `${filename}.csv`;
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();

        // Cleanup
        setTimeout(() => {
            document.body.removeChild(link);
            URL.revokeObjectURL(url);
        }, 100);
    }

    private escapeCsvValue(value: string): string {
        if (value == null) return '';
        const str = String(value);
        if (str.includes(',') || str.includes('"') || str.includes('\n') || str.includes('\r')) {
            return `"${str.replace(/"/g, '""')}"`;
        }
        return str;
    }

    private defaultFormat(value: any): string {
        if (value == null || value === undefined) return '';
        if (typeof value === 'boolean') return value ? 'Yes' : 'No';
        if (value instanceof Date) return value.toISOString().split('T')[0];
        return String(value);
    }
}
