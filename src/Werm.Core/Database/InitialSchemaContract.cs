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
                "Product",
                "Customer",
                "CustomerProductPrice",
                "ProductAuditEvent",
                "ProductAuditChange"
            });
    }
}
