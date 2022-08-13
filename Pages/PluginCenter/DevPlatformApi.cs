using GraphicalMirai;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pages.PluginCenter
{
    public interface IDevPlatformApi
    {
        #region 结构体
        public struct Release
        {
            public struct Asset
            {
                public readonly string Name;
                public readonly User Uploader;
                public readonly string DownloadUrl;
                public readonly int DownloadCount;
                public readonly long Size;
                public readonly long CreatedTime;
                public readonly long UpdatedTime;

                public Asset(string name, User uploader, string downloadUrl, int downloadCount, long size, long createdTime, long updatedTime)
                {
                    Name = name;
                    Uploader = uploader;
                    DownloadUrl = downloadUrl;
                    DownloadCount = downloadCount;
                    Size = size;
                    CreatedTime = createdTime;
                    UpdatedTime = updatedTime;
                }
            }
            public readonly string TagName;
            public readonly string Name;
            public readonly string TargetCommitish;
            public readonly User Author;
            public readonly bool IsPreRelease;
            public readonly long CreatedTime;
            public readonly long PublishedTime;
            public readonly string Body;
            public readonly List<Asset> Assets;
            
            public Release(string tagName, string name, string targetCommitish, User author, bool isPreRelease, long createdTime, long publishedTime, string body, List<Asset> assets)
            {
                TagName = tagName;
                Name = name;
                TargetCommitish = targetCommitish;
                Author = author;
                IsPreRelease = isPreRelease;
                CreatedTime = createdTime;
                PublishedTime = publishedTime;
                Body = body;
                Assets = assets;
            }
        }
        public struct User
        {
            public readonly string Login;
            public readonly string Name;
            public readonly string Url;
            public readonly string AvatarUrl;
            public User(string login, string name, string url, string avatarUrl)
            {
                Login = login;
                Name = name;
                Url = url;
                AvatarUrl = avatarUrl;
            }
        }
        #endregion

        public static readonly Dictionary<string, IDevPlatformApi> apis = new()
        {
            { "github", new GithubApi() },
            { "gitee", new GiteeApi() }
        };

        public virtual (User, Exception?) GetUser(string user)
        {
            return GetUserAsync(user).Result;
        }
        public Task<(User, Exception?)> GetUserAsync(string user);
        public virtual (List<Release>, Exception?) GetReleases(string user, string repo)
        {
            return GetReleasesAsync(user, repo).Result;
        }
        public Task<(List<Release>, Exception?)> GetReleasesAsync(string user, string repo);
    }

    public class GithubApi : IDevPlatformApi
    {

        #region Json 实体类
        internal class JsonUser
        {
            string login;
            string name;
            string avatar_url;
            string html_url;

            public JsonUser(string login, string name, string avatar_url, string html_url)
            {
                this.login = login;
                this.name = name;
                this.avatar_url = avatar_url;
                this.html_url = html_url;
            }

            internal IDevPlatformApi.User ToUser()
            {
                if (name == null) name = login;
                return new IDevPlatformApi.User(login, name, html_url, avatar_url);
            }
        }
        internal class JsonRelease
        {
            JsonUser author;
            string tag_name;
            string name;
            string target_commitish;
            bool prerelease;
            string created_at;
            string published_at;
            List<JsonAsset> assets;
            string body;

            public JsonRelease(JsonUser author, string tag_name, string name, string target_commitish, bool prerelease, string created_at, string published_at, List<JsonAsset> assets, string body)
            {
                this.author = author;
                this.tag_name = tag_name;
                this.name = name;
                this.target_commitish = target_commitish;
                this.prerelease = prerelease;
                this.created_at = created_at;
                this.published_at = published_at;
                this.assets = assets;
                this.body = body;
            }

            internal IDevPlatformApi.Release ToRelease()
            {
                List<IDevPlatformApi.Release.Asset> processedAssets = new();
                foreach (JsonAsset asset in assets)
                {
                    processedAssets.Add(asset.ToAsset());
                }
                long createTime = created_at == null ? 0 : App.ToTimestamp(DateTime.Parse(created_at));
                long publishTime = published_at == null ? createTime : App.ToTimestamp(DateTime.Parse(published_at));

                return new IDevPlatformApi.Release(tag_name, name, target_commitish, author.ToUser(), prerelease, createTime, publishTime, body, processedAssets);
            }
        }
        internal class JsonAsset
        {
            string name;
            JsonUser uploader;
            int size = 0;
            string created_at;
            string updated_at;
            int download_count = 0;
            string browser_download_url;

            public JsonAsset(string name, JsonUser uploader, int size, string created_at, string updated_at, int download_count, string browser_download_url)
            {
                this.name = name;
                this.uploader = uploader;
                this.size = size;
                this.created_at = created_at;
                this.updated_at = updated_at;
                this.download_count = download_count;
                this.browser_download_url = browser_download_url;
            }

            internal IDevPlatformApi.Release.Asset ToAsset()
            {
                if (name == null)
                {
                    name = browser_download_url.Substring(browser_download_url.LastIndexOf('/') + 1);
                }
                long createTime = created_at == null ? 0 : App.ToTimestamp(DateTime.Parse(created_at));
                long updateTime = updated_at == null ? createTime : App.ToTimestamp(DateTime.Parse(updated_at));

                return new IDevPlatformApi.Release.Asset(name, uploader.ToUser(), browser_download_url, download_count, size, createTime, updateTime);
            }
        }
        #endregion
        private readonly HttpClient http = new HttpClient()
        {
            BaseAddress = new Uri("https://api.github.com/")
        };
        public GithubApi()
        {
            http.DefaultRequestVersion = new System.Version(1, 1);
            http.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            http.DefaultRequestHeaders.Add("User-Agent", App.UserAgent);
            http.Timeout = TimeSpan.FromSeconds(30);
        }
        public async Task<(List<IDevPlatformApi.Release>, Exception?)> GetReleasesAsync(string user, string repo)
        {
           
            List<IDevPlatformApi.Release> releases = new();
            Exception? ex = null;
            try
            {
                string json = await http.GetStringAsync("repos/" + user + "/" + repo + "/releases");
                List<JsonRelease>? jr = JsonConvert.DeserializeObject<List<JsonRelease>>(json);
                if (jr == null) throw new JsonException("无法读取 json");
                foreach(JsonRelease r in jr)
                {
                    releases.Add(r.ToRelease());
                }
            }
            catch (Exception e)
            {
                ex = e;
            }
            return (releases, ex);
        }

        public async Task<(IDevPlatformApi.User, Exception?)> GetUserAsync(string user)
        {
            List<IDevPlatformApi.Release> releases = new();
            try
            {
                string json = await http.GetStringAsync("users/" + user);
                JsonUser? ju = JsonConvert.DeserializeObject<JsonUser>(json);
                if (ju == null) throw new JsonException("无法读取 json");
                return (ju.ToUser(), null);
            }
            catch (Exception e)
            {
                return (new IDevPlatformApi.User(), e);
            }
        }
    }
    public class GiteeApi : IDevPlatformApi
    {
        private readonly HttpClient http = new HttpClient()
        {
            BaseAddress = new Uri("https://gitee.com/api/v5/")
        };
        public GiteeApi()
        {
            http.DefaultRequestHeaders.Add("User-Agent", App.UserAgent);
            http.Timeout = TimeSpan.FromSeconds(30);
        }
        public async Task<(List<IDevPlatformApi.Release>, Exception?)> GetReleasesAsync(string user, string repo)
        {
            List<IDevPlatformApi.Release> releases = new();
            Exception? ex = null;
            try
            {
                string json = await http.GetStringAsync("repos/" + user + "/" + repo + "/releases");
                List<GithubApi.JsonRelease>? jr = JsonConvert.DeserializeObject<List<GithubApi.JsonRelease>>(json);
                if (jr == null) throw new JsonException("无法读取 json");
                foreach (GithubApi.JsonRelease r in jr)
                {
                    releases.Add(r.ToRelease());
                }
            }
            catch (Exception e)
            {
                ex = e;
            }
            return (releases, ex);
        }

        public async Task<(IDevPlatformApi.User, Exception?)> GetUserAsync(string user)
        {
            List<IDevPlatformApi.Release> releases = new();
            try
            {
                string json = await http.GetStringAsync("users/" + user);
                GithubApi.JsonUser? ju = JsonConvert.DeserializeObject<GithubApi.JsonUser>(json);
                if (ju == null) throw new JsonException("无法读取 json");
                return (ju.ToUser(), null);
            }
            catch (Exception e)
            {
                return (new IDevPlatformApi.User(), e);
            }
        }
    }

}
