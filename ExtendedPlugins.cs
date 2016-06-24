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

        public static bool LibraryHasInterface<T>(string filename)
        {
            bool result = false;
            AppDomain domain = AppDomain.CreateDomain("VirtualExtendedPlugins");
            try
            {
                Assembly assembly;
                assembly = domain.Load(new AssemblyName(filename));

                result = assembly.GetTypes().Where(s => s.GetInterfaces().Contains(typeof(T))).Count() > 0;
            }
            catch { }
            finally
            {
                AppDomain.Unload(domain);
            }

            return result;
        }

        public static void Register()
        {
            ExtendedPlugin plugin;
            bool success;
            foreach (var library in Settings.Instance.WatchList)
            {
                plugin = new ExtendedPlugin(library.Name, out success);
                if (success)
                {
                    if (string.IsNullOrEmpty(library.Version)) library.Version = plugin.FileVersion;
                    if (library.LastUpdate == null) library.LastUpdate = DateTime.MinValue;
                    m_plugins.Add(plugin);
                }
            }
        }

        private static async Task<Serialization.GitHubAPI.Release> GetGitHubReleasAsync(ExtendedPlugin plugin)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(RepositoryURL, plugin.Info.UserName, plugin.Info.RepositoryName));
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

        public static void CheckPluginsUpdates()
        {
            Utilities.Log.WriteLineAndConsole("Check plugins updates...");

            List<Task<Serialization.GitHubAPI.Release>> tasks = new List<Task<Serialization.GitHubAPI.Release>>();
            foreach (var plugin in m_plugins)
                tasks.Add(GetGitHubReleasAsync(plugin));

            Task.WhenAll(tasks).Wait();

            foreach(var task in tasks)
            {
                ///----------------------task.Result
            }
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
            public string FileVersion { get; private set; }
            public PackageInfo Info { get; private set; }
            public string DomainName { get; private set; }
            public AppDomain Domain { get; private set; }
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
                    DomainName = Guid.NewGuid().ToString();
                    Domain = AppDomain.CreateDomain(DomainName);
                    Assembly = Assembly.Load(new AssemblyName(Name));
                    var pluginInterface = Assembly.GetTypes().Where(s => s.GetInterfaces().Contains(typeof(IPlugin))).FirstOrDefault();
                    if (pluginInterface != null)
                    {
                        FileVersion = Assembly.FullName;
                        Utilities.Log.WriteLineAndConsole($"Creating instance of: {name} / {pluginInterface.FullName}");
                        Instance = (IPlugin)Activator.CreateInstance(pluginInterface);
                        Info = PackageInfo.Create(this);

                        success = true;
                    }
                    else
                    {
                        Utilities.Log.WriteLineAndConsole($"Error registration plugin. File: \"{name}\" does not contain an interface \"IExtendedPlugin\"");
                        AppDomain.Unload(Domain);
                        success = false;
                    }
                }
                catch (Exception e)
                {
                    Utilities.Log.WriteLineAndConsole($"Cannot load Plugin: {name}, Error: {e.ToString()}");
                    AppDomain.Unload(Domain);
                    success = false;
                }
            }

            public void Dispose()
            {
                if (Instance != null)
                    Instance.Dispose();

                Name = null; DomainName = null; FileVersion = null;
                Info = null; Instance = null; Assembly = null;

                if (Domain != null)
                    AppDomain.Unload(Domain);

                GC.SuppressFinalize(this);
            }
        }

        private class PackageInfo : System.Attribute
        {
            public string UserName;
            public string RepositoryName;
            public string ReleaseFileName;
            public string PackageFileName;

            /// <summary>
            ///[PluginManagerInfoAttribute]
            ///public class Plugin : IPlugin { } 
            /// </summary>
            /// <param name="plugin"></param>
            /// <returns></returns>
            public static PackageInfo Create(ExtendedPlugin plugin)
            {
                var instanceType = plugin.Instance.GetType();
                var attributeType = plugin.Assembly.GetType(instanceType.Namespace + ".PackageInfoAttribute");
                if (attributeType != null)
                {

                    PackageInfo attribute;
                    try { attribute = PackageInfo.Create(instanceType.GetCustomAttribute(attributeType)); }
                    catch { attribute = null; }

                    if (attribute != null)
                        return attribute;
                }
                return null;
            }
            public static PackageInfo Create(dynamic obj)
            {
                try
                {
                    return new PackageInfo()
                    {
                        UserName = obj.UserName,
                        RepositoryName = obj.RepositoryName,
                        ReleaseFileName = obj.FileName,
                        PackageFileName = obj.PackageFileName
                    };
                }
                catch { }

                return null;
            }
        }

        private ExtendedPlugins() { }
    }
}
