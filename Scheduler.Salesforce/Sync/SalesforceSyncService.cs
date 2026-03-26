//
// SalesforceSyncService.cs
//
// Core orchestrator for bidirectional Salesforce synchronization.
// Handles pull (Salesforce -> Scheduler) and push (Scheduler -> Salesforce) operations
// for Client/Account, Contact/Contact, and ScheduledEvent/Event entities.
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration, following the RebrickableSyncService pattern.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Foundation.Scheduler.Database;
using Scheduler.Salesforce.Api;
using Scheduler.Salesforce.Auth;
using Scheduler.Salesforce.Models;


namespace Scheduler.Salesforce.Sync
{
    public class SalesforceSyncService
    {
        private readonly SchedulerContext _context;
        private readonly SalesforceClient _sfClient;
        private readonly SalesforceWebApiService _sfWebApiService;
        private readonly ITokenCacheService _tokenCacheService;
        private readonly ILogger<SalesforceSyncService> _logger;

        public const string TRIGGER_PERIODIC_PULL = "PeriodicPull";
        public const string TRIGGER_QUEUE_PROCESSOR = "QueueProcessor";
        public const string TRIGGER_MANUAL = "Manual";


        public SalesforceSyncService(
            SchedulerContext context,
            SalesforceClient sfClient,
            SalesforceWebApiService sfWebApiService,
            ITokenCacheService tokenCacheService,
            ILogger<SalesforceSyncService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _sfClient = sfClient ?? throw new ArgumentNullException(nameof(sfClient));
            _sfWebApiService = sfWebApiService ?? throw new ArgumentNullException(nameof(sfWebApiService));
            _tokenCacheService = tokenCacheService ?? throw new ArgumentNullException(nameof(tokenCacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        #region Pull Methods (Salesforce -> Scheduler)


        /// <summary>
        ///
        /// Pulls Salesforce Accounts and upserts them into the Client table.
        /// Uses externalId for match-or-create correlation.
        ///
        /// </summary>
        public async Task<SalesforceSyncResult> PullAccountsAsync(SalesforceConfig config, DateTime? modifiedSince = null, CancellationToken ct = default)
        {
            SalesforceSyncResult result = new SalesforceSyncResult();

            try
            {
                var (accessToken, instanceUrl) = await _sfWebApiService.GetValidTokenAsync(config, ct);

                string rawJson = await _sfClient.GetAccountsAsync(accessToken, instanceUrl, config.ApiVersion, modifiedSince, ct);

                SalesforceQueryResponse<AccountRecord> response = JsonSerializer.Deserialize<SalesforceQueryResponse<AccountRecord>>(rawJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (response?.records == null)
                {
                    _logger.LogWarning("PullAccountsAsync: No records in response for tenant {TenantGuid}", config.TenantGuid);
                    return result;
                }

                //
                // Find the default client type for this tenant
                //
                int defaultClientTypeId = await _context.ClientTypes
                    .Where(ct2 => ct2.active == true && ct2.deleted == false)
                    .OrderBy(ct2 => ct2.id)
                    .Select(ct2 => ct2.id)
                    .FirstOrDefaultAsync(ct);

                if (defaultClientTypeId == 0)
                {
                    _logger.LogError("PullAccountsAsync: No active client type found for tenant {TenantGuid}", config.TenantGuid);
                    result.Errors.Add("No active client type found.");
                    result.ErrorCount++;
                    return result;
                }

                foreach (AccountRecord record in response.records)
                {
                    try
                    {
                        await UpsertClientFromAccountAsync(record, config.TenantGuid, defaultClientTypeId, ct);

                        if (record.IsDeleted == true)
                        {
                            result.TotalSkipped++;
                        }
                        else
                        {
                            result.TotalCreated++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "PullAccountsAsync: Error processing account {AccountId}", record.Id);
                        result.Errors.Add($"Account {record.Id}: {ex.Message}");
                        result.ErrorCount++;
                    }
                }

                _logger.LogInformation("PullAccountsAsync completed for tenant {TenantGuid}: created/updated={Count}, errors={Errors}",
                    config.TenantGuid, result.TotalCreated, result.ErrorCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PullAccountsAsync: Unhandled exception for tenant {TenantGuid}", config.TenantGuid);
                result.Errors.Add(ex.Message);
                result.ErrorCount++;
            }

            return result;
        }


        /// <summary>
        ///
        /// Pulls Salesforce Contacts and upserts them into the Contact table.
        /// Uses externalId for match-or-create correlation.
        /// Also creates ClientContact join records when AccountId is present.
        ///
        /// </summary>
        public async Task<SalesforceSyncResult> PullContactsAsync(SalesforceConfig config, DateTime? modifiedSince = null, CancellationToken ct = default)
        {
            SalesforceSyncResult result = new SalesforceSyncResult();

            try
            {
                var (accessToken, instanceUrl) = await _sfWebApiService.GetValidTokenAsync(config, ct);

                string rawJson = await _sfClient.GetContactsAsync(accessToken, instanceUrl, config.ApiVersion, modifiedSince, ct);

                SalesforceQueryResponse<ContactRecord> response = JsonSerializer.Deserialize<SalesforceQueryResponse<ContactRecord>>(rawJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (response?.records == null)
                {
                    _logger.LogWarning("PullContactsAsync: No records in response for tenant {TenantGuid}", config.TenantGuid);
                    return result;
                }

                //
                // Find the default contact type for this tenant
                //
                int defaultContactTypeId = await _context.ContactTypes
                    .Where(ct2 => ct2.active == true && ct2.deleted == false)
                    .OrderBy(ct2 => ct2.id)
                    .Select(ct2 => ct2.id)
                    .FirstOrDefaultAsync(ct);

                if (defaultContactTypeId == 0)
                {
                    _logger.LogError("PullContactsAsync: No active contact type found for tenant {TenantGuid}", config.TenantGuid);
                    result.Errors.Add("No active contact type found.");
                    result.ErrorCount++;
                    return result;
                }

                foreach (ContactRecord record in response.records)
                {
                    try
                    {
                        await UpsertContactFromRecordAsync(record, config.TenantGuid, defaultContactTypeId, ct);
                        result.TotalCreated++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "PullContactsAsync: Error processing contact {ContactId}", record.Id);
                        result.Errors.Add($"Contact {record.Id}: {ex.Message}");
                        result.ErrorCount++;
                    }
                }

                _logger.LogInformation("PullContactsAsync completed for tenant {TenantGuid}: created/updated={Count}, errors={Errors}",
                    config.TenantGuid, result.TotalCreated, result.ErrorCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PullContactsAsync: Unhandled exception for tenant {TenantGuid}", config.TenantGuid);
                result.Errors.Add(ex.Message);
                result.ErrorCount++;
            }

            return result;
        }


        /// <summary>
        ///
        /// Pulls Salesforce Events and upserts them into the ScheduledEvent table.
        /// Uses externalId for match-or-create correlation.
        ///
        /// </summary>
        public async Task<SalesforceSyncResult> PullEventsAsync(SalesforceConfig config, DateTime? modifiedSince = null, CancellationToken ct = default)
        {
            SalesforceSyncResult result = new SalesforceSyncResult();

            try
            {
                var (accessToken, instanceUrl) = await _sfWebApiService.GetValidTokenAsync(config, ct);

                string rawJson = await _sfClient.GetEventsAsync(accessToken, instanceUrl, config.ApiVersion, modifiedSince, ct);

                SalesforceQueryResponse<EventRecord> response = JsonSerializer.Deserialize<SalesforceQueryResponse<EventRecord>>(rawJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (response?.records == null)
                {
                    _logger.LogWarning("PullEventsAsync: No records in response for tenant {TenantGuid}", config.TenantGuid);
                    return result;
                }

                foreach (EventRecord record in response.records)
                {
                    try
                    {
                        await UpsertScheduledEventFromRecordAsync(record, config.TenantGuid, ct);
                        result.TotalCreated++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "PullEventsAsync: Error processing event {EventId}", record.Id);
                        result.Errors.Add($"Event {record.Id}: {ex.Message}");
                        result.ErrorCount++;
                    }
                }

                _logger.LogInformation("PullEventsAsync completed for tenant {TenantGuid}: created/updated={Count}, errors={Errors}",
                    config.TenantGuid, result.TotalCreated, result.ErrorCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PullEventsAsync: Unhandled exception for tenant {TenantGuid}", config.TenantGuid);
                result.Errors.Add(ex.Message);
                result.ErrorCount++;
            }

            return result;
        }


        /// <summary>
        ///
        /// Runs all three pull operations in sequence.
        ///
        /// </summary>
        public async Task<SalesforceSyncResult> PullAllAsync(SalesforceConfig config, DateTime? modifiedSince = null, CancellationToken ct = default)
        {
            SalesforceSyncResult combined = new SalesforceSyncResult();

            SalesforceSyncResult accountResult = await PullAccountsAsync(config, modifiedSince, ct);
            combined.TotalCreated += accountResult.TotalCreated;
            combined.TotalUpdated += accountResult.TotalUpdated;
            combined.ErrorCount += accountResult.ErrorCount;
            combined.Errors.AddRange(accountResult.Errors);

            SalesforceSyncResult contactResult = await PullContactsAsync(config, modifiedSince, ct);
            combined.TotalCreated += contactResult.TotalCreated;
            combined.TotalUpdated += contactResult.TotalUpdated;
            combined.ErrorCount += contactResult.ErrorCount;
            combined.Errors.AddRange(contactResult.Errors);

            SalesforceSyncResult eventResult = await PullEventsAsync(config, modifiedSince, ct);
            combined.TotalCreated += eventResult.TotalCreated;
            combined.TotalUpdated += eventResult.TotalUpdated;
            combined.ErrorCount += eventResult.ErrorCount;
            combined.Errors.AddRange(eventResult.Errors);

            return combined;
        }


        #endregion


        #region Push Methods (Scheduler -> Salesforce)


        /// <summary>
        ///
        /// Pushes a Client to Salesforce as an Account (Create).
        /// Returns the Salesforce ID of the created Account.
        ///
        /// </summary>
        public async Task<string> PushClientCreatedAsync(Guid tenantGuid, int clientId, string trigger = null, CancellationToken ct = default)
        {
            SalesforceConfig config = await LoadConfigForTenantAsync(tenantGuid, ct);
            if (config == null) return null;

            var (accessToken, instanceUrl) = await _sfWebApiService.GetValidTokenAsync(config, ct);

            Client client = await _context.Clients.FirstOrDefaultAsync(c => c.id == clientId && c.tenantGuid == tenantGuid && c.deleted == false, ct);
            if (client == null) return null;

            string payload = JsonSerializer.Serialize(new
            {
                Name = client.name ?? "",
                Phone = client.phone ?? "",
                Website = client.email ?? "",
                BillingStreet = client.addressLine1 ?? "",
                BillingCity = client.city ?? "",
                BillingPostalCode = client.postalCode ?? ""
            });

            string responseJson = await _sfClient.CreateSObjectAsync(accessToken, instanceUrl, "Account", payload, config.ApiVersion, ct);

            //
            // Parse the Salesforce ID from the response and write it back to externalId
            //
            JsonDocument responseDoc = JsonDocument.Parse(responseJson);
            string salesforceId = responseDoc.RootElement.GetProperty("id").GetString();

            if (string.IsNullOrEmpty(salesforceId) == false)
            {
                client.externalId = salesforceId;
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("PushClientCreated: Client {ClientId} -> SF Account {SfId} (trigger: {Trigger})", clientId, salesforceId, trigger);
            }

            return salesforceId;
        }


        /// <summary>
        ///
        /// Pushes a Client update to Salesforce Account.
        ///
        /// </summary>
        public async Task PushClientUpdatedAsync(Guid tenantGuid, int clientId, string trigger = null, CancellationToken ct = default)
        {
            SalesforceConfig config = await LoadConfigForTenantAsync(tenantGuid, ct);
            if (config == null) return;

            var (accessToken, instanceUrl) = await _sfWebApiService.GetValidTokenAsync(config, ct);

            Client client = await _context.Clients.FirstOrDefaultAsync(c => c.id == clientId && c.tenantGuid == tenantGuid, ct);
            if (client == null || string.IsNullOrEmpty(client.externalId) == true) return;

            string payload = JsonSerializer.Serialize(new
            {
                Name = client.name ?? "",
                Phone = client.phone ?? "",
                Website = client.email ?? "",
                BillingStreet = client.addressLine1 ?? "",
                BillingCity = client.city ?? "",
                BillingPostalCode = client.postalCode ?? ""
            });

            await _sfClient.UpdateSObjectAsync(accessToken, instanceUrl, "Account", client.externalId, payload, config.ApiVersion, ct);

            _logger.LogInformation("PushClientUpdated: Client {ClientId} -> SF Account {SfId} (trigger: {Trigger})", clientId, client.externalId, trigger);
        }


        /// <summary>
        ///
        /// Pushes a Client deletion to Salesforce (soft-delete the Account).
        ///
        /// </summary>
        public async Task PushClientDeletedAsync(Guid tenantGuid, string salesforceId, string trigger = null, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(salesforceId) == true) return;

            SalesforceConfig config = await LoadConfigForTenantAsync(tenantGuid, ct);
            if (config == null) return;

            var (accessToken, instanceUrl) = await _sfWebApiService.GetValidTokenAsync(config, ct);

            await _sfClient.DeleteSObjectAsync(accessToken, instanceUrl, "Account", salesforceId, config.ApiVersion, ct);

            _logger.LogInformation("PushClientDeleted: SF Account {SfId} (trigger: {Trigger})", salesforceId, trigger);
        }


        /// <summary>
        ///
        /// Pushes a Contact to Salesforce (Create).
        /// Returns the Salesforce ID of the created Contact.
        ///
        /// </summary>
        public async Task<string> PushContactCreatedAsync(Guid tenantGuid, int contactId, string trigger = null, CancellationToken ct = default)
        {
            SalesforceConfig config = await LoadConfigForTenantAsync(tenantGuid, ct);
            if (config == null) return null;

            var (accessToken, instanceUrl) = await _sfWebApiService.GetValidTokenAsync(config, ct);

            Contact contact = await _context.Contacts.FirstOrDefaultAsync(c => c.id == contactId && c.tenantGuid == tenantGuid && c.deleted == false, ct);
            if (contact == null) return null;

            //
            // Look up the parent Client's Salesforce Account ID if the contact is linked via ClientContact
            //
            string accountId = null;
            ClientContact clientContact = await _context.ClientContacts
                .Include(cc => cc.client)
                .Where(cc => cc.contactId == contactId && cc.tenantGuid == tenantGuid && cc.deleted == false)
                .FirstOrDefaultAsync(ct);

            if (clientContact?.client != null && string.IsNullOrEmpty(clientContact.client.externalId) == false)
            {
                accountId = clientContact.client.externalId;
            }

            var payloadObj = new Dictionary<string, object>
            {
                { "FirstName", contact.firstName ?? "" },
                { "LastName", contact.lastName ?? "" },
                { "Email", contact.email ?? "" },
                { "Phone", contact.phone ?? "" },
                { "MobilePhone", contact.mobile ?? "" },
                { "Title", contact.title ?? "" }
            };

            if (string.IsNullOrEmpty(accountId) == false)
            {
                payloadObj["AccountId"] = accountId;
            }

            string payload = JsonSerializer.Serialize(payloadObj);
            string responseJson = await _sfClient.CreateSObjectAsync(accessToken, instanceUrl, "Contact", payload, config.ApiVersion, ct);

            JsonDocument responseDoc = JsonDocument.Parse(responseJson);
            string salesforceId = responseDoc.RootElement.GetProperty("id").GetString();

            if (string.IsNullOrEmpty(salesforceId) == false)
            {
                contact.externalId = salesforceId;
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("PushContactCreated: Contact {ContactId} -> SF Contact {SfId} (trigger: {Trigger})", contactId, salesforceId, trigger);
            }

            return salesforceId;
        }


        /// <summary>
        ///
        /// Pushes a Contact update to Salesforce.
        ///
        /// </summary>
        public async Task PushContactUpdatedAsync(Guid tenantGuid, int contactId, string trigger = null, CancellationToken ct = default)
        {
            SalesforceConfig config = await LoadConfigForTenantAsync(tenantGuid, ct);
            if (config == null) return;

            var (accessToken, instanceUrl) = await _sfWebApiService.GetValidTokenAsync(config, ct);

            Contact contact = await _context.Contacts.FirstOrDefaultAsync(c => c.id == contactId && c.tenantGuid == tenantGuid, ct);
            if (contact == null || string.IsNullOrEmpty(contact.externalId) == true) return;

            string payload = JsonSerializer.Serialize(new
            {
                FirstName = contact.firstName ?? "",
                LastName = contact.lastName ?? "",
                Email = contact.email ?? "",
                Phone = contact.phone ?? "",
                MobilePhone = contact.mobile ?? "",
                Title = contact.title ?? ""
            });

            await _sfClient.UpdateSObjectAsync(accessToken, instanceUrl, "Contact", contact.externalId, payload, config.ApiVersion, ct);

            _logger.LogInformation("PushContactUpdated: Contact {ContactId} -> SF Contact {SfId} (trigger: {Trigger})", contactId, contact.externalId, trigger);
        }


        /// <summary>
        ///
        /// Pushes a Contact deletion to Salesforce.
        ///
        /// </summary>
        public async Task PushContactDeletedAsync(Guid tenantGuid, string salesforceId, string trigger = null, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(salesforceId) == true) return;

            SalesforceConfig config = await LoadConfigForTenantAsync(tenantGuid, ct);
            if (config == null) return;

            var (accessToken, instanceUrl) = await _sfWebApiService.GetValidTokenAsync(config, ct);

            await _sfClient.DeleteSObjectAsync(accessToken, instanceUrl, "Contact", salesforceId, config.ApiVersion, ct);

            _logger.LogInformation("PushContactDeleted: SF Contact {SfId} (trigger: {Trigger})", salesforceId, trigger);
        }


        /// <summary>
        ///
        /// Pushes a ScheduledEvent to Salesforce as an Event (Create).
        /// Returns the Salesforce ID of the created Event.
        ///
        /// </summary>
        public async Task<string> PushEventCreatedAsync(Guid tenantGuid, int eventId, string trigger = null, CancellationToken ct = default)
        {
            SalesforceConfig config = await LoadConfigForTenantAsync(tenantGuid, ct);
            if (config == null) return null;

            var (accessToken, instanceUrl) = await _sfWebApiService.GetValidTokenAsync(config, ct);

            ScheduledEvent scheduledEvent = await _context.ScheduledEvents
                .Include(e => e.client)
                .FirstOrDefaultAsync(e => e.id == eventId && e.tenantGuid == tenantGuid && e.deleted == false, ct);

            if (scheduledEvent == null) return null;

            var payloadObj = new Dictionary<string, object>
            {
                { "Subject", scheduledEvent.name ?? "" },
                { "Description", scheduledEvent.description ?? "" },
                { "StartDateTime", scheduledEvent.startDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "EndDateTime", scheduledEvent.endDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "Location", scheduledEvent.location ?? "" },
                { "IsAllDayEvent", scheduledEvent.isAllDay == true }
            };

            //
            // Link to the SF Account if the event has a client with an externalId
            //
            if (scheduledEvent.client != null && string.IsNullOrEmpty(scheduledEvent.client.externalId) == false)
            {
                payloadObj["WhatId"] = scheduledEvent.client.externalId;
            }

            string payload = JsonSerializer.Serialize(payloadObj);
            string responseJson = await _sfClient.CreateSObjectAsync(accessToken, instanceUrl, "Event", payload, config.ApiVersion, ct);

            JsonDocument responseDoc = JsonDocument.Parse(responseJson);
            string salesforceId = responseDoc.RootElement.GetProperty("id").GetString();

            if (string.IsNullOrEmpty(salesforceId) == false)
            {
                scheduledEvent.externalId = salesforceId;
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("PushEventCreated: ScheduledEvent {EventId} -> SF Event {SfId} (trigger: {Trigger})", eventId, salesforceId, trigger);
            }

            return salesforceId;
        }


        /// <summary>
        ///
        /// Pushes a ScheduledEvent update to Salesforce.
        ///
        /// </summary>
        public async Task PushEventUpdatedAsync(Guid tenantGuid, int eventId, string trigger = null, CancellationToken ct = default)
        {
            SalesforceConfig config = await LoadConfigForTenantAsync(tenantGuid, ct);
            if (config == null) return;

            var (accessToken, instanceUrl) = await _sfWebApiService.GetValidTokenAsync(config, ct);

            ScheduledEvent scheduledEvent = await _context.ScheduledEvents
                .Include(e => e.client)
                .FirstOrDefaultAsync(e => e.id == eventId && e.tenantGuid == tenantGuid, ct);

            if (scheduledEvent == null || string.IsNullOrEmpty(scheduledEvent.externalId) == true) return;

            var payloadObj = new Dictionary<string, object>
            {
                { "Subject", scheduledEvent.name ?? "" },
                { "Description", scheduledEvent.description ?? "" },
                { "StartDateTime", scheduledEvent.startDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "EndDateTime", scheduledEvent.endDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "Location", scheduledEvent.location ?? "" },
                { "IsAllDayEvent", scheduledEvent.isAllDay == true }
            };

            string payload = JsonSerializer.Serialize(payloadObj);

            await _sfClient.UpdateSObjectAsync(accessToken, instanceUrl, "Event", scheduledEvent.externalId, payload, config.ApiVersion, ct);

            _logger.LogInformation("PushEventUpdated: ScheduledEvent {EventId} -> SF Event {SfId} (trigger: {Trigger})", eventId, scheduledEvent.externalId, trigger);
        }


        /// <summary>
        ///
        /// Pushes a ScheduledEvent deletion to Salesforce.
        ///
        /// </summary>
        public async Task PushEventDeletedAsync(Guid tenantGuid, string salesforceId, string trigger = null, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(salesforceId) == true) return;

            SalesforceConfig config = await LoadConfigForTenantAsync(tenantGuid, ct);
            if (config == null) return;

            var (accessToken, instanceUrl) = await _sfWebApiService.GetValidTokenAsync(config, ct);

            await _sfClient.DeleteSObjectAsync(accessToken, instanceUrl, "Event", salesforceId, config.ApiVersion, ct);

            _logger.LogInformation("PushEventDeleted: SF Event {SfId} (trigger: {Trigger})", salesforceId, trigger);
        }


        #endregion


        #region Private Upsert Helpers


        private async Task UpsertClientFromAccountAsync(AccountRecord record, Guid tenantGuid, int defaultClientTypeId, CancellationToken ct)
        {
            Client existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.externalId == record.Id && c.tenantGuid == tenantGuid, ct);

            if (existingClient != null)
            {
                bool hasChange = false;

                if (existingClient.name != record.Name)
                {
                    existingClient.name = record.Name;
                    hasChange = true;
                }

                if (existingClient.phone != record.Phone && string.IsNullOrEmpty(record.Phone) == false)
                {
                    existingClient.phone = record.Phone;
                    hasChange = true;
                }

                if (existingClient.addressLine1 != record.BillingStreet && string.IsNullOrEmpty(record.BillingStreet) == false)
                {
                    existingClient.addressLine1 = record.BillingStreet;
                    hasChange = true;
                }

                if (existingClient.city != record.BillingCity && string.IsNullOrEmpty(record.BillingCity) == false)
                {
                    existingClient.city = record.BillingCity;
                    hasChange = true;
                }

                if (existingClient.postalCode != record.BillingPostalCode && string.IsNullOrEmpty(record.BillingPostalCode) == false)
                {
                    existingClient.postalCode = record.BillingPostalCode;
                    hasChange = true;
                }

                if (hasChange == true)
                {
                    existingClient.versionNumber++;
                    await _context.SaveChangesAsync(ct);
                    _logger.LogInformation("Updated Client {ClientId} from SF Account {SfId}", existingClient.id, record.Id);
                }
            }
            else
            {
                Client newClient = new Client
                {
                    id = 0,
                    tenantGuid = tenantGuid,
                    name = record.Name ?? "",
                    phone = record.Phone ?? "",
                    addressLine1 = record.BillingStreet ?? "",
                    city = record.BillingCity ?? "",
                    postalCode = record.BillingPostalCode ?? "",
                    clientTypeId = defaultClientTypeId,
                    externalId = record.Id,
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.Clients.Add(newClient);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Created Client {ClientId} from SF Account {SfId}", newClient.id, record.Id);
            }
        }


        private async Task UpsertContactFromRecordAsync(ContactRecord record, Guid tenantGuid, int defaultContactTypeId, CancellationToken ct)
        {
            Contact existingContact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.externalId == record.Id && c.tenantGuid == tenantGuid, ct);

            if (existingContact != null)
            {
                bool hasChange = false;

                if (existingContact.firstName != record.FirstName)
                {
                    existingContact.firstName = record.FirstName;
                    hasChange = true;
                }

                if (existingContact.lastName != record.LastName)
                {
                    existingContact.lastName = record.LastName;
                    hasChange = true;
                }

                if (existingContact.email != record.Email)
                {
                    existingContact.email = record.Email;
                    hasChange = true;
                }

                if (existingContact.phone != record.Phone && string.IsNullOrEmpty(record.Phone) == false)
                {
                    existingContact.phone = record.Phone;
                    hasChange = true;
                }

                if (existingContact.mobile != record.MobilePhone && string.IsNullOrEmpty(record.MobilePhone) == false)
                {
                    existingContact.mobile = record.MobilePhone;
                    hasChange = true;
                }

                if (existingContact.title != record.Title && string.IsNullOrEmpty(record.Title) == false)
                {
                    existingContact.title = record.Title;
                    hasChange = true;
                }

                if (hasChange == true)
                {
                    existingContact.versionNumber++;
                    await _context.SaveChangesAsync(ct);
                    _logger.LogInformation("Updated Contact {ContactId} from SF Contact {SfId}", existingContact.id, record.Id);
                }

                //
                // Ensure ClientContact join record exists if AccountId is present
                //
                await EnsureClientContactLinkAsync(existingContact.id, record.AccountId, tenantGuid, ct);
            }
            else
            {
                Contact newContact = new Contact
                {
                    id = 0,
                    tenantGuid = tenantGuid,
                    firstName = record.FirstName ?? "",
                    lastName = record.LastName ?? "",
                    email = record.Email ?? "",
                    phone = record.Phone ?? "",
                    mobile = record.MobilePhone ?? "",
                    title = record.Title ?? "",
                    company = record.Account?.Name ?? "",
                    contactTypeId = defaultContactTypeId,
                    externalId = record.Id,
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.Contacts.Add(newContact);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Created Contact {ContactId} from SF Contact {SfId}", newContact.id, record.Id);

                //
                // Create ClientContact join record if AccountId is present
                //
                await EnsureClientContactLinkAsync(newContact.id, record.AccountId, tenantGuid, ct);
            }
        }


        private async Task UpsertScheduledEventFromRecordAsync(EventRecord record, Guid tenantGuid, CancellationToken ct)
        {
            ScheduledEvent existingEvent = await _context.ScheduledEvents
                .FirstOrDefaultAsync(e => e.externalId == record.Id && e.tenantGuid == tenantGuid, ct);

            //
            // Resolve the Client from the WhatId (Salesforce Account ID)
            //
            int? clientId = null;
            if (string.IsNullOrEmpty(record.WhatId) == false)
            {
                Client linkedClient = await _context.Clients
                    .FirstOrDefaultAsync(c => c.externalId == record.WhatId && c.tenantGuid == tenantGuid, ct);

                if (linkedClient != null)
                {
                    clientId = linkedClient.id;
                }
            }

            //
            // Find the default event status for this tenant
            //
            int defaultEventStatusId = await _context.EventStatuses
                .Where(es => es.active == true && es.deleted == false)
                .OrderBy(es => es.id)
                .Select(es => es.id)
                .FirstOrDefaultAsync(ct);

            if (existingEvent != null)
            {
                bool hasChange = false;

                if (existingEvent.name != record.Subject)
                {
                    existingEvent.name = record.Subject;
                    hasChange = true;
                }

                if (existingEvent.description != record.Description)
                {
                    existingEvent.description = record.Description;
                    hasChange = true;
                }

                if (record.StartDateTime.HasValue == true && existingEvent.startDateTime != record.StartDateTime.Value)
                {
                    existingEvent.startDateTime = record.StartDateTime.Value;
                    hasChange = true;
                }

                if (record.EndDateTime.HasValue == true && existingEvent.endDateTime != record.EndDateTime.Value)
                {
                    existingEvent.endDateTime = record.EndDateTime.Value;
                    hasChange = true;
                }

                if (existingEvent.location != record.Location)
                {
                    existingEvent.location = record.Location;
                    hasChange = true;
                }

                if (existingEvent.clientId != clientId && clientId.HasValue == true)
                {
                    existingEvent.clientId = clientId;
                    hasChange = true;
                }

                if (hasChange == true)
                {
                    existingEvent.versionNumber++;
                    await _context.SaveChangesAsync(ct);
                    _logger.LogInformation("Updated ScheduledEvent {EventId} from SF Event {SfId}", existingEvent.id, record.Id);
                }
            }
            else
            {
                if (record.StartDateTime.HasValue == false || record.EndDateTime.HasValue == false)
                {
                    _logger.LogWarning("Skipping SF Event {SfId} — missing start or end date", record.Id);
                    return;
                }

                ScheduledEvent newEvent = new ScheduledEvent
                {
                    id = 0,
                    tenantGuid = tenantGuid,
                    name = record.Subject ?? "",
                    description = record.Description ?? "",
                    startDateTime = record.StartDateTime.Value,
                    endDateTime = record.EndDateTime.Value,
                    location = record.Location ?? "",
                    isAllDay = record.IsAllDayEvent,
                    clientId = clientId,
                    eventStatusId = defaultEventStatusId > 0 ? defaultEventStatusId : 1,
                    externalId = record.Id,
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.ScheduledEvents.Add(newEvent);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Created ScheduledEvent {EventId} from SF Event {SfId}", newEvent.id, record.Id);
            }
        }


        private async Task EnsureClientContactLinkAsync(int contactId, string salesforceAccountId, Guid tenantGuid, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(salesforceAccountId) == true) return;

            Client client = await _context.Clients
                .FirstOrDefaultAsync(c => c.externalId == salesforceAccountId && c.tenantGuid == tenantGuid, ct);

            if (client == null) return;

            bool linkExists = await _context.ClientContacts
                .AnyAsync(cc => cc.clientId == client.id && cc.contactId == contactId && cc.tenantGuid == tenantGuid && cc.deleted == false, ct);

            if (linkExists == false)
            {
                //
                // Resolve the default relationship type
                //
                int defaultRelTypeId = await _context.RelationshipTypes
                    .Where(rt => rt.active == true && rt.deleted == false)
                    .OrderBy(rt => rt.id)
                    .Select(rt => rt.id)
                    .FirstOrDefaultAsync(ct);

                if (defaultRelTypeId == 0) return;

                ClientContact newLink = new ClientContact
                {
                    tenantGuid = tenantGuid,
                    clientId = client.id,
                    contactId = contactId,
                    isPrimary = false,
                    relationshipTypeId = defaultRelTypeId,
                    versionNumber = 1,
                    objectGuid = Guid.NewGuid(),
                    active = true,
                    deleted = false
                };

                _context.ClientContacts.Add(newLink);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Created ClientContact link: Client {ClientId} <-> Contact {ContactId}", client.id, contactId);
            }
        }


        #endregion


        #region Config Helpers


        /// <summary>
        ///
        /// Loads the Salesforce configuration for a tenant from the SalesforceTenantLinks table.
        ///
        /// </summary>
        public async Task<SalesforceConfig> LoadConfigForTenantAsync(Guid tenantGuid, CancellationToken ct = default)
        {
            SalesforceTenantLink link = await _context.SalesforceTenantLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.tenantGuid == tenantGuid && l.active == true && l.deleted == false && l.syncEnabled == true, ct);

            if (link == null)
            {
                _logger.LogDebug("No active SalesforceTenantLink found for tenant {TenantGuid}", tenantGuid);
                return null;
            }

            return new SalesforceConfig
            {
                TenantGuid = tenantGuid,
                LoginUrl = link.loginUrl,
                ClientId = link.sfClientId,
                ClientSecret = link.sfClientSecret,
                Username = link.sfUsername,
                Password = link.sfPassword,
                SecurityToken = link.sfSecurityToken,
                ApiVersion = link.apiVersion ?? "v56.0",
                SyncDirectionFlags = link.syncDirectionFlags ?? SyncDirection.None,
                PullIntervalMinutes = link.pullIntervalMinutes ?? 5
            };
        }


        #endregion
    }
}
