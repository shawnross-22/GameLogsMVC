namespace GameLogsMVC.Models.PullData
{
    public class Players
    {
        public Athlete Athlete { get; set; }
    }
    public class Athlete
    {
        public string ID { get; set; }
        public string FullName { get; set; }
        public string Jersey { get; set; }
        public Headshot Headshot { get; set; }
        public Position Position { get; set; }
        public Team Team { get; set; }
    }

    public class Headshot
    {
        public string Href { get; set; }
    }

    public class Position
    {
        public string Abbreviation { get; set; }
    }
}
