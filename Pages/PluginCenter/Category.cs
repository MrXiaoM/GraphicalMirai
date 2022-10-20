using System.Collections.Generic;

namespace GraphicalMirai.Pages.PluginCenter
{
#pragma warning disable CS8618
    public class Category
    {
        public List<CTopic> topics;
        public CPagination pagination;
    }

    public class CTopic
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
    }

    public class CUser
    {
        public int uid;
        public string username;
        public string displayname;
        public string? picture;
        public bool banned;
        public bool muted;
        public List<UGroup> selectedGroups;
        public int reputation;
        public int topiccount;
    }

    public class UGroup
    {
        public string labelColor;
        public string textColor;
        public string userTitle;
    }

    public class CTag
    {
        public string value;
        public override string ToString()
        {
            return value;
        }
    }

    public class CPagination
    {
        public int currentPage;
        public int pageCount;
    }
#pragma warning restore CS8618
}
