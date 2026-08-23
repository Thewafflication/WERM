using System;
using System.IO;
using Werm.Core.Printing;

namespace Werm.Printing
{
    public sealed class WordComLabelDocumentFactory : ILabelDocumentFactory
    {
        public ILabelDocument CreateFromTemplate(string templatePath)
        {
            if (string.IsNullOrWhiteSpace(templatePath))
            {
                throw new ArgumentException("A Word template path is required.", nameof(templatePath));
            }

            string fullPath = Path.GetFullPath(templatePath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The Word label template was not found.", fullPath);
            }

            Type wordType = Type.GetTypeFromProgID("Word.Application");
            if (wordType == null)
            {
                throw new InvalidOperationException(
                    "Microsoft Word desktop automation is not registered on this workstation.");
            }

            return WordComLabelDocument.Create(wordType, fullPath);
        }
    }
}
