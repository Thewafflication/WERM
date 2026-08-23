using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Werm.Core.Printing
{
    public sealed class LabelPrintJob
    {
        public LabelPrintJob(
            string templatePath,
            string printerName,
            int copies,
            IDictionary<string, string> fieldValues)
        {
            if (string.IsNullOrWhiteSpace(templatePath))
            {
                throw new ArgumentException("A Word template path is required.", nameof(templatePath));
            }
            if (string.IsNullOrWhiteSpace(printerName))
            {
                throw new ArgumentException("A printer name is required.", nameof(printerName));
            }
            if (copies <= 0 || copies > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(copies));
            }
            if (fieldValues == null)
            {
                throw new ArgumentNullException(nameof(fieldValues));
            }

            TemplatePath = templatePath;
            PrinterName = printerName;
            Copies = copies;
            FieldValues = new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(fieldValues, StringComparer.Ordinal));
        }

        public string TemplatePath { get; private set; }
        public string PrinterName { get; private set; }
        public int Copies { get; private set; }
        public ReadOnlyDictionary<string, string> FieldValues { get; private set; }
    }
}
