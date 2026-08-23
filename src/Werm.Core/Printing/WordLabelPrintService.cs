using System;
using System.Collections.Generic;

namespace Werm.Core.Printing
{
    public sealed class WordLabelPrintService : ILabelPrintService
    {
        private readonly ILabelDocumentFactory _documentFactory;

        public WordLabelPrintService(ILabelDocumentFactory documentFactory)
        {
            _documentFactory = documentFactory ??
                throw new ArgumentNullException(nameof(documentFactory));
        }

        public void Print(LabelPrintJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            using (ILabelDocument document = _documentFactory.CreateFromTemplate(job.TemplatePath))
            {
                var available = new HashSet<string>(
                    document.AvailableFieldNames, StringComparer.Ordinal);
                foreach (string requiredField in LabelFieldNames.Required)
                {
                    if (!available.Contains(requiredField))
                    {
                        throw new LabelTemplateException(
                            "The Word template is missing required content-control tag '" +
                            requiredField + "'.");
                    }
                }

                foreach (KeyValuePair<string, string> field in job.FieldValues)
                {
                    document.SetField(field.Key, field.Value);
                }
                document.Print(job.PrinterName, job.Copies);
            }
        }
    }
}
