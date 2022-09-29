using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace GraphicalMirai
{
    public class AvailableReleases
    {
        public List<int> available_lts_releases;
        public List<int> available_releases;
        public int most_recent_feature_release;
        public int most_recent_feature_version;
        public int most_recent_lts;
        public int tip_version;
    }

    public class ReleaseVersionsParams : VersionsParams
    {
        public string? version;

        public override string ToString()
        {
            string s = base.ToString();
            if (version != null) s += ((s.Length > 0 ? "&" : "?")) + "version=" + UrlEncoder.Default.Encode(version);
            return s;
        }
    }
    public class AssetVersionsParams : VersionsParams
    {
        public string version;
        public AssetVersionsParams(string version) { this.version = version; }

        public override string ToString()
        {
            return "/" + version + base.ToString();
        }
    }
    public class VersionsParams
    {
        public enum Architecture
        {
            x64, x86, x32, ppc64, ppc64le, s390x, aarch64, arm, sparcv9, riscv64
        }
        public enum CLib
        {
            musl, glibc
        }
        public enum HeapSize
        {
            normal, large
        }
        public enum ImageType
        {
            jdk, jre/*, testimage, debugimage, staticlibs, sources, sbom*/
        }
        public enum OS
        {
            linux, windows, mac, solaris, aix, alpine_linux
        }
        public enum Project
        {
            jdk, valhalla, metropolis, jfr, shenandoah
        }
        public enum ReleaseType
        {
            ga, ea
        }
        public enum SortMethod
        {
            DEFAULT, DATE
        }
        public enum SortOrder
        {
            ASC, DESC
        }
        public Architecture? architecture;
        public CLib? c_lib;
        public HeapSize? heap_size;
        public ImageType? image_type;
        public bool lts = false;
        public OS? os;
        public int page = 0;
        public int page_size = 10;
        public Project? project;
        public ReleaseType? release_type;
        public SortMethod? sort_method;
        public SortOrder? sort_order;
        public void CopyFrom(VersionsParams v)
        {
            architecture = v.architecture;
            c_lib = v.c_lib;
            heap_size = v.heap_size;
            image_type = v.image_type;
            lts = v.lts;
            os = v.os;
            page = v.page;
            page_size = v.page_size;
            project = v.project;
            release_type = v.release_type;
            sort_method = v.sort_method;
            sort_order = v.sort_order;
        }
        public override string ToString()
        {
            Dictionary<string, string> @params = new();
            if (architecture.HasValue) @params.Add("architecture", architecture.Value.ToString());
            if (c_lib.HasValue) @params.Add("c_lib", c_lib.Value.ToString());
            if (heap_size.HasValue) @params.Add("heap_size", heap_size.Value.ToString());
            if (image_type.HasValue) @params.Add("image_type", image_type.Value.ToString());
            if (lts) @params.Add("lts", lts.ToString());
            if (os.HasValue) @params.Add("os", os.Value.ToString());
            if (page != 0) @params.Add("page", page.ToString());
            if (page_size != 10) @params.Add("page_size", page_size.ToString());
            if (project.HasValue) @params.Add("project", project.Value.ToString());
            if (release_type.HasValue) @params.Add("release_type", release_type.Value.ToString());
            if (sort_method.HasValue) @params.Add("sort_method", sort_method.Value.ToString());
            if (sort_order.HasValue) @params.Add("sort_order", sort_order.Value.ToString());
            string s = string.Join("&", @params.Select((kv, i) => kv.Key + "=" + UrlEncoder.Default.Encode(kv.Value)));
            return s.Length > 0 ? ("?" + s) : "";
        }
    }
    public class ReleaseVersions
    {
        public List<ReleaseVersion> versions;
    }
    public class ReleaseVersion
    {
        public int major;
        public int minor;
        public int security;
        public int patch;
        public string pre;
        public int adopt_build_number;
        public string semver;
        public string openjdk_version;
        public int build;
        public string optional;
    }
    public class AssetVersions
    {
        public List<AssetBinary> binaries;
        public int download_count;
        public string release_link;
        public string release_name;
        public string release_type;
        public ReleaseSource source;
        public string timestamp;
        public string updated_at;
        public string vendor;
        public ReleaseVersionData version_data;
    }
    public class AssetBinary
    {
        public string architecture;
        public int download_count;
        public string heap_size;
        public string image_type;
        public string jvm_impl;
        public string os;
        public AssetPackage package;
        public string project;
        public string scm_ref;
        public string updated_at;
    }
    public class AssetPackage
    {
        public string checksum;
        public string checksum_link;
        public int download_count;
        public string link;
        public string metadata_link;
        public string name;
        public string signature_link;
        public int size;
    }
    public class ReleaseVersionData
    {
        public int build;
        public int major;
        public int minor;
        public string openjdk_version;
        public string optional;
        public string pre;
        public int security;
        public string semver;
    }
    public class ReleaseSource
    {
        public string link;
        public string nane;
        public int size;
    }
    public class AdoptiumApi
    {
        private static readonly Lazy<HttpClient> lazyHttpClient = new(() =>
        {
            var http = new HttpClient()
            {
                BaseAddress = new Uri("https://api.adoptium.net/v3/"),
                DefaultRequestVersion = new System.Version(1, 1),
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
                Timeout = TimeSpan.FromSeconds(30)
            };
            http.DefaultRequestHeaders.Add("User-Agent", App.UserAgent);
            return http;
        });
        private static HttpClient httpClient => lazyHttpClient.Value;
        public static async Task<AvailableReleases?> GetAvailableReleases()
        {
            var path = "info/available_releases";
            AvailableReleases? result = null;
            var jsonText = await httpClient.GetStringAsync(path, jsonText =>
            {
                result = JsonConvert.DeserializeObject<AvailableReleases>(jsonText);
            }, ex =>
            {
                if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("There is an exception when fetching " + path);
                    Console.WriteLine(ex);
                }
            });
            return result;
        }
        public static async Task<ReleaseVersions?> GetReleaseVersions(ReleaseVersionsParams? @params)
        {
            var path = "info/release_versions" + (@params?.ToString() ?? "");
            ReleaseVersions? result = null;
            await httpClient.GetStringAsync(path, jsonText =>
            {
                result = JsonConvert.DeserializeObject<ReleaseVersions>(jsonText);
            }, ex =>
            {
                if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("There is an exception when fetching " + path);
                    Console.WriteLine(ex);
                }
            });
            return result;
        }
        public static async Task<List<AssetVersions>> GetAssetVersions(AssetVersionsParams @params)
        {
            var path = "assets/version" + @params.ToString();
            List<AssetVersions> result = new();
            await httpClient.GetStringAsync(path, jsonText =>
            {
                var temp = JsonConvert.DeserializeObject<List<AssetVersions>?>(jsonText);
                if (temp != null) result = temp;
            }, ex =>
            {
                if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("There is an exception when fetching " + path);
                    Console.WriteLine(ex);
                }
            });
            return result;
        }
    }
}
