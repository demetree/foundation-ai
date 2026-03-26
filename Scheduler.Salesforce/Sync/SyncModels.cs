//
// SyncModels.cs
//
// DTOs for deserializing Salesforce REST API responses into strongly-typed records.
// These map Salesforce JSON into the shapes needed by SalesforceSyncService.
//
// AI Assisted Development:  This file was created with AI assistance for the
// Scheduler Salesforce integration.
//

using System;
using System.Collections.Generic;


namespace Scheduler.Salesforce.Sync
{
    #region Response Wrappers


    public class SalesforceQueryResponse<T>
    {
        public int totalSize { get; set; }

        public bool done { get; set; }

        public List<T> records { get; set; }
    }


    #endregion


    #region Account (maps to Client)


    public class AccountRecord
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Website { get; set; }

        public string Industry { get; set; }

        public string Phone { get; set; }

        public string Type { get; set; }

        public string BillingStreet { get; set; }

        public string BillingCity { get; set; }

        public string BillingState { get; set; }

        public string BillingPostalCode { get; set; }

        public string BillingCountry { get; set; }

        public bool IsDeleted { get; set; }
    }


    #endregion


    #region Contact (maps to Contact)


    public class ContactRecord
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string MobilePhone { get; set; }

        public string Title { get; set; }

        public bool IsDeleted { get; set; }

        public string AccountId { get; set; }

        public AccountReference Account { get; set; }
    }


    public class AccountReference
    {
        public string Name { get; set; }
    }


    #endregion


    #region Event (maps to ScheduledEvent)


    public class EventRecord
    {
        public string Id { get; set; }

        public string Subject { get; set; }

        public string Description { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public string Location { get; set; }

        public string WhatId { get; set; }

        public bool IsAllDayEvent { get; set; }

        public bool IsDeleted { get; set; }
    }


    #endregion


    #region Sync Results


    public class SalesforceSyncResult
    {
        public int TotalCreated { get; set; }

        public int TotalUpdated { get; set; }

        public int TotalSkipped { get; set; }

        public int ErrorCount { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
    }


    #endregion


    #region Sync Direction Constants


    public static class SyncDirection
    {
        public const string None = "None";

        public const string ImportOnly = "ImportOnly";

        public const string PushOnly = "PushOnly";

        public const string RealTime = "RealTime";
    }


    #endregion
}
