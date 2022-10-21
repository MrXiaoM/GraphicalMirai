using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GraphicalMirai.Pages.PluginCenter
{
#pragma warning disable CS8618
    public class Topic
    {
        public int tid;
        public long timestamp;
        public string titleRaw;
        public int viewcount;
        public int votes;
        public int deleted;
        public int pinned;
        public List<CTag> tags;
        public CUser user;
        public List<Post> posts;
        
        public CPagination pagination;

        public THeader _header;

        /// <summary>
        /// 生成帖子 html
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (posts.Count > 0)
            {
                return posts[0].content ?? "<p>找不到帖子内容 :(</p>";
            }
            return "";
        }
    }
    public class Repo
    {
        public readonly string platform;
        public readonly string username;
        public readonly string repo;

        public Repo(string platform, string username, string repo)
        {
            this.platform = platform;
            this.username = username;
            this.repo = repo;
        }

        public override string ToString()
        {
            return "https://" + platform + ".com/" + username + "/" + repo;
        }
    }
    public class Post
    {
        private static readonly Regex regexLink = new Regex("(<a .*? ?href=\")(.*?)(\".*?>)", RegexOptions.IgnoreCase);
        private static readonly Regex regexRepo = new Regex("https?://(www\\.)?(github|gitee)\\.com/([A-Za-z0-9_-]+)/([A-Za-z0-9_-]+)", RegexOptions.IgnoreCase);

        public string content;
        public CUser user;
        public long timestamp;
        public long edited;
        public int deleted;
        public int index;
        public int pid;
        public string toPid;
        public PReplies replies;
        public int votes;

        public List<Repo> repo()
        {
            return links().Select<string, Repo?>(s =>
            {
                Match m = regexRepo.Match(s);
                if (!m.Success) return null;
                string platform = m.Groups[2].Value;
                string username = m.Groups[3].Value;
                string repo = m.Groups[4].Value;
                return new Repo(platform, username, repo);

            }).SkipWhile(repo => repo == null).OfType<Repo>().ToList();
        }

        public List<string> links()
        {
            List<string> links = new();
            foreach (Match m in regexLink.Matches(content))
            {
                links.Add(m.Groups[2].Value);
            }
            return links;
        }
    }
    public class PReplies
    {
        public bool hasMore;
        public List<PRUser> users;
        public int count;
        public string timestampISO;
    }
    public class PRUser
    {
        public int uid;
        public string username;
        public string userslug;
        public string picture;
        public string fullname;
        public string displayname;
    }
    public class THeader
    {
        public HTags tags;
        public override string ToString()
        {
            return tags.ToString();
        }
    }
    public class HTags
    {
        public static readonly TMeta charset = new() { charset = "utf-8" };
        public static readonly TLink defaultStyle = new() { rel = "stylesheet", type = "text/css", href = "https://mirai.mamoe.net/assets/client.css" };

        public List<TMeta> meta = new();
        public List<TLink> link = new();

        public override string ToString()
        {
            List<string> header = new();
            header.Add(charset.ToString());
            // json 返回的头信息添加后似乎没有起到什么作用，故不添加到头
            /* 
            foreach(var m in meta)
            {
                header.Add(m.ToString());
            }
            */
            header.Add(defaultStyle.ToString());
            /*
            foreach(var l in link)
            {
                header.Add(l.ToString());
            }
            */
            return "<head>\n    " + string.Join("\n    ", header) + "\n</head>";
        }
    }
    public class TMeta
    {
        public string? charset;
        public string? name;
        public string? property;
        public string? content;

        public override string ToString()
        {
            List<string> args = new();
            if (charset != null) args.Add("charset=\"" + charset + "\"");
            if (name != null) args.Add("name=\"" + name + "\"");
            if (property != null) args.Add("property=\"" + property + "\"");
            if (content != null) args.Add("content=\"" + content + "\"");
            return "<meta " + string.Join(' ', args) + "/>";
        }
    }

    public class TLink
    {
        public string? rel;
        public string? type;
        public string? href;
        public string? crossorigin;
        public string? title;
        public string? sizes;
        public override string ToString()
        {

            List<string> args = new();
            if (rel != null) args.Add("rel=\"" + rel + "\"");
            if (type != null) args.Add("type=\"" + type + "\"");
            if (href != null)
            {
                if (href.StartsWith("/")) href = "https://mirai.mamoe.net" + href;
                args.Add("href=\"" + href + "\"");
            }
            if (crossorigin != null) args.Add("crossorigin=\"" + crossorigin + "\"");
            if (title != null) args.Add("title=\"" + title + "\"");
            if (sizes != null) args.Add("sizes=\"" + sizes + "\"");
            return "<link " + string.Join(' ', args) + "/>";
        }
    }
#pragma warning restore CS8618
}
