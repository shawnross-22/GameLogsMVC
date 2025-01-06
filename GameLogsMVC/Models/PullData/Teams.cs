namespace GameLogsMVC.Models.PullData
{
    public class Logo
    {
        public string Href { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Alt { get; set; }
        public List<string> Rel { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class Team
    {
        public string Id { get; set; }
        public string Uid { get; set; }
        public string Slug { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Abbreviation { get; set; }
        public string DisplayName { get; set; }
        public string ShortDisplayName { get; set; }
        public string Color { get; set; }
        public string AlternateColor { get; set; }
        public bool IsActive { get; set; }
        public List<Logo> Logos { get; set; }
    }

    public class Teams
    {
        public Team Team { get; set; }
    }
}
