using GameLogsMVC.Models.DBData;
using GameLogsMVC.Models.PullData;

namespace GameLogsMVC.Models.ViewData
{
    public class TeamView
    {
        public string UserID { get; set; }
        public string League { get; set; }
        public PullData.Teams Team { get; set; }
        public string RecordWatched { get; set; }
        public string RecordAttended { get; set; }
        public List<Game> GamesWatched { get; set; }
        public List<Game> GamesAttended { get; set; }

        public TeamView()
        {
            GamesWatched = new List<Game>();
            GamesAttended = new List<Game>();
        }
    }
}
