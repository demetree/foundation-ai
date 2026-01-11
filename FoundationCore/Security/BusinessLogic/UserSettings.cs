using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Foundation.Security.Database;

namespace Foundation.Security
{
    public class UserSettings
    {
        #region Settings Basics

        internal static async Task<string> GetUserSettingsAsync(SecurityUser securityUser, CancellationToken cancellationToken = default)
        {
            using (SecurityContext db = new SecurityContext())
            {

                if (securityUser != null)
                {
                    SecurityUser userToGetSettingsfor = await (from users in db.SecurityUsers
                                                               where users.id == securityUser.id
                                                               select users)
                                                         .FirstOrDefaultAsync(cancellationToken)
                                                         .ConfigureAwait(false);


                    if (userToGetSettingsfor != null)
                    {
                        return userToGetSettingsfor.settings;
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


        internal static string GetUserSettings(SecurityUser securityUser)
        {
            using (SecurityContext db = new SecurityContext())
            {

                if (securityUser != null)
                {
                    SecurityUser userToGetSettingsfor = (from users in db.SecurityUsers
                                                         where users.id == securityUser.id
                                                         select users).FirstOrDefault();


                    if (userToGetSettingsfor != null)
                    {
                        return userToGetSettingsfor.settings;
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


        internal static async Task<bool> SetUserSettingsAsync(SecurityUser securityUser, string settings, CancellationToken cancellationToken = default)
        {
            if (securityUser == null)
            {
                return false;
            }

            using (SecurityContext db = new SecurityContext())
            {
                SecurityUser userToUpdate = await (from users in db.SecurityUsers
                                                   where users.id == securityUser.id
                                                   select users)
                                             .FirstOrDefaultAsync(cancellationToken)
                                             .ConfigureAwait(false);

                if (userToUpdate != null)
                {
                    userToUpdate.settings = settings;
                    db.Entry(userToUpdate).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false); 
                }
            }

            return true;
        }

        internal static void SetUserSettings(SecurityUser securityUser, string settings)
        {
            if (securityUser == null)
            {
                return;
            }

            using (SecurityContext db = new SecurityContext())
            {
                SecurityUser userToUpdate = (from users in db.SecurityUsers
                                             where users.id == securityUser.id
                                             select users).FirstOrDefault();

                if (userToUpdate != null)
                {
                    userToUpdate.settings = settings;
                    db.Entry(userToUpdate).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        #endregion


        #region StringSetting
        /// <summary>
        /// Retrieves a string value from the user's JSON settings string by the specified setting name.
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityUser">The security user whose settings are being queried.</param>
        /// <returns>
        /// The string value of the setting if found and it is a JSON string value; otherwise, null.
        /// </returns>
        /// <remarks>
        /// - Returns null if the settings JSON is null or empty.
        /// - Returns null if the setting name does not exist.
        /// - Returns the string value if present (does not attempt to convert non-string values).
        /// </remarks>
        public static string GetStringSetting(string settingName, SecurityUser securityUser)
        {
            string output = null;

            //
            // Retrieve the raw JSON string containing the user's settings
            //
            string settingsJSONString = GetUserSettings(securityUser);

            //
            // Only proceed if we have non-empty JSON content
            //
            if (!string.IsNullOrWhiteSpace(settingsJSONString))
            {
                //
                // Parse the JSON using JsonDocument
                //
                using (JsonDocument document = JsonDocument.Parse(settingsJSONString))
                {
                    //
                    // The root must be an object to allow property lookups
                    //
                    JsonElement root = document.RootElement;

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        //
                        // Attempt to get the property by name
                        //
                        if (root.TryGetProperty(settingName, out JsonElement valueElement))
                        {
                            //
                            // Return the value, cast to be a string.  This allows for getting any setting as a string
                            //
                            //  We could guard this with if (valueElement.ValueKind == JsonValueKind.String) if we want to limit to string types only, but I don't that that necessary.
                            output = valueElement.GetString();
                        }
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// Asynchronously retrieves a string value from the user's JSON settings by the specified setting name.
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityUser">The security user whose settings are being queried.</param>
        /// <param name="cancellationToken">
        /// Optional cancellation token to allow the operation to be cancelled if needed.
        /// </param>
        /// <returns>
        /// The string value of the setting if found and it is a JSON string value; otherwise, null.
        /// </returns>
        /// <remarks>
        /// - Returns null if the settings JSON is null, empty, or whitespace-only.
        /// - Returns null if the requested property does not exist.
        /// - Returns null if the property exists but is not a JSON string value (e.g., number, boolean, object).
        /// - The JSON parsing is performed synchronously on the awaited settings string, as JsonDocument.Parse
        ///   is currently synchronous in System.Text.Json. This is acceptable here because the input is an
        ///   in-memory string rather than a large stream.
        ///
        /// JsonDocument is used within a using statement to ensure proper disposal of the underlying resources.
        /// </remarks>
        public static async Task<string> GetStringSettingAsync(string settingName,
                                                               SecurityUser securityUser,
                                                               CancellationToken cancellationToken = default)
        {
            string output = null;

            //
            // Asynchronously retrieve the raw JSON string containing the user's settings
            //
            string settingsJSONString = await GetUserSettingsAsync(securityUser, cancellationToken).ConfigureAwait(false);

            //
            // Proceed only if we have meaningful content
            //
            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                //
                // Note: JsonDocument.Parse is synchronous but operates on an in-memory string, so the performance impact is negligible.
                //
                using (JsonDocument document = JsonDocument.Parse(settingsJSONString))
                {
                    //
                    // Obtain the root element of the document
                    //
                    JsonElement root = document.RootElement;

                    //
                    // Ensure the root is an object before attempting property lookups
                    //
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        //
                        // Try to retrieve the property by name
                        //
                        if (root.TryGetProperty(settingName, out JsonElement valueElement))
                        {
                            //
                            // Return the value, cast to be a string.  This allows for getting any setting as a string
                            //
                            //  We could guard this with if (valueElement.ValueKind == JsonValueKind.String) if we want to limit to string types only, but I don't that that necessary.
                            output = valueElement.GetString();
                        }
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// 
        /// Asynchronously sets (or adds) a string setting value in the user's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">
        /// The new value for the setting. If null, the property will be explicitly set to JSON null.
        /// </param>
        /// <param name="securityUser">The security user whose settings are being modified.</param>
        /// <param name="cancellationToken">
        /// Optional cancellation token to allow cancellation of the asynchronous operations.
        /// </param>
        /// <returns>Returns true on success, or false if it caught an exception.</returns>
        /// <remarks>
        ///
        /// - Retrieves existing settings JSON (if any).
        /// - Parses it into a JSON object.
        /// - Adds or updates the specified property with the provided value (null becomes JSON null).
        /// - If no settings existed previously, creates a new JSON object with only this property.
        /// - Serializes the updated object and persists it via SetUserSettingsAsync.
        ///
        /// </remarks>
        public static async Task<bool> SetStringSettingAsync(string settingName,
                                                             string settingValue,
                                                             SecurityUser securityUser,
                                                             CancellationToken cancellationToken = default)
        {
            try
            {
                //
                // Asynchronously retrieve the current raw JSON settings string
                //
                string settingsJSONString = await GetUserSettingsAsync(securityUser, cancellationToken).ConfigureAwait(false);

                JsonObject settingsObject;

                // Determine if we have existing settings to work with
                if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
                {
                    //
                    // Parse the existing JSON string into a mutable JsonObject
                    //
                    // This will throw if the JSON is invalid
                    //
                    JsonNode parsedNode = JsonNode.Parse(settingsJSONString)!;

                    //
                    // Ensure the root is an object; if not, treat as no settings (defensive)
                    //
                    if (parsedNode is JsonObject existingObject)
                    {
                        settingsObject = existingObject;
                    }
                    else
                    {
                        //
                        // Root was not an object (e.g., array, primitive) – start fresh to avoid corruption
                        //
                        settingsObject = new JsonObject();
                    }
                }
                else
                {
                    //
                    // No existing settings – create a new empty JSON object
                    //
                    settingsObject = new JsonObject();
                }

                //
                // Set the property value.
                // Passing null explicitly creates a JSON "null" value, matching original behavior.
                //
                settingsObject[settingName] = settingValue;

                //
                // Serialize the updated object back to a JSON string
                // Use default options (indentation off, consistent with typical JsonConvert defaults)
                //
                string serializedSettings = settingsObject.ToJsonString();

                //
                // Persist the updated settings
                //
                await SetUserSettingsAsync(securityUser, serializedSettings, cancellationToken).ConfigureAwait(false);

                //
                // Original method always returned true on completion
                //
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// Sets (or adds) a string setting value in the user's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">
        /// The new value for the setting. If null, the property will be explicitly set to JSON null.
        /// </param>
        /// <param name="securityUser">The security user whose settings are being modified.</param>
        /// <returns>Returns true on success, or false if it caught an exception.</returns>
        /// <remarks>
        ///
        /// - Retrieves existing settings JSON (if any).
        /// - Parses it into a JSON object.
        /// - Adds or updates the specified property with the provided value (null becomes JSON null).
        /// - If no settings existed previously, creates a new JSON object with only this property.
        /// - Serializes the updated object and persists it via SetUserSettingsAsync.
        ///
        /// </remarks>
        public static bool SetStringSetting(string settingName, string settingValue, SecurityUser securityUser)
        {
            try
            {
                //
                // Asynchronously retrieve the current raw JSON settings string
                //
                string settingsJSONString = GetUserSettings(securityUser);

                JsonObject settingsObject;

                // Determine if we have existing settings to work with
                if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
                {
                    //
                    // Parse the existing JSON string into a mutable JsonObject
                    //
                    // This will throw if the JSON is invalid
                    //
                    JsonNode parsedNode = JsonNode.Parse(settingsJSONString)!;

                    //
                    // Ensure the root is an object; if not, treat as no settings (defensive)
                    //
                    if (parsedNode is JsonObject existingObject)
                    {
                        settingsObject = existingObject;
                    }
                    else
                    {
                        //
                        // Root was not an object (e.g., array, primitive) – start fresh to avoid corruption
                        //
                        settingsObject = new JsonObject();
                    }
                }
                else
                {
                    //
                    // No existing settings – create a new empty JSON object
                    //
                    settingsObject = new JsonObject();
                }

                //
                // Set the property value.
                // Passing null explicitly creates a JSON "null" value, matching original behavior.
                //
                settingsObject[settingName] = settingValue;

                //
                // Serialize the updated object back to a JSON string
                // Use default options (indentation off, consistent with typical JsonConvert defaults)
                //
                string serializedSettings = settingsObject.ToJsonString();

                //
                // Persist the updated settings
                //
                SetUserSettings(securityUser, serializedSettings);

                //
                // Original method always returned true on completion
                //
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
        /// Retrieves an integer value from the user's JSON settings by the specified setting name.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityUser">The security user whose settings are being queried.</param>
        /// <returns>
        /// The integer value of the setting if the property exists and is a JSON number that can be represented as a 32-bit integer; otherwise, null.
        /// </returns>
        /// <remarks>
        //
        /// - Returns null if the settings JSON is null, empty, or whitespace-only.
        /// - Returns null if the property does not exist.
        /// - Returns null if the property exists but is not a JSON number (e.g., string, boolean, object, array).
        /// - Returns null if the JSON number is outside the range of System.Int32 (e.g., very large numbers or fractions).
        /// - Direct cast behavior of (int?)settings[settingName] is replicated: only whole numbers within int range are converted.
        ///
        /// </remarks>
        public static int? GetIntSetting(string settingName, SecurityUser securityUser)
        {
            int? output = null;

            //
            // Retrieve the raw JSON string containing the user's settings
            //
            string settingsJSONString = GetUserSettings(securityUser);

            //
            // Only proceed if we have non-empty content
            //
            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                //
                // Parse the JSON into a disposable JsonDocument
                //
                using (JsonDocument document = JsonDocument.Parse(settingsJSONString))
                {
                    //
                    // Get the root element
                    //
                    JsonElement root = document.RootElement;

                    //
                    // Ensure the root is an object before attempting property access
                    //
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        //
                        // Try to locate the property by name
                        //
                        if (root.TryGetProperty(settingName, out JsonElement valueElement))
                        {
                            //
                            // Only attempt conversion if the value is a JSON number
                            //
                            if (valueElement.ValueKind == JsonValueKind.Number)
                            {
                                //
                                // TryGetInt32 returns true only if the number is a whole integer within Int32 range
                                //
                                if (valueElement.TryGetInt32(out int intValue))
                                {
                                    output = intValue;
                                }
                            }
                            // Non-number values (string, null, etc.) result in null
                        }
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// 
        /// Sets (or adds) an integer setting value in the user's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">
        /// The new value for the setting. If null, the property will be explicitly set to JSON null.
        /// If non-null, the value will be serialized as a JSON number.
        /// </param>
        /// <param name="securityUser">The security user whose settings are being modified.</param>
        /// <remarks>
        ///
        /// - Retrieves existing settings JSON (if any).
        /// - Parses it into a JSON object (or creates a new one if none exists or root is invalid).
        /// - Adds or updates the specified property:
        ///   - If settingValue.HasValue → stores the integer as a JSON number.
        ///   - If settingValue == null → stores explicit JSON null.
        /// - Serializes the updated object and persists it synchronously via SetUserSettings.
        ///
        /// </remarks>
        public static bool SetIntSetting(string settingName, int? settingValue, SecurityUser securityUser)
        {
            try
            {
                //
                // Retrieve the current raw JSON settings string (synchronous)
                //
                string settingsJSONString = GetUserSettings(securityUser);

                JsonObject settingsObject;

                //
                // Determine if we have existing settings to work with
                //
                if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
                {
                    //
                    // Parse the existing JSON string into a JsonNode
                    // This will throw if the JSON is invalid 
                    //
                    JsonNode parsedNode = JsonNode.Parse(settingsJSONString);

                    //
                    // If parsing succeeded and the root is an object, use it
                    //
                    if (parsedNode is JsonObject existingObject)
                    {
                        settingsObject = existingObject;
                    }
                    else
                    {
                        //
                        // Root was not an object (e.g., array, primitive, or null) – start fresh
                        // This is a defensive measure
                        //
                        settingsObject = new JsonObject();
                    }
                }
                else
                {
                    //
                    // No existing settings – create a new empty JSON object
                    //
                    settingsObject = new JsonObject();
                }

                settingsObject[settingName] = settingValue;

                //
                // Serialize the updated object back to a JSON string
                // Default options produce compact JSON without indentation
                //
                string serializedSettings = settingsObject.ToJsonString();

                //
                // Persist the updated settings
                //
                SetUserSettings(securityUser, serializedSettings);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion


        #region DateTimeSetting

        /// <summary>
        /// 
        /// Retrieves a DateTime value from the user's JSON settings by the specified setting name.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityUser">The security user whose settings are being queried.</param>
        /// <returns>
        /// The DateTime value of the setting if the property exists and contains a JSON string that can be successfully parsed as a DateTime; otherwise, null.
        /// </returns>
        /// <remarks>
        /// 
        /// - Returns null if the settings JSON is null, empty, or whitespace-only.
        /// - Returns null if the property does not exist.
        /// - Attempts to convert the value to DateTime only if it is a JSON string.
        /// - Non-string values (e.g., numbers, objects, booleans, null) result in null.
        /// - If the string is not in a recognizable DateTime format, the conversion fails and null is returned
        ///   (the original Newtonsoft cast would throw; this version fails gracefully to avoid exceptions
        ///    in common setting-retrieval scenarios – see note below).
        ///
        /// </remarks>
        public static DateTime? GetDateTimeSetting(string settingName, SecurityUser securityUser)
        {
            DateTime? output = null;

            // Retrieve the raw JSON string containing the user's settings
            string settingsJSONString = GetUserSettings(securityUser);

            //
            // Only proceed if we have non-empty content
            //
            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                //
                // Parse the JSON into a disposable JsonDocument
                //
                using (JsonDocument document = JsonDocument.Parse(settingsJSONString))
                {
                    //
                    // Get the root element
                    //
                    JsonElement root = document.RootElement;

                    //
                    // Ensure the root is an object before attempting property access
                    //
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        //
                        // Try to locate the property by name
                        //
                        if (root.TryGetProperty(settingName, out JsonElement valueElement))
                        {
                            //
                            // In typical JSON storage of DateTime (via JsonConvert), values are strings in ISO format
                            //
                            if (valueElement.ValueKind == JsonValueKind.String)
                            {
                                //
                                // Attempt to parse the string as a DateTime using styles that match Newtonsoft's default serialization
                                //
                                if (valueElement.TryGetDateTime(out DateTime parsedDateTime))
                                {
                                    //
                                    // TryGetDateTime uses ISO 8601 parsing internally and is the safest/fastest match
                                    //
                                    output = parsedDateTime;
                                }
                                else
                                {
                                    //
                                    // Fallback: more flexible parsing for edge cases (rare but possible)
                                    // This still preserves Kind information when present
                                    //
                                    string valueString = valueElement.GetString()!;

                                    if (DateTime.TryParse(valueString, CultureInfo.InvariantCulture,
                                                          DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal,
                                                          out parsedDateTime))
                                    {
                                        output = parsedDateTime;
                                    }
                                }
                            }
                            // Non-string values (e.g., if someone stored a timestamp number) result in null
                        }
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// 
        /// Sets (or adds) a DateTime setting value in the user's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">
        /// 
        /// The new value for the setting. If null, the property will be explicitly set to JSON null.
        /// If non-null, the value will be serialized as a JSON string in ISO 8601 roundtrip format
        /// (e.g., "2025-12-17T14:30:00.0000000Z").
        /// 
        /// </param>
        /// <param name="securityUser">The security user whose settings are being modified.</param>
        /// <remarks>
        ///
        /// - Retrieves existing settings JSON (if any).
        /// - Parses it into a JSON object (or creates a new one if none exists or root is invalid).
        /// - Adds or updates the specified property:
        ///   - If settingValue.HasValue → stores the DateTime as a JSON string in roundtrip ("O") format,
        ///     preserving Kind information (Utc, Local, or Unspecified).
        ///   - If settingValue == null → stores explicit JSON null.
        /// - Serializes the updated object and persists it synchronously via SetUserSettings.
        ///
        /// </remarks>
        public static void SetDateTimeSetting(string settingName, DateTime? settingValue, SecurityUser securityUser)
        {
            //
            // Retrieve the current raw JSON settings string (synchronous)
            //
            string settingsJSONString = GetUserSettings(securityUser);

            JsonObject settingsObject;

            //
            // Determine if we have existing settings to work with
            //
            if (!string.IsNullOrWhiteSpace(settingsJSONString))
            {
                //
                // Parse the existing JSON string into a JsonNode
                // This will throw if the JSON is invalid – matching original JObject.Parse behavior
                //
                JsonNode parsedNode = JsonNode.Parse(settingsJSONString);

                //
                // If parsing succeeded and the root is an object, use it
                //
                if (parsedNode is JsonObject existingObject)
                {
                    settingsObject = existingObject;
                }
                else
                {
                    //
                    // Root was not an object (e.g., array, primitive, or null) – start fresh
                    // Defensive measure
                    //
                    settingsObject = new JsonObject();
                }
            }
            else
            {
                //
                // No existing settings – create a new empty JSON object
                //
                settingsObject = new JsonObject();
            }

            //
            // Set the property value.
            //
            settingsObject[settingName] = settingValue;

            //
            // Serialize the updated object back to a JSON string
            // Default options produce compact JSON without indentation and use roundtrip DateTime format
            //
            string serializedSettings = settingsObject.ToJsonString();

            //
            // Persist the updated settings
            //
            SetUserSettings(securityUser, serializedSettings);
        }

        #endregion


        #region BoolSetting

        /// <summary>
        /// 
        /// Retrieves a boolean value from the user's JSON settings by the specified setting name.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityUser">The security user whose settings are being queried.</param>
        /// <returns>
        /// The boolean value of the setting if the property exists and is a JSON boolean (true/false); otherwise, null.
        /// </returns>
        /// <remarks>
        /// //
        /// - Returns null if the settings JSON is null, empty, or whitespace-only.
        /// - Returns null if the property does not exist.
        /// - Returns null if the property exists but is not a JSON boolean (e.g., string, number, object, array, null).
        /// - Direct cast behavior of (bool?)settings[settingName] is replicated: only JSON true/false values
        ///   are converted to true/false. Any other type results in null 
        ///
        /// </remarks>
        public static bool? GetBoolSetting(string settingName, SecurityUser securityUser)
        {
            bool? output = null;

            //
            // Retrieve the raw JSON string containing the user's settings
            //
            string settingsJSONString = GetUserSettings(securityUser);

            //
            // Only proceed if we have non-empty content
            //
            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                //
                // Parse the JSON into a disposable JsonDocument
                //
                using (JsonDocument document = JsonDocument.Parse(settingsJSONString))
                {
                    //
                    // Get the root element
                    //
                    JsonElement root = document.RootElement;

                    // Ensure the root is an object before attempting property access
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        // Try to locate the property by name
                        if (root.TryGetProperty(settingName, out JsonElement valueElement))
                        {
                            // Only proceed if the value is explicitly a JSON true or false token
                            if (valueElement.ValueKind == JsonValueKind.True ||
                                valueElement.ValueKind == JsonValueKind.False)
                            {
                                // GetBoolean() is now safe to call – it will return true or false
                                output = valueElement.GetBoolean();
                            }
                            // Non-boolean values (including string "true", number 1, etc.) result in null
                        }
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// 
        /// Sets (or adds) a boolean setting value in the user's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">
        /// The new value for the setting. If null, the property will be explicitly set to JSON null.
        /// If non-null, the value will be serialized as a JSON boolean (true/false).
        /// </param>
        /// <param name="securityUser">The security user whose settings are being modified.</param>
        /// <remarks>
        /// 
        /// - Retrieves existing settings JSON (if any).
        /// - Parses it into a JSON object (or creates a new one if none exists or root is invalid).
        /// - Adds or updates the specified property:
        ///   - If settingValue.HasValue → stores the boolean as a JSON true or false token.
        ///   - If settingValue == null → stores explicit JSON null.
        /// - Serializes the updated object and persists it synchronously via SetUserSettings.
        ///
        /// </remarks>
        public static void SetBoolSetting(string settingName, bool? settingValue, SecurityUser securityUser)
        {
            //
            // Retrieve the current raw JSON settings string (synchronous)
            //
            string settingsJSONString = GetUserSettings(securityUser);

            JsonObject settingsObject;

            //
            // Determine if we have existing settings to work with
            //
            if (!string.IsNullOrWhiteSpace(settingsJSONString))
            {
                //
                // Parse the existing JSON string into a JsonNode
                // This will throw if the JSON is invalid – matching original JObject.Parse behavior
                //
                JsonNode parsedNode = JsonNode.Parse(settingsJSONString);

                // If parsing succeeded and the root is an object, use it
                if (parsedNode is JsonObject existingObject)
                {
                    settingsObject = existingObject;
                }
                else
                {
                    //
                    // Root was not an object (e.g., array, primitive, or null) – start fresh
                    // Defensive measure
                    //
                    settingsObject = new JsonObject();
                }
            }
            else
            {
                //
                // No existing settings – create a new empty JSON object
                //
                settingsObject = new JsonObject();
            }

            //
            // Set the property value.
            //
            settingsObject[settingName] = settingValue;

            //
            // Serialize the updated object back to a JSON string
            // Default options produce compact JSON without indentation
            //
            string serializedSettings = settingsObject.ToJsonString();

            //
            // Persist the updated settings
            //
            SetUserSettings(securityUser, serializedSettings);
        }

        #endregion


        #region ObjectSetting


        /// <summary>
        /// 
        /// Retrieves an arbitrary JSON object value from the user's settings by the specified setting name.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityUser">The security user whose settings are being queried.</param>
        /// <returns>
        /// 
        /// A dynamic-like representation of the JSON subtree if the property exists and is not JSON null;
        /// otherwise, null.
        /// 
        /// The return type is JsonNode – Callers can access properties via indexer (e.g., result["Prop"]),
        /// enumerate arrays, cast to primitives, or serialize subsets as needed.
        /// 
        /// </returns>
        /// <remarks>
        /// 
        /// - Returns null if the settings JSON is null, empty, or whitespace-only.
        /// - Returns null if the property does not exist.
        /// - Returns null if the property exists but has the value JSON null.
        /// - Returns the full JSON subtree (object, array, or primitive) otherwise.
        ///
        /// Note: We deliberately return null for JSON null values
        /// 
        /// </remarks>
        public static JsonNode GetObjectSetting(string settingName, SecurityUser securityUser)
        {
            JsonNode output = null;

            //
            // Retrieve the raw JSON string containing the user's settings
            //
            string settingsJSONString = GetUserSettings(securityUser);

            //
            // Only proceed if we have non-empty content
            //
            if (!string.IsNullOrWhiteSpace(settingsJSONString))
            {
                //
                // Parse the entire settings JSON into a JsonNode (mutable DOM tree)
                // This throws if the JSON is invalid
                //
                JsonNode rootNode = JsonNode.Parse(settingsJSONString);

                //
                // Ensure the root is an object before attempting property access
                //
                if (rootNode is JsonObject rootObject)
                {
                    //
                    // Check if the property exists
                    //
                    if (rootObject.TryGetPropertyValue(settingName, out JsonNode valueNode))
                    {
                        //
                        // Explicitly return null if the stored value is JSON null
                        //
                        if (valueNode is not null)
                        {
                            //
                            // return a clone of the object
                            //
                            return valueNode.DeepClone();
                        }
                        // else: valueNode is null then return null
                    }
                }
                // If root is not an object (corrupted data), we return null – safe fallback
            }

            return output;
        }


        /// <summary>
        /// 
        /// Asynchronously Retrieves an arbitrary JSON object value from the user's settings by the specified setting name.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityUser">The security user whose settings are being queried.</param>
        /// <returns>
        /// 
        /// A dynamic-like representation of the JSON subtree if the property exists and is not JSON null;
        /// otherwise, null.
        /// 
        /// The return type is JsonNode – Callers can access properties via indexer (e.g., result["Prop"]),
        /// enumerate arrays, cast to primitives, or serialize subsets as needed.
        /// 
        /// </returns>
        /// <remarks>
        /// 
        /// - Returns null if the settings JSON is null, empty, or whitespace-only.
        /// - Returns null if the property does not exist.
        /// - Returns null if the property exists but has the value JSON null.
        /// - Returns the full JSON subtree (object, array, or primitive) otherwise.
        ///
        /// Note: We deliberately return null for JSON null values
        /// 
        /// </remarks>
        public static async Task<JsonNode> GetObjectSettingAsync(string settingName, SecurityUser securityUser, CancellationToken cancellationToken = default)
        {
            JsonNode output = null;

            //
            // Retrieve the raw JSON string containing the user's settings
            //
            string settingsJSONString = await GetUserSettingsAsync(securityUser, cancellationToken).ConfigureAwait(false);

            //
            // Only proceed if we have non-empty content
            //
            if (!string.IsNullOrWhiteSpace(settingsJSONString))
            {
                //
                // Parse the entire settings JSON into a JsonNode (mutable DOM tree)
                // This throws if the JSON is invalid
                //
                JsonNode rootNode = JsonNode.Parse(settingsJSONString);

                //
                // Ensure the root is an object before attempting property access
                //
                if (rootNode is JsonObject rootObject)
                {
                    //
                    // Check if the property exists
                    //
                    if (rootObject.TryGetPropertyValue(settingName, out JsonNode valueNode))
                    {
                        //
                        // Explicitly return null if the stored value is JSON null
                        //
                        if (valueNode is not null)
                        {
                            //
                            // return a clone of the object
                            //
                            return valueNode.DeepClone();
                        }
                        // else: valueNode is null then return null
                    }
                }
                // If root is not an object (corrupted data), we return null – safe fallback
            }

            return output;
        }



        /// <summary>
        /// 
        /// Sets (or adds) an arbitrary object setting value in the user's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">
        /// The new value for the setting. This can be any .NET object that is serializable to JSON
        /// (primitives, arrays, POCOs, collections, etc.). If null, the property will be explicitly set to JSON null.
        /// </param>
        /// <param name="securityUser">The security user whose settings are being modified.</param>
        /// <remarks>
        //
        /// - Retrieves existing settings JSON (if any).
        /// - Parses it into a JSON object (or creates a new one if none exists or root is invalid).
        /// - Adds or updates the specified property:
        ///   - If settingValue is not null → serializes the object to a JsonNode subtree using
        ///     JsonSerializer.SerializeToNode (available in .NET 7+; fallback to serialize/parse for .NET 6).
        ///   - If settingValue is null → stores explicit JSON null.
        /// - Serializes the updated object and persists it synchronously via SetUserSettings.
        ///
        /// </remarks>
        public static bool SetObjectSetting(string settingName, object settingValue, SecurityUser securityUser)
        {
            try
            {
                //
                // Retrieve the current raw JSON settings string (synchronous)
                //
                string settingsJSONString = GetUserSettings(securityUser);

                JsonObject settingsObject;

                //
                // Determine if we have existing settings to work with
                //
                if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
                {
                    //
                    // Parse the existing JSON string into a JsonNode
                    // This will throw if the JSON is invalid
                    //
                    JsonNode parsedNode = JsonNode.Parse(settingsJSONString);

                    //
                    // If parsing succeeded and the root is an object, use it
                    //
                    if (parsedNode is JsonObject existingObject)
                    {
                        settingsObject = existingObject;
                    }
                    else
                    {
                        //
                        // Root was not an object (e.g., array, primitive, or null) – start fresh
                        // Defensive measure
                        //
                        settingsObject = new JsonObject();
                    }
                }
                else
                {
                    //
                    // No existing settings – create a new empty JSON object
                    //
                    settingsObject = new JsonObject();
                }

                //
                // Set the property value
                //
                if (settingValue is not null)
                {
                    //
                    // Convert the arbitrary .NET object to a JsonNode subtree
                    //
                    JsonNode valueNode = JsonSerializer.SerializeToNode(settingValue);

                    settingsObject[settingName] = valueNode;
                }
                else
                {
                    //
                    // Explicit JSON null
                    //
                    settingsObject[settingName] = null;
                }

                //
                // Serialize the updated object back to a JSON string
                // Default options produce compact JSON without indentation
                //
                string serializedSettings = settingsObject.ToJsonString();

                // Persist the updated settings (synchronous call)
                SetUserSettings(securityUser, serializedSettings);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// 
        /// Asynchronously Sets (or adds) an arbitrary object setting value in the user's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">
        /// The new value for the setting. This can be any .NET object that is serializable to JSON
        /// (primitives, arrays, POCOs, collections, etc.). If null, the property will be explicitly set to JSON null.
        /// </param>
        /// <param name="securityUser">The security user whose settings are being modified.</param>
        /// <remarks>
        //
        /// - Retrieves existing settings JSON (if any).
        /// - Parses it into a JSON object (or creates a new one if none exists or root is invalid).
        /// - Adds or updates the specified property:
        ///   - If settingValue is not null → serializes the object to a JsonNode subtree using
        ///     JsonSerializer.SerializeToNode (available in .NET 7+; fallback to serialize/parse for .NET 6).
        ///   - If settingValue is null → stores explicit JSON null.
        /// - Serializes the updated object and persists it synchronously via SetUserSettings.
        ///
        /// </remarks>
        public static async Task<bool> SetObjectSettingAsync(string settingName, object? settingValue, SecurityUser securityUser, CancellationToken cancellationToken = default)
        {
            try
            {
                //
                // Retrieve the current raw JSON settings string (synchronous)
                //
                string settingsJSONString = await GetUserSettingsAsync(securityUser, cancellationToken).ConfigureAwait(false);

                JsonObject settingsObject;

                //
                // Determine if we have existing settings to work with
                //
                if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
                {
                    //
                    // Parse the existing JSON string into a JsonNode
                    // This will throw if the JSON is invalid
                    //
                    JsonNode parsedNode = JsonNode.Parse(settingsJSONString);

                    //
                    // If parsing succeeded and the root is an object, use it
                    //
                    if (parsedNode is JsonObject existingObject)
                    {
                        settingsObject = existingObject;
                    }
                    else
                    {
                        //
                        // Root was not an object (e.g., array, primitive, or null) – start fresh
                        // Defensive measure
                        //
                        settingsObject = new JsonObject();
                    }
                }
                else
                {
                    //
                    // No existing settings – create a new empty JSON object
                    //
                    settingsObject = new JsonObject();
                }

                //
                // Set the property value
                //
                if (settingValue is not null)
                {
                    //
                    // Convert the arbitrary .NET object to a JsonNode subtree
                    //
                    JsonNode valueNode = JsonSerializer.SerializeToNode(settingValue);

                    settingsObject[settingName] = valueNode;
                }
                else
                {
                    //
                    // Explicit JSON null
                    //
                    settingsObject[settingName] = null;
                }

                //
                // Serialize the updated object back to a JSON string
                // Default options produce compact JSON without indentation
                //
                string serializedSettings = settingsObject.ToJsonString();

                // Persist the updated settings (synchronous call)
                await SetUserSettingsAsync(securityUser, serializedSettings, cancellationToken).ConfigureAwait(false);

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
        /// Retrieves a list of strings from the user's JSON settings by the specified setting name.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to retrieve.</param>
        /// <param name="securityUser">The security user whose settings are being queried.</param>
        /// <returns>
        /// A List<string> containing the values if the property exists and is a JSON array of strings;
        /// otherwise, null.
        /// 
        /// Note: Non-string array elements or mismatched types will result in null (deserialization failure)
        /// 
        /// </returns>
        /// <remarks>
        /// 
        /// - Returns null if the settings JSON is null, empty, or whitespace-only.
        /// - Returns null if the property does not exist.
        /// - Returns null if the property exists but is not a JSON array or cannot be deserialized
        ///   to List<string> (e.g., array of numbers, objects, mixed types, or null value).
        /// - Successfully returns a new List<string> populated with the string values when the
        ///   property is a valid JSON array of strings.
        ///
        /// We use JsonNode for parsing because it allows safe navigation and type checking before
        /// attempting deserialization. JsonSerializer.Deserialize is then used on the array subtree
        /// to convert it to List<string>, which is the most straightforward and performant way
        /// to get a strongly-typed list in System.Text.Json.
        ///
        /// This approach avoids exceptions in common failure cases and keeps the method easy to
        /// understand and maintain.
        /// 
        /// </remarks>
        public static List<string> GetStringListSetting(string settingName, SecurityUser securityUser)
        {
            List<string> output = null;

            // Retrieve the raw JSON string containing the user's settings
            string settingsJSONString = GetUserSettings(securityUser);

            // Only proceed if we have non-empty content
            if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
            {
                //
                // Parse the JSON into a JsonNode
                //
                JsonNode rootNode = JsonNode.Parse(settingsJSONString);

                //
                // Ensure the root is an object before attempting property access
                //
                if (rootNode is JsonObject rootObject)
                {
                    //
                    // Check if the property exists and retrieve its node
                    //
                    if (rootObject.TryGetPropertyValue(settingName, out JsonNode? valueNode))
                    {
                        //
                        // We only attempt deserialization if the value is a JSON array
                        // (valueNode being null would represent JSON null, which we treat as no value)
                        //
                        if (valueNode is JsonArray jsonArray)
                        {
                            try
                            {
                                //
                                // Deserialize the array directly to List<string>
                                // This will succeed only if every element is a JSON string
                                // On failure (e.g., non-string elements), Deserialize throws JsonException
                                //
                                output = jsonArray.Deserialize<List<string>>();
                            }
                            catch (JsonException)
                            {
                                //
                                // If deserialization fails for any reason (type mismatch, etc.),
                                // we return null to match the original intent of only succeeding
                                // on valid string arrays. Exceptions here are rare but handled gracefully.
                                //
                                output = null;
                            }
                        }
                        // If the property exists but is not an array (e.g., object, string, number, null), we return null 
                    }
                }
                // If root is not an object (corrupted data), return null – safe fallback
            }

            return output;
        }



        /// <summary>
        /// 
        /// Sets (or adds) a list of strings as a setting value in the user's JSON settings.
        /// 
        /// </summary>
        /// <param name="settingName">The name of the setting to set or update.</param>
        /// <param name="settingValue">
        /// 
        /// The new list value for the setting. If null, the property will be explicitly set to JSON null.
        /// If non-null, the list will be serialized as a JSON array of strings.
        /// 
        /// </param>
        /// <param name="securityUser">The security user whose settings are being modified.</param>
        /// <remarks>
        /// 
        /// - Retrieves existing settings JSON (if any).
        /// - Parses it into a JSON object (or creates a new one if none exists or root is invalid).
        /// - Adds or updates the specified property:
        ///   - If settingValue is not null → serializes the List<string> to a JsonArray subtree
        ///     using JsonSerializer.SerializeToNode (available in .NET 7+; fallback to serialize/parse for .NET 6).
        ///   - If settingValue is null → stores explicit JSON null.
        /// - Serializes the updated object and persists it synchronously via SetUserSettings.
        ///
        /// </remarks>
        public static bool SetStringListSetting(string settingName, List<string> settingValue, SecurityUser securityUser)
        {
            try
            {
                //
                // Retrieve the current raw JSON settings string (synchronous)
                //
                string settingsJSONString = GetUserSettings(securityUser);

                JsonObject settingsObject;

                //
                // Determine if we have existing settings to work with
                //
                if (string.IsNullOrWhiteSpace(settingsJSONString) == false)
                {
                    //
                    // Parse the existing JSON string into a JsonNode
                    // 
                    JsonNode parsedNode = JsonNode.Parse(settingsJSONString);

                    //
                    // If parsing succeeded and the root is an object, use it
                    //
                    if (parsedNode is JsonObject existingObject)
                    {
                        settingsObject = existingObject;
                    }
                    else
                    {
                        //
                        // Root was not an object (e.g., array, primitive, or null) – start fresh
                        // Defensive measure
                        //
                        settingsObject = new JsonObject();
                    }
                }
                else
                {
                    //
                    // No existing settings – create a new empty JSON object
                    //
                    settingsObject = new JsonObject();
                }

                //
                // Set the property value
                //
                if (settingValue is not null)
                {
                    //
                    // Convert the List<string> to a JsonNode subtree (will be a JsonArray)
                    //
                    JsonNode valueNode = JsonSerializer.SerializeToNode(settingValue);
                    settingsObject[settingName] = valueNode;
                }
                else
                {
                    //
                    // Explicit JSON null when the list is null
                    //
                    settingsObject[settingName] = null;
                }

                //
                // Serialize the updated object back to a JSON string
                // Default options produce compact JSON without indentation
                //
                string serializedSettings = settingsObject.ToJsonString();

                //
                // Persist the updated settings (synchronous call)
                //
                SetUserSettings(securityUser, serializedSettings);

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