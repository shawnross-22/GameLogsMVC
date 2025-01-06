using GameLogsMVC.Models.PullData;

namespace GameLogsMVC.Models.ViewData
{
    public class HomeView
    {
        public Dictionary<string, DBData.Team> FavTeams { get; set; }
        public Dictionary<string, Result> FavGames { get; set; }
        public Dictionary<string, DBData.Game> FollowingGames { get; set; }

        public HomeView()
        {
            FavTeams = new Dictionary<string, DBData.Team>();
            FavGames = new Dictionary<string, Result>();
            FollowingGames = new Dictionary<string, DBData.Game>();
        }
    }

    
}
