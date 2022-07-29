using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicalMirai.Pages.PluginCenter
{
    public class Category
    {
        public List<Topic> topics;
        public Pagination pagination;
    }

    public class Topic
    {
        public int tid;
        public long timestamp;
        public string titleRaw;
        public int viewcount;
        public int votes;
        public int deleted;
        public List<Tag> tags;
        public User user;
    }

    public class User
    {
        public string username;
        public string displayname;
        public string? picture;
    }

    public class Tag
    {
        public string value;
    }

    public class Pagination
    {
        public int currentPage;
        public int pageCount;
    }
}
