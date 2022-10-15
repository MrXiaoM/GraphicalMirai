using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace GraphicalMirai
{
    public class Package
    {
        [YamlMember(Alias = "group", ScalarStyle = ScalarStyle.SingleQuoted)]
        public string Group { get; set; } = "";
        [YamlMember(Alias = "name", ScalarStyle = ScalarStyle.SingleQuoted)]
        public string Name { get; set; } = "";
        [YamlMember(Alias = "version", ScalarStyle = ScalarStyle.SingleQuoted)]
        public string Version { get; set; } = "";
        [YamlMember(Alias = "classifier", ScalarStyle = ScalarStyle.SingleQuoted)]
        public string Classifier { get; set; } = "";
        [YamlMember(Alias = "is-plugin", ScalarStyle = ScalarStyle.SingleQuoted)]
        public bool IsPlugin { get; set; } = false;

        public Package() { }
        public Package(string group, string name, string version, bool isPlugin = false, string classifier = "")
        {
            Group = group;
            Name = name;
            Version = version;
            Classifier = classifier;
            IsPlugin = isPlugin;
        }

        [YamlIgnore]
        public bool IsExist
        {
            get
            {
                string prefix = "mirai/" + (IsPlugin ? "plugins" : "content") + "/" + MavenFileName;
                if (IsPlugin)
                {
                    return App.exists(prefix + ".mirai.jar") || App.exists(prefix + ".mirai2.jar");
                }
                return App.exists(prefix + ".jar");
            }
        }
        /// <summary>
        /// ".后缀"
        /// </summary>
        [YamlIgnore]
        public string MavenFileExt
        {
            get { return ".jar"; }
        }
        /// <summary>
        /// "名称-版本.后缀"
        /// </summary>
        [YamlIgnore]
        public string MavenFileName
        {
            get
            {
                return Name + "-" + Version + (Classifier.Length > 0 ? ("-" + Classifier) : "");
            }
        }
        /// <summary>
        /// "组/名称/版本/名称-版本.后缀"
        /// </summary>
        [YamlIgnore]
        public string MavenUriRelative
        {
            get
            {
                return Group.Replace(".", "/") + "/" + Name + "/" + Version + "/" + MavenFileName + MavenFileExt;
            }
        }
    }
    public class PackagesData
    {
        [YamlIgnore]
        private static readonly ISerializer SERIALIZER = new SerializerBuilder()
            .EnsureRoundtrip()
            .WithTagMapping("!PackagesData", typeof(PackagesData))
            .Build();
        [YamlIgnore]
        private static readonly IDeserializer DESERIALIZER = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithTagMapping("!PackagesData", typeof(PackagesData))
            .Build();
        [YamlIgnore]
        public static string ConfigPath => App.path("packages.yml");
        [YamlIgnore]
        private static PackagesData? instance;
        [YamlIgnore]
        public static PackagesData Instance
        {
            get
            {
                try
                {
                    if (!File.Exists(ConfigPath)) throw new FileNotFoundException(ConfigPath);
                    return instance ??= DESERIALIZER.Deserialize<PackagesData>(File.ReadAllText(ConfigPath));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    instance = new PackagesData();
                    Save();
                    return instance;
                }
            }
        }
        public static void Save()
        {
            if (instance == null) instance = new PackagesData();
            File.WriteAllText(ConfigPath, SERIALIZER.Serialize(instance));
        }


        [YamlMember(Alias = "mirai-repo", Description = "mirai-repo 地址", ScalarStyle = ScalarStyle.SingleQuoted)]
        public string MiraiRepoUrl { get; set; } = "https://mirai.mamoe.net/assets/mcl/";

        [YamlMember(Alias = "repositories", Description = "下载依赖/插件时使用的 Maven 仓库", ScalarStyle = ScalarStyle.SingleQuoted)]
        public Dictionary<string, string> Repositories { get; set; } = new()
        {
            { "https://maven.aliyun.com/repository/central", "阿里云 Maven 镜像" },
            { "https://repo1.maven.org/maven2", "Maven Central" },
        };

        [YamlMember(Alias = "selected-mirai-version", Description = "指定要下载/启动的 mirai 版本", ScalarStyle = ScalarStyle.SingleQuoted)]
        public string? selectedMiraiVersion { get; set; }

        [YamlIgnore]
        public Version? SelectedMiraiVersion { get { return Version.Parse(selectedMiraiVersion); } }
        [YamlMember(Alias = "packages", Description = "启动 mirai 时自动下载的包", ScalarStyle = ScalarStyle.SingleQuoted)]
        public List<Package> Packages { get; set; } = new List<Package>()
        {
            new("org.bouncycastle", "bcprov-jdk15on", "1.70"),
            new("net.mamoe", "mirai-core-all", "{version}", false, "all"),
            new("net.mamoe", "mirai-console-all", "{version}", false, "all"),
            new("net.mamoe", "mirai-core-all", "{version}", false, "all"),
        };
    }
}
