using PluginManager.Core;
using Sandbox;
using System;
using System.IO;
using VRage.Plugins;

namespace PluginManager
{
    public class Plugin : IPlugin
    {
        private static Settings settings;
        public void Init(object gameInstance)
        {
            settings = Settings.Load();
            if (settings.AutomaticSearchPlugins)
            {
                Utilities.Log.WriteLineAndConsole($"Search plugins in the root directory: {Environment.CurrentDirectory}");
                DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory);
                var files = directory.GetFiles("*.dll");
                string filename;
                foreach (var file in files)
                {
                    filename = file.Name.ToLower();
                    if (!settings.SkipList.Contains(filename))
                    {
                        if (ExtendedPlugins.HasEntryPoint<IExtendedPlugin>(filename))
                        {
                            Utilities.Log.WriteLineAndConsole($"Add file in WatchList: {filename}");
                            settings.WatchList.Add(new Settings.Library() { Name = filename });
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

    public struct UpdateInfo
    {
        public string UserName;
        public string RepositoryName;
        public string FileName;
        public string Version;
    }

    public interface IExtendedPlugin : IDisposable
    {
        UpdateInfo UpdateInfo { get; }

        void Init(object gameInstance);
        void Update();
    }
}
