using GameLogsMVC.Models.DBData;
namespace GameLogsMVC.Models.ViewData
{
    public class StatsView
    {
        public Stats StatsWatched { get; set; }
        public Stats StatsAttended { get; set; }
        public string User { get; set; }
        public string League { get; set; }

        public class Stats
        {
            public int Games { get; set; }
            public int TeamsSeen { get; set; }
            public int TeamsVisited { get; set; }
            public Dictionary<string, int> Stadium { get; set; }
            public Dictionary<string, List<string>> Player { get; set; }
            public Dictionary<string, List<string>> Pitcher { get; set; }
            public List<Dictionary<List<string>, string>> SingleStats { get; set; }
            public List<Dictionary<List<string>, string>> AccStats { get; set; }
            public List<Game> GameStats { get; set; }
            public List<Dictionary<string, List<string>>> FavsCount { get; set; }
            public List<Dictionary<string, List<string>>> FavsRecord { get; set; }

            public Stats()
            {
                Stadium = new Dictionary<string, int>();
                Player = new Dictionary<string, List<string>>();
                Pitcher = new Dictionary<string, List<string>>();
                SingleStats = new List<Dictionary<List<string>, string>>();
                AccStats = new List<Dictionary<List<string>, string>>();
                GameStats = new List<Game>();
                FavsCount = new List<Dictionary<string, List<string>>>();
                FavsRecord = new List<Dictionary<string, List<string>>>();
            }
        }
        

    }
}
