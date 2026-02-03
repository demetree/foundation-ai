using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation.Security
{
    public class SystemSettings
    {
        public class SystemSettingSyncRoot
        {
            private static volatile SystemSettingSyncRoot instance;
            public static object syncRoot = new object();

            public static SystemSettingSyncRoot Instance
            {
                get
                {
                    if (instance == null)
                    {
                        lock (syncRoot)
                        {
                            if (instance == null)
                            {
                                instance = new SystemSettingSyncRoot();
                            }
                        }
                    }

                    return instance;
                }
            }
        }


        #region String Settings (Sync)


        public static string GetSystemSetting(string name, string defaultValue = null)
        {
            using (SecurityContext db = new SecurityContext())
            {

                SystemSetting setting = (from ss in db.SystemSettings
                                         where ss.name.ToUpper() == name.ToUpper()
                                            && ss.active == true
                                            && ss.deleted == false
                                         select ss).FirstOrDefault();

                if (setting != null)
                {
                    return setting.value;
                }
                else
                {
                    //
                    // No setting, but was read. Create it as a convenience using the default value provided
                    //
                    SetSystemSetting(name, defaultValue);

                    return defaultValue;
                }
            }
        }

        public static void SetSystemSetting(string name, string value)
        {
            lock (SystemSettingSyncRoot.syncRoot)
            {
                using (SecurityContext db = new SecurityContext())
                {
                    if (name != null)
                    {
                        SystemSetting setting = (from ss in db.SystemSettings
                                                 where ss.name.ToUpper() == name.ToUpper()
                                                    && ss.deleted == false
                                                 select ss).FirstOrDefault();


                        if (setting != null)
                        {
                            setting.value = value;
                            setting.active = true;
                            db.Entry(setting).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            setting = new SystemSetting();

                            setting.name = name;
                            setting.description = setting.name;
                            setting.value = value;

                            setting.active = true;
                            setting.deleted = false;

                            db.SystemSettings.Add(setting);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }


        #endregion


        #region String Settings (Async)


        public static async Task<string> GetSystemSettingAsync(string name, string defaultValue = null, CancellationToken cancellationToken = default)
        {
            using (SecurityContext db = new SecurityContext())
            {
                SystemSetting setting = await (from ss in db.SystemSettings
                                               where ss.name.ToUpper() == name.ToUpper()
                                                  && ss.active == true
                                                  && ss.deleted == false
                                               select ss).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                if (setting != null)
                {
                    return setting.value;
                }
                else
                {
                    //
                    // No setting, but was read. Create it as a convenience using the default value provided
                    //
                    await SetSystemSettingAsync(name, defaultValue, cancellationToken).ConfigureAwait(false);

                    return defaultValue;
                }
            }
        }

        public static async Task SetSystemSettingAsync(string name, string value, CancellationToken cancellationToken = default)
        {
            if (name == null) return;

            using (SecurityContext db = new SecurityContext())
            {
                SystemSetting setting = await (from ss in db.SystemSettings
                                               where ss.name.ToUpper() == name.ToUpper()
                                                  && ss.deleted == false
                                               select ss).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                if (setting != null)
                {
                    setting.value = value;
                    setting.active = true;
                    db.Entry(setting).State = EntityState.Modified;
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    setting = new SystemSetting();

                    setting.name = name;
                    setting.description = setting.name;
                    setting.value = value;

                    setting.active = true;
                    setting.deleted = false;

                    db.SystemSettings.Add(setting);
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }


        #endregion


        #region Int Settings


        public static int? GetIntSystemSetting(string settingName, int? defaultValue = null)
        {
            int? output = null;

            string settingsString = GetSystemSetting(settingName, defaultValue.HasValue == true ? defaultValue.ToString() : null);

            int intFromString;
            if (settingsString != null &&
                int.TryParse(settingsString, out intFromString) == true)
            {
                output = intFromString;
            }

            return output;
        }


        public static void SetIntSystemSetting(string settingName, int? settingValue)
        {

            if (settingValue.HasValue == true)
            {
                SetSystemSetting(settingName, settingValue.Value.ToString());
            }
            else
            {
                SetSystemSetting(settingName, null);
            }
        }


        #endregion


        #region DateTime Settings


        public static DateTime? GetDateTimeSystemSetting(string settingName, DateTime? defaultValue = null)
        {
            DateTime? output = null;

            string settingsString = GetSystemSetting(settingName, defaultValue.HasValue == true ? defaultValue.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null);

            DateTime dateTimeFromString;
            if (settingsString != null &&
                DateTime.TryParse(settingsString, out dateTimeFromString) == true)
            {
                output = dateTimeFromString;
            }

            return output;
        }


        public static void SetDateTimeSystemSetting(string settingName, DateTime? settingValue)
        {

            if (settingValue.HasValue == true)
            {
                SetSystemSetting(settingName, settingValue.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            }
            else
            {
                SetSystemSetting(settingName, null);
            }
        }


        #endregion


        #region Boolean Settings


        public static Boolean? GetBooleanSystemSetting(string settingName, Boolean? defaultValue = null)
        {
            Boolean? output = null;

            string settingsString = GetSystemSetting(settingName, defaultValue.HasValue == true ? defaultValue.ToString() : null);

            Boolean boolString;
            if (settingsString != null &&
                Boolean.TryParse(settingsString, out boolString) == true)
            {
                output = boolString;
            }

            return output;
        }


        public static void SetBooleanSystemSetting(string settingName, Boolean? settingValue)
        {
            if (settingValue.HasValue == true)
            {
                SetSystemSetting(settingName, settingValue.Value.ToString());
            }
            else
            {
                SetSystemSetting(settingName, null);
            }
        }


        #endregion


        #region Object Settings


        public static Object GetObjectSystemSetting(string settingName)
        {
            Object output = null;

            string settingsString = GetSystemSetting(settingName);

            output = settingsString;

            return output;
        }


        public static void SetObjectSystemSetting(string settingName, Object settingValue)
        {
            if (settingValue != null)
            {
                SetSystemSetting(settingName, settingValue.ToString());
            }
            else
            {
                SetSystemSetting(settingName, null);
            }
        }


        #endregion


        #region String List Settings


        /// <summary>
        /// Retrieves a list of strings from a system setting stored as JSON array.
        /// </summary>
        /// <param name="settingName">The name of the setting.</param>
        /// <returns>List of strings, or empty list if not found or invalid JSON.</returns>
        public static List<string> GetStringListSystemSetting(string settingName)
        {
            List<string> output = new List<string>();

            string settingsString = GetSystemSetting(settingName);

            if (string.IsNullOrWhiteSpace(settingsString) == false)
            {
                try
                {
                    output = JsonSerializer.Deserialize<List<string>>(settingsString) ?? new List<string>();
                }
                catch
                {
                    // Invalid JSON, return empty list
                }
            }

            return output;
        }


        /// <summary>
        /// Stores a list of strings as a JSON array in a system setting.
        /// </summary>
        /// <param name="settingName">The name of the setting.</param>
        /// <param name="settingValue">The list of strings to store.</param>
        public static void SetStringListSystemSetting(string settingName, List<string> settingValue)
        {
            if (settingValue != null)
            {
                string jsonValue = JsonSerializer.Serialize(settingValue);
                SetSystemSetting(settingName, jsonValue);
            }
            else
            {
                SetSystemSetting(settingName, null);
            }
        }


        #endregion


        #region Delete


        /// <summary>
        /// Soft-deletes a system setting by name.
        /// </summary>
        /// <param name="settingName">The name of the setting to delete.</param>
        /// <returns>True if deleted; false if not found.</returns>
        public static bool DeleteSystemSetting(string settingName)
        {
            lock (SystemSettingSyncRoot.syncRoot)
            {
                using (SecurityContext db = new SecurityContext())
                {
                    if (settingName == null) return false;

                    SystemSetting setting = (from ss in db.SystemSettings
                                             where ss.name.ToUpper() == settingName.ToUpper()
                                                && ss.deleted == false
                                             select ss).FirstOrDefault();

                    if (setting != null)
                    {
                        setting.deleted = true;
                        setting.active = false;
                        db.Entry(setting).State = EntityState.Modified;
                        db.SaveChanges();
                        return true;
                    }

                    return false;
                }
            }
        }


        /// <summary>
        /// Asynchronously soft-deletes a system setting by name.
        /// </summary>
        /// <param name="settingName">The name of the setting to delete.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if deleted; false if not found.</returns>
        public static async Task<bool> DeleteSystemSettingAsync(string settingName, CancellationToken cancellationToken = default)
        {
            if (settingName == null) return false;

            using (SecurityContext db = new SecurityContext())
            {
                SystemSetting setting = await (from ss in db.SystemSettings
                                               where ss.name.ToUpper() == settingName.ToUpper()
                                                  && ss.deleted == false
                                               select ss).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                if (setting != null)
                {
                    setting.deleted = true;
                    setting.active = false;
                    db.Entry(setting).State = EntityState.Modified;
                    await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    return true;
                }

                return false;
            }
        }


        #endregion

    }
}