using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using Werm.Core.Domain;
using Werm.Core.Persistence;
using Werm.Core.Security;
using Werm.Data.Connection;

namespace Werm.Data
{
    public sealed class OdbcWermDataStore : IWermDataStore
    {
        private sealed class PendingChange
        {
            public string EntityType { get; set; }
            public string EntityKey { get; set; }
            public string FieldName { get; set; }
            public string OldValue { get; set; }
            public string NewValue { get; set; }
        }

        private sealed class AuditEventRow
        {
            public long AuditEventId { get; set; }
            public string ProductPlu { get; set; }
            public long? ParentAuditEventId { get; set; }
            public int RevisionNumber { get; set; }
            public AuditChangeType ChangeType { get; set; }
            public DateTimeOffset ChangedAtUtc { get; set; }
            public string ChangedBy { get; set; }
            public string ChangeReason { get; set; }
        }

        private readonly IWermDbConnectionFactory _connectionFactory;
        private readonly IUtcClock _clock;

        public OdbcWermDataStore(IWermDbConnectionFactory connectionFactory, IUtcClock clock)
        {
            _connectionFactory = connectionFactory ??
                throw new ArgumentNullException(nameof(connectionFactory));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public Product GetProduct(string plu)
        {
            string normalizedPlu = RequireText(plu, nameof(plu));
            using (IDbConnection connection = _connectionFactory.OpenConnection())
            {
                return LoadProduct(connection, null, normalizedPlu);
            }
        }

        public Customer GetCustomer(long customerId)
        {
            if (customerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(customerId));
            }

            using (IDbConnection connection = _connectionFactory.OpenConnection())
            using (IDbCommand command = SqlCommandBuilder.Create(
                connection,
                null,
                "SELECT CustomerId, CustomerCode, CustomerName, IsActive " +
                "FROM Customer WHERE CustomerId = ?",
                SqlCommandBuilder.Integer(customerId)))
            using (IDataReader reader = command.ExecuteReader())
            {
                return reader.Read() ? ReadCustomer(reader) : null;
            }
        }

        public CustomerProductPrice GetCustomerProductPrice(
            long customerId,
            string productPlu,
            string priceType)
        {
            using (IDbConnection connection = _connectionFactory.OpenConnection())
            {
                return LoadPrice(
                    connection,
                    null,
                    customerId,
                    RequireText(productPlu, nameof(productPlu)),
                    RequireText(priceType, nameof(priceType)));
            }
        }

        public IReadOnlyList<ProductAuditEvent> GetProductAuditHistory(string plu)
        {
            string normalizedPlu = RequireText(plu, nameof(plu));
            var eventRows = new List<AuditEventRow>();

            using (IDbConnection connection = _connectionFactory.OpenConnection())
            {
                using (IDbCommand command = SqlCommandBuilder.Create(
                    connection,
                    null,
                    "SELECT AuditEventId, ProductPLU, ParentAuditEventId, RevisionNumber, " +
                    "ChangeType, ChangedAtUtc, ChangedBy, ChangeReason " +
                    "FROM ProductAuditEvent WHERE ProductPLU = ? ORDER BY RevisionNumber",
                    SqlCommandBuilder.Text(normalizedPlu)))
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        eventRows.Add(ReadAuditEventRow(reader));
                    }
                }

                var events = new List<ProductAuditEvent>();
                foreach (AuditEventRow eventRow in eventRows)
                {
                    IList<ProductAuditChange> changes = LoadAuditChanges(
                        connection, eventRow.AuditEventId);
                    events.Add(new ProductAuditEvent(
                        eventRow.AuditEventId,
                        eventRow.ProductPlu,
                        eventRow.ParentAuditEventId,
                        eventRow.RevisionNumber,
                        eventRow.ChangeType,
                        eventRow.ChangedAtUtc,
                        eventRow.ChangedBy,
                        eventRow.ChangeReason,
                        changes));
                }

                return events.AsReadOnly();
            }
        }

        public bool SaveProduct(Product product, string changedBy, string changeReason)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            string operatorName = RequireText(changedBy, nameof(changedBy));
            string now = SqlCommandBuilder.FormatUtc(_clock.UtcNow);

            using (IDbConnection connection = _connectionFactory.OpenConnection())
            using (IDbTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    Product existing = LoadProduct(connection, transaction, product.Plu);
                    var changes = BuildProductChanges(existing, product);
                    if (changes.Count == 0)
                    {
                        transaction.Commit();
                        return false;
                    }

                    if (existing == null)
                    {
                        InsertProduct(connection, transaction, product, now);
                    }
                    else
                    {
                        UpdateProduct(connection, transaction, product, now);
                    }

                    AuditChangeType changeType = SelectChangeType(existing, product.IsActive);
                    AppendAuditEvent(
                        connection,
                        transaction,
                        product.Plu,
                        changeType,
                        now,
                        operatorName,
                        changeReason,
                        changes);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    TryRollback(transaction);
                    throw;
                }
            }
        }

        public long SaveCustomer(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }
            string now = SqlCommandBuilder.FormatUtc(_clock.UtcNow);

            using (IDbConnection connection = _connectionFactory.OpenConnection())
            using (IDbTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    long customerId = customer.CustomerId;
                    if (customerId == 0)
                    {
                        using (IDbCommand insert = SqlCommandBuilder.Create(
                            connection,
                            transaction,
                            "INSERT INTO Customer " +
                            "(CustomerCode, CustomerName, IsActive, CreatedUtc, ModifiedUtc) " +
                            "VALUES (?, ?, ?, ?, ?)",
                            SqlCommandBuilder.Text(customer.CustomerCode),
                            SqlCommandBuilder.Text(customer.CustomerName),
                            SqlCommandBuilder.Boolean(customer.IsActive),
                            SqlCommandBuilder.Text(now),
                            SqlCommandBuilder.Text(now)))
                        {
                            RequireSingleRow(insert.ExecuteNonQuery(), "Customer insert");
                        }
                        customerId = GetLastInsertRowId(connection, transaction);
                    }
                    else
                    {
                        using (IDbCommand update = SqlCommandBuilder.Create(
                            connection,
                            transaction,
                            "UPDATE Customer SET CustomerCode = ?, CustomerName = ?, " +
                            "IsActive = ?, ModifiedUtc = ? WHERE CustomerId = ?",
                            SqlCommandBuilder.Text(customer.CustomerCode),
                            SqlCommandBuilder.Text(customer.CustomerName),
                            SqlCommandBuilder.Boolean(customer.IsActive),
                            SqlCommandBuilder.Text(now),
                            SqlCommandBuilder.Integer(customerId)))
                        {
                            RequireSingleRow(update.ExecuteNonQuery(), "Customer update");
                        }
                    }

                    transaction.Commit();
                    return customerId;
                }
                catch
                {
                    TryRollback(transaction);
                    throw;
                }
            }
        }

        public bool SaveCustomerProductPrice(
            CustomerProductPrice price,
            string changedBy,
            string changeReason)
        {
            if (price == null)
            {
                throw new ArgumentNullException(nameof(price));
            }
            string operatorName = RequireText(changedBy, nameof(changedBy));
            string now = SqlCommandBuilder.FormatUtc(_clock.UtcNow);

            using (IDbConnection connection = _connectionFactory.OpenConnection())
            using (IDbTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    CustomerProductPrice existing = LoadPrice(
                        connection,
                        transaction,
                        price.CustomerId,
                        price.ProductPlu,
                        price.PriceType);
                    var changes = BuildPriceChanges(existing, price);
                    if (changes.Count == 0)
                    {
                        transaction.Commit();
                        return false;
                    }

                    if (existing == null)
                    {
                        InsertPrice(connection, transaction, price, now);
                    }
                    else
                    {
                        UpdatePrice(connection, transaction, price, now);
                    }

                    AuditChangeType changeType = SelectChangeType(existing, price.IsActive);
                    AppendAuditEvent(
                        connection,
                        transaction,
                        price.ProductPlu,
                        changeType,
                        now,
                        operatorName,
                        changeReason,
                        changes);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    TryRollback(transaction);
                    throw;
                }
            }
        }

        private static Product LoadProduct(
            IDbConnection connection,
            IDbTransaction transaction,
            string plu)
        {
            using (IDbCommand command = SqlCommandBuilder.Create(
                connection,
                transaction,
                "SELECT PLU, Description, IngredientsStatement, SafeHandlingRequired, IsActive " +
                "FROM Product WHERE PLU = ?",
                SqlCommandBuilder.Text(plu)))
            using (IDataReader reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    return null;
                }

                return new Product(
                    Convert.ToString(reader.GetValue(0)),
                    Convert.ToString(reader.GetValue(1)),
                    reader.IsDBNull(2) ? null : Convert.ToString(reader.GetValue(2)),
                    Convert.ToInt64(reader.GetValue(3)) != 0,
                    Convert.ToInt64(reader.GetValue(4)) != 0);
            }
        }

        private static CustomerProductPrice LoadPrice(
            IDbConnection connection,
            IDbTransaction transaction,
            long customerId,
            string productPlu,
            string priceType)
        {
            using (IDbCommand command = SqlCommandBuilder.Create(
                connection,
                transaction,
                "SELECT CustomerId, ProductPLU, PriceType, AmountMinorUnits, " +
                "CurrencyCode, PriceBasis, IsActive FROM CustomerProductPrice " +
                "WHERE CustomerId = ? AND ProductPLU = ? AND PriceType = ?",
                SqlCommandBuilder.Integer(customerId),
                SqlCommandBuilder.Text(productPlu),
                SqlCommandBuilder.Text(priceType)))
            using (IDataReader reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    return null;
                }

                return new CustomerProductPrice(
                    Convert.ToInt64(reader.GetValue(0)),
                    Convert.ToString(reader.GetValue(1)),
                    Convert.ToString(reader.GetValue(2)),
                    Convert.ToInt64(reader.GetValue(3)),
                    Convert.ToString(reader.GetValue(4)),
                    reader.IsDBNull(5) ? null : Convert.ToString(reader.GetValue(5)),
                    Convert.ToInt64(reader.GetValue(6)) != 0);
            }
        }

        private static Customer ReadCustomer(IDataRecord reader)
        {
            return new Customer(
                Convert.ToInt64(reader.GetValue(0)),
                Convert.ToString(reader.GetValue(1)),
                Convert.ToString(reader.GetValue(2)),
                Convert.ToInt64(reader.GetValue(3)) != 0);
        }

        private static AuditEventRow ReadAuditEventRow(IDataRecord reader)
        {
            AuditChangeType changeType;
            if (!Enum.TryParse(Convert.ToString(reader.GetValue(4)), false, out changeType))
            {
                throw new DataException("Unknown audit change type: " + reader.GetValue(4));
            }

            return new AuditEventRow
            {
                AuditEventId = Convert.ToInt64(reader.GetValue(0)),
                ProductPlu = Convert.ToString(reader.GetValue(1)),
                ParentAuditEventId = reader.IsDBNull(2) ? (long?)null : Convert.ToInt64(reader.GetValue(2)),
                RevisionNumber = Convert.ToInt32(reader.GetValue(3)),
                ChangeType = changeType,
                ChangedAtUtc = SqlCommandBuilder.ParseUtc(reader.GetValue(5)),
                ChangedBy = Convert.ToString(reader.GetValue(6)),
                ChangeReason = reader.IsDBNull(7) ? null : Convert.ToString(reader.GetValue(7))
            };
        }

        private static IList<ProductAuditChange> LoadAuditChanges(
            IDbConnection connection,
            long auditEventId)
        {
            var changes = new List<ProductAuditChange>();
            using (IDbCommand command = SqlCommandBuilder.Create(
                connection,
                null,
                "SELECT ChangeSequence, EntityType, EntityKey, FieldName, OldValue, NewValue " +
                "FROM ProductAuditChange WHERE AuditEventId = ? ORDER BY ChangeSequence",
                SqlCommandBuilder.Integer(auditEventId)))
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    changes.Add(new ProductAuditChange(
                        Convert.ToInt32(reader.GetValue(0)),
                        Convert.ToString(reader.GetValue(1)),
                        Convert.ToString(reader.GetValue(2)),
                        Convert.ToString(reader.GetValue(3)),
                        reader.IsDBNull(4) ? null : Convert.ToString(reader.GetValue(4)),
                        reader.IsDBNull(5) ? null : Convert.ToString(reader.GetValue(5))));
                }
            }
            return changes;
        }

        private static void InsertProduct(
            IDbConnection connection,
            IDbTransaction transaction,
            Product product,
            string now)
        {
            using (IDbCommand command = SqlCommandBuilder.Create(
                connection,
                transaction,
                "INSERT INTO Product " +
                "(PLU, Description, IngredientsStatement, SafeHandlingRequired, IsActive, CreatedUtc, ModifiedUtc) " +
                "VALUES (?, ?, ?, ?, ?, ?, ?)",
                SqlCommandBuilder.Text(product.Plu),
                SqlCommandBuilder.Text(product.Description),
                SqlCommandBuilder.Text(product.IngredientsStatement),
                SqlCommandBuilder.Boolean(product.SafeHandlingRequired),
                SqlCommandBuilder.Boolean(product.IsActive),
                SqlCommandBuilder.Text(now),
                SqlCommandBuilder.Text(now)))
            {
                RequireSingleRow(command.ExecuteNonQuery(), "Product insert");
            }
        }

        private static void UpdateProduct(
            IDbConnection connection,
            IDbTransaction transaction,
            Product product,
            string now)
        {
            using (IDbCommand command = SqlCommandBuilder.Create(
                connection,
                transaction,
                "UPDATE Product SET Description = ?, IngredientsStatement = ?, " +
                "SafeHandlingRequired = ?, IsActive = ?, ModifiedUtc = ? WHERE PLU = ?",
                SqlCommandBuilder.Text(product.Description),
                SqlCommandBuilder.Text(product.IngredientsStatement),
                SqlCommandBuilder.Boolean(product.SafeHandlingRequired),
                SqlCommandBuilder.Boolean(product.IsActive),
                SqlCommandBuilder.Text(now),
                SqlCommandBuilder.Text(product.Plu)))
            {
                RequireSingleRow(command.ExecuteNonQuery(), "Product update");
            }
        }

        private static void InsertPrice(
            IDbConnection connection,
            IDbTransaction transaction,
            CustomerProductPrice price,
            string now)
        {
            using (IDbCommand command = SqlCommandBuilder.Create(
                connection,
                transaction,
                "INSERT INTO CustomerProductPrice " +
                "(CustomerId, ProductPLU, PriceType, AmountMinorUnits, CurrencyCode, " +
                "PriceBasis, IsActive, CreatedUtc, ModifiedUtc) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
                SqlCommandBuilder.Integer(price.CustomerId),
                SqlCommandBuilder.Text(price.ProductPlu),
                SqlCommandBuilder.Text(price.PriceType),
                SqlCommandBuilder.Integer(price.AmountMinorUnits),
                SqlCommandBuilder.Text(price.CurrencyCode),
                SqlCommandBuilder.Text(price.PriceBasis),
                SqlCommandBuilder.Boolean(price.IsActive),
                SqlCommandBuilder.Text(now),
                SqlCommandBuilder.Text(now)))
            {
                RequireSingleRow(command.ExecuteNonQuery(), "Customer-product price insert");
            }
        }

        private static void UpdatePrice(
            IDbConnection connection,
            IDbTransaction transaction,
            CustomerProductPrice price,
            string now)
        {
            using (IDbCommand command = SqlCommandBuilder.Create(
                connection,
                transaction,
                "UPDATE CustomerProductPrice SET AmountMinorUnits = ?, CurrencyCode = ?, " +
                "PriceBasis = ?, IsActive = ?, ModifiedUtc = ? " +
                "WHERE CustomerId = ? AND ProductPLU = ? AND PriceType = ?",
                SqlCommandBuilder.Integer(price.AmountMinorUnits),
                SqlCommandBuilder.Text(price.CurrencyCode),
                SqlCommandBuilder.Text(price.PriceBasis),
                SqlCommandBuilder.Boolean(price.IsActive),
                SqlCommandBuilder.Text(now),
                SqlCommandBuilder.Integer(price.CustomerId),
                SqlCommandBuilder.Text(price.ProductPlu),
                SqlCommandBuilder.Text(price.PriceType)))
            {
                RequireSingleRow(command.ExecuteNonQuery(), "Customer-product price update");
            }
        }

        private static List<PendingChange> BuildProductChanges(Product existing, Product desired)
        {
            var changes = new List<PendingChange>();
            AddChange(changes, "Product", desired.Plu, "PLU", existing == null ? null : existing.Plu, desired.Plu);
            AddChange(changes, "Product", desired.Plu, "Description", existing == null ? null : existing.Description, desired.Description);
            AddChange(changes, "Product", desired.Plu, "IngredientsStatement", existing == null ? null : existing.IngredientsStatement, desired.IngredientsStatement);
            AddChange(changes, "Product", desired.Plu, "SafeHandlingRequired", existing == null ? null : BooleanText(existing.SafeHandlingRequired), BooleanText(desired.SafeHandlingRequired));
            AddChange(changes, "Product", desired.Plu, "IsActive", existing == null ? null : BooleanText(existing.IsActive), BooleanText(desired.IsActive));
            return changes;
        }

        private static List<PendingChange> BuildPriceChanges(
            CustomerProductPrice existing,
            CustomerProductPrice desired)
        {
            string key = BuildPriceEntityKey(desired);
            var changes = new List<PendingChange>();
            AddChange(changes, "CustomerProductPrice", key, "CustomerId", existing == null ? null : existing.CustomerId.ToString(CultureInfo.InvariantCulture), desired.CustomerId.ToString(CultureInfo.InvariantCulture));
            AddChange(changes, "CustomerProductPrice", key, "ProductPLU", existing == null ? null : existing.ProductPlu, desired.ProductPlu);
            AddChange(changes, "CustomerProductPrice", key, "PriceType", existing == null ? null : existing.PriceType, desired.PriceType);
            AddChange(changes, "CustomerProductPrice", key, "AmountMinorUnits", existing == null ? null : existing.AmountMinorUnits.ToString(CultureInfo.InvariantCulture), desired.AmountMinorUnits.ToString(CultureInfo.InvariantCulture));
            AddChange(changes, "CustomerProductPrice", key, "CurrencyCode", existing == null ? null : existing.CurrencyCode, desired.CurrencyCode);
            AddChange(changes, "CustomerProductPrice", key, "PriceBasis", existing == null ? null : existing.PriceBasis, desired.PriceBasis);
            AddChange(changes, "CustomerProductPrice", key, "IsActive", existing == null ? null : BooleanText(existing.IsActive), BooleanText(desired.IsActive));
            return changes;
        }

        private static void AddChange(
            ICollection<PendingChange> changes,
            string entityType,
            string entityKey,
            string fieldName,
            string oldValue,
            string newValue)
        {
            if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
            {
                return;
            }
            changes.Add(new PendingChange
            {
                EntityType = entityType,
                EntityKey = entityKey,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue
            });
        }

        private static AuditChangeType SelectChangeType(Product existing, bool desiredIsActive)
        {
            return existing == null
                ? AuditChangeType.Create
                : SelectChangeType(existing.IsActive, desiredIsActive);
        }

        private static AuditChangeType SelectChangeType(
            CustomerProductPrice existing,
            bool desiredIsActive)
        {
            return existing == null
                ? AuditChangeType.Update
                : SelectChangeType(existing.IsActive, desiredIsActive);
        }

        private static AuditChangeType SelectChangeType(bool existingIsActive, bool desiredIsActive)
        {
            if (existingIsActive && !desiredIsActive)
            {
                return AuditChangeType.Deactivate;
            }
            if (!existingIsActive && desiredIsActive)
            {
                return AuditChangeType.Restore;
            }
            return AuditChangeType.Update;
        }

        private static void AppendAuditEvent(
            IDbConnection connection,
            IDbTransaction transaction,
            string productPlu,
            AuditChangeType changeType,
            string changedAtUtc,
            string changedBy,
            string changeReason,
            IList<PendingChange> changes)
        {
            long? parentEventId = null;
            int revisionNumber = 1;
            using (IDbCommand previous = SqlCommandBuilder.Create(
                connection,
                transaction,
                "SELECT AuditEventId, RevisionNumber FROM ProductAuditEvent " +
                "WHERE ProductPLU = ? ORDER BY RevisionNumber DESC LIMIT 1",
                SqlCommandBuilder.Text(productPlu)))
            using (IDataReader reader = previous.ExecuteReader())
            {
                if (reader.Read())
                {
                    parentEventId = Convert.ToInt64(reader.GetValue(0));
                    revisionNumber = checked(Convert.ToInt32(reader.GetValue(1)) + 1);
                }
            }

            using (IDbCommand insertEvent = SqlCommandBuilder.Create(
                connection,
                transaction,
                "INSERT INTO ProductAuditEvent " +
                "(ProductPLU, ParentAuditEventId, RevisionNumber, ChangeType, " +
                "ChangedAtUtc, ChangedBy, ChangeReason) VALUES (?, ?, ?, ?, ?, ?, ?)",
                SqlCommandBuilder.Text(productPlu),
                SqlCommandBuilder.NullableInteger(parentEventId),
                SqlCommandBuilder.Integer(revisionNumber),
                SqlCommandBuilder.Text(changeType.ToString()),
                SqlCommandBuilder.Text(changedAtUtc),
                SqlCommandBuilder.Text(changedBy),
                SqlCommandBuilder.Text(changeReason)))
            {
                RequireSingleRow(insertEvent.ExecuteNonQuery(), "Audit-event insert");
            }

            long auditEventId = GetLastInsertRowId(connection, transaction);
            for (int index = 0; index < changes.Count; index++)
            {
                PendingChange change = changes[index];
                using (IDbCommand insertChange = SqlCommandBuilder.Create(
                    connection,
                    transaction,
                    "INSERT INTO ProductAuditChange " +
                    "(AuditEventId, ChangeSequence, EntityType, EntityKey, FieldName, OldValue, NewValue) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?)",
                    SqlCommandBuilder.Integer(auditEventId),
                    SqlCommandBuilder.Integer(index + 1),
                    SqlCommandBuilder.Text(change.EntityType),
                    SqlCommandBuilder.Text(change.EntityKey),
                    SqlCommandBuilder.Text(change.FieldName),
                    SqlCommandBuilder.Text(change.OldValue),
                    SqlCommandBuilder.Text(change.NewValue)))
                {
                    RequireSingleRow(insertChange.ExecuteNonQuery(), "Audit-change insert");
                }
            }
        }

        private static long GetLastInsertRowId(
            IDbConnection connection,
            IDbTransaction transaction)
        {
            using (IDbCommand command = SqlCommandBuilder.Create(
                connection, transaction, "SELECT last_insert_rowid()"))
            {
                return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private static string BuildPriceEntityKey(CustomerProductPrice price)
        {
            return price.CustomerId.ToString(CultureInfo.InvariantCulture) + ":" +
                Convert.ToBase64String(Encoding.UTF8.GetBytes(price.ProductPlu)) + ":" +
                Convert.ToBase64String(Encoding.UTF8.GetBytes(price.PriceType));
        }

        private static string BooleanText(bool value)
        {
            return value ? "1" : "0";
        }

        private static string RequireText(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            string normalized = value.Trim();
            if (normalized.Length == 0)
            {
                throw new ArgumentException("A value is required.", parameterName);
            }
            return normalized;
        }

        private static void RequireSingleRow(int affectedRows, string operation)
        {
            if (affectedRows != 1)
            {
                throw new DataException(
                    operation + " affected " + affectedRows.ToString(CultureInfo.InvariantCulture) +
                    " rows instead of one.");
            }
        }

        private static void TryRollback(IDbTransaction transaction)
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
                // Preserve the original write or audit failure.
            }
        }
    }
}
