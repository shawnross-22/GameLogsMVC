namespace GameLogsMVC.Models.ViewData
{
    public class StatsView
    {
        public string User { get; set; }
        public string League { get; set; }
        public int Games { get; set; }
        public int TeamsSeen { get; set; }
        public int TeamsVisited { get; set; } 
        public Dictionary<string, int> Stadium { get; set; }
        public Dictionary<string, int> Player { get; set; }
        public Dictionary<string, int> Pitcher { get; set; }
        public List<Dictionary<string, string>> Stats { get; set; }
        public Dictionary<string, string> LongGame { get; set; }
        public Dictionary<string, string> ShortGame { get; set; }
        public Dictionary<string, string> HighScoring { get; set; }
        public string Play { get; set; }
        public List<Dictionary<string, int>> FavsCount { get; set; }
        public List<Dictionary<string, string>> FavsRecord { get; set; }

        public StatsView()
        {
            Stadium = new Dictionary<string, int>();
            Player = new Dictionary<string, int>();
            Pitcher = new Dictionary<string, int>();
            Stats = new List<Dictionary<string, string>>();
            LongGame = new Dictionary<string, string>();
            ShortGame = new Dictionary<string, string>();
            HighScoring = new Dictionary<string, string>();
            FavsCount = new List<Dictionary<string, int>>();
            FavsRecord = new List<Dictionary<string, string>>();
        }

    }
}
