using System;

namespace Werm.Core.Printing
{
    public sealed class LabelTemplateException : InvalidOperationException
    {
        public LabelTemplateException(string message)
            : base(message)
        {
        }
    }
}
