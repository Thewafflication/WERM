using System;
using System.Collections.Generic;

namespace Werm.Core.Printing
{
    public interface ILabelDocument : IDisposable
    {
        IReadOnlyCollection<string> AvailableFieldNames { get; }
        void SetField(string fieldName, string value);
        void Print(string printerName, int copies);
    }
}
