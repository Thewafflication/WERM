using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Werm.Core.Database
{
    public sealed class SqlMigration
    {
        internal SqlMigration(string sourceName, string sql, IList<string> batches)
        {
            SourceName = sourceName;
            Sql = sql;
            Batches = new ReadOnlyCollection<string>(batches);
        }

        public string SourceName { get; private set; }
        public string Sql { get; private set; }
        public ReadOnlyCollection<string> Batches { get; private set; }
    }
}
