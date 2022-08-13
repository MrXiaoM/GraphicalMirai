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
        private static readonly ISerializer SERIALIZER = new SerializerBuilder().Build();
        [YamlIgnore]
        private static readonly IDeserializer DESERIALIZER = new DeserializerBuilder()
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
        [YamlMember(Alias = "repositories", Description = "下载依赖/插件时使用的 Maven 仓库")]
        public Dictionary<string, string> repositories { get; set; } = new()
        {
            { "https://maven.aliyun.com/repository/central", "阿里云 Maven 镜像" },
            { "https://repo1.maven.org/maven2", "Maven Central" },
        };
        
        [YamlMember(Alias = "selected-mirai-version", Description = "指定要启动的 mirai 版本")]
        public string? selectedMiraiVersion { get; set; }
        
        [YamlMember(Alias = "main-class", Description = "指定启动时 mirai 的主类")]
        public string mainClass { get; set; } = "net.mamoe.mirai.console.terminal.MiraiConsoleTerminalLoader";
        
        [YamlMember(Alias = "java-path", Description = "指定启动 mirai 所使用的 java 可执行文件路径")]
        public string javaPath { get; set; } = "java";
        
        [YamlMember(Alias = "extra-arguments", Description = "启动 mirai 时额外添加的 jvm 参数")]
        public string extArgs { get; set; } = "-Dfile.encoding=utf-8";

        [YamlMember(Alias = "console-color", Description = "控制台颜色格式控制符与颜色对照表\n除非有特殊需求，否则不要修改")]
        public Dictionary<string, string> dict_color { get; set; } = new() {
            { "[92m", "#4FC414" },
            { "[0m", "#BBBBBB" },
            { "[91m", "#FF4050" },
            { "[96m", "#00E5E5" },
            { "[31m", "#F0524F" }
        };

        [YamlMember(Alias = "webp-codec-check", Description = "是否在启动时检查程序是否可加载 webp 图片")]
        public bool webp_codec_check { get; set; } = true;
    }
}
