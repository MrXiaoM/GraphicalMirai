using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Permissions;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

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

    public class ReleaseVersionsParams
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
            jdk, jre, testimage, debugimage, staticlibs, sources, sbom
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
        public bool lts = false;
        public OS? os;
        public int page = 0;
        public int page_size = 10;
        public Project? project;
        public ReleaseType? release_type;
        public SortMethod? sort_method;
        public SortOrder? sort_order;
        public string? version;

        public override string ToString()
        {
            Dictionary<string, string> @params = new ();
            if (architecture.HasValue) @params.Add("architecture", architecture.Value.ToString());
            if (c_lib.HasValue) @params.Add("c_lib", c_lib.Value.ToString());
            if (heap_size.HasValue) @params.Add("heap_size", heap_size.Value.ToString());
            if (lts) @params.Add("lts", lts.ToString());
            if (os.HasValue) @params.Add("os", os.Value.ToString());
            if (page != 0) @params.Add("page", page.ToString());
            if (page_size != 10) @params.Add("page_size", page_size.ToString());
            if (project.HasValue) @params.Add("project", project.Value.ToString());
            if (release_type.HasValue) @params.Add("release_type", release_type.Value.ToString());
            if (sort_method.HasValue) @params.Add("sort_method", sort_method.Value.ToString());
            if (sort_order.HasValue) @params.Add("sort_order", sort_order.Value.ToString());
            if (version?.Length > 0) @params.Add("version", version);
            string s = string.Join("&", @params.Select((kv,i) => kv.Key + "=" + UrlEncoder.Default.Encode(kv.Value)));
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
            var jsonText = await httpClient.GetStringAsync("info/available_releases");
            return JsonConvert.DeserializeObject<AvailableReleases>(jsonText);
        }
        public static async Task<ReleaseVersions?> GetReleaseVersions(ReleaseVersionsParams? @params)
        {
            var jsonText = await httpClient.GetStringAsync("info/release_versions" + (@params?.ToString() ?? ""));
            return JsonConvert.DeserializeObject<ReleaseVersions>(jsonText);
        }
    }
}
