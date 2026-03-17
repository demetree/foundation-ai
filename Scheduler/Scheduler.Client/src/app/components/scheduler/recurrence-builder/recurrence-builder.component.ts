import { Component, EventEmitter, Input, OnInit, Output, OnChanges, SimpleChanges, Inject, forwardRef } from '@angular/core';
import { RecurrenceRuleData } from '../../../scheduler-data-services/recurrence-rule.service';
import { RecurrenceFrequencyData, RecurrenceFrequencyService } from '../../../scheduler-data-services/recurrence-frequency.service';
import { lastValueFrom } from 'rxjs';

@Component({
    selector: 'app-recurrence-builder',
    templateUrl: './recurrence-builder.component.html',
    styleUrls: ['./recurrence-builder.component.scss']
})
export class RecurrenceBuilderComponent implements OnInit, OnChanges {
    @Input() recurrenceRule: RecurrenceRuleData | null = null;
    @Input() simpleMode: boolean = false;
    @Output() recurrenceRuleChange = new EventEmitter<RecurrenceRuleData>();

    frequencies: RecurrenceFrequencyData[] = [];
    frequenciesLoading = false;
    frequenciesError: string | null = null;
    selectedFrequencyId: number | null = null;

    // Local state for builder form
    recurrenceInterval: number = 1;
    endType: 'never' | 'count' | 'until' = 'never';
    recurrenceCount: number = 10;
    recurrenceUntilStr: string = ''; // yyyy-MM-dd string for the date input

    // Weekly
    selectedDaysMask: number = 0;

    // Monthly
    monthlyType: 'dayOfMonth' | 'dayOfWeek' = 'dayOfMonth';
    dayOfMonth: number = 1;
    dayOfWeekInMonth: number = 1; // 1st, 2nd, etc.
    selectedDayOfWeekForMonthly: number = 1; // Sunday bitmask value

    // Yearly
    yearlyMonth: number = 1; // January=1
    yearlyDayOfMonth: number = 1;

    // Preserve original rule metadata for updates
    private _originalRuleId: number | bigint = 0;
    private _originalObjectGuid: string = '';
    private _originalVersionNumber: number | bigint = 0;
    private _originalActive: boolean = true;
    private _originalDeleted: boolean = false;

    // Constants for Frequencies (Hardcoded based on SchedulerDatabaseGenerator)
    readonly FREQ_ONCE = 1;
    readonly FREQ_DAILY = 2;
    readonly FREQ_WEEKLY = 3;
    readonly FREQ_MONTHLY = 4;
    readonly FREQ_YEARLY = 5;

    daysOfWeek = [
        { name: 'S', value: 1, label: 'Sunday' },
        { name: 'M', value: 2, label: 'Monday' },
        { name: 'T', value: 4, label: 'Tuesday' },
        { name: 'W', value: 8, label: 'Wednesday' },
        { name: 'T', value: 16, label: 'Thursday' },
        { name: 'F', value: 32, label: 'Friday' },
        { name: 'S', value: 64, label: 'Saturday' }
    ];

    weekOrders = [
        { value: 1, label: 'First' },
        { value: 2, label: 'Second' },
        { value: 3, label: 'Third' },
        { value: 4, label: 'Fourth' },
        { value: 5, label: 'Last' }
    ];

    months = [
        { value: 1, label: 'January' },
        { value: 2, label: 'February' },
        { value: 3, label: 'March' },
        { value: 4, label: 'April' },
        { value: 5, label: 'May' },
        { value: 6, label: 'June' },
        { value: 7, label: 'July' },
        { value: 8, label: 'August' },
        { value: 9, label: 'September' },
        { value: 10, label: 'October' },
        { value: 11, label: 'November' },
        { value: 12, label: 'December' }
    ];

    constructor(@Inject(forwardRef(() => RecurrenceFrequencyService)) private frequencyService: RecurrenceFrequencyService) { }

    async ngOnInit() {
        await this.loadFrequencies();
        this.initializeFromInput();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['recurrenceRule'] && !changes['recurrenceRule'].firstChange) {
            this.initializeFromInput();
        }
    }

    async loadFrequencies() {
        this.frequenciesLoading = true;
        this.frequenciesError = null;
        try {
            this.frequencies = await lastValueFrom(this.frequencyService.GetRecurrenceFrequencyList());
        } catch (err: any) {
            this.frequenciesError = 'Failed to load recurrence frequencies. Please try again.';
            console.error('RecurrenceBuilder: Failed to load frequencies', err);
        } finally {
            this.frequenciesLoading = false;
        }
    }

    initializeFromInput() {
        if (this.recurrenceRule) {
            // Preserve original metadata so updateRule() can re-attach it
            this._originalRuleId = this.recurrenceRule.id ?? 0;
            this._originalObjectGuid = this.recurrenceRule.objectGuid ?? '';
            this._originalVersionNumber = this.recurrenceRule.versionNumber ?? 0;
            this._originalActive = this.recurrenceRule.active ?? true;
            this._originalDeleted = this.recurrenceRule.deleted ?? false;

            // Map recurrenceRule to local state
            this.selectedFrequencyId = Number(this.recurrenceRule.recurrenceFrequencyId);
            this.recurrenceInterval = Number(this.recurrenceRule.interval || 1);

            // End condition
            if (this.recurrenceRule.count && Number(this.recurrenceRule.count) > 0) {
                this.endType = 'count';
                this.recurrenceCount = Number(this.recurrenceRule.count);
            } else if (this.recurrenceRule.untilDateTime) {
                this.endType = 'until';
                // Convert ISO string to yyyy-MM-dd for the date input
                this.recurrenceUntilStr = this.recurrenceRule.untilDateTime.substring(0, 10);
            } else {
                this.endType = 'never';
            }

            this.selectedDaysMask = this.recurrenceRule.dayOfWeekMask ? Number(this.recurrenceRule.dayOfWeekMask) : 0;
            this.dayOfMonth = this.recurrenceRule.dayOfMonth ? Number(this.recurrenceRule.dayOfMonth) : new Date().getDate();
            this.dayOfWeekInMonth = this.recurrenceRule.dayOfWeekInMonth ? Number(this.recurrenceRule.dayOfWeekInMonth) : 1;

            // For monthly specific day of week, the mask should have only one bit set
            if (this.recurrenceRule.dayOfWeekMask && Number(this.recurrenceRule.dayOfWeekMask) > 0) {
                this.selectedDayOfWeekForMonthly = Number(this.recurrenceRule.dayOfWeekMask);
            }

            if (this.recurrenceRule.dayOfMonth) {
                this.monthlyType = 'dayOfMonth';
            } else if (this.recurrenceRule.dayOfWeekInMonth) {
                this.monthlyType = 'dayOfWeek';
            }

            // Yearly defaults (using dayOfMonth for month-day)
            // The schema doesn't have a dedicated month field, so we'll use dayOfMonth
            // and store month info in a way the backend understands
            this.yearlyMonth = new Date().getMonth() + 1;
            this.yearlyDayOfMonth = this.recurrenceRule.dayOfMonth ? Number(this.recurrenceRule.dayOfMonth) : 1;
        } else {
            this.resetDefaults();
        }
    }

    resetDefaults() {
        this._originalRuleId = 0;
        this._originalObjectGuid = '';
        this._originalVersionNumber = 0;
        this._originalActive = true;
        this._originalDeleted = false;

        this.selectedFrequencyId = this.FREQ_DAILY;
        this.recurrenceInterval = 1;
        this.endType = 'never';
        this.recurrenceCount = 10;
        this.recurrenceUntilStr = '';
        this.selectedDaysMask = 0;
        this.monthlyType = 'dayOfMonth';
        this.dayOfMonth = 1;
        this.dayOfWeekInMonth = 1;
        this.selectedDayOfWeekForMonthly = 1;
        this.yearlyMonth = new Date().getMonth() + 1;
        this.yearlyDayOfMonth = 1;
    }

    onFrequencyChange(freqId: number) {
        this.selectedFrequencyId = freqId;
        this.updateRule();
    }

    onEndTypeChange(type: 'never' | 'count' | 'until') {
        this.endType = type;
        if (type === 'until' && !this.recurrenceUntilStr) {
            // Default to 30 days from now
            const defaultEnd = new Date();
            defaultEnd.setDate(defaultEnd.getDate() + 30);
            this.recurrenceUntilStr = defaultEnd.toISOString().substring(0, 10);
        }
        this.updateRule();
    }

    toggleDay(dayValue: number) {
        if (this.selectedDaysMask & dayValue) {
            this.selectedDaysMask &= ~dayValue;
        } else {
            this.selectedDaysMask |= dayValue;
        }
        this.updateRule();
    }

    updateRule() {
        // Build a new rule BUT preserve original metadata
        const rule = new RecurrenceRuleData();

        // Re-attach original identity so the save logic can distinguish create vs update
        rule.id = this._originalRuleId;
        rule.objectGuid = this._originalObjectGuid;
        rule.versionNumber = this._originalVersionNumber;
        rule.active = this._originalActive;
        rule.deleted = this._originalDeleted;

        rule.recurrenceFrequencyId = this.selectedFrequencyId!;
        rule.interval = this.recurrenceInterval;

        // End condition
        switch (this.endType) {
            case 'count':
                rule.count = this.recurrenceCount as any;
                rule.untilDateTime = null;
                break;
            case 'until':
                rule.count = null as any;
                rule.untilDateTime = this.recurrenceUntilStr
                    ? new Date(this.recurrenceUntilStr + 'T23:59:59Z').toISOString()
                    : null;
                break;
            default: // 'never'
                rule.count = null as any;
                rule.untilDateTime = null;
                break;
        }

        // Frequency-specific fields
        if (this.selectedFrequencyId === this.FREQ_WEEKLY) {
            rule.dayOfWeekMask = this.selectedDaysMask;
            rule.dayOfMonth = null as any;
            rule.dayOfWeekInMonth = null as any;
        } else if (this.selectedFrequencyId === this.FREQ_MONTHLY) {
            if (this.monthlyType === 'dayOfMonth') {
                rule.dayOfMonth = this.dayOfMonth;
                rule.dayOfWeekMask = null as any;
                rule.dayOfWeekInMonth = null as any;
            } else {
                rule.dayOfWeekMask = this.selectedDayOfWeekForMonthly;
                rule.dayOfWeekInMonth = this.dayOfWeekInMonth;
                rule.dayOfMonth = null as any;
            }
        } else if (this.selectedFrequencyId === this.FREQ_YEARLY) {
            rule.dayOfMonth = this.yearlyDayOfMonth;
            // Month is stored conceptually — the backend may use dayOfMonth + the event start to determine month
            rule.dayOfWeekMask = null as any;
            rule.dayOfWeekInMonth = null as any;
        } else {
            // Daily / Once — clear all day-specific fields
            rule.dayOfWeekMask = null as any;
            rule.dayOfMonth = null as any;
            rule.dayOfWeekInMonth = null as any;
        }

        this.recurrenceRuleChange.emit(rule);
    }

    // Helper for UI to check if day is selected
    isDaySelected(dayValue: number): boolean {
        return (this.selectedDaysMask & dayValue) === dayValue;
    }

    // Generate human-readable recurrence preview
    getRecurrenceSummary(): string {
        if (!this.selectedFrequencyId) return '';

        const interval = this.recurrenceInterval || 1;
        let summary = 'Repeats ';

        switch (this.selectedFrequencyId) {
            case this.FREQ_DAILY:
                summary += interval === 1 ? 'every day' : `every ${interval} days`;
                break;
            case this.FREQ_WEEKLY:
                summary += interval === 1 ? 'every week' : `every ${interval} weeks`;
                const selectedDays = this.daysOfWeek
                    .filter(d => this.isDaySelected(d.value))
                    .map(d => d.label.substring(0, 3));
                if (selectedDays.length > 0) {
                    summary += ` on ${selectedDays.join(', ')}`;
                }
                break;
            case this.FREQ_MONTHLY:
                summary += interval === 1 ? 'every month' : `every ${interval} months`;
                if (this.monthlyType === 'dayOfMonth') {
                    summary += ` on day ${this.dayOfMonth}`;
                } else {
                    const order = this.weekOrders.find(w => w.value === this.dayOfWeekInMonth)?.label?.toLowerCase() || '';
                    const dayName = this.daysOfWeek.find(d => d.value === this.selectedDayOfWeekForMonthly)?.label || '';
                    summary += ` on the ${order} ${dayName}`;
                }
                break;
            case this.FREQ_YEARLY:
                summary += interval === 1 ? 'every year' : `every ${interval} years`;
                const monthName = this.months.find(m => m.value === this.yearlyMonth)?.label || '';
                summary += ` on ${monthName} ${this.yearlyDayOfMonth}`;
                break;
        }

        switch (this.endType) {
            case 'count':
                summary += `, ${this.recurrenceCount} times`;
                break;
            case 'until':
                if (this.recurrenceUntilStr) {
                    summary += `, until ${this.recurrenceUntilStr}`;
                }
                break;
        }

        return summary;
    }
}
