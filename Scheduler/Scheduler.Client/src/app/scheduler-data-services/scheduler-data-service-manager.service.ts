/*

   GENERATED SERVICE FOR THE SCHEDULER TABLE - DO NOT MODIFY DIRECTLY
   =======================================================================================
   This is the default data interaction service for the Scheduler table.

   It should suffice for many workflows and data access needs, but if anything more is needed, then extend this in a 
   custom version or add an additional targeted helper service.

*/
import {Injectable} from '@angular/core';
import {AccountTypeService} from  './account-type.service';
import {AppealService} from  './appeal.service';
import {AppealChangeHistoryService} from  './appeal-change-history.service';
import {AssignmentRoleService} from  './assignment-role.service';
import {AssignmentRoleQualificationRequirementService} from  './assignment-role-qualification-requirement.service';
import {AssignmentRoleQualificationRequirementChangeHistoryService} from  './assignment-role-qualification-requirement-change-history.service';
import {AssignmentStatusService} from  './assignment-status.service';
import {AttributeDefinitionService} from  './attribute-definition.service';
import {AttributeDefinitionChangeHistoryService} from  './attribute-definition-change-history.service';
import {AttributeDefinitionEntityService} from  './attribute-definition-entity.service';
import {AttributeDefinitionTypeService} from  './attribute-definition-type.service';
import {BatchService} from  './batch.service';
import {BatchChangeHistoryService} from  './batch-change-history.service';
import {BatchStatusService} from  './batch-status.service';
import {BookingSourceTypeService} from  './booking-source-type.service';
import {BudgetService} from  './budget.service';
import {BudgetChangeHistoryService} from  './budget-change-history.service';
import {CalendarService} from  './calendar.service';
import {CalendarChangeHistoryService} from  './calendar-change-history.service';
import {CampaignService} from  './campaign.service';
import {CampaignChangeHistoryService} from  './campaign-change-history.service';
import {ChargeStatusService} from  './charge-status.service';
import {ChargeStatusChangeHistoryService} from  './charge-status-change-history.service';
import {ChargeTypeService} from  './charge-type.service';
import {ChargeTypeChangeHistoryService} from  './charge-type-change-history.service';
import {ClientService} from  './client.service';
import {ClientChangeHistoryService} from  './client-change-history.service';
import {ClientContactService} from  './client-contact.service';
import {ClientContactChangeHistoryService} from  './client-contact-change-history.service';
import {ClientTypeService} from  './client-type.service';
import {ConstituentService} from  './constituent.service';
import {ConstituentChangeHistoryService} from  './constituent-change-history.service';
import {ConstituentJourneyStageService} from  './constituent-journey-stage.service';
import {ConstituentJourneyStageChangeHistoryService} from  './constituent-journey-stage-change-history.service';
import {ContactService} from  './contact.service';
import {ContactChangeHistoryService} from  './contact-change-history.service';
import {ContactContactService} from  './contact-contact.service';
import {ContactContactChangeHistoryService} from  './contact-contact-change-history.service';
import {ContactInteractionService} from  './contact-interaction.service';
import {ContactInteractionChangeHistoryService} from  './contact-interaction-change-history.service';
import {ContactMethodService} from  './contact-method.service';
import {ContactTagService} from  './contact-tag.service';
import {ContactTagChangeHistoryService} from  './contact-tag-change-history.service';
import {ContactTypeService} from  './contact-type.service';
import {CountryService} from  './country.service';
import {CrewService} from  './crew.service';
import {CrewChangeHistoryService} from  './crew-change-history.service';
import {CrewMemberService} from  './crew-member.service';
import {CrewMemberChangeHistoryService} from  './crew-member-change-history.service';
import {CurrencyService} from  './currency.service';
import {DependencyTypeService} from  './dependency-type.service';
import {DocumentService} from  './document.service';
import {DocumentChangeHistoryService} from  './document-change-history.service';
import {DocumentTypeService} from  './document-type.service';
import {EventCalendarService} from  './event-calendar.service';
import {EventChargeService} from  './event-charge.service';
import {EventChargeChangeHistoryService} from  './event-charge-change-history.service';
import {EventResourceAssignmentService} from  './event-resource-assignment.service';
import {EventResourceAssignmentChangeHistoryService} from  './event-resource-assignment-change-history.service';
import {EventStatusService} from  './event-status.service';
import {FinancialCategoryService} from  './financial-category.service';
import {FinancialCategoryChangeHistoryService} from  './financial-category-change-history.service';
import {FinancialOfficeService} from  './financial-office.service';
import {FinancialOfficeChangeHistoryService} from  './financial-office-change-history.service';
import {FinancialTransactionService} from  './financial-transaction.service';
import {FinancialTransactionChangeHistoryService} from  './financial-transaction-change-history.service';
import {FiscalPeriodService} from  './fiscal-period.service';
import {FiscalPeriodChangeHistoryService} from  './fiscal-period-change-history.service';
import {FundService} from  './fund.service';
import {FundChangeHistoryService} from  './fund-change-history.service';
import {GeneralLedgerEntryService} from  './general-ledger-entry.service';
import {GeneralLedgerLineService} from  './general-ledger-line.service';
import {GiftService} from  './gift.service';
import {GiftChangeHistoryService} from  './gift-change-history.service';
import {HouseholdService} from  './household.service';
import {HouseholdChangeHistoryService} from  './household-change-history.service';
import {IconService} from  './icon.service';
import {InteractionTypeService} from  './interaction-type.service';
import {InvoiceService} from  './invoice.service';
import {InvoiceChangeHistoryService} from  './invoice-change-history.service';
import {InvoiceLineItemService} from  './invoice-line-item.service';
import {InvoiceStatusService} from  './invoice-status.service';
import {NotificationSubscriptionService} from  './notification-subscription.service';
import {NotificationSubscriptionChangeHistoryService} from  './notification-subscription-change-history.service';
import {NotificationTypeService} from  './notification-type.service';
import {OfficeService} from  './office.service';
import {OfficeChangeHistoryService} from  './office-change-history.service';
import {OfficeContactService} from  './office-contact.service';
import {OfficeContactChangeHistoryService} from  './office-contact-change-history.service';
import {OfficeTypeService} from  './office-type.service';
import {PaymentMethodService} from  './payment-method.service';
import {PaymentProviderService} from  './payment-provider.service';
import {PaymentProviderChangeHistoryService} from  './payment-provider-change-history.service';
import {PaymentTransactionService} from  './payment-transaction.service';
import {PaymentTransactionChangeHistoryService} from  './payment-transaction-change-history.service';
import {PaymentTypeService} from  './payment-type.service';
import {PaymentTypeChangeHistoryService} from  './payment-type-change-history.service';
import {PeriodStatusService} from  './period-status.service';
import {PledgeService} from  './pledge.service';
import {PledgeChangeHistoryService} from  './pledge-change-history.service';
import {PriorityService} from  './priority.service';
import {QualificationService} from  './qualification.service';
import {RateSheetService} from  './rate-sheet.service';
import {RateSheetChangeHistoryService} from  './rate-sheet-change-history.service';
import {RateTypeService} from  './rate-type.service';
import {ReceiptService} from  './receipt.service';
import {ReceiptChangeHistoryService} from  './receipt-change-history.service';
import {ReceiptTypeService} from  './receipt-type.service';
import {ReceiptTypeChangeHistoryService} from  './receipt-type-change-history.service';
import {RecurrenceExceptionService} from  './recurrence-exception.service';
import {RecurrenceExceptionChangeHistoryService} from  './recurrence-exception-change-history.service';
import {RecurrenceFrequencyService} from  './recurrence-frequency.service';
import {RecurrenceRuleService} from  './recurrence-rule.service';
import {RecurrenceRuleChangeHistoryService} from  './recurrence-rule-change-history.service';
import {RelationshipTypeService} from  './relationship-type.service';
import {ResourceService} from  './resource.service';
import {ResourceAvailabilityService} from  './resource-availability.service';
import {ResourceAvailabilityChangeHistoryService} from  './resource-availability-change-history.service';
import {ResourceChangeHistoryService} from  './resource-change-history.service';
import {ResourceContactService} from  './resource-contact.service';
import {ResourceContactChangeHistoryService} from  './resource-contact-change-history.service';
import {ResourceQualificationService} from  './resource-qualification.service';
import {ResourceQualificationChangeHistoryService} from  './resource-qualification-change-history.service';
import {ResourceShiftService} from  './resource-shift.service';
import {ResourceShiftChangeHistoryService} from  './resource-shift-change-history.service';
import {ResourceTypeService} from  './resource-type.service';
import {SalutationService} from  './salutation.service';
import {ScheduledEventService} from  './scheduled-event.service';
import {ScheduledEventChangeHistoryService} from  './scheduled-event-change-history.service';
import {ScheduledEventDependencyService} from  './scheduled-event-dependency.service';
import {ScheduledEventDependencyChangeHistoryService} from  './scheduled-event-dependency-change-history.service';
import {ScheduledEventQualificationRequirementService} from  './scheduled-event-qualification-requirement.service';
import {ScheduledEventQualificationRequirementChangeHistoryService} from  './scheduled-event-qualification-requirement-change-history.service';
import {ScheduledEventTemplateService} from  './scheduled-event-template.service';
import {ScheduledEventTemplateChangeHistoryService} from  './scheduled-event-template-change-history.service';
import {ScheduledEventTemplateChargeService} from  './scheduled-event-template-charge.service';
import {ScheduledEventTemplateChargeChangeHistoryService} from  './scheduled-event-template-charge-change-history.service';
import {ScheduledEventTemplateQualificationRequirementService} from  './scheduled-event-template-qualification-requirement.service';
import {ScheduledEventTemplateQualificationRequirementChangeHistoryService} from  './scheduled-event-template-qualification-requirement-change-history.service';
import {SchedulingTargetService} from  './scheduling-target.service';
import {SchedulingTargetAddressService} from  './scheduling-target-address.service';
import {SchedulingTargetAddressChangeHistoryService} from  './scheduling-target-address-change-history.service';
import {SchedulingTargetChangeHistoryService} from  './scheduling-target-change-history.service';
import {SchedulingTargetContactService} from  './scheduling-target-contact.service';
import {SchedulingTargetContactChangeHistoryService} from  './scheduling-target-contact-change-history.service';
import {SchedulingTargetQualificationRequirementService} from  './scheduling-target-qualification-requirement.service';
import {SchedulingTargetQualificationRequirementChangeHistoryService} from  './scheduling-target-qualification-requirement-change-history.service';
import {SchedulingTargetTypeService} from  './scheduling-target-type.service';
import {ShiftPatternService} from  './shift-pattern.service';
import {ShiftPatternChangeHistoryService} from  './shift-pattern-change-history.service';
import {ShiftPatternDayService} from  './shift-pattern-day.service';
import {ShiftPatternDayChangeHistoryService} from  './shift-pattern-day-change-history.service';
import {SoftCreditService} from  './soft-credit.service';
import {SoftCreditChangeHistoryService} from  './soft-credit-change-history.service';
import {StateProvinceService} from  './state-province.service';
import {TagService} from  './tag.service';
import {TaxCodeService} from  './tax-code.service';
import {TenantProfileService} from  './tenant-profile.service';
import {TenantProfileChangeHistoryService} from  './tenant-profile-change-history.service';
import {TimeZoneService} from  './time-zone.service';
import {TributeService} from  './tribute.service';
import {TributeChangeHistoryService} from  './tribute-change-history.service';
import {TributeTypeService} from  './tribute-type.service';
import {VolunteerGroupService} from  './volunteer-group.service';
import {VolunteerGroupChangeHistoryService} from  './volunteer-group-change-history.service';
import {VolunteerGroupMemberService} from  './volunteer-group-member.service';
import {VolunteerGroupMemberChangeHistoryService} from  './volunteer-group-member-change-history.service';
import {VolunteerProfileService} from  './volunteer-profile.service';
import {VolunteerProfileChangeHistoryService} from  './volunteer-profile-change-history.service';
import {VolunteerStatusService} from  './volunteer-status.service';


@Injectable({
  providedIn: 'root'
})
export class SchedulerDataServiceManagerService  {

    constructor(public accountTypeService: AccountTypeService
              , public appealService: AppealService
              , public appealChangeHistoryService: AppealChangeHistoryService
              , public assignmentRoleService: AssignmentRoleService
              , public assignmentRoleQualificationRequirementService: AssignmentRoleQualificationRequirementService
              , public assignmentRoleQualificationRequirementChangeHistoryService: AssignmentRoleQualificationRequirementChangeHistoryService
              , public assignmentStatusService: AssignmentStatusService
              , public attributeDefinitionService: AttributeDefinitionService
              , public attributeDefinitionChangeHistoryService: AttributeDefinitionChangeHistoryService
              , public attributeDefinitionEntityService: AttributeDefinitionEntityService
              , public attributeDefinitionTypeService: AttributeDefinitionTypeService
              , public batchService: BatchService
              , public batchChangeHistoryService: BatchChangeHistoryService
              , public batchStatusService: BatchStatusService
              , public bookingSourceTypeService: BookingSourceTypeService
              , public budgetService: BudgetService
              , public budgetChangeHistoryService: BudgetChangeHistoryService
              , public calendarService: CalendarService
              , public calendarChangeHistoryService: CalendarChangeHistoryService
              , public campaignService: CampaignService
              , public campaignChangeHistoryService: CampaignChangeHistoryService
              , public chargeStatusService: ChargeStatusService
              , public chargeStatusChangeHistoryService: ChargeStatusChangeHistoryService
              , public chargeTypeService: ChargeTypeService
              , public chargeTypeChangeHistoryService: ChargeTypeChangeHistoryService
              , public clientService: ClientService
              , public clientChangeHistoryService: ClientChangeHistoryService
              , public clientContactService: ClientContactService
              , public clientContactChangeHistoryService: ClientContactChangeHistoryService
              , public clientTypeService: ClientTypeService
              , public constituentService: ConstituentService
              , public constituentChangeHistoryService: ConstituentChangeHistoryService
              , public constituentJourneyStageService: ConstituentJourneyStageService
              , public constituentJourneyStageChangeHistoryService: ConstituentJourneyStageChangeHistoryService
              , public contactService: ContactService
              , public contactChangeHistoryService: ContactChangeHistoryService
              , public contactContactService: ContactContactService
              , public contactContactChangeHistoryService: ContactContactChangeHistoryService
              , public contactInteractionService: ContactInteractionService
              , public contactInteractionChangeHistoryService: ContactInteractionChangeHistoryService
              , public contactMethodService: ContactMethodService
              , public contactTagService: ContactTagService
              , public contactTagChangeHistoryService: ContactTagChangeHistoryService
              , public contactTypeService: ContactTypeService
              , public countryService: CountryService
              , public crewService: CrewService
              , public crewChangeHistoryService: CrewChangeHistoryService
              , public crewMemberService: CrewMemberService
              , public crewMemberChangeHistoryService: CrewMemberChangeHistoryService
              , public currencyService: CurrencyService
              , public dependencyTypeService: DependencyTypeService
              , public documentService: DocumentService
              , public documentChangeHistoryService: DocumentChangeHistoryService
              , public documentTypeService: DocumentTypeService
              , public eventCalendarService: EventCalendarService
              , public eventChargeService: EventChargeService
              , public eventChargeChangeHistoryService: EventChargeChangeHistoryService
              , public eventResourceAssignmentService: EventResourceAssignmentService
              , public eventResourceAssignmentChangeHistoryService: EventResourceAssignmentChangeHistoryService
              , public eventStatusService: EventStatusService
              , public financialCategoryService: FinancialCategoryService
              , public financialCategoryChangeHistoryService: FinancialCategoryChangeHistoryService
              , public financialOfficeService: FinancialOfficeService
              , public financialOfficeChangeHistoryService: FinancialOfficeChangeHistoryService
              , public financialTransactionService: FinancialTransactionService
              , public financialTransactionChangeHistoryService: FinancialTransactionChangeHistoryService
              , public fiscalPeriodService: FiscalPeriodService
              , public fiscalPeriodChangeHistoryService: FiscalPeriodChangeHistoryService
              , public fundService: FundService
              , public fundChangeHistoryService: FundChangeHistoryService
              , public generalLedgerEntryService: GeneralLedgerEntryService
              , public generalLedgerLineService: GeneralLedgerLineService
              , public giftService: GiftService
              , public giftChangeHistoryService: GiftChangeHistoryService
              , public householdService: HouseholdService
              , public householdChangeHistoryService: HouseholdChangeHistoryService
              , public iconService: IconService
              , public interactionTypeService: InteractionTypeService
              , public invoiceService: InvoiceService
              , public invoiceChangeHistoryService: InvoiceChangeHistoryService
              , public invoiceLineItemService: InvoiceLineItemService
              , public invoiceStatusService: InvoiceStatusService
              , public notificationSubscriptionService: NotificationSubscriptionService
              , public notificationSubscriptionChangeHistoryService: NotificationSubscriptionChangeHistoryService
              , public notificationTypeService: NotificationTypeService
              , public officeService: OfficeService
              , public officeChangeHistoryService: OfficeChangeHistoryService
              , public officeContactService: OfficeContactService
              , public officeContactChangeHistoryService: OfficeContactChangeHistoryService
              , public officeTypeService: OfficeTypeService
              , public paymentMethodService: PaymentMethodService
              , public paymentProviderService: PaymentProviderService
              , public paymentProviderChangeHistoryService: PaymentProviderChangeHistoryService
              , public paymentTransactionService: PaymentTransactionService
              , public paymentTransactionChangeHistoryService: PaymentTransactionChangeHistoryService
              , public paymentTypeService: PaymentTypeService
              , public paymentTypeChangeHistoryService: PaymentTypeChangeHistoryService
              , public periodStatusService: PeriodStatusService
              , public pledgeService: PledgeService
              , public pledgeChangeHistoryService: PledgeChangeHistoryService
              , public priorityService: PriorityService
              , public qualificationService: QualificationService
              , public rateSheetService: RateSheetService
              , public rateSheetChangeHistoryService: RateSheetChangeHistoryService
              , public rateTypeService: RateTypeService
              , public receiptService: ReceiptService
              , public receiptChangeHistoryService: ReceiptChangeHistoryService
              , public receiptTypeService: ReceiptTypeService
              , public receiptTypeChangeHistoryService: ReceiptTypeChangeHistoryService
              , public recurrenceExceptionService: RecurrenceExceptionService
              , public recurrenceExceptionChangeHistoryService: RecurrenceExceptionChangeHistoryService
              , public recurrenceFrequencyService: RecurrenceFrequencyService
              , public recurrenceRuleService: RecurrenceRuleService
              , public recurrenceRuleChangeHistoryService: RecurrenceRuleChangeHistoryService
              , public relationshipTypeService: RelationshipTypeService
              , public resourceService: ResourceService
              , public resourceAvailabilityService: ResourceAvailabilityService
              , public resourceAvailabilityChangeHistoryService: ResourceAvailabilityChangeHistoryService
              , public resourceChangeHistoryService: ResourceChangeHistoryService
              , public resourceContactService: ResourceContactService
              , public resourceContactChangeHistoryService: ResourceContactChangeHistoryService
              , public resourceQualificationService: ResourceQualificationService
              , public resourceQualificationChangeHistoryService: ResourceQualificationChangeHistoryService
              , public resourceShiftService: ResourceShiftService
              , public resourceShiftChangeHistoryService: ResourceShiftChangeHistoryService
              , public resourceTypeService: ResourceTypeService
              , public salutationService: SalutationService
              , public scheduledEventService: ScheduledEventService
              , public scheduledEventChangeHistoryService: ScheduledEventChangeHistoryService
              , public scheduledEventDependencyService: ScheduledEventDependencyService
              , public scheduledEventDependencyChangeHistoryService: ScheduledEventDependencyChangeHistoryService
              , public scheduledEventQualificationRequirementService: ScheduledEventQualificationRequirementService
              , public scheduledEventQualificationRequirementChangeHistoryService: ScheduledEventQualificationRequirementChangeHistoryService
              , public scheduledEventTemplateService: ScheduledEventTemplateService
              , public scheduledEventTemplateChangeHistoryService: ScheduledEventTemplateChangeHistoryService
              , public scheduledEventTemplateChargeService: ScheduledEventTemplateChargeService
              , public scheduledEventTemplateChargeChangeHistoryService: ScheduledEventTemplateChargeChangeHistoryService
              , public scheduledEventTemplateQualificationRequirementService: ScheduledEventTemplateQualificationRequirementService
              , public scheduledEventTemplateQualificationRequirementChangeHistoryService: ScheduledEventTemplateQualificationRequirementChangeHistoryService
              , public schedulingTargetService: SchedulingTargetService
              , public schedulingTargetAddressService: SchedulingTargetAddressService
              , public schedulingTargetAddressChangeHistoryService: SchedulingTargetAddressChangeHistoryService
              , public schedulingTargetChangeHistoryService: SchedulingTargetChangeHistoryService
              , public schedulingTargetContactService: SchedulingTargetContactService
              , public schedulingTargetContactChangeHistoryService: SchedulingTargetContactChangeHistoryService
              , public schedulingTargetQualificationRequirementService: SchedulingTargetQualificationRequirementService
              , public schedulingTargetQualificationRequirementChangeHistoryService: SchedulingTargetQualificationRequirementChangeHistoryService
              , public schedulingTargetTypeService: SchedulingTargetTypeService
              , public shiftPatternService: ShiftPatternService
              , public shiftPatternChangeHistoryService: ShiftPatternChangeHistoryService
              , public shiftPatternDayService: ShiftPatternDayService
              , public shiftPatternDayChangeHistoryService: ShiftPatternDayChangeHistoryService
              , public softCreditService: SoftCreditService
              , public softCreditChangeHistoryService: SoftCreditChangeHistoryService
              , public stateProvinceService: StateProvinceService
              , public tagService: TagService
              , public taxCodeService: TaxCodeService
              , public tenantProfileService: TenantProfileService
              , public tenantProfileChangeHistoryService: TenantProfileChangeHistoryService
              , public timeZoneService: TimeZoneService
              , public tributeService: TributeService
              , public tributeChangeHistoryService: TributeChangeHistoryService
              , public tributeTypeService: TributeTypeService
              , public volunteerGroupService: VolunteerGroupService
              , public volunteerGroupChangeHistoryService: VolunteerGroupChangeHistoryService
              , public volunteerGroupMemberService: VolunteerGroupMemberService
              , public volunteerGroupMemberChangeHistoryService: VolunteerGroupMemberChangeHistoryService
              , public volunteerProfileService: VolunteerProfileService
              , public volunteerProfileChangeHistoryService: VolunteerProfileChangeHistoryService
              , public volunteerStatusService: VolunteerStatusService
) { }  


    public ClearAllCaches() {

        this.accountTypeService.ClearAllCaches();
        this.appealService.ClearAllCaches();
        this.appealChangeHistoryService.ClearAllCaches();
        this.assignmentRoleService.ClearAllCaches();
        this.assignmentRoleQualificationRequirementService.ClearAllCaches();
        this.assignmentRoleQualificationRequirementChangeHistoryService.ClearAllCaches();
        this.assignmentStatusService.ClearAllCaches();
        this.attributeDefinitionService.ClearAllCaches();
        this.attributeDefinitionChangeHistoryService.ClearAllCaches();
        this.attributeDefinitionEntityService.ClearAllCaches();
        this.attributeDefinitionTypeService.ClearAllCaches();
        this.batchService.ClearAllCaches();
        this.batchChangeHistoryService.ClearAllCaches();
        this.batchStatusService.ClearAllCaches();
        this.bookingSourceTypeService.ClearAllCaches();
        this.budgetService.ClearAllCaches();
        this.budgetChangeHistoryService.ClearAllCaches();
        this.calendarService.ClearAllCaches();
        this.calendarChangeHistoryService.ClearAllCaches();
        this.campaignService.ClearAllCaches();
        this.campaignChangeHistoryService.ClearAllCaches();
        this.chargeStatusService.ClearAllCaches();
        this.chargeStatusChangeHistoryService.ClearAllCaches();
        this.chargeTypeService.ClearAllCaches();
        this.chargeTypeChangeHistoryService.ClearAllCaches();
        this.clientService.ClearAllCaches();
        this.clientChangeHistoryService.ClearAllCaches();
        this.clientContactService.ClearAllCaches();
        this.clientContactChangeHistoryService.ClearAllCaches();
        this.clientTypeService.ClearAllCaches();
        this.constituentService.ClearAllCaches();
        this.constituentChangeHistoryService.ClearAllCaches();
        this.constituentJourneyStageService.ClearAllCaches();
        this.constituentJourneyStageChangeHistoryService.ClearAllCaches();
        this.contactService.ClearAllCaches();
        this.contactChangeHistoryService.ClearAllCaches();
        this.contactContactService.ClearAllCaches();
        this.contactContactChangeHistoryService.ClearAllCaches();
        this.contactInteractionService.ClearAllCaches();
        this.contactInteractionChangeHistoryService.ClearAllCaches();
        this.contactMethodService.ClearAllCaches();
        this.contactTagService.ClearAllCaches();
        this.contactTagChangeHistoryService.ClearAllCaches();
        this.contactTypeService.ClearAllCaches();
        this.countryService.ClearAllCaches();
        this.crewService.ClearAllCaches();
        this.crewChangeHistoryService.ClearAllCaches();
        this.crewMemberService.ClearAllCaches();
        this.crewMemberChangeHistoryService.ClearAllCaches();
        this.currencyService.ClearAllCaches();
        this.dependencyTypeService.ClearAllCaches();
        this.documentService.ClearAllCaches();
        this.documentChangeHistoryService.ClearAllCaches();
        this.documentTypeService.ClearAllCaches();
        this.eventCalendarService.ClearAllCaches();
        this.eventChargeService.ClearAllCaches();
        this.eventChargeChangeHistoryService.ClearAllCaches();
        this.eventResourceAssignmentService.ClearAllCaches();
        this.eventResourceAssignmentChangeHistoryService.ClearAllCaches();
        this.eventStatusService.ClearAllCaches();
        this.financialCategoryService.ClearAllCaches();
        this.financialCategoryChangeHistoryService.ClearAllCaches();
        this.financialOfficeService.ClearAllCaches();
        this.financialOfficeChangeHistoryService.ClearAllCaches();
        this.financialTransactionService.ClearAllCaches();
        this.financialTransactionChangeHistoryService.ClearAllCaches();
        this.fiscalPeriodService.ClearAllCaches();
        this.fiscalPeriodChangeHistoryService.ClearAllCaches();
        this.fundService.ClearAllCaches();
        this.fundChangeHistoryService.ClearAllCaches();
        this.generalLedgerEntryService.ClearAllCaches();
        this.generalLedgerLineService.ClearAllCaches();
        this.giftService.ClearAllCaches();
        this.giftChangeHistoryService.ClearAllCaches();
        this.householdService.ClearAllCaches();
        this.householdChangeHistoryService.ClearAllCaches();
        this.iconService.ClearAllCaches();
        this.interactionTypeService.ClearAllCaches();
        this.invoiceService.ClearAllCaches();
        this.invoiceChangeHistoryService.ClearAllCaches();
        this.invoiceLineItemService.ClearAllCaches();
        this.invoiceStatusService.ClearAllCaches();
        this.notificationSubscriptionService.ClearAllCaches();
        this.notificationSubscriptionChangeHistoryService.ClearAllCaches();
        this.notificationTypeService.ClearAllCaches();
        this.officeService.ClearAllCaches();
        this.officeChangeHistoryService.ClearAllCaches();
        this.officeContactService.ClearAllCaches();
        this.officeContactChangeHistoryService.ClearAllCaches();
        this.officeTypeService.ClearAllCaches();
        this.paymentMethodService.ClearAllCaches();
        this.paymentProviderService.ClearAllCaches();
        this.paymentProviderChangeHistoryService.ClearAllCaches();
        this.paymentTransactionService.ClearAllCaches();
        this.paymentTransactionChangeHistoryService.ClearAllCaches();
        this.paymentTypeService.ClearAllCaches();
        this.paymentTypeChangeHistoryService.ClearAllCaches();
        this.periodStatusService.ClearAllCaches();
        this.pledgeService.ClearAllCaches();
        this.pledgeChangeHistoryService.ClearAllCaches();
        this.priorityService.ClearAllCaches();
        this.qualificationService.ClearAllCaches();
        this.rateSheetService.ClearAllCaches();
        this.rateSheetChangeHistoryService.ClearAllCaches();
        this.rateTypeService.ClearAllCaches();
        this.receiptService.ClearAllCaches();
        this.receiptChangeHistoryService.ClearAllCaches();
        this.receiptTypeService.ClearAllCaches();
        this.receiptTypeChangeHistoryService.ClearAllCaches();
        this.recurrenceExceptionService.ClearAllCaches();
        this.recurrenceExceptionChangeHistoryService.ClearAllCaches();
        this.recurrenceFrequencyService.ClearAllCaches();
        this.recurrenceRuleService.ClearAllCaches();
        this.recurrenceRuleChangeHistoryService.ClearAllCaches();
        this.relationshipTypeService.ClearAllCaches();
        this.resourceService.ClearAllCaches();
        this.resourceAvailabilityService.ClearAllCaches();
        this.resourceAvailabilityChangeHistoryService.ClearAllCaches();
        this.resourceChangeHistoryService.ClearAllCaches();
        this.resourceContactService.ClearAllCaches();
        this.resourceContactChangeHistoryService.ClearAllCaches();
        this.resourceQualificationService.ClearAllCaches();
        this.resourceQualificationChangeHistoryService.ClearAllCaches();
        this.resourceShiftService.ClearAllCaches();
        this.resourceShiftChangeHistoryService.ClearAllCaches();
        this.resourceTypeService.ClearAllCaches();
        this.salutationService.ClearAllCaches();
        this.scheduledEventService.ClearAllCaches();
        this.scheduledEventChangeHistoryService.ClearAllCaches();
        this.scheduledEventDependencyService.ClearAllCaches();
        this.scheduledEventDependencyChangeHistoryService.ClearAllCaches();
        this.scheduledEventQualificationRequirementService.ClearAllCaches();
        this.scheduledEventQualificationRequirementChangeHistoryService.ClearAllCaches();
        this.scheduledEventTemplateService.ClearAllCaches();
        this.scheduledEventTemplateChangeHistoryService.ClearAllCaches();
        this.scheduledEventTemplateChargeService.ClearAllCaches();
        this.scheduledEventTemplateChargeChangeHistoryService.ClearAllCaches();
        this.scheduledEventTemplateQualificationRequirementService.ClearAllCaches();
        this.scheduledEventTemplateQualificationRequirementChangeHistoryService.ClearAllCaches();
        this.schedulingTargetService.ClearAllCaches();
        this.schedulingTargetAddressService.ClearAllCaches();
        this.schedulingTargetAddressChangeHistoryService.ClearAllCaches();
        this.schedulingTargetChangeHistoryService.ClearAllCaches();
        this.schedulingTargetContactService.ClearAllCaches();
        this.schedulingTargetContactChangeHistoryService.ClearAllCaches();
        this.schedulingTargetQualificationRequirementService.ClearAllCaches();
        this.schedulingTargetQualificationRequirementChangeHistoryService.ClearAllCaches();
        this.schedulingTargetTypeService.ClearAllCaches();
        this.shiftPatternService.ClearAllCaches();
        this.shiftPatternChangeHistoryService.ClearAllCaches();
        this.shiftPatternDayService.ClearAllCaches();
        this.shiftPatternDayChangeHistoryService.ClearAllCaches();
        this.softCreditService.ClearAllCaches();
        this.softCreditChangeHistoryService.ClearAllCaches();
        this.stateProvinceService.ClearAllCaches();
        this.tagService.ClearAllCaches();
        this.taxCodeService.ClearAllCaches();
        this.tenantProfileService.ClearAllCaches();
        this.tenantProfileChangeHistoryService.ClearAllCaches();
        this.timeZoneService.ClearAllCaches();
        this.tributeService.ClearAllCaches();
        this.tributeChangeHistoryService.ClearAllCaches();
        this.tributeTypeService.ClearAllCaches();
        this.volunteerGroupService.ClearAllCaches();
        this.volunteerGroupChangeHistoryService.ClearAllCaches();
        this.volunteerGroupMemberService.ClearAllCaches();
        this.volunteerGroupMemberChangeHistoryService.ClearAllCaches();
        this.volunteerProfileService.ClearAllCaches();
        this.volunteerProfileChangeHistoryService.ClearAllCaches();
        this.volunteerStatusService.ClearAllCaches();
    }
}