namespace GameLogsMVC.Models.PullData
{
    public class Logos
    {
        public string href { get; set; }
        public string alt { get; set; }
        public List<string> rel { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Links
    {
        public string language { get; set; }
        public List<string> rel { get; set; }
        public string href { get; set; }
        public string text { get; set; }
        public string shortText { get; set; }
        public bool isExternal { get; set; }
        public bool isPremium { get; set; }
        public bool isHidden { get; set; }
    }

    public class TeamInfo
    {
        public string id { get; set; }
        public string uid { get; set; }
        public string slug { get; set; }
        public string abbreviation { get; set; }
        public string displayName { get; set; }
        public string shortDisplayName { get; set; }
        public string name { get; set; }
        public string nickname { get; set; }
        public string location { get; set; }
        public string color { get; set; }
        public string alternateColor { get; set; }
        public bool isActive { get; set; }
        public bool isAllStar { get; set; }
        public List<Logos> logos { get; set; }
        public List<Links> links { get; set; }
    }

    public class Team
    {
        public TeamInfo team { get; set; }
    }

    public class Leagues
    {
        public string id { get; set; }
        public string uid { get; set; }
        public string name { get; set; }
        public string abbreviation { get; set; }
        public string shortName { get; set; }
        public string slug { get; set; }
        public List<Team> teams { get; set; }
    }

    public class Sports
    {
        public string id { get; set; }
        public string uid { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public List<Leagues> leagues { get; set; }
    }

    public class Root
    {
        public List<Sports> sports { get; set; }
    }
}
