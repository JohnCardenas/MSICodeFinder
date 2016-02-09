using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace MSICodeFinder
{
    /**
     * Some code from http://stackoverflow.com/questions/17936064/how-can-i-find-the-upgrade-code-for-an-installed-application-in-c
     */
    internal static class RegistryHelper
    {
        private const string UPGRADE_CODE_REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UpgradeCodes";

        private static readonly int[] GuidRegistryFormatPattern = new[] { 8, 4, 4, 2, 2, 2, 2, 2, 2, 2, 2 };

        public static Guid? GetUpgradeCode(Guid productCode)
        {
            // Convert the product code to the format found in the registry
            var productCodeSearchString = ConvertToRegistryFormat(productCode);

            // Open the upgrade code registry key
            using (RegistryKey upgradeCodeRegistryRoot = Registry.LocalMachine.OpenSubKey(UPGRADE_CODE_REGISTRY_KEY))
            {
                if (upgradeCodeRegistryRoot == null)
                    return null;

                // Iterate over each sub-key
                foreach (string subKeyName in upgradeCodeRegistryRoot.GetSubKeyNames())
                {
                    using (RegistryKey subkey = upgradeCodeRegistryRoot.OpenSubKey(subKeyName))
                    {
                        if (subkey == null)
                            continue;

                        // Check for a value containing the product code
                        if (subkey.GetValueNames().Any(s => s.IndexOf(productCodeSearchString, StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            // Extract the name of the subkey from the qualified name
                            string formattedUpgradeCode = subkey.Name.Split('\\').LastOrDefault();

                            // Convert it back to a Guid
                            return ConvertFromRegistryFormat(formattedUpgradeCode);
                        }
                    }
                }

                return null;
            }
        }

        public static List<Guid> GetProductCodes(Guid upgradeCode)
        {
            List<Guid> codes = new List<Guid>();

            // Convert the upgrade code to the format found in the registry
            var upgradeCodeKeyName = ConvertToRegistryFormat(upgradeCode);

            // Open the upgrade code registry key
            using (RegistryKey upgradeCodeRegKey = Registry.LocalMachine.OpenSubKey(UPGRADE_CODE_REGISTRY_KEY + "\\" + upgradeCodeKeyName))
            {
                if (upgradeCodeRegKey == null)
                    return codes;

                // Iterate over each product code value
                foreach (string valueName in upgradeCodeRegKey.GetValueNames())
                {
                    codes.Add(ConvertFromRegistryFormat(valueName));
                }
            }

            return codes;
        }

        private static string ConvertToRegistryFormat(Guid productCode)
        {
            return Reverse(productCode, GuidRegistryFormatPattern);
        }

        private static Guid ConvertFromRegistryFormat(string upgradeCode)
        {
            if (upgradeCode == null || upgradeCode.Length != 32)
                throw new FormatException("Product code was in an invalid format");

            upgradeCode = Reverse(upgradeCode, GuidRegistryFormatPattern);

            return Guid.Parse(upgradeCode);
        }

        private static string Reverse(object value, params int[] pattern)
        {
            // Strip the hyphens
            var inputString = value.ToString().Replace("-", "");

            var returnString = new StringBuilder();

            var index = 0;

            // Iterate over the reversal pattern
            foreach (var length in pattern)
            {
                // Reverse the sub-string and append it
                returnString.Append(inputString.Substring(index, length).Reverse().ToArray());

                // Increment our posistion in the string
                index += length;
            }

            return returnString.ToString();
        }
    }
}
