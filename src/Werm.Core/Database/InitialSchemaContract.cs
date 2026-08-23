using System.Collections.ObjectModel;

namespace Werm.Core.Database
{
    public static class InitialSchemaContract
    {
        public const int Version = 1;
        public const string MigrationName = "0001-initial-schema.sql";

        public static readonly ReadOnlyCollection<string> ExpectedTableNames =
            new ReadOnlyCollection<string>(new[]
            {
                "WermSchemaVersion",
                "MaintenanceCredential",
                "Product",
                "Customer",
                "CustomerProductPrice",
                "ProductAuditEvent",
                "ProductAuditChange"
            });

        public static readonly ReadOnlyCollection<string> ExpectedTriggerNames =
            new ReadOnlyCollection<string>(new[]
            {
                "TR_ProductAuditEvent_RejectUpdate",
                "TR_ProductAuditEvent_RejectDelete",
                "TR_ProductAuditChange_RejectUpdate",
                "TR_ProductAuditChange_RejectDelete"
            });
    }
}
