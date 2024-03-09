using GameLogsMVC.Models.DBData;

namespace GameLogsMVC.Models.ViewData
{
    public class DiaryView
    {
        public string User { get; set; }
        public string League { get; set; }
        public GameResult GameResult { get; set; }

    }

    public class GameResult
    {
        public List<Game> games { get; set; }
        public List<bool>? attended { get; set; }
    }
}
