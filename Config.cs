using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using System.IO;

namespace GraphicalMirai
{
    public class Config
    {
        [YamlIgnore]
        private static readonly Serializer SERIALIZER = new SerializerBuilder().Build();
        [YamlIgnore]
        private static readonly Deserializer DESERIALIZER = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

        [YamlIgnore]
        public static string ConfigPath { get { return Environment.CurrentDirectory + "\\config.yml"; } }
        [YamlIgnore]
        private static Config? instance;
        [YamlIgnore]
        public static Config Instance
        {
            get {
                try
                {
                    if (!File.Exists(ConfigPath)) throw new FileNotFoundException(ConfigPath);
                    return instance ??= DESERIALIZER.Deserialize<Config>(File.ReadAllText(ConfigPath));
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                    instance = new Config();
                    Save();
                    return instance;
                }
            }
        }
        public static void Save()
        {
            if (instance == null) instance = new Config();
            File.WriteAllText(ConfigPath, SERIALIZER.Serialize(instance));
        }
        [YamlMember(Alias = "repositories")]
        public Dictionary<string, string> repositories { get; set; } = new()
        {
            { "https://maven.aliyun.com/repository/central", "阿里云 Maven 镜像" },
            { "https://repo1.maven.org/maven2", "Maven Central" },
        };
        [YamlMember(Alias = "selected-mirai-version")]
        public string? selectedMiraiVersion { get; set; }
        [YamlMember(Alias = "main-class")]
        public string mainClass { get; set; } = "net.mamoe.mirai.console.terminal.MiraiConsoleTerminalLoader";
        [YamlMember(Alias = "java-path")]
        public string javaPath { get; set; } = "java";
        [YamlMember(Alias = "extra-arguments")]
        public string extArgs { get; set; } = "-Dfile.encoding=utf-8";
        [YamlMember(Alias = "console-color")]
        public Dictionary<string, string> dict_color { get; set; } = new() {
            { "[92m", "#4FC414" },
            { "[0m", "#BBBBBB" },
            { "[91m", "#FF4050" },
            { "[96m", "#00E5E5" },
            { "[31m", "#F0524F" }
        };
    }
}
