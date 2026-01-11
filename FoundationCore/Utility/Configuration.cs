using Foundation.Concurrent;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Foundation
{
    public class Configuration
    {
        private const string MULTI_TENANCY_MODE_SETTING = "MultiTenancyMode";
        private const string DATA_VISIBILITY_MODE_SETTING = "DataVisibilityMode";
        private const string DISK_BASED_BINARY_STORAGE_MODE = "DiskBasedBinaryStorageMode";

        private const string SETTINGS_OBJECT_NAME = "Settings";
        private const string CONNECTION_STRINGS_OBJECT_NAME = "ConnectionStrings";

        private static ExpiringCache<string, string> expiringStringCache = new Concurrent.ExpiringCache<string, string>(60);
        private static ExpiringCache<string, bool> expiringBoolCache = new Concurrent.ExpiringCache<string, bool>(60);
        private static ExpiringCache<string, int> expiringIntCache = new Concurrent.ExpiringCache<string, int>(60);
        private static ExpiringCache<string, float> expiringFloatCache = new Concurrent.ExpiringCache<string, float>(60);
        private static ExpiringCache<string, string> connectionStringCache = new Concurrent.ExpiringCache<string, string>(60);

        private static ExpiringCache<string, List<string>> expiringListCache = new Concurrent.ExpiringCache<string, List<string>>(60);


        /// <summary>
        /// This retrieves a configuration object that reads from the configuration files.
        /// </summary>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration()
        {
            List<string> appSettingsFiles = GetConfigurationFileNames();

            ConfigurationBuilder configBuilder = new ConfigurationBuilder();

            foreach (string appSettingsFile in appSettingsFiles)
            {
                if (System.IO.File.Exists(appSettingsFile) == true)
                {
                    configBuilder.AddJsonFile(appSettingsFile);
                }
            }

            return configBuilder.Build();
        }


        public static List<string> GetConfigurationFileNames()
        {
            List<string> output = new List<string>();

            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";


            // Always add appsettings.json first .
            output.Add(Path.Combine(currentPath, "appsettings.json"));

            // Them if there is an environment specific file, then add that one too.
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            // If we have an aspnet environment variable, and there is an associated config file that exists, then use it. 
            if (string.IsNullOrEmpty(env) == false)
            {
                string environmentSpecificSettingsFile = "appsettings." + env + ".json";

                if (File.Exists(environmentSpecificSettingsFile) == true)
                {
                    output.Add(Path.Combine(currentPath, environmentSpecificSettingsFile));
                }
            }

            return output;
        }


        /// <summary>
        /// Gets a list of a given type from configuration for a given key, with caching.
        /// </summary>
        public static List<string> GetListOfStringsConfigurationSetting(string settingName, List<string> valueForNoSetting = null)
        {
            if (expiringListCache.TryGetValue(settingName, out List<string> cachedValue) && cachedValue is List<string> cachedList)
            {
                return cachedList;
            }
            else
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" ||
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    configurationBuilder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                }

                IConfiguration configuration = configurationBuilder.Build();

                try
                {
                    var section = configuration.GetRequiredSection(SETTINGS_OBJECT_NAME).GetSection(settingName);
                    var values = section.Get<List<string>>();
                    if (values != null && values.Count > 0)
                    {
                        expiringListCache.Add(settingName, values);
                        return values;
                    }
                    else
                    {
                        expiringListCache.Add(settingName, valueForNoSetting);
                        return valueForNoSetting;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Caught error attempting to read list setting {settingName} from appsettings.json.  Error is: {ex.Message}");
                    expiringListCache.Add(settingName, valueForNoSetting);
                    return valueForNoSetting;
                }
            }
        }

        public static string GetStringConfigurationSetting(string settingName, string valueForNoSetting = null)
        {
            if (expiringStringCache.TryGetValue(settingName, out string cachedValue) == true)
            {
                return cachedValue;
            }
            else
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                // Check for a custom or standard environment variable
                if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" ||
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    configurationBuilder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                }

                IConfiguration configuration = configurationBuilder.Build();

                try
                {
                    string value = configuration.GetRequiredSection(SETTINGS_OBJECT_NAME).GetValue<string>(settingName);

                    if (value != null &&
                        value.Length > 0)
                    {
                        expiringStringCache.Add(settingName, value);

                        return value;
                    }
                    else
                    {
                        expiringStringCache.Add(settingName, valueForNoSetting);

                        return valueForNoSetting;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Caught error attempting to read setting {settingName} from appsettings.json.  Error is: {ex.Message}");
                    return valueForNoSetting;
                }
            }
        }


        public static bool GetBooleanConfigurationSetting(string settingName, bool valueForNoSetting = false)
        {
            if (expiringBoolCache.TryGetValue(settingName, out bool cachedValue) == true)
            {
                return cachedValue;
            }
            else
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                // Check for a custom or standard environment variable
                if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" ||
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    configurationBuilder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                }

                IConfiguration configuration = configurationBuilder.Build();

                bool? value = configuration.GetRequiredSection(SETTINGS_OBJECT_NAME).GetValue<bool?>(settingName);

                if (value != null)
                {
                    expiringBoolCache.Add(settingName, value.Value);

                    return value.Value;
                }
                else
                {
                    expiringBoolCache.Add(settingName, valueForNoSetting);

                    return valueForNoSetting;
                }
            }
        }


        public static int GetIntegerConfigurationSetting(string settingName, int valueForNoSetting = 0)
        {
            if (expiringIntCache.TryGetValue(settingName, out int cachedValue) == true)
            {
                return cachedValue;
            }
            else
            {

                // Do we have an override, and is it a number?
                //string value = ConfigurationManager.AppSettings[settingName];

                //IConfiguration configuration = new ConfigurationBuilder()
                //    .AddJsonFile("appsettings.json")
                //    .Build();


                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                // Check for a custom or standard environment variable
                if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" ||
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    configurationBuilder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                }

                IConfiguration configuration = configurationBuilder.Build();

                int? value = configuration.GetRequiredSection(SETTINGS_OBJECT_NAME).GetValue<int?>(settingName);


                if (value != null)
                {
                    expiringIntCache.Add(settingName, value.Value);

                    return value.Value;
                }
                else
                {
                    expiringIntCache.Add(settingName, valueForNoSetting);

                    return valueForNoSetting;
                }
            }
        }

        public static float GetFloatConfigurationSetting(string settingName, float valueForNoSetting = 0)
        {
            if (expiringFloatCache.TryGetValue(settingName, out float cachedValue) == true)
            {
                return cachedValue;
            }
            else
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                // Check for a custom or standard environment variable
                if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" ||
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    configurationBuilder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                }

                IConfiguration configuration = configurationBuilder.Build();

                float? value = configuration.GetRequiredSection(SETTINGS_OBJECT_NAME).GetValue<float?>(settingName);


                if (value != null)
                {
                    expiringFloatCache.Add(settingName, value.Value);

                    return value.Value;
                }
                else
                {
                    expiringFloatCache.Add(settingName, valueForNoSetting);

                    return valueForNoSetting;
                }
            }
        }

        public static string GetConnectionString(string connectionStringName, string valueForNoSetting = null)
        {
            if (connectionStringCache.TryGetValue(connectionStringName, out string cachedValue) == true)
            {
                return cachedValue;
            }
            else
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                // Check for a custom or standard environment variable
                if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development" ||
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    configurationBuilder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                }

                IConfiguration configuration = configurationBuilder.Build();

                try
                {
                    string value = configuration.GetRequiredSection(CONNECTION_STRINGS_OBJECT_NAME).GetValue<string>(connectionStringName);

                    if (value != null &&
                        value.Length > 0)
                    {
                        connectionStringCache.Add(connectionStringName, value);

                        return value;
                    }
                    else
                    {
                        connectionStringCache.Add(connectionStringName, valueForNoSetting);

                        return valueForNoSetting;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Caught error attempting to read connection string {connectionStringName} from appsettings.json.  Error is: {ex.Message}");
                    return valueForNoSetting;
                }
            }
        }


        public static bool GetMultiTenancyMode()
        {
            return GetBooleanConfigurationSetting(MULTI_TENANCY_MODE_SETTING, false);
        }

        public static bool GetDataVisibilityMode()
        {
            return GetBooleanConfigurationSetting(DATA_VISIBILITY_MODE_SETTING, false);
        }

        public static bool GetDiskBasedBinaryStorageMode()
        {
            return GetBooleanConfigurationSetting(DISK_BASED_BINARY_STORAGE_MODE, false);
        }
    }
}
