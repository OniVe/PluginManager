using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace PluginManager
{
    [Serializable]
    [XmlRoot("Settings")]
    public class Settings
    {
        private const string FILE_NAME = "PluginManager.config";

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
            }
            catch (Exception e)
            {
                Utilities.Log.WriteLineAndConsole($"Error load settings: {e.ToString()}");
                settings = new Settings();
                settings.AutomaticSearchPlugins = true;
            }

            if (settings.WatchList == null) settings.WatchList = new List<Library>();
            if (settings.SkipList == null) settings.SkipList = new List<string>();

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

        public bool AutomaticSearchPlugins { get; set; }
        public List<Library> WatchList { get; set; }
        public List<string> SkipList { get; set; }

        private Settings() { }

        [Serializable]
        [XmlRoot("Library")]
        public class Library
        {
            [XmlText]
            public string Name { get; set; }
            [XmlAttribute]
            public string Version { get; set; }
            [XmlAttribute]
            public DateTime UpToDate { get; set; }
        }
    }
}
