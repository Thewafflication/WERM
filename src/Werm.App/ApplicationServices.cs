using System;
using System.Configuration;
using System.IO;
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
            string databasePath,
            string defaultTemplatePath,
            string configurationError)
        {
            Authorizer = authorizer;
            MaintenanceService = maintenanceService;
            LabelWorkflowService = labelWorkflowService;
            DatabasePath = databasePath;
            DefaultTemplatePath = defaultTemplatePath;
            ConfigurationError = configurationError;
        }

        public MaintenanceAuthorizer Authorizer { get; private set; }
        public MaintenanceService MaintenanceService { get; private set; }
        public LabelWorkflowService LabelWorkflowService { get; private set; }
        public string DatabasePath { get; private set; }
        public string DefaultTemplatePath { get; private set; }
        public string ConfigurationError { get; private set; }

        public bool IsConfigured
        {
            get { return string.IsNullOrEmpty(ConfigurationError); }
        }

        public static ApplicationServices Create()
        {
            string databasePath = ReadSetting("DatabasePath");
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                databasePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "WERM",
                    "werm.db");
            }

            string templatePath = ReadSetting("WordTemplatePath");
            string driverName = ReadSetting("OdbcDriverName");
            string dsn = ReadSetting("OdbcDsn");
            try
            {
                OdbcConnectionOptions options;
                if (!string.IsNullOrWhiteSpace(dsn))
                {
                    options = OdbcConnectionOptions.ForDsn(databasePath, dsn);
                }
                else if (!string.IsNullOrWhiteSpace(driverName))
                {
                    options = OdbcConnectionOptions.ForDriver(databasePath, driverName);
                }
                else
                {
                    throw new ConfigurationErrorsException(
                        "Set WERM_ODBC_DSN or WERM_ODBC_DRIVER before using the database.");
                }

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
                    Path.GetFullPath(databasePath),
                    templatePath,
                    null);
            }
            catch (Exception exception)
            {
                return new ApplicationServices(
                    null,
                    null,
                    null,
                    databasePath,
                    templatePath,
                    exception.Message);
            }
        }

        private static string ReadSetting(string name)
        {
            string environmentValue = Environment.GetEnvironmentVariable("WERM_" +
                ConvertSettingName(name));
            return !string.IsNullOrWhiteSpace(environmentValue)
                ? environmentValue.Trim()
                : (ConfigurationManager.AppSettings[name] ?? string.Empty).Trim();
        }

        private static string ConvertSettingName(string settingName)
        {
            switch (settingName)
            {
                case "DatabasePath": return "DATABASE_PATH";
                case "OdbcDriverName": return "ODBC_DRIVER";
                case "OdbcDsn": return "ODBC_DSN";
                case "WordTemplatePath": return "WORD_TEMPLATE";
                default: throw new ArgumentOutOfRangeException(nameof(settingName));
            }
        }
    }
}
