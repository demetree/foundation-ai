using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;

namespace Foundation.Web.Utility
{
    public static class Config
    {
        /// <summary>
        /// This retrieves a configuration object that reads from the appsettings.json file.
        /// </summary>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration()
        {
            List<string> appSettingsFiles = GetConfigurationFileNames();

            ConfigurationBuilder configBuilder = new ConfigurationBuilder();

            foreach (string appSettingsFile in appSettingsFiles)
            {
                configBuilder.AddJsonFile(appSettingsFile);
            }

            return configBuilder.Build();
        }


        public static List<string> GetConfigurationFileNames()
        {
            List<string> output = new List<string>();


            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";

            // Always add appsettings.json
            output.Add(Path.Combine(currentPath, "appsettings.json"));

            // If there is an environment specific file, then add that one too.
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

    }
}
