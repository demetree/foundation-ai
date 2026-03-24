using Foundation.Security;
using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;


namespace Foundation.Messaging.Services
{
    /// <summary>
    /// 
    /// Foundation Messaging Profile Service - manages user notification profile settings
    /// for the messaging and notification push system.
    /// 
    /// This service provides a typed wrapper around the Foundation UserSettings infrastructure
    /// (SecurityUser.settings JSON blob) for messaging-specific notification configuration.
    /// 
    /// Stored settings keys (all prefixed with "messaging."):
    ///   - messaging.email              : External email address for push notifications
    ///   - messaging.emailVerified      : Whether the email has been verified
    ///   - messaging.phone              : Phone number for SMS push
    ///   - messaging.phoneVerified      : Whether the phone has been verified
    ///   - messaging.emailEnabled       : Master switch for email push delivery
    ///   - messaging.smsEnabled         : Master switch for SMS push delivery
    ///   - messaging.emailPreference    : "all", "mentions", or "none"
    ///   - messaging.smsPreference      : "all", "mentions", or "none"
    ///   - messaging.quietHoursEnabled  : Enable quiet hours window
    ///   - messaging.quietHoursStart    : Quiet hours start time (HH:mm)
    ///   - messaging.quietHoursEnd      : Quiet hours end time (HH:mm)
    ///   - messaging.timezone           : IANA timezone ID
    ///   - messaging.pinnedConversations: JSON array of pinned conversation IDs
    ///   - messaging.notificationPrefs  : JSON object of per-conversation notification preferences
    /// 
    /// This is a Foundation-level service that can be used by any module.
    /// 
    /// AI-developed as part of Foundation.Messaging Phase 3, March 2026.
    /// 
    /// </summary>
    public class MessagingProfileService
    {
        //
        // Settings key constants
        //
        private const string KEY_EMAIL = "messaging.email";
        private const string KEY_EMAIL_VERIFIED = "messaging.emailVerified";
        private const string KEY_PHONE = "messaging.phone";
        private const string KEY_PHONE_VERIFIED = "messaging.phoneVerified";
        private const string KEY_EMAIL_ENABLED = "messaging.emailEnabled";
        private const string KEY_SMS_ENABLED = "messaging.smsEnabled";
        private const string KEY_EMAIL_PREFERENCE = "messaging.emailPreference";
        private const string KEY_SMS_PREFERENCE = "messaging.smsPreference";
        private const string KEY_QUIET_HOURS_ENABLED = "messaging.quietHoursEnabled";
        private const string KEY_QUIET_HOURS_START = "messaging.quietHoursStart";
        private const string KEY_QUIET_HOURS_END = "messaging.quietHoursEnd";
        private const string KEY_TIMEZONE = "messaging.timezone";
        private const string KEY_PINNED_CONVERSATIONS = "messaging.pinnedConversations";
        private const string KEY_NOTIFICATION_PREFS = "messaging.notificationPrefs";

        //
        // Default values
        //
        private const string DEFAULT_EMAIL_PREFERENCE = "mentions";
        private const string DEFAULT_SMS_PREFERENCE = "mentions";
        private const string DEFAULT_QUIET_HOURS_START = "22:00";
        private const string DEFAULT_QUIET_HOURS_END = "07:00";
        private const string DEFAULT_TIMEZONE = "America/St_Johns";


        #region DTOs

        /// <summary>
        /// Complete notification profile for a user.
        /// </summary>
        public class NotificationProfile
        {
            // Contact information
            public string email { get; set; }
            public bool emailVerified { get; set; }
            public string phone { get; set; }
            public bool phoneVerified { get; set; }

            // Delivery preferences
            public bool emailEnabled { get; set; }
            public bool smsEnabled { get; set; }
            public string emailPreference { get; set; }
            public string smsPreference { get; set; }

            // Quiet hours
            public bool quietHoursEnabled { get; set; }
            public string quietHoursStart { get; set; }
            public string quietHoursEnd { get; set; }
            public string timezone { get; set; }

            // Messaging preferences (migrated from localStorage)
            public List<int> pinnedConversations { get; set; }
            public Dictionary<int, string> notificationPrefs { get; set; }
        }


        /// <summary>
        /// Input DTO for updating notification profile fields.
        /// Null fields are not updated (partial update support).
        /// </summary>
        public class NotificationProfileUpdate
        {
            public string email { get; set; }
            public string phone { get; set; }
            public bool? emailEnabled { get; set; }
            public bool? smsEnabled { get; set; }
            public string emailPreference { get; set; }
            public string smsPreference { get; set; }
            public bool? quietHoursEnabled { get; set; }
            public string quietHoursStart { get; set; }
            public string quietHoursEnd { get; set; }
            public string timezone { get; set; }
            public List<int> pinnedConversations { get; set; }
            public Dictionary<int, string> notificationPrefs { get; set; }
        }

        #endregion


        #region Profile Retrieval


        /// <summary>
        /// Gets the notification profile for the current user.
        /// Returns a profile with defaults if no settings have been saved yet.
        /// </summary>
        public async Task<NotificationProfile> GetNotificationProfileAsync(SecurityUser securityUser, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                return null;
            }

            NotificationProfile profile = new NotificationProfile();

            //
            // Read all messaging settings from the user's settings JSON
            //
            profile.email = await UserSettings.GetStringSettingAsync(KEY_EMAIL, securityUser, cancellationToken);
            profile.emailVerified = UserSettings.GetBoolSetting(KEY_EMAIL_VERIFIED, securityUser) ?? false;
            profile.phone = await UserSettings.GetStringSettingAsync(KEY_PHONE, securityUser, cancellationToken);
            profile.phoneVerified = UserSettings.GetBoolSetting(KEY_PHONE_VERIFIED, securityUser) ?? false;

            profile.emailEnabled = UserSettings.GetBoolSetting(KEY_EMAIL_ENABLED, securityUser) ?? false;
            profile.smsEnabled = UserSettings.GetBoolSetting(KEY_SMS_ENABLED, securityUser) ?? false;
            profile.emailPreference = UserSettings.GetStringSetting(KEY_EMAIL_PREFERENCE, securityUser) ?? DEFAULT_EMAIL_PREFERENCE;
            profile.smsPreference = UserSettings.GetStringSetting(KEY_SMS_PREFERENCE, securityUser) ?? DEFAULT_SMS_PREFERENCE;

            profile.quietHoursEnabled = UserSettings.GetBoolSetting(KEY_QUIET_HOURS_ENABLED, securityUser) ?? false;
            profile.quietHoursStart = UserSettings.GetStringSetting(KEY_QUIET_HOURS_START, securityUser) ?? DEFAULT_QUIET_HOURS_START;
            profile.quietHoursEnd = UserSettings.GetStringSetting(KEY_QUIET_HOURS_END, securityUser) ?? DEFAULT_QUIET_HOURS_END;
            profile.timezone = UserSettings.GetStringSetting(KEY_TIMEZONE, securityUser) ?? DEFAULT_TIMEZONE;

            //
            // Read pinned conversations and notification preferences from JSON arrays/objects
            //
            profile.pinnedConversations = ReadPinnedConversations(securityUser);
            profile.notificationPrefs = ReadNotificationPrefs(securityUser);

            return profile;
        }


        /// <summary>
        /// Gets the notification profile for a specific user (admin access).
        /// Requires the caller to have already verified admin permissions.
        /// </summary>
        public async Task<NotificationProfile> GetNotificationProfileAdminAsync(SecurityUser targetUser, CancellationToken cancellationToken = default)
        {
            //
            // Same implementation — the authorization check happens at the controller level.
            // We just read the target user's settings instead of the caller's.
            //
            return await GetNotificationProfileAsync(targetUser, cancellationToken);
        }

        #endregion


        #region Profile Update


        /// <summary>
        /// Updates the notification profile for the current user.
        /// Only non-null fields in the update DTO are written (partial update).
        /// </summary>
        public async Task<bool> UpdateNotificationProfileAsync(SecurityUser securityUser, NotificationProfileUpdate update, CancellationToken cancellationToken = default)
        {
            if (securityUser == null || update == null)
            {
                return false;
            }

            bool success = true;

            //
            // Update only the fields that were provided (non-null)
            //
            if (update.email != null)
            {
                success &= await UserSettings.SetStringSettingAsync(KEY_EMAIL, update.email, securityUser, cancellationToken);

                //
                // Reset verification when email changes
                //
                UserSettings.SetBoolSetting(KEY_EMAIL_VERIFIED, false, securityUser);
            }

            if (update.phone != null)
            {
                success &= await UserSettings.SetStringSettingAsync(KEY_PHONE, update.phone, securityUser, cancellationToken);

                //
                // Reset verification when phone changes
                //
                UserSettings.SetBoolSetting(KEY_PHONE_VERIFIED, false, securityUser);
            }

            if (update.emailEnabled.HasValue)
            {
                UserSettings.SetBoolSetting(KEY_EMAIL_ENABLED, update.emailEnabled.Value, securityUser);
            }

            if (update.smsEnabled.HasValue)
            {
                UserSettings.SetBoolSetting(KEY_SMS_ENABLED, update.smsEnabled.Value, securityUser);
            }

            if (update.emailPreference != null)
            {
                success &= await UserSettings.SetStringSettingAsync(KEY_EMAIL_PREFERENCE, update.emailPreference, securityUser, cancellationToken);
            }

            if (update.smsPreference != null)
            {
                success &= await UserSettings.SetStringSettingAsync(KEY_SMS_PREFERENCE, update.smsPreference, securityUser, cancellationToken);
            }

            if (update.quietHoursEnabled.HasValue)
            {
                UserSettings.SetBoolSetting(KEY_QUIET_HOURS_ENABLED, update.quietHoursEnabled.Value, securityUser);
            }

            if (update.quietHoursStart != null)
            {
                success &= await UserSettings.SetStringSettingAsync(KEY_QUIET_HOURS_START, update.quietHoursStart, securityUser, cancellationToken);
            }

            if (update.quietHoursEnd != null)
            {
                success &= await UserSettings.SetStringSettingAsync(KEY_QUIET_HOURS_END, update.quietHoursEnd, securityUser, cancellationToken);
            }

            if (update.timezone != null)
            {
                success &= await UserSettings.SetStringSettingAsync(KEY_TIMEZONE, update.timezone, securityUser, cancellationToken);
            }

            if (update.pinnedConversations != null)
            {
                WritePinnedConversations(securityUser, update.pinnedConversations);
            }

            if (update.notificationPrefs != null)
            {
                WriteNotificationPrefs(securityUser, update.notificationPrefs);
            }

            return success;
        }


        /// <summary>
        /// Updates the notification profile for a specific user (admin access).
        /// Requires the caller to have already verified admin permissions.
        /// </summary>
        public async Task<bool> UpdateNotificationProfileAdminAsync(SecurityUser targetUser, NotificationProfileUpdate update, CancellationToken cancellationToken = default)
        {
            return await UpdateNotificationProfileAsync(targetUser, update, cancellationToken);
        }

        #endregion


        #region Delivery Checks


        /// <summary>
        /// Quick check: should we deliver to this user via the specified provider?
        /// Checks enabled status, preference level, and quiet hours.
        /// </summary>
        /// <param name="securityUser">The recipient user</param>
        /// <param name="providerId">"email" or "sms"</param>
        /// <param name="isMention">Whether the message mentions this user (for "mentions" preference)</param>
        /// <returns>True if delivery should proceed</returns>
        public bool IsDeliveryEnabled(SecurityUser securityUser, string providerId, bool isMention = false)
        {
            if (securityUser == null || string.IsNullOrWhiteSpace(providerId))
            {
                return false;
            }

            //
            // Check master switch
            //
            bool enabled;
            string preference;

            if (providerId == "email")
            {
                enabled = UserSettings.GetBoolSetting(KEY_EMAIL_ENABLED, securityUser) ?? false;
                preference = UserSettings.GetStringSetting(KEY_EMAIL_PREFERENCE, securityUser) ?? DEFAULT_EMAIL_PREFERENCE;

                //
                // Must have a configured email address
                //
                string email = UserSettings.GetStringSetting(KEY_EMAIL, securityUser);
                if (string.IsNullOrWhiteSpace(email)) return false;
            }
            else if (providerId == "sms")
            {
                enabled = UserSettings.GetBoolSetting(KEY_SMS_ENABLED, securityUser) ?? false;
                preference = UserSettings.GetStringSetting(KEY_SMS_PREFERENCE, securityUser) ?? DEFAULT_SMS_PREFERENCE;

                //
                // Must have a configured phone number
                //
                string phone = UserSettings.GetStringSetting(KEY_PHONE, securityUser);
                if (string.IsNullOrWhiteSpace(phone)) return false;
            }
            else
            {
                return false;
            }

            if (enabled == false)
            {
                return false;
            }

            //
            // Check preference level
            //
            if (preference == "none")
            {
                return false;
            }

            if (preference == "mentions" && isMention == false)
            {
                return false;
            }

            //
            // Check quiet hours
            //
            if (IsInQuietHours(securityUser))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns the destination address for a given provider.
        /// </summary>
        public string GetDestination(SecurityUser securityUser, string providerId)
        {
            if (securityUser == null || string.IsNullOrWhiteSpace(providerId))
            {
                return null;
            }

            if (providerId == "email")
            {
                return UserSettings.GetStringSetting(KEY_EMAIL, securityUser);
            }
            else if (providerId == "sms")
            {
                return UserSettings.GetStringSetting(KEY_PHONE, securityUser);
            }

            return null;
        }


        /// <summary>
        /// Checks whether the user is currently in their configured quiet hours window.
        /// </summary>
        public bool IsInQuietHours(SecurityUser securityUser)
        {
            bool quietHoursEnabled = UserSettings.GetBoolSetting(KEY_QUIET_HOURS_ENABLED, securityUser) ?? false;

            if (quietHoursEnabled == false)
            {
                return false;
            }

            string startStr = UserSettings.GetStringSetting(KEY_QUIET_HOURS_START, securityUser) ?? DEFAULT_QUIET_HOURS_START;
            string endStr = UserSettings.GetStringSetting(KEY_QUIET_HOURS_END, securityUser) ?? DEFAULT_QUIET_HOURS_END;
            string timezone = UserSettings.GetStringSetting(KEY_TIMEZONE, securityUser) ?? DEFAULT_TIMEZONE;


            //
            // Parse quiet hours start and end times
            //
            if (TimeSpan.TryParse(startStr, out TimeSpan start) == false ||
                TimeSpan.TryParse(endStr, out TimeSpan end) == false)
            {
                return false;
            }

            //
            // Get the current time in the user's timezone
            //
            DateTime utcNow = DateTime.UtcNow;
            TimeSpan userLocalTime;

            try
            {
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                DateTime userLocalDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
                userLocalTime = userLocalDateTime.TimeOfDay;
            }
            catch
            {
                //
                // If timezone is invalid, fall back to UTC
                //
                userLocalTime = utcNow.TimeOfDay;
            }


            //
            // Check if current time falls within the quiet hours window.
            // Handles overnight windows (e.g., 22:00 → 07:00) as well as
            // same-day windows (e.g., 12:00 → 14:00).
            //
            if (start <= end)
            {
                //
                // Same-day window (e.g., 12:00 → 14:00)
                //
                return userLocalTime >= start && userLocalTime < end;
            }
            else
            {
                //
                // Overnight window (e.g., 22:00 → 07:00)
                //
                return userLocalTime >= start || userLocalTime < end;
            }
        }

        #endregion


        #region Pinned Conversations & Per-Conversation Prefs


        /// <summary>
        /// Gets the user's per-conversation notification preference.
        /// Returns "all", "mentions", or "none". Defaults to "all" if not set.
        /// </summary>
        public string GetConversationNotificationPreference(SecurityUser securityUser, int conversationId)
        {
            Dictionary<int, string> prefs = ReadNotificationPrefs(securityUser);

            if (prefs != null && prefs.ContainsKey(conversationId))
            {
                return prefs[conversationId];
            }

            return "all";
        }


        /// <summary>
        /// Sets the user's notification preference for a specific conversation.
        /// </summary>
        public void SetConversationNotificationPreference(SecurityUser securityUser, int conversationId, string preference)
        {
            Dictionary<int, string> prefs = ReadNotificationPrefs(securityUser) ?? new Dictionary<int, string>();

            prefs[conversationId] = preference;

            WriteNotificationPrefs(securityUser, prefs);
        }


        /// <summary>
        /// Checks if a conversation is pinned.
        /// </summary>
        public bool IsConversationPinned(SecurityUser securityUser, int conversationId)
        {
            List<int> pinned = ReadPinnedConversations(securityUser);
            return pinned != null && pinned.Contains(conversationId);
        }


        /// <summary>
        /// Toggles a conversation's pinned state.
        /// </summary>
        public void TogglePinnedConversation(SecurityUser securityUser, int conversationId)
        {
            List<int> pinned = ReadPinnedConversations(securityUser) ?? new List<int>();

            if (pinned.Contains(conversationId))
            {
                pinned.Remove(conversationId);
            }
            else
            {
                pinned.Add(conversationId);
            }

            WritePinnedConversations(securityUser, pinned);
        }

        #endregion


        #region Private Helpers


        /// <summary>
        /// Reads the pinned conversations list from user settings.
        /// </summary>
        private static List<int> ReadPinnedConversations(SecurityUser securityUser)
        {
            List<int> result = new List<int>();

            try
            {
                string json = UserSettings.GetStringSetting(KEY_PINNED_CONVERSATIONS, securityUser);

                if (string.IsNullOrWhiteSpace(json) == false)
                {
                    result = JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
                }
            }
            catch
            {
                // Return empty list on parse failure
            }

            return result;
        }


        /// <summary>
        /// Writes the pinned conversations list to user settings.
        /// </summary>
        private static void WritePinnedConversations(SecurityUser securityUser, List<int> pinnedConversations)
        {
            string json = JsonSerializer.Serialize(pinnedConversations ?? new List<int>());
            UserSettings.SetStringSetting(KEY_PINNED_CONVERSATIONS, json, securityUser);
        }


        /// <summary>
        /// Reads the per-conversation notification preferences from user settings.
        /// </summary>
        private static Dictionary<int, string> ReadNotificationPrefs(SecurityUser securityUser)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();

            try
            {
                string json = UserSettings.GetStringSetting(KEY_NOTIFICATION_PREFS, securityUser);

                if (string.IsNullOrWhiteSpace(json) == false)
                {
                    //
                    // JSON keys are strings, so we deserialize as Dictionary<string, string>
                    // and convert to Dictionary<int, string>
                    //
                    Dictionary<string, string> stringKeyed = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                    if (stringKeyed != null)
                    {
                        foreach (var kvp in stringKeyed)
                        {
                            if (int.TryParse(kvp.Key, out int convId))
                            {
                                result[convId] = kvp.Value;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Return empty dictionary on parse failure
            }

            return result;
        }


        /// <summary>
        /// Writes the per-conversation notification preferences to user settings.
        /// </summary>
        private static void WriteNotificationPrefs(SecurityUser securityUser, Dictionary<int, string> notificationPrefs)
        {
            //
            // Convert int keys to string keys for JSON serialization
            //
            Dictionary<string, string> stringKeyed = new Dictionary<string, string>();

            if (notificationPrefs != null)
            {
                foreach (var kvp in notificationPrefs)
                {
                    stringKeyed[kvp.Key.ToString()] = kvp.Value;
                }
            }

            string json = JsonSerializer.Serialize(stringKeyed);
            UserSettings.SetStringSetting(KEY_NOTIFICATION_PREFS, json, securityUser);
        }

        #endregion
    }
}
