using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Werm.Core.Printing;

namespace Werm.Printing
{
    internal sealed class WordComLabelDocument : ILabelDocument
    {
        private object _application;
        private object _document;
        private readonly ReadOnlyCollection<string> _availableFieldNames;
        private bool _disposed;

        private WordComLabelDocument(object application, object document)
        {
            _application = application;
            _document = document;
            _availableFieldNames = ReadAvailableFieldNames();
        }

        public IReadOnlyCollection<string> AvailableFieldNames
        {
            get { return _availableFieldNames; }
        }

        public static WordComLabelDocument Create(Type wordType, string templatePath)
        {
            object application = null;
            object documents = null;
            object document = null;
            try
            {
                application = Activator.CreateInstance(wordType);
                dynamic word = application;
                word.Visible = false;
                word.DisplayAlerts = 0;
                documents = word.Documents;
                dynamic collection = documents;
                document = collection.Add(
                    Template: templatePath,
                    NewTemplate: false,
                    Visible: false);
                return new WordComLabelDocument(application, document);
            }
            catch
            {
                CloseDocument(document);
                QuitApplication(application);
                ReleaseComObject(document);
                ReleaseComObject(application);
                throw;
            }
            finally
            {
                ReleaseComObject(documents);
            }
        }

        public void SetField(string fieldName, string value)
        {
            ThrowIfDisposed();
            bool populated = false;
            object controls = null;
            try
            {
                dynamic document = _document;
                controls = document.ContentControls;
                dynamic collection = controls;
                int count = collection.Count;
                for (int index = 1; index <= count; index++)
                {
                    object control = null;
                    try
                    {
                        control = collection.Item(index);
                        dynamic contentControl = control;
                        if (string.Equals(
                            Convert.ToString(contentControl.Tag), fieldName, StringComparison.Ordinal))
                        {
                            contentControl.LockContents = false;
                            object range = null;
                            try
                            {
                                range = contentControl.Range;
                                ((dynamic)range).Text = value ?? string.Empty;
                                populated = true;
                            }
                            finally
                            {
                                ReleaseComObject(range);
                            }
                        }
                    }
                    finally
                    {
                        ReleaseComObject(control);
                    }
                }
            }
            finally
            {
                ReleaseComObject(controls);
            }

            if (!populated)
            {
                throw new LabelTemplateException(
                    "The Word template field was not found: " + fieldName);
            }
        }

        public void Print(string printerName, int copies)
        {
            ThrowIfDisposed();
            dynamic word = _application;
            dynamic document = _document;
            string previousPrinter = null;
            try
            {
                previousPrinter = Convert.ToString(word.ActivePrinter);
                word.ActivePrinter = printerName;
                document.PrintOut(Background: false, Copies: copies);
            }
            finally
            {
                if (!string.IsNullOrEmpty(previousPrinter))
                {
                    word.ActivePrinter = previousPrinter;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            CloseDocument(_document);
            QuitApplication(_application);
            ReleaseComObject(_document);
            ReleaseComObject(_application);
            _document = null;
            _application = null;
        }

        private ReadOnlyCollection<string> ReadAvailableFieldNames()
        {
            var names = new List<string>();
            object controls = null;
            try
            {
                dynamic document = _document;
                controls = document.ContentControls;
                dynamic collection = controls;
                int count = collection.Count;
                for (int index = 1; index <= count; index++)
                {
                    object control = null;
                    try
                    {
                        control = collection.Item(index);
                        dynamic contentControl = control;
                        string tag = Convert.ToString(contentControl.Tag);
                        if (!string.IsNullOrWhiteSpace(tag) && !names.Contains(tag))
                        {
                            names.Add(tag);
                        }
                    }
                    finally
                    {
                        ReleaseComObject(control);
                    }
                }
            }
            finally
            {
                ReleaseComObject(controls);
            }
            return names.AsReadOnly();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WordComLabelDocument));
            }
        }

        private static void CloseDocument(object document)
        {
            if (document == null)
            {
                return;
            }
            try
            {
                ((dynamic)document).Close(SaveChanges: 0);
            }
            catch
            {
                // Cleanup is best effort; the original operation remains authoritative.
            }
        }

        private static void QuitApplication(object application)
        {
            if (application == null)
            {
                return;
            }
            try
            {
                ((dynamic)application).Quit(SaveChanges: 0);
            }
            catch
            {
                // Cleanup is best effort; the original operation remains authoritative.
            }
        }

        private static void ReleaseComObject(object value)
        {
            if (value != null && Marshal.IsComObject(value))
            {
                Marshal.FinalReleaseComObject(value);
            }
        }
    }
}
