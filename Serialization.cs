using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace PluginManager.Serialization
{
    class GitHubAPI
    {
        [DataContract(Name = "release")]
        public class Release
        {
            [IgnoreDataMember]
            private static DataContractJsonSerializer Formatter => new DataContractJsonSerializer(
                    typeof(Release),
                    new DataContractJsonSerializerSettings() { DateTimeFormat = new DateTimeFormat("yyyy-MM-dd'T'HH:mm:ssZ") });

            [DataMember(Name = "url")]
            public string Url { get; set; }

            [DataMember(Name = "assets_url")]
            public string AssetsUrl { get; set; }

            [DataMember(Name = "upload_url")]
            public string UploadUrl { get; set; }

            [DataMember(Name = "id")]
            public uint Id { get; set; }

            [DataMember(Name = "tag_name")]
            public string TagName { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "created_at")]
            public DateTime CreatedAt { get; set; }

            [DataMember(Name = "published_at")]
            public DateTime PublishedAt { get; set; }

            [DataMember(Name = "assets")]
            public Asset[] Assets { get; set; }

            public static Release Deserialize(string fileName)
            {
                try
                {
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
                    {
                        return (Release)Formatter.ReadObject(fileStream);
                    }
                }
                catch { }

                return null;
            }
            public static Release Deserialize(Stream stream)
            {
                try
                {
                    return (Release)Formatter.ReadObject(stream);
                }
                catch { }

                return null;
            }

            public static void Serialize(Stream stream, Release obj)
            {
                try
                {
                    Formatter.WriteObject(stream, obj);
                }
                catch { }
            }

            [DataContract(Name = "asset")]
            public class Asset
            {
                [DataMember(Name = "url")]
                public string Url { get; set; }

                [DataMember(Name = "id")]
                public uint Id { get; set; }

                [DataMember(Name = "name")]
                public string Name { get; set; }

                [DataMember(Name = "content_type")]
                public string ContentType { get; set; }

                [DataMember(Name = "size")]
                public long Size { get; set; }

                [DataMember(Name = "download_count")]
                public long DownloadCount { get; set; }

                [DataMember(Name = "created_at")]
                public DateTime CreatedAt { get; set; }

                [DataMember(Name = "published_at")]
                public DateTime PublishedAt { get; set; }

                [DataMember(Name = "browser_download_url")]
                public string BrowserDownloadUrl { get; set; }
            }
        }
    }
}
