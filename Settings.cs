using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace PluginManager
{
    [Serializable]
    [XmlRoot("Settings")]
    public class Settings
    {
        private const string FILE_NAME = "PluginManager.config";

        #region Default Skip List
        private static readonly HashSet<string> defaultSkipList = new HashSet<string>() {
            (new FileInfo(Assembly.GetExecutingAssembly().Location)).Name.ToLower(),
            "sandbox.game.dll"
        };
        #endregion

        public static Settings Load()
        {
            Settings settings;
            try
            {
                XmlSerializer formatter = new XmlSerializer(typeof(Settings));
                using (FileStream fStream = new FileStream(Environment.CurrentDirectory + "\\" + FILE_NAME, FileMode.Open))
                {
                    settings = (Settings)formatter.Deserialize(fStream);
                }
                if (settings == null)
                    settings = new Settings();
                else
                    settings.SkipList.UnionWith(defaultSkipList);
            }
            catch
            {
                //Utilities.Log.WriteLineAndConsole($"Error load settings: {e.ToString()}");
                settings = new Settings();
            }

            return settings;
        }
        public void Save()
        {
            try
            {
                XmlSerializer formatter = new XmlSerializer(typeof(Settings));

                using (FileStream fStream = new FileStream(Environment.CurrentDirectory + "\\" + FILE_NAME, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fStream, this);
                }
            }
            catch (Exception e)
            {
                Utilities.Log.WriteLineAndConsole($"Error save settings: {e.ToString()}");
            }
        }

        public bool AutomaticPluginsSearch { get; set; }

        [XmlArrayItem("Library")]
        public HashSet<Library> WatchList { get; set; }

        [XmlArrayItem("Library")]
        public HashSet<string> SkipList { get; set; }

        private Settings()
        {
            AutomaticPluginsSearch = true;
            WatchList = new HashSet<Library>();
            SkipList = new HashSet<string>();
            SkipList.UnionWith(defaultSkipList);
        }

        [Serializable]
        public class Library
        {
            [XmlIgnore]
            private string name;

            [XmlText]
            public string Name
            {
                get { return name; }
                set { name = value == null ? null : value.ToLower(); }
            }
            [XmlAttribute]
            public string Version { get; set; }
            [XmlAttribute]
            public DateTime LastUpdate { get; set; }
            [XmlAttribute]
            public DateTime LastCheck { get; set; }

            public Library()
            {
                Name = null;
                Version = null;
                LastUpdate = DateTime.MinValue;
            }
            public Library(string name) : base()
            {
                Name = name;
            }

            public override int GetHashCode() => name == null ? string.Empty.GetHashCode() : name.GetHashCode();
        }
    }
}
