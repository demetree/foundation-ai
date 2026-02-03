using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;

namespace Foundation.Security
{
    /// <summary>
    /// 
    /// Provides static methods for retrieving and persisting tenant-specific settings stored
    /// as a JSON object in the SecurityTenant.settings field.
    ///
    /// This class mirrors the pattern of UserSettings.cs but operates on SecurityTenant entities
    /// instead of SecurityUser entities.
    /// 
    /// </summary>
    /// <remarks>
    /// 
    /// Settings are stored as a single JSON object in the database, where each property 
    /// represents a named setting and its value. This class provides typed accessors for 
    /// common data types (string, int, DateTime, bool, object, List&lt;string&gt;), handling
    /// JSON parsing and serialization internally.
    ///
    /// Thread Safety: These methods are not thread-safe for concurrent modifications to the
    /// same tenant's settings. Callers should ensure proper synchronization if concurrent 
    /// writes are possible.
    ///
    /// </remarks>
    public class TenantSettings
    {

        #region RawSettings


        /// <summary>
        /// 
        /// Asynchronously retrieves the raw JSON settings string for the specified tenant.
        /// 
        /// </summary>
        /// <param name="securityTenant">The tenant whose settings are being retrieved.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>
        /// The raw JSON string from the tenant's settings field, or null if not set.
        /// </returns>
        public static async Task<string> GetTenantSettingsAsync(SecurityTenant securityTenant,
                                                                 CancellationToken cancellationToken = default)
        {
            using (SecurityContext db = new SecurityContext())
            {
                if (securityTenant != null)
                {
                    SecurityTenant tenantToGetSettingsFor = await (from tenants in db.SecurityTenants
                                                                    where tenants.id == securityTenant.id
                                                                    select tenants)
                                                             .FirstOrDefaultAsync(cancellationToken)
                                                             .ConfigureAwait(false);

                    if (tenantToGetSettingsFor != null)
                    {
                        return tenantToGetSettingsFor.settings;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// 
        /// Synchronously retrieves the raw JSON settings string for the specified tenant.
        /// 
        /// </summary>
        /// <param name="securityTenant">The tenant whose settings are being retrieved.</param>
        /// <returns>
        /// The raw JSON string from the tenant's settings field, or null if not set.
        /// </returns>
        public static string GetTenantSettings(SecurityTenant securityTenant)
        {
            using (SecurityContext db = new SecurityContext())
            {
                if (securityTenant != null)
                {
                    SecurityTenant tenantToGetSettingsFor = (from tenants in db.SecurityTenants
                                                              where tenants.id == securityTenant.id
                                                              select tenants).FirstOrDefault();

                    if (tenantToGetSettingsFor != null)
                    {
                        return tenantToGetSettingsFor.settings;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// 
        /// Asynchronously persists the raw JSON settings string for the specified tenant.
        /// 
        /// </summary>
        /// <param name="securityTenant">The tenant whose settings are being updated.</param>
        /// <param name="settingsJSON">The JSON string to persist.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        public static async Task<bool> SetTenantSettingsAsync(SecurityTenant securityTenant,
                                                               string settingsJSON,
                                                               CancellationToken cancellationToken = default)
        {
            if (securityTenant == null)
            {
                return false;
            }

            using (SecurityContext db = new SecurityContext())
            {
                SecurityTenant tenantToUpdate = await (from tenants in db.SecurityTenants
                                                        where tenants.id == securityTenant.id
                                                        select tenants)
                                                 .FirstOrDefaultAsync(cancellationToken)
                                                 .ConfigureAwait(false);

                if (tenantToUpdate != null)
                {
                    tenantToUpdate.settings = settingsJSON;
                    db.Entry(tenantToUpdate).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            return true;
        }


        /// <summary>
        /// 
        /// Synchronously persists the raw JSON settings string for the specified tenant.
        /// 
        /// </summary>
        /// <param name="securityTenant">The tenant whose settings are being updated.</param>
        /// <param name="settingsJSON">The JSON string to persist.</param>
        public static void SetTenantSettings(SecurityTenant securityTenant, string settingsJSON)
        {
            if (securityTenant == null)
            {
                return;
            }

            using (SecurityContext db = new SecurityContext())
            {
                SecurityTenant tenantToUpdate = (from tenants in db.SecurityTenants
                                                  where tenants.id == securityTenant.id
                                                  select tenants).FirstOrDefault();

                if (tenantToUpdate != null)
                {
                    tenantToUpdate.settings = settingsJSON;
                    db.Entry(tenantToUpdate).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        #endregion


        #region StringSetting


        /// <summary>
        /// 
        /// Asynchronously retrieves a string setting value from the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityTenant">The tenant whose settings are being queried.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>
        /// The string value of the setting if found; otherwise, null.
        /// </returns>
        public static async Task<string> GetStringSettingAsync(string settingName,
                                                               SecurityTenant securityTenant,
                                                               CancellationToken cancellationToken = default)
        {
            string output = null;
            string settingsJSONString = await GetTenantSettingsAsync(securityTenant, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                using (JsonDocument document = JsonDocument.Parse(settingsJSONString))
                {
                    JsonElement root = document.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty(settingName, out JsonElement valueElement))
                        {
                            output = valueElement.GetString();
                        }
                    }
                }
            }
            return output;
        }


        /// <summary>
        /// 
        /// Synchronously retrieves a string setting value from the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityTenant">The tenant whose settings are being queried.</param>
        /// <returns>
        /// The string value of the setting if found; otherwise, null.
        /// </returns>
        public static string GetStringSetting(string settingName, SecurityTenant securityTenant)
        {
            string output = null;
            string settingsJSONString = GetTenantSettings(securityTenant);

            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                using (JsonDocument document = JsonDocument.Parse(settingsJSONString))
                {
                    JsonElement root = document.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty(settingName, out JsonElement valueElement))
                        {
                            output = valueElement.GetString();
                        }
                    }
                }
            }
            return output;
        }


        /// <summary>
        /// 
        /// Asynchronously sets (or adds) a string setting value in the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">The new value for the setting.</param>
        /// <param name="securityTenant">The tenant whose settings are being modified.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        public static async Task<bool> SetStringSettingAsync(string settingName,
                                                              string settingValue,
                                                              SecurityTenant securityTenant,
                                                              CancellationToken cancellationToken = default)
        {
            try
            {
                string settingsJSONString = await GetTenantSettingsAsync(securityTenant, cancellationToken).ConfigureAwait(false);
                JsonObject settingsObject;

                if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
                {
                    JsonNode parsedNode = JsonNode.Parse(settingsJSONString);
                    if (parsedNode is JsonObject existingObject)
                    {
                        settingsObject = existingObject;
                    }
                    else
                    {
                        settingsObject = new JsonObject();
                    }
                }
                else
                {
                    settingsObject = new JsonObject();
                }

                settingsObject[settingName] = settingValue;
                string serializedSettings = settingsObject.ToJsonString();
                await SetTenantSettingsAsync(securityTenant, serializedSettings, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// 
        /// Synchronously sets (or adds) a string setting value in the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">The new value for the setting.</param>
        /// <param name="securityTenant">The tenant whose settings are being modified.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        public static bool SetStringSetting(string settingName, string settingValue, SecurityTenant securityTenant)
        {
            try
            {
                string settingsJSONString = GetTenantSettings(securityTenant);
                JsonObject settingsObject;

                if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
                {
                    JsonNode parsedNode = JsonNode.Parse(settingsJSONString);
                    if (parsedNode is JsonObject existingObject)
                    {
                        settingsObject = existingObject;
                    }
                    else
                    {
                        settingsObject = new JsonObject();
                    }
                }
                else
                {
                    settingsObject = new JsonObject();
                }

                settingsObject[settingName] = settingValue;
                string serializedSettings = settingsObject.ToJsonString();
                SetTenantSettings(securityTenant, serializedSettings);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion


        #region IntSetting


        /// <summary>
        /// 
        /// Retrieves an integer setting value from the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityTenant">The tenant whose settings are being queried.</param>
        /// <returns>
        /// The integer value of the setting if found and is a valid number; otherwise, null.
        /// </returns>
        public static int? GetIntSetting(string settingName, SecurityTenant securityTenant)
        {
            int? output = null;
            string settingsJSONString = GetTenantSettings(securityTenant);

            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                using (JsonDocument document = JsonDocument.Parse(settingsJSONString))
                {
                    JsonElement root = document.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty(settingName, out JsonElement valueElement))
                        {
                            if (valueElement.ValueKind == JsonValueKind.Number)
                            {
                                if (valueElement.TryGetInt32(out int intValue))
                                {
                                    output = intValue;
                                }
                            }
                        }
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// 
        /// Sets (or adds) an integer setting value in the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">The new value for the setting.</param>
        /// <param name="securityTenant">The tenant whose settings are being modified.</param>
        public static void SetIntSetting(string settingName, int? settingValue, SecurityTenant securityTenant)
        {
            string settingsJSONString = GetTenantSettings(securityTenant);
            JsonObject settingsObject;

            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                JsonNode parsedNode = JsonNode.Parse(settingsJSONString);
                if (parsedNode is JsonObject existingObject)
                {
                    settingsObject = existingObject;
                }
                else
                {
                    settingsObject = new JsonObject();
                }
            }
            else
            {
                settingsObject = new JsonObject();
            }

            settingsObject[settingName] = settingValue;
            string serializedSettings = settingsObject.ToJsonString();
            SetTenantSettings(securityTenant, serializedSettings);
        }

        #endregion


        #region DateTimeSetting


        /// <summary>
        /// 
        /// Retrieves a DateTime setting value from the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityTenant">The tenant whose settings are being queried.</param>
        /// <returns>
        /// The DateTime value of the setting if found and parseable; otherwise, null.
        /// </returns>
        public static DateTime? GetDateTimeSetting(string settingName, SecurityTenant securityTenant)
        {
            DateTime? output = null;
            string settingsJSONString = GetTenantSettings(securityTenant);

            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                using (JsonDocument document = JsonDocument.Parse(settingsJSONString))
                {
                    JsonElement root = document.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty(settingName, out JsonElement valueElement))
                        {
                            if (valueElement.ValueKind == JsonValueKind.String)
                            {
                                if (valueElement.TryGetDateTime(out DateTime dateTimeValue))
                                {
                                    output = dateTimeValue;
                                }
                            }
                        }
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// 
        /// Sets (or adds) a DateTime setting value in the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">The new value for the setting.</param>
        /// <param name="securityTenant">The tenant whose settings are being modified.</param>
        public static void SetDateTimeSetting(string settingName, DateTime? settingValue, SecurityTenant securityTenant)
        {
            string settingsJSONString = GetTenantSettings(securityTenant);
            JsonObject settingsObject;

            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                JsonNode parsedNode = JsonNode.Parse(settingsJSONString);
                if (parsedNode is JsonObject existingObject)
                {
                    settingsObject = existingObject;
                }
                else
                {
                    settingsObject = new JsonObject();
                }
            }
            else
            {
                settingsObject = new JsonObject();
            }

            settingsObject[settingName] = settingValue;
            string serializedSettings = settingsObject.ToJsonString();
            SetTenantSettings(securityTenant, serializedSettings);
        }

        #endregion


        #region BoolSetting


        /// <summary>
        /// 
        /// Retrieves a boolean setting value from the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityTenant">The tenant whose settings are being queried.</param>
        /// <returns>
        /// The boolean value of the setting if found and is a JSON boolean; otherwise, null.
        /// </returns>
        public static bool? GetBoolSetting(string settingName, SecurityTenant securityTenant)
        {
            bool? output = null;
            string settingsJSONString = GetTenantSettings(securityTenant);

            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                using (JsonDocument document = JsonDocument.Parse(settingsJSONString))
                {
                    JsonElement root = document.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty(settingName, out JsonElement valueElement))
                        {
                            if (valueElement.ValueKind == JsonValueKind.True ||
                                valueElement.ValueKind == JsonValueKind.False)
                            {
                                output = valueElement.GetBoolean();
                            }
                        }
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// 
        /// Sets (or adds) a boolean setting value in the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">The new value for the setting.</param>
        /// <param name="securityTenant">The tenant whose settings are being modified.</param>
        public static void SetBoolSetting(string settingName, bool? settingValue, SecurityTenant securityTenant)
        {
            string settingsJSONString = GetTenantSettings(securityTenant);
            JsonObject settingsObject;

            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                JsonNode parsedNode = JsonNode.Parse(settingsJSONString);
                if (parsedNode is JsonObject existingObject)
                {
                    settingsObject = existingObject;
                }
                else
                {
                    settingsObject = new JsonObject();
                }
            }
            else
            {
                settingsObject = new JsonObject();
            }

            settingsObject[settingName] = settingValue;
            string serializedSettings = settingsObject.ToJsonString();
            SetTenantSettings(securityTenant, serializedSettings);
        }

        #endregion


        #region ObjectSetting


        /// <summary>
        /// 
        /// Retrieves an arbitrary JSON object value from the tenant's settings by the specified setting name.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityTenant">The tenant whose settings are being queried.</param>
        /// <returns>
        /// A JsonNode representing the value if found and not null; otherwise, null.
        /// </returns>
        public static JsonNode GetObjectSetting(string settingName, SecurityTenant securityTenant)
        {
            JsonNode output = null;
            string settingsJSONString = GetTenantSettings(securityTenant);

            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                JsonNode rootNode = JsonNode.Parse(settingsJSONString);

                if (rootNode is JsonObject rootObject)
                {
                    if (rootObject.TryGetPropertyValue(settingName, out JsonNode valueNode))
                    {
                        if (valueNode != null)
                        {
                            return valueNode.DeepClone();
                        }
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// 
        /// Asynchronously retrieves an arbitrary JSON object value from the tenant's settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityTenant">The tenant whose settings are being queried.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>
        /// A JsonNode representing the value if found and not null; otherwise, null.
        /// </returns>
        public static async Task<JsonNode> GetObjectSettingAsync(string settingName,
                                                                  SecurityTenant securityTenant,
                                                                  CancellationToken cancellationToken = default)
        {
            JsonNode output = null;
            string settingsJSONString = await GetTenantSettingsAsync(securityTenant, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                JsonNode rootNode = JsonNode.Parse(settingsJSONString);

                if (rootNode is JsonObject rootObject)
                {
                    if (rootObject.TryGetPropertyValue(settingName, out JsonNode valueNode))
                    {
                        if (valueNode != null)
                        {
                            return valueNode.DeepClone();
                        }
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// 
        /// Sets (or adds) an arbitrary object setting value in the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">The new value for the setting (any JSON-serializable object).</param>
        /// <param name="securityTenant">The tenant whose settings are being modified.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        public static bool SetObjectSetting(string settingName, object settingValue, SecurityTenant securityTenant)
        {
            try
            {
                string settingsJSONString = GetTenantSettings(securityTenant);
                JsonObject settingsObject;

                if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
                {
                    JsonNode parsedNode = JsonNode.Parse(settingsJSONString);

                    if (parsedNode is JsonObject existingObject)
                    {
                        settingsObject = existingObject;
                    }
                    else
                    {
                        settingsObject = new JsonObject();
                    }
                }
                else
                {
                    settingsObject = new JsonObject();
                }

                if (settingValue != null)
                {
                    JsonNode valueNode = JsonSerializer.SerializeToNode(settingValue);
                    settingsObject[settingName] = valueNode;
                }
                else
                {
                    settingsObject[settingName] = null;
                }

                string serializedSettings = settingsObject.ToJsonString();
                SetTenantSettings(securityTenant, serializedSettings);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// 
        /// Asynchronously sets (or adds) an arbitrary object setting value in the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">The new value for the setting (any JSON-serializable object).</param>
        /// <param name="securityTenant">The tenant whose settings are being modified.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        public static async Task<bool> SetObjectSettingAsync(string settingName,
                                                              object settingValue,
                                                              SecurityTenant securityTenant,
                                                              CancellationToken cancellationToken = default)
        {
            try
            {
                string settingsJSONString = await GetTenantSettingsAsync(securityTenant, cancellationToken).ConfigureAwait(false);
                JsonObject settingsObject;

                if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
                {
                    JsonNode parsedNode = JsonNode.Parse(settingsJSONString);

                    if (parsedNode is JsonObject existingObject)
                    {
                        settingsObject = existingObject;
                    }
                    else
                    {
                        settingsObject = new JsonObject();
                    }
                }
                else
                {
                    settingsObject = new JsonObject();
                }

                if (settingValue != null)
                {
                    JsonNode valueNode = JsonSerializer.SerializeToNode(settingValue);
                    settingsObject[settingName] = valueNode;
                }
                else
                {
                    settingsObject[settingName] = null;
                }

                string serializedSettings = settingsObject.ToJsonString();
                await SetTenantSettingsAsync(securityTenant, serializedSettings, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion


        #region StringListSetting


        /// <summary>
        /// 
        /// Retrieves a list of strings from the tenant's JSON settings by the specified setting name.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityTenant">The tenant whose settings are being queried.</param>
        /// <returns>
        /// A List&lt;string&gt; containing the values if found and is a valid JSON array of strings;
        /// otherwise, null.
        /// </returns>
        public static List<string> GetStringListSetting(string settingName, SecurityTenant securityTenant)
        {
            List<string> output = null;
            string settingsJSONString = GetTenantSettings(securityTenant);

            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                JsonNode rootNode = JsonNode.Parse(settingsJSONString);

                if (rootNode is JsonObject rootObject)
                {
                    if (rootObject.TryGetPropertyValue(settingName, out JsonNode valueNode))
                    {
                        if (valueNode is JsonArray jsonArray)
                        {
                            try
                            {
                                output = jsonArray.Deserialize<List<string>>();
                            }
                            catch (JsonException)
                            {
                                output = null;
                            }
                        }
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// 
        /// Sets (or adds) a list of strings as a setting value in the tenant's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">The new list value for the setting.</param>
        /// <param name="securityTenant">The tenant whose settings are being modified.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        public static bool SetStringListSetting(string settingName, List<string> settingValue, SecurityTenant securityTenant)
        {
            try
            {
                string settingsJSONString = GetTenantSettings(securityTenant);
                JsonObject settingsObject;

                if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
                {
                    JsonNode parsedNode = JsonNode.Parse(settingsJSONString);

                    if (parsedNode is JsonObject existingObject)
                    {
                        settingsObject = existingObject;
                    }
                    else
                    {
                        settingsObject = new JsonObject();
                    }
                }
                else
                {
                    settingsObject = new JsonObject();
                }

                if (settingValue != null)
                {
                    JsonNode valueNode = JsonSerializer.SerializeToNode(settingValue);
                    settingsObject[settingName] = valueNode;
                }
                else
                {
                    settingsObject[settingName] = null;
                }

                string serializedSettings = settingsObject.ToJsonString();
                SetTenantSettings(securityTenant, serializedSettings);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

    }
}
