using GameLogsMVC.Models.DBData;
using GameLogsMVC.Models.PullData;
using Microsoft.EntityFrameworkCore;

namespace GameLogsMVC.Models.ViewData
{
    public class PlayerView
    {
        public string UserID { get; set; }
        public string League { get; set; }
        public Players Player { get; set; }
        public PlayerViewInfo PlayerViewInfoWatched { get; set; }
        public PlayerViewInfo PlayerViewInfoAttended { get; set; }

    }

    public class PlayerViewInfo
    {
        public List<PlayerGame> PlayerGames { get; set; }
        public Dictionary<Game, List<PlayerGame>> Games { get; set; }
        public List<string> Sums { get; set; }

        public PlayerViewInfo()
        {
            PlayerGames = new List<PlayerGame>();
            Games = new Dictionary<Game, List<PlayerGame>>();
            Sums = new List<string>();
        }
    }

}
