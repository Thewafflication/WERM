using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Werm.Core.Configuration;
using Werm.Core.Database;
using Werm.Core.Persistence;
using Werm.Core.Printing;
using Werm.Core.Security;
using Werm.Data;
using Werm.Data.Connection;
using Werm.Printing;

namespace Werm.App
{
    public sealed class ApplicationServices
    {
        private ApplicationServices(
            MaintenanceAuthorizer authorizer,
            MaintenanceService maintenanceService,
            LabelWorkflowService labelWorkflowService,
            WermSettings settings,
            string settingsFilePath,
            IList<string> environmentOverrides,
            string configurationError)
        {
            Authorizer = authorizer;
            MaintenanceService = maintenanceService;
            LabelWorkflowService = labelWorkflowService;
            Settings = settings;
            SettingsFilePath = settingsFilePath;
            EnvironmentOverrides = environmentOverrides;
            ConfigurationError = configurationError;
        }

        public MaintenanceAuthorizer Authorizer { get; private set; }
        public MaintenanceService MaintenanceService { get; private set; }
        public LabelWorkflowService LabelWorkflowService { get; private set; }
        public WermSettings Settings { get; private set; }
        public string SettingsFilePath { get; private set; }
        public IList<string> EnvironmentOverrides { get; private set; }
        public string ConfigurationError { get; private set; }

        public string DatabasePath
        {
            get { return Settings.DatabasePath; }
        }

        public string DefaultTemplatePath
        {
            get { return Settings.WordTemplatePath; }
        }

        public bool IsConfigured
        {
            get { return string.IsNullOrEmpty(ConfigurationError); }
        }

        public static ApplicationServices Create()
        {
            string settingsPath = GetSettingsFilePath();
            try
            {
                IList<string> overrides;
                WermSettings settings = LoadEffectiveSettings(settingsPath, out overrides);
                return Create(settings, settingsPath, overrides);
            }
            catch (Exception exception)
            {
                return new ApplicationServices(
                    null,
                    null,
                    null,
                    CreateDefaultSettings(),
                    settingsPath,
                    new List<string>(),
                    "The per-user settings could not be loaded: " + exception.Message);
            }
        }

        public static ApplicationServices Create(WermSettings settings)
        {
            return Create(settings, GetSettingsFilePath(), new List<string>());
        }

        public static void SaveUserSettings(WermSettings settings)
        {
            ValidateSettings(settings);
            new WermSettingsStore(GetSettingsFilePath()).Save(settings);
        }

        public void VerifyDatabase()
        {
            DemandConfigured();
            CreateInstaller(Settings).VerifyExisting(Settings.DatabasePath);
        }

        public bool InstallOrValidateDatabase()
        {
            DemandConfigured();
            return CreateInstaller(Settings).InstallOrValidate(
                Settings.DatabasePath,
                GetMigrationPath());
        }

        private static ApplicationServices Create(
            WermSettings settings,
            string settingsFilePath,
            IList<string> environmentOverrides)
        {
            try
            {
                ValidateSettings(settings);
                OdbcConnectionOptions options = CreateOptions(settings);
                var clock = new SystemUtcClock();
                var connectionFactory = new OdbcConnectionFactory(options);
                var dataStore = new OdbcWermDataStore(connectionFactory, clock);
                var credentialStore = new OdbcMaintenanceCredentialStore(connectionFactory, clock);
                var authorizer = new MaintenanceAuthorizer(
                    credentialStore,
                    new Pbkdf2PasswordHasher(),
                    clock);
                var maintenance = new MaintenanceService(dataStore, authorizer);
                var printService = new WordLabelPrintService(
                    new WordComLabelDocumentFactory());
                var labelWorkflow = new LabelWorkflowService(dataStore, printService);

                return new ApplicationServices(
                    authorizer,
                    maintenance,
                    labelWorkflow,
                    settings.Clone(),
                    settingsFilePath,
                    environmentOverrides,
                    null);
            }
            catch (Exception exception)
            {
                return new ApplicationServices(
                    null,
                    null,
                    null,
                    settings == null ? new WermSettings() : settings.Clone(),
                    settingsFilePath,
                    environmentOverrides,
                    exception.Message);
            }
        }

        private static WermDatabaseInstaller CreateInstaller(WermSettings settings)
        {
            return new WermDatabaseInstaller(
                new OdbcConnectionFactory(CreateOptions(settings)));
        }

        private static OdbcConnectionOptions CreateOptions(WermSettings settings)
        {
            return !string.IsNullOrWhiteSpace(settings.OdbcDsn)
                ? OdbcConnectionOptions.ForDsn(settings.DatabasePath, settings.OdbcDsn)
                : OdbcConnectionOptions.ForDriver(
                    settings.DatabasePath,
                    settings.OdbcDriverName);
        }

        private static void ValidateSettings(WermSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            if (string.IsNullOrWhiteSpace(settings.DatabasePath))
            {
                throw new ConfigurationErrorsException("Select a SQLite database file.");
            }
            settings.DatabasePath = Path.GetFullPath(settings.DatabasePath.Trim());
            settings.OdbcDriverName = (settings.OdbcDriverName ?? string.Empty).Trim();
            settings.OdbcDsn = (settings.OdbcDsn ?? string.Empty).Trim();
            settings.WordTemplatePath = (settings.WordTemplatePath ?? string.Empty).Trim();
            if (settings.OdbcDsn.Length == 0 && settings.OdbcDriverName.Length == 0)
            {
                throw new ConfigurationErrorsException(
                    "Select a registered ODBC driver or data-source name (DSN).");
            }
        }

        private static WermSettings LoadEffectiveSettings(
            string settingsPath,
            out IList<string> overrides)
        {
            WermSettings defaults = CreateDefaultSettings();
            WermSettings settings = new WermSettingsStore(settingsPath).Load(defaults);
            var names = new List<string>();
            Override(settings, "DatabasePath", "WERM_DATABASE_PATH", names);
            Override(settings, "OdbcDriverName", "WERM_ODBC_DRIVER", names);
            Override(settings, "OdbcDsn", "WERM_ODBC_DSN", names);
            Override(settings, "WordTemplatePath", "WERM_WORD_TEMPLATE", names);
            overrides = names;
            return settings;
        }

        private static WermSettings CreateDefaultSettings()
        {
            var defaults = new WermSettings
            {
                DatabasePath = ReadAppSetting("DatabasePath"),
                OdbcDriverName = ReadAppSetting("OdbcDriverName"),
                OdbcDsn = ReadAppSetting("OdbcDsn"),
                WordTemplatePath = ReadAppSetting("WordTemplatePath")
            };
            if (string.IsNullOrWhiteSpace(defaults.DatabasePath))
            {
                defaults.DatabasePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "WERM",
                    "werm.db");
            }
            return defaults;
        }

        private static void Override(
            WermSettings settings,
            string property,
            string environmentName,
            IList<string> names)
        {
            string value = Environment.GetEnvironmentVariable(environmentName);
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            typeof(WermSettings).GetProperty(property).SetValue(settings, value.Trim(), null);
            names.Add(environmentName);
        }

        private static string ReadAppSetting(string name)
        {
            return (ConfigurationManager.AppSettings[name] ?? string.Empty).Trim();
        }

        private static string GetSettingsFilePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WERM",
                "settings.xml");
        }

        private static string GetMigrationPath()
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "database",
                "migrations",
                InitialSchemaContract.MigrationName);
        }

        private void DemandConfigured()
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException(ConfigurationError);
            }
        }
    }
}
