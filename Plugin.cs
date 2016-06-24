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
        public void Init(object gameInstance)
        {
            Settings.Instance.Load();
            if (Settings.Instance.AutomaticPluginsSearch)
            {
                Utilities.Log.WriteLineAndConsole($"Search plugins in the root directory: {Environment.CurrentDirectory}");
                DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory);
                var files = directory.GetFiles("*.dll");
                string filename;
                foreach (var file in files)
                {
                    filename = Path.GetFileNameWithoutExtension(file.Name);
                    if (!Settings.Instance.SkipList.Exists((e) => string.Equals(e, filename, StringComparison.InvariantCultureIgnoreCase))
                        && !Settings.Instance.WatchList.Exists((e) => string.Equals(e.Name, filename, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (ExtendedPlugins.LibraryHasInterface<IPlugin>(filename))
                        {
                            Utilities.Log.WriteLineAndConsole($"Add assembly in WatchList: {filename}");
                            Settings.Instance.WatchList.Add(new Settings.Library(filename));
                        }
                        else
                        {
                            Utilities.Log.WriteLineAndConsole($"Add assembly in SkipList: {filename}");
                            Settings.Instance.SkipList.Add(filename);
                        }
                    }
                }
            }

            ExtendedPlugins.Register();
            ExtendedPlugins.CheckPluginsUpdates();
            ExtendedPlugins.Init(gameInstance);
        }

        public void Update() => ExtendedPlugins.Update();
        public void Dispose()
        {
            Settings.Instance.Save();

            ExtendedPlugins.Unload();
        }
    }
}
