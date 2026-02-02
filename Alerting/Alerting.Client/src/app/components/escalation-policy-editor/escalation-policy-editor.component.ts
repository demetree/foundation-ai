import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

import { EscalationPolicyService, EscalationPolicyData, EscalationPolicySubmitData } from '../../alerting-data-services/escalation-policy.service';
import { EscalationRuleService, EscalationRuleData, EscalationRuleSubmitData } from '../../alerting-data-services/escalation-rule.service';
import { OnCallScheduleService, OnCallScheduleData } from '../../alerting-data-services/on-call-schedule.service';
import { ServiceData } from '../../alerting-data-services/service.service';
import { AlertingUserService, AlertingUser } from '../../services/alerting-user.service';
import { AlertService } from '../../services/alert.service';

/**
 * Target types supported by escalation rules
 */
export type EscalationTargetType = 'User' | 'Schedule' | 'AllUsers';

/**
 * Local model for editing rules (includes UI state)
 */
export interface EditableRule {
    id: number | null;
    ruleOrder: number;
    delayMinutes: number;
    repeatCount: number;
    repeatDelayMinutes: number | null;
    targetType: EscalationTargetType;
    targetObjectGuid: string | null;
    targetDisplayName: string;
    isNew: boolean;
    isModified: boolean;
    originalData: EscalationRuleData | null;
}

@Component({
    selector: 'app-escalation-policy-editor',
    templateUrl: './escalation-policy-editor.component.html',
    styleUrls: ['./escalation-policy-editor.component.scss']
})
export class EscalationPolicyEditorComponent implements OnInit, OnDestroy {

    private destroy$ = new Subject<void>();

    // Route
    policyId: number | null = null;
    isNewPolicy = false;

    // Loading states
    isLoading = true;
    isSaving = false;
    errorMessage: string | null = null;

    // Policy data
    policy: EscalationPolicyData | null = null;
    originalPolicy: EscalationPolicyData | null = null;

    // Form fields for policy metadata
    formName = '';
    formDescription = '';
    formActive = true;

    // Rules
    rules: EditableRule[] = [];
    originalRulesSnapshot: string = '';

    // Available targets
    users: AlertingUser[] = [];
    schedules: OnCallScheduleData[] = [];

    // Linked services (loaded asynchronously)
    linkedServices: ServiceData[] = [];

    // Track unsaved changes
    hasUnsavedChanges = false;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private escalationPolicyService: EscalationPolicyService,
        private escalationRuleService: EscalationRuleService,
        private onCallScheduleService: OnCallScheduleService,
        private alertingUserService: AlertingUserService,
        private alertService: AlertService
    ) { }


    ngOnInit(): void {
        // Get policy ID from route
        const idParam = this.route.snapshot.paramMap.get('id');

        if (idParam === 'new') {
            this.isNewPolicy = true;
            this.isLoading = false;
            this.loadTargetOptions();
        } else if (idParam) {
            this.policyId = parseInt(idParam, 10);
            this.loadPolicyData();
        } else {
            this.errorMessage = 'Invalid policy ID';
            this.isLoading = false;
        }
    }


    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }


    private loadPolicyData(): void {
        if (!this.policyId) return;

        this.isLoading = true;
        this.errorMessage = null;

        forkJoin({
            policy: this.escalationPolicyService.GetEscalationPolicy(this.policyId, true),
            rules: this.escalationRuleService.GetEscalationRuleList({
                escalationPolicyId: this.policyId,
                active: true,
                deleted: false
            }),
            schedules: this.onCallScheduleService.GetOnCallScheduleList({ active: true, deleted: false })
        })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: ({ policy, rules, schedules }) => {
                    this.policy = policy;
                    this.originalPolicy = { ...policy } as EscalationPolicyData;
                    this.schedules = schedules;

                    // Populate form fields
                    this.formName = policy.name;
                    this.formDescription = policy.description || '';
                    this.formActive = policy.active;

                    // Convert rules to editable format
                    this.rules = rules
                        .sort((a, b) => Number(a.ruleOrder) - Number(b.ruleOrder))
                        .map(r => this.convertToEditableRule(r));

                    // Snapshot for change detection
                    this.originalRulesSnapshot = JSON.stringify(this.rules.map(r => ({
                        id: r.id,
                        ruleOrder: r.ruleOrder,
                        delayMinutes: r.delayMinutes,
                        repeatCount: r.repeatCount,
                        repeatDelayMinutes: r.repeatDelayMinutes,
                        targetType: r.targetType,
                        targetObjectGuid: r.targetObjectGuid
                    })));

                    this.loadTargetOptions();
                    this.loadLinkedServices();
                    this.isLoading = false;
                },
                error: (err) => {
                    console.error('Error loading policy:', err);
                    this.errorMessage = 'Failed to load escalation policy';
                    this.isLoading = false;
                }
            });
    }


    private loadTargetOptions(): void {
        // Load users for target selection
        this.alertingUserService.getUsers()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (users) => {
                    this.users = users;
                },
                error: (err) => {
                    console.error('Error loading users:', err);
                    this.users = [];
                }
            });
    }


    private loadLinkedServices(): void {
        if (!this.policy) return;

        this.policy.Services.then((services: ServiceData[]) => {
            this.linkedServices = services;
        }).catch((err: Error) => {
            console.error('Error loading linked services:', err);
            this.linkedServices = [];
        });
    }


    private convertToEditableRule(rule: EscalationRuleData): EditableRule {
        return {
            id: Number(rule.id),
            ruleOrder: Number(rule.ruleOrder),
            delayMinutes: Number(rule.delayMinutes),
            repeatCount: Number(rule.repeatCount),
            repeatDelayMinutes: rule.repeatDelayMinutes != null ? Number(rule.repeatDelayMinutes) : null,
            targetType: (rule.targetType as EscalationTargetType) || 'User',
            targetObjectGuid: rule.targetObjectGuid,
            targetDisplayName: this.getTargetDisplayName(rule.targetType as EscalationTargetType, rule.targetObjectGuid),
            isNew: false,
            isModified: false,
            originalData: rule
        };
    }


    getTargetDisplayName(targetType: EscalationTargetType, objectGuid: string | null): string {
        if (targetType === 'AllUsers') {
            return 'All Users';
        }

        if (!objectGuid) {
            return 'Not Set';
        }

        if (targetType === 'User') {
            const user = this.users.find(u => u.objectGuid === objectGuid);
            return user?.displayName || 'Unknown User';
        }

        if (targetType === 'Schedule') {
            const schedule = this.schedules.find(s => s.objectGuid === objectGuid);
            return schedule?.name || 'Unknown Schedule';
        }

        return 'Unknown';
    }


    // ─────────────────────────────────────────────────────────────────────────────
    // Rule Management
    // ─────────────────────────────────────────────────────────────────────────────

    addRule(): void {
        const newOrder = this.rules.length > 0
            ? Math.max(...this.rules.map(r => r.ruleOrder)) + 1
            : 1;

        const newRule: EditableRule = {
            id: null,
            ruleOrder: newOrder,
            delayMinutes: 5,
            repeatCount: 0,
            repeatDelayMinutes: null,
            targetType: 'User',
            targetObjectGuid: null,
            targetDisplayName: 'Not Set',
            isNew: true,
            isModified: false,
            originalData: null
        };

        this.rules.push(newRule);
        this.markUnsavedChanges();
    }


    removeRule(index: number): void {
        this.rules.splice(index, 1);
        this.reorderRules();
        this.markUnsavedChanges();
    }


    onRuleDrop(event: CdkDragDrop<EditableRule[]>): void {
        moveItemInArray(this.rules, event.previousIndex, event.currentIndex);
        this.reorderRules();
        this.markUnsavedChanges();
    }


    private reorderRules(): void {
        this.rules.forEach((rule, index) => {
            rule.ruleOrder = index + 1;
        });
    }


    onRuleChange(rule: EditableRule): void {
        if (!rule.isNew) {
            rule.isModified = true;
        }
        rule.targetDisplayName = this.getTargetDisplayName(rule.targetType, rule.targetObjectGuid);
        this.markUnsavedChanges();
    }


    onTargetTypeChange(rule: EditableRule): void {
        // Clear target when type changes
        if (rule.targetType === 'AllUsers') {
            rule.targetObjectGuid = null;
        } else {
            rule.targetObjectGuid = null;
        }
        this.onRuleChange(rule);
    }


    onUserSelection(rule: EditableRule, userGuid: string | null): void {
        rule.targetObjectGuid = userGuid;
        // Find user display name
        if (userGuid) {
            const user = this.users.find(u => u.objectGuid === userGuid);
            rule.targetDisplayName = user?.displayName || userGuid;
        } else {
            rule.targetDisplayName = '';
        }
        this.onRuleChange(rule);
    }



    // ─────────────────────────────────────────────────────────────────────────────
    // Form State
    // ─────────────────────────────────────────────────────────────────────────────

    markUnsavedChanges(): void {
        this.hasUnsavedChanges = true;
    }


    onMetadataChange(): void {
        this.markUnsavedChanges();
    }


    // ─────────────────────────────────────────────────────────────────────────────
    // Navigation
    // ─────────────────────────────────────────────────────────────────────────────

    goBack(): void {
        if (this.hasUnsavedChanges) {
            if (!confirm('You have unsaved changes. Are you sure you want to leave?')) {
                return;
            }
        }
        this.router.navigate(['/escalation-policy-management']);
    }


    cancel(): void {
        this.goBack();
    }


    // ─────────────────────────────────────────────────────────────────────────────
    // Save Logic
    // ─────────────────────────────────────────────────────────────────────────────

    async save(): Promise<void> {
        if (!this.validateForm()) {
            return;
        }

        this.isSaving = true;

        try {
            // Save policy metadata
            if (this.policy) {
                const policyUpdate: EscalationPolicySubmitData = {
                    id: this.policy.id as number,
                    name: this.formName.trim(),
                    description: this.formDescription.trim() || null,
                    versionNumber: this.policy.versionNumber as number,
                    active: this.formActive,
                    deleted: false
                };

                await this.escalationPolicyService.PutEscalationPolicy(this.policy.id as number, policyUpdate).toPromise();
            }

            // Sync rules using Relational Synchronizer pattern
            await this.syncRules();

            // Clear caches and reload
            this.escalationPolicyService.ClearAllCaches();
            this.escalationRuleService.ClearAllCaches();

            this.hasUnsavedChanges = false;
            this.alertService.showSuccessMessage('Success', 'Escalation policy saved successfully');

            // Reload to get fresh data
            this.loadPolicyData();

        } catch (err) {
            console.error('Error saving policy:', err);
            this.alertService.showErrorMessage('Error', 'Failed to save escalation policy');
        } finally {
            this.isSaving = false;
        }
    }


    private async syncRules(): Promise<void> {
        if (!this.policyId) return;

        const originalRules = this.rules
            .filter(r => !r.isNew && r.originalData)
            .map(r => r.originalData!);

        // Find deleted rules (in original but not in current)
        const currentRuleIds = new Set(this.rules.filter(r => r.id).map(r => r.id));
        const deletedRules = originalRules.filter(r => !currentRuleIds.has(Number(r.id)));

        // Soft-delete removed rules
        for (const rule of deletedRules) {
            const deleteData: EscalationRuleSubmitData = {
                id: rule.id as number,
                escalationPolicyId: this.policyId,
                ruleOrder: rule.ruleOrder as number,
                delayMinutes: rule.delayMinutes as number,
                repeatCount: rule.repeatCount as number,
                repeatDelayMinutes: rule.repeatDelayMinutes as number | null,
                targetType: rule.targetType,
                targetObjectGuid: rule.targetObjectGuid,
                versionNumber: rule.versionNumber as number,
                active: false,
                deleted: true
            };
            await this.escalationRuleService.PutEscalationRule(rule.id as number, deleteData).toPromise();
        }

        // Create new rules
        for (const rule of this.rules.filter(r => r.isNew)) {
            const createData: EscalationRuleSubmitData = {
                id: 0,
                escalationPolicyId: this.policyId,
                ruleOrder: rule.ruleOrder,
                delayMinutes: rule.delayMinutes,
                repeatCount: rule.repeatCount,
                repeatDelayMinutes: rule.repeatDelayMinutes,
                targetType: rule.targetType,
                targetObjectGuid: rule.targetObjectGuid,
                versionNumber: 1,
                active: true,
                deleted: false
            };
            await this.escalationRuleService.PostEscalationRule(createData).toPromise();
        }

        // Update modified rules
        for (const rule of this.rules.filter(r => !r.isNew && r.isModified && r.originalData)) {
            const updateData: EscalationRuleSubmitData = {
                id: rule.id!,
                escalationPolicyId: this.policyId,
                ruleOrder: rule.ruleOrder,
                delayMinutes: rule.delayMinutes,
                repeatCount: rule.repeatCount,
                repeatDelayMinutes: rule.repeatDelayMinutes,
                targetType: rule.targetType,
                targetObjectGuid: rule.targetObjectGuid,
                versionNumber: rule.originalData!.versionNumber as number,
                active: true,
                deleted: false
            };
            await this.escalationRuleService.PutEscalationRule(rule.id!, updateData).toPromise();
        }

        // Update order for unchanged rules that moved
        for (const rule of this.rules.filter(r => !r.isNew && !r.isModified && r.originalData)) {
            const origOrder = Number(rule.originalData!.ruleOrder);
            if (origOrder !== rule.ruleOrder) {
                const updateData: EscalationRuleSubmitData = {
                    id: rule.id!,
                    escalationPolicyId: this.policyId,
                    ruleOrder: rule.ruleOrder,
                    delayMinutes: rule.delayMinutes,
                    repeatCount: rule.repeatCount,
                    repeatDelayMinutes: rule.repeatDelayMinutes,
                    targetType: rule.targetType,
                    targetObjectGuid: rule.targetObjectGuid,
                    versionNumber: rule.originalData!.versionNumber as number,
                    active: true,
                    deleted: false
                };
                await this.escalationRuleService.PutEscalationRule(rule.id!, updateData).toPromise();
            }
        }
    }


    private validateForm(): boolean {
        if (!this.formName.trim()) {
            this.alertService.showErrorMessage('Validation', 'Policy name is required');
            return false;
        }

        // Validate rules
        for (let i = 0; i < this.rules.length; i++) {
            const rule = this.rules[i];
            if (rule.targetType !== 'AllUsers' && !rule.targetObjectGuid) {
                this.alertService.showErrorMessage('Validation', `Step ${i + 1}: Please select a target`);
                return false;
            }
        }

        return true;
    }


    // ─────────────────────────────────────────────────────────────────────────────
    // UnsavedChangesGuard Support
    // ─────────────────────────────────────────────────────────────────────────────

    canDeactivate(): boolean {
        if (this.hasUnsavedChanges) {
            return confirm('You have unsaved changes. Are you sure you want to leave?');
        }
        return true;
    }


    // ─────────────────────────────────────────────────────────────────────────────
    // Helper Methods
    // ─────────────────────────────────────────────────────────────────────────────

    getDelayDisplay(minutes: number): string {
        if (minutes === 0) return 'Immediately';
        if (minutes < 60) return `After ${minutes} min`;
        const hours = Math.floor(minutes / 60);
        const mins = minutes % 60;
        if (mins === 0) return `After ${hours}h`;
        return `After ${hours}h ${mins}m`;
    }
}
