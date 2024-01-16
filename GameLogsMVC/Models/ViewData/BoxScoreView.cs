using GameLogsMVC.Models.GameData;

namespace GameLogsMVC.Models.ViewData
{
    public class BoxScoreView
    {
        public Summary Summary { get; set; }
        public Scoring Scoring { get; set; }
        public BaseballBoxScore BSBatting { get; set; }
        public BaseballBoxScore BSPitching { get; set; }

        public BoxScoreView()
        {
            Summary = new Summary();
            Scoring = new Scoring();
            BSBatting = new BaseballBoxScore();
            BSPitching = new BaseballBoxScore();
        }
    }

    public class Summary
    {
        public List<string> Away { get; set; }
        public List<string> Home { get; set; }
        public string Date { get; set; }
        public string StartTime { get; set; }
        public string Attendance { get; set; }
        public string Venue { get; set; }
        public string GameDuration { get; set; }

        public Summary()
        {
            Away = new List<string>();
            Home = new List<string>();
        }
    }

    public class Scoring 
    { 
        public List<string> Categories { get; set; }
        public List<string> HomeScore { get; set; }
        public List<string> AwayScore { get; set; }

        public Scoring()
        {
            Categories = new List<string>();
            HomeScore = new List<string>();
            AwayScore = new List<string>();
        }
    }

    public class BaseballBoxScore
    {
        public List<string> Categories { get; set; }
        public List<string> AwayBoxScore { get; set; }
        public List<string> HomeBoxScore { get; set; }

        public BaseballBoxScore()
        {
            Categories = new List<string>();
            AwayBoxScore = new List<string>();
            HomeBoxScore = new List<string>();
        }
    }
}

