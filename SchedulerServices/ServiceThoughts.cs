/* these are a bunch of ideas I watn to revisit when building the business logic/

 QuickBooks (API)

Use QuickBooksIntegrator to create JournalEntries (for expenses/revenue) or Invoices (for billable events).
Batch export example:C#public async Task ExportPendingChargesToQuickBooks()
{
    var pendingCharges = EventChargeService.GetPendingCharges(); // WHERE exportedDate IS NULL AND active=1 AND deleted=0

    foreach (var charge in pendingCharges)
    {
        var qboEntry = await quickBooksIntegrator.SyncChargeAsJournalEntry(charge);
        charge.ExternalId = qboEntry.Id;
        charge.ExportedDate = DateTime.UtcNow;
        EventChargeService.UpdateCharge(charge); // Save back
    }
}
 
 
 * 
 * /// <summary>
/// Traditional method to create a ScheduledEvent from a template.
/// Automatically drops default charges from the template to the new event.
/// Allows manual overrides/additions.
/// </summary>
/// <param>templateId</param> // ID of the ScheduledEventTemplate to use
/// <param>otherEventData</param> // Other event details (start/end, etc.)
/// <param>manualCharges</param> // Optional dictionary of manual charge overrides/adds (key: chargeTypeId, value: amount)
/// <returns>The ID of the created ScheduledEvent</returns>
public int CreateEventFromTemplate(int templateId, ScheduledEvent otherEventData, Dictionary<int, decimal> manualCharges = null)
{
    // Step 1: Load template (assume synchronous for simplicity; use async if needed)
    ScheduledEventTemplate template = ScheduledEventTemplateService.GetTemplate(templateId);

    if (template == null)
    {
        throw new InvalidOperationException("Template not found");
    }

    // Step 2: Create the base event
    int newEventId = ScheduledEventService.CreateEvent(otherEventData);

    // Step 3: Auto-drop charges from template
    List<EventTypeCharge> defaultCharges = EventTypeChargeService.GetChargesForTemplate(templateId);
    foreach (EventTypeCharge defaultCharge in defaultCharges)
    {
        decimal amount = defaultCharge.DefaultAmount;

        // Apply manual override if provided
        if (manualCharges != null && manualCharges.ContainsKey(defaultCharge.ChargeTypeId))
        {
            amount = manualCharges[defaultCharge.ChargeTypeId];
            manualCharges.Remove(defaultCharge.ChargeTypeId); // Remove so we can add extras later
        }

        // Create the event charge
        EventCharge newCharge = new EventCharge
        {
            ScheduledEventId = newEventId,
            ChargeTypeId = defaultCharge.ChargeTypeId,
            Amount = amount,
            CurrencyId = defaultCharge.CurrencyId, // From ChargeType
            RateTypeId = defaultCharge.RateTypeId, // From ChargeType
            IsAutomatic = true
        };

        EventChargeService.AddCharge(newCharge);
    }

    // Step 4: Add any remaining manual charges (extras not in template)
    if (manualCharges != null && manualCharges.Count > 0)
    {
        foreach (var kvp in manualCharges)
        {
            EventCharge manualCharge = new EventCharge
            {
                ScheduledEventId = newEventId,
                ChargeTypeId = kvp.Key,
                Amount = kvp.Value,
                CurrencyId = 1, // Default currency ID — replace with logic to get from template or user
                RateTypeId = null, // Optional for manual
                IsAutomatic = false
            };

            EventChargeService.AddCharge(manualCharge);
        }
    }

    return newEventId;
}



using OfficeOpenXml;

// Example: Export all charges for a batch of events to Excel
public void ExportEventChargesToExcel(int[] eventIds, string filePath)
{
    var charges = EventChargeService.GetChargesForEvents(eventIds);

    using (var package = new ExcelPackage(new FileInfo(filePath)))
    {
        var worksheet = package.Workbook.Worksheets.Add("Event Charges");

        // Header row
        worksheet.Cells[1, 1].Value = "Event ID";
        worksheet.Cells[1, 2].Value = "Charge Type";
        worksheet.Cells[1, 3].Value = "Amount";
        worksheet.Cells[1, 4].Value = "Currency";
        worksheet.Cells[1, 5].Value = "Is Automatic";

        // Data rows
        int row = 2;
        foreach (var charge in charges)
        {
            worksheet.Cells[row, 1].Value = charge.ScheduledEventId;
            worksheet.Cells[row, 2].Value = charge.ChargeType.Name; // Assume navigation property
            worksheet.Cells[row, 3].Value = charge.Amount;
            worksheet.Cells[row, 4].Value = charge.Currency.Code; // Assume navigation
            worksheet.Cells[row, 5].Value = charge.IsAutomatic ? "Yes" : "No";
            row++;
        }

        package.Save();
    }
}
*/