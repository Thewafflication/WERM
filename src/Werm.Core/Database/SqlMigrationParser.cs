using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Werm.Core.Database
{
    public static class SqlMigrationParser
    {
        private const string BatchDelimiter = @"(?m)^\s*-- WERM-BATCH\s*$";

        public static SqlMigration Read(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            string fullPath = Path.GetFullPath(path);
            return Parse(Path.GetFileName(fullPath), File.ReadAllText(fullPath));
        }

        public static SqlMigration Parse(string sourceName, string sql)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                throw new ArgumentException("A migration source name is required.", nameof(sourceName));
            }
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("Migration SQL is required.", nameof(sql));
            }

            var batches = new List<string>();
            foreach (string candidate in Regex.Split(sql, BatchDelimiter))
            {
                string batch = candidate.Trim();
                if (batch.Length != 0)
                {
                    batches.Add(batch);
                }
            }

            if (batches.Count == 0)
            {
                throw new InvalidDataException("The migration contains no executable batches.");
            }

            return new SqlMigration(sourceName.Trim(), sql, batches);
        }
    }
}
