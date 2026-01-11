using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeGenerationCommon
{
    public static class Utility
    {
        public static bool IsNetFramework48()
        {
            // .NET Framework 4.8 can be checked via version or OSPlatform
            Version frameworkVersion = Environment.Version;
            return frameworkVersion.Major == 4 && frameworkVersion.Minor == 0 && frameworkVersion.Build >= 30319; // This is a rough check for .NET Framework 4.8
        }

        public static bool IsNetCore()
        {
            string description = RuntimeInformation.FrameworkDescription;

            //if (description.StartsWith(".NET Core", StringComparison.OrdinalIgnoreCase) ||
            //    description.StartsWith(".NET 9", StringComparison.OrdinalIgnoreCase) ||
            //    description.StartsWith(".NET 8", StringComparison.OrdinalIgnoreCase) ||
            //    description.StartsWith(".NET 7", StringComparison.OrdinalIgnoreCase) ||
            //    description.StartsWith(".NET 6", StringComparison.OrdinalIgnoreCase) ||
            //    description.StartsWith(".NET 5", StringComparison.OrdinalIgnoreCase) )
            //
            //
            if (IsNetFramework48() == false)    // simplified.
            { 
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
