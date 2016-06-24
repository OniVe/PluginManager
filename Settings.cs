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
        private static XmlSerializer Formatter => new XmlSerializer(typeof(Settings));

        #region Default SkipList
        private static readonly HashSet<string> defaultSkipList = new HashSet<string>() {
            Path.GetFileNameWithoutExtension((new FileInfo(Assembly.GetExecutingAssembly().Location)).Name),
            "Sandbox.Game",
            "VRage",
            "VRage.Game"
        };
        #endregion

        private static Settings instance;
        public static Settings Instance { get { if (instance == null) instance = new Settings(); return instance; } }

        public void Load()
        {
            Settings settings;
            try
            {
                using (FileStream fStream = new FileStream(Environment.CurrentDirectory + "\\" + FILE_NAME, FileMode.Open))
                    settings = (Settings)Formatter.Deserialize(fStream);

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

            this.AutomaticPluginsSearch = settings.AutomaticPluginsSearch;
            this.WatchList = settings.WatchList;
            this.SkipList = settings.SkipList;
        }
        public void Save()
        {
            try
            {
                using (FileStream fStream = new FileStream(Environment.CurrentDirectory + "\\" + FILE_NAME, FileMode.OpenOrCreate))
                    Formatter.Serialize(fStream, this);
            }
            catch (Exception e)
            {
                Utilities.Log.WriteLineAndConsole($"Error save settings: {e.ToString()}");
            }
        }

        public bool AutomaticPluginsSearch { get; set; }

        [XmlArrayItem("Assembly")]
        public HashSet<Library> WatchList { get; set; }

        [XmlArrayItem("Assembly")]
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
            [XmlText]
            public string Name { get; set; }
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

            public override int GetHashCode() => Name == null ? string.Empty.GetHashCode() : Name.ToLower().GetHashCode();
        }
    }
}
