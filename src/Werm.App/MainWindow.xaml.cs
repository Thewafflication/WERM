using System;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Werm.Core.Configuration;
using Werm.Core.Domain;
using Werm.Core.Security;

namespace Werm.App
{
    public partial class MainWindow : Window
    {
        private ApplicationServices _services;
        private MaintenanceSession _maintenanceSession;

        public MainWindow(ApplicationServices services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            InitializeComponent();
            TemplatePathTextBox.Text = services.DefaultTemplatePath ?? string.Empty;
            LoadConfigurationForm();
            SetStatus(services.IsConfigured
                ? "Ready. Database: " + services.DatabasePath
                : "Configuration required: " + services.ConfigurationError);
            LoadInstalledPrinters();
            RefreshOdbcCatalog();
        }

        private void LoadConfigurationForm()
        {
            WermSettings settings = _services.Settings;
            DatabasePathTextBox.Text = settings.DatabasePath ?? string.Empty;
            OdbcDriverComboBox.Text = settings.OdbcDriverName ?? string.Empty;
            OdbcDsnComboBox.Text = settings.OdbcDsn ?? string.Empty;
            ConfigurationTemplatePathTextBox.Text = settings.WordTemplatePath ?? string.Empty;
            DsnModeRadioButton.IsChecked = !string.IsNullOrWhiteSpace(settings.OdbcDsn);
            DriverModeRadioButton.IsChecked = DsnModeRadioButton.IsChecked != true;
            SettingsLocationTextBlock.Text = "Per-user settings file: " +
                _services.SettingsFilePath;
            EnvironmentOverridesTextBlock.Text = _services.EnvironmentOverrides.Count == 0
                ? "No WERM environment-variable overrides are active."
                : "Environment overrides are active and take precedence: " +
                    string.Join(", ", _services.EnvironmentOverrides);
            UpdateConnectionMode();
        }

        private WermSettings ReadConfigurationForm()
        {
            return new WermSettings
            {
                DatabasePath = DatabasePathTextBox.Text,
                OdbcDriverName = DriverModeRadioButton.IsChecked == true
                    ? OdbcDriverComboBox.Text
                    : string.Empty,
                OdbcDsn = DsnModeRadioButton.IsChecked == true
                    ? OdbcDsnComboBox.Text
                    : string.Empty,
                WordTemplatePath = ConfigurationTemplatePathTextBox.Text
            };
        }

        private ApplicationServices CreateServicesFromForm()
        {
            ApplicationServices services = ApplicationServices.Create(ReadConfigurationForm());
            if (!services.IsConfigured)
            {
                throw new InvalidOperationException(services.ConfigurationError);
            }
            return services;
        }

        private void BrowseDatabase_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                DefaultExt = ".db",
                Filter = "SQLite databases|*.db;*.sqlite;*.sqlite3|All files|*.*",
                FileName = Path.GetFileName(DatabasePathTextBox.Text)
            };
            string currentDirectory = Path.GetDirectoryName(DatabasePathTextBox.Text);
            if (!string.IsNullOrWhiteSpace(currentDirectory) && Directory.Exists(currentDirectory))
            {
                dialog.InitialDirectory = currentDirectory;
            }
            if (dialog.ShowDialog(this) == true)
            {
                DatabasePathTextBox.Text = dialog.FileName;
            }
        }

        private void BrowseConfigurationTemplate_Click(object sender, RoutedEventArgs e)
        {
            string selected = SelectWordTemplate(ConfigurationTemplatePathTextBox.Text);
            if (selected != null)
            {
                ConfigurationTemplatePathTextBox.Text = selected;
            }
        }

        private void ConnectionMode_Checked(object sender, RoutedEventArgs e)
        {
            UpdateConnectionMode();
        }

        private void UpdateConnectionMode()
        {
            if (OdbcDriverComboBox == null || OdbcDsnComboBox == null)
            {
                return;
            }
            OdbcDriverComboBox.IsEnabled = DriverModeRadioButton.IsChecked == true;
            OdbcDsnComboBox.IsEnabled = DsnModeRadioButton.IsChecked == true;
        }

        private void RefreshOdbc_Click(object sender, RoutedEventArgs e)
        {
            RefreshOdbcCatalog();
        }

        private void RefreshOdbcCatalog()
        {
            string selectedDriver = OdbcDriverComboBox.Text;
            string selectedDsn = OdbcDsnComboBox.Text;
            try
            {
                var catalog = new OdbcRegistryCatalog();
                OdbcDriverComboBox.Items.Clear();
                foreach (string driver in catalog.GetInstalledDrivers())
                {
                    OdbcDriverComboBox.Items.Add(driver);
                }
                OdbcDsnComboBox.Items.Clear();
                foreach (string dsn in catalog.GetDataSourceNames())
                {
                    OdbcDsnComboBox.Items.Add(dsn);
                }
                OdbcDriverComboBox.Text = selectedDriver;
                OdbcDsnComboBox.Text = selectedDsn;
            }
            catch (Exception exception)
            {
                SetStatus("ODBC registrations could not be enumerated: " + exception.Message);
            }
        }

        private void TestDatabase_Click(object sender, RoutedEventArgs e)
        {
            Run("Connection succeeded and the WERM schema is valid.", () =>
            {
                CreateServicesFromForm().VerifyDatabase();
            });
        }

        private void CreateDatabase_Click(object sender, RoutedEventArgs e)
        {
            Run("Database creation/validation succeeded. Save these settings to use it.", () =>
            {
                CreateServicesFromForm().InstallOrValidateDatabase();
            });
        }

        private void SaveDatabaseSettings_Click(object sender, RoutedEventArgs e)
        {
            Run("Database settings saved and applied.", () =>
            {
                WermSettings settings = ReadConfigurationForm();
                ApplicationServices.SaveUserSettings(settings);
                if (_services.Authorizer != null)
                {
                    _services.Authorizer.EndSession(_maintenanceSession);
                }
                _maintenanceSession = null;
                SetMaintenanceWritesEnabled(false);
                _services = ApplicationServices.Create();
                LoadConfigurationForm();
                TemplatePathTextBox.Text = _services.DefaultTemplatePath ?? string.Empty;
                DemandConfigured();
            });
        }

        private void LoadInstalledPrinters()
        {
            try
            {
                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    PrinterComboBox.Items.Add(printerName);
                }
                var settings = new PrinterSettings();
                if (settings.IsDefaultPrinter)
                {
                    PrinterComboBox.Text = settings.PrinterName;
                }
            }
            catch (Exception exception)
            {
                SetStatus("Windows printers could not be enumerated: " + exception.Message);
            }
        }

        private void BrowseTemplate_Click(object sender, RoutedEventArgs e)
        {
            string selected = SelectWordTemplate(TemplatePathTextBox.Text);
            if (selected != null)
            {
                TemplatePathTextBox.Text = selected;
            }
        }

        private string SelectWordTemplate(string currentPath)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "Word templates and documents|*.dotx;*.dotm;*.docx;*.docm|All files|*.*",
                FileName = Path.GetFileName(currentPath)
            };
            string directory = Path.GetDirectoryName(currentPath);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            {
                dialog.InitialDirectory = directory;
            }
            return dialog.ShowDialog(this) == true ? dialog.FileName : null;
        }

        private void PrintLabel_Click(object sender, RoutedEventArgs e)
        {
            Run("Label submitted to Word and the selected printer.", () =>
            {
                DemandConfigured();
                _services.LabelWorkflowService.PrintLabel(
                    PrintPluTextBox.Text,
                    ParsePositiveLong(PrintCustomerIdTextBox.Text, "Customer ID"),
                    PrintPriceTypeTextBox.Text,
                    TemplatePathTextBox.Text,
                    PrinterComboBox.Text,
                    ParsePositiveInt(CopiesTextBox.Text, "Copies"));
            });
        }

        private void InitializePassword_Click(object sender, RoutedEventArgs e)
        {
            string password = MaintenancePasswordBox.Password;
            string confirmation = ConfirmPasswordBox.Password;
            MaintenancePasswordBox.Clear();
            ConfirmPasswordBox.Clear();
            Run("Maintenance password initialized. Unlock maintenance to make changes.", () =>
            {
                DemandConfigured();
                if (!string.Equals(password, confirmation, StringComparison.Ordinal))
                {
                    throw new ArgumentException("The password confirmation does not match.");
                }
                if (_services.Authorizer.IsCredentialConfigured)
                {
                    throw new InvalidOperationException("The maintenance password is already initialized.");
                }
                _services.Authorizer.InitializePassword(password);
            });
        }

        private void UnlockMaintenance_Click(object sender, RoutedEventArgs e)
        {
            string operatorName = OperatorTextBox.Text.Trim();
            string password = MaintenancePasswordBox.Password;
            MaintenancePasswordBox.Clear();
            Run("Maintenance unlocked for " + operatorName + ".", () =>
            {
                DemandConfigured();
                MaintenanceSession session;
                if (!_services.Authorizer.TryAuthenticate(
                    password,
                    operatorName,
                    out session))
                {
                    throw new MaintenanceAuthorizationException(
                        "Authentication failed or maintenance is temporarily locked.");
                }
                _maintenanceSession = session;
                SetMaintenanceWritesEnabled(true);
            });
        }

        private void LockMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if (_services.Authorizer != null)
            {
                _services.Authorizer.EndSession(_maintenanceSession);
            }
            _maintenanceSession = null;
            SetMaintenanceWritesEnabled(false);
            MaintenancePasswordBox.Clear();
            ConfirmPasswordBox.Clear();
            SetStatus("Maintenance locked.");
        }

        private void LoadProduct_Click(object sender, RoutedEventArgs e)
        {
            Run("Product loaded.", () =>
            {
                DemandConfigured();
                Product product = _services.MaintenanceService.GetProduct(ProductPluTextBox.Text);
                if (product == null)
                {
                    throw new InvalidOperationException("Product was not found.");
                }
                ProductDescriptionTextBox.Text = product.Description;
                IngredientsTextBox.Text = product.IngredientsStatement;
                SafeHandlingCheckBox.IsChecked = product.SafeHandlingRequired;
                ProductActiveCheckBox.IsChecked = product.IsActive;
            });
        }

        private void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            Run("Product save completed; changed values are recorded in append-only audit history.", () =>
            {
                DemandConfigured();
                var product = new Product(
                    ProductPluTextBox.Text,
                    ProductDescriptionTextBox.Text,
                    IngredientsTextBox.Text,
                    SafeHandlingCheckBox.IsChecked == true,
                    ProductActiveCheckBox.IsChecked == true);
                _services.MaintenanceService.SaveProduct(
                    _maintenanceSession, product, ProductChangeReasonTextBox.Text);
            });
        }

        private void LoadAuditHistory_Click(object sender, RoutedEventArgs e)
        {
            Run("Product audit history loaded.", () =>
            {
                DemandConfigured();
                AuditHistoryListBox.Items.Clear();
                foreach (var auditEvent in _services.MaintenanceService
                    .GetProductAuditHistory(ProductPluTextBox.Text))
                {
                    string parent = auditEvent.ParentAuditEventId.HasValue
                        ? auditEvent.ParentAuditEventId.Value.ToString(CultureInfo.InvariantCulture)
                        : "root";
                    AuditHistoryListBox.Items.Add(string.Format(
                        CultureInfo.InvariantCulture,
                        "Revision {0} | Event {1} | Parent {2} | {3:u} | {4} | {5} | {6}",
                        auditEvent.RevisionNumber,
                        auditEvent.AuditEventId,
                        parent,
                        auditEvent.ChangedAtUtc,
                        auditEvent.ChangeType,
                        auditEvent.ChangedBy,
                        auditEvent.ChangeReason));
                    foreach (var change in auditEvent.Changes)
                    {
                        AuditHistoryListBox.Items.Add(string.Format(
                            CultureInfo.InvariantCulture,
                            "  {0}:{1}  {2}  [{3}] -> [{4}]",
                            change.EntityType,
                            change.EntityKey,
                            change.FieldName,
                            change.OldValue ?? "<null>",
                            change.NewValue ?? "<null>"));
                    }
                }
            });
        }

        private void LoadCustomer_Click(object sender, RoutedEventArgs e)
        {
            Run("Customer loaded.", () =>
            {
                DemandConfigured();
                Customer customer = _services.MaintenanceService.GetCustomer(
                    ParsePositiveLong(CustomerIdTextBox.Text, "Customer ID"));
                if (customer == null)
                {
                    throw new InvalidOperationException("Customer was not found.");
                }
                CustomerCodeTextBox.Text = customer.CustomerCode;
                CustomerNameTextBox.Text = customer.CustomerName;
                CustomerActiveCheckBox.IsChecked = customer.IsActive;
            });
        }

        private void SaveCustomer_Click(object sender, RoutedEventArgs e)
        {
            Run("Customer saved.", () =>
            {
                DemandConfigured();
                long customerId = ParseNonNegativeLong(CustomerIdTextBox.Text, "Customer ID");
                var customer = new Customer(
                    customerId,
                    CustomerCodeTextBox.Text,
                    CustomerNameTextBox.Text,
                    CustomerActiveCheckBox.IsChecked == true);
                long savedId = _services.MaintenanceService.SaveCustomer(
                    _maintenanceSession, customer);
                CustomerIdTextBox.Text = savedId.ToString(CultureInfo.InvariantCulture);
            });
        }

        private void LoadPrice_Click(object sender, RoutedEventArgs e)
        {
            Run("Customer product price loaded.", () =>
            {
                DemandConfigured();
                CustomerProductPrice price = _services.MaintenanceService.GetCustomerProductPrice(
                    ParsePositiveLong(PriceCustomerIdTextBox.Text, "Customer ID"),
                    PriceProductPluTextBox.Text,
                    PriceTypeTextBox.Text);
                if (price == null)
                {
                    throw new InvalidOperationException("Customer product price was not found.");
                }
                PriceAmountTextBox.Text = (price.AmountMinorUnits / 100m).ToString(
                    "0.00", CultureInfo.CurrentCulture);
                CurrencyCodeTextBox.Text = price.CurrencyCode;
                PriceBasisTextBox.Text = price.PriceBasis;
                PriceActiveCheckBox.IsChecked = price.IsActive;
            });
        }

        private void SavePrice_Click(object sender, RoutedEventArgs e)
        {
            Run("Price save completed; changed values are recorded in append-only audit history.", () =>
            {
                DemandConfigured();
                decimal amount;
                if (!decimal.TryParse(
                    PriceAmountTextBox.Text,
                    NumberStyles.Currency,
                    CultureInfo.CurrentCulture,
                    out amount) || amount < 0)
                {
                    throw new ArgumentException("Amount must be a non-negative monetary value.");
                }
                long amountMinorUnits = decimal.ToInt64(decimal.Round(
                    amount * 100m, 0, MidpointRounding.AwayFromZero));
                var price = new CustomerProductPrice(
                    ParsePositiveLong(PriceCustomerIdTextBox.Text, "Customer ID"),
                    PriceProductPluTextBox.Text,
                    PriceTypeTextBox.Text,
                    amountMinorUnits,
                    CurrencyCodeTextBox.Text,
                    PriceBasisTextBox.Text,
                    PriceActiveCheckBox.IsChecked == true);
                _services.MaintenanceService.SaveCustomerProductPrice(
                    _maintenanceSession, price, PriceChangeReasonTextBox.Text);
            });
        }

        private void DemandConfigured()
        {
            if (!_services.IsConfigured)
            {
                throw new InvalidOperationException(_services.ConfigurationError);
            }
        }

        private void Run(string successMessage, Action action)
        {
            try
            {
                action();
                SetStatus(successMessage);
            }
            catch (MaintenanceAuthorizationException exception)
            {
                _maintenanceSession = null;
                SetMaintenanceWritesEnabled(false);
                SetStatus("Maintenance locked: " + exception.Message);
            }
            catch (Exception exception)
            {
                SetStatus("Error: " + exception.Message);
            }
        }

        private void SetStatus(string message)
        {
            if (StatusTextBlock != null)
            {
                StatusTextBlock.Text = message;
            }
        }

        private void SetMaintenanceWritesEnabled(bool enabled)
        {
            SaveProductButton.IsEnabled = enabled;
            SaveCustomerButton.IsEnabled = enabled;
            SavePriceButton.IsEnabled = enabled;
        }

        private static long ParsePositiveLong(string value, string name)
        {
            long parsed;
            if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ||
                parsed <= 0)
            {
                throw new ArgumentException(name + " must be a positive whole number.");
            }
            return parsed;
        }

        private static long ParseNonNegativeLong(string value, string name)
        {
            long parsed;
            if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ||
                parsed < 0)
            {
                throw new ArgumentException(name + " must be zero or a positive whole number.");
            }
            return parsed;
        }

        private static int ParsePositiveInt(string value, string name)
        {
            int parsed;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ||
                parsed <= 0)
            {
                throw new ArgumentException(name + " must be a positive whole number.");
            }
            return parsed;
        }
    }
}
