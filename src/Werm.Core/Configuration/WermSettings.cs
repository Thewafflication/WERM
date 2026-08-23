using System;
using System.IO;
using System.Xml;

namespace Werm.Core.Configuration
{
    public sealed class WermSettings
    {
        public string DatabasePath { get; set; }
        public string OdbcDriverName { get; set; }
        public string OdbcDsn { get; set; }
        public string WordTemplatePath { get; set; }

        public WermSettings Clone()
        {
            return new WermSettings
            {
                DatabasePath = DatabasePath,
                OdbcDriverName = OdbcDriverName,
                OdbcDsn = OdbcDsn,
                WordTemplatePath = WordTemplatePath
            };
        }
    }

    public sealed class WermSettingsStore
    {
        private readonly string _path;

        public WermSettingsStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A settings path is required.", nameof(path));
            }
            _path = Path.GetFullPath(path);
        }

        public string PathName
        {
            get { return _path; }
        }

        public WermSettings Load(WermSettings fallback)
        {
            WermSettings settings = (fallback ?? new WermSettings()).Clone();
            if (!File.Exists(_path))
            {
                return settings;
            }

            var document = new XmlDocument { XmlResolver = null };
            using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            }))
            {
                document.Load(reader);
            }

            XmlElement root = document.DocumentElement;
            if (root == null || root.Name != "WermSettings" || root.GetAttribute("version") != "1")
            {
                throw new InvalidDataException("The WERM settings file has an unsupported format.");
            }

            settings.DatabasePath = Read(root, "DatabasePath", settings.DatabasePath);
            settings.OdbcDriverName = Read(root, "OdbcDriverName", settings.OdbcDriverName);
            settings.OdbcDsn = Read(root, "OdbcDsn", settings.OdbcDsn);
            settings.WordTemplatePath = Read(root, "WordTemplatePath", settings.WordTemplatePath);
            return settings;
        }

        public void Save(WermSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            string directory = System.IO.Path.GetDirectoryName(_path);
            Directory.CreateDirectory(directory);

            var document = new XmlDocument { XmlResolver = null };
            XmlElement root = document.CreateElement("WermSettings");
            root.SetAttribute("version", "1");
            document.AppendChild(root);
            Append(document, root, "DatabasePath", settings.DatabasePath);
            Append(document, root, "OdbcDriverName", settings.OdbcDriverName);
            Append(document, root, "OdbcDsn", settings.OdbcDsn);
            Append(document, root, "WordTemplatePath", settings.WordTemplatePath);

            string temporaryPath = _path + ".tmp";
            try
            {
                using (var stream = new FileStream(
                    temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = XmlWriter.Create(stream, new XmlWriterSettings
                {
                    Encoding = new System.Text.UTF8Encoding(false),
                    Indent = true
                }))
                {
                    document.Save(writer);
                }

                if (File.Exists(_path))
                {
                    File.Replace(temporaryPath, _path, null);
                }
                else
                {
                    File.Move(temporaryPath, _path);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        private static string Read(XmlElement root, string name, string fallback)
        {
            XmlElement element = root[name];
            return element == null ? fallback : element.InnerText.Trim();
        }

        private static void Append(
            XmlDocument document,
            XmlElement root,
            string name,
            string value)
        {
            XmlElement element = document.CreateElement(name);
            element.InnerText = (value ?? string.Empty).Trim();
            root.AppendChild(element);
        }
    }
}
