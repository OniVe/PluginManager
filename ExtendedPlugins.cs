using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using VRage.Plugins;

namespace PluginManager.Core
{
    class ExtendedPlugins
    {
        private const string RepositoryURL = "https://api.github.com/repos/{0}/{1}/releases/latest";

        private static List<ExtendedPlugin> m_plugins = new List<ExtendedPlugin>();

        public static bool HasInterface<T>(string filename)
        {
            try
            {
                var assembly = Assembly.LoadFrom(filename);
                return assembly.GetTypes().Where(s => s.GetInterfaces().Contains(typeof(T))).Count() > 0;
            }
            catch { }

            return false;
        }

        public static void Register(HashSet<Settings.Library> files)
        {
            ExtendedPlugin plugin;
            bool success;
            foreach (var file in files)
            {
                plugin = new ExtendedPlugin(file.Name, out success);
                if (success)
                {
                    if (string.IsNullOrEmpty(file.Version)) file.Version = plugin.Version;
                    if (file.LastUpdate == null) file.LastUpdate = DateTime.MinValue;
                    m_plugins.Add(plugin);
                }
            }
        }

        /*private static async Task<Serialization.GitHubAPI.Release> GetGitHubReleasAsync(ExtendedPlugin plugin)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(RepositoryURL, plugin.Instance.UpdateInfo.UserName, plugin.Instance.UpdateInfo.RepositoryName));
                request.KeepAlive = false;
                request.UserAgent = ".NET Framework";
                request.ContentType = "application/json";

                var response = (HttpWebResponse)(await request.GetResponseAsync());
                if (response.StatusCode == HttpStatusCode.OK)
                    using (Stream webStream = response.GetResponseStream())
                        return Serialization.GitHubAPI.Release.Deserialize(webStream);
            }
            catch { }

            return null;
        }

        private static async Task<List<Serialization.GitHubAPI.Release>> GetGitHubReleasesAsync()
        {
            List<Task<Serialization.GitHubAPI.Release>> tasks = new List<Task<Serialization.GitHubAPI.Release>>();
            foreach (var plugin in m_plugins)
                tasks.Add(GetGitHubReleasAsync(plugin));

            return (await Task.WhenAll(tasks)).ToList();
        }
        */
        public static void CheckPluginsUpdates()
        {
            Utilities.Log.WriteLineAndConsole("Check plugins updates START");

            foreach (var plugin in m_plugins)
            {
                //[PluginManagerInfoAttribute]
                //public class Plugin : IPlugin { }
                var instanceType = plugin.Instance.GetType();
                var attributeType = plugin.Assembly.GetType(instanceType.Namespace + ".PluginManagerInfoAttribute");
                if (attributeType == null)
                    continue;

                PluginInfo attribute;
                try { attribute = PluginInfo.Create(instanceType.GetCustomAttribute(attributeType)); }
                catch { attribute = null; }

                if (attribute == null)
                    continue;

            }

            Utilities.Log.WriteLineAndConsole("Check plugins updates END");
        }

        public static void Init(object gameInstance)
        {
            foreach (var plugin in m_plugins)
                plugin.Instance.Init(gameInstance);
        }
        public static void Update()
        {
            foreach (var plugin in m_plugins)
                plugin.Instance.Update();
        }
        public static void Unload()
        {
            foreach (var plugin in m_plugins)
                plugin.Dispose();

            m_plugins.Clear();
        }

        private class ExtendedPlugin : IDisposable
        {
            public string Name { get; private set; }
            public string Version { get; private set; }
            public Assembly Assembly { get; private set; }
            /// <summary>
            /// First Instance in Assembly
            /// </summary>
            public IPlugin Instance { get; private set; }

            private ExtendedPlugin() { }
            public ExtendedPlugin(string name, out bool success)
            {
                try
                {
                    Name = name;
                    Assembly = Assembly.LoadFrom(name);
                    var pluginInterface = Assembly.GetTypes().Where(s => s.GetInterfaces().Contains(typeof(IPlugin))).FirstOrDefault();
                    if (pluginInterface != null)
                    {
                        var myFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(name);
                        Version = myFileVersionInfo.FileVersion;
                        Utilities.Log.WriteLineAndConsole($"Creating instance of: {name} / {pluginInterface.FullName}");
                        Instance = (IPlugin)Activator.CreateInstance(pluginInterface);
                        success = true;
                    }
                    else
                    {
                        Utilities.Log.WriteLineAndConsole($"Error registration plugin. File: \"{name}\" does not contain an interface \"IExtendedPlugin\"");
                        success = false;
                    }
                }
                catch (Exception e)
                {
                    Utilities.Log.WriteLineAndConsole($"Cannot load Plugin: {name}, Error: {e.ToString()}");
                    success = false;
                }
            }

            public void Dispose()
            {
                if (Instance != null)
                    Instance.Dispose();

                Name = null;
                Assembly = null;
                Instance = null;

                GC.SuppressFinalize(this);
            }
        }

        private class PluginInfo : System.Attribute
        {
            public string UserName;
            public string RepositoryName;
            public string FileName;
            public string Version;

            public static PluginInfo Create(dynamic obj)
            {
                try
                {
                    return new PluginInfo()
                    {
                        FileName = obj.FileName,
                        RepositoryName = obj.RepositoryName,
                        UserName = obj.UserName,
                        Version = obj.Version
                    };
                }
                catch { }

                return null;
            }
        }

        private ExtendedPlugins() { }
    }
}
