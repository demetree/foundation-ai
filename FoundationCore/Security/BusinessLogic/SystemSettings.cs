using Foundation.Security.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;

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


        public static string GetSystemSetting(string name, string defaultValue = null)
        {
            using (SecurityContext db = new SecurityContext())
            {

                SystemSetting setting = (from ss in db.SystemSettings
                                         where ss.name.ToUpper() == name.ToUpper()
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
            using (SecurityContext db = new SecurityContext())
            {
                lock (SystemSettingSyncRoot.syncRoot)
                {
                    if (name != null)
                    {
                        SystemSetting setting = (from ss in db.SystemSettings
                                                 where ss.name.ToUpper() == name.ToUpper()
                                                 select ss).FirstOrDefault();


                        if (setting != null)
                        {
                            setting.value = value;
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
    }
}