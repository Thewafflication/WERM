using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Werm.App
{
    public sealed class OdbcRegistryCatalog
    {
        public IList<string> GetInstalledDrivers()
        {
            var values = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
            ReadValueNames(
                RegistryHive.LocalMachine,
                @"SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers",
                values);
            return new List<string>(values);
        }

        public IList<string> GetDataSourceNames()
        {
            var values = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
            ReadValueNames(
                RegistryHive.CurrentUser,
                @"SOFTWARE\ODBC\ODBC.INI\ODBC Data Sources",
                values);
            ReadValueNames(
                RegistryHive.LocalMachine,
                @"SOFTWARE\ODBC\ODBC.INI\ODBC Data Sources",
                values);
            return new List<string>(values);
        }

        private static void ReadValueNames(
            RegistryHive hive,
            string keyPath,
            ISet<string> destination)
        {
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
            using (RegistryKey key = baseKey.OpenSubKey(keyPath, false))
            {
                if (key == null)
                {
                    return;
                }
                foreach (string name in key.GetValueNames())
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        destination.Add(name.Trim());
                    }
                }
            }
        }
    }
}
