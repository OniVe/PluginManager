using PluginManager.Core;
using Sandbox;
using System;
using System.IO;
using VRage.Plugins;
using PluginManager.Utilities;

namespace PluginManager
{
    public class Plugin : IPlugin
    {
        private static Settings settings;
        public void Init(object gameInstance)
        {
            settings = Settings.Load();
            if (settings.AutomaticPluginsSearch)
            {
                Utilities.Log.WriteLineAndConsole($"Search plugins in the root directory: {Environment.CurrentDirectory}");
                DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory);
                var files = directory.GetFiles("*.dll");
                string filename;
                foreach (var file in files)
                {
                    filename = file.Name.ToLower();
                    if (!settings.SkipList.Exists((e) => e == filename) && !settings.WatchList.Exists((e) => e.Name == filename))
                    {
                        if (ExtendedPlugins.HasInterface<IPlugin>(filename))
                        {
                            Utilities.Log.WriteLineAndConsole($"Add file in WatchList: {filename}");
                            settings.WatchList.Add(new Settings.Library(filename));
                        }
                        else
                        {
                            Utilities.Log.WriteLineAndConsole($"Add file in SkipList: {filename}");
                            settings.SkipList.Add(filename);
                        }
                    }
                }
            }

            ExtendedPlugins.Register(settings.WatchList);
            ExtendedPlugins.CheckPluginsUpdates();
            ExtendedPlugins.Init(gameInstance);
        }

        public void Update() => ExtendedPlugins.Update();
        public void Dispose()
        {
            if (settings != null)
                settings.Save();

            ExtendedPlugins.Unload();
        }
    }
}
