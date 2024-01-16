using GameLogsMVC.Models.DBData;

namespace GameLogsMVC.Models.ViewData
{
    public class DiaryView
    {
        public User User { get; set; }
        public List<Game> MLBGames { get; set; }
        public List<Game> NCAAFGames { get; set; }
        public List<Game> NBAGames { get; set; }
        public List<Game> NFLGames { get; set; }

    }
}
