using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluginManager.Core
{
    class ExtendedPlugins
    {
        private static List<ExtendedPlugin> m_plugins = new List<ExtendedPlugin>();

        public static bool HasEntryPoint<T>(string filename)
        {
            try
            {
                var assembly = Assembly.LoadFrom(filename);
                return assembly.GetTypes().Where(s => s.GetInterfaces().Contains(typeof(T))).Count() > 0;
            }
            catch { }

            return false;
        }

        public static void Register(List<Settings.Library> files)
        {
            ExtendedPlugin plugin;
            bool success;
            foreach (var file in files)
            {
                plugin = new ExtendedPlugin(file.Name, out success);
                if (success)
                {
                    if (string.IsNullOrEmpty(file.Version)) file.Version = plugin.Version;
                    if (file.UpToDate == null) file.UpToDate = new DateTime(1900, 1, 1);
                    m_plugins.Add(plugin);
                }
            }
        }

        public static void CheckPluginsUpdates()
        {

            Utilities.Log.WriteLineAndConsole("Check plugins updates START");
            string repositoryURL = "https://api.github.com/repos/{0}/{1}/releases/latest";

            //Parallel.ForEach<ExtendedPlugin>(m_plugins, (plugin) => { plugin... });
            foreach (var plugin in m_plugins)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format(repositoryURL, plugin.Instance.UpdateInfo.UserName, plugin.Instance.UpdateInfo.RepositoryName));
                    request.KeepAlive = false;
                    request.UserAgent = ".NET Framework";
                    request.ContentType = "application/json";

                    var response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                        using (Stream webStream = response.GetResponseStream())
                        using (FileStream fStream = new FileStream(Environment.CurrentDirectory + $@"\{plugin.Name}.data.json", FileMode.OpenOrCreate))
                            webStream.CopyTo(fStream);
                }
                catch (Exception e)
                {
                    Utilities.Log.WriteLineAndConsole($"Plugin \"{plugin.Name}\" update error: {e.ToString()}");
                }
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
            public IExtendedPlugin Instance { get; private set; }

            private ExtendedPlugin() { }
            public ExtendedPlugin(string name, out bool success)
            {
                try
                {
                    Name = name;
                    var assembly = Assembly.LoadFrom(name);
                    var pluginInterfaceClasses = assembly.GetTypes().Where(s => s.GetInterfaces().Contains(typeof(IExtendedPlugin)));
                    if (pluginInterfaceClasses.Count() > 0)
                    {
                        var myFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(name);
                        Version = myFileVersionInfo.FileVersion;
                        foreach (var pluginClass in pluginInterfaceClasses)
                        {
                            Utilities.Log.WriteLineAndConsole($"Creating instance of: {name} / {pluginClass.FullName}");
                            Instance = (IExtendedPlugin)Activator.CreateInstance(pluginClass);
                            break;
                        }
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

        private ExtendedPlugins() { }
    }
}
